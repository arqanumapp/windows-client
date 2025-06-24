using Microsoft.UI.Dispatching;
using System;
using System.ComponentModel;

namespace Arqanum.Utilities
{
    public class UiSafeViewModel<T> : INotifyPropertyChanged where T : INotifyPropertyChanged
    {
        public T Inner { get; }

        private readonly DispatcherQueue _dispatcher;

        public UiSafeViewModel(T inner, DispatcherQueue dispatcher)
        {
            Inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

            Inner.PropertyChanged += Inner_PropertyChanged;
        }

        private void Inner_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_dispatcher.HasThreadAccess)
            {
                PropertyChanged?.Invoke(this, e); // Здесь this, а не sender
            }
            else
            {
                _dispatcher.TryEnqueue(() => PropertyChanged?.Invoke(this, e)); // Здесь this, а не sender
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        public void Dispose()
        {
            Inner.PropertyChanged -= Inner_PropertyChanged;
        }
    }
}
