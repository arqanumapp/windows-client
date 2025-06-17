using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using YourApp.Controls;

namespace Arqanum.Pages
{
    public sealed partial class DefaultChatPage : Page
    {
        public DefaultChatPage()
        {
            InitializeComponent();
        }

        //private void EmojiButton_Click(object sender, RoutedEventArgs e)
        //{
        //    var picker = new EmojiPicker();

        //    picker.SetCallback(emoji =>
        //    {
        //        MessageBox.Text += emoji;
        //    });

        //    var flyout = new Flyout
        //    {
        //        Content = picker,
        //        Placement = FlyoutPlacementMode.Bottom
        //    };

        //    flyout.ShowAt((FrameworkElement)sender);
        //}
    }
}
