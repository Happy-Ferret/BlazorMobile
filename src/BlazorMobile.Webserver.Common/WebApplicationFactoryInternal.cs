using BlazorMobile.Common.Interfaces;
using BlazorMobile.Common.Services;
using BlazorMobile.Webserver.Common.Consts;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using BlazorMobile.Webserver.Common.Interop;

[assembly: InternalsVisibleTo("BlazorMobile")]
[assembly: InternalsVisibleTo("BlazorMobile.Android")]
[assembly: InternalsVisibleTo("BlazorMobile.iOS")]
[assembly: InternalsVisibleTo("BlazorMobile.UWP")]
[assembly: InternalsVisibleTo("BlazorMobile.Webserver.Mono")]
[assembly: InternalsVisibleTo("BlazorMobile.Webserver.UWP")]
namespace BlazorMobile.Webserver.Common
{
    internal static class WebApplicationFactoryInternal
    {
        private static bool _firstCall = true;

        /// <summary>
        /// Init the WebApplicationFactory with the given app stream resolver.
        /// Shorthand for <see cref="WebApplicationFactory.RegisterAppStreamResolver" />
        /// </summary>
        /// <param name="appStreamResolver"></param>
        internal static void Init(Func<Stream> appStreamResolver)
        {
            RegisterAppStreamResolver(appStreamResolver);
        }

        internal static void Init()
        {
            if (_firstCall)
            {
                //Do something in the future if required
                _firstCall = false;
            }
        }


        private static Func<Stream> _appResolver = null;

        internal static Func<Stream> GetAppStreamResolver()
        {
            return _appResolver;
        }

        /// <summary>
        /// Register how you get the Stream to your Blazor zipped application
        /// <para>For a performant unique entry point, getting a Stream from an</para>
        /// <para>Assembly manifest resource stream is recommended.</para>
        /// <para></para>
        /// <para>See <see cref="System.Reflection.Assembly.GetManifestResourceStream(string)"/></para>
        /// </summary>
        internal static void RegisterAppStreamResolver(Func<Stream> resolver)
        {
            _appResolver = resolver;

            //Calling Init if not yet done. This will be a no-op if already called
            //We register init like this, as because of some linker problem with Xamarin,
            //we need to call the initializer from the "specialized project" (like Android)
            //that init itself and his components/renderer, then initializing this
            //
            //As iOS and UWP doesn't need a specific initialize at the moment but we may need
            //to call a generic init, the generic init is call in RegisterAppStream
            Init();
        }

        private static ZipArchive _zipArchive = null;
        private static object _zipLock = new object();
        internal static ZipArchive GetZipArchive()
        {
            if (_zipArchive != null)
                return _zipArchive;

            //Stream is not disposed as it can be called in the future
            //Data source must be ideally loaded as a ressource like <see cref="Assembly.GetManifestResourceStream" /> for memory performance
            Stream dataSource = _appResolver();
            _zipArchive = new ZipArchive(dataSource, ZipArchiveMode.Read);

            return _zipArchive;
        }

