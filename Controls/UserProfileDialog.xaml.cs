using Arqanum.Services;
using Arqanum.Utilities;
using Arqanum.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Arqanum.Controls;

public sealed partial class UserProfileDialog : UserControl
{
    public event EventHandler Closed;

    private UserProfileViewModel _viewModel;

    public UserProfileDialog()
    {
        InitializeComponent();

        _viewModel = new UserProfileViewModel();

        DataContext = _viewModel;

        UsernameTextBox.BeforeTextChanging += UsernameTextBox_BeforeTextChanging;
    }

    #region Update user profile

    private async void SaveFullNameButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not UserProfileViewModel vm)
            return;

        ErrorTextBlock.Visibility = Visibility.Collapsed;
        SaveFullNameButton.Visibility = Visibility.Collapsed;
        SaveProgressRing.IsActive = true;
        SaveProgressRing.Visibility = Visibility.Visible;

        var result = await vm.UpdateFullName(FirstNameTextBox.Text, LastNameTextBox.Text);

        if (result)
        {
            UpdateFullNameFlyout.Hide();
        }
        else
        {
            ErrorTextBlock.Visibility = Visibility.Visible;
            SaveFullNameButton.Visibility = Visibility.Visible;
            SaveProgressRing.IsActive = false;
            SaveProgressRing.Visibility = Visibility.Collapsed;
        }

        SaveProgressRing.IsActive = false;
        SaveProgressRing.Visibility = Visibility.Collapsed;
        SaveFullNameButton.Visibility = Visibility.Visible;
    }

    private async void SaveUsernameButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not UserProfileViewModel vm)
            return;

        var result = await vm.UpdateUsernameAsync(UsernameTextBox.Text);
        if (result)
        {
            UpdateUserNameFlyout.Hide();
        }
        else
        {
            UserNameIsNotAvaible.Visibility = Visibility.Visible;
        }

    }

    private async void SaveBioButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not UserProfileViewModel vm)
            return;

        BioErrorTextBlock.Visibility = Visibility.Collapsed;
        SaveBioButton.Visibility = Visibility.Collapsed;
        SaveBioProgressRing.IsActive = true;
        SaveBioProgressRing.Visibility = Visibility.Visible;

        var result = await vm.UpdateBioAsync(BioTextBox.Text);

        if (result)
        {
            UpdateBioFlyout.Hide();
        }
        else
        {
            BioErrorTextBlock.Visibility = Visibility.Visible;
            SaveBioButton.Visibility = Visibility.Visible;
            SaveBioProgressRing.IsActive = false;
            SaveBioProgressRing.Visibility = Visibility.Collapsed;
        }
        SaveBioProgressRing.IsActive = false;
        SaveBioProgressRing.Visibility = Visibility.Collapsed;
        SaveBioButton.Visibility = Visibility.Visible;
    }
    #endregion

    #region Copy buttons

    private void CopyUsername_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel?.CurrentAccount.Username is string username && !string.IsNullOrEmpty(username))
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(username);
            Clipboard.SetContent(dataPackage);
        }
    }

    private void CopyUserId_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel?.CurrentAccount.AccountId is string userId && !string.IsNullOrEmpty(userId))
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(userId);
            Clipboard.SetContent(dataPackage);
        }
    }

    #endregion

    #region Avatar

    private void Avatar_Tapped(object sender, TappedRoutedEventArgs e)
    {
        ImagePreviewService.Show(_viewModel.CurrentAccount.AvatarUrl);
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

        var file = await picker.PickSingleFileAsync();
        if (file == null)
            return;

        var format = Path.GetExtension(file.Name)?.TrimStart('.').ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(format))
            return;

        const long MaxFileSize = 2 * 1024 * 1024;
        var properties = await file.GetBasicPropertiesAsync();
        if (properties.Size > MaxFileSize)
        {
            var dialog = new ContentDialog
            {
                Title = "File Too Large",
                Content = "The selected image must not exceed 2 MB.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style
            };
            await dialog.ShowAsync();
            return;
        }

        using var stream = await file.OpenStreamForReadAsync();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();

        await _viewModel.UpdateAvatarAsync(bytes, format);
    }

    #endregion

    private async void CheckUsernameButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not UserProfileViewModel vm)
            return;

        CheckUsernameProgressRing.IsActive = true;
        CheckUsernameProgressRing.Visibility = Visibility.Visible;
        CheckUsernameButton.Visibility = Visibility.Collapsed;
        var username = UsernameTextBox.Text;

        if (string.IsNullOrWhiteSpace(username) || username.Length < 3 || username.Length > 20)
        {
            ErrorTextBlock.Visibility = Visibility.Visible;
            CheckUsernameProgressRing.IsActive = false;
            CheckUsernameProgressRing.Visibility = Visibility.Collapsed;
            CheckUsernameButton.Visibility = Visibility.Visible;
            return;
        }

        var isAvailable = await vm.OnCheckUsername(username);

        if (isAvailable)
        {
            UserNameIsNotAvaible.Visibility = Visibility.Collapsed;
            CheckUsernameButton.Visibility = Visibility.Collapsed;
            UserNameIsAvaible.Visibility = Visibility.Visible;
            SaveUsernameButton.Visibility = Visibility.Visible;
            ErrorTextBlock.Visibility = Visibility.Collapsed;
        }
        else
        {
            CheckUsernameButton.Visibility = Visibility.Collapsed;
            UserNameIsNotAvaible.Visibility = Visibility.Visible;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }
        CheckUsernameProgressRing.IsActive = false;
        CheckUsernameProgressRing.Visibility = Visibility.Collapsed;
    }

    private void UsernameTextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        args.Cancel = args.NewText.Any(c => !(
            (c >= 'a' && c <= 'z') ||
            (c >= 'A' && c <= 'Z') ||
            (c >= '0' && c <= '9') ||
            c == '_'));
        SaveUsernameButton.Visibility = Visibility.Collapsed;
        UserNameIsAvaible.Visibility = Visibility.Collapsed;
        UserNameIsNotAvaible.Visibility = Visibility.Collapsed;
        CheckUsernameButton.Visibility = Visibility.Visible;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        var parentDialog = this.FindParent<ContentDialog>();
        parentDialog?.Hide();
    }
}
