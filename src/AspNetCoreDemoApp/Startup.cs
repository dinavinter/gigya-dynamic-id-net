using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DSStore;
using FluentSiren.AspNetCore.Mvc.Formatters;
using Gigya.Identity.Client;
using Gigya.Identity.Client.Models;
using Http.Options;
using Http.Options.Tracing.OpenTelemetry;
using InteractionApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Trace;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace AspNetCoreDemoApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddHttpsRedirection(options => { options.HttpsPort = 443; })
                .AddMvcCore(options => options.OutputFormatters.Add(new SystemTextJsonOutputFormatter(
                    new JsonSerializerOptions()
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    })))
                .AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy",
                        builder => builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader());
                });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                           ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            services.AddControllersWithViews();

            services.AddRazorPages(x =>
                {
                    x.Conventions
                        .AddFolderApplicationModelConvention("/GigyaProxy",
                            model => model.Filters.Add(new ProxyPageFilter()));

                    x.Conventions
                        .AddPageApplicationModelConvention("/proxy",
                            model => model.Filters.Add(new ProxyPageFilter()));
                }
            );

            services.AddScoped(sp =>
                {
                    sp.GetRequiredService<IHttpContextAccessor>().HttpContext.Items
                        .TryGetValue(typeof(GigyaOP), out var op);

                    return op as GigyaOP ?? new GigyaOP()
                    {
                        DataCenter = "us1",
                        ApiKey = "3__NKd98KtcRCL_Z98TO7bbTtMhZqe83A4hOjMA2wblxL8PAoduwgW9FTvdQ6OqYIB"
                    };
                }
            );

            services.AddTransient<IActionContextAccessor, ActionContextAccessor>();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();

            // services.AddDsStore("loyalty-program");
            // services.AddDsStore("loved-items");
            services.AddDsStore();

            // services
            //     .ConfigureAll<OpenTelemetryOptions>(openTelemetryOptions =>
            //     {
            //
            //         openTelemetryOptions.ConfigureBuilder += providerBuilder => providerBuilder.AddConsoleExporter();
            //         openTelemetryOptions.ConfigureBuilder += providerBuilder => providerBuilder.AddHttpClientInstrumentation();
            //     });

            services.AddOpenTelemetryTracing(providerBuilder =>
            {
                providerBuilder.AddHttpClientInstrumentation();

                providerBuilder.AddConsoleExporter();
            });


            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();

                c.SwaggerDoc("v1", new OpenApiInfo {Title = "WebApiDevoTo", Version = "v1"});
            });


            services.AddLogging(builder => builder.AddConsole());
            services.AddHttpLogging(logging =>
            {
                logging.LoggingFields = HttpLoggingFields.All;
                logging.RequestHeaders.Add("sec-ch-ua");
                logging.ResponseHeaders.Add("MyResponseHeader");
                logging.MediaTypeOptions.AddText("application/javascript");
                logging.RequestBodyLogLimit = 4096;
                logging.ResponseBodyLogLimit = 4096;
            });
            services.AddInteraction();
            services.AddMvc(options => options.OutputFormatters.Add(new SystemTextJsonOutputFormatter(
                new JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                })));

            services.AddControllers().AddJsonOptions(j =>
            {
                // var stockConverterOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                j.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                // j.JsonSerializerOptions.IgnoreNullValues = true;
                // stockConverterOptions.Converters.Add(new JsonStringEnumConverter());
                // var stockConverter = new StocksConverter(stockConverterOptions);
                //
                // j.JsonSerializerOptions.Converters.Add(stockConverter);
            });


        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            app.UseForwardedHeaders();

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DYNO")))
            {
                Console.WriteLine("Use https redirection");
                app.UseHttpsRedirection();
            }

            app
                .UseRouting()
                .UseDefaultFiles()
                .UseStaticFiles()
                .UseCors("CorsPolicy")
                .UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });

            app.UseStaticFiles();
             app.UseSpaStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.ShowCommonExtensions();
                c.ConfigObject.DeepLinking = true;
                // c.Interceptors.RequestInterceptorFunction =
                //     " { (req) =>console.log(req);  return req; }";
                // c.DefaultModelRendering(new  ModelRendering()
                // {
                //
                // });
                c.SwaggerEndpoint("/swagger/v1/swagger.json",
                    "WebApiDevoTo v1");
            })
               ;
            app.UseReDoc(c =>
            {
                c.DocumentTitle = "REDOC API Documentation";
                c.SpecUrl = "/swagger/v1/swagger.json";

            });

            app.UseHttpLogging();
            app.UseInteraction();
            //
            // app.UseSpa(spa =>
            // {
            //     spa.Options.SourcePath = "../../";
            //
            //     if (env.IsDevelopment())
            //     {
            //         spa.UseReactDevelopmentServer(npmScript: "start");
            //     }
            // });
        }
    }
}