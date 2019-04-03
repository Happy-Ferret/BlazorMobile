using BlazorMobile.Webserver.Common;
using Microsoft.AspNetCore.Http;
using System.IO;
using Windows.Web.Http;
using System.Threading.Tasks;

namespace BlazorMobile.Webserver.Mono.Interop
{
    public class AspNetCoreWebResponse : IWebResponse
    {
        private Stream tempStream = new MemoryStream();
        private string contentType = "text/plain";
        private HttpRequest _request;
        private HttpResponseMessage _response;

        public AspNetCoreWebResponse(HttpRequest request, HttpResponseMessage response)
        {
            _request = request;
            _response = response;
        }

        public void AddResponseHeader(string key, string value)
        {
            _response.Headers.Add(key, value);
        }

        public string GetRequestedPath()
        {
            return _request.Path;
        }

        internal Stream GetBodyStream()
        {
            return tempStream;
        }

        internal void DisposeBodyContent()
        {
            if (tempStream != null)
            {
                try
                {
                    tempStream.Dispose();
                }
                catch (System.Exception)
                {
                }
            }
        }

        public Task SetDataAsync(MemoryStream data)
        {
            //Sanity check
            data.Seek(0, SeekOrigin.Begin);

            tempStream = data;

            return Task.CompletedTask;
        }

        public void SetEncoding(string encoding)
        {
            //No-op , encoding is readonly
        }

        public string GetMimeType()
        {
            return contentType;
        }

        public void SetMimeType(string mimetype)
        {
            contentType = mimetype;
        }

        public void SetReasonPhrase(string reasonPhrase)
        {
            _response.ReasonPhrase = reasonPhrase;
        }

        public void SetStatutCode(int statutCode)
        {
            _response.StatusCode = (HttpStatusCode)statutCode;
        }

        public HttpResponseMessage GetHttpResponseMessage()
        {
            return _response;
        }
    }
}
