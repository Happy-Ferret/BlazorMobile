using BlazorMobile.Services;
using BlazorMobile.Webserver.Common;
using System.Web;
using Xamarin.Forms;

namespace BlazorMobile.Components
{
    public class BlazorWebView : WebView, IBlazorWebView
    {
        public BlazorWebView()
        {
        }



        private bool uwpFirstLaunch = true;

        private void SetLaunchURISource()
        {
            string url = WebApplicationFactory.GetBaseURL();

            if (Device.RuntimePlatform == Device.UWP)
            {
                url += "/";
            }

            Source = new UrlWebViewSource()
            {
                Url = url
            };
            blazorAppLaunched = true;
        }

        public void LaunchBlazorApp()
        {
            switch (Device.RuntimePlatform)
            {
                case Device.UWP:
                    if (uwpFirstLaunch)
                    {
                        WebApplicationFactoryInternal.GetWebApplicationFactoryImplementation().ServerStarted += BlazorWebView_ServerStarted;
                    }
                    else
                    {
                        SetLaunchURISource();
                    }
                    break;
                default:
                    SetLaunchURISource();
                    break;
            }
        }

        private void BlazorWebView_ServerStarted(object sender, System.EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (uwpFirstLaunch)
                {
                    uwpFirstLaunch = false;
                    SetLaunchURISource();
                    Reload();

                    WebApplicationFactoryInternal.GetWebApplicationFactoryImplementation().ServerStarted -= BlazorWebView_ServerStarted;
                }
            });
        }

        private bool blazorAppLaunched = false;

        public View GetView()
        {
            return this;
        }
    }
}
