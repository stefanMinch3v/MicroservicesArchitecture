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
            public const string GET_ALL_FLAT_FOR_SEARCH = @"
                SELECT * FROM {0}
                WHERE CatalogId = @catalogId
                    AND (RootId = @rootFolderId OR RootId IS NULL)
                ORDER BY FolderId ASC";

            public const string ADD = @"
                INSERT INTO {0}
                (
                    CatalogId,
                    ParentId,
                    RootId,
                    Name,
                    IsPrivate,
                    EmployeeId,
                    CreateDate
                )
                VALUES
                (
                   @CatalogId,
                   @ParentId,
                   @RootId,
                   @Name,
                   @IsPrivate,
                   @EmployeeId,
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
                SELECT COUNT(*) FROM {0}
                    WHERE ParentId = @parentFolderId
                        AND [Name] LIKE CONCAT('%', @name, '%')";

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
					                AND p.EmployeeId = @employeeId
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

            public const string GET_ROOT_FOLDER_ID = @"
                SELECT RootId FROM {0}
                WHERE FolderId = @folderId";

            public const string CHECK_IF_FOLDER_IS_PRIVATE = @"
                 SELECT IsPrivate FROM {0}
                    WHERE FolderId = @folderId";

            public const string DELETE_FOLDER = @"
                DELETE FROM {0}
                WHERE FolderId = @folderId
                    AND CatalogId = @CatalogId";

            public const string COUNT_FOLDERS_FOR_EMPLOYEE = @"
                SELECT COUNT(*) FROM {0}
                WHERE EmployeeId = @employeeId";

            public const string FLAT_FOLDERS_FOR_INITIATOR = @"
                SELECT FolderId, [Name], IsPrivate, CreateDate FROM {0}
                WHERE EmployeeId = @employeeId";
        }

        public class PermissionsSql
        {
            public const string ADD_SINGLE = @"
                INSERT INTO {0}
                (
                    CatalogId,
                    FolderId,
                    EmployeeId
                )
                VALUES
                (
                    @CatalogId,
                    @FolderId,
                    @EmployeeId
                );";

            public const string GET_USER_FOLDER_PERMISSIONS = @"
                SELECT FolderId FROM {0}                   
                WHERE EmployeeId = @employeeId
                    AND CatalogId = @catalogId";

            public const string HAS_USER_PERMISSION_FOR_FOLDER = @"
                SELECT COUNT(1) FROM {0}
                WHERE EmployeeId = @employeeId
                    AND CatalogId = @catalogId
                    AND FolderId = @folderId";

            public const string GET_USERNAME_BY_USER_ID = @"
                SELECT TOP(1) UserName, FirstName, LastName
                FROM {0}
                WHERE Id = @employeeId";
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
                    [EmployeeId],
                    [FileName],
                    [Timestamp],
                    [FileSize],
                    [FinishedUpload],
                    [Data]
                )
                VALUES
                (
                    @employeeId,
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
                    EmployeeId = @employeeId 
                    AND FinishedUpload = 0
                    AND FileName = @fileName";

            public const string UPDATE_BLOB_DATA = @"
                UPDATE {0}
                SET FinishedUpload = 1
                OUTPUT INSERTED.BlobId
                WHERE EmployeeId = @employeeId
                    AND FinishedUpload = 0
                    AND FileName = @fileName";

            public const string UPDATE_BLOB_DATA_WITH_OLD_FILENAME = @"
                UPDATE {0}
                SET FinishedUpload = 1
                OUTPUT INSERTED.BlobId
                WHERE EmployeeId = @employeeId
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
            public const string ADD_NEW_MESSAGE = @"
                INSERT INTO {0}
                (
                    [Type],
                    [Published],
                    [serializedData]
                )
                VALUES
                (
                    @Type,
                    @Published,
                    @serializedData
                ); SELECT CAST (SCOPE_IDENTITY() AS INT)";

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
                    [EmployeeId],
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
                    @EmployeeId,
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
                      CatalogId = @catalogId                   
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
                WHERE FolderId IN @accessibleFolders
                    AND CHARINDEX('{1}', Filename) > 0 
                    OR CHARINDEX('{1}', FileType) > 0
                    AND CatalogId = @CatalogId";

            public const string COUNT_FILES_FOR_EMPLOYEE = @"
                SELECT COUNT(*) FROM {0}
                WHERE EmployeeId = @employeeId";

        }
    }
}
