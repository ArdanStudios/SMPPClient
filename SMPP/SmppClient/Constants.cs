#region Namespaces

using System;

#endregion

namespace ArdanStudios.Common.SmppClient
{
    #region DataCodings Enum
    
    /// <summary> Indicates the encoding scheme of the short message </summary>
    public enum DataCodings : byte
    {
        /// <summary> SMSC Default Alphabet (0x0) </summary>
        Default = 0x0,
        
        /// <summary> IA5 (CCITT T.50)/ASCII (ANSI X3.4) (0x1) </summary>
        ASCII = 0x1,
        
        /// <summary> NOT SUPPORTED : Octet unspecified (8-bit binary) (0x2) </summary>
        Octets = 0x2,
        
        /// <summary> Latin 1 (ISO-8859-1) (0x3) </summary>
        Latin1 = 0x3,
        
        /// <summary> NOT SUPPORTED :  Octet unspecified (8-bit binary) (0x4) </summary>
        OctetUnspecified = 0x4,
        
        /// <summary> NOT SUPPORTED :  Cyrllic (ISO-8859-5)(0x6) </summary>
        Cyrllic = 0x6,
        
        /// <summary> NOT SUPPORTED :  Latin/Hebrew (ISO-8859-8) (0x7) </summary>
        LatinHebrew = 0x7,
        
        /// <summary> UCS2 (ISO/IEC-10646) (0x8) </summary>
        UCS2 = 0x8,
        
        /// <summary> NOT SUPPORTED :  Flash SMS </summary>
        DefaultFlashSMS = 0x10,
        
        /// <summary> NOT SUPPORTED :  Flash SMS with Unicode characters (0x18) </summary>
        UnicodeFlashSMS = 0x18,

        /// <summary> NOT SUPPORTED :  Latin 1 (ISO-8859-1) (0x3) </summary>
        Latin1Escape = 0xC0,
        
        /// <summary> NOT SUPPORTED :  Message Class 0 (0xF0) </summary>
        Class0 = 0xF0,
        
        /// <summary> NOT SUPPORTED :  Message Class 1 (0xF1) </summary>
        Class1 = 0xF1,
        
        /// <summary> NOT SUPPORTED :  Message Class 2 (0xF2) </summary>
        Class2 = 0xF2,
        
        /// <summary> NOT SUPPORTED :  Message Class 3 (0xF3) </summary>
        Class3 = 0xF3,
        
        /// <summary> NOT SUPPORTED :  Class 0 Alert 8bit data (0xF4) </summary>
        Class0Alert8Bit = 0xF4,
        
        /// <summary> NOT SUPPORTED :  Class 1 ME specific 8-bit data (0xF5) </summary>
        Class1ME8Bit = 0xF5
    }

    #endregion
    
    #region ConnectionStatus Enum
    
    /// <summary> Client connection status </summary>
    public enum ConnectionStatus
    {
        /// <summary> Client connected, but not bound </summary>
        Open,
        
        /// <summary> Client bound </summary>
        Bound,
        
        /// <summary> Connection closed </summary>
        Closed
    }
    
    #endregion
    
    #region ConnectionMode Enum

    /// <summary> ESME (Extended Short Message Entity) may bind to the SMSC (Short Message Service Centre) as either a
    /// Transmitter (called ESME Transmitter), a Receiver (called ESME Receiver) or a Transceiver (called ESME Transceiver) </summary>
    public enum ConnectionModes
    {
        /// <summary> An ESME bound as a Transmitter is authorised to send short messages to the SMSC and to
        /// receive the corresponding SMPP responses from the SMSC </summary>
        Transmitter,
        
        /// <summary> An ESME bound as a Receiver is authorised to receive short messages from the SMSC and to
        /// return the corresponding SMPP message responses to the SMSC </summary>
        Receiver,
        
        /// <summary> An ESME bound as a Transceiver is allowed to send messages to the SMSC and receive
        /// messages from the SMSC over a single SMPP session </summary>
        Transceiver
    }
    
    #endregion
    
    #region CommandSet Enum

    /// <summary> The CommandSet identifies the type of message the SMPP PDU represents
    /// 5.1.2.1 table 5.1 </summary>
    public enum CommandSet : uint 
    {
        /// <summary> Defined by us to be able to save unknown PDUs </summary>
        Unknown = 0x70000000,

