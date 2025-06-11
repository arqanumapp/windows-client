using Microsoft.UI.Xaml;
using System;
using Windows.Storage;

namespace Arqanum.Services
{
    public class ThemeService 
    {
        private const string ThemeSettingKey = "AppTheme";

        public ElementTheme CurrentTheme { get; private set; }

        public event Action<ElementTheme>? ThemeChanged;


        public void ToggleTheme()
        {
            var newTheme = CurrentTheme switch
            {
                ElementTheme.Dark => ElementTheme.Light,
                ElementTheme.Light => ElementTheme.Dark,
                _ => ElementTheme.Dark
            };

            ApplyTheme(newTheme);
        }

        public void ApplyTheme(ElementTheme theme)
        {
            CurrentTheme = theme;
            ApplicationData.Current.LocalSettings.Values[ThemeSettingKey] = theme.ToString();
            ThemeChanged?.Invoke(theme);
        }


        public void RestoreTheme()
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(ThemeSettingKey, out object savedTheme) &&
                Enum.TryParse(savedTheme.ToString(), out ElementTheme theme))
            {
                CurrentTheme = theme;
            }
            else
            {
                CurrentTheme = ElementTheme.Dark;
            }

            if (App.Window?.Content is FrameworkElement root)
            {
                root.RequestedTheme = CurrentTheme;
            }

            ThemeChanged?.Invoke(CurrentTheme);
        }
    }
}
