using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TcpIp;
using TcpIpProxy.Configuration;
using TcpIpProxy.Helper;
using TcpIpProxy.Threading;

namespace TcpIpProxy.Networking
{
    class Server
    {
        private readonly StateReporter stateReporter;

        private TcpIpServer server;
        private CancellationTokenSource startCancellationTokenSource;
        private string lastRequestString;
        private IPEndPoint displayedLocalEndPoint;

        private bool automaticResponseEnabled;
        private bool useCommandsEnabled;
        private bool closeConnectionAfterRespondingEnabled;

        public Server()
        {
            stateReporter = new StateReporter(this);
        }

        public bool AutomaticResponseEnabled
        {
            get
            {
                return automaticResponseEnabled;
            }
            set
            {
                automaticResponseEnabled = value;

                if (automaticResponseEnabled && Connected)
                {
                    RespondAutomatically();
                }
            }
        }

        public string AutomaticResponseString
        {
            get;
            set;
        }

        public bool UseCommandsEnabled
        {
            get
            {
                return useCommandsEnabled;
            }
            set
            {
                useCommandsEnabled = value;

                if (useCommandsEnabled && automaticResponseEnabled && Connected)
                {
                    RespondAutomatically();
                }
            }
        }

        public IList CommandsList
        {
            get;
            set;
        }

        public bool CloseConnectionAfterRespondingEnabled
        {
            get
            {
                return closeConnectionAfterRespondingEnabled;
            }
            set
            {
                closeConnectionAfterRespondingEnabled = value;

                if (closeConnectionAfterRespondingEnabled && Connected)
                {
                    Respond(null);
                }
            }
        }

        public bool Running
        {
            get
            {
                if (server != null)
                {
                    return !server.Disposed;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool Connected
        {
            get
            {
                if (server != null)
                {
                    return server.Connected;
                }
                else
                {
                    return false;
                }
            }
        }

        public IPEndPoint RemoteEndPoint
        {
            get
            {
                if (Connected)
                {
                    return server.RemoteEndPoint;
                }
                else
                {
                    return null;
                }
            }
        }

        public IPEndPoint LocalEndPoint
        {
            get
            {
                if (Running)
                {
                    return displayedLocalEndPoint;
                }
                else
                {
                    return null;
                }
            }
        }

        public Task StartAsync(int port)
        {
            startCancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = startCancellationTokenSource.Token;
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);

            return Task.Run(() =>
            {
                try
                {
                    using (server = new TcpIpServer(localEndPoint, localEndPoint.AddressFamily, Preferences.ServerStringEncoding, Preferences.ReceiveTimeout, Preferences.SendTimeout, TcpIpServer.DEF_LISTENER_QUEUE_SIZE, TcpIpServer.DEF_RECEIVE_BUFFER_SIZE))
                    {
                        displayedLocalEndPoint = new IPEndPoint(TcpIp.Helper.GetHostIpAsync(Dns.GetHostName()).Result, port);
                        server.Listen();
                        stateReporter.Report("Server started at " + LocalEndPoint + ".");

                        while (!cancellationToken.IsCancellationRequested)
                        {
                            server.AcceptSocket();
                            stateReporter.Report("Connected to " + server.RemoteEndPoint + ".");

                            lastRequestString = string.Empty;

                            while (!cancellationToken.IsCancellationRequested)
                            {
                                try
                                {
                                    lastRequestString = server.Receive();
                                }
                                catch (SocketException e)
                                {
                                    stateReporter.Report("Socket Error " + e.ErrorCode + ":\n" + e.Message);
                                    break;
                                }

                                if (lastRequestString == null)
                                {
                                    stateReporter.Report("The socket at " + server.RemoteEndPoint + " was shut down.");
                                    break;
                                }
                                else
                                {
                                    stateReporter.Report("Request received from " + server.RemoteEndPoint + ":", lastRequestString, true);
                                }

                                if (AutomaticResponseEnabled)
                                {
                                    RespondAutomatically();
                                }
                            }

                            //if (server.Connected)
                            //{
                            EndPoint remoteEndPoint = server.RemoteEndPoint;
                            server.Disconnect();
                            stateReporter.Report("Disconnected from " + remoteEndPoint + ".");
                            //}
                        }
                    }
                }
                catch (SocketException e)
                {
                    // SocketError.Interrupted is thrown when Dispose() is called 
                    // while the TcpIpServer is waiting for a connection using AcceptSocket()
                    // See Stop() below
                    if (e.SocketErrorCode != SocketError.Interrupted)
                    {
                        stateReporter.Report("Socket Error " + e.SocketErrorCode + ":\n" + e.Message);
                    }
                }

                displayedLocalEndPoint = null;
                stateReporter.Report("Server stopped.");
            });
        }

        public void Respond(string responseString)
        {
            if (!string.IsNullOrEmpty(responseString))
            {
                if (Preferences.ReplaceHex)
                {
                    responseString = Utilities.ConvertAllHex(responseString, Preferences.ServerStringEncoding);
                }

                server.Send(responseString);
                stateReporter.Report("Response sent to " + server.RemoteEndPoint + ":", responseString, true);
            }

            if (CloseConnectionAfterRespondingEnabled)
            {
                server.Shutdown(false);
            }
        }

        private void RespondAutomatically()
        {
            string responseString = null;

            if (UseCommandsEnabled && !string.IsNullOrEmpty(lastRequestString))
            {
                foreach (Command c in CommandsList)
                {
                    if ((Preferences.ReplaceHex && lastRequestString == Utilities.ConvertAllHex(c.Request, Preferences.ServerStringEncoding))
                        || (!Preferences.ReplaceHex && lastRequestString == c.Request))
                    {
                        responseString = c.Response;
                        break;
                    }
                }
            }
            else
            {
                responseString = AutomaticResponseString;
            }

            Respond(responseString);
        }

        public void Stop()
        {
            stateReporter.Report("Stopping server...");

            // If the server is already connected,
            // Receive() can be stopped by shutting down the socket completely
            if (server.Connected)
            {
                startCancellationTokenSource.Cancel();
                stateReporter.Report("Shutting down connected socket...");
                server.Shutdown(true);
            }
            // If the server is waiting for a connection using AcceptSocket(),
            // the server has to be disposed in order to be stopped
            else
            {
                server.Dispose();
            }
        }
    }
}
