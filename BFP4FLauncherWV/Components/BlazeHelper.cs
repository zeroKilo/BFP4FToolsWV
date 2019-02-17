using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazeLibWV;
using System.Net.Sockets;

namespace BFP4FLauncherWV
{
    public static class BlazeHelper
    {
        public static Blaze.TdfStruct MakeStatGroupEntry(int idx, string catg, string dflt, string ldsc, string name, string sdsc)
        {
            List<Blaze.Tdf> result = new List<Blaze.Tdf>();
            result.Add(Blaze.TdfString.Create("CATG", catg));
            result.Add(Blaze.TdfString.Create("DFLT", dflt));
            result.Add(Blaze.TdfInteger.Create("DRVD", 0));
            result.Add(Blaze.TdfString.Create("FRMT", "%d"));
            result.Add(Blaze.TdfString.Create("KIND", ""));
            result.Add(Blaze.TdfString.Create("LDSC", ldsc));
            result.Add(Blaze.TdfString.Create("META", ""));
            result.Add(Blaze.TdfString.Create("NAME", name));
            result.Add(Blaze.TdfString.Create("SDSC", sdsc));
            result.Add(Blaze.TdfInteger.Create("TYPE", 0));
            return Blaze.TdfStruct.Create(idx.ToString(), result);
        }

        public static Blaze.TdfStruct MakePROSEntry(int idx, PlayerInfo pi)
        {
            uint t = Blaze.GetUnixTimeStamp();
            List<Blaze.Tdf> result = new List<Blaze.Tdf>();
            result.Add(Blaze.TdfInteger.Create("EXID", pi.userId));
            result.Add(Blaze.TdfInteger.Create("GID\0", pi.game.id));
            result.Add(Blaze.TdfInteger.Create("LOC\0", pi.loc));
            result.Add(Blaze.TdfString.Create("NAME", pi.profile.name));
            result.Add(Blaze.TdfInteger.Create("PID\0", pi.userId));
            result.Add(BlazeHelper.CreateNETFieldUnion(pi, "PNET"));
            result.Add(Blaze.TdfInteger.Create("SID\0", pi.slot));
            result.Add(Blaze.TdfInteger.Create("STAT", pi.stat));
            result.Add(Blaze.TdfInteger.Create("TIDX", 0xFFFF));
            result.Add(Blaze.TdfInteger.Create("TIME", t));
            result.Add(Blaze.TdfInteger.Create("UID\0", pi.userId));
            return Blaze.TdfStruct.Create(idx.ToString(), result);
        }

        public static Blaze.Tdf CreateNETField(PlayerInfo pi, string label)
        {
            List<Blaze.TdfStruct> list = new List<Blaze.TdfStruct>();
                List<Blaze.Tdf> e0 = new List<Blaze.Tdf>();
                    List<Blaze.Tdf> EXIP = new List<Blaze.Tdf>();
                        EXIP.Add(Blaze.TdfInteger.Create("IP\0\0", pi.exIp));
                        EXIP.Add(Blaze.TdfInteger.Create("PORT", pi.exPort));
                e0.Add(Blaze.TdfStruct.Create("EXIP", EXIP));
                    List<Blaze.Tdf> INIP = new List<Blaze.Tdf>();
                        INIP.Add(Blaze.TdfInteger.Create("IP\0\0", pi.inIp));
                        INIP.Add(Blaze.TdfInteger.Create("PORT", pi.inPort));
                e0.Add(Blaze.TdfStruct.Create("INIP", INIP));
            list.Add(Blaze.TdfStruct.Create("0", e0, true));
            return Blaze.TdfList.Create(label, 3, 1, list);
        }

        public static Blaze.Tdf CreateNETFieldUnion(PlayerInfo pi, string label)
        {
            List<Blaze.Tdf> VALU = new List<Blaze.Tdf>();
                List<Blaze.Tdf> EXIP = new List<Blaze.Tdf>();
                    EXIP.Add(Blaze.TdfInteger.Create("IP", pi.exIp));
                    EXIP.Add(Blaze.TdfInteger.Create("PORT", pi.exPort));
                VALU.Add(Blaze.TdfStruct.Create("EXIP", EXIP));
            List<Blaze.Tdf> INIP = new List<Blaze.Tdf>();
                    INIP.Add(Blaze.TdfInteger.Create("IP", pi.inIp));
                    INIP.Add(Blaze.TdfInteger.Create("PORT", pi.inPort));
                VALU.Add(Blaze.TdfStruct.Create("INIP", INIP));
            return Blaze.TdfUnion.Create(label, 2, Blaze.TdfStruct.Create("VALU", VALU));
        }

        public static Blaze.Tdf CreateADDRField(PlayerInfo pi)
        {
            List<Blaze.Tdf> ADDR = new List<Blaze.Tdf>();
                List<Blaze.Tdf> EXIP = new List<Blaze.Tdf>();
                    EXIP.Add(Blaze.TdfInteger.Create("IP", pi.exIp));
                    EXIP.Add(Blaze.TdfInteger.Create("PORT", pi.exPort));
            ADDR.Add(Blaze.TdfStruct.Create("EXIP", EXIP));
                List<Blaze.Tdf> INIP = new List<Blaze.Tdf>();
                    INIP.Add(Blaze.TdfInteger.Create("IP", pi.inIp));
                    INIP.Add(Blaze.TdfInteger.Create("PORT", pi.inPort));
            ADDR.Add(Blaze.TdfStruct.Create("INIP", INIP));
            return Blaze.TdfStruct.Create("ADDR", ADDR, true);
        }

        public static Blaze.Tdf CreateNQOSField(PlayerInfo pi, string label)
        {
            List<Blaze.Tdf> NQOS = new List<Blaze.Tdf>();
                NQOS.Add(Blaze.TdfInteger.Create("DBPS", 0));
                NQOS.Add(Blaze.TdfInteger.Create("NATT", pi.nat));
                NQOS.Add(Blaze.TdfInteger.Create("UBPS", 0));
            return Blaze.TdfStruct.Create(label, NQOS);
        }

        public static Blaze.TdfStruct CreateUserStruct(PlayerInfo pi)
        {
            List<Blaze.Tdf> USER = new List<Blaze.Tdf>();
            USER.Add(Blaze.TdfInteger.Create("AID\0", pi.userId));
            USER.Add(Blaze.TdfInteger.Create("ALOC", pi.loc));
            USER.Add(Blaze.TdfInteger.Create("EXID\0", pi.userId));
            USER.Add(Blaze.TdfInteger.Create("ID\0\0", pi.userId));
            USER.Add(Blaze.TdfString.Create("NAME", pi.profile.name));
            return Blaze.TdfStruct.Create("USER", USER);
        }

        public static Blaze.Tdf CreateUserDataStruct(PlayerInfo pi, string name = "DATA")
        {
            List<Blaze.Tdf> DATA = new List<Blaze.Tdf>();
                DATA.Add(BlazeHelper.CreateNETFieldUnion(pi, "ADDR"));
                DATA.Add(BlazeHelper.CreateNQOSField(pi, "QDAT"));
            return Blaze.TdfStruct.Create(name, DATA);
        }
    }
}
