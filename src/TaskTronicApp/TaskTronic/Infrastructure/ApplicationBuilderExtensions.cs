namespace TaskTronic.Infrastructure
{
    using HealthChecks.UI.Client;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using TaskTronic.Services;

    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseApiService(
            this IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app
                //.UseHttpsRedirection()
                .UseRouting()
                .UseCors(builder =>
                {
                    builder.WithOrigins("http://localhost:4200");

                    builder.AllowAnyHeader()
                        .WithMethods("GET", "POST", "PUT", "DELETE")
                        .AllowCredentials();
                })
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(builder =>
                {
                    builder.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                    {
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse    
                    });

                    builder.MapControllers();
                });

            return app;
        }

        public static IApplicationBuilder Initialize(
            this IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var serviceProvider = serviceScope.ServiceProvider;

            var dbContext = serviceProvider.GetRequiredService<DbContext>();
            dbContext.Database.Migrate();

            var seeders = serviceProvider.GetServices<IDbSeeder>();
            foreach (var seeder in seeders)
            {
                seeder.SeedData();
            }

            return app;
        }

        public static IApplicationBuilder UseGatewayApiService(
            this IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app
                .UseHttpsRedirection()
                .UseRouting()
                .UseMiddleware<JwtHeaderAuthenticationMiddleware>()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(builder => builder.MapControllers());

            return app;
        }
    }
}
