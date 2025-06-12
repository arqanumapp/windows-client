using Arqanum.Services;
using Arqanum.Utilities;
using Arqanum.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

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

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        var parentDialog = this.FindParent<ContentDialog>();
        parentDialog?.Hide();
    }
    private void Avatar_Tapped(object sender, TappedRoutedEventArgs e)
    {
        ImagePreviewService.Show(_viewModel.AvatarUrl);
    }
    private void AvatarGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        CameraButton.Visibility = Visibility.Visible;
    }

    private void AvatarGrid_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        CameraButton.Visibility = Visibility.Collapsed;
    }

    private void CameraButton_Click(object sender, RoutedEventArgs e)
    {
        CameraService.OpenPhotoCamera();
    }

    private async void FileButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();

        var hwnd = WindowNative.GetWindowHandle(App.Window);
        InitializeWithWindow.Initialize(picker, hwnd);

        picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        picker.ViewMode = PickerViewMode.Thumbnail;

        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");

        StorageFile file = await picker.PickSingleFileAsync();

        if (file != null)
        {
            var stream = await file.OpenReadAsync();

            using var randomStream = stream.AsStreamForRead();
        }
    }
}
