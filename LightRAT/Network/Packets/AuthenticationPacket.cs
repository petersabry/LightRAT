﻿using System;
using System.Threading.Tasks;
using LightRAT.Data;

namespace LightRAT.Network.Packets
{
    [Serializable]
    public class AuthenticationPacket : IPacket
    {
        public Account Account { get; set; }

        public async Task Execute(Client client)
        {
            await client.SendPacket(this);
        }
    }
}