        /// <summary> Generic negative acknowledgement to an SMPP PDU submitted with an invalid message header </summary>
        GenericNack = 0x80000000,
        
        /// <summary></summary>
        BindReceiver = 0x00000001,
        
        /// <summary></summary>
        BindReceiverResp = 0x80000001,
        
        /// <summary></summary>
        BindTransmitter = 0x00000002,
        
        /// <summary></summary>
        BindTransmitterResp = 0x80000002,
        
        /// <summary></summary>
        QuerySm = 0x00000003,
        
        /// <summary></summary>
        QuerySmResp = 0x80000003,
        
        /// <summary></summary>
        SubmitSm = 0x00000004,
        
        /// <summary></summary>
        SubmitSmResp = 0x80000004,
        
        /// <summary></summary>
        DeliverSm = 0x00000005,
        
        /// <summary></summary>
        DeliverSmResp = 0x80000005,
        
        /// <summary></summary>
        Unbind = 0x00000006,
        
        /// <summary></summary>
        UnbindResp = 0x80000006,
        
        /// <summary></summary>
        ReplaceSm = 0x00000007,
        
        /// <summary></summary>
        ReplaceSmResp = 0x80000007,
        
        /// <summary></summary>
        CancelSm = 0x00000008,
        
        /// <summary></summary>
        CancelSmResp = 0x80000008,
        
        /// <summary></summary>
        BindTransceiver = 0x00000009,
        
        /// <summary></summary>
        BindTransceiverResp = 0x80000009,
        
        /// <summary></summary>
        Outbind = 0x0000000B,
        
        /// <summary></summary>
        EnquireLink = 0x00000015,
        
        /// <summary></summary>
        EnquireLinkResp = 0x80000015,
        
        /// <summary></summary>
        SubmitMultiSm = 0x00000021,
        
        /// <summary></summary>
        SubmitMultiSmResp = 0x80000021,
        
        /// <summary></summary>
        AlertNotification = 0x00000102,
        
        /// <summary></summary>
        DataSm = 0x00000103,
        
        /// <summary></summary>
        DataSmResp = 0x80000103
    }
    
    #endregion
    
    #region CommandStatus Enum

    /// <summary> CommandStatus of an SMPP message response indicates the success or failure of an SMPP request.
    /// 5.1.3 table 5.2 </summary>
    public enum CommandStatus : uint
    {
        /// <summary> Defined by us to be able to save unknown PDUs </summary>
        Unknown = 0x70000000,

        /// <summary> No Error </summary>
        ESME_ROK = 0x00000000,
        
        /// <summary> Message Length is invalid </summary>
        ESME_RINVMSGLEN = 0x00000001,
        
        /// <summary> Command Length is invalid </summary>
        ESME_RINVCMDLEN = 0x00000002,
        
        /// <summary> Invalid Command ID </summary>
        ESME_RINVCMDID = 0x00000003,
        
        /// <summary> Incorrect BIND Status for given command </summary>
        ESME_RINVBNDSTS = 0x00000004,
        
        /// <summary> ESME Already in Bound State </summary>
        ESME_RALYBND = 0x00000005,
        
        /// <summary> Invalid Priority Flag </summary>
        ESME_RINVPRTFLG = 0x00000006,
        
        /// <summary> Invalid Registered Delivery Flag </summary>
        ESME_RINVREGDLVFLG = 0x00000007,
        
        /// <summary> System Error </summary>
        ESME_RSYSERR = 0x00000008,
        
        /// <summary> Invalid Source Address </summary>
        ESME_RINVSRCADR = 0x0000000A,
        
        /// <summary> Invalid Dest Addr </summary>
        ESME_RINVDSTADR = 0x0000000B,
        
        /// <summary> Message ID is invalid </summary>
        ESME_RINVMSGID = 0x0000000C,
        
        /// <summary> Bind Failed </summary>
        ESME_RBINDFAIL = 0x0000000D,
        
        /// <summary> Invalid Password </summary>
        ESME_RINVPASWD = 0x0000000E,
        
        /// <summary> Invalid System ID </summary>
        ESME_RINVSYSID = 0x0000000F,
        
        /// <summary> Cancel SM Failed </summary>
        ESME_RCANCELFAIL = 0x00000011,
        
        /// <summary> Replace SM Failed </summary>
        ESME_RREPLACEFAIL = 0x00000013,
        
