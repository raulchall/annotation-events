using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Prometheus;
using SlackAppBackend.Configuration;
using SlackAppBackend.Utils.Interfaces;
using SlackAppBackend.Services;
using SlackAppBackend.Utils;
using SystemEvents.Api.Client.CSharp.Contracts;
using SystemEvents.Api.Client.CSharp;
using SlackAppBackend.Middlewares;

namespace SlackAppBackend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseHttpMetrics();

            app.UseAuthorization();
            
            app.UseMiddleware<SlackRequestMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = new AppConfiguration();

            configuration.ValidateAppConfiguration();
            
            // Inject the configuration
            services.AddSingleton<IAppConfiguration>(provider => configuration);

            services.AddHttpClient<ISystemEventsClient, SystemEventsClient>((provider, client) =>
            {
                client.BaseAddress = configuration.SystemEventServiceUri;
            });

            services.AddHttpClient<ICategoriesClient, CategoriesClient>((provider, client) =>
            {
                client.BaseAddress = configuration.SystemEventServiceUri;
            });
            
            services.AddSingleton<ISlackModalTemplateBuilder, SlackModalTemplateBuilder>();
            services.AddSingleton<IMonitoredSystemEventServiceClient, MonitoredSystemEventServiceClient>();
          
            services.AddHttpClient<SlackApiService>();

            services.AddHealthChecks();

            services.AddControllers()
                    .AddNewtonsoftJson(
                        options =>
                        {
                            options.SerializerSettings.Converters.Add(
                                new StringEnumConverter(new CamelCaseNamingStrategy()));
                                
                            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                        });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerDocument(settings =>
            {
                settings.PostProcess = document =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "Slack App API Backend";
                    document.Info.Description = "REST API for System Events Slack App";
                };
            });
        }
    }
}
