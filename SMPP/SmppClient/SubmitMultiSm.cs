#region Namespaces

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> SubmitMulti PDU to submit an SMPP message for delivery to multiple
    /// recipients or to one or more Distribution Lists </summary>
    public class SubmitMultiSm : Header, IPacket, IPduDetails
    {
        #region Private Properties

        /// <summary> Indicates how long the message is valid. Set to NULL to request the SMSC default validity period. Format 'YYMMDDhhmmsstnnp </summary>
        private string _ValidityPeriod;

        #endregion

        #region Public Properties

        /// <summary> Application service associated with the message </summary>
        public string ServiceType { get; set; }

        /// <summary> Source address type of number </summary>
        public byte SourceTon { get; set; }

        /// <summary> Source address numbering plan indicator </summary>
        public byte SourceNpi { get; set; }

        /// <summary> Source phone number </summary>
        public string SourceAddr { get; set; }
        
        /// <summary> Contains one or more SME addresses or/and Distribution List names </summary>
        public List<DestinationAddress> DestinationAddresses { get; set; }

        /// <summary> Indicates Message Mode and Message Type </summary>
        public byte EsmClass { get; set; }

        /// <summary> Indicates which protocol to use </summary>
        public byte ProtocolId { get; set; }

        /// <summary> Indicate the priority level of the message </summary>
        public byte PriorityFlag { get; set; }

        /// <summary> The date and time the message should be delivered. Set to NULL for immediate message delivery </summary>
        public string ScheduleDeliveryTime { get; set; }

        /// <summary> Indicates how long the message is valid. Set to NULL to request the SMSC default validity period. Format 'YYMMDDhhmmsstnnp </summary>
        public string ValidityPeriod
        {
            get
            {
                return _ValidityPeriod;
            }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (!Regex.IsMatch(value, "^[0-9][0-9][0-1][0-9][0-3][0-9][0-2][0-9][0-5][0-9][0-5][0-9][0-9][0-4][0-9][\\+\\-R]$"))
                    {
                        throw new System.FormatException("Incorrect format. 'YYMMDDhhmmsstnnp'");
                    }
                }

                _ValidityPeriod = value;
            }
        }

        /// <summary> Indicator to signify if an SMSC delivery receipt or an SME acknowledgement is required </summary>
        public byte RegisteredDelivery { get; set; }

        /// <summary> Requests the SMSC to replace a previously submitted message, that is still pending delivery </summary>
        public byte ReplaceIfPresent { get; set; }

        /// <summary> Indicates the encoding scheme of the payload data </summary>
        public DataCodings DataCoding { get; set; }

        /// <summary> Indicates the short message to send from a list of predefined (‘canned’) short messages stored on the SMSC. If not using an SMSC canned message, set to NULL </summary>
        public byte DefaultMsgId { get; set; }
        
        /// <summary> Short Message data length </summary>
        public byte ShortMessageLength { get; set; }

        /// <summary> The smpp data packet </summary>
        public SmppBuffer UserDataBuffer { get; set; }

        /// <summary> The user data portion of the data packet </summary>
        public UserData UserData { get; set; }

        /// <summary> Optional Parameters </summary>
        public TLVCollection Optional { get; set; }

        /// <summary> Short message text in identified data coding </summary>
        public string ShortMessage
        {
            get
            {
                return UserData.ShortMessageText(DefaultEncoding, DataCoding);
            }
            
            set
            {
                UserData.ShortMessage = new SmppBuffer(DefaultEncoding, value, DataCoding).Buffer;
            }
        }

        /// <summary> Up to 254 octets of short message user data. The exact physical limit for short_message size may vary according to the underlying network </summary>
        public byte[] ShortMessageBytes { get { return UserData.ShortMessage; } set { UserData.ShortMessage = value; } }

        /// <summary> Indicates message mode associated with the short message </summary>
        public MessageModes MessageMode { get { return (MessageModes) (EsmClass & 0x03); } set { EsmClass |= (byte) value; } }

        /// <summary> Indicates message type associated with the short message </summary>
        public MessageTypes MessageType { get { return (MessageTypes) (EsmClass & 0x3c); } set { EsmClass |= (byte) value; } }

        /// <summary> Indicates GSM Network Specific Features associated with the short message </summary>
        public GSMSpecificFeatures MessageFeature { get { return (GSMSpecificFeatures) (EsmClass & 0xc0); } }
        
        /// <summary> SMSC Delivery Receipt </summary>
        public SMSCDeliveryReceipt SMSCReceipt { get { return (SMSCDeliveryReceipt) (RegisteredDelivery & 0x03); } set { RegisteredDelivery |= (byte) value; } }

        /// <summary> SME originated Acknowledgement </summary>
        public SMEAcknowledgement Acknowledgement { get { return (SMEAcknowledgement) (RegisteredDelivery & 0x0C); } set { RegisteredDelivery |= (byte) value; } }

        /// <summary> Intermediate Notification </summary>
        public IntermediateNotification Notification { get { return (IntermediateNotification) (RegisteredDelivery & 0x10); } set { RegisteredDelivery |= (byte) value; } }

        #endregion

        #region Constructor
        
        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        private SubmitMultiSm(DataCodings defaultEncoding) : base(defaultEncoding, CommandSet.SubmitMultiSm, SequenceGenerator.Counter)
        {
            DestinationAddresses = new List<DestinationAddress>();

            Optional = new TLVCollection();
            UserData = UserData.Create();
        }

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="serviceType"></param>
        /// <param name="sourceTon"></param>
        /// <param name="sourceNpi"></param>
        /// <param name="sourceAddr"></param>
        /// <param name="destinationAddresses"></param>
        private SubmitMultiSm(DataCodings defaultEncoding, string serviceType, byte sourceTon, byte sourceNpi, string sourceAddr, List<DestinationAddress> destinationAddresses) : base(defaultEncoding, CommandSet.SubmitMultiSm, SequenceGenerator.Counter)
        {
            Optional = new TLVCollection();
            UserData = UserData.Create();

            ServiceType = serviceType;
            SourceTon = sourceTon;
            SourceNpi = sourceNpi;
            SourceAddr = sourceAddr;
            DestinationAddresses = destinationAddresses;
        }
        
        #endregion
        
        #region Factory Methods
        
        /// <summary> Called to create a SubmitMulti object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="serviceType"></param>
        /// <param name="srcTon"></param>
        /// <param name="srcNpi"></param>
        /// <param name="srcAddr"></param>
        /// <param name="destList"></param>
        /// <returns> SubmitMulti </returns>
        public static SubmitMultiSm Create(DataCodings defaultEncoding, string serviceType, byte srcTon, byte srcNpi, string srcAddr, List<DestinationAddress> destList)
        {
            return new SubmitMultiSm(defaultEncoding, serviceType, srcTon, srcNpi, srcAddr, destList);
        }

        /// <summary> Called to create a SubmitMulti object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <returns> SubmitMulti </returns>
        public static SubmitMultiSm Create(DataCodings defaultEncoding, SmppBuffer buf, ref int offset)
        {
            SubmitMultiSm submitMulti = new SubmitMultiSm(defaultEncoding);
            
            try
            {
                int start_offset = offset;

                buf.ExtractHeader(submitMulti, ref offset);

                submitMulti.ServiceType = buf.ExtractCString(ref offset);
                submitMulti.SourceTon = buf.ExtractByte(ref offset);
                submitMulti.SourceNpi = buf.ExtractByte(ref offset);
                submitMulti.SourceAddr = buf.ExtractCString(ref offset);
                submitMulti.DestinationAddresses = buf.ExtractDestinationAddresses(ref offset);
                submitMulti.EsmClass = buf.ExtractByte(ref offset);
                submitMulti.ProtocolId = buf.ExtractByte(ref offset);
                submitMulti.PriorityFlag = buf.ExtractByte(ref offset);
                submitMulti.ScheduleDeliveryTime = buf.ExtractCString(ref offset);
                submitMulti.ValidityPeriod = buf.ExtractCString(ref offset);
                submitMulti.RegisteredDelivery = buf.ExtractByte(ref offset);
                submitMulti.ReplaceIfPresent = buf.ExtractByte(ref offset);
                submitMulti.DataCoding = (DataCodings) buf.ExtractByte(ref offset);
                submitMulti.DefaultMsgId = buf.ExtractByte(ref offset);
                submitMulti.ShortMessageLength = buf.ExtractByte(ref offset);

                if (submitMulti.ShortMessageLength > 0)
                {
                    submitMulti.UserDataBuffer = new SmppBuffer(defaultEncoding, buf.ExtractByteArray(ref offset, submitMulti.ShortMessageLength));
                }
            
                while (offset - start_offset < submitMulti.Length)
                {
                    submitMulti.Optional.Add(buf.ExtractTLV(ref offset));
                }
            }
            
            catch
            {
                submitMulti = null;
            }

            return submitMulti;
        }

        #endregion

        #region PDU Detail Methods

        /// <summary> Called to return a list of property details from the PDU </summary>
        /// <returns> List PduPropertyDetail </returns>
        public List<PduPropertyDetail> Details()
        {
            List<PduPropertyDetail> details = null;

            try
            {
            }

            catch
            {
            }

            return details;
        }

        #endregion
        
        #region IPacket Methods

        /// <summary> Called to return the PDU for this type of object </summary>
        /// <returns> A byte array </returns>
        public byte[] GetPDU()
        {
            if (UserData.Headers.Count > 0)
                EsmClass |= 0x40;

            SmppBuffer tmpBuff = new SmppBuffer(DefaultEncoding, this);

            tmpBuff.AddCString(ServiceType);
            tmpBuff.AddByte(SourceTon);
            tmpBuff.AddByte(SourceNpi);
            tmpBuff.AddCString(SourceAddr);
            tmpBuff.AddDestinationAddresses(DestinationAddresses);
            tmpBuff.AddByte(EsmClass);
            tmpBuff.AddByte(ProtocolId);
            tmpBuff.AddByte(PriorityFlag);
            tmpBuff.AddTimeString(ScheduleDeliveryTime, 17);
            tmpBuff.AddTimeString(ValidityPeriod, 17);
            tmpBuff.AddByte(RegisteredDelivery);
            tmpBuff.AddByte(ReplaceIfPresent);
            tmpBuff.AddByte(Convert.ToByte(DataCoding));
            tmpBuff.AddByte(DefaultMsgId);
            tmpBuff.AddUserData(UserData);
            tmpBuff.AddTLVCollection(Optional);

            tmpBuff.AddFinalLength();

            return tmpBuff.Buffer;
        }

        #endregion
    }
}
