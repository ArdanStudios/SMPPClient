#region Namespaces

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> Represents SME address or/and Distribution name to which submission was unsuccessful </summary>
    public class UnsuccessDestinationAddress
    {
        #region Public Properties

        /// <summary> Destination address type of number </summary>
        public byte DestTon { get; set; }

        /// <summary> Destination address numbering plan indicator </summary>
        public byte DestNpi { get; set; }

        /// <summary> Destination phone number </summary>
        public string DestAddr { get; set; }

        /// <summary> Indicates the success or failure of the request to this SME address </summary>
        public CommandStatus ErrorStatus { get; set; }

        #endregion

        #region Constructor

        /// <summary> Constructor </summary>
        private UnsuccessDestinationAddress()
        {
        }

        #endregion
        
        #region Factory Methods

        /// <summary> Called to create a UnsuccessDestinationAddress object </summary>
        /// <returns> UnsuccessDestinationAddress </returns>
        internal static UnsuccessDestinationAddress Create()
        {
            return new UnsuccessDestinationAddress();
        }

        #endregion
    }
}
