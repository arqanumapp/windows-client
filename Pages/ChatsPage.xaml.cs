using Arqanum.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Linq;

namespace Arqanum.Pages
{
    public sealed partial class ChatsPage : Page
    {
        public ChatsViewModel ViewModel { get; } = new ChatsViewModel();

        public ChatsPage()
        {
            this.InitializeComponent();
            ChatContentControl.Navigate(typeof(DefaultChatPage), null, new ContinuumNavigationTransitionInfo());
        }

        private void ChatListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var searchText = sender.Text?.Trim();

                if (string.IsNullOrEmpty(searchText))
                {
                    ChatListView.ItemsSource = ViewModel.Chats;
                }
                else
                {
                    var filtered = ViewModel.Chats
                        .Where(c => c.ChatName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    ChatListView.ItemsSource = filtered;
                }
            }
        }
    }
}
