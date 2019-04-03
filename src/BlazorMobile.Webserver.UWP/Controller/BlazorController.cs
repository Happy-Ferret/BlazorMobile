using BlazorMobile.Webserver.Common;
using BlazorMobile.Webserver.Mono.Interop;
using BlazorMobile.Webserver.UWP.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace BlazorMobile.Webserver.UWP.Controller
{
    public class BlazorController : Microsoft.AspNetCore.Mvc.Controller
    {
        public async Task<IActionResult> BlazorAppRessources()
        {
            try
            {
                HttpResponseMessage responseMessage = new HttpResponseMessage();

                AspNetCoreWebResponse response = new AspNetCoreWebResponse(HttpContext.Request, responseMessage);
                await WebApplicationFactoryInternal.GetWebApplicationFactoryImplementation().ManageRequest(response);

                // Here we ask the framework to dispose the response object a the end of the user resquest
                this.HttpContext.Response.RegisterForDispose(responseMessage);

                return new HttpResponseMessageResult(response);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }
    }
}
