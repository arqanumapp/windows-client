using Arqanum.CoreImplementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

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
    }
}
