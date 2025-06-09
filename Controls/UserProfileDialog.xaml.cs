using Arqanum.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using Windows.ApplicationModel.DataTransfer;

namespace Arqanum.Controls;

public sealed partial class UserProfileDialog : UserControl
{
    public event EventHandler Closed;

    private UserProfileViewModel _viewModel;

    public UserProfileDialog()
    {
        this.InitializeComponent();

        _viewModel = new UserProfileViewModel();
        DataContext = _viewModel;

        Loaded += UserProfileDialog_Loaded;
    }
    private void Update_Click(object sender, RoutedEventArgs e)
    {
        // Логика обновления профиля,валидация и отправка данных
    }

    private async void UserProfileDialog_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadAsync();

        var fadeIn = new DoubleAnimation { To = 0.5, Duration = TimeSpan.FromMilliseconds(200) };
        var scaleX = new DoubleAnimation { To = 1.0, Duration = TimeSpan.FromMilliseconds(200) };
        var scaleY = new DoubleAnimation { To = 1.0, Duration = TimeSpan.FromMilliseconds(200) };

        var sb = new Storyboard();
        sb.Children.Add(fadeIn);
        sb.Children.Add(scaleX);
        sb.Children.Add(scaleY);

        Storyboard.SetTarget(fadeIn, Overlay);
        Storyboard.SetTargetProperty(fadeIn, "Opacity");

        Storyboard.SetTarget(scaleX, DialogScale);
        Storyboard.SetTargetProperty(scaleX, "ScaleX");

        Storyboard.SetTarget(scaleY, DialogScale);
        Storyboard.SetTargetProperty(scaleY, "ScaleY");

        sb.Begin();
    }

    private void CopyUsername_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel?.Username is string username && !string.IsNullOrEmpty(username))
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(username);
            Clipboard.SetContent(dataPackage);
        }
    }

    private void CopyUserId_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel?.UserId is string userId && !string.IsNullOrEmpty(userId))
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(userId);
            Clipboard.SetContent(dataPackage);
        }
    }

    private void Overlay_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        CloseDialog();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        CloseDialog();
    }

    private void CloseDialog()
    {
        var fadeOut = new DoubleAnimation { To = 0.0, Duration = TimeSpan.FromMilliseconds(200) };
        var scaleX = new DoubleAnimation { To = 0.8, Duration = TimeSpan.FromMilliseconds(200) };
        var scaleY = new DoubleAnimation { To = 0.8, Duration = TimeSpan.FromMilliseconds(200) };

        var sb = new Storyboard();
        sb.Children.Add(fadeOut);
        sb.Children.Add(scaleX);
        sb.Children.Add(scaleY);

        Storyboard.SetTarget(fadeOut, Overlay);
        Storyboard.SetTargetProperty(fadeOut, "Opacity");

        Storyboard.SetTarget(scaleX, DialogScale);
        Storyboard.SetTargetProperty(scaleX, "ScaleX");

        Storyboard.SetTarget(scaleY, DialogScale);
        Storyboard.SetTargetProperty(scaleY, "ScaleY");

        sb.Completed += (s, e) => Closed?.Invoke(this, EventArgs.Empty);
        sb.Begin();
    }
}
