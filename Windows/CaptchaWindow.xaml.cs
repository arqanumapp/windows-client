using Microsoft.UI.Xaml;
using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;

namespace Arqanum.Windows
{
    public sealed partial class CaptchaWindow : Window
    {
        private readonly TaskCompletionSource<string> _tcs = new();

        public CaptchaWindow()
        {
            this.InitializeComponent();
        }

        private void WebView_WebMessageReceived(Microsoft.UI.Xaml.Controls.WebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            var token = args.TryGetWebMessageAsString();
            if (!string.IsNullOrWhiteSpace(token))
            {
                _tcs.TrySetResult(token);
                this.Close();
            }
        }


        public Task<string> GetCaptchaTokenAsync() => _tcs.Task;
    }
}
