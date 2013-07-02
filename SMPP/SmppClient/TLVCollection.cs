#region Namespaces

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> Collection of optional parametersin TLV format </summary>
    public class TLVCollection : List<TLV>
    {
        #region Operator Properties

        /// <summary> Access to the collection by tag </summary>
        /// <param name="tag"></param>
        /// <returns> A TLV object </returns>
        public TLV this[ushort tag] { get { return this.FirstOrDefault(k => k.Tag == tag); } }

        /// <summary> Access to the collection by an optional tag </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public TLV this[OptionalTags tag] { get { return this[Convert.ToUInt16(tag)]; } }

        #endregion
        
        #region Public Methods

        /// <summary> Adds the SAR reference number </summary>
        /// <param name="val"></param>
        public void AddSARReferenceNumber(ushort val)
        {
            SmppBuffer tmpBuff = new SmppBuffer(DataCodings.Default);
            tmpBuff.AddShort(val);

            this.Add(TLV.Create(OptionalTags.SarMsgRefNum, Convert.ToUInt16(tmpBuff.Length), tmpBuff.Buffer));
        }

        /// <summary> Adds the SAR sequence number </summary>
        /// <param name="val"></param>
        public void AddSARSequenceNumber(byte val)
        {
            SmppBuffer tmpBuff = new SmppBuffer(DataCodings.Default);
            tmpBuff.AddByte(val);

            Add(TLV.Create(OptionalTags.SarSegmentSeqnum, Convert.ToUInt16(tmpBuff.Length), tmpBuff.Buffer));
        }

        /// <summary> Adds the SAR total segments </summary>
        /// <param name="val"></param>
        public void AddSARTotalSegments(byte val)
        {
            SmppBuffer tmpBuff = new SmppBuffer(DataCodings.Default);
            tmpBuff.AddByte(val);

            Add(TLV.Create(OptionalTags.SarTotalSegments, Convert.ToUInt16(tmpBuff.Length), tmpBuff.Buffer));
        }

        /// <summary> Adds the more messages to send </summary>
        /// <param name="val"></param>
        public void AddMoreMessagesToSend(bool val)
        {
            byte b = 0;
            if (val)
            {
                b = 1;
            }

            SmppBuffer tmpBuff = new SmppBuffer(DataCodings.Default);
            tmpBuff.AddByte(b);

            Add(TLV.Create(OptionalTags.MoreMessagesToSend, Convert.ToUInt16(tmpBuff.Length), tmpBuff.Buffer));
        }

        /// <summary> Adds the more message pay load </summary>
        /// <param name="data"></param>
        public void AddMessagePayload(byte[] data)
        {
            SmppBuffer tmpBuff = new SmppBuffer(DataCodings.Default);
            tmpBuff.AddBytes(data);

            Add(TLV.Create(OptionalTags.MessagePayload, Convert.ToUInt16(tmpBuff.Length), tmpBuff.Buffer));
        }

        /// <summary> Adds the specified optional tag value </summary>
        /// <param name="tag"></param>
        /// <param name="val"></param>
        public void Add(OptionalTags tag, byte[] val)
        {
            Add(TLV.Create(tag, Convert.ToUInt16(val.Length), val));
        }

        #endregion
    }
}
