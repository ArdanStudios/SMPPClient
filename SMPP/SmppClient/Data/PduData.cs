#region Namespaces

using System;
using System.Collections.Generic;
using System.Data.Common;
using ArdanStudios.Common.Data;
using ArdanStudios.Common.SmppClient.DataTypes;

#endregion

namespace ArdanStudios.Common.SmppClient.Data
{
    /// <summary> Provides access to the pdu releated database tables </summary>
    internal static class PduData
    {
        #region Insert Methods

        /// <summary> Called to save a pdu into the database </summary>
        /// <param name="dataTransaction"></param>
        /// <param name="logKey"></param>
        /// <param name="smscServiceId"></param>
        /// <param name="pduDirectionType"></param>
        /// <param name="commandLength"></param>
        /// <param name="commandSet"></param>
        /// <param name="commandStatus"></param>
        /// <param name="sequenceNumber"></param>
        /// <param name="details"></param>
        /// <param name="pduDataBlocks"></param>
        /// <returns> long </returns>
        public static Guid? Insert(DataTransaction dataTransaction, string logKey, int smscServiceId, PduDirectionTypes pduDirectionType, long commandLength,
                                   CommandSet commandSet, CommandStatus commandStatus, long sequenceNumber, List<PduPropertyDetail> details, List<byte[]> pduDataBlocks)
        {
            Guid? pduHeaderId = null;

            // Prepare the database call
            using (DbCommand dbCommand = dataTransaction.GetStoredProcCommand("spSMPP_Pdu_Insert"))
            {
                // Prepare the call
                dataTransaction.AddInParameterString(dbCommand, "LogKey", logKey);
                dataTransaction.AddInParameterInteger(dbCommand, "SMSCServiceID", smscServiceId);
                dataTransaction.AddInParameterShort(dbCommand, "Direction", (short) pduDirectionType);
                dataTransaction.AddInParameterLong(dbCommand, "CommandLength", commandLength);
                dataTransaction.AddInParameterString(dbCommand, "CommandId", commandSet.ToString());
                dataTransaction.AddInParameterString(dbCommand, "CommandStatusId", commandStatus.ToString());
                dataTransaction.AddInParameterLong(dbCommand, "SequenceNumber", sequenceNumber);
                dataTransaction.AddInParameterTable(dbCommand, "PduPropertyDetails", new PduPropertyDetails(details));
                dataTransaction.AddInParameterTable(dbCommand, "PduDataBlocks", new SMSCPduDataBlocks(pduDataBlocks));

                dataTransaction.AddOutParameterGuid(dbCommand, "PduHeaderId");

                dataTransaction.ExecuteNonQuery(dbCommand);

                pduHeaderId = dataTransaction.GetParameterValueGuid(dbCommand, "PduHeaderId");
            }

            return pduHeaderId;
        }

        #endregion
    }
}
