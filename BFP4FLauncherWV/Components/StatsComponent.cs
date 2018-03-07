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
                case 0x4:
                    GetStatGroup(p, pi, ns);
                    break;
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
                List<Blaze.Tdf> E0 = new List<Blaze.Tdf>();
                E0.Add(Blaze.TdfString.Create("CATG", "const"));
                E0.Add(Blaze.TdfString.Create("DFLT", "0"));
                E0.Add(Blaze.TdfInteger.Create("DRVD", 0));
                E0.Add(Blaze.TdfString.Create("FRMT", "%d"));
                E0.Add(Blaze.TdfString.Create("KIND", ""));
                E0.Add(Blaze.TdfString.Create("LDSC", "CONST_KIT_LONG"));
                E0.Add(Blaze.TdfString.Create("META", ""));
                E0.Add(Blaze.TdfString.Create("NAME", "kit"));
                E0.Add(Blaze.TdfString.Create("SDSC", "CONST_KIT"));
                E0.Add(Blaze.TdfInteger.Create("TYPE", 0));
                STAT.Add(Blaze.TdfStruct.Create("0", E0));
                List<Blaze.Tdf> E1 = new List<Blaze.Tdf>();
                E1.Add(Blaze.TdfString.Create("CATG", "const"));
                E1.Add(Blaze.TdfString.Create("DFLT", "2"));
                E1.Add(Blaze.TdfInteger.Create("DRVD", 0));
                E1.Add(Blaze.TdfString.Create("FRMT", "%d"));
                E1.Add(Blaze.TdfString.Create("KIND", ""));
                E1.Add(Blaze.TdfString.Create("LDSC", "CONST_HEAD_LONG"));
                E1.Add(Blaze.TdfString.Create("META", ""));
                E1.Add(Blaze.TdfString.Create("NAME", "head"));
                E1.Add(Blaze.TdfString.Create("SDSC", "CONST_HEAD"));
                E1.Add(Blaze.TdfInteger.Create("TYPE", 0));
                STAT.Add(Blaze.TdfStruct.Create("1", E1));
                List<Blaze.Tdf> E2 = new List<Blaze.Tdf>();
                E2.Add(Blaze.TdfString.Create("CATG", "const"));
                E2.Add(Blaze.TdfString.Create("DFLT", "0"));
                E2.Add(Blaze.TdfInteger.Create("DRVD", 0));
                E2.Add(Blaze.TdfString.Create("FRMT", "%d"));
                E2.Add(Blaze.TdfString.Create("KIND", ""));
                E2.Add(Blaze.TdfString.Create("LDSC", "CONST_HAIR_LONG"));
                E2.Add(Blaze.TdfString.Create("META", ""));
                E2.Add(Blaze.TdfString.Create("NAME", "hair"));
                E2.Add(Blaze.TdfString.Create("SDSC", "CONST_HAIR"));
                E2.Add(Blaze.TdfInteger.Create("TYPE", 0));
                STAT.Add(Blaze.TdfStruct.Create("2", E2));
                List<Blaze.Tdf> E3 = new List<Blaze.Tdf>();
                E3.Add(Blaze.TdfString.Create("CATG", "const"));
                E3.Add(Blaze.TdfString.Create("DFLT", "0"));
                E3.Add(Blaze.TdfInteger.Create("DRVD", 0));
                E3.Add(Blaze.TdfString.Create("FRMT", "%d"));
                E3.Add(Blaze.TdfString.Create("KIND", ""));
                E3.Add(Blaze.TdfString.Create("LDSC", "CONST_FACIAL_FEATURE_LONG"));
                E3.Add(Blaze.TdfString.Create("META", ""));
                E3.Add(Blaze.TdfString.Create("NAME", "facialFeature"));
                E3.Add(Blaze.TdfString.Create("SDSC", "CONST_FACIAL_FEATURE"));
                E3.Add(Blaze.TdfInteger.Create("TYPE", 0));
                STAT.Add(Blaze.TdfStruct.Create("3", E3));
                Result.Add(Blaze.TdfList.Create("STAT", 3, 4, STAT));
            }
            if (which == "crit")
            {
                Result.Add(Blaze.TdfString.Create("DESC", "Critical stats"));
                Result.Add(Blaze.TdfDoubleVal.Create("ETYP", new Blaze.DoubleVal(30722, 1)));
                Result.Add(Blaze.TdfString.Create("META", ""));
                Result.Add(Blaze.TdfString.Create("NAME", "crit"));
                List<Blaze.TdfStruct> STAT = new List<Blaze.TdfStruct>();
                List<Blaze.Tdf> E0 = new List<Blaze.Tdf>();
                E0.Add(Blaze.TdfString.Create("CATG", "crit"));
                E0.Add(Blaze.TdfString.Create("DFLT", "1"));
                E0.Add(Blaze.TdfInteger.Create("DRVD", 0));
                E0.Add(Blaze.TdfString.Create("FRMT", "%d"));
                E0.Add(Blaze.TdfString.Create("KIND", ""));
                E0.Add(Blaze.TdfString.Create("LDSC", "CRIT_LEVEL_LONG"));
                E0.Add(Blaze.TdfString.Create("META", ""));
                E0.Add(Blaze.TdfString.Create("NAME", "level"));
                E0.Add(Blaze.TdfString.Create("SDSC", "CRIT_LEVEL"));
                E0.Add(Blaze.TdfInteger.Create("TYPE", 0));
                STAT.Add(Blaze.TdfStruct.Create("0", E0));
                List<Blaze.Tdf> E1 = new List<Blaze.Tdf>();
                E1.Add(Blaze.TdfString.Create("CATG", "crit"));
                E1.Add(Blaze.TdfString.Create("DFLT", "0"));
                E1.Add(Blaze.TdfInteger.Create("DRVD", 0));
                E1.Add(Blaze.TdfString.Create("FRMT", "%d"));
                E1.Add(Blaze.TdfString.Create("KIND", ""));
                E1.Add(Blaze.TdfString.Create("LDSC", "CRIT_XP_LONG"));
                E1.Add(Blaze.TdfString.Create("META", ""));
                E1.Add(Blaze.TdfString.Create("NAME", "xp"));
                E1.Add(Blaze.TdfString.Create("SDSC", "CRIT_XP"));
                E1.Add(Blaze.TdfInteger.Create("TYPE", 0));
                STAT.Add(Blaze.TdfStruct.Create("1", E1));
                List<Blaze.Tdf> E2 = new List<Blaze.Tdf>();
                E2.Add(Blaze.TdfString.Create("CATG", "crit"));
                E2.Add(Blaze.TdfString.Create("DFLT", "0"));
                E2.Add(Blaze.TdfInteger.Create("DRVD", 0));
                E2.Add(Blaze.TdfString.Create("FRMT", "%d"));
                E2.Add(Blaze.TdfString.Create("KIND", ""));
                E2.Add(Blaze.TdfString.Create("LDSC", "CRIT_ELO_LONG"));
                E2.Add(Blaze.TdfString.Create("META", ""));
                E2.Add(Blaze.TdfString.Create("NAME", "elo"));
                E2.Add(Blaze.TdfString.Create("SDSC", "CRIT_ELO"));
                E2.Add(Blaze.TdfInteger.Create("TYPE", 0));
                STAT.Add(Blaze.TdfStruct.Create("2", E2));
                Result.Add(Blaze.TdfList.Create("STAT", 3, 3, STAT));
            }
            byte[] buff = Blaze.CreatePacket(p.Component, p.Command, 0, 0x1000, p.ID, Result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }
    }
}
