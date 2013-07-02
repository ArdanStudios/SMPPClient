#region Namespaces

using System;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    /// <summary> Represents GSM User Data Header information in the short message user data </summary>
    public class UserDataHeader
    {
        #region Private Properties

        /// <summary> Identifies Information Element in the User Data Header </summary>
        private InformationElementIdentifiers _IEI;
        
        /// <summary> Information Element Data </summary>
        private byte[] _Data;

        #endregion
        
        #region Public Properties

        /// <summary> Identifies Information Element id </summary>
        private byte IEI_Id { get; set; }

        /// <summary> Identifies Information Element in the User Data Header </summary>
        public InformationElementIdentifiers IEI { get { return _IEI; } set { _IEI = value; IEI_Id = (byte) value; } }
        
        /// <summary> Length of Information Element </summary>
        public byte Length { get; set; }
        
        /// <summary> Information Element Data </summary>
        public byte[] Data { get { return _Data; } set { _Data = value; Length = Convert.ToByte(_Data.Length); } }

        #endregion
        
        #region Constructor
        
        /// <summary> Constructor </summary>
        private UserDataHeader()
        {
        }

        /// <summary> Constructor </summary>
        /// <param name="iei"></param>
        /// <param name="len"></param>
        /// <param name="data"></param>
        private UserDataHeader(byte iei, byte len, byte[] data)
        {
            IEI_Id = iei;
            Length = len;
            _Data = data;
            
            object obj = InformationElementIdentifiers.Parse(typeof(InformationElementIdentifiers), IEI_Id.ToString());
            if (obj != null)
            {
                _IEI = (InformationElementIdentifiers) obj;
            }
            else
            {
                _IEI = InformationElementIdentifiers.Unknown;
            }
        }

        /// <summary> Constructor </summary>
        /// <param name="iei"></param>
        /// <param name="len"></param>
        /// <param name="data"></param>
        private UserDataHeader(InformationElementIdentifiers iei, byte len, byte[] data)
        {
            IEI_Id = Convert.ToByte(iei);
            Length = len;
            _Data = data;
            _IEI = iei;
        }
        
        #endregion

        #region Factory Methods

        /// <summary> Called to create a UserDataHeader object </summary>
        /// <returns> UserDataHeader </returns>
        internal static UserDataHeader Create()
        {
            return new UserDataHeader();
        }

        /// <summary> Called to create a UserDataHeader object </summary>
        /// <param name="iei"></param>
        /// <param name="len"></param>
        /// <param name="data"></param>
        /// <returns> UserDataHeader </returns>
        internal static UserDataHeader Create(byte iei, byte len, byte[] data)
        {
            return new UserDataHeader(iei, len, data);
        }

        /// <summary> Called to create a UserDataHeader object </summary>
        /// <param name="iei"></param>
        /// <param name="len"></param>
        /// <param name="data"></param>
        /// <returns> UserDataHeader </returns>
        internal static UserDataHeader Create(InformationElementIdentifiers iei, byte len, byte[] data)
        {
            return new UserDataHeader(iei, len, data);
        }
        
        #endregion
    }
}
