using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Web.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using BlazorMobile.Webserver.Mono.Interop;
using System.IO;

namespace BlazorMobile.Webserver.UWP.Extensions
{
    public class HttpResponseMessageResult : IActionResult
    {
        private readonly AspNetCoreWebResponse _responseMessage;

        public HttpResponseMessageResult(AspNetCoreWebResponse responseMessage)
        {
            _responseMessage = responseMessage; // could add throw if null
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)_responseMessage.GetHttpResponseMessage().StatusCode;
            context.HttpContext.Response.ContentType = _responseMessage.GetMimeType();

            foreach (var header in _responseMessage.GetHttpResponseMessage().Headers)
            {
                context.HttpContext.Response.Headers.TryAdd(header.Key, new StringValues(header.Value));
            }

            using (Stream stream = _responseMessage.GetBodyStream())
            {
                await stream.CopyToAsync(context.HttpContext.Response.Body);
                await context.HttpContext.Response.Body.FlushAsync();
            }
        }
    }
}
