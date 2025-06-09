using Arqanum.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

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
    }
}
