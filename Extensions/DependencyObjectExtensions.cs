using System.Windows;
using System.Windows.Media;

namespace Extensions
{
    public static class DependencyObjectExtensions
    {
        public static bool IsInitial(this DependencyObject source, DependencyProperty property)
        {
            return source.ReadLocalValue(property) == DependencyProperty.UnsetValue;
        }

        public static T FindParent<T>(this DependencyObject child)
        where T : DependencyObject
        {
            // Get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            // End of tree reached
            if (parentObject == null)
                return null;

            // Check if the parent matches the specified type
            var parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            return FindParent<T>(parentObject);
        }
    }
}
