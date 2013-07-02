#region Namespaces

using System;
using System.Collections.Generic;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> Register an instance of an ESME (Extended Short Message Entity) with the SMSC (Short Message Service Centre)
    /// system and request an SMPP session over this network connection for the submission or
    /// delivery of messages </summary>
    public class BindSm : Header, IPacket, IPduDetails
    {
        #region Public Properties
        
        /// <summary> User system id </summary>
        public string SystemId { get; set; }

        /// <summary> User password </summary>
        public string Password { get; set; }

        /// <summary> The type of ESME that is binding to the SMSC </summary>
        public string SystemType { get; set; }

        /// <summary> Indicate the version of the SMPP protocol </summary>
        private byte InterfaceVersion { get; set; }

        /// <summary> Source address type of number </summary>
        public byte AddrTon { get; set; }
        
        /// <summary> Source address numbering plan indicator </summary>
        public byte AddrNpi { get; set; }
        
        /// <summary> Identifies a set of SME addresses. Regular Expression notation should be used to
        /// specify a range of addresses </summary>
        public string AddressRange { get; set; }
        
        #endregion
        
        #region Constructor
        
        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        private BindSm(DataCodings defaultEncoding) : base (defaultEncoding, CommandSet.BindTransceiver, SequenceGenerator.Counter)
        {
            InterfaceVersion = 0x34;
        }

        #endregion
        
        #region Factory Methods

        /// <summary> Create a new pduBind object </summary>
        /// <param name="defaultEncoding"></param>
        /// <returns> PduBind </returns>
        public static BindSm Create(DataCodings defaultEncoding)
        {
            return new BindSm(defaultEncoding);
        }

        /// <summary> Called to create a PduBind object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <returns> BindSm </returns>
        public static BindSm Create(DataCodings defaultEncoding, SmppBuffer buf, ref int offset)
        {
            BindSm bindSm = new BindSm(defaultEncoding);
            
            try
            {
                int start_offset = offset;

                buf.ExtractHeader(bindSm, ref offset);

                bindSm.SystemId = buf.ExtractCString(ref offset);
                bindSm.Password = buf.ExtractCString(ref offset);
                bindSm.SystemType = buf.ExtractCString(ref offset);
                bindSm.InterfaceVersion = buf.ExtractByte(ref offset);
                bindSm.AddrTon = buf.ExtractByte(ref offset);
                bindSm.AddrNpi = buf.ExtractByte(ref offset);
                bindSm.AddressRange = buf.ExtractCString(ref offset);
            }
            
            catch
            {
                bindSm = null;
            }
            
            return bindSm;
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
                details.Add(PduData.ExtractCString("Password", ref offset));
                details.Add(PduData.ExtractCString("SystemType", ref offset));
                details.Add(PduData.ExtractByte("InterfaceVersion", ref offset));
                details.Add(PduData.ExtractByte("AddrTon", ref offset));
                details.Add(PduData.ExtractByte("AddrNpi", ref offset));
                details.Add(PduData.ExtractCString("AddressRange", ref offset));
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
            tmpBuff.AddCString(Password);
            tmpBuff.AddCString(SystemType);
            tmpBuff.AddByte(InterfaceVersion);
            tmpBuff.AddByte(AddrTon);
            tmpBuff.AddByte(AddrNpi);
            tmpBuff.AddCString(AddressRange);
                
            tmpBuff.AddFinalLength();

            return tmpBuff.Buffer;
        }

        #endregion
    }
}
