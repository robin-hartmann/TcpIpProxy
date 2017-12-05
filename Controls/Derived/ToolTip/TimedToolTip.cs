using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Controls.Derived.ToolTip
{
    public class TimedToolTip : System.Windows.Controls.ToolTip
    {
        public static readonly bool DEF_CLOSE_ON_MOUSE_LEAVE = true;
        public static readonly bool DEF_CLOSE_ON_LOST_FOCUS = true;
        public static readonly int DEF_SHOW_DURATION = 3500;
        public static readonly int INFINITE_SHOW_DURATION = -1;

        private int showDuration;
        private DispatcherTimer closeTimer = new DispatcherTimer();

        public TimedToolTip(UIElement placementTarget)
        {
            ToolTipService.SetShowDuration(placementTarget, int.MaxValue);
            this.PlacementTarget = placementTarget;
            placementTarget.LostFocus += placementTarget_LostFocus;
            placementTarget.MouseLeave += placementTarget_MouseLeave;
            closeTimer.Tick += closeTimer_Tick;
            this.Opened += ToolTip_Opened;
            this.Closed += ToolTip_Closed;

            CloseOnMouseLeave = DEF_CLOSE_ON_MOUSE_LEAVE;
            CloseOnLostFocus = DEF_CLOSE_ON_LOST_FOCUS;
            ShowDuration = DEF_SHOW_DURATION;
        }

        void placementTarget_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (CloseOnMouseLeave)
            {
                Close();
            }
        }

        void placementTarget_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CloseOnLostFocus)
            {
                Close();
            }
        }

        void closeTimer_Tick(object sender, EventArgs e)
        {
            if (ShowDuration != 0)
            {
                this.IsOpen = false;
            }
        }

        void ToolTip_Opened(object sender, RoutedEventArgs e)
        {
            if (ShowDuration != 0)
            {
                closeTimer.Stop();
                closeTimer.Start();
            }
        }

        void ToolTip_Closed(object sender, RoutedEventArgs e)
        {
            closeTimer.Stop();
        }

        public int ShowDuration
        {
            get
            {
                return showDuration;
            }
            set
            {
                if (value == INFINITE_SHOW_DURATION)
                {
                    showDuration = value;
                    closeTimer.Interval = TimeSpan.Zero;
                }
                else if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("ShowDuration must be zero or greater.");
                }
                else
                {
                    showDuration = value;
                    closeTimer.Interval = new TimeSpan(0, 0, 0, 0, value);
                }
            }

        }

        public bool CloseOnMouseLeave
        {
            get;
            set;
        }

        public bool CloseOnLostFocus
        {
            get;
            set;
        }

        public void Show()
        {
            if (CanShow())
            {
                this.IsOpen = true;
            }
        }

        public void Close()
        {
            this.IsOpen = false;
        }

        protected virtual bool CanShow()
        {
            return Content != null;
        }
    }
}
