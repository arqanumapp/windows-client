using Arqanum.Services;
using Arqanum.Utilities;
using Arqanum.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace Arqanum.Controls;

public sealed partial class AddContactDialog : UserControl
{
    public event EventHandler Closed;

    public AddContactViewModel _viewModel;

    public AddContactDialog()
    {
        this.InitializeComponent();
        _viewModel = new AddContactViewModel();
        DataContext = _viewModel;

    }
    private void Close_Click(object sender, RoutedEventArgs e)
    {
        var parentDialog = this.FindParent<ContentDialog>();
        parentDialog?.Hide();
    }


    private async void Search_Click(object sender, RoutedEventArgs e)
    {
        NotFoundText.Visibility = Visibility.Collapsed;
        ResultPanel.Visibility = Visibility.Collapsed;

        SearchButton.Visibility = Visibility.Collapsed;
        SearchSpinner.Visibility = Visibility.Visible;
        SearchSpinner.IsActive = true;

        var found = await _viewModel.SearchAsync();

        SearchSpinner.IsActive = false;
        SearchSpinner.Visibility = Visibility.Collapsed;
        SearchButton.Visibility = Visibility.Visible;

        if (found)
        {
            ResultPanel.Visibility = Visibility.Visible;
        }
        else
        {
            NotFoundText.Visibility = Visibility.Visible;
        }
    }

    private void Avatar_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (_viewModel?.Result?.AvatarUrl != null)
        {
            ImagePreviewService.Show(_viewModel.Result.AvatarUrl);
        }
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.AddContactAsync();
    }
}
