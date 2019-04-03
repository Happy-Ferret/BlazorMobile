using BlazorMobile.UWP.Renderer;
using BlazorMobile.Services;
using System;
using System.IO;
using BlazorMobile.Webserver.Common;
using Xamarin.Forms;
using BlazorMobile.Common.Interfaces;
using BlazorMobile.Webserver.UWP.Services;

namespace BlazorMobile.UWP.Services
{
    public class BlazorWebViewService
    {
        private static IWebApplicationFactory _factory = null;

        private static void InitComponent()
        {
            if (_factory == null)
            {
                //Register IBlazorXamarinDeviceService for getting base metadata for Blazor
                DependencyService.Register<IBlazorXamarinDeviceService, BlazorXamarinDeviceService>();

                //We must resolve current WebApplicationFactory implementation
                _factory = new AspNetCoreWebApplicationFactory();
                WebApplicationFactoryInternal.SetWebApplicationFactoryImplementation(_factory);
            }

            BlazorWebViewRenderer.Init();
        }

        /// <summary>
        /// Init the WebApplicationFactory with the given app stream resolver.
        /// Shorthand for <see cref="WebApplicationFactory.RegisterAppStreamResolver" />
        /// </summary>
        /// <param name="appStreamResolver"></param>
        public static void Init(Func<Stream> appStreamResolver)
        {
            InitComponent();
            WebApplicationFactory.Init(appStreamResolver);
        }

        public static void Init()
        {
            InitComponent();
            WebApplicationFactory.Init();
        }
    }
}
