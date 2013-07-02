#region Namespaces

using System;
using System.Collections.Generic;

#endregion

namespace ArdanStudios.Common.SmppClient
{
	/// <summary> UserData class contains the user data of PDU </summary>
	public class UserData
	{
        #region Private Properties
        
        /// <summary> The octets of the text message </summary>
		private List<byte> _ShortMessage;

        #endregion
        
        #region Public Properties
        
        /// <summary> The octets of the text message </summary>
		public byte[] ShortMessage
        {
            get
            {
                return _ShortMessage.ToArray();
            }
            
            set
            {
                _ShortMessage = new List<byte>(value);
            }
        }

		/// <summary> GSM User Data Header information in the short message </summary>
		public UserDataHeaderCollection Headers { get; set; }
        
        #endregion
        
        #region Constructor
        
        /// <summary> Constructor </summary>
		private UserData()
		{
            _ShortMessage = new List<byte>();
			Headers = new UserDataHeaderCollection();
		}
		
		#endregion

        #region Helper Methods

        /// <summary> Called to covnert the short message into a string </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="dataCoding"></param>
        /// <returns> string </returns>
        public string ShortMessageText(DataCodings defaultEncoding, DataCodings dataCoding)
        {
            SmppBuffer smppBuff = new SmppBuffer(defaultEncoding, ShortMessage);

            return smppBuff.ExtractEncodedString(dataCoding);
        }

        #endregion
		
		#region Factory Methods

        /// <summary> Called to create a UserData object </summary>
        /// <returns> UserData </returns>
        public static UserData Create()
        {
            return new UserData();
        }

        /// <summary> Called to create a UserData object </summary>
        /// <param name="buf"></param>
        /// <param name="udhi"></param>
        /// <returns> UserData</returns>
		public static UserData Create(SmppBuffer buf, bool udhi)
		{
            return buf.ExtractUserData(udhi, 0);
		}

        #endregion

        #region Public Properties

        /// <summary> Called to add a new user data object to the buffer </summary>
        /// <param name="userData"></param>
        public void Add(UserData userData)
        {
            foreach (UserDataHeader userDataHeader in userData.Headers)
            {
                Headers.Add(userDataHeader);
            }

            _ShortMessage.AddRange(userData.ShortMessage);
        }

        #endregion
	}
}
