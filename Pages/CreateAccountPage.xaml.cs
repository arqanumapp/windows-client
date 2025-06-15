using Arqanum.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Linq;

namespace Arqanum.Pages
{
    public sealed partial class CreateAccountPage : Page
    {
        private readonly CreateAccountViewModel _viewModel;

        public CreateAccountPage()
        {
            InitializeComponent();

            _viewModel = new CreateAccountViewModel();
            DataContext = _viewModel;

            UsernameTextBox.BeforeTextChanging += UsernameTextBox_BeforeTextChanging;

            _viewModel.ShowMessageDialog = async (msg) =>
            {
                var dialog = new ContentDialog
                {
                    Title = "Message",
                    Content = msg,
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };

                await dialog.ShowAsync();
            };

            _viewModel.GoBackAction = () =>
            {
                if (Frame.CanGoBack)
                    Frame.GoBack();
            };

            _viewModel.NavigateToMainPageAction = () =>
            {
                Frame.Navigate(typeof(MainPage));
            };
        }

        private void UsernameTextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !(
                (c >= 'a' && c <= 'z') ||
                (c >= 'A' && c <= 'Z') ||
                (c >= '0' && c <= '9') ||
                c == '_'));
        }

        private void Button_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            AnimatedIcon.SetState(this.SearchAnimatedIcon, "PointerOver");
        }

        private void Button_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            AnimatedIcon.SetState(this.SearchAnimatedIcon, "Normal");
        }
    }
}
