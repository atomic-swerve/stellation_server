using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace stellation_server
{
    public class PlayerState
    {
        public int id = -1;
        public NetConnection connection;
        public float x = 0;
        public float y = 0;

        public Room currentRoom;

        public PlayerState(NetConnection conn) {
            connection = conn;
        }

        public void UpdateState(float newX, float newY)
        {
            x = newX;
            y = newY;
            
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
