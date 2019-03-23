using BlazorMobile.Webserver.Common;
using BlazorMobile.Webserver.Mono.Interop;
using System;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Constants;
using Unosquare.Labs.EmbedIO.Modules;

namespace BlazorMobile.Webserver.Mono.Controller
{
    public class BlazorController : WebApiController
    {
        public BlazorController(IHttpContext context) : base(context)
        {
        }

        [WebApiHandler(HttpVerbs.Get, @"^(?!\/contextBridge.*$).*")]
        public async Task<bool> BlazorAppRessources()
        {
            try
            {
                IWebResponse response = new EmbedIOWebResponse(this.Request, this.Response);
                await WebApplicationFactoryInternal.GetWebApplicationFactoryImplementation().ManageRequest(response);
                return true;
            }
            catch (Exception ex)
            {
                return await this.JsonExceptionResponseAsync(ex);
            }
        }

        // You can override the default headers and add custom headers to each API Response.
        public override void SetDefaultHeaders() => this.NoCache();
    }
}
