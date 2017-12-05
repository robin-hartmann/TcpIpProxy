using Behaviors.DataGrid;
using Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using TcpIpProxy.Networking;
using TcpIpProxy.Threading;

namespace TcpIpProxy.GUI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string REQUEST_SEND = "Send Request";
        private static readonly string REQUEST_TIMER_START = "Start Timer";
        private static readonly string REQUEST_TIMER_STOP = "Stop Timer";

        private static readonly string SERVER_START = "Start Server";
        private static readonly string SERVER_STOP = "Stop Server";
        private static readonly string SERVER_STATE_STOPPED = "Server stopped";
        private static readonly string SERVER_STATE_WAITING = "Server listening at ";
        private static readonly string SERVER_STATE_CONNECTED = "Server connected to ";

        private readonly Client client;
        private readonly Server server;
        private readonly StateReporter clientStatusReporter;
        private readonly StateReporter serverStatusReporter;

        private List<Command> commands = new List<Command>();
        private DataGridCellEditEndingEventArgs lastCellEditEndingEventArgs;

        public MainWindow()
        {
            InitializeComponent();
            DataObject.AddPastingHandler(tbxRemoteIp, tbxRemoteIp_OnPasting);

            client = new Client();
            server = new Server();
            clientStatusReporter = new StateReporter(client);
            serverStatusReporter = new StateReporter(server);

            this.Title = "TCP/IP-Proxy " + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

            StateReporter.setReporter(new Progress<State>(ProcessState));

            Interaction.GetBehaviors(dgCommands).Add(new RemoveEmptyRowBehavior<Command>());

            //commands.Add(new Command()
            //{
            //    Request = "Marco",
            //    Response = "Polo"
            //});

            dgCommands.ItemsSource = commands;
            server.CommandsList = commands;

            btnToggleServer.Content = SERVER_START;
            sbServerStatus.Text = SERVER_STATE_STOPPED;
            checkBoxCloseConnection.IsChecked = true;

            tbxRequestInterval.Value = 1000;
            tbxLocalPort.Value = 8000;

            //tbxRemoteIp.Text = "webservice";
            //tbxRequest.Text = "GET /qs1/ HTTP/1.1\nHost: " + tbxRemoteIp.Text + "\n\n";
            //tbxRemotePort.Value = 80;

            //tbxRequest.Text = "Marco";
            //tbxResponse.Text = "Polo";

            tbxRemoteIp.Text = TcpIp.Helper.GetHostIpAsync(Dns.GetHostName()).Result.ToString();
            tbxRemotePort.Value = 8000;

            UpdateView();
        }

        public async void BtnToggleServer_Click(object sender, RoutedEventArgs e)
        {
            if (server.Running)
            {
                server.Stop();
            }
            else
            {
                UInt16 port = (UInt16)tbxLocalPort.Value;

                if (TcpIp.Helper.IsAvailablePort(port))
                {
                    await server.StartAsync(port);
                }
                else
                {
                    serverStatusReporter.Report("Server can't bind to port " + port + ", it's already in use.");
                }
            }
        }

        private void tbxRemoteIp_OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pasteText = (string)e.DataObject.GetData(typeof(string));
                int colonIndex = pasteText.IndexOf(':');

                if (colonIndex >= 0)
                {
                    UInt16 port;

                    if (UInt16.TryParse(pasteText.Substring(colonIndex + 1), out port))
                    {
                        e.CancelCommand();
                        tbxRemoteIp.Text = pasteText.Substring(0, colonIndex);
                        tbxRemotePort.Value = port;
                    }
                }
            }
        }

        private void TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (this.IsInitialized)
            {
                UpdateView();
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuPreferences_Click(object sender, RoutedEventArgs e)
        {
            PreferencesWindow preferences = new PreferencesWindow();
            preferences.Owner = this;
            preferences.ShowDialog();
        }

        private async void BtnSendRequest_Click(object sender, RoutedEventArgs e)
        {
            if (btnSendRequest.Content.Equals(REQUEST_TIMER_STOP))
            {
                tbxRequestInterval.IsReadOnly = false;

                client.AutomaticRequestInterval = Client.DEF_AUTOMATIC_REQUEST_INTERVAL;
                client.AutomaticRequestEndPoint = null;
                client.AutomaticRequestString = string.Empty;

                btnSendRequest.Content = REQUEST_TIMER_START;
                return;
            }

            IPAddress ip;

            try
            {
                clientStatusReporter.Report("Resolving remote host adress " + tbxRemoteIp.Text + "...");
                ip = await TcpIp.Helper.GetHostIpAsync(tbxRemoteIp.Text);
            }
            catch (Exception ex)
            {
                clientStatusReporter.Report("An error occurred:\n" + ex.Message);
                return;
            }

            IPEndPoint requestEndpoint = new IPEndPoint(ip, tbxRemotePort.Value);

            if (btnSendRequest.Content.Equals(REQUEST_SEND))
            {
                await client.RequestAsync(tbxRequest.Text, requestEndpoint);
            }
            else
            {
                tbxRequestInterval.IsReadOnly = true;

                client.AutomaticRequestInterval = tbxRequestInterval.Value;
                client.AutomaticRequestEndPoint = requestEndpoint;
                client.AutomaticRequestString = tbxRequest.Text;

                btnSendRequest.Content = REQUEST_TIMER_STOP;
            }
        }

        private void dgCommands_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel)
            {
                lastCellEditEndingEventArgs = e;
                return;
            }

            DataGridCell requestCell = dgCommands.ExtractDataGridCell(new DataGridCellInfo(e.Row.Item, dgCommands.Columns[0]));
            string requestText;

            if (e.Column == dgCommands.Columns[0])
            {
                requestText = (requestCell.Content as TextBox).Text;
            }
            else
            {
                requestText = (requestCell.Content as TextBlock).Text;
            }

            if (string.IsNullOrEmpty(requestText))
            {
                e.Cancel = true;
                lastCellEditEndingEventArgs = e;
                dgCommands.CancelEdit();
                return;
            }

            if (e.Column != dgCommands.Columns[0])
            {
                lastCellEditEndingEventArgs = e;
                return;
            }

            TextBlock cellContent;

            foreach (object item in dgCommands.Items)
            {
                cellContent = (e.Column.GetCellContent(item) as TextBlock);

                if (cellContent == null || item == CollectionView.NewItemPlaceholder)
                {
                    continue;
                }
                else if (cellContent.Text == requestText)
                {
                    e.Cancel = true;
                    lastCellEditEndingEventArgs = e;
                    dgCommands.CancelEdit();
                    break;
                }
            }

            lastCellEditEndingEventArgs = e;
        }

        private void BtnSendResponse_Click(object sender, RoutedEventArgs e)
        {
            server.Respond(tbxResponse.Text);
        }

        private void checkBoxAutoRequest_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool isChecked = checkBoxAutoRequest.IsChecked.GetValueOrDefault();

            tbxRequest.IsReadOnly = tbxRemoteIp.IsReadOnly = tbxRemotePort.IsReadOnly = isChecked;
            UpdateView();

            if (isChecked)
            {
                btnSendRequest.Content = REQUEST_TIMER_START;
            }
            else
            {
                tbxRequestInterval.IsReadOnly = false;

                client.AutomaticRequestInterval = Client.DEF_AUTOMATIC_REQUEST_INTERVAL;
                btnSendRequest.Content = REQUEST_SEND;
            }
        }

        private void CheckBoxAutoResponse_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool isChecked = checkBoxAutoResponse.IsChecked.GetValueOrDefault();

            tbxResponse.IsReadOnly = isChecked;
            UpdateView();

            if (isChecked)
            {
                server.AutomaticResponseString = tbxResponse.Text;
            }
            else
            {
                checkBoxUseCommands.IsChecked = false;
                server.AutomaticResponseString = string.Empty;
            }

            server.AutomaticResponseEnabled = isChecked;
        }

        private void checkBoxCommandMode_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool isChecked = checkBoxUseCommands.IsChecked.GetValueOrDefault();

            if (isChecked)
            {
                tbxResponse.Visibility = Visibility.Collapsed;
                dgCommands.Visibility = Visibility.Visible;
            }
            else
            {
                tbxResponse.Visibility = Visibility.Visible;
                dgCommands.Visibility = Visibility.Collapsed;
            }

            server.UseCommandsEnabled = isChecked;
        }

        private void checkBoxCloseConnection_CheckedChanged(object sender, RoutedEventArgs e)
        {
            server.CloseConnectionAfterRespondingEnabled = checkBoxCloseConnection.IsChecked.GetValueOrDefault();
        }

        public bool ServerRunning
        {
            get;
            private set;
        }

        private void UpdateView()
        {
            btnSendRequest.IsEnabled = !string.IsNullOrWhiteSpace(tbxRemoteIp.Text) && !tbxRemotePort.HasValidationError
                && (!checkBoxAutoRequest.IsChecked.GetValueOrDefault() || !tbxRequestInterval.HasValidationError);
            checkBoxAutoRequest.IsEnabled = !string.IsNullOrWhiteSpace(tbxRemoteIp.Text) && !tbxRemotePort.HasValidationError;
            tbxRequestInterval.IsEnabled = lblRequestInterval.IsEnabled = checkBoxAutoRequest.IsChecked.GetValueOrDefault();

            btnSendResponse.IsEnabled = server.Connected && !checkBoxAutoResponse.IsChecked.GetValueOrDefault();
            checkBoxUseCommands.IsEnabled = checkBoxAutoResponse.IsChecked.GetValueOrDefault();

            btnToggleServer.IsEnabled = !tbxLocalPort.HasValidationError;
        }

        private void ProcessState(State state)
        {
            // Update the server's StatusBarItem
            // when the server reports a change
            if (state.Sender is Server)
            {
                UpdateView();
                ServerRunning = server.Running;

                if (server.Running)
                {
                    tbxLocalPort.IsReadOnly = true;
                    btnToggleServer.Content = SERVER_STOP;

                    if (server.Connected)
                    {
                        sbServerStatus.Text = SERVER_STATE_CONNECTED + server.RemoteEndPoint;
                    }
                    else
                    {
                        sbServerStatus.Text = SERVER_STATE_WAITING + server.LocalEndPoint;
                    }
                }
                else
                {
                    tbxLocalPort.IsReadOnly = false;
                    btnToggleServer.Content = SERVER_START;
                    sbServerStatus.Text = SERVER_STATE_STOPPED;
                }
            }

            hexConsole.AppendLine(state);
        }
    }
}
