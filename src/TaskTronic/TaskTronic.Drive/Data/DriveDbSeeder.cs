namespace TaskTronic.Drive.Data
{
    using Microsoft.EntityFrameworkCore.Internal;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TaskTronic.Drive.Data.Models;
    using TaskTronic.Drive.Services.Files;
    using TaskTronic.Services;

    public class DriveDbSeeder : IDbSeeder
    {
        private const int ITERATIONS = 10;
        private readonly DriveDbContext dbContext;
        private readonly IFileService fileService;

        public DriveDbSeeder(
            DriveDbContext dbContext,
            IFileService fileService)
        {
            this.dbContext = dbContext;
            this.fileService = fileService;
        }

        public void SeedData()
        {
            Task.Run(async () =>
            {
                if (this.dbContext.Catalogs.Any()
                || this.dbContext.Employees.Any()
                || this.dbContext.Folders.Any())
                {
                    return;
                }

                // catalog
                var catalog = new Catalog { CompanyId = 1, DepartmentId = 1 };

                this.dbContext.Catalogs.Add(catalog);
                this.dbContext.SaveChanges();

                // employee
                var employee = new Employee
                {
                    Email = "asd@abv.bg",
                    UserId = "43fc03f9-c98e-49fd-b9e2-c6008540df1e" // asd - test user
                };

                this.dbContext.Employees.Add(employee);
                this.dbContext.SaveChanges();

                // root folder
                var rootFolder = new Folder
                {
                    CatalogId = catalog.CatalogId,
                    Name = "Root",
                    CreateDate = DateTime.UtcNow,
                    EmployeeId = employee.EmployeeId
                };

                this.dbContext.Folders.Add(rootFolder);
                this.dbContext.SaveChanges();

                // random parent folders
                var parentFolders = new List<Folder>();

                for (int i = 0; i < ITERATIONS; i++)
                {
                    var folder = new Folder
                    {
                        CatalogId = catalog.CatalogId,
                        CreateDate = DateTime.UtcNow,
                        Name = $"Awesome folder {i}",
                        RootId = rootFolder.FolderId,
                        EmployeeId = employee.EmployeeId,
                        ParentId = rootFolder.FolderId
                    };

                    parentFolders.Add(folder);
                }

                this.dbContext.Folders.AddRange(parentFolders);
                this.dbContext.SaveChanges();

                var subFolders = new List<Folder>();

                // sub folders
                for (int i = 0; i < ITERATIONS / 2; i++)
                {
                    var subFolder = new Folder
                    {
                        CatalogId = catalog.CatalogId,
                        CreateDate = DateTime.UtcNow,
                        Name = $"Sub folder {i}",
                        RootId = rootFolder.FolderId,
                        ParentId = parentFolders[i].FolderId,
                        EmployeeId = employee.EmployeeId
                    };

                    subFolders.Add(subFolder);
                }

                this.dbContext.Folders.AddRange(subFolders);
                this.dbContext.SaveChanges();

                // random word/excel files
                for (int i = 0; i < ITERATIONS; i++)
                {
                    if (i % 2 == 0)
                    {
                        // word
                        for (int j = 0; j < 3; j++)
                        {
                            await this.fileService.CreateNewFileAsync(
                                catalog.CatalogId,
                                employee.EmployeeId,
                                parentFolders[i].FolderId,
                                NewFileType.Word);
                        }
                    }
                    else
                    {
                        // excel
                        for (int k = 0; k < 2; k++)
                        {
                            await this.fileService.CreateNewFileAsync(
                                catalog.CatalogId,
                                employee.EmployeeId,
                                parentFolders[i].FolderId,
                                NewFileType.Excel);
                        }
                    }
                }
            }).GetAwaiter().GetResult();
        }
    }
}
