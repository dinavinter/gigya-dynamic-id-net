using System;
using Gigya.Identity.Client;
using Gigya.Identity.Client.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreDemoApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddHttpsRedirection(options => { options.HttpsPort = 443; })
                .AddMvcCore()
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

        }

        public void Configure(IApplicationBuilder app)
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
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapDefaultControllerRoute();
                });

            app.UseStaticFiles();
            // app.UseSpaStaticFiles();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
            //
            // app.UseSpa(spa =>
            // {
            //     spa.Options.SourcePath = "ClientApp";
            //
            //     if (env.IsDevelopment())
            //     {
            //         spa.UseReactDevelopmentServer(npmScript: "start");
            //     }
            // });
        }
    }
}