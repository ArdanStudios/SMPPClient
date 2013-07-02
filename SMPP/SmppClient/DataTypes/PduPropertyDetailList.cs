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
    /// <summary> Provides support for inserting pdu's into the sql server </summary>
    public class PduPropertyDetails : List<PduPropertyDetail>, IEnumerable<SqlDataRecord>
    {
        #region Constructor

        /// <summary> Constructor </summary>
        /// <param name="pduPropertyDetails"></param>
        public PduPropertyDetails(List<PduPropertyDetail> pduPropertyDetails)
        {
            this.AddRange(pduPropertyDetails);
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
								        	  new SqlMetaData("PduDataTypeId", SqlDbType.TinyInt),
								        	  new SqlMetaData("DataBlock", SqlDbType.NVarChar, SqlMetaData.Max),
								        	  new SqlMetaData("PropertyName", SqlDbType.NVarChar, 50),
								        	  new SqlMetaData("Value", SqlDbType.NVarChar, 255)
								          );

			foreach (PduPropertyDetail pduPropertyDetail in this)
			{
				sqlDataRecord.SetInt32(0, pduPropertyDetail.StartingBytePosition);
				sqlDataRecord.SetByte(1, (byte) pduPropertyDetail.PduDataType);

                if (pduPropertyDetail.DataBlock != null)
                {
				    sqlDataRecord.SetValue(2, BitConverter.ToString(pduPropertyDetail.DataBlock).Replace("-", " "));
                }
                else
                {
                    sqlDataRecord.SetValue(2, null);
                }

				sqlDataRecord.SetString(3, pduPropertyDetail.Name);

                switch (pduPropertyDetail.PduDataType)
                {
                    case PduDataTypes.Byte:
                        sqlDataRecord.SetString(4, pduPropertyDetail.ValueByte.ToString());
                        break;

                    case PduDataTypes.CString:
                    case PduDataTypes.EncodedString:
                        if (pduPropertyDetail.ValueString != null)
                        {
                            sqlDataRecord.SetString(4, pduPropertyDetail.ValueString);
                        }
                        else
                        {
                            sqlDataRecord.SetValue(4, null);
                        }
                        break;

                    case PduDataTypes.UInt:
                        sqlDataRecord.SetString(4, pduPropertyDetail.ValueUInt.ToString());
                        break;

                    case PduDataTypes.UShort:
                        sqlDataRecord.SetString(4, pduPropertyDetail.ValueUShort.ToString());
                        break;
                    
                    case PduDataTypes.ByteArray:
                        sqlDataRecord.SetValue(4, BitConverter.ToString(pduPropertyDetail.DataBlock).Replace("-", " "));
                        break;

                    default:
                        sqlDataRecord.SetValue(4, null);
                        break;
                }

				yield return sqlDataRecord;
			}
        }

        #endregion
    }
}
