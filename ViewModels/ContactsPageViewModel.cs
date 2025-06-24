using Arqanum;
using Arqanum.Utilities;
using ArqanumCore.Services;
using ArqanumCore.ViewModels.Contact;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using System;
using System.ComponentModel;

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

        // Копируем стартовые значения
        foreach (var item in _contactService.ConfirmedContacts)
            ConfirmedContacts.Add(item);
        foreach (var item in _contactService.PendingContacts)
            PendingContacts.Add(item);
        foreach (var item in _contactService.RequestContacts)
            RequestContacts.Add(item);
        foreach (var item in _contactService.BlockedContacts)
            BlockedContacts.Add(item);

        RequestContactsCount = _contactService.RequestContactsCount;

        // Подписка на обновление количества заявок
        _contactService.RequestContactsCountChanged += (s, count) =>
        {
            dispatcher.TryEnqueue(() => RequestContactsCount = count);
        };

        // Подписки на CollectionChanged
        _contactService.ConfirmedContacts.CollectionChanged += (s, e) =>
        {
            dispatcher.TryEnqueue(() =>
            {
                if (e.NewItems != null)
                    foreach (ContactsItemViewModel item in e.NewItems)
                        ConfirmedContacts.Add(item);
                if (e.OldItems != null)
                    foreach (ContactsItemViewModel item in e.OldItems)
                        ConfirmedContacts.Remove(item);
            });
        };

        _contactService.PendingContacts.CollectionChanged += (s, e) =>
        {
            dispatcher.TryEnqueue(() =>
            {
                if (e.NewItems != null)
                    foreach (ContactsItemViewModel item in e.NewItems)
                        PendingContacts.Add(item);
                if (e.OldItems != null)
                    foreach (ContactsItemViewModel item in e.OldItems)
                        PendingContacts.Remove(item);
            });
        };

        _contactService.RequestContacts.CollectionChanged += (s, e) =>
        {
            dispatcher.TryEnqueue(() =>
            {
                if (e.NewItems != null)
                    foreach (ContactsItemViewModel item in e.NewItems)
                        RequestContacts.Add(item);
                if (e.OldItems != null)
                    foreach (ContactsItemViewModel item in e.OldItems)
                        RequestContacts.Remove(item);
            });
        };

        _contactService.BlockedContacts.CollectionChanged += (s, e) =>
        {
            dispatcher.TryEnqueue(() =>
            {
                if (e.NewItems != null)
                    foreach (ContactsItemViewModel item in e.NewItems)
                        BlockedContacts.Add(item);
                if (e.OldItems != null)
                    foreach (ContactsItemViewModel item in e.OldItems)
                        BlockedContacts.Remove(item);
            });
        };
    }

    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
