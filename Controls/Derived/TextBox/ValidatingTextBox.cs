using Controls.Derived.ToolTip;
using Extensions;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Controls.Derived.TextBox
{
    public class ValidatingTextBox : FilteredTextBox
    {
        public static readonly DependencyProperty TextPatternProperty =
            DependencyProperty.Register("TextPattern", typeof(string), typeof(FilteredTextBox), new FrameworkPropertyMetadata(null, OnTextPatternChanged));
        public static readonly DependencyProperty ToolTipTextProperty =
            DependencyProperty.Register("ToolTipText", typeof(string), typeof(FilteredTextBox), new FrameworkPropertyMetadata("Invalid format", OnToolTipTextChanged));
        public static readonly DependencyProperty ToolTipShowDurationProperty =
            DependencyProperty.Register("ToolTipShowDuration", typeof(int), typeof(FilteredTextBox), new FrameworkPropertyMetadata(TimedToolTip.DEF_SHOW_DURATION, OnToolTipShowDurationChanged));

        protected List<string> validationErrors = new List<string>();
        protected TimedToolTip validationToolTip;

        private bool borderModified = false;

        public ValidatingTextBox()
        {
            validationToolTip = new TimedToolTip(this)
            {
                Placement = PlacementMode.Bottom,
                Foreground = System.Windows.Media.Brushes.White,
                Background = STRONG_RED,
                BorderBrush = STRONG_RED
            };

            IsEnabledChanged += ValidatingTextBox_IsEnabledChanged;
        }

        protected override void OnTextChanged(System.Windows.Controls.TextChangedEventArgs e)
        {
            Validate();
            base.OnTextChanged(e);
        }

        private void ValidatingTextBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ValidatingTextBox sourceControl = sender as ValidatingTextBox;

            if (!sourceControl.IsInitialized)
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                UpdateErrorState();
            }
            else
            {
                this.ClearValue(BorderThicknessProperty);
                borderModified = false;
            }
        }

        public string TextPattern
        {
            get
            {
                return (string)GetValue(TextPatternProperty);
            }
            set
            {
                SetValue(TextPatternProperty, value);
            }
        }

        public string ToolTipText
        {
            get
            {
                return (string)GetValue(ToolTipTextProperty);
            }
            set
            {
                SetValue(ToolTipTextProperty, value);
            }
        }

        public int ToolTipShowDuration
        {
            get
            {
                return (int)GetValue(ToolTipShowDurationProperty);
            }
            set
            {
                SetValue(ToolTipShowDurationProperty, value);
            }
        }

        public bool HasValidationError
        {
            get
            {
                return validationErrors.Count != 0;
            }
        }

        public new object ToolTip
        {
            get
            {
                return base.ToolTip;
            }
            private set
            {
                base.ToolTip = value;
            }
        }

        protected static void OnTextPatternChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ValidatingTextBox sourceControl = (source as ValidatingTextBox);

            if (sourceControl.IsInitialized)
            {
                sourceControl.Validate();
            }
        }

        protected static void OnToolTipTextChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ValidatingTextBox sourceControl = (source as ValidatingTextBox);

            if (sourceControl.IsInitialized)
            {
                sourceControl.validationErrors.Clear();
                sourceControl.InternalValidate();
            }
        }

        protected static void OnToolTipShowDurationChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as ValidatingTextBox).validationToolTip.ShowDuration = (int)e.NewValue;
        }

        protected void Validate()
        {
            validationErrors.Clear();
            InternalValidate();
            UpdateErrorState();
        }

        protected virtual void InternalValidate()
        {
            if (!this.IsInitial(TextPatternProperty) && !string.IsNullOrEmpty(TextPattern) && !Regex.IsMatch(Text, TextPattern))
            {
                validationErrors.Add(ToolTipText);
            }
        }

        private void UpdateErrorState()
        {
            if (!this.IsInitialized || !this.IsEnabled)
            {
                return;
            }

            if (HasValidationError)
            {
                for (int i = 0; i < validationErrors.Count; i++)
                {
                    if (i == 0)
                    {
                        validationToolTip.Content = validationErrors[i];
                    }
                    else
                    {
                        validationToolTip.Content += "\n" + validationErrors[i];
                    }
                }

                ToolTip = validationToolTip;
                Background = LIGHT_RED;
                BorderBrush = STRONG_RED;

                if (!borderModified)
                {
                    borderModified = true;
                    // Thickness needs to be changed or else the default MouseEnter animation will change the color
                    // 0.01 ist just enough to achieve that and its not noticeable
                    BorderThickness = new Thickness(BorderThickness.Left + 0.01, BorderThickness.Top + 0.01, BorderThickness.Right + 0.01, BorderThickness.Bottom + 0.01);
                }
            }
            else
            {
                ToolTip = null;
                this.ClearValue(BackgroundProperty);
                this.ClearValue(BorderBrushProperty);
                this.ClearValue(BorderThicknessProperty);
                borderModified = false;
            }
        }
    }
}
