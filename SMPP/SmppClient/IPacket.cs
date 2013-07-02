#region Namespaces

using System;
using System.Collections.Generic;

#endregion

namespace ArdanStudios.Common.SmppClient
{
	/// <summary> IPacket Interface </summary>
	public interface IPacket
	{
        /// <summary> Interface to support processing PDU's </summary>
		byte[] GetPDU();
	}
}
