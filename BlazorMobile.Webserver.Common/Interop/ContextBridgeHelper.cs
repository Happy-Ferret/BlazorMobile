using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

[assembly: InternalsVisibleTo("BlazorMobile.Android")]
[assembly: InternalsVisibleTo("BlazorMobile.iOS")]
[assembly: InternalsVisibleTo("BlazorMobile.UWP")]
namespace BlazorMobile.Webserver.Common.Interop
{
    internal static class ContextBridgeHelper
    {
        private static string JsFilesPath = "Interop.Javascript.";

        private static string GetFileContent(string filename)
        {
            var assembly = typeof(ContextBridgeHelper).Assembly;

            string JsNamespace = $"{assembly.GetName().Name}.{JsFilesPath}";

            using (var contentStream = assembly.GetManifestResourceStream($"{JsNamespace}{filename}"))
            {
                using (var streamReader = new StreamReader(contentStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        private const string MainResourceFile = "contextbridge.js";


        public static string GetInjectableJavascript(bool isAnonymousAutoEvalMethod = true)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(GetFileContent(MainResourceFile));
            sb.AppendLine();
            sb.AppendLine();
            var content = sb.ToString();


            if (!isAnonymousAutoEvalMethod)
                return content;

            content = $"(function() {{ {content} }})();";

            return content;
        }
    }
}
