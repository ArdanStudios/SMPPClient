#region Namespaces

using System;
using System.Collections.Generic;

#endregion

namespace ArdanStudios.Common.SmppClient
{
	/// <summary> Represents the response to the unbind PDU </summary>
	public class UnBindSmResp : Header, IPacket, IPduDetails
	{
        #region Constructor
        
        /// <summary> Constructor </summary>
		/// <param name="defaultEncoding"></param>
        private UnBindSmResp(DataCodings defaultEncoding) : base(defaultEncoding, CommandSet.UnbindResp)
		{
		}

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="commandStatus"></param>
		private UnBindSmResp(DataCodings defaultEncoding, CommandStatus commandStatus) : base(defaultEncoding, CommandSet.UnbindResp, commandStatus)
		{
		}

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="commandStatus"></param>
        /// <param name="sequence"></param>
		private UnBindSmResp(DataCodings defaultEncoding, CommandStatus commandStatus, uint sequence) : base(defaultEncoding, CommandSet.UnbindResp, commandStatus, sequence)
		{
		}
		
        #endregion
        
        #region Factory Methods
        
        /// <summary> Called to create a PduUnBindResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="commandStatus"></param>
        /// <returns> PduUnBindResp </returns>
        internal static UnBindSmResp Create(DataCodings defaultEncoding, CommandStatus commandStatus)
        {
            return new UnBindSmResp(defaultEncoding, commandStatus);
        }

        /// <summary> Called to create a SubmitMultiResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="commandStatus"></param>
        /// <param name="sequence"></param>
        /// <returns> UnBindSmResp </returns>
        public static UnBindSmResp Create(DataCodings defaultEncoding, CommandStatus commandStatus, uint sequence)
        {
            return new UnBindSmResp(defaultEncoding, commandStatus, sequence);
        }

        /// <summary> Called to create a PduUnBindResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="unBindSm"></param>
        /// <param name="commandStatus"></param>
        /// <returns> PduUnBindResp </returns>
        internal static UnBindSmResp Create(DataCodings defaultEncoding, UnBindSm unBindSm, CommandStatus commandStatus)
        {
            return new UnBindSmResp(defaultEncoding, commandStatus, unBindSm.Sequence);
        }

        /// <summary> Called to create a PduUnBindResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <returns> PduUnBindResp </returns>
		internal static UnBindSmResp Create(DataCodings defaultEncoding, SmppBuffer buf, ref int offset)
		{
            UnBindSmResp res = new UnBindSmResp(defaultEncoding);

            try
            {
                buf.ExtractHeader(res, ref offset);
            }
            
            catch
            {
                res = null;
            }

            return res;
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

        /// <summary> Called to return the buffer of byte for this object </summary>
        /// <returns> A byte array </returns>
        public byte[] GetPDU()
		{
			SmppBuffer tmpBuff = new SmppBuffer(DefaultEncoding, this);
			
			tmpBuff.AddFinalLength();

            return tmpBuff.Buffer;
		}

		#endregion
	}
}
