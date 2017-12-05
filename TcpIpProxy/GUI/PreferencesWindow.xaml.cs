using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using TcpIpProxy.Configuration;

namespace TcpIpProxy.GUI
{
    /// <summary>
    /// Interaktionslogik für PreferencesWindow.xaml
    /// </summary>
    public partial class PreferencesWindow : Window
    {
        private bool initialized = false;
        private bool firstSelectionChange = true;
        private Encoding lastServerEncoding;

        public PreferencesWindow()
        {
            InitializeComponent();

            List<Encoding> supportedEncodings = new List<Encoding>()
            {
                Encoding.ASCII,
                Encoding.UTF8,
                Encoding.Unicode,
                Encoding.BigEndianUnicode,
                Encoding.UTF32,
                Encoding.GetEncoding("utf-32be"),
                Encoding.GetEncoding("Windows-1252")
            };

            cbxClientEncoding.DisplayMemberPath = "WebName";
            cbxClientEncoding.ItemsSource = supportedEncodings;
            cbxServerEncoding.DisplayMemberPath = cbxClientEncoding.DisplayMemberPath;
            cbxServerEncoding.ItemsSource = cbxClientEncoding.ItemsSource;
            LoadConfig();
            lastServerEncoding = Preferences.ServerStringEncoding;
            initialized = true;
        }

        private void LoadConfig()
        {
            tbxSendTimeout.Value = Preferences.SendTimeout;
            tbxReceiveTimeout.Value = Preferences.ReceiveTimeout;
            tbxNonPrintableReplacer.Text = Preferences.NonPrintableReplacer.ToString();
            tbxHexInputFormat.Text = Preferences.HexInputFormat;
            tbxHexOutputFormat.Text = Preferences.HexOutputFormat;
            chbReplaceHex.IsChecked = Preferences.ReplaceHex;
            cbxClientEncoding.SelectedItem = Preferences.ClientStringEncoding;
            cbxServerEncoding.SelectedItem = Preferences.ServerStringEncoding;
        }

        private bool SaveConfig()
        {
            MainWindow mainWindow = (Application.Current.MainWindow as MainWindow);

            if (mainWindow.ServerRunning && !cbxServerEncoding.SelectedItem.Equals(lastServerEncoding))
            {
                MessageBoxResult result = MessageBox.Show(this, "In order to apply the new server encoding,\nthe server has to be restarted.\n\nDo you want to restart the server now?", "Server Encoding changed", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);

                if (result == MessageBoxResult.Cancel)
                {
                    return false;
                }
                else
                {
                    SaveConfigInternal();

                    if (result == MessageBoxResult.Yes)
                    {
                        mainWindow.BtnToggleServer_Click(null, null);
                        mainWindow.BtnToggleServer_Click(null, null);
                    }
                }
            }
            else
            {
                SaveConfigInternal();
            }

            return true;
        }

        private void SaveConfigInternal()
        {
            Preferences.SendTimeout = tbxSendTimeout.Value;
            Preferences.ReceiveTimeout = tbxReceiveTimeout.Value;
            Preferences.NonPrintableReplacer = tbxNonPrintableReplacer.Text.ToCharArray()[0];
            Preferences.HexInputFormat = tbxHexInputFormat.Text;
            Preferences.HexOutputFormat = tbxHexOutputFormat.Text;
            Preferences.ReplaceHex = chbReplaceHex.IsChecked.GetValueOrDefault();
            Preferences.ClientStringEncoding = cbxClientEncoding.SelectedItem as Encoding;
            Preferences.ServerStringEncoding = cbxServerEncoding.SelectedItem as Encoding;
            lastServerEncoding = Preferences.ServerStringEncoding;
        }

        private void Defaults_Click(object sender, RoutedEventArgs e)
        {
            tbxSendTimeout.Value = Preferences.DEF_SEND_TIMEOUT;
            tbxReceiveTimeout.Value = Preferences.DEF_RECEIVE_TIMEOUT;
            tbxNonPrintableReplacer.Text = Preferences.DEF_NON_PRINTABLE_REPLACER.ToString();
            tbxHexInputFormat.Text = Preferences.DEF_HEX_INPUT_FORMAT;
            tbxHexOutputFormat.Text = Preferences.DEF_HEX_OUTPUT_FORMAT;
            chbReplaceHex.IsChecked = Preferences.DEF_REPLACE_HEX;
            cbxClientEncoding.SelectedItem = Preferences.DEF_STRING_ENCODING;
            cbxServerEncoding.SelectedItem = Preferences.DEF_STRING_ENCODING;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (SaveConfig())
            {
                this.Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (SaveConfig())
            {
                btnApply.IsEnabled = false;
            }
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateButtonState();
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (initialized && firstSelectionChange)
            {
                cbxClientEncoding.SelectedItem = e.AddedItems[0];
                cbxServerEncoding.SelectedItem = e.AddedItems[0];
                firstSelectionChange = false;
            }

            UpdateButtonState();
        }

        private void CheckedChanged(object sender, RoutedEventArgs e)
        {
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            if (initialized)
            {
                bool inputValid = !tbxSendTimeout.HasValidationError
                    && !tbxReceiveTimeout.HasValidationError
                    && !tbxNonPrintableReplacer.HasValidationError
                    && !tbxHexOutputFormat.HasValidationError
                    && (!chbReplaceHex.IsChecked.GetValueOrDefault() || !tbxHexInputFormat.HasValidationError);

                btnOk.IsEnabled = inputValid;
                btnApply.IsEnabled = inputValid;
            }
        }
    }
}