        /// <summary> Message Queue Full </summary>
        ESME_RMSGQFUL = 0x00000014,
        
        /// <summary> Invalid Service Type </summary>
        ESME_RINVSERTYP = 0x00000015,
        
        /// <summary> Invalid number of destinations </summary>
        ESME_RINVNUMDESTS = 0x00000033,
        
        /// <summary> Invalid Distribution List name </summary>
        ESME_RINVDLNAME = 0x00000034,
        
        /// <summary> Destination flag is invalid (submit_multi) </summary>
        ESME_RINVDESTFLAG = 0x00000040,
        
        /// <summary> Invalid "submit with replace" request(i.e. submit_sm with replace_if_present_flag set) </summary>
        ESME_RINVSUBREP = 0x00000042,
        
        /// <summary> Invalid esm_class field data </summary>
        ESME_RINVESMCLASS = 0x00000043,
        
        /// <summary> Cannot Submit to Distribution List </summary>
        ESME_RCNTSUBDL = 0x00000044,
        
        /// <summary> Submit_sm or submit_multi failed </summary>
        ESME_RSUBMITFAIL = 0x00000045,
        
        /// <summary> Invalid Source address TON </summary>
        ESME_RINVSRCTON = 0x00000048,
        
        /// <summary> Invalid Source address NPI </summary>
        ESME_RINVSRCNPI = 0x00000049,
        
        /// <summary> Invalid Destination address TON </summary>
        ESME_RINVDSTTON = 0x00000050,
        
        /// <summary> Invalid Destination address NPI </summary>
        ESME_RINVDSTNPI = 0x00000051,
        
        /// <summary> Invalid system_type field </summary>
        ESME_RINVSYSTYP = 0x00000053,
        
        /// <summary> Invalid replace_if_present flag </summary>
        ESME_RINVREPFLAG = 0x00000054,
        
        /// <summary> Invalid number of messages </summary>
        ESME_RINVNUMMSGS = 0x00000055,
        
        /// <summary> Throttling error (ESME has exceeded allowed message limits) </summary>
        ESME_RTHROTTLED = 0x00000058,
        
        /// <summary> Invalid Scheduled Delivery Time </summary>
        ESME_RINVSCHED = 0x00000061,
        
        /// <summary> Invalid message validity period (Expiry time) </summary>
        ESME_RINVEXPIRY = 0x00000062,
        
        /// <summary> Predefined Message Invalid or Not Found </summary>
        ESME_RINVDFTMSGID = 0x00000063,
        
        /// <summary> ESME Receiver Temporary App Error Code </summary>
        ESME_RX_T_APPN = 0x00000064,
        
        /// <summary> ESME Receiver Permanent App Error Code </summary>
        ESME_RX_P_APPN = 0x00000065,
        
        /// <summary> ESME Receiver Reject Message Error Code </summary>
        ESME_RX_R_APPN = 0x00000066,
        
        /// <summary> QUERY_SM request failed </summary>
        ESME_RQUERYFAIL = 0x00000067,
        
        /// <summary> Error in the optional part of the PDU Body </summary>
        ESME_RINVOPTPARSTREAM = 0x000000C0,
        
        /// <summary> Optional Parameter not allowed </summary>
        ESME_ROPTPARNOTALLWD = 0x000000C1,
        
        /// <summary> Invalid Parameter Length </summary>
        ESME_RINVPARLEN = 0x000000C2,
        
        /// <summary> Expected Optional Parameter missing </summary>
        ESME_RMISSINGOPTPARAM = 0x000000C3,
        
        /// <summary> Invalid Optional Parameter Value </summary>
        ESME_RINVOPTPARAMVAL = 0x000000C4,
        
        /// <summary> Delivery Failure (used fordata_sm_resp) </summary>
        ESME_RDELIVERYFAILURE = 0x000000FE,
        
        /// <summary> Unknown Error </summary>
        ESME_RUNKNOWNERR = 0x000000FF,

        /// <summary> SMPP Client Unexpected Response </summary>
        SMPPCLIENT_UNEXPRESP = 0x00001001,
        
        /// <summary> SMPP Client Receive Timeout </summary>
        SMPPCLIENT_RCVTIMEOUT = 0x00001002,
        
