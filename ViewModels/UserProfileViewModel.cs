using ArqanumCore.Services;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Arqanum.ViewModels;

public class UserProfileViewModel : INotifyPropertyChanged
{
    private AccountService _accountService;

    private string _avatarUrl = string.Empty;
    private string _fullName = string.Empty;
    private string _username = string.Empty;
    private string _bio = string.Empty;
    private string _userId = string.Empty;

    public string AvatarUrl
    {
        get => _avatarUrl;
        set => SetField(ref _avatarUrl, value);
    }

    public string FullName
    {
        get => _fullName;
        set => SetField(ref _fullName, value);
    }

    public string Username
    {
        get => _username;
        set => SetField(ref _username, value);
    }

    public string Bio
    {
        get => _bio;
        set => SetField(ref _bio, value);
    }

    public string UserId
    {
        get => _userId;
        set => SetField(ref _userId, value);
    }


    public async Task LoadAsync()
    {
        _accountService = App.Services.GetRequiredService<AccountService>();
        var userData = await _accountService.GetAccountAsync();

        if (userData != null)
        {
            UserId = userData.AccountId;
            AvatarUrl = userData.AvatarUrl;
            FullName = userData.FirstName + " " + userData.LastName;
            Username = userData.Username;
            Bio = userData.Bio;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

