#region Namespaces

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using ArdanStudios.Common.Utilities;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> SMPP protocol client class for connection with an SMSC over a TCP/IP 
    /// and for sending or receiving short messages to and from the SMSC </summary>
    public class SmppClient : IDisposable
    {
        #region Handler Delegates

        /// <summary> Occurs when received AlertNotification PDU </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public delegate void AlertNotificationEventHandler(object sender, AlertNotification data);

        /// <summary> Occurs when Bind packet received from SMSC </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        /// <returns> CommandStatus </returns>
        public delegate CommandStatus BindSmEventHandler(object sender, BindSm data);

        /// <summary> Occurs when Bind Response packet received from SMSC </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public delegate void BindSmRespEventHandler(object sender, BindSmResp data);

        /// <summary> Occurs when received CancelSm PDU </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        /// <returns> CommandStatus </returns>
        public delegate CommandStatus CancelSmEventHandler(object sender, CancelSm data);

        /// <summary> Occurs when received CancelSmResp PDU </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public delegate void CancelSmRespEventHandler(object sender, CancelSmResp data);

        /// <summary> Occurs when ESME connected to SMSC </summary>
        /// <param name="sender"></param>
        /// <param name="bSuccess"></param>
        public delegate void ConnectedEventHandler(object sender, bool bSuccess);

        /// <summary> Occurs when received DeliverSm PDU </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        /// <returns> CommandStatus </returns>
        public delegate CommandStatus DeliverSmEventHandler(object sender, DeliverSm data);

        /// <summary> Occurs when received DeliverSmResp PDU </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public delegate void DeliverSmRespEventHandler(object sender, DeliverSmResp data);

        /// <summary> Occurs when receiving DataSm PDU </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        /// <param name="messageId"></param>
        /// <returns> CommandStatus </returns>
        public delegate CommandStatus DataSmEventHandler(object sender, DataSm data, out string messageId);

        /// <summary> Occurs when receiving DataSmResp PDU </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public delegate void DataSmRespEventHandler(object sender, DataSmResp data);

        /// <summary> Occurs when ESME disconnected from SMSC </summary>
        /// <param name="sender"></param>
        public delegate void DisconnectEventHandler(object sender);

        /// <summary> Occurs when received EnquireLinkSm PDU </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        /// <returns> CommandStatus </returns>
        public delegate CommandStatus EnquireLinkSmEventHandler(object sender, EnquireLinkSm data);
        
        /// <summary> Occurs when received EnquireLinkSmResp PDU </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public delegate void EnquireLinkSmRespEventHandler(object sender, EnquireLinkSmResp data);

        /// <summary> Occurs when SmppClient got an error or exception </summary>
        /// <param name="sender"></param>
        /// <param name="comment"></param>
        /// <param name="message"></param>
        public delegate void ErrorEventHandler(object sender, string comment, Exception message);

        /// <summary> Occurs when received GenericNack PDU </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public delegate void GenericNackSmEventHandler(object sender, GenericNackSm data);

        /// <summary> Occurs when a PDU is received or sent </summary>
        /// <param name="sender"></param>
        /// <param name="pduDirectionType"></param>
        /// <param name="pdu"></param>
        /// <param name="details"></param>
        /// <returns> External Id </returns>
        public delegate Guid? PduDetailsEventHandler(object sender, PduDirectionTypes pduDirectionType, Header pdu, List<PduPropertyDetail> details);

        /// <summary> Occurs when received QuerySm PDU </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        /// <param name="querySmResp"></param>
        public delegate void QuerySmEventHandler(object sender, QuerySm data, QuerySmResp querySmResp);

        /// <summary> Occurs when received QuerySmResp PDU </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public delegate void QuerySmRespEventHandler(object sender, QuerySmResp data);

        /// <summary> Occurs when any PDU was sent from ESME to SMSC </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public delegate void SendDataEventHandler(object sender, byte[] data);

        /// <summary> Occurs when received SubmitSm PDU </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        /// <param name="messageId"></param>
        public delegate CommandStatus SubmitSmEventHandler(object sender, SubmitSm data, out string messageId);

        /// <summary> Occurs when received SubmitSmResp PDU </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public delegate void SubmitSmRespEventHandler(object sender, SubmitSmResp data);

        /// <summary> Occurs when received SubmitMultiSm PDU </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        /// <param name="messageId"></param>
        /// <returns> CommandStatus </returns>
        public delegate CommandStatus SubmitMultiSmEventHandler(object sender, SubmitMultiSm data, out string messageId);

        /// <summary> Occurs when received SubmitMultiSmResp PDU </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public delegate void SubmitMultiSmRespEventHandler(object sender, SubmitMultiSmResp data);

        /// <summary> Occurs when any PDU was received from SMSC to ESME </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public delegate void ReceiveDataEventHandler(object sender, byte[] data);

        /// <summary> Occurs when Unbind packet received from SMSC </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        /// <returns> CommandStatus </returns>
        public delegate CommandStatus UnBindSmEventHandler(object sender, UnBindSm data);

        /// <summary> Occurs when Unbind Response packet received from SMSC </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public delegate void UnBindSmRespEventHandler(object sender, UnBindSmResp data);

        #endregion

        #region Private Properties
        
        /// <summary> Identifies if the object is being destroyed </summary>
        private bool _Disposed = false;
        
        /// <summary> Identifies the type requesting to bind with the SMSC </summary>
        private string _SystemType;

        /// <summary> Identifies a set of SME addresses. Regular Expression notation should be used to specify a range of addresses </summary>
        private string _AddressRange;

        /// <summary> Numbering Plan Indicator for ESME address </summary>
        private byte _AddrNpi;

        /// <summary> Indicates Type of Number of the ESME address. </summary>
        private byte _AddrTon;

        /// <summary> Response waiting timeout </summary>
        private int _Timeout = 30 * 1000;

        /// <summary> Interval in seconds between EnquireLink submition </summary>
        private int _EnqInterval = 10 * 1000;

        /// <summary> Defines maximum bytes length in one part of the concatenated short messages. The GSM 7bit standard defines a maximum of 160 octets for a single short message </summary>
        private int _ShortMessageMaxBytes = 160;

        /// <summary> Time to keep the connection with the SMPP server alive </summary>
        private SynchronousTimer _EnquireLinkTimer = null;

        /// <summary> The socket to the SMPP server </summary>
        private SocketClient _SocketClient = null;
        
        /// <summary> Provides connection status information </summary>
        private ConnectionStatus _Status = ConnectionStatus.Closed;

        /// <summary> The date and time of the last submit </summary>
        private DateTime _LastSubmit = DateTime.MinValue;
         
         /// <summary> Used to serialze access to the last submit object </summary>
        private object _LastSubmitLock = new object();

        /// <summary> The date and time of the last pdu received </summary>
        private DateTime _LastPdu = DateTime.MinValue;
        
        /// <summary> Used to serialze access to the last pdu object </summary>
        private object _LastPduLock = new object();
        
        /// <summary> The connection mode for this connection </summary>
        private ConnectionModes _ConnectionMode;

        #endregion

        #region Private Response Properties

        /// <summary> Response to the bind request </summary>
        private BindSmResp BindSmResp = null;

        /// <summary> Response to the unbind request </summary>
        private UnBindSmResp UnBindSmResp = null;

        /// <summary> Response to the enquire link request </summary>
        private EnquireLinkSmResp EnquireLinkSmResp = null;

        #endregion

        #region Handler Properties

        /// <summary> Occurs when received AlertNotification PDU </summary>
        public event AlertNotificationEventHandler AlertNotificationEvent;

        /// <summary> Occurs when Bind packet received from SMSC </summary>
        public event BindSmEventHandler BindSmEvent;

        /// <summary> Occurs when Bind packet received from SMSC </summary>
        public event BindSmRespEventHandler BindSmRespEvent;

        /// <summary> Occurs when received CancelSm PDU </summary>
        public event CancelSmEventHandler CancelSmEvent;

        /// <summary> Occurs when received CancelSmResp PDU </summary>
        public event CancelSmRespEventHandler CancelSmRespEvent;

        /// <summary> Occurs when ESME connected to SMSC </summary>
        public event ConnectedEventHandler ConnectEvent;

        /// <summary> Occurs when receiving DataSm PDU </summary>
        public event DataSmEventHandler DataSmEvent;

        /// <summary> Occurs when received DataSmResp PDU </summary>
        public event DataSmRespEventHandler DataSmRespEvent;

         /// <summary> Occurs when receiving DeliverSm PDU </summary>
        public event DeliverSmEventHandler DeliverSmEvent;

        /// <summary> Occurs when received DeliverSmResp PDU </summary>
        public event DeliverSmRespEventHandler DeliverSmRespEvent;

        /// <summary> Occurs when ESME disconnected from SMSC </summary>
        public event DisconnectEventHandler DisconnectEvent;

        /// <summary> Occurs when received EnquireLink PDU </summary>
        public event EnquireLinkSmEventHandler EnquireLinkSmEvent;
        
        /// <summary> Occurs when received EnquireLink PDU </summary>
        public event EnquireLinkSmRespEventHandler EnquireLinkSmRespEvent;

        /// <summary> Occurs when SmppClient got an error or exception </summary>
        public event ErrorEventHandler ErrorEvent;

        /// <summary> Occurs when received GenericNack PDU </summary>
        public event GenericNackSmEventHandler GenericNackSmEvent;

        /// <summary> Occurs when a PDU is received or sent </summary>
        public event PduDetailsEventHandler PduDetailsEvent;

        /// <summary> Occurs when received QuerySm PDU </summary>
        public event QuerySmEventHandler QuerySmEvent;

        /// <summary> Occurs when received QuerySmResp PDU </summary>
        public event QuerySmRespEventHandler QuerySmRespEvent;

        /// <summary> Occurs when received SubmitSm PDU </summary>
        public event SubmitSmEventHandler SubmitSmEvent;

        /// <summary> Occurs when received SubmitSmResp PDU </summary>
        public event SubmitSmRespEventHandler SubmitSmRespEvent;

        /// <summary> Occurs when received SubmitMultiResp PDU </summary>
        public event SubmitMultiSmEventHandler SubmitMultiSmEvent;

        /// <summary> Occurs when received SubmitMultiResp PDU </summary>
        public event SubmitMultiSmRespEventHandler SubmitMultiSmRespEvent;

        /// <summary> Occurs when Unbind packet received from SMSC </summary>
        public event UnBindSmEventHandler UnBindSmEvent;

        /// <summary> Occurs when Unbind packet received from SMSC </summary>
        public event UnBindSmRespEventHandler UnBindSmRespEvent;

        #endregion

        #region Event Signaling Properties

        /// <summary> Used to manage a submit/response exchange </summary>
        private class EventData
        {
            #region Properties

            public AutoResetEvent SignalEvent = new AutoResetEvent(false);
            public Header ResponseObject = null;

            #endregion

            #region Methods

            /// <summary> Resets the object for new use </summary>
            public void Release()
            {
                SignalEvent.Reset();
                ResponseObject = null;
            }

            #endregion
        }

        /// <summary> Event to signal genric event is done </summary>
        private ManualResetEvent GenericDone = new ManualResetEvent(false);
        
        /// <summary> Event to signal connection is done </summary>
        private ManualResetEvent ConnectDone = new ManualResetEvent(false);

        /// <summary> Event to signal bind is done </summary>
        private ManualResetEvent BindDone = new ManualResetEvent(false);

        /// <summary> Event to signal unbind is done </summary>
        private ManualResetEvent UnbindDone = new ManualResetEvent(false);

        /// <summary> Event to signal enquire is done </summary>
        private ManualResetEvent EnquireDone = new ManualResetEvent(false);

        /// <summary> Events to signal submit is done </summary>
        private Dictionary<uint, EventData> SubmitDone = new Dictionary<uint, EventData>();

        /// <summary> A set of events for processing </summary>
        private Stack<EventData> EventDataList = new Stack<EventData>();

        #endregion
        
        #region Public Properties

        /// <summary> Identifies the type requesting to bind with the SMSC </summary>
        public string SystemType { get { return _SystemType; } set { _SystemType = value; } }

        /// <summary> Identifies a set of SME addresses. Regular Expression notation should be used to specify a range of addresses </summary>
        public string AddressRange { get { return _AddressRange; } set { _AddressRange = value; } }

        /// <summary> Numbering Plan Indicator for ESME address </summary>
        public byte AddrNpi { get { return _AddrNpi; } set { _AddrNpi = value; } }

        /// <summary> Indicates Type of Number of the ESME address. </summary>
        public byte AddrTon { get { return _AddrTon; } set { _AddrTon = value; } }

        /// <summary> Response waiting timeout </summary>
        public int Timeout { get { return _Timeout; } set { _Timeout = value; } }

        /// <summary> Interval in seconds between EnquireLink submition </summary>
        public int EnquireInterval { get { return _EnqInterval; } set { _EnqInterval = value; } }

        /// <summary> Defines maximum bytes length in one part of the concatenated short messages.  The GSM standard defines a maximum of 140 octets for a single short message </summary>
        public int ShortMessageMaxBytes { get { return _ShortMessageMaxBytes; } set { _ShortMessageMaxBytes = value; } }

        /// <summary> Client connection status </summary>
        public ConnectionStatus Status { get { return _Status; } set { _Status = value; } }
        
        /// <summary> The connection mode for this connection </summary>
        public ConnectionModes ConnectionModes { get { return _ConnectionMode; } }

        /// <summary> The encoding for the default DCS </summary>
        public DataCodings DefaultEncoding;

        #endregion

        #region UserDataControl Class

        /// <summary> Provides control support for collections data segments </summary>
        public class UserDataControl
        {
            /// <summary> The number of seqments left to receive </summary>
            public int SegmentsLeftToReceived = 0;

            /// <summary> The user data </summary>
            public UserData UserData = null;

            /// <summary> Constructor </summary>
            /// <param name="segmentsLeftToReceived"></param>
            public UserDataControl(int segmentsLeftToReceived)
            {
                SegmentsLeftToReceived = segmentsLeftToReceived;
                UserData = UserData.Create();
            }
        }

        #endregion

        #region Constructor

        /// <summary> Constructor for ESME support </summary>
        public SmppClient(DataCodings defaultEncoding)
        {
            DefaultEncoding = defaultEncoding;

            // Create the socket client
            _SocketClient = new SocketClient(10485760, 0, this,
                new SocketClient.MESSAGE_HANDLER(MessageHandler),
                new SocketClient.CLOSE_HANDLER(CloseHandler),
                new SocketClient.ERROR_HANDLER(ErrorHandler));
            
            _EnquireLinkTimer = new SynchronousTimer(new SynchronousTimer.SynchronousTimerHandler(SendEnquireLink), null, _EnqInterval);
        }

        /// <summary> Constructor for SMSC support </summary>
        public SmppClient(DataCodings defaultEncoding, SocketClient socketClient)
        {
            DefaultEncoding = defaultEncoding;

            _SocketClient = socketClient;

            // Mark the connection as open
            _Status = ConnectionStatus.Open;
        }
        
        /// <summary> Dispose </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary> Dispose </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    if (_EnquireLinkTimer != null)
                    {
                        _EnquireLinkTimer.Dispose();
                        _EnquireLinkTimer = null;
                    }
                    
                    _SocketClient.Dispose();

                    // Wake up any waiting threads
                    lock (SubmitDone)
                    {
                        foreach (EventData eventData in SubmitDone.Values)
                        {
                            eventData.SignalEvent.Set();
                        }
                    }
                }
                
                _Disposed = true;
            }
        }

        #endregion

        #region Signal Event Methods

        /// <summary> Called to request an event data object for processing </summary>
        /// <returns> AutoResetEvent </returns>
        private EventData RequestEventData()
        {
            EventData eventData = null;

            lock (EventDataList)
            {
                if (EventDataList.Count == 0)
                {
                    eventData = new EventData();
                }
                else
                {
                    eventData = EventDataList.Pop();
                }
            }

            return eventData;
        }

        /// <summary> Called to release an event data object back into the stack </summary>
        /// <param name="eventData"></param>
        private void ReleaseEventData(EventData eventData)
        {
            lock (EventDataList)
            {
                eventData.Release();
                EventDataList.Push(eventData);
            }
        }

        #endregion
        
        #region Handler Methods

        /// <summary> Called when a message is extracted from the socket </summary>
        /// <param name="socket"> The SocketClient object the message came from </param>
        private void MessageHandler(SocketClient socket)
        {
            try
            {
                // Store the raw message data in the buffer
                socket.MessageBuffer.Write(_SocketClient.RawBuffer, 0, _SocketClient.MessageLength);
                
                ProcessReceivedData();
            }

            catch (Exception exception)
            {
                OnError("Message Handler", exception);
            }
        }

        /// <summary> Called when a socket connection is closed </summary>
        /// <param name="socket"> The SocketClient object the message came from </param>
        private void CloseHandler(SocketClient socket)
        {
            try
            {
                _Status = ConnectionStatus.Closed;
                OnDisconnect();
                
                // Wake up this thread if it is waiting
                EnquireDone.Set();
            }
            
            catch (Exception exception)
            {
                OnError("Close Handler", exception);
            }
        }

        /// <summary> Called when a socket error occurs </summary>
        /// <param name="socket"> The SocketClient object the message came from </param>
        /// <param name="exception"> The reason for the error </param>
        private void ErrorHandler(SocketClient socket, Exception exception)
        {
            try
            {
                OnError("Error Handler", exception);
            }
            
            catch
            {
            }
        }
        
        #endregion
        
        #region Synchronous Fire Event Methods

        /// <summary> Called to fire the alert notification event </summary>
        /// <param name="data"></param>
        internal void OnAlertNotification(AlertNotification data)
        {
            try
            {
                if (AlertNotificationEvent != null)
                {
                    AlertNotificationEvent(this, data);
                }
            }
            
            catch
            {
            }
        }

        /// <summary> Called to fire the bind pdu event </summary>
        /// <param name="data"></param>
        /// <returns> CommandStatus </returns>
        internal CommandStatus OnBindSm(BindSm data)
        {
            CommandStatus commandStatus = CommandStatus.ESME_RBINDFAIL;

            try
            {
                if (BindSmEvent != null)
                {
                    commandStatus = BindSmEvent(this, data);

                    if (commandStatus == CommandStatus.ESME_ROK)
                    {
                        _Status = ConnectionStatus.Bound;
                    }
                }
            }
            
            catch
            {
            }

            return commandStatus;
        }
        
        /// <summary> Called to fire the bind response pdu event </summary>
        /// <param name="data"></param>
        internal void OnBindSmResp(BindSmResp data)
        {
            try
            {
                if (BindSmRespEvent != null)
                {
                    BindSmRespEvent(this, data);
                }
            }
            
            catch
            {
            }
        }

        /// <summary> Called to fire the cancel pdu event </summary>
        /// <param name="data"></param>
        /// <returns> CommandStatus </returns>
        internal CommandStatus OnCancelSm(CancelSm data)
        {
            CommandStatus commandStatus = CommandStatus.ESME_RCANCELFAIL;

            try
            {
                if (CancelSmEvent != null)
                {
                    commandStatus = CancelSmEvent(this, data);
                }
            }
            
            catch
            {
            }

            return commandStatus;
        }

        /// <summary> Called to fire the cancel response pdu event </summary>
        /// <param name="data"></param>
        internal void OnCancelSmResp(CancelSmResp data)
        {
            try
            {
                if (CancelSmRespEvent != null)
                {
                    CancelSmRespEvent(this, data);
                }
            }
            
            catch
            {
            }
        }

        /// <summary> Called to fire the connect event </summary>
        /// <param name="bSuccess"></param>
        internal void OnConnect(bool bSuccess)
        {
            try
            {
                if (ConnectEvent != null)
                {
                    ConnectEvent(this, bSuccess);
                }
            }
            
            catch
            {
            }
        }

        /// <summary> Called to fire the data pdu event </summary>
        /// <param name="data"></param>
        /// <param name="messageId"></param>
        /// <returns> CommandStatus </returns>
        internal CommandStatus OnDataSm(DataSm data, out string messageId)
        {
            CommandStatus commandStatus = CommandStatus.ESME_ROK;
            messageId = null;

            try
            {
                if (DataSmEvent != null)
                {
                    commandStatus = DataSmEvent(this, data, out messageId);
                }
            }
            
            catch
            {
            }

            return commandStatus;
        }
        
        /// <summary> Called to fire the data response pdu event </summary>
        /// <param name="data"></param>
        internal void OnDataSmResp(DataSmResp data)
        {
            try
            {
                if (DataSmRespEvent != null)
                {
                    DataSmRespEvent(this, data);
                }
            }
            
            catch
            {
            }
        }

        /// <summary> Called to fire the deliver pdu event </summary>
        /// <param name="data"></param>
        /// <returns> CommandStatus </returns>
        internal CommandStatus OnDeliverSm(DeliverSm data)
        {
            CommandStatus commandStatus = CommandStatus.ESME_ROK;

            try
            {
                if (DeliverSmEvent != null)
                {
                   
                    commandStatus = DeliverSmEvent(this, data);
                }
            }
            
            catch
            {
            }

            return commandStatus;
        }
        
        /// <summary> Called to fire the deliver response pdu event </summary>
        /// <param name="data"></param>
        internal void OnDeliverSmResp(DeliverSmResp data)
        {
            try
            {
                if (DeliverSmRespEvent != null)
                {
                    DeliverSmRespEvent(this, data);
                }
            }
            
            catch
            {
            }
        }

        /// <summary> Called to fire the disconnect event </summary>
        internal void OnDisconnect()
        {
            try
            {
                if (DisconnectEvent != null)
                {
                    DisconnectEvent(this);
                }
            }
            
            catch
            {
            }
        }

        /// <summary> Called to fire the enquire link event </summary>
        /// <param name="data"></param>
        /// <returns> CommandStatus </returns>
        internal CommandStatus OnEnquireLinkSm(EnquireLinkSm data)
        {
            CommandStatus commandStatus = CommandStatus.ESME_ROK;

            try
            {
                if (EnquireLinkSmEvent != null)
                {
                    commandStatus = EnquireLinkSmEvent(this, data);
                }
            }
            
            catch
            {
            }

            return commandStatus;
        }
        
        /// <summary> Called to fire the enquire link resp event </summary>
        /// <param name="data"></param>
        internal void OnEnquireLinkSmResp(EnquireLinkSmResp data)
        {
            try
            {
                if (EnquireLinkSmRespEvent != null)
                {
                    EnquireLinkSmRespEvent(this, data);
                }
            }
            
            catch
            {
            }
        }

        /// <summary> Called to fire the error event </summary>
        /// <param name="Comment"></param>
        internal void OnError(string Comment)
        {
            try
            {
                if (ErrorEvent != null)
                {
                    ErrorEvent(this, Comment, null);
                }
            }
            
            catch
            {
            }
        }

        /// <summary> Called to fire the error event </summary>
        /// <param name="Comment"></param>
        /// <param name="exception"></param>
        internal void OnError(string Comment, Exception exception)
        {
            try
            {
                if (ErrorEvent != null)
                {
                    ErrorEvent(this, Comment, exception);
                }
            }
            
            catch
            {
            }
        }

        /// <summary> Called to fire the generic nack pdu event </summary>
        /// <param name="data"></param>
        internal void OnGenericNackSm(GenericNackSm data)
        {
            try
            {
                if (GenericNackSmEvent != null)
                {
                    GenericNackSmEvent(this, data);
                }
            }
            
            catch
            {
            }
        }

        /// <summary> Called to fire the pdu detail event </summary>
        /// <param name="pduDirectionType"></param>
        /// <param name="pdu"></param>
        internal void OnPduDetails(PduDirectionTypes pduDirectionType, Header pdu)
        {
            try
            {
                if (PduDetailsEvent != null)
                {
                    if (pduDirectionType == PduDirectionTypes.Sent)
                    {
                        // Build the PDU 
                        pdu.PduData = new SmppBuffer(DefaultEncoding, ((IPacket) pdu).GetPDU());
                    }

                    pdu.ExternalId = PduDetailsEvent(this, pduDirectionType, pdu, ((IPduDetails) pdu).Details());
                }
            }
            
            catch
            {
            }
        }

        /// <summary> Called to fire the query pdu event </summary>
        /// <param name="data"></param>
        /// <returns> QuerySmResp </returns>
        internal QuerySmResp OnQuerySm(QuerySm data)
        {
            QuerySmResp querySmResp = QuerySmResp.Create(DefaultEncoding, data, CommandStatus.ESME_RSYSERR);
            querySmResp.MessageId = data.MessageId;

            try
            {
                if (QuerySmEvent != null)
                {
                    QuerySmEvent(this, data, querySmResp);
                }
            }
            
            catch
            {
                querySmResp = QuerySmResp.Create(DefaultEncoding, CommandStatus.ESME_RSYSERR, data.Sequence);
            }

            return querySmResp;
        }

        /// <summary> Called to fire the query resp pdu event </summary>
        /// <param name="data"></param>
        internal void OnQuerySmResp(QuerySmResp data)
        {
            try
            {
                if (QuerySmRespEvent != null)
                {
                    QuerySmRespEvent(this, data);
                }
            }
            
            catch
            {
            }
        }

        /// <summary> Called to fire the submit pdu event </summary>
        /// <param name="data"></param>
        /// <param name="messageId"></param>
        /// <returns> CommandStatus </returns>
        internal CommandStatus OnSubmitSm(SubmitSm data, out string messageId)
        {
            CommandStatus commandStatus = CommandStatus.ESME_RSUBMITFAIL;
            messageId = null;

            try
            {
                if (SubmitSmEvent != null)
                {
                    commandStatus = SubmitSmEvent(this, data, out messageId);
                }
            }
            
            catch
            {
            }

            return commandStatus;
        }

        /// <summary> Called to fire the submit pdu event </summary>
        /// <param name="data"></param>
        internal void OnSubmitSmResp(SubmitSmResp data)
        {
            try
            {
                if (SubmitSmRespEvent != null)
                {
                    SubmitSmRespEvent(this, data);
                }
            }
            
            catch
            {
            }
        }

        /// <summary> Called to fire the submit multi pdu event </summary>
        /// <param name="data"></param>
        /// <param name="messageId"></param>
        /// <returns> CommandStatus </returns>
        internal CommandStatus OnSubmitMultiSm(SubmitMultiSm data, out string messageId)
        {
            CommandStatus commandStatus = CommandStatus.ESME_RSUBMITFAIL;
            messageId = null;

            try
            {
                if (SubmitMultiSmEvent != null)
                {
                    commandStatus = SubmitMultiSmEvent(this, data, out messageId);
                }
            }
            
            catch
            {
            }

            return commandStatus;
        }

        /// <summary> Called to fire the submit multi response pdu event </summary>
        /// <param name="data"></param>
        internal void OnSubmitMultiSmResp(SubmitMultiSmResp data)
        {
            try
            {
                if (SubmitMultiSmRespEvent != null)
                {
                    SubmitMultiSmRespEvent(this, data);
                }
            }
            
            catch
            {
            }
        }

        /// <summary> Called to fire the unbind pdu event </summary>
        /// <param name="data"></param>
        /// <returns> CommandStatus </returns>
        internal CommandStatus OnUnBindSm(UnBindSm data)
        {
            CommandStatus commandStatus = CommandStatus.ESME_RSYSERR;

            try
            {
                if (UnBindSmEvent != null)
                {
                    commandStatus = UnBindSmEvent(this, data);
                }
            }
            
            catch
            {
            }

            return commandStatus;
        }
        
        /// <summary> Called to fire the unbind response pdu event </summary>
        /// <param name="data"></param>
        internal void OnUnBindSmResp(UnBindSmResp data)
        {
            try
            {
                if (UnBindSmRespEvent != null)
                {
                    UnBindSmRespEvent(this, data);
                }
            }
            
            catch
            {
            }
        }

        #endregion
        
        #region Connectivity Methods
        
        /// <summary> Connect to SMSC using hostname and port number </summary>
        /// <param name="hostName"> SMSC hostname </param>
        /// <param name="port"> SMSC port </param>
        /// <returns> A Boolean value indicating true if the client is connected, false if not </returns>
        public bool Connect(string hostName, int port)
        {
            bool retVal = true;
            
            try
            {
                // Connect to the server
                _SocketClient.Connect(hostName, port);
            
                // We are now connected
                _Status = ConnectionStatus.Open;
            }
            
            catch (Exception exception)
            {
                OnError("Failed To Connect", exception);
                retVal = false;
            }
            
            return retVal;
        }

        /// <summary> Disconnect client from SMSC </summary>
        public void Disconnect()
        {
            try
            {
                // Connect to the server
                _SocketClient.Disconnect();

                // We are now disconnected
                _Status = ConnectionStatus.Closed;
            }
            
            catch (Exception exception)
            {
                // We are now disconnected
                _Status = ConnectionStatus.Closed;

                OnError("Failed To Disconnect", exception);
            }
        }
        
        /// <summary> Register client as Transceiver with the SMSC system and request an SMPP session </summary>
        /// <param name="SystemId"> Identifies the ESME system requesting to bind with the SMSC </param>
        /// <param name="Password"> Password used by the SMSC to authenticate the client </param>
        /// <returns> A BindSmResp object </returns>
        public BindSmResp Bind(string SystemId, string Password)
        {
            return Bind(SystemId, Password, ConnectionModes.Transceiver);
        }
        
        /// <summary> Register client with the SMSC system and request an SMPP session </summary>
        /// <param name="SystemId"></param>
        /// <param name="Password"></param>
        /// <param name="mode"></param>
        /// <returns> BindSmResp </returns>
        public BindSmResp Bind(string SystemId, string Password, ConnectionModes mode)
        {
            if (SystemId == null || SystemId.Length == 0)
            {
                throw new ArgumentException("SystemId");
            }

            if (Password == null || Password.Length == 0)
            {
                throw new ArgumentException("Password");
            }

            // Set the connection mode
            _ConnectionMode = mode;

            BindSmResp bindSmResp = BindSmResp.Create(DefaultEncoding);
            
            try
            {
                BindSm bindSm = BindSm.Create(DefaultEncoding);
                
                switch (mode)
                {
                    case ConnectionModes.Receiver:
                        bindSm.Command = CommandSet.BindReceiver;
                        bindSmResp.Command = CommandSet.BindReceiverResp;
                        break;
                        
                    case ConnectionModes.Transmitter:
                        bindSm.Command = CommandSet.BindTransmitter;
                        bindSmResp.Command = CommandSet.BindTransmitterResp;
                        break;
                        
                    default:
                        bindSm.Command = CommandSet.BindTransceiver;
                        bindSmResp.Command = CommandSet.BindTransceiverResp;
                        break;
                }

                if (!_SocketClient.IsAvailable)
                {
                    bindSmResp.Status = CommandStatus.SMPPCLIENT_NOCONN;
                    return bindSmResp;
                }

                bindSm.SystemId = SystemId;
                bindSm.Password = Password;
                bindSm.SystemType = SystemType;
                bindSm.AddrNpi = AddrNpi;
                bindSm.AddrTon = AddrTon;
                bindSm.AddressRange = AddressRange;

                BindDone.Reset();
                GenericDone.Reset();

                WaitHandle[] handles = new WaitHandle[2] { BindDone, GenericDone };

                SendPacket(bindSm);

                int res = WaitHandle.WaitAny(handles, Timeout, false);

                if (res == 0)
                {
                    // Set the response we received
                    bindSmResp = BindSmResp;
                    if (bindSmResp.Status == CommandStatus.ESME_ROK)
                    {
                        _Status = ConnectionStatus.Bound;
                        
                        OnConnect(true);
                    }
                    else
                    {
                        OnConnect(false);
                    }
                }
                else if (res == 1)
                {
                    bindSmResp.Status = CommandStatus.SMPPCLIENT_GENERIC_NACK;
                    
                    OnConnect(false);
                }
                else
                {
                    bindSmResp.Status = CommandStatus.SMPPCLIENT_RCVTIMEOUT;
                    
                    OnConnect(false);
                }
            }
            
            catch (Exception exception)
            {
                OnError("Bind error : ", exception);
                
                bindSmResp.Status = CommandStatus.SMPPCLIENT_UNKNOWNERROR;
            }
            
            return bindSmResp;
        }

        /// <summary> Unbind the client from the SMSC </summary>
        /// <returns> UnBindSmResp </returns>
        public UnBindSmResp UnBind()
        {
            UnBindSmResp unBindSmResp = null;
            
            try
            {
                if (_Status != ConnectionStatus.Bound)
                {
                    return UnBindSmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_NOCONN);
                }

                UnBindSm unBindSm = UnBindSm.Create(DefaultEncoding);

                UnbindDone.Reset();
                GenericDone.Reset();

                WaitHandle[] handles = new WaitHandle[2] { UnbindDone, GenericDone };

                SendPacket(unBindSm);

                int res = WaitHandle.WaitAny(handles, Timeout, false);

                if (res == 0)
                {
                    unBindSmResp = UnBindSmResp;
                    if (unBindSmResp.Status == CommandStatus.ESME_ROK)
                    {
                        _Status = ConnectionStatus.Open;
                    }
                }
                else if (res == 1)
                {
                    unBindSmResp = UnBindSmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_GENERIC_NACK);
                }
                else
                {
                    unBindSmResp = UnBindSmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_RCVTIMEOUT);
                }
            }
            
            catch (Exception exception)
            {
                OnError("UnBind error : ", exception);
                
                unBindSmResp = UnBindSmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_UNKNOWNERROR);
            }
            
            return unBindSmResp;
        }
        
        #endregion
        
        #region Submit Methods
        
        /// <summary> Submit a short message to the SMSC </summary>
        /// <param name="data"></param>
        /// <returns> A SubmitSmResp object </returns>
        public SubmitSmResp Submit(SubmitSm data)
        {
            SubmitSmResp submitSmResp = null;
            EventData eventData = null;
            
            if (_Status != ConnectionStatus.Bound)
            {
                return SubmitSmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_UNBOUND, data.Sequence);
            }

            try
            {
                // Request an event data object for the submit
                eventData = RequestEventData();

                // Add the event data object to the submit done list
                lock (SubmitDone)
                {
                    SubmitDone.Add(data.Sequence, eventData);
                }

                // Send the submit
                SendPacket(data);

                // Wait the response
                bool success = eventData.SignalEvent.WaitOne(5000);

                if (success == true)
                {
                    // Return the response
                    submitSmResp = (SubmitSmResp) eventData.ResponseObject;
                }
                else
                {
                    submitSmResp = SubmitSmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_RCVTIMEOUT, data.Sequence);
                }
            }
            
            catch (Exception exception)
            {
                OnError("Submit error : ", exception);
                
                submitSmResp = SubmitSmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_UNKNOWNERROR, data.Sequence);
            }

            // Remove the submit done object
            lock (SubmitDone)
            {
                SubmitDone.Remove(data.Sequence);
            }

            // Release the event data
            ReleaseEventData(eventData);
            
            return submitSmResp;
        }
        
        /// <summary> Submit a short message to the SMSC</summary>
        /// <param name="data"></param>
        /// <returns> A SubmitMultiResp object </returns>
        public SubmitMultiSmResp Submit(SubmitMultiSm data)
        {
            SubmitMultiSmResp submitMultiSmResp = null;
            EventData eventData = null;

            if (_Status != ConnectionStatus.Bound)
            {
                return SubmitMultiSmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_UNBOUND, data.Sequence);
            }

            try
            {
                // Request an event data object for the submit
                eventData = RequestEventData();

                // Add the event data object to the submit done list
                lock (SubmitDone)
                {
                    SubmitDone.Add(data.Sequence, eventData);
                }

                // Send the submit
                SendPacket(data);

                // Wait the response
                bool success = eventData.SignalEvent.WaitOne(5000);

                if (success == true)
                {
                    // Return the response
                    submitMultiSmResp = (SubmitMultiSmResp) eventData.ResponseObject;
                }
                else
                {
                    submitMultiSmResp = SubmitMultiSmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_RCVTIMEOUT, data.Sequence);
                }
            }
            
            catch (Exception exception)
            {
                OnError("Submit Multi error : ", exception);
                
                submitMultiSmResp = SubmitMultiSmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_UNKNOWNERROR, data.Sequence);
            }

            // Remove the submit done object
            lock (SubmitDone)
            {
                SubmitDone.Remove(data.Sequence);
            }

            // Release the event data
            ReleaseEventData(eventData);

            return submitMultiSmResp;
        }
        
        /// <summary> Submit array of short messages to the SMSC </summary>
        /// <param name="submitSmList"></param>
        /// <returns> A list of SubmitSmResp objects </returns>
        public List<SubmitSmResp> Submit(List<SubmitSm> submitSmList)
        {
            List<SubmitSmResp> list = new List<SubmitSmResp>();
            
            foreach (SubmitSm submitSm in submitSmList)
            {
                SubmitSmResp resp = Submit(submitSm);
                
                list.Add(resp);
                
                if (resp.Status != CommandStatus.ESME_ROK)
                {
                    break;
                }
            }
            
            return list;
        }
        
        /// <summary> Submit array of short messages to the SMSC </summary>
        /// <param name="submitMultiList"></param>
        /// <returns> A list of SubmitMultiResp objects </returns>
        public List<SubmitMultiSmResp> Submit(List<SubmitMultiSm> submitMultiList)
        {
            List<SubmitMultiSmResp> list = new List<SubmitMultiSmResp>();
            
            foreach (SubmitMultiSm submitMulti in submitMultiList)
            {
                SubmitMultiSmResp resp = Submit(submitMulti);
                
                list.Add(resp);
                
                if (resp.Status != CommandStatus.ESME_ROK)
                {
                    break;
                }
            }
            
            return list;
        }
        
        #endregion

        #region Prepare Deliver Methods

        /// <summary> Prepare DeliverSm instances for sending to ESME </summary>
        /// <param name="mode"></param>
        /// <param name="serviceType"></param>
        /// <param name="srcTon"></param>
        /// <param name="srcNpi"></param>
        /// <param name="srcAddr"></param>
        /// <param name="destTon"></param>
        /// <param name="destNpi"></param>
        /// <param name="destAddr"></param>
        /// <param name="submitDataCoding"></param>
        /// <param name="encodeDataCoding"></param>
        /// <param name="message"></param>
        /// <returns> DeliverSm </returns>
        public DeliverSm PrepareDeliver(DeliverMode mode, string serviceType, byte srcTon, byte srcNpi, string srcAddr, byte destTon, byte destNpi,
                                        string destAddr, DataCodings submitDataCoding, DataCodings encodeDataCoding, string message)
        {
            DeliverSm deliverSm = null;

            if (!SmppBuffer.IsSupportedTextDataCoding(encodeDataCoding))
            {
                OnError(string.Format("Unsupported data coding [{0}]", encodeDataCoding));
                encodeDataCoding = DataCodings.Default;
            }
            
            // Standard 160 bytes
            int maxBytes = ShortMessageMaxBytes;
            if (encodeDataCoding == DataCodings.UCS2)
            {
                // Unicode message
                maxBytes = 140;
            }
            
            // Convert the message to a byte array
            byte[] messageBytes = new SmppBuffer(DefaultEncoding, message, encodeDataCoding).Buffer;
            
            switch (mode)
            {
                case DeliverMode.ShortMessage:
                    deliverSm = DeliverSm.Create(DefaultEncoding, serviceType, srcTon, srcNpi, srcAddr, destTon, destNpi, destAddr);
                    deliverSm.DataCoding = submitDataCoding;

                    if (messageBytes.Length > maxBytes)
                    {
                        List<byte> memory = new List<byte>(messageBytes);
                        deliverSm.ShortMessageBytes = memory.GetRange(0, 160).ToArray();
                    }
                    else
                    {
                        deliverSm.ShortMessageBytes = messageBytes;
                    }
                    break;
                    
                case DeliverMode.Payload:
                    deliverSm = DeliverSm.Create(DefaultEncoding, _SystemType, srcTon, srcNpi, srcAddr, destTon, destNpi, destAddr);
                    deliverSm.DataCoding = submitDataCoding;
                    deliverSm.Optional.AddMessagePayload(messageBytes);
                    break;
            }

            return deliverSm;
        }

        #endregion
        
        #region Prepare Submit Methods
        
        /// <summary> Prepare SubmitSm instances for sending with methods Submit or SubmitAsync </summary>
        /// <param name="mode"></param>
        /// <param name="serviceType"></param>
        /// <param name="srcTon"></param>
        /// <param name="srcNpi"></param>
        /// <param name="srcAddr"></param>
        /// <param name="destTon"></param>
        /// <param name="destNpi"></param>
        /// <param name="destAddr"></param>
        /// <param name="submitDataCoding"></param>
        /// <param name="encodeDataCoding"></param>
        /// <param name="message"></param>
        /// <returns> A SubmitSm object </returns>
        public SubmitSm PrepareSubmit(SubmitMode mode, string serviceType, byte srcTon, byte srcNpi, string srcAddr, byte destTon, byte destNpi,
                                      string destAddr, DataCodings submitDataCoding, DataCodings encodeDataCoding, string message)
        {
            SubmitSm submitSm = null;

            if (!SmppBuffer.IsSupportedTextDataCoding(encodeDataCoding))
            {
                OnError(string.Format("Unsupported data coding [{0}]", encodeDataCoding));
                encodeDataCoding = DataCodings.Default;
            }
            
            // Standard 160 bytes
            int maxBytes = ShortMessageMaxBytes;
            if (encodeDataCoding == DataCodings.UCS2)
            {
                // Unicode message
                maxBytes = 140;
            }
            
            // Convert the message to a byte array
            byte[] messageBytes = new SmppBuffer(DefaultEncoding, message, encodeDataCoding).Buffer;
            
            switch (mode)
            {
                case SubmitMode.ShortMessage:
                    submitSm = SubmitSm.Create(DefaultEncoding, serviceType, srcTon, srcNpi, srcAddr, destTon, destNpi, destAddr);
                    submitSm.DataCoding = submitDataCoding;

                    if (messageBytes.Length > maxBytes)
                    {
                        List<byte> memory = new List<byte>(messageBytes);
                        submitSm.ShortMessageBytes = memory.GetRange(0, 160).ToArray();
                    }
                    else
                    {
                        submitSm.ShortMessageBytes = messageBytes;
                    }
                    break;
                    
                case SubmitMode.Payload:
                    submitSm = SubmitSm.Create(DefaultEncoding, _SystemType, srcTon, srcNpi, srcAddr, destTon, destNpi, destAddr);
                    submitSm.DataCoding = submitDataCoding;
                    submitSm.Optional.AddMessagePayload(messageBytes);
                    break;
            }

            return submitSm;
        }

        /// <summary> Prepare SubmitSm instances for sending with methods Submit or SubmitAsync </summary>
        /// <param name="mode"></param>
        /// <param name="serviceType"></param>
        /// <param name="srcTon"></param>
        /// <param name="srcNpi"></param>
        /// <param name="srcAddr"></param>
        /// <param name="destTon"></param>
        /// <param name="destNpi"></param>
        /// <param name="destAddr"></param>
        /// <param name="submitDataCoding"></param>
        /// <param name="encodeDataCoding"></param>
        /// <param name="message"></param>
        /// <returns> List SubmitSm </returns>
        public List<SubmitSm> PrepareSubmitLarge(SubmitMode mode, string serviceType, byte srcTon, byte srcNpi, string srcAddr, byte destTon, byte destNpi,
                                                 string destAddr, DataCodings submitDataCoding, DataCodings encodeDataCoding, string message)
        {
            List<SubmitSm> submitSmList = new List<SubmitSm>();

            if (!SmppBuffer.IsSupportedTextDataCoding(encodeDataCoding))
            {
                OnError(string.Format("Unsupported data coding [{0}]", encodeDataCoding));
                encodeDataCoding = DataCodings.Default;
            }
            
            // Standard 160 bytes
            int maxBytes = ShortMessageMaxBytes;
            if (encodeDataCoding == DataCodings.UCS2)
            {
                // Unicode message
                maxBytes = 140;
            }

            // Convert the message to a byte array
            byte[] messageBytes = new SmppBuffer(DefaultEncoding, message, encodeDataCoding).Buffer;
            
            switch (mode)
            {
                case SubmitMode.ShortMessage:
                    if (messageBytes.Length <= maxBytes)
                    {
                        SubmitSm submitSm = SubmitSm.Create(DefaultEncoding, serviceType, srcTon, srcNpi, srcAddr, destTon, destNpi, destAddr);

                        submitSm.DataCoding = submitDataCoding;
                        submitSm.ShortMessageBytes = messageBytes;

                        submitSmList.Add(submitSm);
                    }
                    else
                    {
                        // Subtract 6 bytes for the UDHI header
                        maxBytes = maxBytes - 6;

                        if (encodeDataCoding == DataCodings.Default       ||
                            encodeDataCoding == DataCodings.ASCII         ||
                            encodeDataCoding == DataCodings.Latin1        ||
                            encodeDataCoding == DataCodings.Latin1Escape  ||
                            encodeDataCoding == DataCodings.DefaultFlashSMS)
                        {
                            maxBytes = Convert.ToInt32(Math.Floor(Convert.ToDouble(maxBytes) * 8 / 7));
                        }

                        byte messageReference = SequenceGenerator.ByteCounter;
                        int sequenceNumber = 1;

                        // Split the message in parts we can send
                        List<byte[]> parts = SmppBuffer.SplitMessageOnParts(messageBytes, maxBytes);

                        foreach (byte[] part in parts)
                        {
                            SubmitSm submitSm = SubmitSm.Create(DefaultEncoding, serviceType, srcTon, srcNpi, srcAddr, destTon, destNpi, destAddr);

                            submitSm.DataCoding = submitDataCoding;
                            submitSm.UserData.Headers.AddConcatenatedShortMessages8bit(DefaultEncoding, messageReference, Convert.ToByte(parts.Count), Convert.ToByte(sequenceNumber));
                            submitSm.ShortMessageBytes = part;
                            submitSmList.Add(submitSm);

                            sequenceNumber++;
                        }
                    }
                    break;
                    
                case SubmitMode.Payload:
                    if (messageBytes.Length <= maxBytes)
                    {
                        SubmitSm submitSm = SubmitSm.Create(DefaultEncoding, _SystemType, srcTon, srcNpi, srcAddr, destTon, destNpi, destAddr);

                        submitSm.DataCoding = submitDataCoding;
                        submitSm.Optional.AddMessagePayload(messageBytes);

                        submitSmList.Add(submitSm);
                    }
                    else
                    {
                        // Subtract 6 bytes for the UDHI header
                        maxBytes = maxBytes - 6;

                        if (encodeDataCoding == DataCodings.Default       ||
                            encodeDataCoding == DataCodings.ASCII         ||
                            encodeDataCoding == DataCodings.Latin1        ||
                            encodeDataCoding == DataCodings.Latin1Escape  ||
                            encodeDataCoding == DataCodings.DefaultFlashSMS)
                        {
                            maxBytes = Convert.ToInt32(Math.Floor(Convert.ToDouble(maxBytes) * 8 / 7));
                        }

                        byte messageReference = SequenceGenerator.ByteCounter;
                        int sequenceNumber = 1;

                        // Split the message in parts we can send
                        List<byte[]> parts = SmppBuffer.SplitMessageOnParts(messageBytes, maxBytes);

                        foreach (byte[] part in parts)
                        {
                            SubmitSm submitSm = SubmitSm.Create(DefaultEncoding, serviceType, srcTon, srcNpi, srcAddr, destTon, destNpi, destAddr);

                            submitSm.DataCoding = submitDataCoding;

                            submitSm.Optional.AddSARReferenceNumber(Convert.ToUInt16(messageReference));
                            submitSm.Optional.AddSARSequenceNumber(Convert.ToByte(sequenceNumber));
                            submitSm.Optional.AddSARTotalSegments(Convert.ToByte(parts.Count));
                            submitSm.Optional.AddMoreMessagesToSend(sequenceNumber < parts.Count);
                            submitSm.Optional.AddMessagePayload(part);

                            submitSmList.Add(submitSm);

                            sequenceNumber++;
                        }
                    }
                    break;
            }

            // All messages need the same validity period
            foreach (SubmitSm submitSm in submitSmList)
            {
                submitSm.ValidityPeriod = DateTime.Now.AddDays(2).ToString("yyMMddhhmmss000+");
            }

            return submitSmList;
        }
        
        /// <summary> Prepare SubmitMulti instances for sending with methods Submit or SubmitAsync </summary>
        /// <param name="mode"></param>
        /// <param name="serviceType"></param>
        /// <param name="srcTon"></param>
        /// <param name="srcNpi"></param>
        /// <param name="srcAddr"></param>
        /// <param name="destList"></param>
        /// <param name="submitDataCoding"></param>
        /// <param name="encodeDataCoding"></param>
        /// <param name="message"></param>
        /// <returns> SubmitMulti </returns>
        public SubmitMultiSm PrepareSubmit(SubmitMode mode, string serviceType, byte srcTon, byte srcNpi, string srcAddr, List<DestinationAddress> destList,
                                         DataCodings submitDataCoding, DataCodings encodeDataCoding, string message)
        {
            SubmitMultiSm submitMulti = null;

            if (!SmppBuffer.IsSupportedTextDataCoding(encodeDataCoding))
            {
                OnError(string.Format("Unsupported data coding [{0}]", encodeDataCoding));
                encodeDataCoding = DataCodings.Default;
            }
            
            // Standard 160 bytes
            int maxBytes = ShortMessageMaxBytes;
            if (encodeDataCoding == DataCodings.UCS2)
            {
                // Unicode message
                maxBytes = 140;
            }
            
            // Convert the message to a byte array
            byte[] messageBytes = new SmppBuffer(DefaultEncoding, message, encodeDataCoding).Buffer;

            switch (mode)
            {
                case SubmitMode.ShortMessage:
                    submitMulti = SubmitMultiSm.Create(DefaultEncoding, serviceType, srcTon, srcNpi, srcAddr, destList);
                    submitMulti.DataCoding = submitDataCoding;

                    if (messageBytes.Length > maxBytes)
                    {
                        List<byte> memory = new List<byte>(messageBytes);
                        submitMulti.ShortMessageBytes = memory.GetRange(0, 160).ToArray();
                    }
                    else
                    {
                        submitMulti.ShortMessageBytes = messageBytes;
                    }
                    break;
                    
                case SubmitMode.Payload:
                    submitMulti = SubmitMultiSm.Create(DefaultEncoding, _SystemType, srcTon, srcNpi, srcAddr, destList);
                    submitMulti.DataCoding = submitDataCoding;
                    submitMulti.Optional.AddMessagePayload(messageBytes);
                    break;
            }

            return submitMulti;
        }
        
        /// <summary> Prepare SubmitSm instances for sending as wap push </summary>
        /// <param name="transactionId"></param>
        /// <param name="serviceType"></param>
        /// <param name="srcTon"></param>
        /// <param name="srcNpi"></param>
        /// <param name="srcAddr"></param>
        /// <param name="destTon"></param>
        /// <param name="destNpi"></param>
        /// <param name="destAddr"></param>
        /// <param name="url"></param>
        /// <param name="title"></param>
        /// <returns> A SubmitSm object </returns>
        public SubmitSm PrepareWapPush(byte transactionId, string serviceType, byte srcTon, byte srcNpi, string srcAddr, byte destTon,
                                       byte destNpi, string destAddr, string url, string title)
        {
            SubmitSm sp = null;

            SmppBuffer sb = new SmppBuffer(DefaultEncoding);

            sb.AddByte(transactionId);
            sb.AddByte(0x06);  //PDU Type: Push
            sb.AddByte(0x01);  //Header Length
            sb.AddByte(0xAE); // application/vnd.wap.sic 0x24 ^ 0x80

            // WAP Binary XML
            sb.AddByte(0x02);  //Version 1.2
            sb.AddByte(0x05);  //Public Identifier
            sb.AddByte(0x6a);  //Character Set: utf-8
            sb.AddByte(0x00);  //String table 0 bytes

            sb.AddByte(0x45);  //(.C) <si>
            sb.AddByte(0xc6);  //(AC) <indication  

            string urlTmp = url;
            if (url.StartsWith("http://www."))
            {
                sb.AddByte(0x0d);
                urlTmp = url.Remove(0, "http://www.".Length);
            }
            else if (url.StartsWith("http://"))
            {
                sb.AddByte(0x0c);
                urlTmp = url.Remove(0, "http://".Length);
            }
            else if (url.StartsWith("https://www."))
            {
                sb.AddByte(0x0f);
                urlTmp = url.Remove(0, "https://www.".Length);
            }
            else if (url.StartsWith("https://"))
            {
                sb.AddByte(0x0e);
                urlTmp = url.Remove(0, "https://".Length);
            }
            else
            {
                sb.AddByte(0x0b);
            }
            
            string[] parts = Regex.Split(urlTmp, @"(\.com/|\.org/\.edu/|\.net/)");
            foreach (string part in parts)
            {
                if (part == ".com/")
                {
                    sb.AddByte(0x85);
                }
                else if (part == ".edu/")
                {
                    sb.AddByte(0x86);
                }
                else if (part == ".net/")
                {
                    sb.AddByte(0x87);
                }
                else if (part == ".org/")
                {
                    sb.AddByte(0x88);
                }
                else
                {
                    sb.AddEncodedString(part, DataCodings.Octets);
                }
            }
            
            sb.AddByte(0x07);  // action='signal-medium'
            sb.AddByte(0x01);  // END attribute list
            sb.AddEncodedString(title, DataCodings.Octets);
            
            sb.AddByte(0x01);  // END </indication>
            sb.AddByte(0x01);  // END </si>

            if (sb.Buffer.Length < 255 - 7)
            {
                sp = SubmitSm.Create(DefaultEncoding, serviceType, srcTon, srcNpi, srcAddr, destTon, destNpi, destAddr);

                sp.EsmClass = 0x40;
                sp.DataCoding = DataCodings.Class1ME8Bit;
                sp.ShortMessageBytes = sb.Buffer;
                
                sp.UserData.Headers.Add(
                    InformationElementIdentifiers.ApplicationPortAddressingScheme16bit,
                    new byte[] { 0x0b, 0x84, 0x23, 0xf0 } );
            }
            else
            {
                throw new ArgumentException("Message too large to send", "message_length");
            }
            
            return sp;
        }

        #endregion
        
        #region Cancel Methods
        
        /// <summary> Cancel previously submitted short message that is still pending delivery </summary>
        /// <param name="serviceType"></param>
        /// <param name="messageId"></param>
        /// <param name="srcTon"></param>
        /// <param name="srcNpi"></param>
        /// <param name="srcAddr"></param>
        /// <param name="dstTon"></param>
        /// <param name="dstNpi"></param>
        /// <param name="dstAddr"></param>
        /// <returns> CancelSmResp </returns>
        public CancelSmResp Cancel(string serviceType, string messageId, byte srcTon, byte srcNpi, string srcAddr, byte dstTon, byte dstNpi, string dstAddr)
        {
            return Cancel(CancelSm.Create(DefaultEncoding, serviceType, messageId, srcTon, srcNpi, srcAddr, dstTon, dstNpi, dstAddr));
        }
        
        /// <summary> Cancel previously submitted short message that is still pending delivery </summary>
        /// <param name="data"></param>
        /// <returns> CancelSmResp </returns>
        public CancelSmResp Cancel(CancelSm data)
        {
            CancelSmResp resp = CancelSmResp.Create(DefaultEncoding, CommandStatus.ESME_ROK);

            if (_Status != ConnectionStatus.Bound)
            {
                resp = CancelSmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_UNBOUND);
                return resp;
            }

            try
            {
                SendPacket(data);
            }
            
            catch (Exception exception)
            {
                OnError("Cancel error : ", exception);
                
                resp = CancelSmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_UNKNOWNERROR);
            }
            
            return resp;
        }
        
        #endregion
        
        #region Query Methods
        
        /// <summary> Query the status of a previously submitted short message </summary>
        /// <param name="messageId"></param>
        /// <param name="sourceTon"></param>
        /// <param name="sourceNpi"></param>
        /// <param name="sourceAddr"></param>
        /// <returns> QuerySmResp </returns>
        public QuerySmResp Query(string messageId, byte sourceTon, byte sourceNpi, string sourceAddr)
        {
            return Query(QuerySm.Create(DefaultEncoding, messageId, sourceTon, sourceNpi, sourceAddr));
        }

        /// <summary> Query the status of a previously submitted short message </summary>
        /// <param name="data"></param>
        /// <returns> QuerySmResp </returns>
        public QuerySmResp Query(QuerySm data)
        {
            QuerySmResp resp = QuerySmResp.Create(DefaultEncoding, data, CommandStatus.ESME_ROK);

            if (_Status != ConnectionStatus.Bound)
            {
                resp = QuerySmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_UNBOUND);
                return resp;
            }

            try
            {
                SendPacket(data);
            }
            
            catch (Exception exception)
            {
                OnError("Query error : ", exception);
                
                resp = QuerySmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_UNKNOWNERROR);
            }

            return resp;
        }

        #endregion
        
        #region EnquireLink Methods
        
        /// <summary> Called by the timer when it fires </summary>
        /// <param name="stateInfo"></param>
        /// <param name="theTimer"></param>
        private void SendEnquireLink(object stateInfo, SynchronousTimer theTimer)
        {
            try
            {
                if (_Status != ConnectionStatus.Bound)
                {
                    OnError("SendEnquireLink: Connection Not Established or Bound");
                    return;
                }
                
                TimeSpan diff;
                lock (_LastPduLock)
                {
                    diff = DateTime.Now - _LastPdu;
                }
                
                // If it has been more than 40 seconds since we talked with the server do this
                if (diff.TotalSeconds > 40)
                {
                    EnquireLinkSmResp res = PerformEnquireLink();
                    if (res.Status != CommandStatus.ESME_ROK)
                    {
                        OnError("SendEnquireLink: It has taken more than " + Timeout + " milliseconds to get Enquire Link response");
                        
                        Disconnect();
                        
                        return;
                    }
                }
            }
            
            catch (Exception exception)
            {
                OnError("Unhandled Exception", exception);
                Disconnect();
            }
        }

        /// <summary> Called to enquire the SMPP about the connection </summary>
        /// <returns> EnquireLinkResp </returns>
        public EnquireLinkSmResp PerformEnquireLink()
        {
            EnquireLinkSmResp resp = null;

            EnquireDone.Reset();
            
            EnquireLinkSm data = EnquireLinkSm.Create(DefaultEncoding);
            
            OnEnquireLinkSm(data);

            SendPacket(data);

            if (EnquireDone.WaitOne(Timeout, false))
            {
                resp = EnquireLinkSmResp;
            }
            else
            {
                resp = EnquireLinkSmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_RCVTIMEOUT, 0);
            }

            return resp;
        }
        
        #endregion
        
        #region Send Methods

        /// <summary> Called to send a message to the SMPP server </summary>
        /// <param name="packet"></param>
        public void SendPacket(IPacket packet)
        {
            try
            {
                OnPduDetails(PduDirectionTypes.Sent, (Header) packet);

                _SocketClient.Send(packet.GetPDU());
            }
            
            catch (Exception exception)
            {
                OnError("SendPacket", exception);
            }
        }
        
        #endregion
        
        #region ProcessReceivedData Method
        
        /// <summary> Called to process received messages from the SMPP server </summary>
        public void ProcessReceivedData()
        {
            try
            {
                // Copy the existing pdu data into a SmppBuffer for processing
                SmppBuffer sb = new SmppBuffer(DefaultEncoding, _SocketClient.MessageBuffer.ToArray());
                int offset = 0;
                
                // While we have data to process
                while (offset < sb.Length)
                {
                    // Where are we starting in the data
                    int startOffset = offset;
                    
                    // Do we have enough data for the header
                    if ((sb.Length - offset) < Header.HEADER_LENGTH)
                    {
                        // Copy the data we have left back into the buffer
                        _SocketClient.MessageBuffer.SetLength(0);
                        _SocketClient.MessageBuffer.Write(sb.Buffer, offset, sb.Length - offset);
                        _SocketClient.MessageBuffer.Seek(0, SeekOrigin.End);
                        
                        return;
                    }

                    // Extract the header
                    Header hd = sb.PeekHeader(offset);
                    if (hd == null)
                    {
                        _SocketClient.MessageBuffer.SetLength(0);
                        OnError("Unable To Extract Header, throw all data in buffer away");
                        
                        return;
                    }
                    
                    // Convert the length to an int
                    // The command length includes the header as well
                    int commandLength = Convert.ToInt32(hd.Length);
                    
                    // Do we have all the data for the command we are receiving
                    if (commandLength + offset > sb.Length)
                    {
                        // Copy the data we have left back into the buffer
                        _SocketClient.MessageBuffer.SetLength(0);
                        _SocketClient.MessageBuffer.Write(sb.Buffer, offset, sb.Length - offset);
                        _SocketClient.MessageBuffer.Seek(0, SeekOrigin.End);
                        
                        return;
                    }
                    
                    #region Process Command
                    
                    switch (hd.Command)
                    {
                        case CommandSet.AlertNotification:
                            #region AlertNotification

                            AlertNotification alertn = AlertNotification.Create(DefaultEncoding, sb, ref offset);
                            
                            OnPduDetails(PduDirectionTypes.Received, alertn);
                            OnAlertNotification(alertn);
                            
                            break;

                            #endregion
                        
                        case CommandSet.BindTransceiverResp:
                        case CommandSet.BindReceiverResp:
                        case CommandSet.BindTransmitterResp:
                            #region Bind Response

                            BindSmResp = BindSmResp.Create(DefaultEncoding, sb, ref offset);
                            if (BindSmResp == null)
                            {
                                BindSmResp = BindSmResp.Create(DefaultEncoding, hd);
                                BindSmResp.Status = CommandStatus.SMPPCLIENT_UNEXPRESP;
                            }
                            
                            OnPduDetails(PduDirectionTypes.Received, BindSmResp);
                            OnBindSmResp(BindSmResp);
                            
                            BindDone.Set();
                            break;

                            #endregion

                        case CommandSet.BindTransceiver:
                        case CommandSet.BindReceiver:
                        case CommandSet.BindTransmitter:
                            #region Bind

                            BindSm bindSm = BindSm.Create(DefaultEncoding, sb, ref offset);
                            if (bindSm != null)
                            {
                                OnPduDetails(PduDirectionTypes.Received, bindSm);
                                CommandStatus commandStatus = OnBindSm(bindSm);

                                CommandSet commandSet = (bindSm.Command == CommandSet.BindTransmitter) ? CommandSet.BindTransmitterResp :
                                                        (bindSm.Command == CommandSet.BindReceiver) ? CommandSet.BindReceiverResp : CommandSet.BindTransceiverResp;
                                
                                SendPacket(BindSmResp.Create(DefaultEncoding, bindSm, commandSet, commandStatus, "AS"));
                            }
                            break;

                            #endregion
                        
                        case CommandSet.CancelSm:
                            #region Cancel

                            CancelSm cancelSm = CancelSm.Create(DefaultEncoding, sb, ref offset);
                            if (cancelSm != null)
                            {
                                OnPduDetails(PduDirectionTypes.Received, cancelSm);
                                CommandStatus commandStatus = OnCancelSm(cancelSm);

                                SendPacket(CancelSmResp.Create(DefaultEncoding, cancelSm, commandStatus));
                            }
                            else
                            {
                                SendPacket(CancelSmResp.Create(DefaultEncoding, CommandStatus.ESME_RSYSERR, hd.Sequence));

                                OnError("Invalid CANCEL_SM packet");
                            }
                            break;

                            #endregion

                        case CommandSet.CancelSmResp:
                            #region Cancel Response

                            CancelSmResp cancelResp = CancelSmResp.Create(DefaultEncoding, sb, ref offset);
                            if (cancelResp == null)
                            {
                                cancelResp = CancelSmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_UNEXPRESP);
                            }
                            
                            OnPduDetails(PduDirectionTypes.Received, cancelResp);
                            OnCancelSmResp(cancelResp);
                            break;

                            #endregion
                        
                        case CommandSet.DataSm:
                            #region Data Received

                            DataSm dataSm = DataSm.Create(DefaultEncoding, sb, ref offset);
                            if (dataSm != null)
                            {
                                OnPduDetails(PduDirectionTypes.Received, dataSm);

                                string messageId = null;
                                CommandStatus commandStatus = OnDataSm(dataSm, out messageId);

                                SendPacket(DataSmResp.Create(DefaultEncoding, dataSm, commandStatus, messageId));
                            }
                            else
                            {
                                SendPacket(DataSmResp.Create(DefaultEncoding, CommandStatus.ESME_RSYSERR, hd.Sequence));

                                OnError("Invalid DATA_SM packet");
                            }
                            break;

                            #endregion
                        
                        case CommandSet.DataSmResp:
                            #region Data Response

                            DataSmResp dataSmResp = DataSmResp.Create(DefaultEncoding, sb, ref offset);
                            if (dataSmResp == null)
                            {
                                dataSmResp = DataSmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_UNEXPRESP);
                            }
                            
                            OnPduDetails(PduDirectionTypes.Received, dataSmResp);
                            OnDataSmResp(dataSmResp);
                            break;

                            #endregion

                        case CommandSet.DeliverSm:
                            #region Deliver Received

                            DeliverSm deliverSm = DeliverSm.Create(DefaultEncoding, sb, ref offset);
                            if (deliverSm != null)
                            {
                                OnPduDetails(PduDirectionTypes.Received, deliverSm);
                                CommandStatus commandStatus = OnDeliverSm(deliverSm);

                                SendPacket(DeliverSmResp.Create(DefaultEncoding, deliverSm, commandStatus));
                            }
                            else
                            {
                                SendPacket(DeliverSmResp.Create(DefaultEncoding, CommandStatus.ESME_RSYSERR, hd.Sequence));

                                OnError("Invalid DELIVER_SM packet");
                            }
                            break;

                            #endregion

                        case CommandSet.DeliverSmResp:
                            #region Deliver Resp Received

                            DeliverSmResp deliverSmResp = DeliverSmResp.Create(DefaultEncoding, sb, ref offset);
                            if (deliverSmResp == null)
                            {
                                deliverSmResp = DeliverSmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_UNEXPRESP);
                            }

                            OnPduDetails(PduDirectionTypes.Received, deliverSmResp);
                            OnDeliverSmResp(deliverSmResp);
                            break;

                            #endregion

                        case CommandSet.EnquireLink:
                            #region EnquireLink

                            EnquireLinkSm enquireLink = EnquireLinkSm.Create(DefaultEncoding, sb, ref offset);
                            if (enquireLink != null)
                            {
                                OnPduDetails(PduDirectionTypes.Received, enquireLink);
                                CommandStatus commandStatus = OnEnquireLinkSm(enquireLink);

                                SendPacket(EnquireLinkSmResp.Create(DefaultEncoding, enquireLink, commandStatus));
                            }
                            else
                            {
                                SendPacket(EnquireLinkSmResp.Create(DefaultEncoding, CommandStatus.ESME_RSYSERR, hd.Sequence));

                                OnError("Invalid ENQUIRELINK packet");
                            }

                            break;

                            #endregion
                        
                        case CommandSet.EnquireLinkResp:
                            #region EnquireLink Response

                            EnquireLinkSmResp = EnquireLinkSmResp.Create(DefaultEncoding, sb, ref offset);
                            if (EnquireLinkSmResp == null)
                            {
                                EnquireLinkSmResp = EnquireLinkSmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_UNEXPRESP, 0);
                            }
                            
                            OnPduDetails(PduDirectionTypes.Received, EnquireLinkSmResp);
                            OnEnquireLinkSmResp(EnquireLinkSmResp);

                            EnquireDone.Set();
                            break;
                            
                            #endregion
                        
                        case CommandSet.GenericNack:
                            #region GenericNack Received

                            GenericNackSm nack = GenericNackSm.Create(DefaultEncoding, sb, ref offset);
                            if (nack == null)
                            {
                                nack = GenericNackSm.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_UNEXPRESP, hd.Sequence);
                            }
                            
                            OnPduDetails(PduDirectionTypes.Received, nack);
                            OnGenericNackSm(nack);

                            GenericDone.Set();
                            break;

                            #endregion
                        
                        case CommandSet.QuerySm:
                            #region Query Received

                            QuerySm querySm = QuerySm.Create(DefaultEncoding, sb, ref offset);
                            if (querySm != null)
                            {
                                OnPduDetails(PduDirectionTypes.Received, querySm);
                                SendPacket(OnQuerySm(querySm));
                            }
                            else
                            {
                                SendPacket(QuerySmResp.Create(DefaultEncoding, CommandStatus.ESME_RSYSERR, hd.Sequence));

                                OnError("Invalid QUERY_SM packet");
                            }
                            break;

                            #endregion

                        case CommandSet.QuerySmResp:
                            #region Query Response

                            QuerySmResp querySmResp = QuerySmResp.Create(DefaultEncoding, sb, ref offset);
                            if (querySmResp == null)
                            {
                                querySmResp = QuerySmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_UNEXPRESP);
                            }
                            
                            OnPduDetails(PduDirectionTypes.Received, querySmResp);
                            OnQuerySmResp(querySmResp);
                            break;

                            #endregion
                        
                        case CommandSet.SubmitSm:
                            #region Submit Received

                            SubmitSm submitSm = SubmitSm.Create(DefaultEncoding, sb, ref offset);
                            if (submitSm != null)
                            {
                                OnPduDetails(PduDirectionTypes.Received, submitSm);

                                string messageId = null;
                                CommandStatus commandStatus = OnSubmitSm(submitSm, out messageId);

                                SendPacket(SubmitSmResp.Create(DefaultEncoding, submitSm, commandStatus, messageId));
                            }
                            else
                            {
                                SendPacket(SubmitSmResp.Create(DefaultEncoding, CommandStatus.ESME_RSYSERR, hd.Sequence));

                                OnError("Invalid SUBMIT_SM packet");
                            }
                            break;

                            #endregion

                        case CommandSet.SubmitSmResp:
                            #region Submit Response

                            SubmitSmResp submitSmResp = SubmitSmResp.Create(DefaultEncoding, sb, ref offset);
                            if (submitSmResp == null)
                            {
                                submitSmResp = SubmitSmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_UNEXPRESP, submitSmResp.Sequence);
                            }

                            OnPduDetails(PduDirectionTypes.Received, submitSmResp);
                            OnSubmitSmResp(submitSmResp);

                            lock (SubmitDone)
                            {
                                if (SubmitDone.ContainsKey(submitSmResp.Sequence) == true)
                                {
                                    EventData eventData = SubmitDone[submitSmResp.Sequence];
                                    eventData.ResponseObject = submitSmResp;
                                    eventData.SignalEvent.Set();
                                }
                            }
                            break;

                            #endregion
                        
                        case CommandSet.SubmitMultiSm:
                            #region SubmitMulti Received

                            SubmitMultiSm submitMultiSm = SubmitMultiSm.Create(DefaultEncoding, sb, ref offset);
                            if (submitMultiSm != null)
                            {
                                OnPduDetails(PduDirectionTypes.Received, submitMultiSm);

                                string messageId = null;
                                CommandStatus commandStatus = OnSubmitMultiSm(submitMultiSm, out messageId);

                                SendPacket(SubmitMultiSmResp.Create(DefaultEncoding, submitMultiSm, commandStatus, messageId));
                            }
                            else
                            {
                                SendPacket(SubmitMultiSmResp.Create(DefaultEncoding, CommandStatus.ESME_RSYSERR, hd.Sequence));

                                OnError("Invalid SUBMIT_MULTI_SM packet");
                            }
                            break;

                            #endregion
                                
                        case CommandSet.SubmitMultiSmResp:
                            #region SubmitMulti Response

                            SubmitMultiSmResp submitMultiSmResp = SubmitMultiSmResp.Create(DefaultEncoding, sb, ref offset);
                            if (submitMultiSmResp == null)
                            {
                                submitMultiSmResp = SubmitMultiSmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_UNEXPRESP, submitMultiSmResp.Sequence);
                            }

                            OnPduDetails(PduDirectionTypes.Received, submitMultiSmResp);
                            OnSubmitMultiSmResp(submitMultiSmResp);

                            lock (SubmitDone)
                            {
                                if (SubmitDone.ContainsKey(submitMultiSmResp.Sequence) == true)
                                {
                                    EventData eventData = SubmitDone[submitMultiSmResp.Sequence];
                                    eventData.ResponseObject = submitMultiSmResp;
                                    eventData.SignalEvent.Set();
                                }
                            }
                            break;

                            #endregion

                        case CommandSet.Unbind:
                            #region Unbind

                            UnBindSm unBindSm = UnBindSm.Create(DefaultEncoding, sb, ref offset);
                            if (unBindSm != null)
                            {
                                OnPduDetails(PduDirectionTypes.Received, unBindSm);
                                
                                SendPacket(UnBindSmResp.Create(DefaultEncoding, unBindSm, CommandStatus.ESME_ROK));
                                OnUnBindSm(unBindSm);
                            }
                            else
                            {
                                SendPacket(UnBindSmResp.Create(DefaultEncoding, CommandStatus.ESME_RSYSERR, unBindSm.Sequence));

                                OnError("Invalid UNBIND_SM packet");
                            }
                            break;

                            #endregion
                            
                        case CommandSet.UnbindResp:
                            #region Unbind Response

                            UnBindSmResp = UnBindSmResp.Create(DefaultEncoding, sb, ref offset);
                            if (UnBindSmResp == null)
                            {
                                UnBindSmResp = UnBindSmResp.Create(DefaultEncoding, CommandStatus.SMPPCLIENT_UNEXPRESP);
                            }
                            
                            OnPduDetails(PduDirectionTypes.Received, UnBindSmResp);
                            OnUnBindSmResp(UnBindSmResp);

                            UnbindDone.Set();
                            break;

                            #endregion
  
                        default:
                            offset++;
                            OnError("Undefined packet received : " + hd.Command.ToString() + " with status " + hd.Status.ToString());
                            break;
                    }
                    
                    #endregion
                    
                    // Move the offset past this command
                    offset = startOffset + commandLength;
                }
                
                // Clear the buffer we processed everything
                _SocketClient.MessageBuffer.SetLength(0);
                
                // Mark we processed a pdu
                lock (_LastPduLock)
                {
                    _LastPdu = DateTime.Now;
                }
            }
            
            catch (Exception exception)
            {
                _SocketClient.MessageBuffer.SetLength(0);
                OnError(exception.ToString());
            }
        }

        #endregion
    }
}
