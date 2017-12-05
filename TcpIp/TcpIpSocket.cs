using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace TcpIp
{
    public abstract class TcpIpSocket : IDisposable
    {
        public static readonly AddressFamily DEF_ADDRESS_FAMILY = AddressFamily.InterNetwork;
        public static readonly Encoding DEF_STRING_ENCODING = Encoding.Default;
        public static readonly int DEF_RECEIVE_TIMEOUT = 30000;
        public static readonly int DEF_SEND_TIMEOUT = 15000;
        public static readonly int DEF_RECEIVE_BUFFER_SIZE = 1024;

        private readonly Encoding stringEncoding;
        private readonly byte[] bytesReceived;

        #region Properties

        public virtual IPEndPoint LocalEndPoint
        {
            get
            {
                return Socket.LocalEndPoint as IPEndPoint;
            }
        }

        public virtual IPEndPoint RemoteEndPoint
        {
            get
            {
                return Socket.RemoteEndPoint as IPEndPoint;
            }
        }

        public virtual int AvailableBytes
        {
            get
            {
                return Socket.Available;
            }
        }

        public virtual bool Connected
        {
            get
            {
                return Socket.Connected;
            }
        }

        public bool Disposed
        {
            get;
            protected set;
        }

        protected Socket Socket
        {
            get;
            private set;
        }

        #endregion Properties

        public TcpIpSocket(AddressFamily addressFamily, Encoding stringEncoding, int receiveTimeout, int sendTimeout, int receiveBufferSize)
        {
            Socket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.stringEncoding = stringEncoding;
            Socket.ReceiveTimeout = receiveTimeout;
            Socket.SendTimeout = sendTimeout;
            this.bytesReceived = new byte[receiveBufferSize];
            Disposed = false;
        }

        /// <summary>
        /// Destructor / Finalize-method
        /// </summary>
        ~TcpIpSocket()
        {
            Dispose(false);
        }

        /// <summary>
        /// Receives all bytes from the specified client socket and returns those as a string using the stringEncoding.
        /// </summary>
        /// <param name="clientSocket">The socket, the bytes should be received from</param>
        /// <returns>A <c>string</c> representing the received bytes; or <c>null</c>, if there is no data available and the client has shut down its socket</returns>
        /// <remarks>
        /// This method blocks until there is data to read or the client shuts down its socket or the receiveTimeout is exceeded.
        /// </remarks>
        protected string Receive(Socket clientSocket)
        {
            int bytesReceivedCount = 0;
            StringBuilder requestStringBuilder = new StringBuilder();

            do
            {
                bytesReceivedCount = clientSocket.Receive(bytesReceived);

                // If Receive() returns 0, the opposing socket was shut down
                if (bytesReceivedCount == 0)
                {
                    return null;
                }

                requestStringBuilder.Append(stringEncoding.GetString(bytesReceived, 0, bytesReceivedCount));
            }
            while (clientSocket.Available != 0);

            return requestStringBuilder.ToString();
        }

        /// <summary>
        /// Converts the specified string to bytes using the stringEncoding and sends those to the specified client socket.
        /// </summary>
        /// <param name="clientSocket">The socket, to which the bytes should be sent</param>
        /// <param name="messageString">The string to be converted and sent</param>
        /// <remarks>
        /// This method blocks until all bytes are sent or the sendTimeout is exceeded.
        /// </remarks>
        protected void Send(Socket clientSocket, string messageString)
        {
            clientSocket.Send(stringEncoding.GetBytes(messageString));
        }

        /// <summary>
        /// Checks, if the socket is still connected to it's counterpart.
        /// </summary>
        /// <param name="socket">The socket to be checked</param>
        /// <returns><c>true</c>, if the socket is connected; <c>false</c>, otherwise</returns>
        protected bool isConnected(Socket socket)
        {
            // Poll returns true, if
            // connection is closed, reset, terminated or pending
            // connection is active and there is data available for reading
            bool pollSuccessful = socket.Poll(socket.SendTimeout, SelectMode.SelectRead);
            bool dataAvailable = socket.Available > 0;

            return (!pollSuccessful || dataAvailable) && socket.Connected;
        }

        /// <summary>
        /// Disables sending on this socket to signal to the opposing socket, it's about to close.
        /// </summary>
        /// <param name="clientSocket">The socket to be shut down</param>
        /// <param name="totalShutdown">If <c>true</c>, receiving will be disabled, too</param>
        /// <remarks>
        /// After this, <see cref="Receive()"/> should be called until it returns null to prevent data loss.
        /// </remarks>
        protected void Shutdown(Socket clientSocket, bool totalShutdown)
        {
            if (totalShutdown)
            {
                clientSocket.Shutdown(SocketShutdown.Both);
            }
            else
            {
                clientSocket.Shutdown(SocketShutdown.Send);
            }
        }

        /// <summary>
        /// Frees all resources associated with the socket.
        /// </summary>
        /// <remarks>
        /// <see cref="Shutdown()"/> and <see cref="Receive()"/> should be called first to prevent data loss.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Frees all resources associated with the socket.
        /// </summary>
        /// <param name="disposeManagedResources">Set to <c>true</c>, if associated managed resources should be disposed; otherwise, set to <c>false</c></param>
        protected abstract void Dispose(bool disposeManagedResources);
    }
}
