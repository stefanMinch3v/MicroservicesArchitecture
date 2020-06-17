namespace TaskTronic.Drive.Data.DapperRepo
{
    internal static class Sqls
    {
        public class LogsSql
        {
            public const string ADD = @"
                INSERT INTO {0}
                (
                    FolderId,
                    FileId,
                    [Action],
                    UserId,
                    [Timestamp],
                    [Text]
                )
                VALUES
                (
                    @FolderId,
                    @FileId,
                    @Action,
                    @UserId,
                    @Timestamp,
                    @Text
                )";

            public const string GET_FILE_LOG = @"
                SELECT * FROM {0}
                WHERE FileId = @fileId 
                    AND Action <> 1
                ORDER BY [Timestamp] DESC";

            public const string GET_FOLDER_LOG = @"
                SELECT * FROM {0}
                WHERE FolderId = @folderId
                ORDER BY [Timestamp] DESC";
        }

        public class CatalogSql
        {
            public const string ADD = @"
                INSERT INTO {0}
                (
                    CompanyId,
                    DepartmentId
                )
                VALUES
                (
                    @CompanyId,
                    @DepartmentId
                ); SELECT CAST(SCOPE_IDENTITY() AS INT)";
        }

        public class FolderSql
        {
            public const string ADD = @"
                INSERT INTO {0}
                (
                    CatalogId,
                    ParentId,
                    RootId,
                    Name,
                    IsPrivate,
                    UserId,
                    CreateDate
                )
                VALUES
                (
                   @CatalogId,
                   @ParentId,
                   @RootId,
                   @Name,
                   @IsPrivate,
                   @UserId,
                   @CreateDate
                ); SELECT CAST (SCOPE_IDENTITY() AS INT)";

            public const string RENAME_FOLDER = @"
                UPDATE {0}
                   SET [Name] = @newFolderName
                WHERE CatalogId = @CatalogId 
                   AND FolderId = @folderId";

            //check for parent or root folder by Id
            public const string CHECK_FOR_FOLDER = @"
                SELECT FolderId FROM {0}
                    WHERE FolderId = @FolderId";

            public const string CHECK_FOR_FOLDER_NAME_EXIST_IN_FOLDER = @"
                SELECT FolderId FROM {0}
                    WHERE ParentId = @parentFolderId
                        AND Name like @name";

            public const string GET_BY_ID = @"
                SELECT * FROM {0}
                    WHERE FolderId = @folderId";

            public const string GET_SUBFOLDERS = @"
                SELECT * FROM {0} 
                    WHERE ParentId = @folderId";

            public const string MOVE_FOLDER_TO_NEW_PARENT = @"
                 UPDATE {0}
                   SET [ParentId] = @newFolderParentId,
                       [Name] = @folderName
                WHERE CatalogId = @CatalogId 
                   AND FolderId = @folderToMoveId";

            public const string GET_FOLDER_TREE = @"
                SELECT 
	                f.*,
	                FileCount = (SELECT COUNT(*) FROM {2} containedFile WHERE containedFile.FolderId = f.FolderId),
	                FolderCount = (SELECT COUNT(*) FROM {0} containedFolder WHERE containedFolder.ParentId = f.FolderId),
	                HasAccess = 
		                CASE WHEN (
			                f.IsPrivate = 0 
			                OR EXISTS (
				                SELECT * 
				                FROM {1} p 
				                WHERE 
					                p.FolderId = f.FolderId
					                AND p.UserId = @userId
			                )
		                )
		                THEN 1
		                ELSE 0
		                END
                FROM {0} f
                WHERE 
	                ( 
                        f.RootId = @folderId
                        OR f.FolderId = @folderId
                    )";


            public const string GET_FOLDERS_BY_SITEID = @"
                SELECT * FROM {0}
                    WHERE ParentId IS NULL
                        AND RootId IS NULL";

            public const string GET_FOLDERS_BY_CATID = @"
                 SELECT * FROM {0}
                    WHERE CatalogId = @CatalogId
                        AND ParentId IS NULL";

            public const string GET_ROOT_FOLDER_BY_CATID = @"
                SELECT * FROM {0}
                    WHERE CatalogId = @CatalogId
                        AND ParentId IS NULL
                        AND RootId IS NULL";

            public const string CHECK_IF_FOLDER_IS_PRIVATE = @"
                 SELECT IsPrivate FROM {0}
                    WHERE FolderId = @folderId";

            public const string DELETE_FOLDER = @"
                DELETE FROM {0}
                WHERE FolderId = @folderId
                    AND CatalogId = @CatalogId";
        }

        public class PermissionsSql
        {
            public const string ADD_SINGLE = @"
                INSERT INTO {0}
                (
                    CatalogId,
                    FolderId,
                    UserId
                )
                VALUES
                (
                    @CatalogId,
                    @FolderId,
                    @UserId
                );";

            public const string GET_USER_FOLDER_PERMISSIONS = @"
                SELECT FolderId FROM {0}                   
                WHERE UserId = @userId
                    AND CatalogId = @catalogId";

            public const string HAS_USER_PERMISSION_FOR_FOLDER = @"
                SELECT COUNT(1) FROM {0}
                WHERE UserId = @userId
                    AND CatalogId = @catalogId
                    AND FolderId = @folderId";

            public const string GET_USERNAME_BY_USER_ID = @"
                SELECT TOP(1) UserName, FirstName, LastName
                FROM {0}
                WHERE Id = @userId";
        }

        public class BlobsSql
        {
            public const string READ_STREAM_FROM_FILE = @"
                SELECT [Data]
                FROM {0}
                WHERE BlobId = @blobId";

            public const string CREATE_NEW = @"
                DECLARE @newdata varbinary(max) = @data
                INSERT INTO {0}
                (
                    [UserId],
                    [FileName],
                    [Timestamp],
                    [Filesize],
                    [FinishedUpload],
                    [Data]
                )
                VALUES
                (
                    @userId,
                    @fileName,
                    @timestamp,
                    @fileSize,
                    @finishedUpload,
                    @newdata
                )";

            public const string APPEND_CHUNK_TO_BLOB = @"
                UPDATE {0} 
                SET Data = Data + @data, 
                    Filesize = Filesize + @fileSize 
                WHERE 
                    UserId = @userId 
                    AND FinishedUpload = 0
                    AND FileName = @fileName";

            public const string UPDATE_BLOB_DATA = @"
                UPDATE {0}
                SET FinishedUpload = 1
                OUTPUT INSERTED.BlobId
                WHERE UserId = @userId
                    AND FinishedUpload = 0
                    AND FileName = @fileName";

            public const string UPDATE_BLOB_DATA_WITH_OLD_FILENAME = @"
                UPDATE {0}
                SET FinishedUpload = 1
                OUTPUT INSERTED.BlobId
                WHERE UserId = @userId
                    AND FinishedUpload = 0
                    AND FileName = @oldFileName";

            public const string Delete_Blob = @"
                DELETE FROM {0}
                WHERE 
                     BlobId = @blobId";

            public const string Delete_Blob_FROM_FILEID = @"
                DELETE FROM {0}
                WHERE 
                     BlobId = (SELECT BlobId 
                               FROM {1} 
                               WHERE FileId = @fileId
                                    AND CatalogId = @CatalogId 
                                    AND FolderId = @folderId)";
        }

        public class FilesSql
        {
            public const string ADD_NEW = @"
                INSERT INTO {0}
                (
                    [CatalogId],
                    [FolderId],
                    [BlobId],
                    [FileName],
                    [FileType],
                    [FileSize],
                    [ContentType],
                    [UserId],
                    [CreateDate]
                )
                VALUES
                (
                    @CatalogId,
                    @FolderId,
                    @BlobId,
                    @Filename,
                    @Filetype,
                    (SELECT Filesize FROM {1} WHERE BlobId = @BlobId),
                    @ContentType,
                    @UserId,
                    @CreateDate
                ); SELECT CAST (SCOPE_IDENTITY() AS INT)";

            public const string UPDATE_FIlE_WITH_NEW_BLOB = @"
                 UPDATE {0}
                   SET [BlobId] = @updatedBlobId,
                       [UpdateDate] = @UpdateDate
                WHERE CatalogId = @CatalogId 
                   AND FolderId = @folderId
                   AND FileId = @fileId";

            public const string GET_FILES_BY_FOLDER_ID = @"
                SELECT * FROM {0}
                WHERE FolderId = @folderId";

            public const string GET_FILE_BY_ID = @"
                SELECT * FROM {0}
                WHERE 
                      CatalogId = @CatalogId                   
                      AND FolderId = @folderId
                      AND FileId = @fileId";

            public const string Delete_file = @"
                DELETE FROM {0}
                WHERE
                   FileId = @fileId
                   AND CatalogId = @CatalogId
                   AND FolderId = @folderId";

            public const string GET_FILE_DETAILS_FOR_DOWNLOAD = @"
                SELECT TOP(1) BlobId, Filename, Filesize, Filetype, ContentType
                FROM {0} 
                WHERE FileId = @fileId";

            public const string RENAME_FILE = @"
                UPDATE {0}
                   SET [Filename] = @newFileName
                WHERE CatalogId = @CatalogId 
                   AND FolderId = @folderId
                   AND FileId = @fileId";

            public const string MOVE_FILE = @"
                UPDATE {0}
                   SET [FolderId] = @newFolderId,
                       [Filename] = @fileName
                WHERE CatalogId = @CatalogId 
                   AND FolderId = @folderId
                   AND FileId = @fileId";

            public const string GET_FILEID_BY_FILENAME = @"
                SELECT FileId FROM {0} 
                WHERE Filename like @fileName
                    AND Filetype like @fileType
                    AND CatalogId = @CatalogId
                    AND FolderId = @folderId";

            public const string SEARCH_FILES = @"
                SELECT * FROM {0}
                    WHERE CHARINDEX('{1}', Filename) > 0 
                        OR CHARINDEX('{1}', FileType) > 0
                    AND CatalogId = @CatalogId";

        }
    }
}