        /// <summary> SMPP Client No Connection </summary>
        SMPPCLIENT_NOCONN = 0x00001003,
        
        /// <summary> SMPP Client Unbound </summary>
        SMPPCLIENT_UNBOUND = 0x00001004,
        
        /// <summary> SMPP Client generic negative acknowledgement </summary>
        SMPPCLIENT_GENERIC_NACK = 0x00001FFE,
        
        /// <summary> SMPP Client Unknown Error </summary>
        SMPPCLIENT_UNKNOWNERROR = 0x00001FFF,
        
        /// <summary> SMPP Client Not required to perform operation </summary>
        SMPPCLIENT_NOTREQUIRED = 0x00002FFF
    }
    
    #endregion
    
    #region TON Enum

    /// <summary> Type of Number (TON) used in the SME address parameters
    /// 5.2.5 table 5.3 </summary>
    public enum Ton : byte
    {
        /// <summary> Unknown </summary>
        Unknown = 0x0,
        
        /// <summary> International </summary>
        International = 0x1,
        
        /// <summary> National </summary>
        National = 0x2,
        
        /// <summary> NetworkSpecific </summary>
        NetworkSpecific = 0x3,
        
        /// <summary> SubscriberNumber</summary>
        SubscriberNumber = 0x4,
        
        /// <summary> Alphanumeric </summary>
        Alphanumeric = 0x5,
        
        /// <summary> Abbreviated </summary>
        Abbreviated = 0x6
    }
    
    #endregion
    
    #region NPI Enum

    /// <summary> Numeric Plan Indicator (NPI) used in the SME address parameters.
    /// 5.2.6 table 5.4 </summary>
    public enum Npi : byte
    {
        /// <summary> Unknown </summary>
        Unknown = 0x00,
        
        /// <summary> ISDN </summary>
        ISDN = 0x01,
        
        /// <summary> DataX121 </summary>
        DataX121 = 0x03,
        
        /// <summary> TelexF69 </summary>
        TelexF69 = 0x04,
        
        /// <summary> LandMobileE212 </summary>
        LandMobileE212 = 0x06,
        
        /// <summary> National </summary>
        National = 0x08,
        
        /// <summary> Private </summary>
        Private = 0x09,
        
        /// <summary> ERMES </summary>
        ERMES = 0x0A,
        
        /// <summary> Internet </summary>
        Internet = 0x0E,
        
        /// <summary> WAP </summary>
        WAP = 0x12
    }
    
    #endregion

    #region MessageModes Enum
    
    /// <summary> Messaging Modes (bits 1-0 of esm_class) </summary>
    public enum MessageModes
    {
        /// <summary> Default SMSC Mode (e.g. Store and Forward) </summary>
        Default = 0x00,
        
        /// <summary> Datagram mode </summary>
        Datagram = 0x01,
        
        /// <summary> Forward (i.e. Transaction) mode </summary>
        Forward = 0x02,
        
        /// <summary> Store and Forward mode </summary>
        StoreForward = 0x03
    }
    
    #endregion

    #region MessageTypes Enum
    
    /// <summary> Messaging Types (bits 5-2 of esm_class) </summary>
    public enum MessageTypes
    {
        /// <summary> Default message Type (i.e. normal message) </summary>
        Default = 0x00,
        
        /// <summary> (ESME -> SMSC)
        /// Short Message contains ESME Delivery Acknowledgement
        /// (SMSC -> ESME)
        /// Short Message contains SME Delivery Acknowledgement </summary>
        SMEDeliveryAcknowledgement = 0x08,
        
        /// <summary> (ESME -> SMSC)
        /// Short Message contains ESME Manual/User Acknowledgement
        /// (SMSC -> ESME)
        /// Short Message contains SME Manual/User Acknowledgment </summary>
        SMEManualAcknowledgement = 0x10,
        
        /// <summary> (SMSC -> ESME)
        /// Short Message contains SMSC Delivery Receipt </summary>
        SMSCDeliveryReceipt = 0x04,
        
        /// <summary> (SMSC -> ESME)
        /// Short Message contains Conversation Abort (Korean CDMA) </summary>
        ConversationAbort = 0x18,
        
        /// <summary> (SMSC -> ESME)
        /// Short Message contains Intermediate Delivery Notification </summary>
        IntermediateDeliveryNotification = 0x20
    }
    
    #endregion
    
