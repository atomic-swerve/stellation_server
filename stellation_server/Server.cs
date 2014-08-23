using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace stellation_server
{
    class Server
    {
        NetServer m_server;
        NetIncomingMessage inc;

        List<Room> rooms;
        List<TargetState> targets;

        Server() { }

        void SetUpServer()
        {
            NetPeerConfiguration config = new NetPeerConfiguration("StellationServer");
            config.Port = Properties.Settings.Default.port;
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            config.MaximumConnections = Properties.Settings.Default.maxConnections;

            m_server = new NetServer(config);
            m_server.Start();
        }

        void TakedownServer(string message)
        {
            m_server.Shutdown(message);
        }

        bool ReadMessages()
        {
            while ((inc = m_server.ReadMessage()) != null)
            {
                switch (inc.MessageType)
                {
                    case NetIncomingMessageType.StatusChanged:
                        Console.WriteLine("New status: " + ((NetConnectionStatus)inc.ReadByte()).ToString());
                        break;
                    case NetIncomingMessageType.Data:
                        Console.WriteLine("Responding with ping");
                        NetOutgoingMessage msg = m_server.CreateMessage();
                        msg.Write("Ping");
                        m_server.SendMessage(msg, inc.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                        break;
                    case NetIncomingMessageType.DebugMessage:
                        Console.WriteLine("Debug: " + inc.ReadString());
                        break;
                    default:
                        Console.WriteLine("Unhandled type: " + inc.MessageType);
                        break;
                }
                m_server.Recycle(inc);
            }
            return true;
        }

        static void Main(string[] args)
        {
            Server server = new Server();
            server.SetUpServer();

            while (true)
            {
                server.ReadMessages();
            }
        }
    }
}
