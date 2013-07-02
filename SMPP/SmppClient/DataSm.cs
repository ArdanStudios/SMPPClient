#region Namespaces

using System;
using System.Collections.Generic;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> This command is used to transfer data between the SMSC (Short Message Service Centre) and the
    /// ESME (Extended Short Message Entity). It may be used by both the ESME and SMSC  </summary>
    public class DataSm : Header, IPacket, IPduDetails
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

        /// <summary> Indicates if the message is a registered short message and thus if a Delivery Receipt is required upon the message attaining a final state </summary>
        public byte RegisteredDelivery { get; set; }

        /// <summary> Indicates the encoding scheme of the payload data </summary>
        public DataCodings DataCoding { get; set; }

        /// <summary> The smpp data packet </summary>
        private SmppBuffer UserDataBuffer { get; set; }

        /// <summary> The user data portion of the data packet </summary>
        private UserData UserData { get; set; }

        /// <summary> A reference assigned by the originating SME to the short message </summary>
        public ushort MessageReferenceNumber { get; set; }

        /// <summary> Total number of short messages within the concatenated short message </summary>
        public byte TotalSegments { get; set; }

        /// <summary> Sequence number of a particular short message within the concatenated short message </summary>
        public byte SequenceNumber { get; set; }

        /// <summary> Optional Parameters </summary>
        public TLVCollection Optional { get; set; }

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
        private DataSm(DataCodings defaultEncoding) : base(defaultEncoding, CommandSet.DataSm)
        {
            Optional = new TLVCollection();
        }

        #endregion

        #region Factory Methods

        /// <summary> Called to create a DataSm object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <returns> DataSm </returns>
        public static DataSm Create(DataCodings defaultEncoding, SmppBuffer buf, ref int offset)
        {
            DataSm dataSm = new DataSm(defaultEncoding);

            try
            {
                int start_offset = offset;

                buf.ExtractHeader(dataSm, ref offset);

                dataSm.ServiceType = buf.ExtractCString(ref offset);
                dataSm.SourceTon = buf.ExtractByte(ref offset);
                dataSm.SourceNpi = buf.ExtractByte(ref offset);
                dataSm.SourceAddr = buf.ExtractCString(ref offset);
                dataSm.DestTon = buf.ExtractByte(ref offset);
                dataSm.DestNpi = buf.ExtractByte(ref offset);
                dataSm.DestAddr = buf.ExtractCString(ref offset);
                dataSm.EsmClass = buf.ExtractByte(ref offset);
                dataSm.RegisteredDelivery = buf.ExtractByte(ref offset);
                dataSm.DataCoding = (DataCodings) buf.ExtractByte(ref offset);

                if (offset - start_offset < dataSm.Length)
                {
                    if (dataSm.Optional == null)
                    {
                        dataSm.Optional = new TLVCollection();
                    }

                    while (offset - start_offset < dataSm.Length)
                    {
                        dataSm.Optional.Add(buf.ExtractTLV(ref offset));
                    }
                }

                if (dataSm.Optional != null && dataSm.Optional.Count > 0)
                {
                    TLV tlv_payload = dataSm.Optional[OptionalTags.MessagePayload];
                    if (tlv_payload != null)
                    {
                        dataSm.UserDataBuffer = new SmppBuffer(defaultEncoding, tlv_payload.Value);
                    }

                    TLV tlv_globalSeq = dataSm.Optional[OptionalTags.SarMsgRefNum];
                    if (tlv_globalSeq != null)
                    {
                        dataSm.MessageReferenceNumber = SmppBuffer.BytesToShort(tlv_globalSeq.Value, 0);
                    }

                    TLV tlv_pocketsNumber = dataSm.Optional[OptionalTags.SarTotalSegments];
                    if (tlv_pocketsNumber != null)
                    {
                        dataSm.TotalSegments = tlv_pocketsNumber.Value[0];
                    }

                    TLV tlv_localSeq = dataSm.Optional[OptionalTags.SarSegmentSeqnum];
                    if (tlv_localSeq != null)
                    {
                        dataSm.SequenceNumber = tlv_localSeq.Value[0];
                    }
                }

                if (dataSm.UserDataBuffer != null && dataSm.UserDataBuffer.Length > 0)
                {
                    dataSm.UserData = UserData.Create(dataSm.UserDataBuffer, dataSm.MessageFeature == GSMSpecificFeatures.UDHI);
                }
                else
                {
                    dataSm.UserData = UserData.Create();
                }
            }

            catch
            {
                dataSm = null;
            }

            return dataSm;
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

                details.Add(PduData.ExtractByte("EsmClass", ref offset));
                details.Add(PduData.ExtractByte("RegisteredDelivery", ref offset));
                details.Add(PduData.ExtractByte("DataCoding", ref offset));

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
        /// <returns> byte[] </returns>
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
            tmpBuff.AddByte(RegisteredDelivery);
            tmpBuff.AddByte((byte) DataCoding);
            tmpBuff.AddTLVCollection(Optional);

            tmpBuff.AddFinalLength();

            return tmpBuff.Buffer;
        }

        #endregion
    }
}
