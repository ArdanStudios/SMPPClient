#region Namespaces

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    #region PduDataTypes

    /// <summary> Set of possible data types for a pdu property </summary>
    public enum PduDataTypes
    {
        /// <summary> Unsigned Integer </summary>
        UInt = 1,

        /// <summary> Unsigned Short </summary>
        UShort = 2,

        /// <summary> Byte </summary>
        Byte = 3,

        /// <summary> Byte Array </summary>
        ByteArray = 4,

        /// <summary> C String </summary>
        CString = 5,

        /// <summary> Encoded String </summary>
        EncodedString = 6
    }

    #endregion

    #region PduPropertyDetail

    /// <summary> Contains details for a single property in the PDU </summary>
    public class PduPropertyDetail
    {
        #region Properties

        /// <summary> The starting byte position in the PDU for this property </summary>
        public int StartingBytePosition { get; set; }

        /// <summary> The type of data </summary>
        public PduDataTypes PduDataType { get; set; }

        /// <summary> The name of the property </summary>
        public string Name { get; set; }

        /// <summary> The block of data for this property </summary>
        public byte[] DataBlock { get; set; }

        /// <summary> The unsigned int value </summary>
        public uint ValueUInt { get; set; }

        /// <summary> The unsigned short value </summary>
        public ushort ValueUShort { get; set; }

        /// <summary> The byte value </summary>
        public byte ValueByte { get; set; }

        /// <summary> The string value </summary>
        public string ValueString { get; set; }

        #endregion

        #region Constructor

        /// <summary> Constructor </summary>
        /// <param name="startingBytePosition"></param>
        /// <param name="pduDataType"></param>
        /// <param name="name"></param>
        public PduPropertyDetail(int startingBytePosition, PduDataTypes pduDataType, string name)
        {
            StartingBytePosition = startingBytePosition;
            PduDataType = pduDataType;
            Name = name;

            DataBlock = null;
        }

        #endregion
    }

    #endregion
}
