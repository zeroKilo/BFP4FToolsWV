using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazeLibWV;
using System.Net.Sockets;

namespace BFP4FLauncherWV
{
    public static class UtilComponent
    {
        public static void HandlePacket(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            switch (p.Command)
            {
                case 2:
                    Ping(p, ns);
                    break;
                case 7:
                    PreAuth(p, ns);
                    break;
            }
        }


        private static void Ping(Blaze.Packet p, NetworkStream ns)
        {
            List<Blaze.Tdf> Result = new List<Blaze.Tdf>();
            Result.Add(Blaze.TdfInteger.Create("STIM", Blaze.GetUnixTimeStamp()));
            byte[] buff = Blaze.CreatePacket(p.Component, p.Command, 0, 0x1000, p.ID, Result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }

        private static void PreAuth(Blaze.Packet p, NetworkStream ns)
        {
            List<Blaze.Tdf> Result = new List<Blaze.Tdf>();
            Result.Add(Blaze.TdfInteger.Create("ANON", 0));
            Result.Add(Blaze.TdfString.Create("ASRC", "300294"));
            List<string> t = Helper.ConvertStringList("{1} {25} {4} {28} {7} {9} {63490} {30720} {15} {30721} {30722} {30723} {30725} {30726} {2000}");
            List<long> t2 = new List<long>();
            foreach (string v in t)
                t2.Add(Convert.ToInt64(v));
            Result.Add(Blaze.TdfList.Create("CIDS", 0, t2.Count, t2));
            t = new List<string>();
            List<string> t3 = new List<string>();
            Helper.ConvertDoubleStringList("{associationListSkipInitialSet ; 1}{blazeServerClientId ; GOS-BlazeServer-BF4-PC}{bytevaultHostname ; bytevault.gameservices.ea.com}{bytevaultPort ; 42210}{bytevaultSecure ; false}{capsStringValidationUri ; client-strings.xboxlive.com}{connIdleTimeout ; 90s}{defaultRequestTimeout ; 60s}{identityDisplayUri ; console2/welcome}{identityRedirectUri ; http://127.0.0.1/success}{nucleusConnect ; http://127.0.0.1}{nucleusProxy ; http://127.0.0.1/}{pingPeriod ; 20s}{userManagerMaxCachedUsers ; 0}{voipHeadsetUpdateRate ; 1000}{xblTokenUrn ; http://127.0.0.1}{xlspConnectionIdleTimeout ; 300}", out t, out t3);
            Blaze.TdfDoubleList conf2 = Blaze.TdfDoubleList.Create("CONF", 1, 1, t, t3, t.Count);
            List<Blaze.Tdf> t4 = new List<Blaze.Tdf>();
            t4.Add(conf2);
            Result.Add(Blaze.TdfStruct.Create("CONF", t4));
            Result.Add(Blaze.TdfString.Create("INST", "assault-pc"));
            Result.Add(Blaze.TdfInteger.Create("MINR", 0));
            Result.Add(Blaze.TdfString.Create("NASP", "cem_ea_id"));
            Result.Add(Blaze.TdfString.Create("PILD", ""));
            Result.Add(Blaze.TdfString.Create("PLAT", "Windows"));
            List<Blaze.Tdf> QOSS = new List<Blaze.Tdf>();
            List<Blaze.Tdf> BWPS = new List<Blaze.Tdf>();
            BWPS.Add(Blaze.TdfString.Create("PSA\0", "127.0.0.1"));
            BWPS.Add(Blaze.TdfInteger.Create("PSP\0", 20000));
            BWPS.Add(Blaze.TdfString.Create("SNA\0", "prod-sjc"));
            QOSS.Add(Blaze.TdfStruct.Create("BWPS", BWPS));
            QOSS.Add(Blaze.TdfInteger.Create("LNP\0", 0xA));
            List<Blaze.Tdf> LTPS1 = new List<Blaze.Tdf>();
            LTPS1.Add(Blaze.TdfString.Create("PSA\0", "127.0.0.1"));
            LTPS1.Add(Blaze.TdfInteger.Create("PSP\0", 21000));
            LTPS1.Add(Blaze.TdfString.Create("SNA\0", "prod-sjc"));
            List<Blaze.Tdf> LTPS2 = new List<Blaze.Tdf>();
            LTPS2.Add(Blaze.TdfString.Create("PSA\0", "127.0.0.1"));
            LTPS2.Add(Blaze.TdfInteger.Create("PSP\0", 22000));
            LTPS2.Add(Blaze.TdfString.Create("SNA\0", "rs-prod-iad"));
            List<Blaze.Tdf> LTPS3 = new List<Blaze.Tdf>();
            LTPS3.Add(Blaze.TdfString.Create("PSA\0", "127.0.0.1"));
            LTPS3.Add(Blaze.TdfInteger.Create("PSP\0", 23000));
            LTPS3.Add(Blaze.TdfString.Create("SNA\0", "rs-prod-lhr"));
            List<Blaze.TdfStruct> LTPS = new List<Blaze.TdfStruct>();
            LTPS.Add(Blaze.CreateStructStub(LTPS1));
            LTPS.Add(Blaze.CreateStructStub(LTPS2));
            LTPS.Add(Blaze.CreateStructStub(LTPS3));
            t = Helper.ConvertStringList("{ea-sjc}{rs-iad}{rs-lhr}");
            QOSS.Add(Blaze.TdfDoubleList.Create("LTPS", 1, 3, t, LTPS, 3));
            QOSS.Add(Blaze.TdfInteger.Create("SVID", 0x45410805));
            Result.Add(Blaze.TdfStruct.Create("QOSS", QOSS));
            Result.Add(Blaze.TdfString.Create("RSRC", "302123"));
            Result.Add(Blaze.TdfString.Create("SVER", "Blaze 3.15.7.0 (CL# 750727)"));
            byte[] buff = Blaze.CreatePacket(p.Component, p.Command, 0, 0x1000, p.ID, Result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }
    }
}
