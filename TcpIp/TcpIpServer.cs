using System.Net;
using System.Net.Sockets;
using System.Text;
namespace TcpIp
{
    public class TcpIpServer : TcpIpSocket
    {
        public static readonly IPEndPoint DEF_LISTENER_END_POINT = new IPEndPoint(IPAddress.Any, 8000);
        public static readonly int DEF_LISTENER_QUEUE_SIZE = 0;

        private readonly IPEndPoint listenerEndPoint;
        private readonly int listenerQueueSize;

        private Socket connectedClientSocket;

        #region Properties

        /// <summary>
        /// Returns the EndPoint of the client socket; or null, if no client socket has been accepted yet.
        /// </summary>
        public override IPEndPoint RemoteEndPoint
        {
            get
            {
                if (connectedClientSocket == null)
                {
                    return null;
                }
                else
                {
                    return connectedClientSocket.RemoteEndPoint as IPEndPoint;
                }
            }
        }

        public override int AvailableBytes
        {
            get
            {
                if (connectedClientSocket == null)
                {
                    return -1;
                }
                else
                {
                    return connectedClientSocket.Available;
                }
            }
        }

        public override bool Connected
        {
            get
            {
                if (connectedClientSocket == null)
                {
                    return false;
                }
                else
                {
                    return connectedClientSocket.Connected;
                }
            }
        }

        #endregion Properties

        #region Constructors

        public TcpIpServer(IPEndPoint listenerEndPoint, AddressFamily addressFamily, Encoding stringEncoding, int receiveTimeout, int sendTimeout, int listenerQueueSize, int receiveBufferSize)
            : base(addressFamily, stringEncoding, receiveTimeout, sendTimeout, receiveBufferSize)
        {
            this.listenerEndPoint = listenerEndPoint;
            this.listenerQueueSize = listenerQueueSize;
        }

        public TcpIpServer(IPEndPoint listenerEndPoint, AddressFamily addressFamily, Encoding stringEncoding)
            : this(listenerEndPoint, addressFamily, stringEncoding, DEF_RECEIVE_TIMEOUT, DEF_SEND_TIMEOUT, DEF_LISTENER_QUEUE_SIZE, DEF_RECEIVE_BUFFER_SIZE)
        {
        }

        public TcpIpServer()
            : this(DEF_LISTENER_END_POINT, DEF_ADDRESS_FAMILY, DEF_STRING_ENCODING)
        {
        }

        #endregion Constructors

        /// <summary>
        /// Binds the socket to the listenerEndPoint and switches the socket into listening mode using the listenerQueueSize.
        /// </summary>
        public void Listen()
        {
            Socket.Bind(listenerEndPoint);
            Socket.Listen(listenerQueueSize);
        }

        /// <summary>
        /// Makes the listening socket accept the first incoming client socket and saves it.
        /// </summary>
        /// <remarks>
        /// <see cref="Listen()"/> has to be called first.
        /// This method blocks until a client connects.
        /// </remarks>
        public void AcceptSocket()
        {
            connectedClientSocket = Socket.Accept();
        }

        /// <summary>
        /// Receives all bytes from the last accepted client socket and returns those as a string using the stringEncoding.
        /// </summary>
        /// <returns>A <c>string</c> representing the received bytes; or <c>null</c>, if there is no data available and the client has shut down its socket</returns>
        /// <remarks>
        /// <see cref="AcceptSocket()"/> has to be called first.
        /// This method blocks until there is data to read or the client shuts down its socket or the receiveTimeout is exceeded.
        /// </remarks>
        public string Receive()
        {
            return base.Receive(connectedClientSocket);
        }

        /// <summary>
        /// Converts the specified string to bytes using the stringEncoding and sends those to the connected socket.
        /// </summary>
        /// <param name="responseString">The string to be converted and sent</param>
        /// <remarks>
        /// <see cref="AcceptSocket()"/> has to be called first.
        /// This method blocks until all bytes have been sent or the sendTimeout is exceeded.
        /// </remarks>
        public void Send(string responseString)
        {
            base.Send(connectedClientSocket, responseString);
        }

        /// <summary>
        /// Checks, if the socket is still connected to the client.
        /// </summary>
        /// <returns><c>true</c>, if the socket is connected; <c>false</c>, otherwise</returns>
        public bool isConnected()
        {
            return base.isConnected(connectedClientSocket);
        }

        /// <summary>
        /// Disables sending on the connected socket to signal, it's about to close.
        /// </summary>
        /// <param name="totalShutdown">If <c>true</c>, receiving will be disabled, too</param>
        /// <remarks>
        /// After this, <see cref="Receive()"/> should be called until it returns null to prevent data loss.
        /// </remarks>
        public void Shutdown(bool totalShutdown)
        {
            base.Shutdown(connectedClientSocket, totalShutdown);
        }

        /// <summary>
        /// Closes the connected client socket and frees its resources.
        /// </summary>
        /// <remarks>
        /// <see cref="AcceptSocket()"/> has to be called first.
        /// </remarks>
        public void Disconnect()
        {
            connectedClientSocket.Close();
        }

        /// <summary>
        /// Closes the server socket, as well as the connected client socket and frees resources.
        /// </summary>
        /// <param name="disposeManagedResources">Set to <c>true</c>, if associated managed resources should be disposed; otherwise, set to <c>false</c></param>
        protected override void Dispose(bool disposeManagedResources)
        {
            Disposed = true;

            if (disposeManagedResources)
            {
                Socket.Close();

                if (connectedClientSocket != null)
                {
                    Disconnect();
                }
            }
        }
    }
}