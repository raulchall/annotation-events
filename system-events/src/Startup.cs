using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Prometheus;
using SystemEvents.Configuration;
using SystemEvents.ServiceExtensions;
using SystemEvents.Utils;
using SystemEvents.Utils.Interfaces;

namespace SystemEvents
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

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SystemEventsApi");
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseHttpMetrics();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = new AppConfiguration();
            
            // Inject the configuration
            services.AddSingleton<IAppConfiguration>(provider => configuration);
            services.AddSingleton<IElasticsearchClientConfiguration>(provider => configuration);

            services.AddSingleton<IElasticsearchTimeStampFactory, ElasticsearchTimeStampFactory>();
            services.AddElasticsearch(configuration);
            services.AddSingleton<IMonitoredElasticsearchClient, PrometheusMonitoredElasticsearchClient>();

            services.AddControllers()
                    .AddNewtonsoftJson(
                        options =>
                        {
                            options.SerializerSettings.Converters.Add(
                                new StringEnumConverter(new CamelCaseNamingStrategy()));
                                
                            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                        });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SystemEventsApi", Version = "v1" });
            });
        }
    }
}
