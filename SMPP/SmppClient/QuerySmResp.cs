#region Namespaces

using System;
using System.Globalization;
using System.Collections.Generic;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> Represents the response to the query_sm_resp PDU </summary>
    public class QuerySmResp : Header, IPacket, IPduDetails
    {
        #region Public Properties
        
        /// <summary> The id of the message </summary>
        public string MessageId { get; set; }

        /// <summary> The raw final date string </summary>
        public string FinalDateString { get; set; }

        /// <summary> The state of the message </summary>
        public MessageState MessageState { get; set; }

        /// <summary> The error code </summary>
        public uint ErrorCode { get; set; }

        /// <summary> The final date </summary>
        public DateTime FinalDate
        {
            get
            {
                DateTime date; 
                if (FinalDateString != null && FinalDateString.Length > 0)
                {
                    try
                    {
                        date = DateTime.ParseExact(
                            FinalDateString,
                            "yyMMddHHmmss",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AllowWhiteSpaces);
                    }
                    
                    catch
                    {

                        //TODO: parse format "tnnp" Time difference 
                        //in quarter hours between local time and UTC
                        date = DateTime.ParseExact(
                            FinalDateString.Substring(0, 12),
                            "yyMMddHHmmss",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AllowWhiteSpaces);
                    }
                }
                else
                {
                    // This is min date in the database
                    date = new DateTime(1753, 1, 1);
                }

                return date;
            }

            set
            {
                FinalDateString = value.ToString("yyMMddHHmmss", CultureInfo.InvariantCulture);
            }
        }
        
        #endregion
        
        #region Constructor
        
        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        private QuerySmResp(DataCodings defaultEncoding) : base(defaultEncoding, CommandSet.QuerySmResp)
        {
        }

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="commandStatus"></param>
        /// <param name="sequence"></param>
        private QuerySmResp(DataCodings defaultEncoding, CommandStatus commandStatus, uint sequence) : base(defaultEncoding, CommandSet.QuerySmResp, commandStatus, sequence)
        {
        }

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="commandStatus"></param>
        /// <param name="sequence"></param>
        /// <param name="messageId"></param>
        private QuerySmResp(DataCodings defaultEncoding, CommandStatus commandStatus, uint sequence, string messageId) : base(defaultEncoding, CommandSet.QuerySmResp, commandStatus, sequence)
        {
            MessageId = messageId;
        }

        #endregion
        
        #region Factory Methods

        /// <summary> Called to create a QuerySmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="status"></param>
        /// <returns> QuerySmResp </returns>
        public static QuerySmResp Create(DataCodings defaultEncoding, CommandStatus status)
        {
            return new QuerySmResp(defaultEncoding, status, 0);
        }

        /// <summary> Called to create a QuerySmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="status"></param>
        /// <param name="sequence"></param>
        /// <returns> QuerySmResp </returns>
        public static QuerySmResp Create(DataCodings defaultEncoding, CommandStatus status, uint sequence)
        {
            return new QuerySmResp(defaultEncoding, status, sequence);
        }

        /// <summary> Called to create a DeliverSmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="querySm"></param>
        /// <param name="commandStatus"></param>
        /// <returns> QuerySmResp </returns>
        public static QuerySmResp Create(DataCodings defaultEncoding, QuerySm querySm, CommandStatus commandStatus)
        {
            return new QuerySmResp(defaultEncoding, commandStatus, querySm.Sequence, querySm.MessageId);
        }
        
        /// <summary> Called to create a QuerySmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <returns> QuerySmResp </returns>
        public static QuerySmResp Create(DataCodings defaultEncoding, SmppBuffer buf, ref int offset)
        {
            QuerySmResp querySmResp = new QuerySmResp(defaultEncoding);
            
            try
            {
                buf.ExtractHeader(querySmResp, ref offset);

                querySmResp.MessageId = buf.ExtractCString(ref offset);
                querySmResp.FinalDateString = buf.ExtractCString(ref offset);
                querySmResp.MessageState = (MessageState) buf.ExtractByte(ref offset);
                querySmResp.ErrorCode = buf.ExtractByte(ref offset);
            }
            
            catch
            {
                querySmResp = null;
            }

            return querySmResp;
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
                details.Add(PduData.ExtractCString("FinalDateString", ref offset));
                details.Add(PduData.ExtractByte("MessageState", ref offset));
                details.Add(PduData.ExtractByte("ErrorCode", ref offset));
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
            tmpBuff.AddCString(FinalDateString);
            tmpBuff.AddByte((byte) MessageState);
            tmpBuff.AddByte((byte) ErrorCode);
                
            tmpBuff.AddFinalLength();

            return tmpBuff.Buffer;
        }

        #endregion
    }
}