    #region GSMSpecificFeatures Enum

    /// <summary> GSM Network Specific Features (bits 7-6 of esm_class) </summary>
    public enum GSMSpecificFeatures
    {
        /// <summary> No specific features selected </summary>
        No = 0x00,
        
        /// <summary> UDHI Indicator set </summary>
        UDHI = 0x40,
        
        /// <summary> Set Reply Path (only relevant for GSM network) </summary>
        ReplyPath = 0x80,
        
        /// <summary> Set UDHI and Reply Path (only relevant for GSM network) </summary>
        UDHIandReplyPath = 0xC0
    }
    
    #endregion
    
    #region SMSCDeliveryReceipt Enum

    /// <summary> SMSC Delivery Receipt (bits 1 and 0 of registered_delivery) </summary>
    public enum SMSCDeliveryReceipt
    {
        /// <summary> No SMSC Delivery Receipt requested (default) </summary>
        NotRequested = 0x00,
        
        /// <summary> SMSC Delivery Receipt requested where final delivery outcome is delivery success or failure </summary>
        SuccessFailure = 0x01,
        
        /// <summary> SMSC Delivery Receipt requested where the final delivery outcome is delivery failure </summary>
        Failure = 0x02
    }
    
    #endregion
    
    #region SMEAcknowledgement Enum

    /// <summary> SME originated Acknowledgement (bits 3 and 2 of registered_delivery) </summary>
    public enum SMEAcknowledgement
    {
        /// <summary> No recipient SME acknowledgment requested (default) </summary>
        NotRequested = 0x00,

        /// <summary> SME Delivery Acknowledgement requested </summary>
        Delivery = 0x04,

        /// <summary> SME Manual/User Acknowledgment requested </summary>
        Manual = 0x08,

        /// <summary> Both Delivery and Manual/User Acknowledgment requested </summary>
        DeliveryManual = 0x0C
    }
    
    #endregion

    #region IntermediateNotification Enum
    
    /// <summary> Intermediate Notification (bit 5 of registered_delivery) </summary>
    public enum IntermediateNotification
    {
        /// <summary> No Intermediate notification requested (default) </summary>
        NotRequested = 0x00,

        /// <summary> Intermediate notification requested </summary>
        Requested = 0x10
    }

    #endregion
    
    #region MessageState Enum
    
    /// <summary> States for a short message </summary>
    public enum MessageState : byte
    {
        /// <summary></summary>
        None = 0,
        
        /// <summary> The message is in enroute state </summary>
        Enroute = 1,

        /// <summary> Message is delivered to destination </summary>
        Delivered = 2,

        /// <summary> Message validity period has expired </summary>
        Expired = 3,

        /// <summary> Message has been deleted </summary>
        Deleted = 4,

        /// <summary> Message is undeliverable </summary>
        Undeliverable = 5,

        /// <summary> Message is in accepted state 
        /// (i.e. has been manually read on behalf of the subscriber by customer service) </summary>
        Accepted = 6,

        /// <summary> Message is in invalid state </summary>
        Unknown = 7,

        /// <summary> Message is in a rejected state </summary>
        Rejected = 8
    }
    
    #endregion
    
    #region DestinationAddressType Enum

    /// <summary> Identifies whether destination address is a Distribution List Name or SME address </summary>
    public enum DestinationAddressType : byte
    {
        /// <summary> SME Address </summary>
        SMEAddress = 1,

        /// <summary> Distribution List Name </summary>
        DistributionListName = 2
    }

    #endregion
    
    #region SubmitMode Enum
    
    /// <summary> Identifies the way how to send user data in the submit_sm PDU </summary>
	public enum SubmitMode
	{
        /// <summary> Send user data in the short_message field </summary>
		ShortMessage = 1,

        /// <summary> Send user data in the optional parameter message_payload </summary>
		Payload = 2
	}
	
	#endregion

    #region DeliverMode Enum
    
    /// <summary> Identifies the way how to send user data in the deliver_sm PDU </summary>
	public enum DeliverMode
	{
        /// <summary> Send user data in the short_message field </summary>
		ShortMessage = 1,

        /// <summary> Send user data in the optional parameter message_payload </summary>
		Payload = 2
	}
	
	#endregion
	
	#region InformationElementIdentifiers Enum
	
