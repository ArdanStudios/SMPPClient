#region Namespaces

using System;
using System.Collections.Generic;

#endregion

namespace ArdanStudios.Common.SmppClient
{
	/// <summary> IPduDetails Interface </summary>
	public interface IPduDetails
	{
        /// <summary> Returns details about the PDU </summary>
        /// <returns> List PduPropertyDetail </returns>
		List<PduPropertyDetail> Details();
	}
}
