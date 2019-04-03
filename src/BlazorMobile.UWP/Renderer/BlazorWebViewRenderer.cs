using BlazorMobile.Components;
using BlazorMobile.UWP.Renderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(BlazorWebView), typeof(BlazorWebViewRenderer))]
namespace BlazorMobile.UWP.Renderer
{
    public class BlazorWebViewRenderer : WebViewRenderer
    {
        internal static void Init()
        {
            //Nothing to do, just force the compiler to not strip our component
        }

        private bool _init = false;

        protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
        {
            base.OnElementChanged(e);
            if (_init == false && Control != null)
            {
                _init = true;
            }
        }
    }
}