	/// <summary> Identifies Information Element in the User Data Header </summary>
	public enum InformationElementIdentifiers : byte
	{
		/// <summary> Concatenated short messages, 8-bit reference number </summary>
		ConcatenatedShortMessages8bit = 0x00,
		
		/// <summary> Special SMS Message Indication </summary>
		SpecialSMSMessageIndication  = 0x01,
		
		/// <summary> Application port addressing scheme, 8 bit address </summary>
		ApplicationPortAddressingScheme8bit	= 0x04,
		
		/// <summary> Application port addressing scheme, 16 bit address </summary>
		ApplicationPortAddressingScheme16bit = 0x05,
		
		/// <summary> SMSC Control Parameters </summary>
		SMSCControlParameters = 0x06,
		
		/// <summary> UDH Source Indicator </summary>
		UDHSourceIndicator = 0x07,
		
		/// <summary> Concatenated short message, 16-bit reference number </summary>
		ConcatenatedShortMessage16bit = 0x08,
		
		/// <summary> Wireless Control Message Protocol </summary>
		WirelessControlMessageProtocol	= 0x09,
		
		/// <summary> Text Formatting </summary>
		TextFormatting = 0x0A,
		
		/// <summary> Predefined Sound </summary>
		PredefinedSound =0x0B,
		
		/// <summary> User Defined Sound (iMelody max 128 bytes) </summary>
		UserDefinedSound = 0x0C,
		
		/// <summary> Predefined Animation </summary>
		PredefinedAnimation = 0x0D,
		
		/// <summary> Large Animation (16*16 times 4 = 32*4 =128 bytes) </summary>
		LargeAnimation= 0x0E,
		
		/// <summary> Small Animation (8*8 times 4 = 8*4 =32 bytes) </summary>
		SmallAnimation= 0x0F,
		
		/// <summary> Large Picture (32*32 = 128 bytes) </summary>
		LargePicture= 0x10,
		
		/// <summary> Small Picture (16*16 = 32 bytes) </summary>
		SmallPicture= 0x11,
		
		/// <summary> Variable Picture </summary>
		VariablePicture= 0x12,
		
		/// <summary> User prompt indicator </summary>
		UserPromptIndicator= 0x13,
		
		/// <summary> Extended Object </summary>
		ExtendedObject= 0x14,
		
		/// <summary> Reused Extended Object </summary>
		ReusedExtendedObject= 0x15,
		
		/// <summary> Compression Control </summary>
		CompressionControl= 0x16,
		
		/// <summary> Object Distribution Indicator </summary>
		ObjectDistributionIndicator= 0x17,
		
		/// <summary> Standard WVG object </summary>
		StandardWvgObject= 0x18,
		
		/// <summary> Character Size WVG object </summary>
		CharacterSizeWvgObject= 0x19,
		
		/// <summary> Extended Object Data Request Command </summary>
		ExtendedObjectDataRequestCommand = 0x1A,
		
		/// <summary> RFC 822 E-Mail Header </summary>
		EMailHeader= 0x20,
		
		/// <summary> Hyperlink format element </summary>
		HyperlinkFormatElement= 0x21,
		
		/// <summary> Reply Address Element </summary>
		ReplyAddressElement = 0x22,
		
		/// <summary> Enhanced Voice Mail Information </summary>
		EnhancedVoiceMailInformation= 0x23,
		
		/// <summary> Unknown </summary>
		Unknown = 0xFF
	}
	
	#endregion
	
	#region OptionalTags
	
	/// <summary> Identifes the Tag field that used to uniquely identify the particular optional parameter </summary>
    public enum OptionalTags : ushort
    {
        /// <summary></summary>
        DestAddrSubunit = 0x0005,
        
        /// <summary></summary>
        DestNetworkType = 0x0006,
        
        /// <summary></summary>
        DestBearerType = 0x0007,
        
        /// <summary></summary>
        DestTelematicsId = 0x0008,
        
        /// <summary></summary>
        SourceAddrSubunit = 0x000D,
        
        /// <summary></summary>
        SourceNetworkType = 0x000E,
        
        /// <summary></summary>
        SourceBearerType = 0x000F,
        
        /// <summary></summary>
        SourceTelematicsId = 0x0010,
        
        /// <summary></summary>
        QosTimeToLive = 0x0017,

        /// <summary></summary>
        PayloadType = 0x0019,
        
