namespace TaskTronic.Drive
{
    using Data;
    using Data.DapperRepo;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Services.Catalogs;
    using Services.Files;
    using Services.Folders;
    using TaskTronic.Drive.Services.Employees;
    using TaskTronic.Infrastructure;
    using TaskTronic.Services;

    public class Startup
    {
        public Startup(IConfiguration configuration) 
            => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
            => services
                .AddApiService<DriveDbContext>(this.Configuration)
                .AddSingleton<IDbConnectionFactory, DbConnectionFactory>()
                .AddTransient<IDbSeeder, DriveDbSeeder>()
                .AddTransient<IFileService, FileService>()
                .AddTransient<IEmployeeService, EmployeeService>()
                .AddTransient<ICatalogService, CatalogService>()
                .AddTransient<IFolderService, FolderService>()
                .AddTransient<IPermissionsDAL, PermissionsDAL>()
                .AddTransient<IFileDAL, FileDAL>()
                .AddTransient<ICatalogDAL, CatalogDAL>()
                .AddTransient<IFolderDAL, FolderDAL>();

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            => app
                .UseApiService(env)
                .Initialize();
    }
}
