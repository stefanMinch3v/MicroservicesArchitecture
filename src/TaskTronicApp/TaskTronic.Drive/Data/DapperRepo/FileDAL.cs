namespace TaskTronic.Drive.Data.DapperRepo
{
    using Dapper;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Threading.Tasks;
    using TaskTronic.Data.Models;
    using TaskTronic.Drive.Models.Files;
    using TaskTronic.Messages.Drive.Files;

    using static Sqls;

    internal class FileDAL : IFileDAL
    {
        private const int DEFAULT_CHUNK_SIZE = 1_048_576; // 1mb
        private const int BUFFER_SIZE = 1024 * 1024;

        private const string BlobsTableName = "[dbo].[Blobsdata]";
        private const string FileTableName = "[dbo].[Files]";
        private const string MessagesTableName = "[dbo].[Messages]";

        private readonly IDbConnectionFactory dbConnectionFactory;

        public FileDAL(IDbConnectionFactory dbConnectionFactory) 
            => this.dbConnectionFactory = dbConnectionFactory;

        public async Task<bool> CreateBlobAsync(InputFileServiceModel file)
        {
            var sql = string.Format(BlobsSql.CREATE_NEW, BlobsTableName);

            int size;

            try
            {
                size = Convert.ToInt32(file.Stream.Length);
            }
            catch (OverflowException)
            {
                size = DEFAULT_CHUNK_SIZE;
            }

            var dbParams = new DynamicParameters();
            dbParams.Add("@data", file.Stream, DbType.Binary, size: size);
            dbParams.Add("@employeeId", file.EmployeeId, DbType.String);
            dbParams.Add("@fileName", file.FileName, DbType.String);
            dbParams.Add("@fileSize", file.Filesize, DbType.Int64);
            dbParams.Add("@finishedUpload", false, DbType.Boolean);
            dbParams.Add("@timestamp", DateTimeOffset.UtcNow, DbType.DateTimeOffset);

            using (var conn = this.dbConnectionFactory.GetSqlConnection)
            {
                return (await conn.ExecuteAsync(sql, dbParams)) > 0;
            }
        }

        public async Task<bool> AppendChunkToBlobAsync(InputFileServiceModel file)
        {
            var sql = string.Format(BlobsSql.APPEND_CHUNK_TO_BLOB, BlobsTableName);

            int size;

            try
            {
                size = Convert.ToInt32(file.Stream.Length);
            }
            catch (OverflowException)
            {
                size = DEFAULT_CHUNK_SIZE;
            }

            var dbParams = new DynamicParameters();
            dbParams.Add("@data", file.Stream, DbType.Binary, size: size);
            dbParams.Add("@employeeId", file.EmployeeId, DbType.Int32);
            dbParams.Add("@fileSize", file.Filesize, DbType.Int64);
            dbParams.Add("@fileName", file.FileName, DbType.String);

            using (var conn = this.dbConnectionFactory.GetSqlConnection)
            {
                return (await conn.ExecuteAsync(sql, dbParams)) > 0;
            }
        }

        #region TODO: merge in one method
        public async Task<(int FileId, int MessageId)> SaveCompletedUploadAsync(InputFileServiceModel file, string oldFileName)
        {
            using (var conn = this.dbConnectionFactory.GetSqlConnection)
            {
                conn.Open();

                var transaction = conn.BeginTransaction();

                int insertedFileId;
                int insertedMessageId;

                try
                {
                    // update blobsdata to done
                    var sqlUpdate = string.Format(BlobsSql.UPDATE_BLOB_DATA_WITH_OLD_FILENAME, BlobsTableName);

                    var employeeId = file.EmployeeId;
                    var fileName = file.FileName;

                    var updatedBlobId = await conn.ExecuteScalarAsync<int>(sqlUpdate, new
                    {
                        employeeId,
                        fileName,
                        oldFileName
                    }, transaction);

                    // insert new file
                    var sqlInsert = string.Format(FilesSql.ADD_NEW, FileTableName, BlobsTableName);

                    file.BlobId = updatedBlobId;
                    file.Revision = string.Empty;
                    file.EmployeeId = employeeId;

                    insertedFileId = (await conn.ExecuteScalarAsync<int>(sqlInsert, file, transaction));

                    // save and send message to all subscribers
                    var sqlMessages = string.Format(FilesSql.ADD_NEW_MESSAGE, MessagesTableName);

                    var messageData = new FileUploadedMessage
                    {
                        FileId = insertedFileId,
                        Name = file.FileName,
                        Type = file.ContentType
                    };

                    var message = new Message(messageData);

                    insertedMessageId = await conn.ExecuteScalarAsync<int>(sqlMessages, new
                    {
                        Type = message.Type.AssemblyQualifiedName,
                        message.Published,
                        message.serializedData
                    }, transaction);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

                return (insertedFileId, insertedMessageId);
            }
        }

        public async Task<(int FileId, int MessageId)> SaveCompletedUploadAsync(InputFileServiceModel file)
        {
            using (var conn = this.dbConnectionFactory.GetSqlConnection)
            {
                conn.Open();

                var transaction = conn.BeginTransaction();

                int insertedFileId;
                int insertedMessageId;

                try
                {
                    // update blob to done
                    var sqlUpdate = string.Format(BlobsSql.UPDATE_BLOB_DATA, BlobsTableName);

                    var employeeId = file.EmployeeId;
                    var fileName = file.FileName;

                    var updatedBlobId = await conn.ExecuteScalarAsync<int>(sqlUpdate, new
                    {
                        employeeId,
                        fileName
                    }, transaction);

                    // insert new file
                    var sqlInsert = string.Format(FilesSql.ADD_NEW, FileTableName, BlobsTableName);

                    file.BlobId = updatedBlobId;
                    file.Revision = string.Empty;
                    file.EmployeeId = employeeId;

                    insertedFileId = (await conn.ExecuteScalarAsync<int>(sqlInsert, file, transaction));

                    // save and send message to all subscribers
                    var sqlMessages = string.Format(FilesSql.ADD_NEW_MESSAGE, MessagesTableName);

                    var messageData = new FileUploadedMessage
                    {
                        FileId = insertedFileId,
                        Name = file.FileName,
                        Type = file.ContentType
                    };

                    var message = new Message(messageData);

                    insertedMessageId = await conn.ExecuteScalarAsync<int>(sqlMessages, new
                    {
                        Type = message.Type.AssemblyQualifiedName,
                        message.Published,
                        message.serializedData
                    }, transaction);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

                return (insertedFileId, insertedMessageId);
            }
        }
        #endregion

        // not yet added to the front-end
        public async Task<int> SaveCompletedUploadAsReplaceExistingFileAsync(InputFileServiceModel file, int fileId)
        {
            using (var conn = this.dbConnectionFactory.GetSqlConnection)
            {
                conn.Open();

                var transaction = conn.BeginTransaction();

                int insertedId;

                try
                {
                    var sqlUpdate = string.Format(BlobsSql.UPDATE_BLOB_DATA, BlobsTableName);

                    var sqlDeleteOldBlob = string.Format(BlobsSql.Delete_Blob_FROM_FILEID, BlobsTableName, FileTableName);

                    var sqlUpdateFile = string.Format(FilesSql.UPDATE_FIlE_WITH_NEW_BLOB, FileTableName);

                    var employeeId = file.EmployeeId;
                    var fileName = file.FileName;

                    var updatedBlobId = await conn.ExecuteScalarAsync<int>(sqlUpdate, new
                    {
                        employeeId,
                        fileName
                    }, transaction);

                    var deletedFileBlob = (await conn.ExecuteAsync(sqlDeleteOldBlob, new
                    {
                        file.CatalogId,
                        file.FolderId,
                        fileId
                    }, transaction)) == 1;
                    if (deletedFileBlob)
                    {
                        insertedId = (await conn.ExecuteScalarAsync<int>(sqlUpdateFile, new
                        {
                            file.CatalogId,
                            file.FolderId,
                            fileId,
                            updatedBlobId,
                            UpdateDate = DateTime.UtcNow
                        }, transaction));

                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                        insertedId = 0;
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

                return insertedId;
            }
        }

        public async Task<IEnumerable<FileServiceModel>> GetFilesByFolderIdAsync(int folderId)
        {
            var sql = string.Format(FilesSql.GET_FILES_BY_FOLDER_ID, FileTableName);

            using (var conn = this.dbConnectionFactory.GetSqlConnection)
            {
                return (await conn.QueryAsync<FileServiceModel>(sql, new { folderId })).AsList();
            }
        }

        public async Task<FileServiceModel> GetFileByIdAsync(int catalogId, int folderId, int fileId)
        {
            var sql = string.Format(FilesSql.GET_FILE_BY_ID, FileTableName);

            using (var conn = this.dbConnectionFactory.GetSqlConnection)
            {
                return (await conn.QueryFirstOrDefaultAsync<FileServiceModel>(sql, new { catalogId, folderId, fileId }));
            }
        }

        public async Task<(bool Success, int MessageId)> DeleteFileAsync(int catalogId, int folderId, int fileId, int blobId)
        {
            using (var conn = this.dbConnectionFactory.GetSqlConnection)
            {
                conn.Open();
                var transaction = conn.BeginTransaction();

                int insertedMessageId;

                try
                {
                    var sqlDeleteBlob = string.Format(BlobsSql.Delete_Blob, BlobsTableName);
                    var sqlDeleteFileObj = string.Format(FilesSql.Delete_file, FileTableName);

                    var deletedFileBlob = (await conn.ExecuteAsync(sqlDeleteBlob, new
                    {
                        blobId
                    }, transaction)) == 1;

                    var deletedFile = (await conn.ExecuteAsync(sqlDeleteFileObj, new
                    {
                        fileId,
                        catalogId,
                        folderId
                    }, transaction)) == 1;

                    if (deletedFileBlob && deletedFile)
                    {
                        // save and send message to all subscribers
                        var sqlMessages = string.Format(FilesSql.ADD_NEW_MESSAGE, MessagesTableName);

                        var messageData = new FileDeletedMessage
                        {
                            FileId = fileId
                        };

                        var message = new Message(messageData);

                        insertedMessageId = await conn.ExecuteScalarAsync<int>(sqlMessages, new
                        {
                            Type = message.Type.AssemblyQualifiedName,
                            message.Published,
                            message.serializedData
                        }, transaction);

                        transaction.Commit();
                        return (true, insertedMessageId);
                    }
                    else
                    {
                        transaction.Rollback();
                        return (false, 0);
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        //https://stackoverflow.com/a/2101447/7588131
        public async Task ReadStreamFromFileAsync(int blobId, Stream stream)
        {
            var sql = string.Format(BlobsSql.READ_STREAM_FROM_FILE, BlobsTableName);

            using (var conn = this.dbConnectionFactory.GetSqlConnection)
            using (var reader = await conn.ExecuteReaderAsync(sql, new { blobId }))
            {
                while (reader.Read())
                {
                    var buffer = new byte[BUFFER_SIZE];
                    var dataIndex = 0L;

                    long bytesRead;
                    while ((bytesRead = reader.GetBytes(0, dataIndex, buffer, 0, buffer.Length)) > 0)
                    {
                        //var actual = new byte[bytesRead];
                        //Array.Copy(buffer, 0, actual, 0, bytesRead);

                        await stream.WriteAsync(buffer, 0, (int)bytesRead);

                        dataIndex += bytesRead;
                    }
                }
            }
        }

        public async Task<OutputFileDownloadServiceModel> GetFileInfoForDownloadAsync(int fileId)
        {
            var sql = string.Format(FilesSql.GET_FILE_DETAILS_FOR_DOWNLOAD, FileTableName);

            using (var conn = this.dbConnectionFactory.GetSqlConnection)
            {
                return await conn.QuerySingleOrDefaultAsync<OutputFileDownloadServiceModel>(sql, new { fileId });
            }
        }

        public async Task<bool> RenameFileAsync(int catalogId, int folderId, int fileId, string newFileName)
        {
            var sql = string.Format(FilesSql.RENAME_FILE, FileTableName);

            using (var db = this.dbConnectionFactory.GetSqlConnection)
            {
                return await db.ExecuteAsync(sql, new { catalogId, folderId, fileId, newFileName }) > 0;
            }
        }

        public async Task<bool> MoveFileAsync(int catalogId, int folderId, int fileId, int newFolderId, string fileName)
        {
            var sql = string.Format(FilesSql.MOVE_FILE, FileTableName);

            using (var db = this.dbConnectionFactory.GetSqlConnection)
            {
                return await db.ExecuteAsync(sql, new { catalogId, folderId, fileId, newFolderId, fileName }) > 0;
            }
        }

        public async Task<IEnumerable<FileServiceModel>> SearchFilesAsync(
            int catalogId, 
            string value, 
            IEnumerable<int> accessibleFolders)
        {
            var sql = string.Format(FilesSql.SEARCH_FILES, FileTableName, value);

            using (var db = this.dbConnectionFactory.GetSqlConnection)
            {
                return (await db.QueryAsync<FileServiceModel>(sql, new { catalogId, accessibleFolders })).AsList();
            }
        }

        public async Task<int?> DoesFileWithSameNameExistInFolder(int catalogId, int folderId, string fileName, string fileType)
        {
            var sql = string.Format(FilesSql.GET_FILEID_BY_FILENAME, FileTableName);

            using (var db = this.dbConnectionFactory.GetSqlConnection)
            {
                return (await db.QuerySingleOrDefaultAsync<int?>(sql, new { catalogId, folderId, fileName, fileType }));
            }
        }

        public async Task<int> CountFilesForEmployeeAsync(int employeeId)
        {
            var sql = string.Format(FilesSql.COUNT_FILES_FOR_EMPLOYEE, FileTableName);

            using (var db = this.dbConnectionFactory.GetSqlConnection)
            {
                return await db.ExecuteScalarAsync<int>(sql, new { employeeId });
            }
        }
    }
}
