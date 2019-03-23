using BlazorMobile.Webserver.Common;
using System;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BlazorMobile.Android")]
[assembly: InternalsVisibleTo("BlazorMobile.iOS")]
[assembly: InternalsVisibleTo("BlazorMobile.UWP")]
namespace BlazorMobile.Services
{
    public static class WebApplicationFactory
    {
        /// <summary>
        /// Register how you get the Stream to your Blazor zipped application
        /// <para>For a performant unique entry point, getting a Stream from an</para>
        /// <para>Assembly manifest resource stream is recommended.</para>
        /// <para></para>
        /// <para>See <see cref="System.Reflection.Assembly.GetManifestResourceStream(string)"/></para>
        /// </summary>
        public static void RegisterAppStreamResolver(Func<Stream> resolver)
        {
            WebApplicationFactoryInternal.GetWebApplicationFactoryImplementation().RegisterAppStreamResolver(resolver);
        }


        /// <summary>
        /// Define the HTTP port used for the webserver of your application.
        /// Default is 8888.
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool SetHttpPort(int port = WebApplicationFactoryInternal.DefaultHttpPort)
        {
            return WebApplicationFactoryInternal.GetWebApplicationFactoryImplementation().SetHttpPort(port);
        }


        public static void SetDefaultPageResult(Func<string, string> defaultPageDelegate)
        {
            WebApplicationFactoryInternal.GetWebApplicationFactoryImplementation().SetDefaultPageResult(defaultPageDelegate);
        }

        public static int GetHttpPort()
        {
            return WebApplicationFactoryInternal.GetWebApplicationFactoryImplementation().GetHttpPort();
        }

        public static string GetBaseURL()
        {
            return WebApplicationFactoryInternal.GetWebApplicationFactoryImplementation().GetBaseURL();
        }

        /// <summary>
        /// Init the WebApplicationFactory with the given app stream resolver.
        /// Shorthand for <see cref="WebApplicationFactory.RegisterAppStreamResolver" />
        /// </summary>
        /// <param name="appStreamResolver"></param>
        internal static void Init(Func<Stream> appStreamResolver)
        {
            WebApplicationFactoryInternal.GetWebApplicationFactoryImplementation().Init(appStreamResolver);
        }

        internal static void Init()
        {
            WebApplicationFactoryInternal.GetWebApplicationFactoryImplementation().Init();
        }

        /// <summary>
        /// Add additionals features like debugging with Mozilla WebIDE for Android platform, allowing CORS request...
        /// </summary>
        public static void EnableDebugFeatures(bool value = true)
        {
            WebApplicationFactoryInternal.GetWebApplicationFactoryImplementation().EnableDebugFeatures(value);
        }

        public static void StartWebServer()
        {
            WebApplicationFactoryInternal.GetWebApplicationFactoryImplementation().StartWebServer();
        }

        public static void StopWebServer()
        {
            WebApplicationFactoryInternal.GetWebApplicationFactoryImplementation().StopWebServer();
        }
    }
}
