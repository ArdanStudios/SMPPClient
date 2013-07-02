#region Namespaces

using System;

#endregion

namespace ArdanStudios.Common.SmppClient
{
	/// <summary> Represents TLV (Tag, Length, Value) format for optional parameters </summary>
	public class TLV
	{
        #region Public Properties
        
        /// <summary> The Tag field is used to uniquely identify the particular optional parameter </summary>
		public ushort Tag { get; set; }

        /// <summary> Indicates the length of the Value property in octets </summary>
		public ushort Length { get; set; }

        /// <summary> Contains the actual data for the optional parameter </summary>
		public byte[] Value { get; set; }
		
		/// <summary> Identifies the particular optional parameter </summary>
		public OptionalTags TagValue { get { return (OptionalTags) Tag; } }
		
		#endregion
		
		#region Constructor

        /// <summary> Constructor </summary>
		private TLV()
		{
		}

        /// <summary> Constructor </summary>
        /// <param name="tag"></param>
        /// <param name="length"></param>
        /// <param name="value"></param>
		private TLV(ushort tag, ushort length, byte[] value)
		{
			Tag = tag;
			Length = length;
			Value = value;
		}

        /// <summary> Constructor </summary>
        /// <param name="tag"></param>
        /// <param name="length"></param>
        /// <param name="value"></param>
		private TLV(OptionalTags tag, ushort length, byte[] value)
		{
			Tag = Convert.ToUInt16(tag);
			Length = length;
			Value = value;
		}

        #endregion

        #region Factory Methods

        /// <summary> Called to create a TLV object </summary>
        /// <returns> TLV </returns>
        public static TLV Create()
        {
            return new TLV();
        }

        /// <summary> Called to create a TLV object </summary>
        /// <param name="tag"></param>
        /// <param name="length"></param>
        /// <param name="value"></param>
        /// <returns> TLV </returns>
        public static TLV Create(ushort tag, ushort length, byte[] value)
        {
            return new TLV(tag, length, value);
        }

        /// <summary> Called to create a TLV object </summary>
        /// <param name="tag"></param>
        /// <param name="length"></param>
        /// <param name="value"></param>
        /// <returns> TLV </returns>
        public static TLV Create(OptionalTags tag, ushort length, byte[] value)
        {
            return new TLV(tag, length, value);
        }

        #endregion
	}
}
