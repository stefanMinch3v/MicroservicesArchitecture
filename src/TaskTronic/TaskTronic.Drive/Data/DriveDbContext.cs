namespace TaskTronic.Drive.Data
{
    using Microsoft.EntityFrameworkCore;
    using System.Reflection;
    using Models;

    public class DriveDbContext : DbContext
    {
        public DriveDbContext(DbContextOptions<DriveDbContext> options)
            : base(options)
        {
        }

        public DbSet<Permission> Permissions { get; set; } // not implemented yet
        public DbSet<File> Files { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<Catalog> Catalogs { get; set; }
        public DbSet<Blobsdata> Blobsdata { get; set; }
        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(builder);
        }
    }
}

//CREATE TABLE[Catalogs]
//(
//	[CatId] int identity primary key,
//	[SiteId] int NULL,
//	[Type] int NOT NULL,
//	[Id]
//int NULL,
//	[CompanyNo] int NULL,
//	[DepartmentNo] int NULL,
//	[LeaseNo] int NULL,
//	[TenantNo] int NULL
//)
//GO
//CREATE TABLE[Folders]
//(
//	[FolderId] int identity primary key,
//	[SiteId] int NOT NULL,
//	[CatId] int NOT NULL,
//	[ParentId]
//int NULL,
//	[RootId] int NULL,
//	[Name] nvarchar(255) NOT NULL,
//	[IsPrivate] bit,
//	[CreatedBy] int NOT NULL	
//)
//GO
//CREATE TABLE[Permissions]
//(
//	[SiteId] int NOT NULL,
//	[CatId] int NOT NULL,
//	[FolderId] int NOT NULL,
//	[UserId]
//int NULL
//)
//GO
//CREATE TABLE[Files]
//(
//	[FileId] int identity primary key,
//	[SiteId] int NOT NULL,
//	[CatId] int NOT NULL,
//	[FolderId] int NOT NULL,
//	[BlobId] int NOT NULL,
//	[Filename] nvarchar(255) NOT NULL,
//	[Revision] nvarchar(25) NOT NULL,
//	[Filetype] nvarchar(50) NOT NULL,
//	[Filesize] bigint NOT NULL,
//	[ContentType] nvarchar(255) NOT NULL,
//	/* Point in time */
//	[CreateDate] datetimeoffset NOT NULL,
//	[UpdateDate] datetimeoffset NOT NULL,
//	[EndDate]
//datetimeoffset NULL,
//	[ReferenceId] int NULL,
//	[State] int NULL,
//	[UpdaterId] int NULL
//)
//GO
//CREATE TABLE[Blobsdata]
//(
//	[BlobId] int identity primary key,
//	[UserId] int NOT NULL,
//	[FileName] nvarchar(1000) NOT NULL,
//	[Filesize] bigint NOT NULL,
//	[FinishedUpload] bit NOT NULL DEFAULT(0),
//	[Timestamp] datetimeoffset NOT NULL,	
//	[Data] varbinary(MAX) NOT NULL
//)
