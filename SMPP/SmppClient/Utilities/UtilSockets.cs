#region Namespaces

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

#endregion

namespace ArdanStudios.Common.Utilities
{
    #region Class - SocketClient

    /// <summary> This class abstract a socket </summary>
    public class SocketClient : IDisposable
    {
        #region Delegate Function Types

        /// <summary> Called when a message is extracted from the socket </summary>
        public delegate void MESSAGE_HANDLER(SocketClient socket);

        /// <summary> Called when a socket connection is closed </summary>
        public delegate void CLOSE_HANDLER(SocketClient socket);

        /// <summary> Called when a socket error occurs </summary>
        public delegate void ERROR_HANDLER(SocketClient socket, Exception exception);

        #endregion

        #region Static Properties

        /// <summary> Maintain the next unique key </summary>
        private static long NextUniqueKey = 0;

        #endregion

        #region Private Properties

        /// <summary> Flag when disposed is called </summary>
        private bool Disposed = false;
        
        /// <summary> The SocketServer for this socket object </summary>
        private SocketServer SocketServer = null;

        /// <summary> The socket for the accepted client connection </summary>
        private Socket ClientSocket = null;

        /// <summary> A TcpClient object for client established connections </summary>
        private TcpClient TcpClient = null;

        /// <summary> A network stream object </summary>
        private NetworkStream NetworkStream = null;

        /// <summary> RetType: A callback object for processing recieved socket data </summary>	    
        private AsyncCallback CallbackReadFunction = null;

        /// <summary> RetType: A callback object for processing send socket data </summary>
        private AsyncCallback CallbackWriteFunction = null;

        /// <summary> A reference to a user supplied function to be called when a socket message arrives </summary>
        private MESSAGE_HANDLER MessageHandler = null;

        /// <summary> A reference to a user supplied function to be called when a socket connection is closed </summary>
        private CLOSE_HANDLER CloseHandler = null;

        /// <summary> A reference to a user supplied function to be called when a socket error occurs </summary>
        private ERROR_HANDLER ErrorHandler = null;

        #endregion

        #region Public Properties

        /// <summary> The IpAddress of the connection </summary>
        public string IpAddress = null;

        /// <summary> The Port of the connection </summary>summary>
        public int Port = int.MinValue;

        /// <summary> The index position in the server dictionary of socket connections </summary>
        public int SocketIndex = -1;

        /// <summary> A raw buffer to capture data comming off the socket </summary>
        public byte[] RawBuffer = null;

        /// <summary> Size of the raw buffer for received socket data </summary>
        public int SizeOfRawBuffer = 0;

        /// <summary> The length of the message </summary>
        public int MessageLength = 0;

        /// <summary> A unique key for the socket object </summary>
        public long UniqueKey = 0;

        /// <summary> A flag to determine if the Socket Client is connected </summary>
        public bool IsConnected { get { return (ClientSocket != null) ? ClientSocket.Connected : (TcpClient != null) ? TcpClient.Connected : false; } }

        #endregion

        #region User Defined Public Properties

        /// <summary> A string buffer to be used by the application developer </summary>
        public StringBuilder StringBuffer = null;

        /// <summary> A memory stream buffer to be used by the application developer </summary>
        public MemoryStream MessageBuffer = null;

        /// <summary> A byte buffer to be used by the application developer </summary>
        public byte[] ByteBuffer = null;

        /// <summary> A list buffer to be used by the application developer </summary>
        public List<byte> ListBuffer = null;

        /// <summary> The number of bytes that have been buffered </summary>
        public int BufferedBytes = 0;

        /// <summary> A reference to a user defined object to be passed through the handler functions </summary>
        public object UserArg = null;

        /// <summary> UserDefined flag to indicate if the socket object is available for use </summary>
        public bool IsAvailable = false;

        #endregion

        #region Constructor

