using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BlazorMobile.UWP.BackgroundTask
{
    class UwpStartup
    {
        public UwpStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            #region Standard initialization

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSignalR();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            #endregion Standard initialization

            var manager = new ApplicationPartManager();
            var assemblies = new Assembly[] { Assembly.GetExecutingAssembly(), Assembly.Load("BlazorMobile.InteropBlazorApp") };

            foreach (var assembly in assemblies)
            {
                var factory = ApplicationPartFactory.GetApplicationPartFactory(assembly);

                foreach (var part in factory.GetApplicationParts(assembly))
                {
                    manager.ApplicationParts.Add(part);
                }
            }

            services.AddSingleton(manager);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            //app.UseCookiePolicy();

            //app.UseSignalR(routes =>
            //{
            //    routes.MapHub<Chat>("/chat");
            //});

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}