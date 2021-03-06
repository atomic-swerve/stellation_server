﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace stellation_server
{
    public class Room
    {
        int maxPlayers;
        Dictionary<int,PlayerState> players;
        Server m_server;
        Queue<int> availableSlots;

        public Room(Server s, int max)
        {
            m_server = s;
            maxPlayers = max;
            players = new Dictionary<int,PlayerState>();
            availableSlots = new Queue<int>();
        }

        public int AddPlayer(PlayerState p) {
            int index;

            if (availableSlots.Count > 0)
            {
                index = availableSlots.Dequeue();
            }
            else
            {
                index = players.Count;
            }

            players.Add(index, p);
            NotifyOneUpdate(p);
            NotifyAllUpdate(p);

            return index;
        }

        public void UpdatePlayer(PlayerState p)
        {
            NotifyAllUpdate(p);
        }

        public void RemovePlayer(PlayerState p)
        {
            players.Remove(p.id);
            availableSlots.Enqueue(p.id);
            NotifyAllRemoved(p.id);
        }
        
        void NotifyOneUpdate(PlayerState p)
        {
            if (players.Count <= 1) return;

            List<NetConnection> l = new List<NetConnection>(1);
            l.Add(p.connection);

            foreach (PlayerState player in players.Values)
            {
                if (player == p)
                {
                    continue;
                }
                m_server.SendUpdate(l, player);
            }
        }

        void NotifyAllUpdate(PlayerState p)
        {
            if (players.Count <= 1) return;

            List<NetConnection> toNotify = new List<NetConnection>();
            foreach (PlayerState player in players.Values)
            {
                if (player == p)
                {
                    continue;
                }
                toNotify.Add(player.connection);
            }
            m_server.SendUpdate(toNotify, p);
        }

        void NotifyAllRemoved(int changed)
        {
            if (players.Count == 0) return;

            List<NetConnection> toNotify = new List<NetConnection>();
            foreach (PlayerState player in players.Values)
            {
                toNotify.Add(player.connection);
            }
            m_server.SendUpdate(toNotify, changed);
        }
    }
}
