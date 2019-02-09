using System;
using System.IO;
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
                case 0x4:
                    GetStatGroup(p, pi, ns);
                    break;
                case 0xF:
                    GetKeyScopesMap(p, pi, ns);
                    break;
                case 0x10:
                    GetStatsByGroupAsync(p, pi, ns);
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

        public static void GetStatGroup(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Tdf> input = Blaze.ReadPacketContent(p);
            string which = ((Blaze.TdfString)input[0]).Value;
            List<Blaze.Tdf> Result = new List<Blaze.Tdf>();
            Result.Add(Blaze.TdfString.Create("CNAM", which));
            if (which == "const")
            {
                Result.Add(Blaze.TdfString.Create("DESC", "Constant stats"));
                Result.Add(Blaze.TdfDoubleVal.Create("ETYP", new Blaze.DoubleVal(30722, 1)));
                Result.Add(Blaze.TdfString.Create("META", ""));
                Result.Add(Blaze.TdfString.Create("NAME", "const"));
                List<Blaze.TdfStruct> STAT = new List<Blaze.TdfStruct>();
                    STAT.Add(BlazeHelper.MakeStatGroupEntry(0, "const", "0", "CONST_KIT_LONG", "kit", "CONST_KIT"));
                    STAT.Add(BlazeHelper.MakeStatGroupEntry(1, "const", "2", "CONST_HEAD_LONG", "head", "CONST_HEAD"));
                    STAT.Add(BlazeHelper.MakeStatGroupEntry(2, "const", "0", "CONST_HAIR_LONG", "hair", "CONST_HAIR"));
                    STAT.Add(BlazeHelper.MakeStatGroupEntry(3, "const", "0", "CONST_FACIAL_FEATURE_LONG", "facialFeature", "CONST_FACIAL_FEATURE"));
                Result.Add(Blaze.TdfList.Create("STAT", 3, 4, STAT));
            }
            if (which == "crit")
            {
                Result.Add(Blaze.TdfString.Create("DESC", "Critical stats"));
                Result.Add(Blaze.TdfDoubleVal.Create("ETYP", new Blaze.DoubleVal(30722, 1)));
                Result.Add(Blaze.TdfString.Create("META", ""));
                Result.Add(Blaze.TdfString.Create("NAME", "crit"));
                List<Blaze.TdfStruct> STAT = new List<Blaze.TdfStruct>();
                    STAT.Add(BlazeHelper.MakeStatGroupEntry(0, "crit", pi.profile.level.ToString(), "CRIT_LEVEL_LONG", "level", "CRIT_LEVEL"));
                    STAT.Add(BlazeHelper.MakeStatGroupEntry(1, "crit", pi.profile.xp.ToString(), "CRIT_XP_LONG", "xp", "CRIT_XP"));
                    STAT.Add(BlazeHelper.MakeStatGroupEntry(2, "crit", "10000", "CRIT_ELO_LONG", "elo", "CRIT_ELO"));
                Result.Add(Blaze.TdfList.Create("STAT", 3, 3, STAT));
            }
            byte[] buff = Blaze.CreatePacket(p.Component, p.Command, 0, 0x1000, p.ID, Result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }

        public static void GetStatsByGroupAsync(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            byte[] buff = Blaze.CreatePacket(p.Component, p.Command, 0, 0x1000, p.ID, new List<Blaze.Tdf>());
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
            AsyncStats.GetStatsAsyncNotification(p, pi, ns);
        }
    }
}
