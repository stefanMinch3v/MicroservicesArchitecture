namespace TaskTronic.Drive
{
    using Data;
    using Data.DapperRepo;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Services.Catalogs;
    using Services.Employees;
    using Services.Files;
    using Services.Folders;
    using Services.Messages;
    using TaskTronic.Drive.Services.CompanyDepartments;
    using TaskTronic.Infrastructure;
    using TaskTronic.Services;

    public class Startup
    {
        public Startup(IConfiguration configuration) 
            => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
            => services
                .AddApiService<DriveDbContext>(this.Configuration, "/api/files/DownloadFile")
                .AddSingleton<IDbConnectionFactory, DbConnectionFactory>()
                .AddTransient<IDbSeeder, DriveDbSeeder>()
                .AddTransient<IFileService, FileService>()
                .AddTransient<IMessageService, MessageService>()
                .AddTransient<IEmployeeService, EmployeeService>()
                .AddTransient<ICatalogService, CatalogService>()
                .AddTransient<IFolderService, FolderService>()
                .AddTransient<ICompanyDepartmentsService, CompanyDepartmentsService>()
                .AddTransient<IPermissionsDAL, PermissionsDAL>()
                .AddTransient<IFileDAL, FileDAL>()
                .AddTransient<ICatalogDAL, CatalogDAL>()
                .AddTransient<IFolderDAL, FolderDAL>()
                .AddMessaging(useHangfireForPublishers: true, configuration: this.Configuration);

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            => app
                .UseApiService(env)
                .Initialize();
    }
}
