using Arqanum.Controls;
using Arqanum.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace Arqanum.Pages
{
    public sealed partial class MainPage : Page
    {
        private MainViewModel ViewModel { get; } = new MainViewModel();

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

            Loaded += async (s, e) =>
            {
                await ViewModel.LoadAccountAvatarUrlAsync();
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

        private void ShowUserProfileDialog()
        {
            var dialog = new UserProfileDialog();
            dialog.HorizontalAlignment = HorizontalAlignment.Stretch;
            dialog.VerticalAlignment = VerticalAlignment.Stretch;
            dialog.Closed += (s, args) => RootGrid.Children.Remove(dialog);
            RootGrid.Children.Add(dialog);
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
