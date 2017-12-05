using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace TcpIp
{
    public class TcpIpClient : TcpIpSocket
    {
        #region Constructors

        public TcpIpClient(AddressFamily addressFamily, Encoding stringEncoding, int receiveTimeout, int sendTimeout, int receiveBufferSize)
            : base(addressFamily, stringEncoding, receiveTimeout, sendTimeout, receiveBufferSize)
        {
        }

        public TcpIpClient(AddressFamily addressFamily, Encoding stringEncoding)
            : this(addressFamily, stringEncoding, DEF_RECEIVE_TIMEOUT, DEF_SEND_TIMEOUT, DEF_RECEIVE_BUFFER_SIZE)
        {
        }

        public TcpIpClient()
            : this(DEF_ADDRESS_FAMILY, DEF_STRING_ENCODING)
        {
        }

        #endregion Constructors

        /// <summary>
        /// Establishes an asynchronous connection to a server socket at the specified IPEndPoint, so that messages can be transferred.
        /// </summary>
        /// <param name="remoteEndpoint">The IPEndPoint to connect to</param>
        /// </returns>
        public void Connect(IPEndPoint remoteEndpoint)
        {
            IAsyncResult connected = Socket.BeginConnect(remoteEndpoint, null, null);

            if (connected.AsyncWaitHandle.WaitOne(Socket.SendTimeout, true))
            {
                Socket.EndConnect(connected);
            }
            else
            {
                // Throw timeout exception
                throw new SocketException(10060);
            }
        }

        /// <summary>
        /// Converts the specified string to bytes using the stringEncoding and sends those to the connected server socket.
        /// </summary>
        /// <param name="requestString">The string to be converted and sent</param>
        /// <remarks>
        /// <see cref="Connect()"/> has to be called first.
        /// This method blocks until all bytes are sent or the sendTimeout is exceeded.
        /// </remarks>
        public void Send(string requestString)
        {
            base.Send(Socket, requestString);
        }

        /// <summary>
        /// Receives all bytes from the connected server socket and returns those as a string using the stringEncoding.
        /// </summary>
        /// <returns>A <c>string</c> representing the received bytes; or <c>null</c>, if there is no data available and the server has shut down its socket</returns>
        /// <remarks>
        /// <see cref="Connect()"/> has to be called first.
        /// This method blocks until there is data to read or the server shuts down its socket or the receiveTimeout is exceeded.
        /// </remarks>
        public string Receive()
        {
            return base.Receive(Socket);
        }

        /// <summary>
        /// Checks, if the socket is still connected to the server.
        /// </summary>
        /// <returns><c>true</c>, if the socket is connected; <c>false</c>, otherwise</returns>
        public bool isConnected()
        {
            return base.isConnected(Socket);
        }

        /// <summary>
        /// Disables sending on the socket to signal, it's about to close.
        /// </summary>
        /// <param name="totalShutdown">If <c>true</c>, receiving will be disabled, too</param>
        /// <remarks>
        /// After this, <see cref="Receive()"/> should be called until it returns null to prevent data loss.
        /// </remarks>
        public void Shutdown(bool totalShutdown)
        {
            base.Shutdown(Socket, totalShutdown);
        }

        /// <summary>
        /// Closes the socket and frees its resources.
        /// </summary>
        /// <param name="disposeManagedResources">Set to <c>true</c>, if associated managed resources should be disposed; otherwise, set to <c>false</c></param>
        protected override void Dispose(bool disposeManagedResources)
        {
            Disposed = true;

            if (disposeManagedResources)
            {
                Socket.Close();
            }
        }
    }
}
