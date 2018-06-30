using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazeLibWV;
using System.Net.Sockets;

namespace BFP4FLauncherWV
{
    public static class AsyncStats
    {
        public static void GetStatsAsyncNotification(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Tdf> input = Blaze.ReadPacketContent(p);
            string statSpace = ((Blaze.TdfString)input[2]).Value;
            long vid = ((Blaze.TdfInteger)input[7]).Value;
            long eid = ((List<long>)((Blaze.TdfList)input[1]).List)[0];
            List<Blaze.Tdf> Result = new List<Blaze.Tdf>();
            Result.Add(Blaze.TdfString.Create("GRNM", statSpace));
            Result.Add(Blaze.TdfString.Create("KEY\0", "No_Scope_Defined"));
            Result.Add(Blaze.TdfInteger.Create("LAST", 1));
            List<Blaze.Tdf> STS = new List<Blaze.Tdf>();
                List<Blaze.TdfStruct> STAT = new List<Blaze.TdfStruct>();
                    List<Blaze.Tdf> e0 = new List<Blaze.Tdf>();
                    e0.Add(Blaze.TdfInteger.Create("EID\0", eid));
                    e0.Add(Blaze.TdfDoubleVal.Create("ETYP", new Blaze.DoubleVal(30722, 1)));
                    e0.Add(Blaze.TdfInteger.Create("POFF", 0));
                    List<string> values = new List<string>();
                    if (statSpace == "crit")
                        values.AddRange(new string[] { pi.profile.level.ToString(), 
                                                       pi.profile.xp.ToString(), 
                                                       "0" });
                    else
                        values.AddRange(new string[] { pi.profile.kit.ToString(), 
                                                       pi.profile.head.ToString(), 
                                                       pi.profile.face.ToString(), 
                                                       pi.profile.shirt.ToString()});
                    e0.Add(Blaze.TdfList.Create("STAT", 1, values.Count, values));
                    STAT.Add(Blaze.TdfStruct.Create("0", e0));
                STS.Add(Blaze.TdfList.Create("STAT", 3, STAT.Count, STAT));
            Result.Add(Blaze.TdfStruct.Create("STS\0", STS));
            Result.Add(Blaze.TdfInteger.Create("VID\0", vid));
            byte[] buff = Blaze.CreatePacket(7, 0x32, 0, 0x2000, 0, Result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }
    }
}
