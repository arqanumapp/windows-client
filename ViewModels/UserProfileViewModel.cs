using ArqanumCore.Services;
using ArqanumCore.ViewModels.Account;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Arqanum.ViewModels;

public class UserProfileViewModel : INotifyPropertyChanged
{
    private AccountService _accountService;

    private AccountViewModel? _currentAccount;

    public UserProfileViewModel()
    {
        _accountService = App.Services.GetRequiredService<AccountService>();
        CurrentAccount = _accountService.CurrentAccount;
    }

    public async Task<bool> OnCheckUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        try
        {
            return await _accountService.IsUsernameAvailableAsync(username);
        }
        catch
        {
            return false;
        }
    }

    #region Binding properties

    public AccountViewModel? CurrentAccount
    {
        get => _currentAccount;
        private set
        {
            if (_currentAccount != value)
            {
                if (_currentAccount != null)
                    _currentAccount.PropertyChanged -= OnCurrentAccountPropertyChanged;

                _currentAccount = value;

                if (_currentAccount != null)
                    _currentAccount.PropertyChanged += OnCurrentAccountPropertyChanged;

                OnPropertyChanged(nameof(CurrentAccount));
                OnPropertyChanged(nameof(FullName));
            }
        }
    }

    public string FullName => string.Join(" ", new[] { _currentAccount?.FirstName, _currentAccount?.LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));

    private void OnCurrentAccountPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "FirstName" || e.PropertyName == "LastName")
        {
            OnPropertyChanged(nameof(FullName));
        }
    }
    protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Update user profile

    public async Task<bool> UpdateUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3 || username.Length > 20)
        {
            return false;
        }
        var result = await _accountService.UpdateUsernameAsync(username);

        return result;
    }

    public async Task<bool> UpdateFullName(string firstName, string lastName) => await _accountService.UpdateFullNameAsync(firstName, lastName);

    public async Task<bool> UpdateBioAsync(string bio) => await _accountService.UpdateBioAsync(bio);

    public async Task<bool> UpdateAvatarAsync(byte[] imageData, string format) => await _accountService.UpdateAvatarAsync(imageData, format);

    #endregion
}