        /// <summary> Constructor for client support </summary>
        /// <param name="sizeOfRawBuffer"> The size of the raw buffer </param>
        /// <param name="sizeOfByteBuffer"> The size of the byte buffer </param>
        /// <param name="userArg"> A Reference to the Users arguments </param>
        /// <param name="messageHandler"> Reference to the user defined message handler function </param>
        /// <param name="closeHandler"> Reference to the user defined close handler function </param>
        /// <param name="errorHandler"> Reference to the user defined error handler function </param>
        public SocketClient(int sizeOfRawBuffer, int sizeOfByteBuffer, object userArg,
                            MESSAGE_HANDLER messageHandler, CLOSE_HANDLER closeHandler, ERROR_HANDLER errorHandler)
        {
            // Create the raw buffer
            SizeOfRawBuffer = sizeOfRawBuffer;
            RawBuffer = new byte[SizeOfRawBuffer];

            // Save the user argument
            UserArg = userArg;

            // Allocate a String Builder class for Application developer use
            StringBuffer = new StringBuilder();

            // Allocate a Memory Stream class for Application developer use
            MessageBuffer = new MemoryStream();

            // Allocate a byte buffer for Application developer use
            ByteBuffer = new byte[sizeOfByteBuffer];
            BufferedBytes = 0;

            // Allocate a list buffer for Application developer use
            ListBuffer = new List<byte>();

            // Set the handler functions
            MessageHandler = messageHandler;
            CloseHandler = closeHandler;
            ErrorHandler = errorHandler;

            // Set the async socket function handlers
            CallbackReadFunction = new AsyncCallback(ReceiveComplete);
            CallbackWriteFunction = new AsyncCallback(SendComplete);

            // Set available flags
            IsAvailable = true;

            // Set the unique key for this object
            UniqueKey = NewUniqueKey();
        }

