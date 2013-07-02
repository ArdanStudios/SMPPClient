#region Namespaces

using System;
using System.Collections.Generic;
using System.Text;
using ArdanStudios.Common.SmppClient;
using ArdanStudios.Common.SmppClient.App;

#endregion

namespace SmppTestClient
{
    class SMSControl
    {
        #region Properties

        /// <summary> A reference to the esme manager </summary>
        private static ESMEManager connectionManager = null;
        
        #endregion

        static void Main(string[] args)
        {
            string server = "gateway.domain.com";       // IP Address or Name of the server
            short port = 9900;                          // Port
            string shortLongCode = "55555";             // The short or long code for this bind
            string systemId = "systemid";               // The system id for authentication
            string password = "password";               // The password of authentication
            DataCodings dataCoding = DataCodings.ASCII; // The encoding to use if Default is returned in any PDU or encoding request

            // Create a esme manager to communicate with an ESME
            connectionManager = new ESMEManager("Test",
                                                shortLongCode,
                                                new ESMEManager.CONNECTION_EVENT_HANDLER(ConnectionEventHandler),
                                                new ESMEManager.RECEIVED_MESSAGE_HANDLER(ReceivedMessageHandler),
                                                new ESMEManager.RECEIVED_GENERICNACK_HANDLER(ReceivedGenericNackHandler),
                                                new ESMEManager.SUBMIT_MESSAGE_HANDLER(SubmitMessageHandler),
                                                new ESMEManager.QUERY_MESSAGE_HANDLER(QueryMessageHandler),
                                                new ESMEManager.LOG_EVENT_HANDLER(LogEventHandler),
                                                new ESMEManager.PDU_DETAILS_EVENT_HANDLER(PduDetailsHandler));

            // Bind one single Receiver connection
            connectionManager.AddConnections(1, ConnectionModes.Receiver, server, port, systemId, password, "Receiver", dataCoding); 
            
            // Bind one Transmitter connection
            connectionManager.AddConnections(1, ConnectionModes.Transmitter, server, port, systemId, password, "Transceiver",  dataCoding); 

            // Accept command input
            bool bQuit = false;

            for (;;)
            {
                // Hit Enter in the terminal once the binds are up to see this prompt

                Console.WriteLine("Commands");
                Console.WriteLine("send 12223334444 hello jack");
                Console.WriteLine("quit");
                Console.WriteLine("");

                Console.Write("\n#>");

                string command = Console.ReadLine();
                if (command.Length == 0)
                    continue;

                switch (command.Split(' ')[0].ToString())
                {
                    case "quit":
                    case "exit":
                        bQuit = true;
                        break;

                    default:
                        ProcessCommand(command);
                        break;
                }

                if (bQuit)
                    break;
            }
            
            if (connectionManager != null)
            {
                connectionManager.Dispose();
            }
        }
        
        private static void ProcessCommand(string command)
        {
            string[] parts = command.Split(' ');
            
            switch (parts[0])
            {
                case "send":
                    SendMessage(command);
                    break;

                case "query":
                    QueryMessage(command);
                    break;
            }
        }
        
        private static void SendMessage(string command)
        {
            string[] parts = command.Split(' ');
            string phoneNumber = parts[1];
            string message = string.Join(" ", parts, 2, parts.Length - 2);

            // This is set in the Submit PDU to the SMSC
            // If you are responding to a received message, make this the same as the received message
            DataCodings submitDataCoding = DataCodings.Default;

            // Use this to encode the message
            // We need to know the actual encoding.
            DataCodings encodeDataCoding = DataCodings.ASCII;

            // There is a default encoding set for each connection. This is used if the encodeDataCoding is Default
            
            SubmitSm submitSm = null;
            SubmitSmResp submitSmResp = null;
            connectionManager.SendMessage(phoneNumber, null, Ton.National, Npi.ISDN, submitDataCoding, encodeDataCoding, message, out submitSm, out submitSmResp);
        }

        private static void QueryMessage(string command)
        {
            string[] parts = command.Split(' ');
            string messageId = parts[1];

            QuerySm querySm = connectionManager.SendQuery(messageId);
        }

        private static void ReceivedMessageHandler(string logKey, string serviceType, Ton sourceTon, Npi sourceNpi, string shortLongCode, DateTime dateReceived, string phoneNumber, DataCodings dataCoding, string message)
        {
            Console.WriteLine("ReceivedMessageHandler: {0}", message);
        }

        private static void ReceivedGenericNackHandler(string logKey, int sequence)
        {
        }

        private static void SubmitMessageHandler(string logKey, int sequence, string messageId)
        {
            Console.WriteLine("SubmitMessageHandler: {0}", messageId);
        }

        private static void QueryMessageHandler(string logKey, int sequence, string messageId, DateTime finalDate, int messageState, long errorCode)
        {
            Console.WriteLine("QueryMessageHandler: {0} {1} {2}", messageId, finalDate, messageState);
        }

        private static void LogEventHandler(LogEventNotificationTypes logEventNotificationType, string logKey, string shortLongCode, string message)
        {
            Console.WriteLine(message);
        }

        private static void ConnectionEventHandler(string logKey, ConnectionEventTypes connectionEventType, string message)
        {
            Console.WriteLine("ConnectionEventHandler: {0} {1}", connectionEventType, message);
        }

        private static Guid? PduDetailsHandler(string logKey, PduDirectionTypes pduDirectionType, Header pdu, List<PduPropertyDetail> details)
        {
            Guid? pduHeaderId = null;

            try
            {
                // Do not store these
                if ((pdu.Command == CommandSet.EnquireLink) || (pdu.Command == CommandSet.EnquireLinkResp))
                {
                    return null;
                }

                string connectionString = null; // If null InsertPdu will just log to stdout
                int serviceId = 0;              // Internal Id used to track multiple SMSC systems

                PduApp.InsertPdu(logKey, connectionString, serviceId, pduDirectionType, details, pdu.PduData.BreakIntoDataBlocks(4096), out pduHeaderId);
            }

            catch (Exception exception)
            {
                Console.WriteLine("{0}", exception.Message);
            }

            return pduHeaderId;
        }
    }
}
