using Extensions;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Controls.Derived.TextBox
{
    public abstract class NumericTextBox<T> : ValidatingTextBox where T : IComparable
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(T), typeof(NumericTextBox<T>), new FrameworkPropertyMetadata(default(T), OnValueChanged));
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(T), typeof(NumericTextBox<T>), new FrameworkPropertyMetadata(default(T), OnMinimumChanged));
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(T), typeof(NumericTextBox<T>), new FrameworkPropertyMetadata(default(T), OnMaximumChanged));
        public static readonly DependencyProperty ToolTipNumericFormatProperty =
            DependencyProperty.Register("ToolTipNumericFormat", typeof(string), typeof(NumericTextBox<T>), new FrameworkPropertyMetadata(null, OnToolTipNumericFormatChanged));

        protected enum NumberType
        {
            Integer,
            Decimal
        };

        protected static readonly string DECIMAL_SEPARATOR;
        protected readonly NumberType numberType;
        protected readonly T typeMinimum;
        protected readonly T typeMaximum;

        private T lastValue;
        private bool valueModified = false;
        private bool notificationTextModified = false;
        private bool userDefinedNotificationText = false;

        static NumericTextBox()
        {
            DECIMAL_SEPARATOR = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        }

        protected NumericTextBox(string inputPattern, NumberType numberType, T typeMinimum, T typeMaximum)
        {
            this.numberType = numberType;
            this.typeMinimum = typeMinimum;
            this.typeMaximum = typeMaximum;
            InputPattern = inputPattern;
            InputNotificationShowDuration = 5500;
            ShowInputNotification = true;
            HorizontalContentAlignment = HorizontalAlignment.Right;
            userDefinedNotificationText = !this.IsInitial(NotificationTextProperty);
            UpdateNotificationText();
        }

        protected override void OnInitialized(System.EventArgs e)
        {
            base.OnInitialized(e);
            RefreshDisplayedValue();
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            if (!IsFocused)
            {
                RefreshDisplayedValue();
            }
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);

            if (e.Handled)
            {
                return;
            }

            if (e.Text == "-")
            {
                e.Handled = true;
                int newCaretIndex;

                if (Text.StartsWith("-") && (this.IsInitial(MaximumProperty) || Maximum.CompareTo(default(T)) >= 0))
                {
                    newCaretIndex = Math.Max(0, CaretIndex - 1);
                    Text = Text.Substring(1);
                }
                else if ((this.IsInitial(MinimumProperty) || Minimum.CompareTo(default(T)) < 0))
                {
                    newCaretIndex = CaretIndex + 1;
                    Text = Text.Insert(0, "-");
                }
                else
                {
                    ShowNotification();
                    return;
                }

                CaretIndex = newCaretIndex;
            }
            else if (e.Text == DECIMAL_SEPARATOR)
            {
                if (Text.Contains(DECIMAL_SEPARATOR))
                {
                    e.Handled = true;
                    ShowNotification();
                }
                else if (Text == string.Empty)
                {
                    e.Handled = true;
                    Text = "0" + DECIMAL_SEPARATOR;
                    CaretIndex = Text.Length;
                }
            }
        }

        protected override void OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            base.OnPasting(sender, e);

            if (e.CommandCancelled)
            {
                return;
            }

            string pasteText = (string)e.DataObject.GetData(typeof(string));
            string newText = Text.Insert(pasteText, CaretIndex, SelectionLength);

            if (!Regex.IsMatch(newText, "^-?[0-9]*" + DECIMAL_SEPARATOR + "?[0-9]*$"))
            {
                e.CancelCommand();
                ShowNotification();
                return;
            }
            else if (newText.StartsWith(DECIMAL_SEPARATOR))
            {
                e.CancelCommand();
                Text = "0" + newText;
                CaretIndex = Text.Length;
                return;
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            if (Text == default(T).ToString())
            {
                lastValue = default(T);
                Text = string.Empty;
                CaretIndex = Math.Max(0, Text.Length - 1);
            }
            else
            {
                lastValue = Value;
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            RefreshDisplayedValue();
        }

        private static void OnToolTipNumericFormatChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            NumericTextBox<T> sourceControl = source as NumericTextBox<T>;

            if (sourceControl.IsInitialized && sourceControl.HasValidationError)
            {
                sourceControl.Validate();
            }
        }

        private static void OnValueChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            NumericTextBox<T> sourceControl = source as NumericTextBox<T>;

            if (sourceControl.valueModified)
            {
                sourceControl.valueModified = false;
                return;
            }

            sourceControl.Text = sourceControl.FormatValueString((T)e.NewValue);

            if (sourceControl.IsInitialized)
            {
                sourceControl.RefreshDisplayedValue();
            }
        }
        private static void OnMinimumChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (!source.IsInitial(MaximumProperty) && (e.NewValue as IComparable).CompareTo(source.GetValue(MaximumProperty)) > 0)
            {
                throw new ArgumentOutOfRangeException("Minimum", "Minimum has to be lower or equal to Maximum.");
            }

            NumericTextBox<T> sourceControl = source as NumericTextBox<T>;
            sourceControl.UpdateNotificationText();

            if (sourceControl.IsInitialized)
            {
                sourceControl.RefreshDisplayedValue();
            }
        }

        private static void OnMaximumChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (!source.IsInitial(MinimumProperty) && (e.NewValue as IComparable).CompareTo(source.GetValue(MinimumProperty)) < 0)
            {
                throw new ArgumentOutOfRangeException("Maximum", "Maximum has to be higher or equal to Minimum.");
            }

            NumericTextBox<T> sourceControl = source as NumericTextBox<T>;
            sourceControl.UpdateNotificationText();

            if (sourceControl.IsInitialized)
            {
                sourceControl.RefreshDisplayedValue();
            }
        }

        public new string InputPattern
        {
            get
            {
                return base.InputPattern;
            }
            private set
            {
                base.InputPattern = value;
            }
        }

        public new string Text
        {
            get
            {
                return base.Text;
            }
            private set
            {
                base.Text = value;
            }
        }

        public override string NotificationText
        {
            get
            {
                return base.NotificationText;
            }
            set
            {
                if (!notificationTextModified)
                {
                    userDefinedNotificationText = value != null;
                }
                else
                {
                    notificationTextModified = false;
                }
                
                base.NotificationText = value;
            }
        }

        public bool HasParseError
        {
            get;
            private set;
        }

        public string ToolTipNumericFormat
        {
            get
            {
                return (string)GetValue(ToolTipNumericFormatProperty);
            }
            set
            {
                SetValue(ToolTipNumericFormatProperty, value);
            }
        }

        public T Value
        {
            get
            {
                return (T)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        public T Minimum
        {
            get
            {
                return (T)GetValue(MinimumProperty);
            }
            set
            {
                SetValue(MinimumProperty, value);
            }
        }

        public T Maximum
        {
            get
            {
                return (T)GetValue(MaximumProperty);
            }
            set
            {
                SetValue(MaximumProperty, value);
            }
        }

        protected override void InternalValidate()
        {
            T parseResult;
            T newValue;

            HasParseError = false;

            if (Text == string.Empty || Text == "-")
            {
                newValue = default(T);
            }
            else if (!Text.TryConvert<T>(out parseResult))
            {
                HasParseError = true;
                validationErrors.Add("Invalid format");
                validationErrors.Add("Has to be between " + FormatToolTipString(typeMinimum) + " and " + FormatToolTipString(typeMaximum));
                newValue = default(T);

                if (!newValue.Equals(Value))
                {
                    valueModified = true;
                    Value = newValue;
                }

                return;
            }
            else
            {
                newValue = parseResult;

                if (!newValue.Equals(Value))
                {
                    valueModified = true;
                    Value = newValue;
                }
            }

            if (IsOutOfBounds(newValue))
            {
                if (!this.IsInitial(MinimumProperty) && !this.IsInitial(MaximumProperty))
                {
                    validationErrors.Add("Has to be between " + FormatToolTipString(Minimum) + " and " + FormatToolTipString(Maximum));
                }
                else if (!this.IsInitial(MinimumProperty) && newValue.CompareTo(Minimum) < 0)
                {
                    validationErrors.Add("Has to be " + FormatToolTipString(Minimum) + " or higher");
                }
                else if (!this.IsInitial(MaximumProperty) && newValue.CompareTo(Maximum) > 0)
                {
                    validationErrors.Add("Has to be " + FormatToolTipString(Maximum) + " or lower");
                }
            }
        }

        protected virtual string FormatValueString(T value)
        {
            return String.Format("{0:F99}", value).TrimEnd('0').TrimEnd(DECIMAL_SEPARATOR.ToCharArray());
        }

        protected string FormatToolTipString(T value)
        {
            if (ToolTipNumericFormat == null)
            {
                return value.ToString();
            }
            else
            {
                return String.Format(ToolTipNumericFormat, value);
            }
        }

        private void RefreshDisplayedValue()
        {
            string validText = CalculateValidValue();

            if (!string.IsNullOrWhiteSpace(validText) && validText != Text)
            {
                Text = validText;
                CaretIndex = Text.Length;
            }

            Validate();
        }

        private string CalculateValidValue()
        {
            T validValue;

            if (Text == string.Empty || Text == "-")
            {
                if (HasValidationError)
                {
                    validValue = lastValue;
                }
                else
                {
                    validValue = default(T);
                }
            }
            else
            {
                T parseResult;

                if (Text.TryConvert<T>(out parseResult))
                {
                    validValue = parseResult;
                }
                else
                {
                    return null;
                }
            }

            return FormatValueString(validValue);
        }

        private T AdjustToBounds(T value)
        {
            if (!this.IsInitial(MinimumProperty) && value.CompareTo(Minimum) < 0)
            {
                return Minimum;
            }
            else if (!this.IsInitial(MaximumProperty) && value.CompareTo(Maximum) > 0)
            {
                return Maximum;
            }
            else
            {
                return value;
            }
        }

        private bool IsOutOfBounds(T value)
        {
            return value.CompareTo(AdjustToBounds(value)) != 0;
        }

        private void UpdateNotificationText()
        {
            if (userDefinedNotificationText)
            {
                return;
            }

            string sign;

            if (!this.IsInitial(MinimumProperty) && Minimum.CompareTo(default(T)) >= 0)
            {
                sign = "positive";
            }
            else if (!this.IsInitial(MinimumProperty) && Minimum.CompareTo(default(T)) < 0)
            {
                sign = "negative";
            }
            else
            {
                sign = null;
            }

            string numberTypeName;

            if (numberType == NumberType.Integer)
            {
                numberTypeName = "integer";
            }
            else
            {
                numberTypeName = "decimal number";
            }

            string phrase;
            string article;

            if (sign != null)
            {
                phrase = "a " + sign + " " + numberTypeName;
            }
            else
            {
                if (numberType == NumberType.Integer)
                {
                    article = "an";
                }
                else
                {
                    article = "a";
                }

                phrase = article + " " + numberTypeName;
            }

            notificationTextModified = true;
            NotificationText = "Invalid input\nPlease enter " + phrase;
        }
    }
}
