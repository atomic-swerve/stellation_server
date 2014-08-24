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

    enum PlayerCommands : byte
    {
        UserReport, Update, Disconnect
    }

    public class Server
    {
        NetServer m_server;
        NetIncomingMessage inc;

        Dictionary<NetConnection, PlayerState> onlinePlayers;
        List<Room> rooms;

        Server() { 
            onlinePlayers = new Dictionary<NetConnection,PlayerState>();
            rooms = new List<Room>();
        }

        void SetUpServer()
        {
            rooms.Add(new Room(this, 5000));

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

        public void SendUpdate(List<NetConnection> conns, PlayerState player)
        {
            NetOutgoingMessage msg = m_server.CreateMessage();
            msg.Write((byte)PlayerCommands.Update);
            msg.Write(player.id);
            msg.Write(player.x);
            msg.Write(player.y);
            msg.Write(true);
            m_server.SendMessage(msg, conns, NetDeliveryMethod.ReliableOrdered, 0);
        }

        public void SendUpdate(List<NetConnection> conns, int id)
        {
            NetOutgoingMessage msg = m_server.CreateMessage();
            msg.Write((byte)PlayerCommands.Disconnect);
            msg.Write(id);
            msg.Write(0);
            msg.Write(0);
            msg.Write(false);
            m_server.SendMessage(msg, conns, NetDeliveryMethod.ReliableOrdered, 0);
        }

        bool ReadMessages()
        {
            while ((inc = m_server.ReadMessage()) != null)
            {
                switch (inc.MessageType)
                {
                    case NetIncomingMessageType.ConnectionApproval:
                        if (inc.ReadString() == Properties.Settings.Default.playerKey)
                        {
                            inc.SenderConnection.Approve();
                            PlayerState p = new PlayerState(inc.SenderConnection);
                            onlinePlayers.Add(inc.SenderConnection, p);
                            p.MoveToRoom(rooms[0]);
                        }
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
                            switch((PlayerCommands)inc.ReadByte())
                            { 
                                case (PlayerCommands.Update):
                                    //Console.WriteLine("Update request");
                                    onlinePlayers[inc.SenderConnection].UpdateState(inc.ReadFloat(), inc.ReadFloat());
                                    break;
                                case (PlayerCommands.Disconnect):
                                    Console.WriteLine("Disconnection request");
                                    onlinePlayers[inc.SenderConnection].Disconnect();
                                    onlinePlayers.Remove(inc.SenderConnection);
                                    break;
                            }
                        }
                        break;
                    case NetIncomingMessageType.DebugMessage:
                        Console.WriteLine("Debug: " + inc.ReadString());
                        break;
                    case NetIncomingMessageType.WarningMessage:
                        Console.WriteLine("Warning: " + inc.ReadString());
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
