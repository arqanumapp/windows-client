using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Arqanum.Utilities
{
    public static class VisualTreeHelpers
    {
        public static T? FindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            DependencyObject? parent = VisualTreeHelper.GetParent(child);

            while (parent != null && parent is not T)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as T;
        }
    }
}
