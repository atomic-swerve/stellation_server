using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace stellation_server
{

    public enum PlayerColours : byte
    {
        Red, Blue, Green, White
    }

    public class PlayerState
    {
        public int id = -1;
        public NetConnection connection;
        public float x = 0;
        public float y = 0;
        public PlayerColours colour;

        public Room currentRoom;

        public PlayerState(NetConnection conn) {
            connection = conn;
            colour = PlayerColours.White;
        }

        public void UpdateState(float newX, float newY, PlayerColours c)
        {
            x = newX;
            y = newY;
            colour = c;
            
            if (currentRoom != null)
            {
                currentRoom.UpdatePlayer(this);
            }
        }
        
        public void Disconnect()
        {
            currentRoom.RemovePlayer(this);
        }

        public void MoveToRoom(Room room)
        {
            if (currentRoom != null)
            {
                currentRoom.RemovePlayer(this);
            }
            id = room.AddPlayer(this);
            currentRoom = room;
        }
    }
}
