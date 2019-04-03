using BlazorMobile.Webserver.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace BlazorMobile.Webserver.Common
{
    public interface IWebApplicationFactory
    {
        void EnableDebugFeatures(bool value = true);
        bool IsStarted();
        void RegisterAppStreamResolver(Func<Stream> resolver);
        ZipArchive GetZipArchive();
        MemoryStream GetResourceStream(string path);
        byte[] GetResource(string path);
        string GetContentType(string path);
        bool SetHttpPort(int port);
        Func<string, string> GetDefaultPageResultDelegate();
        void SetDefaultPageResult(Func<string, string> defaultPageDelegate);
        int GetHttpPort();
        string GetLocalWebServerIP();
        string GetBaseURL();
        string GetQueryPath(string path);
        MemoryStream ManageIndexPageRendering(MemoryStream originalContent);
        Task ManageRequest(IWebResponse response);
        void Init(Func<Stream> appStreamResolver);
        void Init();
        string GetContextBridgeRelativeURI();
        string GetContextBridgeURI();
        bool AreDebugFeaturesEnabled();
        void StartWebServer();
        void StopWebServer();
        event EventHandler ServerStarted;
    }
}
