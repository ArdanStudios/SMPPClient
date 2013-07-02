#region Namespaces

using System;
using System.Collections.Generic;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> The deliver_sm is issued by the SMSC (Short Message Service Centre) to send a
    /// message to an ESME (Extended Short Message Entity). Using this command, the SMSC may route a
    /// short message to the ESME for delivery. </summary>
    public class DeliverSm : Header, IPacket, IPduDetails
    {
        #region Public Properties

        /// <summary> Application service associated with the message </summary>
        public string ServiceType { get; set; }

        /// <summary> Source address type of number </summary>
        public byte SourceTon { get; set; }

        /// <summary> Source address numbering plan indicator </summary>
        public byte SourceNpi { get; set; }

        /// <summary> Source phone number </summary>
        public string SourceAddr { get; set; }

        /// <summary> Destination address type of number </summary>
        public byte DestTon { get; set; }

        /// <summary> Destination address numbering plan indicator </summary>
        public byte DestNpi { get; set; }

        /// <summary> Destination phone number </summary>
        public string DestAddr { get; set; }

        /// <summary> Extended short message class </summary>
        public byte EsmClass { get; set; }

        /// <summary> Indicates which protocol to use </summary>
        private byte ProtocolId { get; set; }
        
        /// <summary> Indicate the priority level of the message </summary>
        private byte PriorityFlag { get; set; }
        
        /// <summary> The date and time the message should be delivered </summary>
        private string ScheduleDeliveryTime { get; set; }
        
        /// <summary> Indicates how long the message is valid </summary>
        private string ValidityPeriod { get; set; }

        /// <summary> Indicates if the message is a registered short message and thus if a Delivery Receipt is required upon the message attaining a final state </summary>
        private byte RegisteredDelivery { get; set; }

        /// <summary> Requests the SMSC to replace a previously submitted message, that is still pending delivery </summary>
        private byte ReplaceIfPresent { get; set; }

        /// <summary> Indicates the encoding scheme of the payload data </summary>
        public DataCodings DataCoding { get; set; }

        /// <summary> Default message id </summary>
        private byte DefaultMessageId { get; set; }

        /// <summary> Message data length </summary>
        private byte ShortMessageLength { get; set; }

        /// <summary> The smpp data packet </summary>
        private SmppBuffer UserDataBuffer { get; set; }

        /// <summary> The user data portion of the data packet </summary>
        public UserData UserData { get; set; }

        /// <summary> Optional Parameters </summary>
        public TLVCollection Optional { get; set; }

        /// <summary> A reference assigned by the originating SME to the short message </summary>
        public ushort MessageReferenceNumber { get; set; }

        /// <summary> Total number of short messages within the concatenated short message </summary>
        public byte TotalSegments { get; set; }

        /// <summary> Indicates the segment number of a particular short message fragment within the concatenated short message </summary>
        public byte SeqmentNumber { get; set; }

        /// <summary> Short message text in specified data_coding </summary>
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
        private DeliverSm(DataCodings defaultEncoding) : base(defaultEncoding, CommandSet.DeliverSm, SequenceGenerator.Counter)
        {
            Optional = new TLVCollection();

            ValidityPeriod = DateTime.Now.AddDays(2).ToString("yyMMddhhmmss000+");
            UserData = UserData.Create();
        }

        /// <summary> Constructor</summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="serviceType"></param>
        /// <param name="sourceTon"></param>
        /// <param name="sourceNpi"></param>
        /// <param name="sourceAddr"></param>
        /// <param name="destTon"></param>
        /// <param name="destNpi"></param>
        /// <param name="destAddr"></param>
        private DeliverSm(DataCodings defaultEncoding, string serviceType, byte sourceTon, byte sourceNpi, string sourceAddr, byte destTon, byte destNpi, string destAddr) : base(defaultEncoding, CommandSet.DeliverSm, SequenceGenerator.Counter)
        {
            Optional = new TLVCollection();

            ValidityPeriod = DateTime.Now.AddDays(2).ToString("yyMMddhhmmss000+");
            UserData = UserData.Create();

            ServiceType = serviceType;
            SourceTon = sourceTon;
            SourceNpi = sourceNpi;
            SourceAddr = sourceAddr;
            DestTon = destTon;
            DestNpi = destNpi;
            DestAddr = destAddr;
        }
        
        #endregion
        
        #region Factory Methods

        /// <summary> Called to create a DeliverSm object </summary>
        /// <param name="defaultEncoding"></param>
        /// <returns> DeliverSm </returns>
        public static DeliverSm Create(DataCodings defaultEncoding)
        {
            return new DeliverSm(defaultEncoding);
        }

        /// <summary> Called to create a DeliverSm object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="serviceType"></param>
        /// <param name="sourceTon"></param>
        /// <param name="sourceNpi"></param>
        /// <param name="sourceAddr"></param>
        /// <param name="destTon"></param>
        /// <param name="destNpi"></param>
        /// <param name="destAddr"></param>
        /// <returns> SubmitSm </returns>
        public static DeliverSm Create(DataCodings defaultEncoding, string serviceType, byte sourceTon, byte sourceNpi, string sourceAddr, byte destTon, byte destNpi, string destAddr)
        {
            return new DeliverSm(defaultEncoding, serviceType, sourceTon, sourceNpi, sourceAddr, destTon, destNpi, destAddr);
        }

        /// <summary> Parses the smpp buffer to a DeliverSm object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <returns> DeliverSm </returns>
        public static DeliverSm Create(DataCodings defaultEncoding, SmppBuffer buf, ref int offset)
        {
            DeliverSm deliverSm = new DeliverSm(defaultEncoding);
            
            try
            {
                int start_offset = offset;

                buf.ExtractHeader(deliverSm, ref offset);

                deliverSm.ServiceType = buf.ExtractCString(ref offset);
                deliverSm.SourceTon = buf.ExtractByte(ref offset);
                deliverSm.SourceNpi = buf.ExtractByte(ref offset);
                deliverSm.SourceAddr = buf.ExtractCString(ref offset);
                deliverSm.DestTon = buf.ExtractByte(ref offset);
                deliverSm.DestNpi = buf.ExtractByte(ref offset);
                deliverSm.DestAddr = buf.ExtractCString(ref offset);
                deliverSm.EsmClass = buf.ExtractByte(ref offset);
                deliverSm.ProtocolId = buf.ExtractByte(ref offset);
                deliverSm.PriorityFlag = buf.ExtractByte(ref offset);
                deliverSm.ScheduleDeliveryTime = buf.ExtractCString(ref offset);
                deliverSm.ValidityPeriod = buf.ExtractCString(ref offset);
                deliverSm.RegisteredDelivery = buf.ExtractByte(ref offset);
                deliverSm.ReplaceIfPresent = buf.ExtractByte(ref offset);
                deliverSm.DataCoding = (DataCodings) buf.ExtractByte(ref offset);
                deliverSm.DefaultMessageId = buf.ExtractByte(ref offset);
                deliverSm.ShortMessageLength = buf.ExtractByte(ref offset);

                if (deliverSm.ShortMessageLength > 0)
                {
                    deliverSm.UserDataBuffer = new SmppBuffer(defaultEncoding, buf.ExtractByteArray(ref offset, deliverSm.ShortMessageLength));
                }
            
                while (offset - start_offset < deliverSm.Length)
                {
                    deliverSm.Optional.Add(buf.ExtractTLV(ref offset));
                }

                if (deliverSm.ShortMessageLength == 0)
                {
                    if (deliverSm.Optional != null && deliverSm.Optional.Count > 0)
                    {
                        TLV tlv_payload = deliverSm.Optional[OptionalTags.MessagePayload];
                        if (tlv_payload != null)
                        {
                            deliverSm.UserDataBuffer = new SmppBuffer(defaultEncoding, tlv_payload.Value);
                        }
                    
                        TLV tlv_globalSeq = deliverSm.Optional[OptionalTags.SarMsgRefNum];
                        if (tlv_globalSeq != null)
                        {
                            deliverSm.MessageReferenceNumber = SmppBuffer.BytesToShort(tlv_globalSeq.Value, 0);
                        }
                    
                        TLV tlv_pocketsNumber = deliverSm.Optional[OptionalTags.SarTotalSegments];
                        if (tlv_pocketsNumber != null)
                        {
                            deliverSm.TotalSegments = tlv_pocketsNumber.Value[0];
                        }
                    
                        TLV tlv_localSeq = deliverSm.Optional[OptionalTags.SarSegmentSeqnum];
                        if (tlv_localSeq != null)
                        {
                            deliverSm.SeqmentNumber = tlv_localSeq.Value[0];
                        }
                    }
                }

                if (deliverSm.UserDataBuffer != null && deliverSm.UserDataBuffer.Length > 0)
                {
                    bool hasUDHI = deliverSm.MessageFeature == GSMSpecificFeatures.UDHI || deliverSm.MessageFeature == GSMSpecificFeatures.UDHIandReplyPath;
                    deliverSm.UserData = UserData.Create(deliverSm.UserDataBuffer, hasUDHI);

                    if (deliverSm.UserData.Headers.Count > 0)
                    {
                        UserDataHeader udh = deliverSm.UserData.Headers[InformationElementIdentifiers.ConcatenatedShortMessages8bit];
                        if (udh != null)
                        {
                            deliverSm.MessageReferenceNumber = udh.Data[0];
                            deliverSm.TotalSegments = udh.Data[1];
                            deliverSm.SeqmentNumber = udh.Data[2];
                        }
                
                        udh = deliverSm.UserData.Headers[InformationElementIdentifiers.ConcatenatedShortMessage16bit];
                        if (udh != null)
                        {
                            deliverSm.MessageReferenceNumber = SmppBuffer.BytesToShort(udh.Data, 0);
                            deliverSm.TotalSegments = udh.Data[2];
                            deliverSm.SeqmentNumber = udh.Data[3];
                        }
                    }
                }
                else
                {
                    deliverSm.UserData = UserData.Create();
                }
            }
            
            catch
            {
                deliverSm = null;
            }

            return deliverSm;
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
                int offset = 0;

                details = PduData.ExtractHeaderDetails(ref offset);

                details.Add(PduData.ExtractCString("ServiceType", ref offset));
                details.Add(PduData.ExtractByte("SourceTon", ref offset));
                details.Add(PduData.ExtractByte("SourceNpi", ref offset));
                details.Add(PduData.ExtractCString("SourceAddr", ref offset));
                details.Add(PduData.ExtractByte("DestTon", ref offset));
                details.Add(PduData.ExtractByte("DestNpi", ref offset));
                details.Add(PduData.ExtractCString("DestAddr", ref offset));
                
                PduPropertyDetail esmClass = PduData.ExtractByte("EsmClass", ref offset);
                details.Add(esmClass);

                details.Add(PduData.ExtractByte("ProtocolId", ref offset));
                details.Add(PduData.ExtractByte("PriorityFlag", ref offset));
                details.Add(PduData.ExtractCString("ScheduleDeliveryTime", ref offset));
                details.Add(PduData.ExtractCString("ValidityPeriod", ref offset));
                details.Add(PduData.ExtractByte("RegisteredDelivery", ref offset));
                details.Add(PduData.ExtractByte("ReplaceIfPresent", ref offset));
                
                PduPropertyDetail dataCoding = PduData.ExtractByte("DataCoding", ref offset);
                details.Add(dataCoding);

                details.Add(PduData.ExtractByte("DefaultMessageId", ref offset));

                PduPropertyDetail messageLength = PduData.ExtractByte("ShortMessageLength", ref offset);
                details.Add(messageLength);

                if (messageLength.ValueByte > 0)
                {
                    PduPropertyDetail userDataProperty = PduData.ExtractByteArray("ShortMessage", ref offset, messageLength.ValueByte);
                    userDataProperty.PduDataType = PduDataTypes.EncodedString;

                    UserData userData = UserData.Create(new SmppBuffer(DefaultEncoding, userDataProperty.DataBlock), false);
                    userDataProperty.ValueString = userData.ShortMessageText(DefaultEncoding, (DataCodings) dataCoding.ValueByte);
                    
                    details.Add(userDataProperty);
                }

                while (offset < PduData.Length)
                {
                    PduData.ExtractTLV(details, ref offset);
     
                    PduPropertyDetail tlvTag = details[details.Count - 3];
                    PduPropertyDetail tlvLength = details[details.Count - 2];
                    PduPropertyDetail tlvValue = details[details.Count - 1];

                    switch (tlvTag.ValueUShort)
                    {
                        case (ushort) OptionalTags.MessagePayload:
                            GSMSpecificFeatures messageFeature = (GSMSpecificFeatures) (esmClass.ValueByte & 0xc0);
                            SmppBuffer userData = new SmppBuffer(DefaultEncoding, tlvValue.DataBlock);
                            userData.ExtractUserData(details, messageFeature == GSMSpecificFeatures.UDHI, offset);
                            break;
                        
                        case (ushort) OptionalTags.SarMsgRefNum:
                            tlvValue.PduDataType = PduDataTypes.UShort;
                            tlvValue.Name = "SARReferenceNumber";
                            tlvValue.ValueUShort = SmppBuffer.BytesToShort(tlvValue.DataBlock, 0);
                            break;

                        case (ushort) OptionalTags.SarTotalSegments:
                            tlvValue.PduDataType = PduDataTypes.Byte;
                            tlvValue.Name = "SARTotalSegments";
                            tlvValue.ValueByte = tlvValue.DataBlock[0];
                            break;

                        case (ushort) OptionalTags.SarSegmentSeqnum:
                            tlvValue.PduDataType = PduDataTypes.Byte;
                            tlvValue.Name = "SARSequenceNumber";
                            tlvValue.ValueByte = tlvValue.DataBlock[0];
                            break;
                    }
                }
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
            {
                EsmClass |= 0x40;
            }

            SmppBuffer tmpBuff = new SmppBuffer(DefaultEncoding, this);
            
            tmpBuff.AddCString(ServiceType);
            tmpBuff.AddByte(SourceTon);
            tmpBuff.AddByte(SourceNpi);
            tmpBuff.AddCString(SourceAddr);
            tmpBuff.AddByte(DestTon);
            tmpBuff.AddByte(DestNpi);
            tmpBuff.AddCString(DestAddr);
            tmpBuff.AddByte(EsmClass);
            tmpBuff.AddByte(ProtocolId);
            tmpBuff.AddByte(PriorityFlag);
            tmpBuff.AddTimeString(ScheduleDeliveryTime, 17);
            tmpBuff.AddTimeString(ValidityPeriod, 17);
            tmpBuff.AddByte(RegisteredDelivery);
            tmpBuff.AddByte(ReplaceIfPresent);
            tmpBuff.AddByte((byte) DataCoding);
            tmpBuff.AddByte(DefaultMessageId);
            tmpBuff.AddUserData(UserData);
            tmpBuff.AddTLVCollection(Optional);

            tmpBuff.AddFinalLength();

            return tmpBuff.Buffer;
        }

        #endregion
    }
}
