using BlazorMobile.Webserver.Common;
using BlazorMobile.Webserver.UWP.Controller;
using BlazorMobile.Webserver.UWP.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.WebSockets;

namespace BlazorMobile.Webserver.UWP
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services
                .AddMvc(options =>
                {
                options.OutputFormatters.RemoveType<TextOutputFormatter>();
                options.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();

                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IApplicationLifetime applicationLifetime)
        {
            applicationLifetime.ApplicationStarted.Register(() =>
            {
                AspNetCoreWebApplicationFactory factory = WebApplicationFactoryInternal.GetWebApplicationFactoryImplementation() as AspNetCoreWebApplicationFactory;
                factory.SetIsStarted(true);
                factory.InvokeServerStarted();
            });

            applicationLifetime.ApplicationStopped.Register(() =>
            {
                ((AspNetCoreWebApplicationFactory)WebApplicationFactoryInternal.GetWebApplicationFactoryImplementation()).SetIsStarted(false);
            });

            applicationLifetime.ApplicationStopping.Register(() =>
            {
                ((AspNetCoreWebApplicationFactory)WebApplicationFactoryInternal.GetWebApplicationFactoryImplementation()).SetIsStarted(false);
            });

            if (WebApplicationFactoryInternal.GetWebApplicationFactoryImplementation().AreDebugFeaturesEnabled())
            {
                app.UseCors();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //var webSocketOptions = new WebSocketOptions()
            //{
            //    KeepAliveInterval = TimeSpan.FromSeconds(120),
            //    ReceiveBufferSize = 4 * 1024
            //};
            //webSocketOptions.AllowedOrigins.Add(WebApplicationFactoryInternal.GetWebApplicationFactoryImplementation().GetBaseURL());


            //app.UseWebSockets(webSocketOptions);
            //app.Use(async (context, next) =>
            //{
            //    if (context.Request.Path == WebApplicationFactoryInternal.GetWebApplicationFactoryImplementation().GetContextBridgeRelativeURI())
            //    {
            //        if (context.WebSockets.IsWebSocketRequest)
            //        {
            //            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            //            await BlazorContextBridge.OnMessageReceived(context, webSocket);
            //        }
            //        else
            //        {
            //            context.Response.StatusCode = 400;
            //        }
            //    }
            //    else
            //    {
            //        await next();
            //    }

            //});

            //app.UseSignalR(routes =>
            //{
            //    routes.MapHub<Chat>("/chat");
            //});

            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller}/{action}");
                routes.MapRoute("blazor", "{*url}", defaults: new { controller = "Blazor", action = "BlazorAppRessources" });
            });
        }
    }
}