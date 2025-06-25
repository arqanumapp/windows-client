using Arqanum;
using Arqanum.Utilities;
using ArqanumCore.Services;
using ArqanumCore.ViewModels.Contact;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

public class ContactsPageViewModel : INotifyPropertyChanged
{
    private readonly ContactService _contactService;

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler<int>? RequestContactsCountChanged;

    public DispatcherObservableCollection<ContactsItemViewModel> ConfirmedContacts { get; }
    public DispatcherObservableCollection<ContactsItemViewModel> PendingContacts { get; }
    public DispatcherObservableCollection<ContactsItemViewModel> RequestContacts { get; }
    public DispatcherObservableCollection<ContactsItemViewModel> BlockedContacts { get; }

    private int _requestContactsCount;
    public int RequestContactsCount
    {
        get => _requestContactsCount;
        private set
        {
            if (_requestContactsCount != value)
            {
                _requestContactsCount = value;
                OnPropertyChanged(nameof(RequestContactsCount));
                RequestContactsCountChanged?.Invoke(this, value);
            }
        }
    }

    public ContactsPageViewModel()
    {
        _contactService = App.Services.GetRequiredService<ContactService>();

        var dispatcher = DispatcherQueue.GetForCurrentThread();

        ConfirmedContacts = new DispatcherObservableCollection<ContactsItemViewModel>(dispatcher);
        PendingContacts = new DispatcherObservableCollection<ContactsItemViewModel>(dispatcher);
        RequestContacts = new DispatcherObservableCollection<ContactsItemViewModel>(dispatcher);
        BlockedContacts = new DispatcherObservableCollection<ContactsItemViewModel>(dispatcher);

        foreach (var item in _contactService.ConfirmedContacts)
            ConfirmedContacts.Add(item);
        foreach (var item in _contactService.PendingContacts)
            PendingContacts.Add(item);
        foreach (var item in _contactService.RequestContacts)
            RequestContacts.Add(item);
        foreach (var item in _contactService.BlockedContacts)
            BlockedContacts.Add(item);

        RequestContactsCount = _contactService.RequestContactsCount;

        _contactService.RequestContactsCountChanged += (s, count) =>
        {
            dispatcher.TryEnqueue(() => RequestContactsCount = count);
        };

        _contactService.ConfirmedContacts.CollectionChanged += (s, e) =>
        {
            dispatcher.TryEnqueue(() => SyncConfirmedContacts());
        };

        _contactService.PendingContacts.CollectionChanged += (s, e) =>
        {
            dispatcher.TryEnqueue(() => SyncPendingContacts());
        };

        _contactService.RequestContacts.CollectionChanged += (s, e) =>
        {
            dispatcher.TryEnqueue(() => SyncRequestContacts());
        };

        _contactService.BlockedContacts.CollectionChanged += (s, e) =>
        {
            dispatcher.TryEnqueue(() => SyncBlockedContacts());
        };
    }

    public async Task RefreshFilteredRequestsAsync()
    {
        await Task.Run(() =>
        {
            _contactService.ApplyRequestFilter(string.Empty);
            _contactService.ApplyBlockedFilter(string.Empty);
            _contactService.ApplyConfirmedFilter(string.Empty);
            _contactService.ApplyRequestFilter(string.Empty);
        });
    }

    #region Request Contacts

    public async Task AcceptContactRequestAsync(ContactsItemViewModel contact)
    {
        if (contact == null) return;
        await _contactService.ConfirmContactAsync(contact);
    }

    public async Task RejectContactRequestAsync(ContactsItemViewModel contact)
    {
        if (contact == null) return;
        await _contactService.RejectContactAsync(contact);
    }

    public async Task RejectAndBlockContactRequestAsync(ContactsItemViewModel contact)
    {
        if (contact == null) return;
        await _contactService.RejectAndBlockContactRequestAsync(contact);
    }

    #endregion

    #region Confirmed Contacts

    public async Task<bool> DeleteContactAsync(ContactsItemViewModel contact)
    {
        if (contact == null) return false;
        return await _contactService.DeleteContactAsync(contact);
    }

    public async Task DeleteAndBlockContactAsync(ContactsItemViewModel contact)
    {
        if (contact == null) return;
        await _contactService.DeleteContactAsync(contact);
        await _contactService.BlockContactAsync(contact);
    }

    #endregion 

    #region Blocked Contacts

    public async Task UnblockContactAsync(ContactsItemViewModel contact)
    {
        if (contact == null) return;
        await _contactService.UnblockContactAsync(contact);
    }

    #endregion
    #region Sync Methods

    private void SyncConfirmedContacts()
    {
        ConfirmedContacts.Clear();
        foreach (var item in _contactService.ConfirmedContacts)
            ConfirmedContacts.Add(item);
    }

    private void SyncRequestContacts()
    {
        RequestContacts.Clear();
        foreach (var item in _contactService.RequestContacts)
            RequestContacts.Add(item);
    }

    private void SyncBlockedContacts()
    {
        BlockedContacts.Clear();
        foreach (var item in _contactService.BlockedContacts)
            BlockedContacts.Add(item);
    }

    private void SyncPendingContacts()
    {
        PendingContacts.Clear();
        foreach (var item in _contactService.PendingContacts)
            PendingContacts.Add(item);
    }

    #endregion

    #region Filter Methods

    public void ApplyPendingFilter(string query)
    {
        _contactService.ApplyPendingFilter(query);
    }
    public void ApplyConfirmedFilter(string query)
    {
        _contactService.ApplyConfirmedFilter(query);
    }
    public void ApplyRequestFilter(string query)
    {
        _contactService.ApplyRequestFilter(query);
    }

    public void ApplyBlockedFilter(string query)
    {
        _contactService.ApplyBlockedFilter(query);
    }

    #endregion

    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
