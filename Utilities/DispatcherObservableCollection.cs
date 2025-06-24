using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Arqanum.Utilities
{
    public class DispatcherObservableCollection<T> : ObservableCollection<T>
    {
        private readonly DispatcherQueue _dispatcher;

        public DispatcherObservableCollection(DispatcherQueue dispatcher)
        {
            _dispatcher = dispatcher;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_dispatcher.HasThreadAccess)
                base.OnCollectionChanged(e);
            else
                _dispatcher.TryEnqueue(() => base.OnCollectionChanged(e));
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (_dispatcher.HasThreadAccess)
                base.OnPropertyChanged(e);
            else
                _dispatcher.TryEnqueue(() => base.OnPropertyChanged(e));
        }
    }
}
