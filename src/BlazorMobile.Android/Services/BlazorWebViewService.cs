using BlazorMobile.Common.Interfaces;
using BlazorMobile.Droid.Renderer;
using BlazorMobile.Services;
using BlazorMobile.Webserver.Common;
using BlazorMobile.Webserver.Mono.Services;
using System;
using System.IO;
using Xamarin.Forms;

namespace BlazorMobile.Droid.Services
{
    [Android.Runtime.Preserve(AllMembers = true)]
    public static class BlazorWebViewService
    {
        private static IWebApplicationFactory _factory = null;

        private static void InitComponent(Android.App.Activity activity)
        {
            if (_factory == null)
            {
                //Register IBlazorXamarinDeviceService for getting base metadata for Blazor
                DependencyService.Register<IBlazorXamarinDeviceService, BlazorXamarinDeviceService>();

                //We must resolve current WebApplicationFactory implementation
                _factory = new EmbedIOWebApplicationFactory();
                WebApplicationFactoryInternal.SetWebApplicationFactoryImplementation(_factory);
            }

            BlazorGeckoViewRenderer.Init(activity);
        }

        /// <summary>
        /// Init the WebApplicationFactory with the given app stream resolver.
        /// Shorthand for <see cref="WebApplicationFactory.RegisterAppStreamResolver" />
        /// </summary>
        /// <param name="appStreamResolver"></param>
        public static void Init(Android.App.Activity activity, Func<Stream> appStreamResolver)
        {
            InitComponent(activity);
            WebApplicationFactory.Init(appStreamResolver);
        }

        public static void Init(Android.App.Activity activity)
        {
            InitComponent(activity);
            WebApplicationFactory.Init();
        }
    }
}
