#region Namespaces

using System;
using System.Collections.Generic;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> General Header class </summary>
    public class Header
    {
        #region Static Properties

        /// <summary> Length of the header section of the PDU </summary>
        public static uint HEADER_LENGTH = 16;

        #endregion
        
        #region Public Properties

        /// <summary> The raw pdu data </summary>
        public SmppBuffer PduData { get; set; }
        
        /// <summary> Overall length of the PDU </summary>
        public uint Length { get; set; }

        /// <summary> The Command id </summary>
        public CommandSet Command { get; set; }

        /// <summary> The Command status </summary>
        public CommandStatus Status { get; set; }

        /// <summary> Unique sequence number. PDU will echo the same sequence number </summary>
        public uint Sequence { get; set; }

        /// <summary> This id is a unique id to identify this pdu in an external system </summary>
        public Guid? ExternalId { get; internal set; }

        /// <summary> The encoding for the default DCS </summary>
        public DataCodings DefaultEncoding;
        
        #endregion
        
        #region Constructor

        /// <summary> Constructor
        /// BindSmResp requires this because we don't know the command set until later </summary>
        /// <param name="defaultEncoding"></param>
        public Header(DataCodings defaultEncoding)
        {
            DefaultEncoding = defaultEncoding;

            PduData = null;
            ExternalId = null;

            Length = HEADER_LENGTH;
            Status = CommandStatus.ESME_ROK;
            Sequence = 0;
        }
        
        /// <summary> Constructor </summary>
        /// <param name="commandSet"></param>
        /// <param name="defaultEncoding"></param>
        public Header(DataCodings defaultEncoding, CommandSet commandSet)
        {
            DefaultEncoding = defaultEncoding;

            PduData = null;
            ExternalId = null;

            Length = HEADER_LENGTH;
            Command = commandSet;
            Status = CommandStatus.ESME_ROK;
            Sequence = 0;
        }

        /// <summary> Constructor </summary>
        /// <param name="commandSet"></param>
        /// <param name="commandStatus"></param>
        /// <param name="defaultEncoding"></param>
        public Header(DataCodings defaultEncoding, CommandSet commandSet, CommandStatus commandStatus)
        {
            DefaultEncoding = defaultEncoding;

            PduData = null;
            ExternalId = null;

            Length = HEADER_LENGTH;
            Command = commandSet;
            Status = commandStatus;
            Sequence = 0;
        }

        /// <summary> Constructor </summary>
        /// <param name="commandSet"></param>
        /// <param name="sequence"></param>
        /// <param name="defaultEncoding"></param>
        public Header(DataCodings defaultEncoding, CommandSet commandSet, uint sequence)
        {
            DefaultEncoding = defaultEncoding;

            PduData = null;

            Length = HEADER_LENGTH;
            Command = commandSet;
            Status = CommandStatus.ESME_ROK;
            Sequence = sequence;
        }

        /// <summary> Constructor </summary>
        /// <param name="commandSet"></param>
        /// <param name="commandStatus"></param>
        /// <param name="sequence"></param>
        /// <param name="defaultEncoding"></param>
        public Header(DataCodings defaultEncoding, CommandSet commandSet, CommandStatus commandStatus, uint sequence)
        {
            DefaultEncoding = defaultEncoding;

            PduData = null;

            Length = HEADER_LENGTH;
            Command = commandSet;
            Status = commandStatus;
            Sequence = sequence;
        }

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="header"></param>
        public Header(DataCodings defaultEncoding, Header header)
        {
            DefaultEncoding = defaultEncoding;

            PduData = null;

            Length = header.Length;
            Command = header.Command;
            Status = header.Status;
            Sequence = header.Sequence;
        }

        #endregion
    }
}
