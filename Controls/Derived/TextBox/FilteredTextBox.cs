using Controls.Derived.ToolTip;
using Extensions;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Controls.Derived.TextBox
{
    public class FilteredTextBox : System.Windows.Controls.TextBox
    {
        public static readonly DependencyProperty InputPatternProperty =
            DependencyProperty.Register("InputPattern", typeof(string), typeof(FilteredTextBox), new FrameworkPropertyMetadata(".*?"));
        public static readonly DependencyProperty ShowNotificationProperty =
            DependencyProperty.Register("ShowNotification", typeof(bool), typeof(FilteredTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty NotificationShowDurationProperty =
            DependencyProperty.Register("NotificationShowDuration", typeof(int), typeof(FilteredTextBox), new FrameworkPropertyMetadata(Notification.DEF_SHOW_DURATION, OnNotificationShowDurationChanged));
        public static readonly DependencyProperty CloseNotificationOnLostFocusProperty =
            DependencyProperty.Register("CloseNotificationOnLostFocus", typeof(bool), typeof(FilteredTextBox), new FrameworkPropertyMetadata(Notification.DEF_CLOSE_ON_LOST_FOCUS, OnCloseNotificationOnLostFocusChanged));
        public static readonly DependencyProperty NotificationTextProperty =
            DependencyProperty.Register("NotificationText", typeof(string), typeof(FilteredTextBox), new FrameworkPropertyMetadata("Invalid input", OnNotificationTextChanged));

        protected static readonly SolidColorBrush STRONG_RED = new SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 0, 0));
        protected static readonly SolidColorBrush LIGHT_RED = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 240, 240));

        private Notification inputNotification;

        private static void OnNotificationShowDurationChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if ((source as FilteredTextBox).IsInitialized)
            {
                (source as FilteredTextBox).inputNotification.ShowDuration = (int)e.NewValue;
            }
        }

        private static void OnCloseNotificationOnLostFocusChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if ((source as FilteredTextBox).IsInitialized)
            {
                (source as FilteredTextBox).inputNotification.CloseOnLostFocus = (bool)e.NewValue;
            }
        }

        protected static void OnNotificationTextChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if ((source as FilteredTextBox).IsInitialized)
            {
                (source as FilteredTextBox).inputNotification.Text = (string)e.NewValue;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            inputNotification = new Notification(this, SystemIcons.Error, NotificationText)
            {
                CloseOnLostFocus = CloseInputNotificationOnLostFocus,
                IconWidth = 22,
                HorizontalOffset = -2,
                Foreground = System.Windows.Media.Brushes.White,
                Background = STRONG_RED,
                BorderBrush = STRONG_RED
            };

            if (!this.IsInitial(NotificationShowDurationProperty))
            {
                inputNotification.ShowDuration = InputNotificationShowDuration;
            }

            DataObject.AddPastingHandler(this, OnPasting);
            base.OnInitialized(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (this.IsReadOnly)
            {
                return;
            }

            if (e.Key == Key.Space && !IsValidInput(" "))
            {
                e.Handled = true;
                ShowNotification();
            }
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (this.IsReadOnly)
            {
                return;
            }

            if (!IsValidInput(e.Text))
            {
                e.Handled = true;

                // Completely ignore ESC and ENTER
                // Not implemented in OnKeyDown to be able to handle it in parent elements (i.e. for closing a window)
                if (e.Text.IndexOfAny(new char[] { '\u001B', '\r' }) < 0)
                {
                    ShowNotification();
                }
            }
        }

        protected virtual void OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pasteText = (string)e.DataObject.GetData(typeof(string));

                if (!IsValidInput(pasteText))
                {
                    e.CancelCommand();
                    ShowNotification();
                }
            }
            else
            {
                e.CancelCommand();
                ShowNotification();
            }
        }

        public string InputPattern
        {
            get
            {
                return (string)GetValue(InputPatternProperty);
            }
            set
            {
                SetValue(InputPatternProperty, value);
            }
        }

        public bool ShowInputNotification
        {
            get
            {
                return (bool)GetValue(ShowNotificationProperty);
            }
            set
            {
                SetValue(ShowNotificationProperty, value);
            }
        }

        public int InputNotificationShowDuration
        {
            get
            {
                return (int)GetValue(NotificationShowDurationProperty);
            }
            set
            {
                SetValue(NotificationShowDurationProperty, value);
            }
        }

        public bool CloseInputNotificationOnLostFocus
        {
            get
            {
                return (bool)GetValue(CloseNotificationOnLostFocusProperty);
            }
            set
            {
                SetValue(CloseNotificationOnLostFocusProperty, value);
            }
        }

        public virtual string NotificationText
        {
            get
            {
                return (string)GetValue(NotificationTextProperty);
            }
            set
            {
                SetValue(NotificationTextProperty, value);
            }
        }

        protected virtual bool IsValidInput(string input)
        {
            return Regex.IsMatch(input, InputPattern);
        }

        protected void ShowNotification()
        {
            if (ShowInputNotification)
            {
                inputNotification.Show();
            }
        }
    }
}
