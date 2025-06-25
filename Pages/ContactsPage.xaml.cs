using Arqanum.Controls;
using Arqanum.Services;
using ArqanumCore.ViewModels.Contact;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
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

        private void Avatar_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is Image button && button.DataContext is ContactsItemViewModel contact)
            {
                ImagePreviewService.Show(contact.AvatarUrl);
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.RefreshFilteredRequestsAsync();
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

        private void OnCancelPendingContactClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ContactsItemViewModel contact)
            {
            }
        }

        #region Request ContextMenu Handlers

        private async void OnAcceptRequestClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item &&
                item.DataContext is ContactsItemViewModel contact)
            {
               await ViewModel.AcceptContactRequestAsync(contact);
            }
        }

        private async void OnRejectRequestClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item &&
                item.DataContext is ContactsItemViewModel contact)
            {
                var dialog = new ContentDialog
                {
                    Title = "Reject Contact?",
                    Content = $"Do you want to reject {contact.Username}?",
                    PrimaryButtonText = "Reject",
                    SecondaryButtonText = "Reject and Block",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    await ViewModel.RejectContactRequestAsync(contact);
                }
                else if (result == ContentDialogResult.Secondary)
                {
                    await ViewModel.RejectAndBlockContactRequestAsync(contact);
                }
            }
        }

        #endregion

        #region Confirmed ContextMenu Handlers

        private async void OnDeleteConfirmedContactClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item &&
                item.DataContext is ContactsItemViewModel contact)
            {
                var dialog = new ContentDialog
                {
                    Title = "Delete Contact?",
                    Content = $"Do you want to remove {contact.Username} from your contacts?",
                    PrimaryButtonText = "Remove",
                    SecondaryButtonText = "Remove and Block",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style
                };


                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    await ViewModel.DeleteContactAsync(contact);
                }
                else if (result == ContentDialogResult.Secondary)
                {
                    await ViewModel.DeleteAndBlockContactAsync(contact);
                }
            }
        }

        private void OnOpenChatClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ContactsItemViewModel contact)
            {
            }
        }

        #endregion

        private void OnUnblockContactClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ContactsItemViewModel contact)
            {
            }
        }

        #region SearchBox Handlers

        private void OutgoingSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ViewModel.ApplyPendingFilter(sender.Text);
            }
        }
        private void ContactsSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ViewModel.ApplyConfirmedFilter(sender.Text);
            }
        }
        private void IncomingSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ViewModel.ApplyRequestFilter(sender.Text);
            }
        }
        private void BlacklistSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ViewModel.ApplyBlockedFilter(sender.Text);
            }
        }

        #endregion

    }
}
