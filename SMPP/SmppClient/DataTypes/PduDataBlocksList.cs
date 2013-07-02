#region Namespaces

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Server;
using System.Data;

#endregion

namespace ArdanStudios.Common.SmppClient.DataTypes
{
    /// <summary> SQL Server Insert support </summary>
    public class SMSCPduDataBlocks : List<byte[]>, IEnumerable<SqlDataRecord>
    {
        #region Constructor

        /// <summary> Constructor </summary>
        /// <param name="pduDataBlocks"></param>
        public SMSCPduDataBlocks(List<byte[]> pduDataBlocks)
        {
            this.AddRange(pduDataBlocks);
        }

        #endregion

        #region Enumerator

        /// <summary> Provides an enumerator for the list of PduPropertyDetails </summary>
        /// <returns> IEnumerator SqlDataRecord </returns>
        IEnumerator<SqlDataRecord> IEnumerable<SqlDataRecord>.GetEnumerator()
		{
			SqlDataRecord sqlDataRecord = new SqlDataRecord
								          (
								        	  new SqlMetaData("StartingBytePosition", SqlDbType.Int),
								        	  new SqlMetaData("DataBlock", SqlDbType.VarBinary, SqlMetaData.Max)
								          );

            int sequence = 0;
			foreach (byte[] dataBlock in this)
			{
				sqlDataRecord.SetInt32(0, ++sequence);
                sqlDataRecord.SetValue(1, dataBlock);

				yield return sqlDataRecord;
			}
        }

        #endregion
    }
}
