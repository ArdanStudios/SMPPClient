#region Namespaces

using System;
using System.Collections.Generic;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> Represents the response to the deliver_sm PDU </summary>
    public class DeliverSmResp : Header, IPacket, IPduDetails
    {
        #region Public Properties

        /// <summary> In DeliverSM this is never used and needs to be NULL </summary>
        public string MessageId { get; set; }

        /// <summary> Optional Parameters </summary>
        public TLVCollection Optional { get; set; }

        #endregion

        #region Constructor

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        private DeliverSmResp(DataCodings defaultEncoding) : base(defaultEncoding, CommandSet.DeliverSmResp)
        {
            Optional = new TLVCollection();
        }

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="commandStatus"></param>
        /// <param name="sequence"></param>
        private DeliverSmResp(DataCodings defaultEncoding, CommandStatus commandStatus, uint sequence) : base(defaultEncoding, CommandSet.DeliverSmResp, commandStatus, sequence)
        {
            Optional = new TLVCollection();
        }

        #endregion
        
        #region Factory Methods

        /// <summary> Called to create a DeliverSmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <returns> DeliverSmResp </returns>
        public static DeliverSmResp Create(DataCodings defaultEncoding)
        {
            return new DeliverSmResp(defaultEncoding);
        }

        /// <summary> Called to create a DeliverSmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="commandStatus"></param>
        /// <returns> DeliverSmResp </returns>
        internal static DeliverSmResp Create(DataCodings defaultEncoding, CommandStatus commandStatus)
        {
            return new DeliverSmResp(defaultEncoding, commandStatus, 0);
        }

        /// <summary> Called to create a DeliverSmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="commandStatus"></param>
        /// <param name="sequence"></param>
        /// <returns> DeliverSmResp </returns>
        public static DeliverSmResp Create(DataCodings defaultEncoding, CommandStatus commandStatus, uint sequence)
        {
            return new DeliverSmResp(defaultEncoding, commandStatus, sequence);
        }

        /// <summary> Called to create a DeliverSmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="deliverSm"></param>
        /// <param name="commandStatus"></param>
        /// <returns> DeliverSmResp </returns>
        public static DeliverSmResp Create(DataCodings defaultEncoding, DeliverSm deliverSm, CommandStatus commandStatus)
        {
            if (deliverSm.SourceAddr == string.Empty || deliverSm.SourceAddr == null)
            {
                commandStatus = CommandStatus.ESME_RINVSRCADR;
            }

            return new DeliverSmResp(defaultEncoding, commandStatus, deliverSm.Sequence);
        }

        /// <summary> Called to create a DeliverSmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <returns> DeliverSmResp </returns>
        public static DeliverSmResp Create(DataCodings defaultEncoding, SmppBuffer buf, ref int offset)
        {
            DeliverSmResp deliverSmResp = new DeliverSmResp(defaultEncoding);
            
            try
            {
                int start_offset = offset;

                buf.ExtractHeader(deliverSmResp, ref offset);

                if (deliverSmResp.Length > Header.HEADER_LENGTH)
                {
                    deliverSmResp.MessageId = buf.ExtractCString(ref offset);

                    while (offset - start_offset < deliverSmResp.Length)
                    {
                        deliverSmResp.Optional.Add(buf.ExtractTLV(ref offset));
                    }
                }
            }
            
            catch
            {
                deliverSmResp = null;
            }

            return deliverSmResp;
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

                if (details[0].ValueUInt > Header.HEADER_LENGTH)
                {
                    details.Add(PduData.ExtractCString("MessageId", ref offset));

                    while (offset < PduData.Length)
                    {
                        PduData.ExtractTLV(details, ref offset);
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
            SmppBuffer tmpBuff = new SmppBuffer(DefaultEncoding, this);

            tmpBuff.AddCString(MessageId);
                
            if (Optional.Count > 0)
            {
                tmpBuff.AddTLVCollection(Optional);
            }

            tmpBuff.AddFinalLength();

            return tmpBuff.Buffer;
        }

        #endregion
    }
}
