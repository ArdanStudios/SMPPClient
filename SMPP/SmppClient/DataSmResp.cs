#region Namespaces

using System;
using System.Collections.Generic;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> Represents the response to the data_sm PDU </summary>
    public class DataSmResp : Header, IPacket, IPduDetails
    {
        #region Public Properties

        /// <summary> The id of the message </summary>
        private string MessageId { get; set; }

        /// <summary> Optional Parameters </summary>
        public TLVCollection Optional { get; set; }

        #endregion

        #region Constructor

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        private DataSmResp(DataCodings defaultEncoding) : base(defaultEncoding, CommandSet.DataSmResp)
        {
            Optional = new TLVCollection();
        }

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="commandStatus"></param>
        /// <param name="sequence"></param>
        private DataSmResp(DataCodings defaultEncoding, CommandStatus commandStatus, uint sequence) : base(defaultEncoding, CommandSet.DataSmResp, commandStatus, sequence)
        {
            Optional = new TLVCollection();
        }

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="commandStatus"></param>
        /// <param name="sequence"></param>
        /// <param name="messageId"></param>
        private DataSmResp(DataCodings defaultEncoding, CommandStatus commandStatus, uint sequence, string messageId) : base(defaultEncoding, CommandSet.DataSmResp, commandStatus, sequence)
        {
            MessageId = messageId;
            Optional = new TLVCollection();
        }

        #endregion

        #region Factory Methods

        /// <summary> Called to create a DataSmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <returns> DataSmResp </returns>
        public static DataSmResp Create(DataCodings defaultEncoding)
        {
            return new DataSmResp(defaultEncoding);
        }

        /// <summary> Called to create a DataSmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="status"></param>
        /// <returns> DataSmResp </returns>
        internal static DataSmResp Create(DataCodings defaultEncoding, CommandStatus status)
        {
            return new DataSmResp(defaultEncoding, status, 0);
        }

        /// <summary> Called to create a DataSmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="status"></param>
        /// <param name="sequence"></param>
        /// <returns> DataSmResp </returns>
        public static DataSmResp Create(DataCodings defaultEncoding, CommandStatus status, uint sequence)
        {
            return new DataSmResp(defaultEncoding, status, sequence);
        }

        /// <summary> Called to create a DataSmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="dataSm"></param>
        /// <param name="commandStatus"></param>
        /// <param name="messageId"></param>
        /// <returns> DataSmResp </returns>
        public static DataSmResp Create(DataCodings defaultEncoding, DataSm dataSm, CommandStatus commandStatus, string messageId)
        {
            if (dataSm.SourceAddr == string.Empty || dataSm.SourceAddr == null)
            {
                commandStatus = CommandStatus.ESME_RINVSRCADR;
            }

            return new DataSmResp(defaultEncoding, commandStatus, dataSm.Sequence, messageId);
        }

        /// <summary> Called to create a DataSmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <returns> DataSmResp </returns>
        public static DataSmResp Create(DataCodings defaultEncoding, SmppBuffer buf, ref int offset)
        {
            DataSmResp dataSmResp = new DataSmResp(defaultEncoding);

            try
            {
                int start_offset = offset;

                buf.ExtractHeader(dataSmResp, ref offset);

                if (dataSmResp.Length > Header.HEADER_LENGTH)
                {
                    dataSmResp.MessageId = buf.ExtractCString(ref offset);

                    while (offset - start_offset < dataSmResp.Length)
                    {
                        dataSmResp.Optional.Add(buf.ExtractTLV(ref offset));
                    }
                }
            }

            catch
            {
                dataSmResp = null;
            }

            return dataSmResp;
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
