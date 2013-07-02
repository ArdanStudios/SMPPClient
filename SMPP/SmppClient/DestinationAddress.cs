#region Namespaces

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> Represents SME address or/and Distribution name </summary>
    public class DestinationAddress
    {
        #region Public Properties

        /// <summary> Identifies whether destination address is a Distribution List name or SME address </summary>
        public DestinationAddressType DestType { get; set; }

        /// <summary> Destination address type of number</summary>
        public byte DestTon { get; set; }

        /// <summary> Destination address numbering plan indicator </summary>
        public byte DestNpi { get; set; }

        /// <summary> Destination phone number </summary>
        public string DestAddr { get; set; }

        #endregion

        #region Constructor

        /// <summary> Constructor </summary>
        private DestinationAddress()
        {
            DestType = DestinationAddressType.SMEAddress;
        }

        #endregion

        #region Factory Methods

        /// <summary> Called to create a DestinationAddress object </summary>
        /// <returns> DestinationAddress </returns>
        public static DestinationAddress Create()
        {
            return new DestinationAddress();
        }

        #endregion
    }
}

