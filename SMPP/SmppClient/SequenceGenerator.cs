#region Namespaces

using System;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> Generates sequence numbers </summary>
    internal class SequenceGenerator
    {
        #region Private Properties

        /// <summary> Provided to lock the shared resource </summary>
        private static object Locker = new object();

        /// <summary> Sequence counter </summary>
        private static uint Sequence = 0;

        /// <summary> Sequence byte counter </summary>
        private static byte ByteSequence = 0;

        /// <summary> Random generator </summary>
        private static Random Rnd = new Random();

        #endregion

        #region Public Properties

        /// <summary> Called to return the next counter </summary>
        public static uint Counter
        {
            get
			{
                lock (Locker)
                {
                    if (Sequence == 0)
                    {
                        Sequence = Convert.ToUInt32(Rnd.Next(0, Convert.ToInt32(0x7FFFFFFF)));
                    }
                    
                    if (Sequence == 0x7FFFFFFF)
                    {
                        Sequence = 1;
                    }
                    
                    Sequence++;
                }
				
				return Sequence;
			}
        }

        /// <summary> Called to return the next byte counter </summary>
        public static byte ByteCounter
        {
            get
			{
                lock (Locker)
                {
                    if (ByteSequence == 0)
                    {
                        ByteSequence = Convert.ToByte(Rnd.Next(0, Convert.ToInt32(byte.MaxValue)));
                    }
                    
                    if (ByteSequence == byte.MaxValue)
                    {
                        ByteSequence = 1;
                    }
                    
                    ByteSequence++;
                }
				
				return ByteSequence;
			}
        }

        #endregion
    }
}
