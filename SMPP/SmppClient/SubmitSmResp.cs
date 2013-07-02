#region Namespaces

using System;
using System.Collections.Generic;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> Represents the response to the submit_sm PDU </summary>
    public class SubmitSmResp : Header, IPacket, IPduDetails
    {
        #region Public Properties

        /// <summary> The id of the message </summary>
        public string MessageId { get; set; }

        /// <summary> Optional Parameters </summary>
        public TLVCollection Optional { get; set; }

        #endregion

        #region Constructor

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        private SubmitSmResp(DataCodings defaultEncoding) : base(defaultEncoding, CommandSet.SubmitSmResp)
        {
            Optional = new TLVCollection();
        }

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="commandStatus"></param>
        /// <param name="sequence"></param>
        private SubmitSmResp(DataCodings defaultEncoding, CommandStatus commandStatus, uint sequence) : base(defaultEncoding, CommandSet.SubmitSmResp, commandStatus, sequence)
        {
            Optional = new TLVCollection();
        }

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="commandStatus"></param>
        /// <param name="sequence"></param>
        /// <param name="messageId"></param>
        private SubmitSmResp(DataCodings defaultEncoding, CommandStatus commandStatus, uint sequence, string messageId) : base(defaultEncoding, CommandSet.SubmitSmResp, commandStatus, sequence)
        {
            MessageId = messageId;
            Optional = new TLVCollection();
        }

        #endregion
        
        #region Factory Methods

        /// <summary> Called to create a SubmitSmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <returns> SubmitSmResp </returns>
        public static SubmitSmResp Create(DataCodings defaultEncoding)
        {
            return new SubmitSmResp(defaultEncoding);
        }

        /// <summary> Called to create a SubmitSmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="status"></param>
        /// <param name="sequence"></param>
        /// <returns> SubmitSmResp </returns>
        public static SubmitSmResp Create(DataCodings defaultEncoding, CommandStatus status, uint sequence)
        {
            return new SubmitSmResp(defaultEncoding, status, sequence);
        }

        /// <summary> Called to create a DataSmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="submitSm"></param>
        /// <param name="commandStatus"></param>
        /// <param name="messageId"></param>
        /// <returns> DataSmResp </returns>
        public static SubmitSmResp Create(DataCodings defaultEncoding, SubmitSm submitSm, CommandStatus commandStatus, string messageId)
        {
            if (submitSm.DestAddr == string.Empty || submitSm.DestAddr == null)
            {
                commandStatus = CommandStatus.ESME_RINVDSTADR;
            }

            return new SubmitSmResp(defaultEncoding, commandStatus, submitSm.Sequence, messageId);
        }

        /// <summary> Called to create a SubmitSmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <returns> SubmitSmResp </returns>
        public static SubmitSmResp Create(DataCodings defaultEncoding, SmppBuffer buf, ref int offset)
        {
            SubmitSmResp submitSmResp = new SubmitSmResp(defaultEncoding);
            
            try
            {
                int start_offset = offset;

                buf.ExtractHeader(submitSmResp, ref offset);
            
                if (submitSmResp.Length > Header.HEADER_LENGTH)
                {
                    submitSmResp.MessageId = buf.ExtractCString(ref offset);

                    while (offset - start_offset < submitSmResp.Length)
                    {
                        submitSmResp.Optional.Add(buf.ExtractTLV(ref offset));
                    }
                }
            }
            
            catch
            {
                submitSmResp = null;
            }

            return submitSmResp;
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
