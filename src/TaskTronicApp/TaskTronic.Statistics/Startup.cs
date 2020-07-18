namespace TaskTronic.Statistics
{
    using Data;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Services.FolderViews;
    using Services.Statistics;
    using TaskTronic.Infrastructure;
    using TaskTronic.Services;
    using TaskTronic.Statistics.Messages;

    public class Startup
    {
        public Startup(IConfiguration configuration) 
            => this.Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
            => services
                .AddApiService<StatisticsDbContext>(this.Configuration)
                .AddTransient<IDbSeeder, StatisticsDbSeeder>()
                .AddTransient<IStatisticsService, StatisticsService>()
                .AddTransient<IFolderViewService, FolderViewService>()
                .AddMessaging(
                    useHangfireForPublishers: false,
                    configuration: null,
                    typeof(FileUploadedConsumer),
                    typeof(FolderCreatedConsumer),
                    typeof(FolderOpenedConsumer),
                    typeof(FileDeletedConsumer),
                    typeof(FolderDeletedConsumer));

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            => app
                .UseApiService(env)
                .Initialize();
    }
}
