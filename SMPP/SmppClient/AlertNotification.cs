#region Namespaces

using System;
using System.Collections.Generic;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> This message is sent by the SMSC (Short Message Service Centre) to the ESME (Extended Short Message Entity),
    /// when the SMSC has detected that a particular mobile subscriber has become available and a delivery
    /// pending flag had been set for that subscriber from a previous data_sm operation </summary>
    public class AlertNotification : Header, IPduDetails
    {
        #region Public Properties
        
        /// <summary> Source address type of number </summary>
        public byte SourceAddrTon { get; set; }

        /// <summary> Source address numbering plan indicator </summary>
        public byte SourceAddrNpi { get; set; }

        /// <summary> Source phone number </summary>
        public string SourceAddr { get; set; }
        
        /// <summary> Destination address type of number </summary>
        public byte EsmeAddrTon { get; set; }

        /// <summary> Destination address numbering plan indicator </summary>
        public byte EsmeAddrNpi { get; set; }

        /// <summary> Destination phone number </summary>
        public string EsmeAddr { get; set; }

        /// <summary> Optional Parameters</summary>
        public TLVCollection Optional { get; set; }
        
        #endregion

        #region Constructor

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        private AlertNotification(DataCodings defaultEncoding) : base(defaultEncoding, CommandSet.AlertNotification)
        {
            Optional = new TLVCollection();
        }

        #endregion

        #region Factory Methods

        /// <summary> Called to create a AlertNotification object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <returns> AlertNotification </returns>
        public static AlertNotification Create(DataCodings defaultEncoding, SmppBuffer buf, ref int offset)
        {
            AlertNotification alertNotification = new AlertNotification(defaultEncoding);

            try
            {
                int start_offset = offset;

                buf.ExtractHeader(alertNotification, ref offset);

                alertNotification.SourceAddrTon = buf.ExtractByte(ref offset);
                alertNotification.SourceAddrNpi = buf.ExtractByte(ref offset);
                alertNotification.SourceAddr = buf.ExtractCString(ref offset);
                alertNotification.EsmeAddrTon = buf.ExtractByte(ref offset);
                alertNotification.EsmeAddrNpi = buf.ExtractByte(ref offset);
                alertNotification.EsmeAddr = buf.ExtractCString(ref offset);

                while (offset - start_offset < alertNotification.Length)
                {
                    alertNotification.Optional.Add(buf.ExtractTLV(ref offset));
                }
            }

            catch
            {
                alertNotification = null;
            }

            return alertNotification;
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

                details.Add(PduData.ExtractByte("SourceAddrTon", ref offset));
                details.Add(PduData.ExtractByte("SourceAddrNpi", ref offset));
                details.Add(PduData.ExtractCString("SourceAddr", ref offset));
                details.Add(PduData.ExtractByte("EsmeAddrTon", ref offset));
                details.Add(PduData.ExtractByte("EsmeAddrNpi", ref offset));
                details.Add(PduData.ExtractCString("EsmeAddr", ref offset));

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
    }
}
