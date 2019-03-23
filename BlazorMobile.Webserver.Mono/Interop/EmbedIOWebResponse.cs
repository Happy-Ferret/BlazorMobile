﻿using BlazorMobile.Webserver.Common;
using System.IO;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;

namespace BlazorMobile.Webserver.Mono.Interop
{
    public class EmbedIOWebResponse : IWebResponse
    {
        private IHttpRequest _request = null;
        private IHttpResponse _response = null;

        public EmbedIOWebResponse(IHttpRequest request, IHttpResponse response)
        {
            _request = request;
            _response = response;
        }

        public void AddResponseHeader(string key, string value)
        {
            _response.AddHeader(key, value);
        }

        public string GetRequestedPath()
        {
            return _request.Url.AbsolutePath;
        }

        public Task SetDataAsync(MemoryStream data)
        {
            //Sanity check
            data.Seek(0, SeekOrigin.Begin);

            return _response.BinaryResponseAsync(data, false, default);
        }

        public void SetEncoding(string encoding)
        {
            //No-op , encoding is readonly
        }

        public void SetMimeType(string mimetype)
        {
            _response.ContentType = mimetype;
        }

        public void SetReasonPhrase(string reasonPhrase)
        {
            _response.StatusDescription = reasonPhrase;
        }

        public void SetStatutCode(int statutCode)
        {
            _response.StatusCode = statutCode;
        }
    }
}
