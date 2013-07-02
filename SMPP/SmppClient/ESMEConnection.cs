#region Namespaces

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> Manages a single ESME (Extended Short Message Entity) connection </summary>
    internal class ESMEConnection : IDisposable
    {
        #region Delegates

        /// <summary> Called when a message is received </summary>
        /// <param name="logKey"></param>
        /// <param name="serviceType"></param>
        /// <param name="sourceTon"></param>
        /// <param name="sourceNpi"></param>
        /// <param name="shortLongCode"></param>
        /// <param name="dateReceived"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="dataCoding"></param>
        /// <param name="message"></param>
        public delegate void RECEIVED_MESSAGE_HANDLER(string logKey, string serviceType, Ton sourceTon, Npi sourceNpi, string shortLongCode, DateTime dateReceived, string phoneNumber, DataCodings dataCoding, string message);

        /// <summary> Called when a submit message is acknowledged </summary>
        /// <param name="logKey"></param>
        /// <param name="sequence"></param>
        public delegate void RECEIVED_GENERICNACK_HANDLER(string logKey, int sequence);

        /// <summary> Called when a submit message is acknowledged </summary>
        /// <param name="logKey"></param>
        /// <param name="sequence"></param>
        /// <param name="messageId"></param>
        public delegate void SUBMIT_MESSAGE_HANDLER(string logKey, int sequence, string messageId);

        /// <summary> Called when a query message is responded </summary>
        /// <param name="logKey"></param>
        /// <param name="sequence"></param>
        /// <param name="messageId"></param>
        /// <param name="finalDate"></param>
        /// <param name="messageState"></param>
        /// <param name="errorCode"></param>
        public delegate void QUERY_MESSAGE_HANDLER(string logKey, int sequence, string messageId, DateTime finalDate, int messageState, long errorCode);

        /// <summary> Called to log an event </summary>
        /// <param name="logEventNotificationType"></param>
        /// <param name="shortLongCode"></param>
        /// <param name="message"></param>
        /// <param name="logKey"></param>
        public delegate void LOG_EVENT_HANDLER(LogEventNotificationTypes logEventNotificationType, string logKey, string shortLongCode, string message);

        /// <summary> Called when a connection event occurrs </summary>
        /// <param name="logKey"></param>
        /// <param name="connectionEventType"></param>
        /// <param name="message"></param>
        public delegate void CONNECTION_EVENT_HANDLER(string logKey, ConnectionEventTypes connectionEventType, string message);

        /// <summary> Called to capture the details of the pdu </summary>
        /// <param name="logKey"></param>
        /// <param name="pduDirectionType"></param>
        /// <param name="pdu"></param>
        /// <param name="details"></param>
        /// <returns> External Id </returns>
        public delegate Guid? PDU_DETAILS_EVENT_HANDLER(string logKey, PduDirectionTypes pduDirectionType, Header pdu, List<PduPropertyDetail> details);

        #endregion

        #region Private Properties

        /// <summary> Flag that determines whether this instance has been disposed or not yet </summary>
        protected bool Disposed = false;
        
        /// <summary> A unique id for logging </summary>
        private int ConnectionId;

        /// <summary> The short/long code being managed </summary>
        private string ShortLongCode = null;

        /// <summary> The connection mode, Transceiver, Transmitter, Receiver</summary>
        private ConnectionModes ConnectionMode;
        
        /// <summary> The host or ipaddress to connect to </summary>
        private string Host = null;

        /// <summary> The port to connect to on the server </summary>
        private int Port = 0;

        /// <summary> The username to authenticate </summary>
        private string UserName = string.Empty;

        /// <summary> The password to authenticate </summary>
        private string Password = string.Empty;

        /// <summary> The logKey for writing logs to the correct files </summary>
        private string LogKey = string.Empty;


        /// <summary> A user supplied method to call when a message is received </summary>
        private RECEIVED_MESSAGE_HANDLER ReceivedMessageHandler = null;

        /// <summary> A user supplied method to call when a generic nack is received </summary>
        private RECEIVED_GENERICNACK_HANDLER ReceivedGenericNackHandler = null;

        /// <summary> A user supplied method to call when a submit is ascknowledged </summary>
        private SUBMIT_MESSAGE_HANDLER SubmitMessageHandler = null;

        /// <summary> A user supplied method to call when a query is responded </summary>
        private QUERY_MESSAGE_HANDLER QueryMessageHandler = null;

        /// <summary> A user supplied method to call to write logs </summary>
        private LOG_EVENT_HANDLER LogEventHandler = null;

        /// <summary> A user supplied method to call for connection events </summary>
        private CONNECTION_EVENT_HANDLER ConnectionEventHandler = null;

        /// <summary> A user supplied method to call for pdu detail data </summary>
        private PDU_DETAILS_EVENT_HANDLER PduDetailsEventHandler = null;


        /// <summary> The SMPP Client object </summary>
        private SmppClient Client = null;

        /// <summary> Handle to the thread trying to connect to the server </summary>
        private Thread ConnectionThread = null;

        /// <summary> An event to tell the connection thread to connect </summary>
        private ManualResetEvent ConnectEvent = new ManualResetEvent(true);

        /// <summary> Dictionary of packet data from cell phones </summary>
        private Dictionary<string, SmppClient.UserDataControl> Collector = new Dictionary<string, SmppClient.UserDataControl>();

        /// <summary> Keeps track of the number of bind errors </summary>
        private bool IsBound = false;
        
        #endregion

        #region Public Properties

        /// <summary> The unique connection id </summary>
        public int ConnectionIdentifier { get { return ConnectionId; } }

        /// <summary> The current status of the connection </summary>
        public ConnectionStatus Status { get { return Client.Status; } }

        #endregion

        #region Constructor

        /// <summary> Constructor For ESMS mode </summary>
        /// <param name="connectionId"></param>
        /// <param name="shortLongCode"></param>
        /// <param name="connectionMode"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="logKey"></param>
        /// <param name="defaultEncoding"></param>
        /// <param name="connectionEventHandler"></param>
        /// <param name="receivedMessageHandler"></param>
        /// <param name="receivedGenericNackHandler"></param>
        /// <param name="submitMessageHandler"></param>
        /// <param name="queryMessageHandler"></param>
        /// <param name="logEventHandler"></param>
        /// <param name="pduDetailsEventHandler"></param>
        public ESMEConnection(int connectionId, string shortLongCode, ConnectionModes connectionMode,
                              string host, int port, string userName, string password, string logKey,
                              DataCodings defaultEncoding,
                              CONNECTION_EVENT_HANDLER connectionEventHandler,
                              RECEIVED_MESSAGE_HANDLER receivedMessageHandler,
                              RECEIVED_GENERICNACK_HANDLER receivedGenericNackHandler,
                              SUBMIT_MESSAGE_HANDLER submitMessageHandler,
                              QUERY_MESSAGE_HANDLER queryMessageHandler,
                              LOG_EVENT_HANDLER logEventHandler,
                              PDU_DETAILS_EVENT_HANDLER pduDetailsEventHandler)
        {
            // Properties
            ConnectionId = connectionId;
            ShortLongCode = shortLongCode;
            ConnectionMode = connectionMode;
            Host = host;
            Port = port;
            UserName = userName;
            Password = password;
            LogKey = string.Format("{0}-{1}-{2}", logKey, ConnectionMode, ConnectionId);

            // Bind user events
            ConnectionEventHandler = connectionEventHandler;
            ReceivedMessageHandler = receivedMessageHandler;
            ReceivedGenericNackHandler = receivedGenericNackHandler;
            SubmitMessageHandler = submitMessageHandler;
            QueryMessageHandler = queryMessageHandler;
            LogEventHandler = logEventHandler;
            PduDetailsEventHandler = pduDetailsEventHandler;

            // Create the connection to the server
            Client = new SmppClient(defaultEncoding);

            // Bind Internal ESME required events
            Client.ConnectEvent += new SmppClient.ConnectedEventHandler(ClientEventConnect);
            Client.DeliverSmEvent += new SmppClient.DeliverSmEventHandler(ClientEventDeliverSm);
            Client.DisconnectEvent += new SmppClient.DisconnectEventHandler(ClientEventDisconnect);
            Client.EnquireLinkSmEvent += new SmppClient.EnquireLinkSmEventHandler(ClientEventEnquireLinkSm);
            Client.EnquireLinkSmRespEvent += new SmppClient.EnquireLinkSmRespEventHandler(ClientEventEnquireLinkSmResp);
            Client.ErrorEvent += new SmppClient.ErrorEventHandler(ClientEventError);
            Client.GenericNackSmEvent += new SmppClient.GenericNackSmEventHandler(ClientEventGenericNackSm);
            Client.QuerySmRespEvent += new SmppClient.QuerySmRespEventHandler(ClientEventQuerySmResp);
            Client.SubmitSmRespEvent += new SmppClient.SubmitSmRespEventHandler(ClientEventSubmitSmResp);
            Client.UnBindSmEvent += new SmppClient.UnBindSmEventHandler(ClientEventUnBindSm);
            Client.PduDetailsEvent += new SmppClient.PduDetailsEventHandler(ClientEventPduDetails);

            // Start a thread to get this connection
            ConnectionThread = new Thread(new ThreadStart(PerformConnectClient));
            ConnectionThread.Start();
        }

        /// <summary> Dispose </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary> Dispose </summary>
        /// <param name="disposing"></param>
        protected void Dispose(bool disposing)
        {
            WriteLog("ESMEConnection : Dispose : Started");

            if (!Disposed)
            {
                // Note disposing has begun
                Disposed = true;

                try
                {
                    WriteLog("ESMEConnection : Dispose : Info : Wait For Connection Thread To Die");

                    // Kill the PerformConnectClient thread
                    ConnectEvent.Set();
                    ConnectionThread.Join(5000);

                    WriteLog("ESMEConnection : Dispose : Info : Disconnect from smpp Started");

                    Client.Dispose();
                    Client = null;

                    WriteLog("ESMEConnection : Dispose : Info : Disconnect from smpp Completed");
                }

                catch (Exception exception)
                {
                    WriteLog(LogEventNotificationTypes.Email, "ESMEConnection : Dispose : ERROR : {0}", exception.ToString());
                }

                // Kill the PerformConnectClient thread
                ConnectEvent.Set();
            }

            WriteLog("ESMEConnection : Dispose : Completed");
        }

        #endregion

        #region Log Methods

        /// <summary> Called to write out to the log </summary>
        /// <param name="message"></param>
        private void WriteLog(string message)
        {
            if (LogEventHandler != null)
            {
                LogEventHandler(LogEventNotificationTypes.None, LogKey, ShortLongCode, message);
            }
        }

        /// <summary> Called to write out to the log </summary>
        /// <param name="message"></param>
        /// <param name="logValues"></param>
        private void WriteLog(string message, params object[] logValues)
        {
            if (LogEventHandler != null)
            {
                LogEventHandler(LogEventNotificationTypes.None, LogKey, ShortLongCode, string.Format(message, logValues));
            }
        }

        /// <summary> Called to write out to the log </summary>
        /// <param name="logEventNotificationType"></param>
        /// <param name="message"></param>
        private void WriteLog(LogEventNotificationTypes logEventNotificationType, string message)
        {
            if (LogEventHandler != null)
            {
                LogEventHandler(logEventNotificationType, LogKey, ShortLongCode, message);
            }
        }

        /// <summary> Called to write out to the log </summary>
        /// <param name="logEventNotificationType"></param>
        /// <param name="message"></param>
        /// <param name="logValues"></param>
        private void WriteLog(LogEventNotificationTypes logEventNotificationType, string message, params object[] logValues)
        {
            if (LogEventHandler != null)
            {
                LogEventHandler(logEventNotificationType, LogKey, ShortLongCode, string.Format(message, logValues));
            }
        }

        #endregion

        #region Private Methods

        /// <summary> Called to connect to the server </summary>
        private void PerformConnectClient()
        {
            for (;;)
            {
                try
                {
                    // Wait to be told to connect
                    ConnectEvent.WaitOne();

                    // Are we shutting down
                    if (Disposed == true)
                    {
                        WriteLog("ESMEConnection : PerformConnectClient : Info : Killing Thread");
                        break;
                    }

                    WriteLog("ESMEConnection : PerformConnectClient : Info : Attempting To Connect");

                    // Connect to the server
                    if (Connect())
                    {
                        if (Bind())
                        {
                            // We are good to go
                            ConnectEvent.Reset();
                        }
                        else
                        {
                            WriteLog("ESMEConnection : PerformConnectClient : Info : Dropping the Connection");

                            // Drop the connection
                            Client.Disconnect();

                            WriteLog("ESMEConnection : PerformConnectClient : Info : Sleep For 5 Seconds and try again");

                            // Wait five second an try again
                            Thread.Sleep(5000);

                            WriteLog("ESMEConnection : PerformConnectClient : Info : Try again");

                            ConnectEvent.Set();
                        }
                    }
                    else
                    {
                        // Wait five second an try again
                        Thread.Sleep(5000);

                        ConnectEvent.Set();
                    }
                }

                catch (Exception exception)
                {
                    WriteLog("ESMEConnection : PerformConnectClient : Info : Dropping the Connection : {0}", exception.Message);

                    if (Client == null)
                    {
                        WriteLog("ESMEConnection : PerformConnectClient : Info : Killing Thread");
                        break;
                    }

                    // Drop the connection
                    Client.Disconnect();

                    WriteLog("ESMEConnection : PerformConnectClient : Info : Sleep For 5 Seconds and try again");

                    // Wait five second an try again
                    Thread.Sleep(5000);

                    WriteLog("ESMEConnection : PerformConnectClient : Info : Try again");

                    ConnectEvent.Set();
                }
            }
        }

        /// <summary> Called to connect to the SMPP server </summary>
        private bool Connect()
        {
            WriteLog("ESMEConnection : Connect : Started : Host[{0}] Port[{1}]", Host, Port);

            bool retVal = false;

            try
            {
                if (Client.Status == ConnectionStatus.Closed)
                {
                    Client.AddrNpi = (byte)Npi.Unknown;
                    Client.AddrTon = (byte)Ton.NetworkSpecific;
                    Client.SystemType = string.Empty;

                    retVal = Client.Connect(Host, Port);

                    if (Client.Status != ConnectionStatus.Closed)
                    {
                        ConnectionEventHandler(LogKey, ConnectionEventTypes.Connected, string.Format("ESMEConnection : Connect : Info : Host[{0}] Port[{1}] Connection Established", Host, Port));
                        retVal = true;
                    }
                    else
                    {
                        ConnectionEventHandler(LogKey, ConnectionEventTypes.ConnectionAttemptFailed, string.Format("ESMEConnection : Connect : WARNING : Host[{0}] Port[{1}] Connection Failure", Host, Port));
                    }
                }
                else
                {
                    WriteLog("ESMEConnection : Connect : Info : Host[{0}] Port[{1}] Connection Already Established", Host, Port);
                    retVal = true;
                }
            }

            catch (Exception exception)
            {
                WriteLog("ESMEConnection : Connect : ERROR : Host[{0}] Port[{1}] Connection Failed {2}", Host, Port, exception.Message);
            }

            WriteLog("ESMEConnection : Connect : Completed : Host[{0}] Port[{1}] RetVal[{2}]", Host, Port, retVal);

            return retVal;
        }

        /// <summary> Called to disconnect to the SMPP server </summary>
        private void Disconnect()
        {
            WriteLog("ESMEConnection : Disconnect : Started : Host[{0}] Port[{1}]", Host, Port);

            try
            {
                if (Client.Status == ConnectionStatus.Bound)
                {
                    if (!UnBind())
                    {
                        Client.Disconnect();
                    }
                }

                if (Client.Status == ConnectionStatus.Open)
                {
                    Client.Disconnect();
                }
            }

            catch (Exception exception)
            {
                WriteLog(LogEventNotificationTypes.Email, "ESMEConnection : Disconnect : ERROR : {0}", exception.Message);
            }

            WriteLog("ESMEConnection : Disconnect : Completed : Host[{0}] Port[{1}]", Host, Port);
        }

        /// <summary> Called to authenticate the system </summary>
        private bool Bind()
        {
            WriteLog("ESMEConnection : Bind : Started : Host[{0}] Port[{1}]", Host, Port);

            bool retVal = false;

            try
            {
                // Authenticate to the SMPP Server
                BindSmResp btrp = Client.Bind(UserName, Password, ConnectionMode);

                // How did we do
                switch (btrp.Status)
                {
                    case CommandStatus.ESME_ROK:
                        IsBound = true;
                        ConnectionEventHandler(LogKey, ConnectionEventTypes.Bound, string.Format("ESMEConnection : Bind : Info : Host[{0}] Port[{1}] Bind Established", Host, Port));

                        retVal = true;
                        break;

                    default:
                        ConnectionEventHandler(LogKey, ConnectionEventTypes.BindingAttemptFailed, string.Format("ESMEConnection : Bind : ERROR : Host[{0}] Port[{1}] Status[{2}]", Host, Port, btrp.Status.ToString()));
                        break;
                }
            }

            catch (Exception exception)
            {
                WriteLog("ESMEConnection : Bind : ERROR : Host[{0}] Port[{1}] Bind Failed {2}", Host, Port, exception.Message);
            }

            WriteLog("ESMEConnection : Bind : Completed : Host[{0}] Port[{1}] RetVal[{2}]", Host, Port, retVal);

            return retVal;
        }

        /// <summary> Called to logoff the system </summary>
        private bool UnBind()
        {
            WriteLog("ESMEConnection : UnBind : Started : Host[{0}] Port[{1}]", Host, Port);

            bool retValue = false;

            try
            {
                UnBindSmResp ubtrp = Client.UnBind();

                switch (ubtrp.Status)
                {
                    case CommandStatus.ESME_ROK:
                        WriteLog("ESMEConnection : UnBind : Info : Host[{0}] Port[{1}] Connection UnBound", Host, Port);
                        retValue = true;
                        break;

                    default:
                        WriteLog("ESMEConnection : UnBind : WARNING : Host[{0}] Port[{1}] Status[{2}] Connection UnBound Failure", Host, Port, ubtrp.Status);
                        break;
                }
            }

            catch (Exception exception)
            {
                WriteLog("ESMEConnection : UnBind : ERROR : Host[{0}] Port[{1}] {2}", Host, Port, exception.Message);
            }

            WriteLog("ESMEConnection : Bind : Completed : Host[{0}] Port[{1}] RetValue[{2}]", Host, Port, retValue);

            return retValue;
        }

        /// <summary> Called to add the message segment to the collector dictionary </summary>
        /// <param name="data"></param>
        private void AddMessageSegmentToCollector(DeliverSm data)
        {
            try
            {
                SmppClient.UserDataControl userDataControl = null;
                string key = data.SourceAddr + data.MessageReferenceNumber;

                lock (Collector)
                {
                    if (Collector.ContainsKey(key))
                    {
                        userDataControl = Collector[key];
                    }
                    else
                    {
                        userDataControl = new SmppClient.UserDataControl(data.TotalSegments);
                        Collector.Add(key, userDataControl);
                    }
                }

                // Add the segment to the current
                userDataControl.UserData.Add(data.UserData);
                --userDataControl.SegmentsLeftToReceived;
            }

            catch (Exception exception)
            {
                WriteLog(LogEventNotificationTypes.Email, "ESMEConnection : AddMessageSegmentToCollector : ERROR : {0}", exception.ToString());
            }
        }

        /// <summary> Called to verify we received the last segment of data </summary>
        /// <param name="data"></param>
        /// <returns> True or False </returns>
        private bool IsLastSegment(DeliverSm data)
        {
            bool finished = false;

            try
            {
                SmppClient.UserDataControl userDataControl = null;
                string key = data.SourceAddr + data.MessageReferenceNumber;

                lock (Collector)
                {
                    if (Collector.ContainsKey(key))
                    {
                        userDataControl = Collector[key];
                    }
                }

                if (userDataControl != null)
                {
                    if (userDataControl.SegmentsLeftToReceived <= 0)
                    {
                        finished = true;
                    }
                }
                else
                {
                    WriteLog(LogEventNotificationTypes.Email, "ESMEConnection : IsLastSegment : ERROR : No User Data Found");
                }
            }

            catch (Exception exception)
            {
                WriteLog(LogEventNotificationTypes.Email,  "ESMEConnection : IsLastSegment : ERROR : {0}", exception.ToString());
            }

            return finished;
        }

        /// <summary> Called to retrieve the full message </summary>
        /// <param name="data"></param>
        /// <returns> The message </returns>
        private string RetrieveFullMessage(DeliverSm data)
        {
            string message = null;

            try
            {
                SmppClient.UserDataControl userDataControl = null;
                string key = data.SourceAddr + data.MessageReferenceNumber;

                lock (Collector)
                {
                    if (Collector.ContainsKey(key))
                    {
                        userDataControl = Collector[key];

                        Collector.Remove(key);
                    }
                }

                if (userDataControl != null)
                {
                    message = userDataControl.UserData.ShortMessageText(data.DefaultEncoding, data.DataCoding);
                }
                else
                {
                    WriteLog(LogEventNotificationTypes.Email, "ESMEConnection : RetrieveFullMessage : ERROR : No User Data Found");
                }
            }

            catch (Exception exception)
            {
                WriteLog(LogEventNotificationTypes.Email, "ESMEConnection : RetrieveFullMessage : ERROR : {0}", exception.ToString());
            }

            return message;
        }

        #endregion

        #region Public Methods

        /// <summary> Called to send the message </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="serviceType"></param>
        /// <param name="destinationTon"></param>
        /// <param name="destinationNpi"></param>
        /// <param name="submitDataCoding"></param>
        /// <param name="encodeDataCoding"></param>
        /// <param name="message"></param>
        /// <param name="submitSm"></param>
        /// <param name="submitSmResp"></param>
        /// <returns> 0 - Successful / 1 - Failed / 2 - Not Connected </returns>
        public int SendMessage(string phoneNumber, string serviceType, Ton destinationTon, Npi destinationNpi, DataCodings submitDataCoding, DataCodings encodeDataCoding, string message, out SubmitSm submitSm, out SubmitSmResp submitSmResp)
        {
            int retVal = 1;

            submitSm = null;
            submitSmResp = null;
            
            try
            {
                if (Client.Status != ConnectionStatus.Bound)
                {
                    WriteLog("ESMEConnection : SendMessage : Warning : Not Connected To The SMPP Server");

                    return 2;
                }

                // The message to send
                string sendMessage = null;

                // Do we need to cut the message down
                if (encodeDataCoding == DataCodings.UCS2)
                {
                    // UCS2 only supports 140 bytes
                    if (message.Length > 70)
                    {
                        WriteLog(LogEventNotificationTypes.Email, "ESMEConnection : SendMessage : WARNING : Truncating UCS2 message to 70 characters.");

                        // The default is Unicode so truncate the message
                        sendMessage = message.Substring(0, 70);
                    }
                }
                else
                {
                    if (message.Length > 160)
                    {
                        WriteLog(LogEventNotificationTypes.Email, "ESMEConnection : SendMessage : WARNING : Truncating Default message to 160 characters.");

                        sendMessage = message.Substring(0, 160);
                    }
                }

                // Prepare the message, I have made sure there is only ever one message
                // with the trunacting above
                submitSm = Client.PrepareSubmit(
                    SubmitMode.ShortMessage,
                    serviceType,
                    (byte) Ton.NetworkSpecific,
                    (byte) Npi.Unknown,
                    ShortLongCode,
                    (byte) destinationTon,
                    (byte) destinationNpi,
                    phoneNumber,
                    submitDataCoding,
                    encodeDataCoding,
                    (sendMessage == null) ? message : sendMessage);

                // Send the message
                submitSmResp = Client.Submit(submitSm);

                // Log the send call
                WriteLog("ESMEConnection : SendMessage : Send : Sequence[{0}] Phone[{1}] Status[{2}]", submitSmResp.Sequence, phoneNumber, submitSmResp.Status);

                // Was it successful
                if (submitSmResp.Status != CommandStatus.ESME_ROK)
                {
                    WriteLog("ESMEConnection : SendMessage : ERROR : Failed For Unknown Reason");

                    retVal = 1;
                }

                // Success
                retVal = 0;
            }

            catch (Exception exception)
            {
                WriteLog(LogEventNotificationTypes.Email, "ESMEConnection : SendMessage : ERROR : {0}", exception.ToString());

                retVal = 1;
            }

            return retVal;
        }

        /// <summary> Called to send the message </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="serviceType"></param>
        /// <param name="destinationTon"></param>
        /// <param name="destinationNpi"></param>
        /// <param name="submitDataCoding"></param>
        /// <param name="encodeDataCoding"></param>
        /// <param name="message"></param>
        /// <param name="submitSmList"></param>
        /// <param name="submitSmRespList"></param>
        /// <returns> 0 - Successful / 1 - Failed / 2 - Not Connected </returns>
        public int SendMessageLarge(string phoneNumber, string serviceType, Ton destinationTon, Npi destinationNpi, DataCodings submitDataCoding, DataCodings encodeDataCoding, string message, out List<SubmitSm> submitSmList, out List<SubmitSmResp> submitSmRespList)
        {
            int retVal = 1;

            submitSmList = null;
            submitSmRespList = null;

            try
            {
                if (Client.Status != ConnectionStatus.Bound)
                {
                    WriteLog("ESMEConnection : SendMessageLarge : Warning : Not Connected To The SMPP Server");

                    return 2;
                }

                // Prepare the message, I have made sure there is only ever one message
                // with the trunacting above
                submitSmList = Client.PrepareSubmitLarge(
                    SubmitMode.ShortMessage,
                    serviceType,
                    (byte) Ton.NetworkSpecific,
                    (byte) Npi.Unknown,
                    ShortLongCode,
                    (byte) destinationTon,
                    (byte) destinationNpi,
                    phoneNumber,
                    submitDataCoding,
                    encodeDataCoding,
                    message);

                // Send the message
                submitSmRespList = Client.Submit(submitSmList);

                foreach (SubmitSmResp submitSmResp in submitSmRespList)
                {
                    // Log the send call
                    WriteLog("ESMEConnection : SendMessage : Send : Sequence[{0}] Phone[{1}] Status[{2}]", submitSmResp.Sequence, phoneNumber, submitSmResp.Status);

                    // Was it successful
                    if (submitSmResp.Status != CommandStatus.ESME_ROK)
                    {
                        WriteLog("ESMEConnection : SendMessage : ERROR : Failed For Unknown Reason");
                    }
                }

                // Success
                retVal = 0;
            }

            catch (Exception exception)
            {
                WriteLog(LogEventNotificationTypes.Email, "ESMEConnection : SendMessage : ERROR : {0}", exception.ToString());

                retVal = 1;
            }

            return retVal;
        }
        
        /// <summary> Called to send a query </summary>
        /// <param name="messageId"></param>
        /// <returns> 1 - Successful / 0 - Failed </returns>
        public QuerySm SendQuery(string messageId)
        {
            QuerySm querySm = null;

            try
            {
                if (Client.Status != ConnectionStatus.Bound)
                {
                    WriteLog("ESMEConnection : SendQuery : Warning : Not Connected To The SMPP Server");

                    return querySm;
                }
                
                // Prepare the query
                querySm = QuerySm.Create(Client.DefaultEncoding, messageId, (byte) Ton.NetworkSpecific, (byte) Npi.Unknown, ShortLongCode);
                
                // Send the query
                QuerySmResp querySmResp = Client.Query(querySm);

                // Log the send call
                WriteLog("ESMEConnection : SendQueryThroughSMPP : Send : MessageId[{0}] Sequence[{1}] Status[{2}]", messageId, querySm.Sequence, querySmResp.Status);

                // Was it successful
                if (querySmResp.Status != CommandStatus.ESME_ROK)
                {
                    WriteLog("ESMEConnection : SendQueryThroughSMPP : ERROR : Failed For Unknown Reason");
                    
                    querySm = null;
                }
            }

            catch (Exception exception)
            {
                WriteLog(LogEventNotificationTypes.Email, "ESMEConnection : SendQueryThroughSMPP : ERROR : {0}", exception.ToString());
            }

            return querySm;
        }

        #endregion

        #region SMPP Event Methods

        /// <summary> Called when a connection is established </summary>
        /// <param name="sender"></param>
        /// <param name="bSuccess"></param>
        private void ClientEventConnect(object sender, bool bSuccess)
        {
        }

        /// <summary> Called when a disconnect occurs </summary>
        /// <param name="sender"></param>
        private void ClientEventDisconnect(object sender)
        {
            try
            {
                if (IsBound == true)
                {
                    IsBound = false;

                    if (ConnectionEventHandler != null)
                    {
                        ConnectionEventHandler(LogKey, ConnectionEventTypes.Disconnected, string.Format("ESMEConnection : ClientEventDisconnect : WARNING : Host[{0}] Port[{1}] Connection Disconnected", Host, Port));
                    }
                }

                // Do we need to try and connect again
                if (Disposed == false)
                {
                    ConnectEvent.Set();
                }
            }

            catch (Exception exception)
            {
                WriteLog(LogEventNotificationTypes.Email, "ESMEConnection : ClientEventDisconnect : ERROR : {0}", exception.ToString());

                // Do we need to try and connect again
                if (Disposed == false)
                {
                    ConnectEvent.Set();
                }
            }
        }

        /// <summary> Called when a message is received from the SMPP server </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private CommandStatus ClientEventDeliverSm(object sender, DeliverSm data)
        {
            WriteLog("ESMEConnection : ClientEventDeliverSm : Started : SegmentNumber[{0}]", data.SeqmentNumber);

            CommandStatus commandStatus = CommandStatus.ESME_ROK;

            try
            {
                string message = null;

                if (data.SeqmentNumber > 0)
                {
                    // There are more than 1 seqments to the outbound message

                    AddMessageSegmentToCollector(data);

                    string logMessage = string.Format("ServiceType[{0}] DestAddr[{1}] SourceAddr[{2}] MessageReferenceNumber[{3}] Sequence[{4}] SeqmentNumber[{5}] TotalSegments[{6}] DataCoding[{7}] MessageText[{8}]",
                        data.ServiceType,
                        data.DestAddr,
                        data.SourceAddr,
                        data.MessageReferenceNumber,
                        data.Sequence,
                        data.SeqmentNumber,
                        data.TotalSegments,
                        data.DataCoding,
                        data.UserData.ShortMessageText(data.DefaultEncoding, data.DataCoding));

                    WriteLog("ESMEConnection : ClientEventDeliverSm : Info : Partial Message : {0}", logMessage);

                    if (IsLastSegment(data))
                    {
                        message = RetrieveFullMessage(data);
                    }
                }
                else
                {
                    // There is only 1 seqment to the outbound message
                    message = data.UserData.ShortMessageText(data.DefaultEncoding, data.DataCoding);
                }

                if ((message != null) && (ReceivedMessageHandler != null))
                {
                    // Message has been received
                    ReceivedMessageHandler(LogKey, data.ServiceType, (Ton) data.SourceTon, (Npi) data.SourceNpi, ShortLongCode, DateTime.Now, data.SourceAddr, data.DataCoding, message);
                }
            }

            catch (Exception exception)
            {
                WriteLog(LogEventNotificationTypes.Email, "ESMEConnection : ClientEventDeliverSm : ERROR : {0}", exception.ToString());
            }

            return commandStatus;
        }

        /// <summary> Called when the SMPP Server receives a sent message </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private void ClientEventSubmitSmResp(object sender, SubmitSmResp data)
        {
            try
            {
                WriteLog("ESMEConnection : ClientEventSubmitSmResp : Info : Sequence[{0}] Status[{1}]", data.Sequence, data.Status);

                if (SubmitMessageHandler != null)
                {
                    SubmitMessageHandler(LogKey, (int) data.Sequence, data.MessageId);
                }
            }

            catch (Exception exception)
            {
                WriteLog(LogEventNotificationTypes.Email, "ESMEConnection : ClientEventSubmitSmResp : ERROR : {0}", exception.ToString());
            }
        }
        
        /// <summary> Called when the SMPP query is returned </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private void ClientEventQuerySmResp(object sender, QuerySmResp data)
        {
            try
            {
                WriteLog("ESMEConnection : ClientEventQuerySmResp : Info : MessageId[{0}] MessageState[{1}] Status[{2}]", data.MessageId, data.MessageState, data.Status);

                if (QueryMessageHandler != null)
                {
                    QueryMessageHandler(LogKey, (int) data.Sequence, data.MessageId, data.FinalDate, (int) data.MessageState, (long) data.ErrorCode);
                }
            }

            catch (Exception exception)
            {
                WriteLog(LogEventNotificationTypes.Email, "ESMEConnection : ClientEventQuerySmResp : ERROR : {0}", exception.ToString());
            }
        }

        /// <summary> Called when an enquire link is received </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private CommandStatus ClientEventEnquireLinkSm(object sender, EnquireLinkSm data)
        {
            WriteLog("ESMEConnection : ClientEventEnquireLinkSm : Info : Enquire Link : Command[{0}] Length[{1}] Sequence[{2}] Status[{3}]", data.Command, data.Length, data.Sequence, data.Status);

            return CommandStatus.ESME_ROK;
        }

        /// <summary> Called when an enquire link resp is received </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private void ClientEventEnquireLinkSmResp(object sender, EnquireLinkSmResp data)
        {
            WriteLog("ESMEConnection : ClientEventEnquireLinkSmResp : Info : Enquire Link Resp : Command[{0}] Length[{1}] Sequence[{2}] Status[{3}]", data.Command, data.Length, data.Sequence, data.Status);
        }

        /// <summary> Called when a generic nack is received </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private void ClientEventGenericNackSm(object sender, GenericNackSm data)
        {
            WriteLog("ESMEConnection : ClientEventGenericNackSm : Info : Received Generic Nack  Status[{0}]", data.Status);

            try
            {
                // Generic Nack has been received
                if (ReceivedGenericNackHandler != null)
                {
                    ReceivedGenericNackHandler(LogKey, (int) data.Sequence);
                }
            }

            catch (Exception exception)
            {
                WriteLog(LogEventNotificationTypes.Email, "ESMEConnection : ClientEventGenericNackSm : ERROR : {0}", exception.ToString());
            }
        }

        /// <summary> Called when an error occurs </summary>
        /// <param name="sender"></param>
        /// <param name="comment"></param>
        /// <param name="ex"></param>
        private void ClientEventError(object sender, string comment, Exception ex)
        {
            if (ex != null)
            {
                WriteLog("ESMEConnection : ClientEventError : Info : {0} : {1}", comment, ex.Message);
            }
            else
            {
                WriteLog("ESMEConnection : ClientEventError : ERROR : {0}", comment);
            }
        }

        /// <summary> Called when an unbind command is complete </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private CommandStatus ClientEventUnBindSm(object sender, UnBindSm data)
        {
            if (ConnectionEventHandler != null)
            {
                ConnectionEventHandler(LogKey, ConnectionEventTypes.UnBound, string.Format("ESMEConnection : ClientEventUnBindSm : WARNING : Host[{0}] Port[{1}] Connection UnBound", Host, Port, ConnectionMode, ConnectionId));
            }

            return CommandStatus.ESME_ROK;
        }

        /// <summary> Called when a pdu details are available </summary>
        /// <param name="send"></param>
        /// <param name="pduDirectionType"></param>
        /// <param name="pdu"></param>
        /// <param name="details"></param>
        /// <returns> External Id </returns>
        private Guid? ClientEventPduDetails(object send, PduDirectionTypes pduDirectionType, Header pdu, List<PduPropertyDetail> details)
        {
            Guid? externalId = null;

            if (PduDetailsEventHandler != null)
            {
                externalId = PduDetailsEventHandler(LogKey, pduDirectionType, pdu, details);
            }

            return externalId;
        }

        #endregion
    }
}