        internal static MemoryStream GetResourceStream(string path)
        {
            if (GetAppStreamResolver() == null)
            {
                throw new NullReferenceException("The Blazor app resolver was not set! Please call WebApplicationFactory.RegisterAppStreamResolver method before launching your app");
            }

            MemoryStream data = null;

            lock (_zipLock)
            {
                try
                {
                    ZipArchive archive = GetZipArchive();

                    //Data will contain the found file, outputed as a stream
                    data = new MemoryStream();
                    ZipArchiveEntry entry = archive.GetEntry(path);

                    if (entry != null)
                    {
                        Stream entryStream = entry.Open();
                        entryStream.CopyTo(data);
                        entryStream.Dispose();
                        data.Seek(0, SeekOrigin.Begin);
                    }
                    else
                    {
                        data?.Dispose();
                        data = null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            return data;
        }

        internal static byte[] GetResource(string path)
        {
            MemoryStream content = GetResourceStream(path);
            if (content == null)
                return null;
            byte[] resultSet = content?.ToArray();
            content?.Dispose();

            return resultSet;
        }

        internal static string GetContentType(string path)
        {
            if (path.EndsWith(".wasm"))
            {
                return "application/wasm";
            }
            if (path.EndsWith(".dll"))
            {
                return "application/octet-stream";
            }

            //No critical mimetypes to check
            return MimeTypes.GetMimeType(path);
        }

        internal static string GetBaseURL()
        {
            return $"http://{GetLocalWebServerIP()}:{GetHttpPort()}";
        }

        internal const int DefaultHttpPort = 8888;

        private static int HttpPort = DefaultHttpPort;

        internal static int GetHttpPort()
        {
            return HttpPort;
        }

        /// <summary>
        /// Define the HTTP port used for the webserver of your application.
        /// Default is 8888.
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool SetHttpPort(int port = DefaultHttpPort)
        {
            if (port <= 1024)
            {
                throw new InvalidOperationException("Cannot bind a port in the reserved port area !");
            }

            HttpPort = port;
            return true;
        }

        internal static string GetQueryPath(string path)
        {
            path = WebUtility.UrlDecode(path.TrimStart('/'));

            if (string.IsNullOrEmpty(path))
            {
                //We are calling the index page. We must call index.html but hide it from url for Blazor routing.
                path = Constants.DefaultPage;
            }

            return path;
        }

        private static string _localIP = null;
        internal static string GetLocalWebServerIP()
        {
            if (_localIP == null)
            {
                //Some Android platform does not have a universal loopback address
                //This is intended to detect the Loopback entry point
                //Thanks to https://github.com/unosquare/embedio/issues/207#issuecomment-433437410
                var listener = new TcpListener(IPAddress.Loopback, 0);
                try
                {
                    listener.Start();
                    _localIP = ((IPEndPoint)listener.LocalEndpoint).Address.ToString();
                }
                catch
                {
                    _localIP = "localhost";
                }
                finally
                {
                    listener.Stop();
                }
            }

            return _localIP;
        }

        internal static async Task ManageRequest(IWebResponse response)
        {
            response.SetEncoding("UTF-8");

            string path = GetQueryPath(response.GetRequestedPath());

            var content = GetResourceStream(path);

            //Manage Index content
            if (path == Constants.DefaultPage)
            {
                content = ManageIndexPageRendering(content);
            }

            response.AddResponseHeader("Cache-Control", "no-cache");
            response.AddResponseHeader("Access-Control-Allow-Origin", GetBaseURL());

            if (content == null)
            {
                //Content not found
                response.SetStatutCode(404);
                response.SetReasonPhrase("Not found");
                response.SetMimeType("text/plain");
                return;
            }

            response.SetStatutCode(200);
            response.SetReasonPhrase("OK");
            response.SetMimeType(GetContentType(path));
            await response.SetDataAsync(content);
        }

        private static Func<string, string> _defaultPageDelegate;

        internal static Func<string, string> GetDefaultPageResultDelegate()
        {
            return _defaultPageDelegate;
        }

        public static void SetDefaultPageResult(Func<string, string> defaultPageDelegate)
        {
            _defaultPageDelegate = defaultPageDelegate;
        }

        internal static MemoryStream ManageIndexPageRendering(MemoryStream originalContent)
        {
            string indexContent = Encoding.UTF8.GetString(originalContent.ToArray());
            originalContent.Dispose();

            //Do user logic
            var userDelegate = GetDefaultPageResultDelegate();
            if (userDelegate != null)
            {
                indexContent = userDelegate(indexContent);
            }

            //Do BlazorContextBridgeLogic
            //Get content to INJECT
            string injectableContent = ContextBridgeHelper.GetInjectableJavascript(false).Replace("%_contextBridgeURI%", GetContextBridgeURI());
            indexContent = indexContent.Replace("</blazorXamarin>", "</blazorXamarin>\r\n<script type=\"application/javascript\">" + injectableContent + "\r\n</script>\r\n\r\n");

            return new MemoryStream(Encoding.UTF8.GetBytes(indexContent));
        }

        internal static bool _debugFeatures = false;

        /// <summary>
        /// Add additionals features like debugging with Mozilla WebIDE for Android platform, allowing CORS request...
        /// </summary>
        public static void EnableDebugFeatures(bool value = true)
        {
            _debugFeatures = value;
        }

        internal static bool AreDebugFeaturesEnabled()
        {
            return _debugFeatures;
        }

        private const string _contextBridgeRelativeURI = "/contextBridge";

        internal static string GetContextBridgeRelativeURI()
        {
            return _contextBridgeRelativeURI;
        }

        internal static string GetContextBridgeURI()
        {
            return GetBaseURL().Replace("http://", "ws://") + GetContextBridgeRelativeURI();
        }

        private static IWebApplicationFactory _factory;

        internal static IWebApplicationFactory GetWebApplicationFactoryImplementation()
        {
            return _factory;
        }

        internal static void SetWebApplicationFactoryImplementation(IWebApplicationFactory factory)
        {
            _factory = factory;
        }
    }
}
