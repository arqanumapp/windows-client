using Arqanum.Controls;
using Arqanum.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Arqanum.Pages
{
    public sealed partial class ContactsPage : Page
    {
        public ContactsPage()
        {
            InitializeComponent();
        }

        private async void AddNewContactButton_Click(object sender, RoutedEventArgs e)
        {
            var theme = App.Services.GetRequiredService<ThemeService>().CurrentTheme;

            ContentDialog dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Content = new AddContactDialog(),
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                RequestedTheme = theme // <- ÂÀÆÍÎ: ñþäà
            };

            var result = await dialog.ShowAsync();
        }

    }
}
