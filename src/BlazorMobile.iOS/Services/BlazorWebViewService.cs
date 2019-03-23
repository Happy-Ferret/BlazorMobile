using BlazorMobile.iOS.Renderer;
using BlazorMobile.Services;
using BlazorMobile.Webserver.Common;
using BlazorMobile.Webserver.Mono.Services;
using System;
using System.IO;

namespace BlazorMobile.iOS.Services
{
    public class BlazorWebViewService
    {
        private static IWebApplicationFactory _factory = null;

        private static void InitComponent()
        {
            if (_factory == null)
            {
                //We must resolve current WebApplicationFactory implementation
                _factory = new EmbedIOWebApplicationFactory();
                WebApplicationFactoryInternal.SetWebApplicationFactoryImplementation(_factory);
            }

            BlazorWebViewRenderer.BlazorInit();
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
