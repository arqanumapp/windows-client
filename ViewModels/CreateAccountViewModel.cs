using Arqanum.Utilities;
using ArqanumCore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Arqanum.ViewModels
{
    public class CreateAccountViewModel : INotifyPropertyChanged
    {

        private CancellationTokenSource? _cancellationTokenSource;

        private string _username = "";
        private string _firstName = "";
        private string _lastName = "";
        private string? _statusMessage;

        private readonly AccountService _accountService;


        public event PropertyChangedEventHandler PropertyChanged;

        public string Username
        {
            get => _username;
            set
            {
                var filtered = new string(value?.Where(c =>
                    (c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                    (c >= '0' && c <= '9') ||
                    c == '_').ToArray() ?? []);

                if (filtered.Length > 32)
                    filtered = filtered[..32];

                if (_username != filtered)
                {
                    _username = filtered;
                    IsUsernameAvailable = null;
                    OnPropertyChanged();
                }
            }
        }

        public string FirstName
        {
            get => _firstName;
            set
            {
                var filtered = FilterName(value);
                if (filtered != _firstName)
                {
                    _firstName = filtered;
                    OnPropertyChanged();
                }
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                var filtered = FilterName(value);
                if (filtered != _lastName)
                {
                    _lastName = filtered;
                    OnPropertyChanged();
                }
            }
        }

        public string? StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged(nameof(StatusMessage));
                }
            }
        }
        private bool _isCheckingUsername;
        private bool? _isUsernameAvailable;

        public bool IsCheckingUsername
        {
            get => _isCheckingUsername;
            set
            {
                if (_isCheckingUsername != value)
                {
                    _isCheckingUsername = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsCheckButtonVisible));
                    OnPropertyChanged(nameof(IsSpinnerVisible));
                }
            }
        }

        public bool? IsUsernameAvailable
        {
            get => _isUsernameAvailable;
            set
            {
                if (_isUsernameAvailable != value)
                {
                    _isUsernameAvailable = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsCheckButtonVisible));
                    OnPropertyChanged(nameof(IsSpinnerVisible));
                    OnPropertyChanged(nameof(IsSuccessIconVisible));
                    OnPropertyChanged(nameof(CanContinue));
                    OnPropertyChanged(nameof(IsFailureIconVisible));
                }
            }
        }

        public bool IsFailureIconVisible => IsUsernameAvailable == false;
        public bool IsCheckButtonVisible => !IsCheckingUsername && IsUsernameAvailable == null;
        public bool IsSpinnerVisible => IsCheckingUsername;
        public bool IsSuccessIconVisible => IsUsernameAvailable == true;
        public bool CanContinue => IsUsernameAvailable == true;

        public ICommand CheckUsernameCommand { get; }
        public ICommand ContinueCommand { get; }
        public ICommand BackCommand { get; }

        public Func<string, Task>? ShowMessageDialog;
        public Action? GoBackAction;

        public CreateAccountViewModel()
        {
            _accountService = App.Services.GetRequiredService<AccountService>();

            CheckUsernameCommand = new AsyncRelayCommand(OnCheckUsername);
            ContinueCommand = new AsyncRelayCommand(OnContinueAsync);
            BackCommand = new RelayCommand(OnBack);
        }

        private void OnBack()
        {
            _cancellationTokenSource?.Cancel();
            GoBackAction?.Invoke();
        }


        private async Task OnContinueAsync()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                await ShowMessageDialog?.Invoke("Username is required.");
                return;
            }

            if (_cancellationTokenSource != null)
                return; 

            _cancellationTokenSource = new CancellationTokenSource();
            StatusMessage = "Creating account...";

            try
            {
                bool success = await _accountService.CreateAccount(
                    Username,
                    FirstName,
                    LastName,
                    new Progress<string>(msg =>
                    {
                        DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
                        {
                            StatusMessage = msg;
                        });
                    }),
                    _cancellationTokenSource.Token);

                if (success)
                {
                    await ShowMessageDialog?.Invoke($"Account '{Username}' created successfully.");
                }
                else
                {
                    await ShowMessageDialog?.Invoke("Account creation failed.");
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = null;
            }
            catch (Exception ex)
            {
                await ShowMessageDialog?.Invoke($"Error: {ex.Message}");
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }



        private async Task OnCheckUsername()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                if (ShowMessageDialog is not null)
                    await ShowMessageDialog.Invoke("Please enter a username to check.");
                return;
            }
            IsCheckingUsername = true;
            IsUsernameAvailable = null;

            try
            {
                await Task.Delay(2000); 

                bool isAvailable = await _accountService.IsUsernameAvailableAsync(Username);
                IsUsernameAvailable = isAvailable;
            }
            catch (Exception ex)
            {
                IsUsernameAvailable = null;
                if (ShowMessageDialog is not null)
                    await ShowMessageDialog.Invoke($"Error while checking username: {ex.Message}");
            }
            finally
            {
                IsCheckingUsername = false;
            }
        }



        private string FilterName(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            var filtered = new string([.. input.Where(c => char.IsLetter(c) || char.IsWhiteSpace(c))]);

            if (filtered.Length > 32)
                filtered = filtered[..32];

            return filtered;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
