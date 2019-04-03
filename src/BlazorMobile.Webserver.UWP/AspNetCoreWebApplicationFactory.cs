using BlazorMobile.Webserver.Common;
using BlazorMobile.Webserver.UWP;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("BlazorMobile.Android")]
[assembly: InternalsVisibleTo("BlazorMobile.iOS")]
namespace BlazorMobile.Webserver.UWP.Services
{
    public class AspNetCoreWebApplicationFactory : IWebApplicationFactory
    {
        private IWebHost server;
        private CancellationTokenSource serverCts = new CancellationTokenSource();

        private bool _isStarted = false;

        internal void SetIsStarted(bool value)
        {
            _isStarted = value;
        }

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

        #region OnServerStarted event for delayed start on UWP / Kestrel

        internal EventHandler m_ServerStarted;

        public event EventHandler ServerStarted
        {
            add
            {
                m_ServerStarted += value;
            }
            remove
            {
                m_ServerStarted -= value;
            }
        }

        internal void InvokeServerStarted()
        {
            m_ServerStarted?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public void StartWebServer()
        {
            if (IsStarted())
            {
                //Already started
                return;
            }

            serverCts = new CancellationTokenSource();

            Task.Factory.StartNew(async () =>
            {
                _isStarted = true;

                Console.WriteLine("BlazorMobile: Starting Server...");

                server = WebHost
                    .CreateDefaultBuilder()
                    .UseSetting("preventHostingStartup", "true")
                    .UseStartup<Startup>()
                    .UseUrls(WebApplicationFactoryInternal.GetBaseURL())
                    .Build();

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
                Console.WriteLine($"{nameof(AspNetCoreWebApplicationFactory)}.{nameof(AspNetCoreWebApplicationFactory.StopWebServer)} - {nameof(serverCts)}: {ex.Message}");
            }

            try
            {
                //if (blazorContextBridgeServer != null)
                //{
                //    blazorContextBridgeServer.Dispose();
                //    blazorContextBridgeServer = null;
                //}
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"{nameof(AspNetCoreWebApplicationFactory)}.{nameof(AspNetCoreWebApplicationFactory.StopWebServer)} - {nameof(blazorContextBridgeServer)}: {ex.Message}");
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
