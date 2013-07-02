#region Namespaces

using System;
using System.Collections.Generic;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> Query the status of a previously submitted short message </summary>
    public class QuerySm : Header, IPacket, IPduDetails
    {
        #region Public Properties
        
        /// <summary> The id of the message </summary>
        public string MessageId { get; set; }

        /// <summary> Source address type of number </summary>
        public byte SourceTon { get; set; }

        /// <summary> Source address numbering plan indicator </summary>
        public byte SourceNpi { get; set; }

        /// <summary> Source phone number </summary>
        public string SourceAddr { get; set; }
        
        #endregion
        
        #region Constructor
        
        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        private QuerySm(DataCodings defaultEncoding) : base(defaultEncoding, CommandSet.QuerySm, SequenceGenerator.Counter)
        {
        }

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="messageId"></param>
        /// <param name="sourceTon"></param>
        /// <param name="sourceNpi"></param>
        /// <param name="sourceAddr"></param>
        private QuerySm(DataCodings defaultEncoding, string messageId, byte sourceTon, byte sourceNpi, string sourceAddr) : base(defaultEncoding, CommandSet.QuerySm, SequenceGenerator.Counter)
        {
            MessageId = messageId;
            SourceTon = sourceTon;
            SourceNpi = sourceNpi;
            SourceAddr = sourceAddr;
        }

        #endregion

        #region Factory Methods

        /// <summary> Called to create a QuerySm object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="messageId"></param>
        /// <param name="sourceTon"></param>
        /// <param name="sourceNpi"></param>
        /// <param name="sourceAddr"></param>
        /// <returns> QuerySm </returns>
        public static QuerySm Create(DataCodings defaultEncoding, string messageId, byte sourceTon, byte sourceNpi, string sourceAddr)
        {
            return new QuerySm(defaultEncoding, messageId, sourceTon, sourceNpi, sourceAddr);
        }

        /// <summary> Called to create a QuerySm object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <returns> QuerySm </returns>
        public static QuerySm Create(DataCodings defaultEncoding, SmppBuffer buf, ref int offset)
        {
            QuerySm querySm = new QuerySm(defaultEncoding);
            
            try
            {
                buf.ExtractHeader(querySm, ref offset);

                querySm.MessageId = buf.ExtractCString(ref offset);
                querySm.SourceTon = buf.ExtractByte(ref offset);
                querySm.SourceNpi = buf.ExtractByte(ref offset);
                querySm.SourceAddr = buf.ExtractCString(ref offset);
            }
            
            catch
            {
                querySm = null;
            }

            return querySm;
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

                details.Add(PduData.ExtractCString("MessageId", ref offset));
                details.Add(PduData.ExtractByte("SourceTon", ref offset));
                details.Add(PduData.ExtractByte("SourceNpi", ref offset));
                details.Add(PduData.ExtractCString("SourceAddr", ref offset));
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
            SmppBuffer tmpBuff = new SmppBuffer(DefaultEncoding, this);

            tmpBuff.AddCString(MessageId);
            tmpBuff.AddByte(SourceTon);
            tmpBuff.AddByte(SourceNpi);
            tmpBuff.AddCString(SourceAddr);

            tmpBuff.AddFinalLength();

            return tmpBuff.Buffer;
        }

        #endregion
    }
}
