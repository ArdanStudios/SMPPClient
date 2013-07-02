#region Namespaces

using System;
using System.Collections.Generic;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> Represents the response to the bind PDU </summary>
    public class BindSmResp : Header, IPacket, IPduDetails
    {
        #region Public Properties
        
        /// <summary> User system id </summary>
        public string SystemId { get; set; }

        /// <summary> Optional Parameters </summary>
        public TLVCollection Optional { get; set; }
        
        #endregion
        
        #region Constructor

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        private BindSmResp(DataCodings defaultEncoding) : base(defaultEncoding)
        {
            Optional = new TLVCollection();
        }
        
        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="header"></param>
        private BindSmResp(DataCodings defaultEncoding, Header header) : base(defaultEncoding, header)
        {
            Optional = new TLVCollection();
        }

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="commandSet"></param>
        /// <param name="commandStatus"></param>
        /// <param name="sequence"></param>
        /// <param name="systemId"></param>
        private BindSmResp(DataCodings defaultEncoding, CommandSet commandSet, CommandStatus commandStatus, uint sequence, string systemId) : base(defaultEncoding, commandSet, commandStatus, sequence)
        {
            SystemId = systemId;
            Optional = new TLVCollection();

            /*
            // Optional Parameter for the Version
            int version = 0x34;
            byte[] b = System.BitConverter.GetBytes(version);
            
            if (System.BitConverter.IsLittleEndian)
            {
                byte tmp = b[0];
                b[0] = b[3];
                b[3] = tmp;
                
                tmp = b[1];
                b[1] = b[2];
                b[2] = tmp;
            }

            Optional.Add(TLV.Create(OptionalTags.SCInterfaceVersion, Convert.ToUInt16(b.Length), b));
            */
        }

        #endregion
        
        #region Factory Methods

        /// <summary> Called to create a PduBindResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <returns> PduBindResp </returns>
        public static BindSmResp Create(DataCodings defaultEncoding)
        {
            return new BindSmResp(defaultEncoding);
        }

        /// <summary> Called to create a PduBindResp object </summary>
       /// <param name="defaultEncoding"></param>
        /// <param name="header"></param>
        /// <returns> PduBindResp </returns>
        public static BindSmResp Create(DataCodings defaultEncoding, Header header)
        {
            return new BindSmResp(defaultEncoding, header);
        }

        /// <summary> Called to create a BindSmResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="bindSm"></param>
        /// <param name="commandSet"></param>
        /// <param name="commandStatus"></param>
        /// <param name="systemId"></param>
        /// <returns> BindSmResp </returns>
        public static BindSmResp Create(DataCodings defaultEncoding, BindSm bindSm, CommandSet commandSet, CommandStatus commandStatus, string systemId)
        {
            return new BindSmResp(defaultEncoding, commandSet, commandStatus, bindSm.Sequence, systemId);
        }

        /// <summary> Called to create a PduBindResp object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <returns> PduBindResp </returns>
        public static BindSmResp Create(DataCodings defaultEncoding, SmppBuffer buf, ref int offset)
        {
            BindSmResp bindResp = new BindSmResp(defaultEncoding);
            
            try
            {
                int start_offset = offset;

                buf.ExtractHeader(bindResp, ref offset);

                bindResp.SystemId = buf.ExtractCString(ref offset);

                while (offset - start_offset < bindResp.Length)
                {
                    bindResp.Optional.Add(buf.ExtractTLV(ref offset));
                }
            }
            
            catch
            {
                bindResp = null;
            }
            
            return bindResp;
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

                details.Add(PduData.ExtractCString("SystemId", ref offset));

                while (offset < PduData.Length)
                {
                    PduData.ExtractTLV(details, ref offset);
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

            tmpBuff.AddCString(SystemId);
                
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
