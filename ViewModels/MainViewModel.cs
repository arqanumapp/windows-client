using ArqanumCore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;

namespace Arqanum.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {

        private AccountService _accountService;

        public event PropertyChangedEventHandler? PropertyChanged;

        private object? _lastSelectedItem;
        private object? _selectedItem;
        private string? _accountAvatarUrl;

        public object? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged();

                    OnSelectedItemChanged();
                }
            }
        }
        public string? AccountAvatarUrl
        {
            get => _accountAvatarUrl;
            private set
            {
                if (_accountAvatarUrl != value)
                {
                    _accountAvatarUrl = value;
                    OnPropertyChanged();
                }
            }
        }
        public async Task LoadAccountAvatarUrlAsync()
        {
            try
            {
                _accountService = App.Services.GetRequiredService<AccountService>();

                var url = await _accountService.GetAccountAvatarUrl();
                AccountAvatarUrl = url;
            }
            catch
            {
                AccountAvatarUrl = null;
            }
        }
        private const string ThemeSettingKey = "AppTheme";

        public event Action? ShowUserProfileRequested;
        public event Action<Type>? NavigateRequested;
        public event Action<string, object?>? NavigateWithParamRequested;
        public event Action<ElementTheme>? ThemeChanged;

        private void OnSelectedItemChanged()
        {
            if (_selectedItem is NavigationViewItem selectedItem)
            {
                var tag = selectedItem.Tag?.ToString();

                if (tag == "ThemeToggle")
                {
                    SelectedItem = _lastSelectedItem;
                    ToggleTheme();
                }
                else if (tag == "Account")
                {
                    SelectedItem = _lastSelectedItem;
                    ShowUserProfileRequested?.Invoke();
                }
                else
                {
                    _lastSelectedItem = _selectedItem;
                    switch (tag)
                    {
                        case "Chat":
                            NavigateRequested?.Invoke(typeof(Pages.ChatsPage));
                            break;
                        case "Contacts":
                            NavigateRequested?.Invoke(typeof(Pages.ContactsPage));
                            break;
                        case "Settings":
                            NavigateRequested?.Invoke(typeof(Pages.SettingsPage));
                            break;
                    }
                }
            }
        }

        public MainViewModel()
        {
            LoadTheme();
        }

        private void ToggleTheme()
        {
            var currentTheme = GetCurrentTheme();
            var newTheme = currentTheme switch
            {
                ElementTheme.Dark => ElementTheme.Light,
                ElementTheme.Light => ElementTheme.Dark,
                _ => ElementTheme.Dark
            };

            SaveTheme(newTheme);
            ThemeChanged?.Invoke(newTheme);
        }

        public ElementTheme GetCurrentTheme()
        {
            var saved = ApplicationData.Current.LocalSettings.Values[ThemeSettingKey] as string;

            if (Enum.TryParse<ElementTheme>(saved, out var theme))
            {
                return theme;
            }

            return ElementTheme.Dark;
        }

        private void SaveTheme(ElementTheme theme)
        {
            ApplicationData.Current.LocalSettings.Values[ThemeSettingKey] = theme.ToString();
        }

        private void LoadTheme()
        {
            var theme = GetCurrentTheme();
            ThemeChanged?.Invoke(theme);
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
