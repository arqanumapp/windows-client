using Arqanum.Controls;
using Arqanum.Services;
using Arqanum.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;

namespace Arqanum.Pages
{
    public sealed partial class MainPage : Page
    {
        private MainViewModel ViewModel { get; } = new MainViewModel();
        private readonly DispatcherQueue _dispatcher = DispatcherQueue.GetForCurrentThread();

        public MainPage()
        {
            InitializeComponent();

            DataContext = ViewModel;

            ViewModel.ShowUserProfileRequested += ShowUserProfileDialog;

            ViewModel.NavigateRequested += pageType =>
            {
                ContentFrame.Navigate(pageType, null, new SuppressNavigationTransitionInfo());
            };

            ViewModel.ThemeChanged += ApplyTheme;

            ViewModel.RequestContactsCountChanged += (s, count) =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    UpdateContactsBadge(count);
                });
            };

            UpdateContactsBadge(ViewModel.RequestContactsCount);

            Loaded += (s, e) =>
            {
                ApplyTheme(ViewModel.GetCurrentTheme());

                foreach (var item in SideNavigation.MenuItems)
                {
                    if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == "Chat")
                    {
                        ViewModel.SelectedItem = navItem;
                        break;
                    }
                }
            };
        }
        private InfoBadge? GetContactsBadge()
        {
            foreach (var item in SideNavigation.MenuItems)
            {
                if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == "Contacts")
                {
                    return navItem.InfoBadge as InfoBadge;
                }
            }
            return null;
        }

        private void UpdateContactsBadge(int count)
        {
            var badge = GetContactsBadge();
            if (badge != null)
            {
                _dispatcher.TryEnqueue(() =>
                {
                    badge.Value = count;
                    badge.Visibility = ViewModel.RequestContactsCount > 0
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                });
            }
        }


        private void ApplyTheme(ElementTheme theme)
        {
            if (App.Window.Content is FrameworkElement root)
            {
                root.RequestedTheme = theme;
            }
            UpdateThemeIcon(theme);
        }

        private void UpdateThemeIcon(ElementTheme theme)
        {
            if (ThemeIcon != null)
            {
                if (theme == ElementTheme.Dark)
                {
                    ThemeIcon.Glyph = "\uE706";
                    ThemeIcon.Foreground = new SolidColorBrush(Colors.Gold);
                }
                else
                {
                    ThemeIcon.Glyph = "\uE708";
                    ThemeIcon.Foreground = new SolidColorBrush(Colors.DarkBlue);
                }
            }

            foreach (var item in SideNavigation.FooterMenuItems)
            {
                if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == "ThemeToggle")
                {
                    navItem.Content = theme == ElementTheme.Dark ? "Light" : "Dark";
                    break;
                }
            }
        }

        private async void ShowUserProfileDialog()
        {
            var theme = App.Services.GetRequiredService<ThemeService>().CurrentTheme;

            ContentDialog dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Content = new UserProfileDialog(),
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                RequestedTheme = theme
            };

            var result = await dialog.ShowAsync();
        }

        private void SideNavigation_SelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
        {
            if (SideNavigation.SelectedItem is NavigationViewItem navItem)
            {
                ViewModel.SelectedItem = navItem;
            }
        }
    }
}
