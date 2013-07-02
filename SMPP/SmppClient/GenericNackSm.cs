#region Namespaces

using System;
using System.Collections.Generic;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> This is a generic negative acknowledgement to an SMPP PDU submitted with an invalid
    /// message header </summary>
    public class GenericNackSm : Header, IPacket, IPduDetails
    {
        #region Constructor

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        private GenericNackSm(DataCodings defaultEncoding) : base(defaultEncoding)
        {
        }
        
        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="status"></param>
        /// <param name="sequence"></param>
        private GenericNackSm(DataCodings defaultEncoding, CommandStatus status, uint sequence) : base(defaultEncoding)
        {
            Length = (uint) Header.HEADER_LENGTH;
            Command = CommandSet.GenericNack;
            Status = status;
            Sequence = sequence;
        }

        #endregion

        #region Factory Methods

        /// <summary> Called to create a GenericNack object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="status"></param>
        /// <param name="sequence"></param>
        /// <returns> GenericNack </returns>
        public static GenericNackSm Create(DataCodings defaultEncoding, CommandStatus status, uint sequence)
        {
            return new GenericNackSm(defaultEncoding, status, sequence);
        }

        /// <summary> Called to create a GenericNack object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <returns> GenericNack </returns>
        public static GenericNackSm Create(DataCodings defaultEncoding, SmppBuffer buf, ref int offset)
        {
            GenericNackSm genericNack = new GenericNackSm(defaultEncoding);

            try
            {
                buf.ExtractHeader(genericNack, ref offset);
            }

            catch
            {
                genericNack = null;
            }

            return genericNack;
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
                
            tmpBuff.AddFinalLength();

            return tmpBuff.Buffer;
        }

        #endregion
    }
}
