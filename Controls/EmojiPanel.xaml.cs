using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace YourApp.Controls;

public sealed partial class EmojiPicker : UserControl
{
    public ObservableCollection<string> Emojis { get; } = new();
    private Action<string>? _onEmojiSelected;

    private static List<string>? _cachedEmojis;
    private int _loadedCount = 0;
    private const int ChunkSize = 200;

    public EmojiPicker()
    {
        this.InitializeComponent();
        _ = LoadEmojisAsync();
    }

    public void SetCallback(Action<string> callback)
    {
        _onEmojiSelected = callback;
    }

    private async Task LoadEmojisAsync()
    {
        if (_cachedEmojis == null)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/emojis.json"));
            var json = await FileIO.ReadTextAsync(file);
            var data = JsonSerializer.Deserialize<EmojiData>(json);
            _cachedEmojis = data?.Emojis ?? new List<string>();
        }

        LoadMoreEmojis();
    }

    private void LoadMoreEmojis()
    {
        if (_cachedEmojis == null) return;

        var toLoad = _cachedEmojis
            .Skip(_loadedCount)
            .Take(ChunkSize)
            .ToList();

        foreach (var emoji in toLoad)
            Emojis.Add(emoji);

        _loadedCount += toLoad.Count;
    }

    private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
    {
        if (sender is ScrollViewer scroller &&
            scroller.VerticalOffset >= scroller.ScrollableHeight - 20)
        {
            LoadMoreEmojis();
        }
    }

    private void Emoji_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is TextBlock tb && tb.Text is string emoji)
        {
            _onEmojiSelected?.Invoke(emoji);

            var parent = this.Parent;
            while (parent != null && parent is not FlyoutPresenter)
                parent = (parent as FrameworkElement)?.Parent;

            if (parent is FlyoutPresenter presenter && presenter.Parent is Flyout flyout)
            {
                flyout.Hide();
            }
        }
    }

    public class EmojiData
    {
        public List<string> Emojis { get; set; } = new();
    }
}
