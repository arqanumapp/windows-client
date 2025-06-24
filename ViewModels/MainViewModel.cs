using Arqanum.Services;
using ArqanumCore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Arqanum.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ThemeService _themeService;
        private AccountService _accountService;
        private ContactService _contactService;
        public event EventHandler<int>? RequestContactsCountChanged;

        private readonly DispatcherQueue _dispatcher;

        public MainViewModel()
        {
            _dispatcher = DispatcherQueue.GetForCurrentThread() ?? throw new InvalidOperationException("DispatcherQueue required");

            _themeService = App.Services.GetRequiredService<ThemeService>();
            _contactService = App.Services.GetRequiredService<ContactService>();
            _accountService = App.Services.GetRequiredService<AccountService>();

            _themeService.ThemeChanged += theme => ThemeChanged?.Invoke(theme);
            var dispatcher = DispatcherQueue.GetForCurrentThread();

            ThemeChanged?.Invoke(_themeService.CurrentTheme);

            if (_accountService.CurrentAccount is INotifyPropertyChanged npc)
                npc.PropertyChanged += OnAccountPropertyChanged;

            _contactService.RequestContactsCountChanged += (s, count) =>
            {
                dispatcher.TryEnqueue(() => RequestContactsCount = count);
            };

            RequestContactsCount = _contactService.RequestContactsCount;

            AccountAvatarUrl = _accountService.CurrentAccount.AvatarUrl;
        }


        #region Binding properties

        private string? _accountAvatarUrl;
        private int _requestContactsCount;

        public int RequestContactsCount
        {
            get => _requestContactsCount;
            set
            {
                if (_requestContactsCount != value)
                {
                    _requestContactsCount = value;
                    RequestContactsCountChanged?.Invoke(this, value);
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

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (_dispatcher.HasThreadAccess)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                _dispatcher.TryEnqueue(() =>
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName))
                );
            }
        }


        private void OnAccountPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_accountService.CurrentAccount.AvatarUrl))
            {
                AccountAvatarUrl = _accountService.CurrentAccount.AvatarUrl;
            }
        }

        //private void OnContactsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "RequestContactsCount")
        //    {
        //        var value = _contactService.ContactsViewModel.RequestContactsCount;
        //        _dispatcher.TryEnqueue(() =>
        //        {
        //            RequestContactsCount = value;
        //            ContactsBadgeVisible = value > 0;
        //        });
        //    }
        //}


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
            if (_selectedItem is Microsoft.UI.Xaml.Controls.NavigationViewItem selectedItem)
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

        public event Action<Microsoft.UI.Xaml.ElementTheme>? ThemeChanged;

        public Microsoft.UI.Xaml.ElementTheme GetCurrentTheme() => _themeService.CurrentTheme;

        private void ToggleTheme()
        {
            _themeService.ToggleTheme();
        }

        #endregion    
    }
}
