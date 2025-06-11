using ArqanumCore.Dtos.Contact;
using ArqanumCore.Services;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Arqanum.ViewModels;

public class AddContactViewModel : INotifyPropertyChanged
{
    private ContactService _contactService;

    public AddContactViewModel()
    {
        _contactService = App.Services.GetRequiredService<ContactService>();
    }

    public string Query { get; set; }

    public GetContactResponceDto Result { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task<bool> SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(Query))
            return false;

        try
        {
            var contact = await _contactService.FindContactAsync(Query);
            if (contact is null)
                return false;
            else
            {
                Result = contact;
                OnPropertyChanged(nameof(Result));
                return true;
            }
        }
        catch
        {
            return false;
        }
    }
}
