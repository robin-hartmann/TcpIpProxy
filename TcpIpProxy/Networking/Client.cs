using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Threading;
using TcpIp;
using TcpIpProxy.Configuration;
using TcpIpProxy.Helper;
using TcpIpProxy.Threading;

namespace TcpIpProxy.Networking
{
    class Client
    {
        private static int CLIENT_ID = 0;

        private int automaticRequestInterval;
        private DispatcherTimer requestTimer = new DispatcherTimer();

        public static readonly int DEF_AUTOMATIC_REQUEST_INTERVAL = -1;

        public Client()
        {
            requestTimer.Tick += requestTimer_Tick;
        }

        private async void requestTimer_Tick(object sender, EventArgs e)
        {
            await RequestAsync(AutomaticRequestString, AutomaticRequestEndPoint);
        }

        public int AutomaticRequestInterval
        {
            get
            {
                return automaticRequestInterval;
            }
            set
            {
                automaticRequestInterval = value;

                if (automaticRequestInterval > 0)
                {
                    requestTimer.Interval = new TimeSpan(0, 0, 0, 0, automaticRequestInterval);
                    requestTimer.IsEnabled = true;
                }
                else
                {
                    requestTimer.IsEnabled = false;
                    requestTimer.Interval = TimeSpan.Zero;
                }
            }
        }

        public string AutomaticRequestString
        {
            get;
            set;
        }

        public IPEndPoint AutomaticRequestEndPoint
        {
            get;
            set;
        }

        public Task RequestAsync(String requestString, IPEndPoint remoteEndPoint)
        {
            return Task.Run(() =>
            {
                StateReporter statusReporter = new StateReporter(++CLIENT_ID, this);

                try 
                {
                    using (TcpIpClient client = new TcpIpClient(remoteEndPoint.AddressFamily, Preferences.ClientStringEncoding, Preferences.ReceiveTimeout, Preferences.SendTimeout, TcpIpClient.DEF_RECEIVE_BUFFER_SIZE))
                    {
                        statusReporter.Report("Connecting to " + remoteEndPoint + "...");
                        client.Connect(remoteEndPoint);

                        if (Preferences.ReplaceHex) 
                        {
                            requestString = Utilities.ConvertAllHex(requestString, Preferences.ClientStringEncoding);
                        }

                        statusReporter.Report("Connected to " + client.RemoteEndPoint + ".");

                        client.Send(requestString);
                        statusReporter.Report("Request sent to " + client.RemoteEndPoint + ":", requestString, true);

                        string respsonseString;

                        while (true)
                        {
                            respsonseString = client.Receive();

                            if (respsonseString == null)
                            {
                                statusReporter.Report("The socket at " + client.RemoteEndPoint + " was shut down.");
                                break;
                            }
                            else
                            {
                                statusReporter.Report("Response received from " + client.RemoteEndPoint + ":", respsonseString, true);
                            }
                        }
                    }
                }
                catch (SocketException ex)
                {
                    statusReporter.Report("Socket Error " + ex.ErrorCode + ":\n" + ex.Message);
                }

                statusReporter.Report("Client stopped.");
            });
        }
    }
}
