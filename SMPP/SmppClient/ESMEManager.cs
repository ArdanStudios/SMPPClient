#region Namespaces

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> Provides ESME (Extended Short Message Entity) Management </summary>
    public class ESMEManager : IDisposable
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
        
        /// <summary> The logKey for writing logs to the correct files </summary>
        private string LogKey = string.Empty;

        /// <summary> The short/long code being managed </summary>
        private string ShortLongCode = null;


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


        /// <summary> A list of receiver connections </summary>
        private List<ESMEConnection> Receivers = new List<ESMEConnection>();

        /// <summary> A dictionary of transmitter connections </summary>
        private Dictionary<int, ESMEConnection> Transmitters = new Dictionary<int,ESMEConnection>();

        /// <summary> A pointer to the next transmitter to use </summary>
        private int NextTransmitter = 1;

        #endregion

        #region Constructor

        /// <summary> Contructor </summary>
        /// <param name="logKey"></param>
        /// <param name="shortLongCode"></param>
        /// <param name="connectionEventHandler"></param>
        /// <param name="receivedMessageHandler"></param>
        /// <param name="receivedGenericNackHandler"></param>
        /// <param name="submitMessageHandler"></param>
        /// <param name="queryMessageHandler"></param>
        /// <param name="logEventHandler"></param>
        /// <param name="pduDetailsEventHandler"></param>
        public ESMEManager(string logKey, string shortLongCode,
                                     CONNECTION_EVENT_HANDLER connectionEventHandler, RECEIVED_MESSAGE_HANDLER receivedMessageHandler,
                                     RECEIVED_GENERICNACK_HANDLER receivedGenericNackHandler, SUBMIT_MESSAGE_HANDLER submitMessageHandler,
                                     QUERY_MESSAGE_HANDLER queryMessageHandler, LOG_EVENT_HANDLER logEventHandler,
                                     PDU_DETAILS_EVENT_HANDLER pduDetailsEventHandler)
        {
            LogKey = logKey;
            ShortLongCode = shortLongCode;

            ConnectionEventHandler = connectionEventHandler;
            ReceivedMessageHandler = receivedMessageHandler;
            ReceivedGenericNackHandler = receivedGenericNackHandler;
            SubmitMessageHandler = submitMessageHandler;
            QueryMessageHandler = queryMessageHandler;
            LogEventHandler = logEventHandler;
            PduDetailsEventHandler = pduDetailsEventHandler;
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
            WriteLog("ESMEManager : Dispose : Started");

            if (!Disposed)
            {
                // Note disposing has begun
                Disposed = true;

                try
                {
                    WriteLog("ESMEManager : Dispose : Info : Disconnect the receiver connections");

                    foreach (ESMEConnection smppConnection in Receivers)
                    {
                        smppConnection.Dispose();
                    }
                }

                catch (Exception exception)
                {
                    WriteLog(LogEventNotificationTypes.Email, "ESMEManager : Dispose : ERROR : {0}", exception.ToString());
                }

                try
                {
                    WriteLog("ESMEManager : Dispose : Info : Disconnect the transmitter connections");

                    foreach (ESMEConnection smppConnection in Transmitters.Values)
                    {
                        smppConnection.Dispose();
                    }
                }

                catch (Exception exception)
                {
                    WriteLog(LogEventNotificationTypes.Email, "ESMEManager : Dispose : ERROR : {0}", exception.ToString());
                }
            }

            WriteLog("ESMEManager : Dispose : Completed");
        }

        #endregion

        #region Log Methods
        
        /// <summary> Called to write out to the log </summary>
        /// <param name="message"></param>
        private void WriteLog(string message)
        {
            LogEventHandler(LogEventNotificationTypes.None, LogKey, ShortLongCode, message);
        }

        /// <summary> Called to write out to the log </summary>
        /// <param name="message"></param>
        /// <param name="logValues"></param>
        private void WriteLog(string message, params object[] logValues)
        {
            LogEventHandler(LogEventNotificationTypes.None, LogKey, ShortLongCode, string.Format(message, logValues));
        }

        /// <summary> Called to write out to the log </summary>
        /// <param name="logEventNotificationType"></param>
        /// <param name="message"></param>
        private void WriteLog(LogEventNotificationTypes logEventNotificationType, string message)
        {
            LogEventHandler(logEventNotificationType, LogKey, ShortLongCode, message);
        }

        /// <summary> Called to write out to the log </summary>
        /// <param name="logEventNotificationType"></param>
        /// <param name="message"></param>
        /// <param name="logValues"></param>
        private void WriteLog(LogEventNotificationTypes logEventNotificationType, string message, params object[] logValues)
        {
            LogEventHandler(logEventNotificationType, LogKey, ShortLongCode, string.Format(message, logValues));
        }

        #endregion

        #region Event Methods

        /// <summary> Called when a connection event is fired </summary>
        /// <param name="logKey"></param>
        /// <param name="connectionEventType"></param>
        /// <param name="message"></param>
        public void ConnectionEventConnectionHandler(string logKey, ConnectionEventTypes connectionEventType, string message)
        {
            if (ConnectionEventHandler != null)
            {
                ConnectionEventHandler(logKey, connectionEventType, message);
            }
        }

        /// <summary> Called when a message is received on a connection </summary>
        /// <param name="logKey"></param>
        /// <param name="serviceType"></param>
        /// <param name="sourceTon"></param>
        /// <param name="sourceNpi"></param>
        /// <param name="shortLongCode"></param>
        /// <param name="dateReceived"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="dataCoding"></param>
        /// <param name="message"></param>
        public void ReceivedMessageConnectionHandler(string logKey, string serviceType, Ton sourceTon, Npi sourceNpi, string shortLongCode, DateTime dateReceived, string phoneNumber, DataCodings dataCoding, string message)
        {
            if (ReceivedMessageHandler != null)
            {
                ReceivedMessageHandler(logKey, serviceType, sourceTon, sourceNpi, shortLongCode, dateReceived, phoneNumber, dataCoding, message);
            }
        }

        /// <summary> A user supplied method to call when a generic nack is received </summary>
        /// <param name="logKey"></param>
        /// <param name="sequence"></param>
        public void ReceivedGenericNackConnectionHandler(string logKey, int sequence)
        {
            if (ReceivedGenericNackHandler != null)
            {
                ReceivedGenericNackHandler(logKey, sequence);
            }
        }

        /// <summary> Called when a submit message is acknowledged </summary>
        /// <param name="logKey"></param>
        /// <param name="sequence"></param>
        /// <param name="messageId"></param>
        public void SubmitMessageConnectionHandler(string logKey, int sequence, string messageId)
        {
            if (SubmitMessageHandler != null)
            {
                SubmitMessageHandler(logKey, sequence, messageId);
            }
        }

        /// <summary> Called when a query message is responded </summary>
        /// <param name="logKey"></param>
        /// <param name="sequence"></param>
        /// <param name="messageId"></param>
        /// <param name="finalDate"></param>
        /// <param name="messageState"></param>
        /// <param name="errorCode"></param>
        public void QueryMessageConnectionHandler(string logKey, int sequence, string messageId, DateTime finalDate, int messageState, long errorCode)
        {
            if (QueryMessageHandler != null)
            {
                QueryMessageHandler(logKey, sequence, messageId, finalDate, messageState, errorCode);
            }
        }

        /// <summary> Called to log an event </summary>
        /// <param name="logEventNotificationType"></param>
        /// <param name="shortLongCode"></param>
        /// <param name="message"></param>
        /// <param name="logKey"></param>
        public void LogEventConnectionHandler(LogEventNotificationTypes logEventNotificationType, string logKey, string shortLongCode, string message)
        {
            if (LogEventHandler != null)
            {
                LogEventHandler(logEventNotificationType, logKey, shortLongCode, message);
            }
        }

        /// <summary> Called when a pdu details are available </summary>
        /// <param name="logKey"></param>
        /// <param name="pduDirectionType"></param>
        /// <param name="pdu"></param>
        /// <param name="details"></param>
        /// <returns> External Id </returns>
        private Guid? PduDetailsConnectionHandler(string logKey, PduDirectionTypes pduDirectionType, Header pdu, List<PduPropertyDetail> details)
        {
            Guid? externalId = null;

            if (PduDetailsEventHandler != null)
            {
                externalId = PduDetailsEventHandler(logKey, pduDirectionType, pdu, details);
            }

            return externalId;
        }

        #endregion

        #region Connection Management Methods

        /// <summary> Called to add a transceiver connection </summary>
        /// <param name="connectionId"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="logKey"></param>
        /// <param name="defaultEncoding"></param>
        private void AddTransceiverConnection(int connectionId, string host, int port, string userName, string password, string logKey, DataCodings defaultEncoding)
        {
            lock (Receivers)
            {
                // Create the smppConnection object
                ESMEConnection smppConnection = new ESMEConnection(connectionId, ShortLongCode, ConnectionModes.Transceiver,
                                                                   host, port, userName, password, logKey, defaultEncoding,
                                                                   new ESMEConnection.CONNECTION_EVENT_HANDLER(ConnectionEventConnectionHandler),
                                                                   new ESMEConnection.RECEIVED_MESSAGE_HANDLER(ReceivedMessageConnectionHandler),
                                                                   new ESMEConnection.RECEIVED_GENERICNACK_HANDLER(ReceivedGenericNackConnectionHandler),
                                                                   new ESMEConnection.SUBMIT_MESSAGE_HANDLER(SubmitMessageConnectionHandler),
                                                                   new ESMEConnection.QUERY_MESSAGE_HANDLER(QueryMessageConnectionHandler),
                                                                   new ESMEConnection.LOG_EVENT_HANDLER(LogEventConnectionHandler),
                                                                   new ESMEConnection.PDU_DETAILS_EVENT_HANDLER(PduDetailsConnectionHandler));
            
                // Add the connection to both list
                Transmitters.Add(connectionId, smppConnection);
            }
        }

        /// <summary> Called to add a receiver connection </summary>
        /// <param name="connectionId"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="logKey"></param>
        /// <param name="defaultEncoding"></param>
        private void AddReceiverConnection(int connectionId, string host, int port, string userName, string password, string logKey, DataCodings defaultEncoding)
        {
            lock (Receivers)
            {
                // Create the smppConnection object
                ESMEConnection smppConnection = new ESMEConnection(connectionId, ShortLongCode, ConnectionModes.Receiver,
                                                                   host, port, userName, password, logKey, defaultEncoding,
                                                                   new ESMEConnection.CONNECTION_EVENT_HANDLER(ConnectionEventConnectionHandler),
                                                                   new ESMEConnection.RECEIVED_MESSAGE_HANDLER(ReceivedMessageConnectionHandler),
                                                                   new ESMEConnection.RECEIVED_GENERICNACK_HANDLER(ReceivedGenericNackConnectionHandler),
                                                                   null,
                                                                   new ESMEConnection.QUERY_MESSAGE_HANDLER(QueryMessageConnectionHandler),
                                                                   new ESMEConnection.LOG_EVENT_HANDLER(LogEventConnectionHandler),
                                                                   new ESMEConnection.PDU_DETAILS_EVENT_HANDLER(PduDetailsConnectionHandler));
            
                // Add the connection to the list
                Receivers.Add(smppConnection);
            }
        }

        /// <summary> Called to add a transmitter connection </summary>
        /// <param name="connectionId"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="logKey"></param>
        /// <param name="defaultEncoding"></param>
        private void AddTransmitterConnection(int connectionId, string host, int port, string userName, string password, string logKey, DataCodings defaultEncoding)
        {
            lock (Transmitters)
            {
                // Create the smppConnection object
                ESMEConnection smppConnection = new ESMEConnection(connectionId, ShortLongCode, ConnectionModes.Transmitter,
                                                                   host, port, userName, password, logKey, defaultEncoding,
                                                                   new ESMEConnection.CONNECTION_EVENT_HANDLER(ConnectionEventConnectionHandler),
                                                                   null,
                                                                   new ESMEConnection.RECEIVED_GENERICNACK_HANDLER(ReceivedGenericNackConnectionHandler),
                                                                   new ESMEConnection.SUBMIT_MESSAGE_HANDLER(SubmitMessageConnectionHandler),
                                                                   new ESMEConnection.QUERY_MESSAGE_HANDLER(QueryMessageConnectionHandler),
                                                                   new ESMEConnection.LOG_EVENT_HANDLER(LogEventConnectionHandler),
                                                                   new ESMEConnection.PDU_DETAILS_EVENT_HANDLER(PduDetailsConnectionHandler));
            
                // Add the connection to the list
                Transmitters.Add(connectionId, smppConnection);
            }
        }

        /// <summary> Called to return the next transmitter for sending </summary>
        /// <returns> SmppConnection </returns>
        private ESMEConnection NextTransmitterConnection()
        {
            ESMEConnection smppConnection = null;

            int totalConnections = Transmitters.Count();

            // We only want a bound connection. We will try them all
            for (int connection = 0; connection < totalConnections; ++connection)
            {
                lock (Transmitters)
                {
                    smppConnection = Transmitters[NextTransmitter];

                    if (++NextTransmitter > Transmitters.Count())
                    {
                        NextTransmitter = 1;
                    }

                    if (smppConnection.Status == ConnectionStatus.Bound)
                    {
                        break;
                    }

                    smppConnection = null;
                }
            }

            return smppConnection;
        }

        #endregion

        #region Public Methods

        /// <summary> Called to add connections </summary>
        /// <param name="howMany"></param>
        /// <param name="connectionMode"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="logKey"></param>
        /// <param name="defaultEncoding"></param>
        public void AddConnections(int howMany, ConnectionModes connectionMode, string host, int port,
                                   string userName, string password, string logKey, DataCodings defaultEncoding)
        {
            WriteLog("ESMEManager : AddConnections : Started : HowMany[{0}] ConnectionMode[{1}] Host[{2}] Port[{3}] LogKey[{4}] DefaultEncoding[{5}]", howMany, connectionMode, host, port, logKey, defaultEncoding);

            for (int connection = 1; connection <= howMany; ++connection)
            {
                if (connectionMode == ConnectionModes.Transceiver)
                {
                    AddTransceiverConnection(connection, host, port, userName, password, logKey, defaultEncoding);
                }
                else if (connectionMode == ConnectionModes.Receiver)
                {
                    AddReceiverConnection(connection, host, port, userName, password, logKey, defaultEncoding);
                }
                else
                {
                    AddTransmitterConnection(connection, host, port, userName, password, logKey, defaultEncoding);
                }
            }
        }

        /// <summary> Called to send the message </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="serviceType"></param>
        /// <param name="sourceTon"></param>
        /// <param name="sourceNpi"></param>
        /// <param name="submitDataCoding"></param>
        /// <param name="encodeDataCoding"></param>
        /// <param name="message"></param>
        /// <param name="submitSm"></param>
        /// <param name="submitSmResp"></param>
        /// <returns> 1 - Successful / 0 - Failed </returns>
        public int SendMessage(string phoneNumber, string serviceType, Ton sourceTon, Npi sourceNpi, DataCodings submitDataCoding, DataCodings encodeDataCoding, string message,  out SubmitSm submitSm, out SubmitSmResp submitSmResp)
        {
            int retVal = 0;

            submitSm = null;
            submitSmResp = null;

            try
            {
                // Capture the next transmitter connection
                ESMEConnection smppConnection = NextTransmitterConnection();

                if (smppConnection == null)
                {
                    WriteLog("ESMEManager : SendMessage : Warning : Not Bound To The SMPP Server");

                    return 2;
                }

                // Send the message
                retVal = smppConnection.SendMessage(phoneNumber, serviceType, sourceTon, sourceNpi, submitDataCoding, encodeDataCoding, message, out submitSm, out submitSmResp);
            }

            catch (Exception exception)
            {
                WriteLog(LogEventNotificationTypes.Email, "ESMEManager : SendMessage : ERROR : {0}", exception.ToString());
            }

            return retVal;
        }

        /// <summary> Called to send the message </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="serviceType"></param>
        /// <param name="sourceTon"></param>
        /// <param name="sourceNpi"></param>
        /// <param name="submitDataCoding"></param>
        /// <param name="encodeDataCoding"></param>
        /// <param name="message"></param>
        /// <param name="submitSmList"></param>
        /// <param name="submitSmRespList"></param>
        /// <returns> 1 - Successful / 0 - Failed </returns>
        public int SendMessageLarge(string phoneNumber, string serviceType, Ton sourceTon, Npi sourceNpi, DataCodings submitDataCoding, DataCodings encodeDataCoding, string message, out List<SubmitSm> submitSmList, out List<SubmitSmResp> submitSmRespList)
        {
            int retVal = 0;

            submitSmList = null;
            submitSmRespList = null;

            try
            {
                // Capture the next transmitter connection
                ESMEConnection smppConnection = NextTransmitterConnection();

                if (smppConnection == null)
                {
                    WriteLog("ESMEManager : SendMessage : Warning : Not Bound To The SMPP Server");

                    return 2;
                }

                // Send the message
                retVal = smppConnection.SendMessageLarge(phoneNumber, serviceType, sourceTon, sourceNpi, submitDataCoding, encodeDataCoding, message, out submitSmList, out submitSmRespList);
            }

            catch (Exception exception)
            {
                WriteLog(LogEventNotificationTypes.Email, "ESMEManager : SendMessage : ERROR : {0}", exception.ToString());
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
                // Capture the next transmitter connection
                ESMEConnection smppConnection = NextTransmitterConnection();

                if (smppConnection.Status != ConnectionStatus.Bound)
                {
                    WriteLog("ESMEManager : SendMessage : Warning : Not Connected To The SMPP Server");

                    return querySm;
                }

                // Send the message
                querySm = smppConnection.SendQuery(messageId);
            }

            catch (Exception exception)
            {
                WriteLog(LogEventNotificationTypes.Email, "ESMEManager : SendMessage : ERROR : {0}", exception.ToString());
            }

            return querySm;
        }

        #endregion
    }
}