        /// <summary></summary>
        AdditionalStatusInfoText = 0x001D,
        
        /// <summary></summary>
        ReceiptedMessageId = 0x001E,
        
        /// <summary></summary>
        MsMsgWaitFacilities = 0x0030,
        
        /// <summary></summary>
        PrivacyIndicator = 0x0201,
        
        /// <summary></summary>
        SourceSubaddress = 0x0202,
        
        /// <summary></summary>
        DestSubaddress = 0x0203,
        
        /// <summary></summary>
        UserMessageReference = 0x0204,
        
        /// <summary></summary>
        UserResponseCode = 0x0205,
        
        /// <summary></summary>
        SourcePort = 0x020A,
        
        /// <summary></summary>
        DestinationPort = 0x020B,
        
        /// <summary> The sar_msg_ref_num parameter is used to indicate the reference number for a particular concatenated short message </summary>
        SarMsgRefNum = 0x020C,
        
        /// <summary></summary>
        LanguageIndicator = 0x020D,
        
        /// <summary> The sar_total_segments parameter is used to indicate the total number of short messages within the concatenated short message </summary>
        SarTotalSegments = 0x020E,

        /// <summary> The sar_segment_seqnum parameter is used to indicate the sequence number of a particular short message within the concatenated short message </summary>
        SarSegmentSeqnum = 0x020F,

        /// <summary></summary>
        SCInterfaceVersion = 0x0210,
        
        /// <summary></summary>
        CallbackNumPresInd = 0x0302,
        
        /// <summary></summary>
        CallbackNumAtag = 0x0303,
        
        /// <summary></summary>
        NumberOfMessages = 0x0304,
        
        /// <summary></summary>
        CallbackNum = 0x0381,
        
        /// <summary></summary>
        DpfResult = 0x0420,
        
        /// <summary></summary>
        SetDpf = 0x0421,
        
        /// <summary></summary>
        MsAvailabilityStatus = 0x0422,
        
        /// <summary></summary>
        NetworkErrorCode = 0x0423,
        
        /// <summary> Message_payload parameter contains the user data </summary>
        MessagePayload = 0x0424,

        /// <summary></summary>
        DeliveryFailureReason = 0x0425,

        /// <summary> The more_messages_to_send parameter is used by the ESME in the submit_sm and data_sm operations to indicate to the SMSC that there are further messages for the same destination SME. The SMSC may use this setting for network resource optimization </summary>
        MoreMessagesToSend = 0x0426,

        /// <summary></summary>
        MessageState = 0x0427,
        
        /// <summary></summary>
        UssdServiceOp = 0x0501,
        
        /// <summary></summary>
        DisplayTime = 0x1201,
        
        /// <summary></summary>
        SmsSignal = 0x1203,

        /// <summary></summary>
        MsValidity = 0x1204,

        /// <summary></summary>
        AlertOnMessageDelivery = 0x130C,

        /// <summary></summary>
        ItsReplyType = 0x1380,

        /// <summary></summary>
        ItsSessionInfo = 0x1383
    }
    
    #endregion

    #region LogEventNotificationTypes Enum

    /// <summary> Internal Log Event types </summary>
    public enum LogEventNotificationTypes
    {
        /// <summary> Standard Write </summary>
        None = 1,

        /// <summary> Send an email alert as well as write the log </summary>
        Email = 2,
    }

    #endregion

    #region ConnectionEventTypes Enum

    /// <summary> Internal connection event type </summary>
    public enum ConnectionEventTypes
    {
        /// <summary> Connection Established </summary>
        Connected = 1,

        /// <summary> Conntion Attempt Has Failed </summary>
        ConnectionAttemptFailed = 2,

        /// <summary> Connection Is Bound </summary>
        Bound = 4,

        /// <summary> Binding Attempt Failed </summary>
        BindingAttemptFailed = 5,

        /// <summary> Connection is UnBound</summary>
        UnBound = 6,

        /// <summary> Disconnected </summary>
        Disconnected = 7
    }

    #endregion

    #region PduDirectionTypes Enum

    /// <summary> Used to flag if the pdu was received or sent </summary>
    public enum PduDirectionTypes
    {
        /// <summary> The PDU has been received from the end point </summary>
        Received = 1,

        /// <summary> The PDU was sent from ourselves </summary>
        Sent = 2
    }

    #endregion
}
