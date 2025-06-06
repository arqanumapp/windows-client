using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
