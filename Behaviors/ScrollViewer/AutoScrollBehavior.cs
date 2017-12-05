using System;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Behaviors.ScrollViewer
{
    // Requires System.Windows.Interactivity
    public class AutoScrollBehavior : Behavior<System.Windows.Controls.ScrollViewer>
    {
        private System.Windows.Controls.ScrollViewer scrollViewer = null;
        private double oldScrollableHeight = 0;
        private bool lockToBottom = true;

        protected override void OnAttached()
        {
            base.OnAttached();

            scrollViewer = base.AssociatedObject;
            scrollViewer.LayoutUpdated += ScrollViewer_LayoutUpdated;
            scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
        }

        private void ScrollViewer_LayoutUpdated(object sender, EventArgs e)
        {
            if (lockToBottom && scrollViewer.ScrollableHeight != oldScrollableHeight)
            {
                scrollViewer.ScrollToBottom();
            }

            oldScrollableHeight = scrollViewer.ScrollableHeight;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange == 0)
            {
                return;
            }

            lockToBottom = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight * 0.99;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (scrollViewer != null)
            {
                scrollViewer.LayoutUpdated -= ScrollViewer_LayoutUpdated;
                scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            }
        }
    }
}
