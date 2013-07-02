#region Namespaces

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArdanStudios.Common.Data;
using ArdanStudios.Common.SmppClient.Data;

#endregion

namespace ArdanStudios.Common.SmppClient.App
{
    /// <summary> Provides access to the pdu related business rules </summary>
    public class PduApp
    {
        #region Insert Methods

        /// <summary> Called to save a pdu by its property details </summary>
        /// <param name="logKey"></param>
        /// <param name="connection"></param>
        /// <param name="smscServiceId"></param>
        /// <param name="pduDirectionType"></param>
        /// <param name="details"></param>
        /// <param name="pduDataBlocks"></param>
        /// <param name="pduHeaderId"></param>
        /// <returns> ResultCodeTypes </returns>
        public static int InsertPdu(string logKey, string connection, int smscServiceId, PduDirectionTypes pduDirectionType, List<PduPropertyDetail> details, List<byte[]> pduDataBlocks, out Guid? pduHeaderId)
        {
            Console.WriteLine("ArdanStudios.Common.SmppClient.App.PduApp : InsertPdu : Started : LogKey[{0}] SMSCServiceId[{1}] PduDirectionType[{2}] DataBlocks[{3}]", logKey, smscServiceId, pduDirectionType, pduDataBlocks.Count);

            int resultType = 0;
            pduHeaderId = null;

            try
            {
                // Capture header fields
                long commandLength = (long) details[0].ValueUInt;

                CommandSet commandSet = CommandSet.Unknown;
                Enum.TryParse<CommandSet>(details[1].ValueUInt.ToString(), out commandSet);
                
                CommandStatus commandStatus = CommandStatus.Unknown;
                Enum.TryParse<CommandStatus>(details[2].ValueUInt.ToString(), out commandStatus);
                
                long sequenceNumber = (long) details[3].ValueUInt;

                if (connection != null)
                {
                    // Capture a database connection
                    using (DataTransaction dataTransaction = new DataTransaction(connection))
                    {
                        // Insert the header
                        pduHeaderId = PduData.Insert(dataTransaction, logKey, smscServiceId, pduDirectionType, commandLength, commandSet, commandStatus, sequenceNumber, details, pduDataBlocks);
                    }
                }

                // Write header
                Console.WriteLine("ArdanStudios.Common.SmppClient.App.PduApp : InsertPdu : PDU : {0} : {1} : {2} : {3}", details[0].StartingBytePosition, details[0].Name, BitConverter.ToString(details[0].DataBlock).Replace("-", " "), commandLength);
                Console.WriteLine("ArdanStudios.Common.SmppClient.App.PduApp : InsertPdu : PDU : {0} : {1} : {2} : {3}", details[1].StartingBytePosition, details[1].Name, BitConverter.ToString(details[1].DataBlock).Replace("-", " "), commandSet);
                Console.WriteLine("ArdanStudios.Common.SmppClient.App.PduApp : InsertPdu : PDU : {0} : {1} : {2} : {3}", details[2].StartingBytePosition, details[2].Name, BitConverter.ToString(details[2].DataBlock).Replace("-", " "), commandStatus);
                Console.WriteLine("ArdanStudios.Common.SmppClient.App.PduApp : InsertPdu : PDU : {0} : {1} : {2} : {3}", details[3].StartingBytePosition, details[3].Name, BitConverter.ToString(details[3].DataBlock).Replace("-", " "), sequenceNumber);

                // Write the details
                for (int detail = 4; detail < details.Count; ++detail)
                {
                    string dataBlock = null;
                    if (details[detail].DataBlock != null)
                    {
				        dataBlock = BitConverter.ToString(details[detail].DataBlock).Replace("-", " ");
                    }

                    string dataValue = null;
                    switch (details[detail].PduDataType)
                    {
                        case PduDataTypes.Byte:
                            dataValue = details[detail].ValueByte.ToString();
                            break;

                        case PduDataTypes.CString:
                        case PduDataTypes.EncodedString:
                            if (details[detail].ValueString != null)
                            {
                                dataValue = details[detail].ValueString;
                            }
                            break;

                        case PduDataTypes.UInt:
                            dataValue = details[detail].ValueUInt.ToString();
                            break;

                        case PduDataTypes.UShort:
                            dataValue = details[detail].ValueUShort.ToString();
                            break;
                    
                        case PduDataTypes.ByteArray:
                            dataValue = BitConverter.ToString(details[detail].DataBlock).Replace("-", " ");
                            break;
                    }

                    Console.WriteLine("ArdanStudios.Common.SmppClient.App.PduApp : InsertPdu : PDU : {0} : {1} : {2} : {3}", details[detail].StartingBytePosition, details[detail].Name, dataBlock, dataValue);
                }
            }

            catch (Exception exception)
            {
                resultType = 900000;

                Console.WriteLine("ArdanStudios.Common.SmppClient.App.PduApp : InsertPdu : ERROR : {0}", exception.ToString());
            }

            Console.WriteLine("ArdanStudios.Common.SmppClient.App.PduApp : InsertPdu : Completed : ResultType[{0}]", resultType);

            return resultType;
        }

        #endregion
    }
}
