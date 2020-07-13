namespace TaskTronic.Drive.Data
{
    using Microsoft.EntityFrameworkCore;
    using System.Reflection;
    using Models;
    using TaskTronic.Data;

    public class DriveDbContext : MessageDbContext
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

        protected override Assembly ConfigurationsAssembly => Assembly.GetExecutingAssembly();
    }
}
