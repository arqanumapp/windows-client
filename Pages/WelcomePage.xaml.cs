using Arqanum.CoreImplementations;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.System;

namespace Arqanum.Pages
{
    public sealed partial class WelcomePage : Page
    {
        public WelcomePage()
        {
            InitializeComponent();
        }
        private void CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            Frame? rootFrame = this.Frame;
            rootFrame?.Navigate(typeof(CreateAccountPage));
        }
        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            var captchaProvider = new CaptchaProvider();
            string ss = await captchaProvider.GetCaptchaTokenAsync();
        }

        private async void OnGitHubClick(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("https://github.com/arqanumapp");
            await Launcher.LaunchUriAsync(uri);
        }
    }
}