        /// <summary> Constructor for SocketServer Suppport </summary>
        /// <param name="socketServer"> A Reference to the parent SocketServer </param>
        /// <param name="clientSocket"> The Socket object we are encapsulating </param>
        /// <param name="ipAddress"> The IpAddress of the remote server </param>
        /// <param name="port"> The Port of the remote server </param>
        /// <param name="sizeOfRawBuffer"> The size of the raw buffer </param>
        /// <param name="sizeOfByteBuffer"> The size of the byte buffer </param>
        /// <param name="userArg"> A Reference to the Users arguments </param>
        /// <param name="messageHandler"> Reference to the user defined message handler function </param>
        /// <param name="closeHandler"> Reference to the user defined close handler function </param>
        /// <param name="errorHandler"> Reference to the user defined error handler function </param>
        public SocketClient(SocketServer socketServer, Socket clientSocket, string ipAddress, int port,
                            int sizeOfRawBuffer, int sizeOfByteBuffer, object userArg,
                            MESSAGE_HANDLER messageHandler, CLOSE_HANDLER closeHandler, ERROR_HANDLER errorHandler)
        {
            // Set reference to SocketServer
            SocketServer = socketServer;

            // Set when this socket came from a SocketServer Accept
            ClientSocket = clientSocket;
            
            // Set the Ipaddress and Port
            IpAddress = ipAddress;
            Port = port;

            // Set the server index
            SocketIndex = clientSocket.Handle.ToInt32();

            // Set the handler functions
            MessageHandler = messageHandler;
            CloseHandler = closeHandler;
            ErrorHandler = errorHandler;

            // Create the raw buffer
            SizeOfRawBuffer = sizeOfRawBuffer;
            RawBuffer = new byte[SizeOfRawBuffer];

            // Save the user argument
            UserArg = userArg;

            // Allocate a String Builder class for Application developer use
            StringBuffer = new StringBuilder();

            // Allocate a Memory Stream class for Application developer use
            MessageBuffer = new MemoryStream();

            // Allocate a byte buffer for Application developer use
            ByteBuffer = new byte[sizeOfByteBuffer];
            BufferedBytes = 0;

            // Allocate a list buffer for Application developer use
            ListBuffer = new List<byte>();

            // Init the NetworkStream reference
            NetworkStream = new NetworkStream(ClientSocket);

            // Set the async socket function handlers
            CallbackReadFunction = new AsyncCallback(ReceiveComplete);
            CallbackWriteFunction = new AsyncCallback(SendComplete);

            // Set Available flags
            IsAvailable = true;

            // Set the unique key for this object
            UniqueKey = NewUniqueKey();

            // Set these socket options
            ClientSocket.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.ReceiveBuffer, sizeOfRawBuffer);
            ClientSocket.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.SendBuffer, sizeOfRawBuffer);
            ClientSocket.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.KeepAlive, 1);
            ClientSocket.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.DontLinger, 1);
            ClientSocket.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Tcp, System.Net.Sockets.SocketOptionName.NoDelay, 1);
        }

        /// <summary> Dispose </summary>
        public void Dispose()
        {
            try
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            catch
            {
            }
        }
        
        /// <summary> Dispose the server </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!Disposed)
            {
                // Note disposing has been done.
                Disposed = true;

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    try
                    {
                        Disconnect();
                    }

                    catch
                    {
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary> Called when a message arrives </summary>
        /// <param name="ar"> An async result interface </param>
        private void ReceiveComplete(IAsyncResult ar)
        {
            try
            {
                if (Thread.CurrentThread.Name == null)
                {
                    Thread.CurrentThread.Name = "NetThreadPool";
                }

                // Is the Network Stream object valid
                if ((NetworkStream != null) && (NetworkStream.CanRead))
                {
                    // Read the current bytes from the stream buffer
                    MessageLength = NetworkStream.EndRead(ar);

                    // If there are bytes to process else the connection is lost
                    if (MessageLength > 0)
                    {
                        try
                        {
                            // A message came in send it to the MessageHandler
                            MessageHandler(this);
                        }

                        catch
                        {
                        }

                        // Wait for a new message
                        Receive();
                    }
                    else
                    {
                        if (NetworkStream != null)
                        {
                            Disconnect();
                        }
                        
                        // Call the close handler
                        CloseHandler(this);
                    }
                }
                else
                {
                    if (NetworkStream != null)
                    {
                        Disconnect();
                    }
                    
                    // Call the close handler
                    CloseHandler(this);
                }
            }

            catch (Exception exception)
            {
                if (NetworkStream != null)
                {
                    Disconnect();
                    
                    if ((!exception.Message.Contains("forcibly closed")) &&
                        (!exception.Message.Contains("thread exit")))
                    {
                        ErrorHandler(this, exception);
                    }
                }
                
                // Call the close handler
                CloseHandler(this);
            }

            ar.AsyncWaitHandle.Close();
        }

        /// <summary> Called when a message is sent </summary>
        /// <param name="ar"> An async result interface </param>
        private void SendComplete(IAsyncResult ar)
        {
            try
            {
                if (Thread.CurrentThread.Name == null)
                {
                    Thread.CurrentThread.Name = "NetThreadPool";
                }

                // Is the Network Stream object valid
                if ((NetworkStream != null) && (NetworkStream.CanWrite))
                {
                    NetworkStream.EndWrite(ar);
                }
            }

            catch
            {
            }

            ar.AsyncWaitHandle.Close();
        }

        #endregion

        #region Public Methods

        /// <summary> Called to generate a unique key </summary>
        /// <returns> long </returns>
        public static long NewUniqueKey()
        {
            // Set the unique key for this object
            return Interlocked.Increment(ref SocketClient.NextUniqueKey);
        }

        /// <summary> Function used to connect to a server </summary>
        /// <param name="ipAddress"> The address to connect to </param>
        /// <param name="port"> The Port to connect to </param>
        public void Connect(string ipAddress, int port)
        {
            // If this object was disposed and they are trying to re-connect clear the flag
            if (Disposed == true)
            {
                throw new Exception("ClientSocket Has Been Disposed");
            }

            if (NetworkStream == null)
            {
                // Set the Ipaddress and Port
                IpAddress = ipAddress;
                Port = port;
                    
                try
                {
                    IPAddress useIpAddress = null;
                    IPHostEntry hostEntries = Dns.GetHostEntry(IpAddress);
                    foreach (IPAddress address in hostEntries.AddressList)
                    {
                        // Find the IPv4 address first
                        if (address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            useIpAddress = address;
                            break;
                        }
                    }
                        
                    // Now just use the first address
                    if (useIpAddress == null)
                    {
                        useIpAddress = hostEntries.AddressList[0];
                    }
                    
                    IpAddress = useIpAddress.ToString();
                }

                catch
                {
                    IpAddress = ipAddress;
                }
                    
                // Attempt to establish a connection
                TcpClient = new TcpClient(IpAddress, Port);
                NetworkStream = TcpClient.GetStream();

                // Set these socket options
                TcpClient.ReceiveBufferSize = SizeOfRawBuffer;
                TcpClient.SendBufferSize = SizeOfRawBuffer;
                TcpClient.NoDelay = true;
                TcpClient.LingerState = new System.Net.Sockets.LingerOption(false, 0);

                // Start to receive messages
                Receive();
            }
        }

        /// <summary> Called to disconnect the client </summary>
        public void Disconnect()
        {
            try
            {
                // Remove the socket from the list
                if (SocketServer != null)
                {
                    SocketServer.RemoveSocket(this);
                }

                // Set when this socket came from a SocketServer Accept
                if (ClientSocket != null)
                {
                    ClientSocket.Close();
                }

                // Set when this socket came from a SocketClient Connect
                if (TcpClient != null)
                {
                    TcpClient.Close();
                }
                   
                // Set it both cases
                if (NetworkStream != null)
                {
                    NetworkStream.Close();
                }

                // Clean up the connection state
                ClientSocket = null;
                TcpClient = null;
                NetworkStream = null;
            }

            catch (Exception exception)
            {
                ErrorHandler(this, exception);
            }
        }

        /// <summary> Function to send a string to the server </summary>
        /// <param name="message"> A string to send </param>
        public void Send(string message)
        {
            try
            {
                if ((NetworkStream != null) && (NetworkStream.CanWrite))
                {
                    // Convert the string into a Raw Buffer
                    byte[] pRawBuffer = System.Text.Encoding.ASCII.GetBytes(message);

                    // Issue an asynchronus write
                    NetworkStream.BeginWrite(pRawBuffer, 0, pRawBuffer.Length, CallbackWriteFunction, null);
                }
                else
                {
                    throw new Exception("No Connection");
                }
            }
            
            catch
            {
                Disconnect();
                
                throw;
            }
        }

        /// <summary> Function to send a raw buffer to the server </summary>
        /// <param name="rawBuffer"> A Raw buffer of bytes to send </param>
        public void Send(byte[] rawBuffer)
        {
            try
            {
                if ((NetworkStream != null) && (NetworkStream.CanWrite))
                {
                    // Issue an asynchronus write
                    NetworkStream.BeginWrite(rawBuffer, 0, rawBuffer.Length, CallbackWriteFunction, null);
                }
                else
                {
                    throw new Exception("No Connection");
                }
            }
            
            catch
            {
                Disconnect();
                
                throw;
            }
        }

        /// <summary> Function to send a char to the server </summary>
        /// <param name="charValue"> A Raw char to send </param>
        public void Send(char charValue)
        {
            try
            {
                if ((NetworkStream != null) && (NetworkStream.CanWrite))
                {
                    // Convert the character to a byte
                    byte[] pRawBuffer = { Convert.ToByte(charValue) };

                    // Issue an asynchronus write
                    NetworkStream.BeginWrite(pRawBuffer, 0, pRawBuffer.Length, CallbackWriteFunction, null);
                }
                else
                {
                    throw new Exception("No Connection");
                }
            }
            
            catch
            {
                Disconnect();
                
                throw;
            }
        }

        /// <summary> Wait for a message to arrive </summary>
        public void Receive()
        {
            if ((NetworkStream != null) && (NetworkStream.CanRead))
            {
                // Issue an asynchronous read
                NetworkStream.BeginRead(RawBuffer, 0, SizeOfRawBuffer, CallbackReadFunction, null);
            }
            else
            {
                throw new Exception("Unable To Read From Stream");
            }
        }

        #endregion
    }

    #endregion

    #region Class - SocketServer

    /// <summary> This class accepts multiple socket connections and handles them asychronously </summary>
    public class SocketServer : IDisposable
    {
        #region Delagate Function Types

        /// <summary> Called when a message is extracted from the socket </summary>
        public delegate void MESSAGE_HANDLER(SocketClient socket);

        /// <summary> Called when a socket connection is closed </summary>
        public delegate void CLOSE_HANDLER(SocketClient socket);

        /// <summary> Called when a socket error occurs </summary>
        public delegate void ERROR_HANDLER(SocketClient socket, Exception exception);

        /// <summary> Called when a socket connection is accepted </summary>
        public delegate void ACCEPT_HANDLER(SocketClient socket);

        #endregion

        #region Private Properties

        /// <summary> Flag when disposed is called </summary>
        private bool Disposed = false;
        
        /// <summary> A TcpListener object to accept socket connections </summary>
        private TcpListener TcpListener = null;

        /// <summary> Size of the raw buffer for received socket data </summary>
        private int SizeOfRawBuffer = 0;

        /// <summary> Size of the raw buffer for user purpose </summary>
        private int SizeOfByteBuffer = 0;

        /// <summary> RetType: A thread to process accepting socket connections </summary>
        private Thread AcceptThread = null;

        /// <summary> A reference to a user supplied function to be called when a socket message arrives </summary>
        private MESSAGE_HANDLER MessageHandler = null;

        /// <summary> A reference to a user supplied function to be called when a socket connection is closed </summary>
        private CLOSE_HANDLER CloseHandler = null;

        /// <summary> A reference to a user supplied function to be called when a socket error occurs </summary>
        private ERROR_HANDLER ErrorHandler = null;

        /// <summary> A reference to a user supplied function to be called when a socket connection is accepted </summary>
        private ACCEPT_HANDLER AcceptHandler = null;

        /// <summary> RefTypeArray: An Array of SocketClient objects </summary>
        private List<SocketClient> SocketClientList = new List<SocketClient>();

        #endregion

        #region Public Properties

        /// <summary> The IpAddress to either connect to or listen on </summary>
        public string IpAddress = null;

        /// <summary> The Port to either connect to or listen on </summary>
        public int Port = int.MinValue;

        /// <summary> A reference to a user defined object to be passed through the handler functions </summary>
        public object UserArg = null;

        #endregion

        #region Constructor

        /// <summary> Constructor </summary>
        public SocketServer()
        {
        }

        /// <summary> Dispose function to shutdown the SocketManager </summary>
        public void Dispose()
        {
            try
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            catch
            {
            }
        }
        
        /// <summary> Dispose the server </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!Disposed)
            {
                // Note disposing has been done.
                Disposed = true;

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Stop the server if the thread is running
                    if (AcceptThread != null)
                    {
                        Stop();
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary> Function to process and accept socket connection requests </summary>
        private void AcceptConnections()
        {
            Socket socket = null;

            try
            {
                IPAddress useIpAddress = null;
                IPHostEntry hostEntries = Dns.GetHostEntry(IpAddress);
                foreach (IPAddress address in hostEntries.AddressList)
                {
                    // Find the IPv4 address first
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        useIpAddress = address;
                        break;
                    }
                }
                    
                // Now just use the first address
                if (useIpAddress == null)
                {
                    useIpAddress = hostEntries.AddressList[0];
                }
                
                IpAddress = useIpAddress.ToString();
                    
                // Create a new TCPListner and start it up
                TcpListener = new TcpListener(useIpAddress, Port);
                TcpListener.Start();

                for (;;)
                {
                    try
                    {
                        // If a client connects, accept the connection.
                        socket = TcpListener.AcceptSocket();
                    }

                    catch (System.Net.Sockets.SocketException e)
                    {
                        // Did we stop the TCPListener
                        if (e.ErrorCode != 10004)
                        {
                            // Call the error handler
                            ErrorHandler(null, e);
                            ErrorHandler(null, new Exception("Waiting for new connection 1"));

                            // Close the socket down if it exists
                            if (socket != null)
                            {
                                if (socket.Connected)
                                {
                                    socket.Dispose();
                                }
                            }
                        }
                        else
                        {
                            ErrorHandler(null, new Exception("Shutting Down Accept Thread"));
                            break;
                        }
                    }

                    catch (Exception e)
                    {
                        // Call the error handler
                        ErrorHandler(null, e);
                        ErrorHandler(null, new Exception("Waiting for new connection 2"));

                        // Close the socket down if it exists
                        if (socket != null)
                        {
                            if (socket.Connected)
                            {
                                socket.Dispose();
                            }
                        }
                    }

                    try
                    {
                        if (socket.Connected)
                        {
                            string remoteEndPoint = socket.RemoteEndPoint.ToString();

                            // Create a SocketClient object
                            SocketClient clientSocket = new SocketClient(this,
                                                                         socket,
                                                                         (remoteEndPoint.Length < 15) ? string.Empty : remoteEndPoint.Substring(0, 15),
                                                                         Port,
                                                                         SizeOfRawBuffer,
                                                                         SizeOfByteBuffer,
                                                                         UserArg,
                                                                         new SocketClient.MESSAGE_HANDLER(MessageHandler),
                                                                         new SocketClient.CLOSE_HANDLER(CloseHandler),
                                                                         new SocketClient.ERROR_HANDLER(ErrorHandler));
                            // Add it to the list
                            lock (SocketClientList)
                            {
                                SocketClientList.Add(clientSocket);
                            }

                            // Call the Accept Handler
                            AcceptHandler(clientSocket);

                            // Wait for a message
                            clientSocket.Receive();
                        }
                    }

                    catch (Exception e)
                    {
                        // Call the error handler
                        ErrorHandler(null, e);
                        ErrorHandler(null, new Exception("Waiting for new connection 3"));
                    }
                }
            }

            catch (Exception e)
            {
                // Call the error handler
                ErrorHandler(null, e);
                ErrorHandler(null, new Exception("Shutting Down Accept Thread"));

                // Close the socket down if it exists
                if (socket != null)
                {
                    if (socket.Connected)
                    {
                        socket.Dispose();
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary> Function to start the SocketServer </summary>
        /// <param name="ipAddress"> The IpAddress to listening on </param>
        /// <param name="port"> The Port to listen on </param>
        /// <param name="sizeOfRawBuffer"> Size of the Raw Buffer </param>
        /// <param name="sizeOfByteBuffer"> Size of the byte buffer </param>
        /// <param name="userArg"> User supplied arguments </param>
        /// <param name="messageHandler"> Function pointer to the user MessageHandler function </param>
        /// <param name="acceptHandler"> Function pointer to the user AcceptHandler function </param>
        /// <param name="closeHandler"> Function pointer to the user CloseHandler function </param>
        /// <param name="errorHandler"> Function pointer to the user ErrorHandler function </param>
        public void Start(string ipAddress, int port, int sizeOfRawBuffer, int sizeOfByteBuffer, object userArg,
                          MESSAGE_HANDLER messageHandler, ACCEPT_HANDLER acceptHandler, CLOSE_HANDLER closeHandler,
                          ERROR_HANDLER errorHandler)
        {
            // Is an AcceptThread currently running
            if (AcceptThread == null)
            {
                // Set connection values
                IpAddress = ipAddress;
                Port = port;

                // Save the Handler Functions
                MessageHandler = messageHandler;
                AcceptHandler = acceptHandler;
                CloseHandler = closeHandler;
                ErrorHandler = errorHandler;

                // Save the buffer size and user arguments
                SizeOfRawBuffer = sizeOfRawBuffer;
                SizeOfByteBuffer = sizeOfByteBuffer;
                UserArg = userArg;

                // Start the listening thread if one is currently not running
                ThreadStart tsThread = new ThreadStart(AcceptConnections);
                AcceptThread = new Thread(tsThread);
                AcceptThread.Name = string.Format("SocketAccept-{0}", ipAddress);
                AcceptThread.Start();
            }
        }

        /// <summary> Function to stop the SocketServer.  It can be restarted with Start </summary>
        public void Stop()
        {
            // Abort the accept thread
            if (AcceptThread != null)
            {
                TcpListener.Stop();
                AcceptThread.Join();
                AcceptThread = null;
            }

            lock (SocketClientList)
            {
                // Dispose of all of the socket connections
                foreach (SocketClient socketClient in SocketClientList)
                {
                    socketClient.Dispose();
                }
            }

            // Wait for all of the socket client objects to be destroyed
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Empty the Socket Client List
            SocketClientList.Clear();

            // Clear the Handler Functions
            MessageHandler = null;
            AcceptHandler = null;
            CloseHandler = null;
            ErrorHandler = null;

            // Clear the buffer size and user arguments
            SizeOfRawBuffer = 0;
            UserArg = null;
        }

        /// <summary> Funciton to remove a socket from the list of sockets </summary>
        /// <param name="socketClient"> A reference to a socket to remove </param>
        public void RemoveSocket(SocketClient socketClient)
        {
            try
            {
                lock (SocketClientList)
                {
                    // Remove ths client socket object from the list
                    SocketClientList.Remove(socketClient);
                }
            }

            catch (Exception exception)
            {
                ErrorHandler(socketClient, exception);
            }
        }

        /// <summary> Called to retrieve the socket object by the Socket Index </summary>
        /// <param name="socketIndex"></param>
        public SocketClient RetrieveSocket(Int32 socketIndex)
        {
            SocketClient socketClient = null;

            try
            {
                lock (SocketClientList)
                {
                    // If the server index exists, return it
                    socketClient = SocketClientList.FirstOrDefault(k => k.SocketIndex == socketIndex);
                }
            }

            catch (Exception)
            {
            }

            return socketClient;
        }

        /// <summary> Called to send a message to call socket clients </summary>
        /// <param name="rawBuffer"></param>
        public void SendAll(Byte[] rawBuffer)
        {
            lock (SocketClientList)
            {
                // If the server index exists, return it
                foreach (SocketClient socketClient in SocketClientList)
                {
                    socketClient.Send(rawBuffer);
                }
            }
        }
        
        /// <summary> Called to send a message to call socket clients </summary>
        /// <param name="message"></param>
        public void SendAll(string message)
        {
            lock (SocketClientList)
            {
                // If the server index exists, return it
                foreach (SocketClient socketClient in SocketClientList)
                {
                    socketClient.Send(message);
                }
            }
        }
        
        #endregion
    }

    #endregion
}
