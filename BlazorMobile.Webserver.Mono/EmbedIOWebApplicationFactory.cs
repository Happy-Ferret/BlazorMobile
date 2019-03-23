using BlazorMobile.Webserver.Common;
using BlazorMobile.Webserver.Mono.Controller;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Constants;
using Unosquare.Labs.EmbedIO.Modules;

[assembly: InternalsVisibleTo("BlazorMobile.Android")]
[assembly: InternalsVisibleTo("BlazorMobile.iOS")]
namespace BlazorMobile.Webserver.Mono.Services
{
    public class EmbedIOWebApplicationFactory : IWebApplicationFactory
    {
        private Unosquare.Labs.EmbedIO.WebServer server;
        private CancellationTokenSource serverCts = new CancellationTokenSource();

        private BlazorContextBridge blazorContextBridgeServer = null;

        private bool _isStarted = false;

        public bool IsStarted()
        {
            return _isStarted;
        }

        private Func<string, string> _defaultPageDelegate;

        internal Func<string, string> GetDefaultPageResultDelegate()
        {
            return _defaultPageDelegate;
        }

        public void SetDefaultPageResult(Func<string, string> defaultPageDelegate)
        {
            _defaultPageDelegate = defaultPageDelegate;
        }

        public int GetHttpPort()
        {
            return WebApplicationFactoryInternal.GetHttpPort();
        }

        private string _localIP = null;
        private string GetLocalWebServerIP()
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

        /// <summary>
        /// As test for the moment. 10 seconds max
        /// </summary>
        private const int NativeSocketTimeout = 1000;

        internal BlazorContextBridge GetBlazorContextBridgeServer()
        {
            return blazorContextBridgeServer;
        }

        public void StartWebServer()
        {
            if (IsStarted())
            {
                //Already started
                return;
            }

            server = new Unosquare.Labs.EmbedIO.WebServer(WebApplicationFactoryInternal.GetBaseURL(), RoutingStrategy.Regex);
            server.WithLocalSession();

            if (WebApplicationFactoryInternal.AreDebugFeaturesEnabled())
            {
                server.EnableCors();
            }

            server.RegisterModule(new WebApiModule());
            server.Module<WebApiModule>().RegisterController<BlazorController>();

            //Reference to the BlazorContextBridge Websocket service
            blazorContextBridgeServer = new BlazorContextBridge();

            server.RegisterModule(new WebSocketsModule());
            server.Module<WebSocketsModule>().RegisterWebSocketsServer(GetContextBridgeRelativeURI(), blazorContextBridgeServer);

            serverCts = new CancellationTokenSource();

            Task.Factory.StartNew(async () =>
            {
                Console.WriteLine("BlazorMobile: Starting Server...");
                await server.RunAsync(serverCts.Token);
            });
        }

        public void StopWebServer()
        {
            //In order to stop the waiting background thread
            try
            {
                serverCts.Cancel();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(EmbedIOWebApplicationFactory)}.{nameof(EmbedIOWebApplicationFactory.StopWebServer)} - {nameof(serverCts)}: {ex.Message}");
            }

            try
            {
                if (blazorContextBridgeServer != null)
                {
                    blazorContextBridgeServer.Dispose();
                    blazorContextBridgeServer = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(EmbedIOWebApplicationFactory)}.{nameof(EmbedIOWebApplicationFactory.StopWebServer)} - {nameof(blazorContextBridgeServer)}: {ex.Message}");
            }

            server?.Dispose();
            server = null;

            _isStarted = false;
        }

        public void RegisterAppStreamResolver(Func<Stream> resolver)
        {
            WebApplicationFactoryInternal.RegisterAppStreamResolver(resolver);
        }

        public ZipArchive GetZipArchive()
        {
            return WebApplicationFactoryInternal.GetZipArchive();
        }

        public MemoryStream GetResourceStream(string path)
        {
            return WebApplicationFactoryInternal.GetResourceStream(path);
        }

        public byte[] GetResource(string path)
        {
            return WebApplicationFactoryInternal.GetResource(path);
        }

        public string GetContentType(string path)
        {
            return WebApplicationFactoryInternal.GetContentType(path);
        }

        public bool SetHttpPort(int port)
        {
            return WebApplicationFactoryInternal.SetHttpPort(port);
        }

        Func<string, string> IWebApplicationFactory.GetDefaultPageResultDelegate()
        {
            return WebApplicationFactoryInternal.GetDefaultPageResultDelegate();
        }

        string IWebApplicationFactory.GetLocalWebServerIP()
        {
            return WebApplicationFactoryInternal.GetLocalWebServerIP();
        }

        public string GetBaseURL()
        {
            return WebApplicationFactoryInternal.GetBaseURL();
        }

        public string GetQueryPath(string path)
        {
            return WebApplicationFactoryInternal.GetQueryPath(path);
        }

        public MemoryStream ManageIndexPageRendering(MemoryStream originalContent)
        {
            return WebApplicationFactoryInternal.ManageIndexPageRendering(originalContent);
        }

        public Task ManageRequest(IWebResponse response)
        {
            return WebApplicationFactoryInternal.ManageRequest(response);
        }

        public void Init(Func<Stream> appStreamResolver)
        {
            WebApplicationFactoryInternal.Init(appStreamResolver);
        }

        public void Init()
        {
            WebApplicationFactoryInternal.Init();
        }

        public string GetContextBridgeRelativeURI()
        {
            return WebApplicationFactoryInternal.GetContextBridgeRelativeURI();
        }

        public string GetContextBridgeURI()
        {
            return WebApplicationFactoryInternal.GetContextBridgeURI();
        }

        public bool AreDebugFeaturesEnabled()
        {
            return WebApplicationFactoryInternal.AreDebugFeaturesEnabled();
        }

        public void EnableDebugFeatures(bool value = true)
        {
            WebApplicationFactoryInternal.EnableDebugFeatures(value);
        }
    }
}
