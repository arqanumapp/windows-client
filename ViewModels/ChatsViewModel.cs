using ArqanumCore.ViewModels.Chat;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Arqanum.ViewModels
{
    public class ChatsViewModel : INotifyPropertyChanged
    {
        private ChatPreviewViewModel _selectedChat;

        public ObservableCollection<ChatPreviewViewModel> Chats { get; } = new ObservableCollection<ChatPreviewViewModel>();

        public ChatPreviewViewModel SelectedChat
        {
            get => _selectedChat;
            set
            {
                if (_selectedChat != value)
                {
                    _selectedChat = value;
                    OnPropertyChanged();
                }
            }
        }

        public ChatsViewModel()
        {
            Chats.Add(new ChatPreviewViewModel { ChatId = "1", ChatName = "Чат 1", LastMessage = "Привет!", LastMessageTimestamp = DateTime.Now });
            Chats.Add(new ChatPreviewViewModel { ChatId = "2", ChatName = "Чат 2", LastMessage = "Как дела?", LastMessageTimestamp = DateTime.Now.AddMinutes(-5) });
            SelectedChat = Chats[0];
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
