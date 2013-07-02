#region Namespaces

using System;
using System.Collections.Generic;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> Represents the response to the cancel_sm PDU </summary>
    public class CancelSmResp : Header, IPacket, IPduDetails
    {
        #region Constructor

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        private CancelSmResp(DataCodings defaultEncoding) : base(defaultEncoding, CommandSet.CancelSmResp)
        {
        }

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="commandStatus"></param>
        private CancelSmResp(DataCodings defaultEncoding, CommandStatus commandStatus) : base(defaultEncoding, CommandSet.CancelSmResp, commandStatus)
        {
        }

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="commandStatus"></param>
        /// <param name="sequence"></param>
        private CancelSmResp(DataCodings defaultEncoding, CommandStatus commandStatus, uint sequence) : base(defaultEncoding, CommandSet.CancelSmResp, commandStatus, sequence)
        {
        }

        #endregion
        
        #region Factory Methods

        /// <summary> Called to create a CancelSmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        internal static CancelSmResp Create(DataCodings defaultEncoding, CommandStatus status)
        {
            return new CancelSmResp(defaultEncoding, status);
        }

        /// <summary> Called to create a CancelSmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="cancelSm"></param>
        /// <param name="status"></param>
        /// <returns> CancelSmResp </returns>
        public static CancelSmResp Create(DataCodings defaultEncoding, CancelSm cancelSm, CommandStatus status)
        {
            return new CancelSmResp(defaultEncoding, status, cancelSm.Sequence);
        }

        /// <summary> Called to create a DataSmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="status"></param>
        /// <param name="sequence"></param>
        /// <returns> CancelSmResp </returns>
        public static CancelSmResp Create(DataCodings defaultEncoding, CommandStatus status, uint sequence)
        {
            return new CancelSmResp(defaultEncoding, status, sequence);
        }

        /// <summary> Called to create a CancelSmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <returns> CancelSmResp </returns>
        internal static CancelSmResp Create(DataCodings defaultEncoding, SmppBuffer buf, ref int offset)
        {
            CancelSmResp cancelSmResp = new CancelSmResp(defaultEncoding);
            
            try
            {
                buf.ExtractHeader(cancelSmResp, ref offset);
            }
            
            catch
            {
                cancelSmResp = null;
            }

            return cancelSmResp;
        }

        #endregion

        #region PDU Detail Methods

        /// <summary> Called to return a list of property details from the PDU </summary>
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
        byte[] IPacket.GetPDU()
        {
            // Create buffer and add header information
            SmppBuffer tmpBuff = new SmppBuffer(DefaultEncoding, this);

            // Insert final length
            tmpBuff.AddFinalLength();

            // Return byte array
            return tmpBuff.Buffer;
        }

        #endregion
    }
}
