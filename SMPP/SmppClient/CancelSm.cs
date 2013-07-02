#region Namespaces

using System;
using System.Collections.Generic;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> Cancel previously submitted short message that is still pending delivery </summary>
    public class CancelSm : Header, IPacket, IPduDetails
    {
        #region Public Properties
        
        /// <summary> Application service associated with the message </summary>
        public string ServiceType { get; set; }

        /// <summary> The id of the message </summary>
        public string MessageId { get; set; }

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

        #endregion
        
        #region Constructor

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        private CancelSm(DataCodings defaultEncoding) : base(defaultEncoding, CommandSet.CancelSm, SequenceGenerator.Counter)
        {
        }
        
        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="serviceType"></param>
        /// <param name="messageId"></param>
        /// <param name="srcTon"></param>
        /// <param name="srcNpi"></param>
        /// <param name="srcAddr"></param>
        /// <param name="dstTon"></param>
        /// <param name="dstNpi"></param>
        /// <param name="dstAddr"></param>
        private CancelSm(DataCodings defaultEncoding, string serviceType, string messageId, byte srcTon, byte srcNpi, string srcAddr, byte dstTon, byte dstNpi, string dstAddr) : this(defaultEncoding)
        {
            ServiceType = serviceType;
            MessageId = messageId;
            SourceTon = srcTon;
            SourceNpi = srcNpi;
            SourceAddr = srcAddr;
            DestTon = dstTon;
            DestNpi = dstNpi;
            DestAddr = dstAddr;
        }

        #endregion
        
        #region Factory Methods

        /// <summary> Called to create a CancelSm object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="serviceType"></param>
        /// <param name="messageId"></param>
        /// <param name="srcTon"></param>
        /// <param name="srcNpi"></param>
        /// <param name="srcAddr"></param>
        /// <param name="dstTon"></param>
        /// <param name="dstNpi"></param>
        /// <param name="dstAddr"></param>
        /// <returns> CancelSm </returns>
        internal static CancelSm Create(DataCodings defaultEncoding, string serviceType, string messageId, byte srcTon, byte srcNpi, string srcAddr, byte dstTon, byte dstNpi, string dstAddr)
        {
            return new CancelSm(defaultEncoding, serviceType, messageId, srcTon, srcNpi, srcAddr, dstTon, dstNpi, dstAddr);
        }

        /// <summary> Called to create a CancelSm object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <returns> CancelSm </returns>
        internal static CancelSm Create(DataCodings defaultEncoding, SmppBuffer buf, ref int offset)
        {
            CancelSm cancelSm = new CancelSm(defaultEncoding);
            
            try
            {
                buf.ExtractHeader(cancelSm, ref offset);

                cancelSm.ServiceType = buf.ExtractCString(ref offset);
                cancelSm.MessageId = buf.ExtractCString(ref offset);
                cancelSm.SourceTon = buf.ExtractByte(ref offset);
                cancelSm.SourceNpi = buf.ExtractByte(ref offset);
                cancelSm.SourceAddr = buf.ExtractCString(ref offset);
                cancelSm.DestTon = buf.ExtractByte(ref offset);
                cancelSm.DestNpi = buf.ExtractByte(ref offset);
                cancelSm.DestAddr = buf.ExtractCString(ref offset);
            }
            
            catch
            {
                cancelSm = null;
            }

            return cancelSm;
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
                details.Add(PduData.ExtractCString("MessageId", ref offset));
                details.Add(PduData.ExtractByte("SourceTon", ref offset));
                details.Add(PduData.ExtractByte("SourceNpi", ref offset));
                details.Add(PduData.ExtractCString("SourceAddr", ref offset));
                details.Add(PduData.ExtractByte("DestTon", ref offset));
                details.Add(PduData.ExtractByte("DestNpi", ref offset));
                details.Add(PduData.ExtractCString("DestAddr", ref offset));
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
            // Create buffer and add header information
            SmppBuffer tmpBuff = new SmppBuffer(DefaultEncoding, this);

            // Add properties
            tmpBuff.AddCString(ServiceType);
            tmpBuff.AddCString(MessageId);
            tmpBuff.AddByte(SourceTon);
            tmpBuff.AddByte(SourceNpi);
            tmpBuff.AddCString(SourceAddr);
            tmpBuff.AddByte(DestTon);
            tmpBuff.AddByte(DestNpi);
            tmpBuff.AddCString(DestAddr);

            // Insert final length
            tmpBuff.AddFinalLength();

            // Return byte array
            return tmpBuff.Buffer;
        }

        #endregion
    }
}
