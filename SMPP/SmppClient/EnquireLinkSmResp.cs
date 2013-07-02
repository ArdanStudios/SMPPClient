#region Namespaces

using System;
using System.Collections.Generic;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> Represents the response to the enquire_link PDU </summary>
    public class EnquireLinkSmResp : Header, IPacket, IPduDetails
    {
        #region Constructor

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        private EnquireLinkSmResp(DataCodings defaultEncoding) : base(defaultEncoding, CommandSet.EnquireLinkResp, SequenceGenerator.Counter)
        {
        }

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="commandStatus"></param>
        /// <param name="sequence"></param>
        private EnquireLinkSmResp(DataCodings defaultEncoding, CommandStatus commandStatus, uint sequence) : base(defaultEncoding, CommandSet.EnquireLinkResp, commandStatus, sequence)
        {
        }

        #endregion
        
        #region Factory Methods

        /// <summary> Called to create a EnquireLinkResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="status"></param>
        /// <param name="sequence"></param>
        /// <returns> EnquireLinkResp </returns>
        public static EnquireLinkSmResp Create(DataCodings defaultEncoding, CommandStatus status, uint sequence)
        {
            return new EnquireLinkSmResp(defaultEncoding, status, sequence);
        }

        /// <summary> Called to create a EnquireLinkResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="enquireLink"></param>
        /// <param name="commandStatus"></param>
        /// <returns> EnquireLinkResp </returns>
        public static EnquireLinkSmResp Create(DataCodings defaultEncoding, EnquireLinkSm enquireLink, CommandStatus commandStatus)
        {
            return new EnquireLinkSmResp(defaultEncoding, commandStatus, enquireLink.Sequence);
        }

        /// <summary> Called to create a EnquireLinkResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <returns> EnquireLinkResp </returns>
        public static EnquireLinkSmResp Create(DataCodings defaultEncoding, SmppBuffer buf, ref int offset)
        {
            EnquireLinkSmResp enquireLinkResp = new EnquireLinkSmResp(defaultEncoding);
            
            try
            {
                buf.ExtractHeader(enquireLinkResp, ref offset);
            }
            
            catch
            {
                enquireLinkResp = null;
            }

            return enquireLinkResp;
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
