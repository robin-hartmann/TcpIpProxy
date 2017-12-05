namespace Controls.Derived.TextBox
{
    public class IntegerTextBox : NumericTextBox<int>
    {
        public IntegerTextBox()
            : base("^[\\-0-9]+$", NumberType.Integer, int.MinValue, int.MaxValue)
        {
            MaxLength = typeMinimum.ToString().Length;
            ToolTipNumericFormat = "{0:N0}";
        }
    }
}
