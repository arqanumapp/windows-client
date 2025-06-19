using Arqanum.Services;
using ArqanumCore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Arqanum.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ThemeService _themeService;

        private AccountService _accountService;

        public MainViewModel()
        {
            _themeService = App.Services.GetRequiredService<ThemeService>();

            _themeService.ThemeChanged += theme => ThemeChanged?.Invoke(theme);

            _accountService = App.Services.GetRequiredService<AccountService>();

            ThemeChanged?.Invoke(_themeService.CurrentTheme);

            if (_accountService.CurrentAccount is INotifyPropertyChanged npc)
            {
                npc.PropertyChanged += OnAccountPropertyChanged;
            }
            AccountAvatarUrl = _accountService.CurrentAccount.AvatarUrl;
        }

        #region Avatar

        private string? _accountAvatarUrl;

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

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnAccountPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_accountService.CurrentAccount.AvatarUrl))
            {
                AccountAvatarUrl = _accountService.CurrentAccount.AvatarUrl;
            }
        }
        #region Navigation

        private object? _lastSelectedItem;

        private object? _selectedItem;

        public event Action? ShowUserProfileRequested;

        public event Action<Type>? NavigateRequested;

        public event Action<string, object?>? NavigateWithParamRequested;

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
        #endregion

        #region Themes

        public event Action<ElementTheme>? ThemeChanged;

        public ElementTheme GetCurrentTheme() => _themeService.CurrentTheme;

        private void ToggleTheme()
        {
            _themeService.ToggleTheme();
        }

        #endregion    
    }
}
