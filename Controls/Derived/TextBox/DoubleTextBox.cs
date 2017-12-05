namespace Controls.Derived.TextBox
{
    public class DoubleTextBox : NumericTextBox<double>
    {
        public DoubleTextBox()
            : base("^[\\-" + DECIMAL_SEPARATOR + "0-9]+$", NumberType.Decimal, double.MinValue, double.MaxValue)
        {
        }
    }
}
