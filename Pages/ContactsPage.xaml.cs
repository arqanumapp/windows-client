using Arqanum.Controls;
using Arqanum.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Arqanum.Pages
{
    public sealed partial class ContactsPage : Page
    {
        private ContactsPageViewModel ViewModel { get; }

        public ContactsPage()
        {
            InitializeComponent();

            ViewModel = new ContactsPageViewModel();

            DataContext = ViewModel;

            ViewModel.RequestContactsCountChanged += (s, count) =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    ContactsBadge.Value = count;
                    ContactsBadge.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
                });
            };


            var initial = ViewModel.RequestContactsCount;
            ContactsBadge.Value = initial;
            ContactsBadge.Visibility = initial > 0 ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        }

        private async void AddNewContactButton_Click(object sender, RoutedEventArgs e)
        {
            var theme = App.Services.GetRequiredService<ThemeService>().CurrentTheme;

            ContentDialog dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Content = new AddContactDialog(),
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                RequestedTheme = theme
            };

            await dialog.ShowAsync();
        }

        private void OnDeleteContactClick(object sender, RoutedEventArgs e)
        {
        }
    }
}
