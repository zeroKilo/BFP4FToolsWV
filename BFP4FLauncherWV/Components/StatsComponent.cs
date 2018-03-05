using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazeLibWV;
using System.Net.Sockets;


namespace BFP4FLauncherWV
{
    public static class StatsComponent
    {

        public static void HandlePacket(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            switch (p.Command)
            {
                case 0xF:
                    GetKeyScopesMap(p, pi, ns);
                    break;
            }
        }

        public static void GetKeyScopesMap(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Tdf> Result = new List<Blaze.Tdf>();
            List<string> l1 = new List<string>() { "ldgy" };
            List<Blaze.TdfStruct> l2 = new List<Blaze.TdfStruct>();            
            List<Blaze.Tdf> l3 = new List<Blaze.Tdf>();
            l3.Add(Blaze.TdfInteger.Create("AGKY", 0));
            l3.Add(Blaze.TdfInteger.Create("ENAG", 0));
            l3.Add(Blaze.TdfDoubleList.Create("KSVL", 0, 0, new List<long>() { 0 }, new List<long>() { 15 }, 1));
            l2.Add(Blaze.TdfStruct.Create("0", l3));
            Result.Add(Blaze.TdfDoubleList.Create("KSIT", 1, 3, l1, l2, 1));
            byte[] buff = Blaze.CreatePacket(p.Component, p.Command, 0, 0x1000, p.ID, Result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }
    }
}
