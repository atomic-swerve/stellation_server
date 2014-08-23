using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace stellation_server
{
    enum AdminCommands : byte
    {
        UserReport, Broadcast, Lock, Unlock, Boot, BootAll, Shutdown
    }

    class Server
    {
        NetServer m_server;
        NetIncomingMessage inc;

        Server() { }

        void SetUpServer()
        {
            NetPeerConfiguration config = new NetPeerConfiguration(Properties.Settings.Default.serverName);
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
                    case NetIncomingMessageType.ConnectionApproval:
                        if (inc.ReadString().Equals(Properties.Settings.Default.playerKey))
                            inc.SenderConnection.Approve();
                        else
                            inc.SenderConnection.Deny();
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        Console.WriteLine("New status: " + ((NetConnectionStatus)inc.ReadByte()).ToString());
                        break;

                    case NetIncomingMessageType.Data:
                        //Check if Admin call
                        if (inc.ReadBoolean()) 
                        {
                            //Check basic authentication
                            if (inc.ReadString().Equals(Properties.Settings.Default.adminKey)) 
                            {
                                //Handle Admin tasks
                                switch ((AdminCommands)inc.ReadByte())
                                {
                                    //Report
                                    //Broadcast
                                    //Lock
                                    //Prevent anyone new from joining
                                    case AdminCommands.Lock:
                                        m_server.Configuration.AcceptIncomingConnections = false;
                                        Console.WriteLine("Blocking new incoming connections");
                                        break;
                                    //Unlock
                                    //Allow new incoming connections
                                    case AdminCommands.Unlock:
                                        m_server.Configuration.AcceptIncomingConnections = true;
                                        Console.WriteLine("Allowing new incoming connections");
                                        break;
                                    //Boot
                                    case AdminCommands.Boot:
                                        break;
                                    //BootAll
                                    case AdminCommands.BootAll:
                                        //m_server.Connections.RemoveAll();
                                        break;
                                    //Shutdown
                                    case AdminCommands.Shutdown:
                                        TakedownServer(inc.ReadString());
                                        Console.WriteLine("Server Shutdown Successful");
                                        return false;
                                }
                            }
                        }
                        //Else not an Admin request
                        else
                        {
                            NetOutgoingMessage msg = m_server.CreateMessage();
                            msg.Write("Ping");
                            m_server.SendMessage(msg, inc.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                        }
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
                if (!server.ReadMessages()) break;
            }
        }
    }
}
