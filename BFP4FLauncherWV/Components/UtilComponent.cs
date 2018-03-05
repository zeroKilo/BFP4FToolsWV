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
                case 8:
                    PostAuth(p, pi, ns);
                    break;
                case 0xC:
                    UserSettingsLoadAll(p, pi, ns);
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
            List<string> t = Helper.ConvertStringList("{1} {25} {4} {27} {28} {6} {7} {9} {10} {11} {30720} {30721} {30722} {30723} {20} {30725} {30726} {2000}");
            List<long> t2 = new List<long>();
            foreach (string v in t)
                t2.Add(Convert.ToInt64(v));
            Result.Add(Blaze.TdfList.Create("CIDS", 0, t2.Count, t2));
            t = new List<string>();
            List<string> t3 = new List<string>();
            Helper.ConvertDoubleStringList("{connIdleTimeout ; 90s} {defaultRequestTimeout ; 60s} {pingPeriod ; 20s} {voipHeadsetUpdateRate ; 1000} {xlspConnectionIdleTimeout ; 300}", out t, out t3);
            Blaze.TdfDoubleList conf2 = Blaze.TdfDoubleList.Create("CONF", 1, 1, t, t3, t.Count);
            List<Blaze.Tdf> t4 = new List<Blaze.Tdf>();
            t4.Add(conf2);
            Result.Add(Blaze.TdfStruct.Create("CONF", t4));
            Result.Add(Blaze.TdfString.Create("INST", "battlefield-assault-pc"));
            Result.Add(Blaze.TdfInteger.Create("MINR", 0));
            Result.Add(Blaze.TdfString.Create("NASP", "cem_ea_id"));
            Result.Add(Blaze.TdfString.Create("PILD", ""));
            Result.Add(Blaze.TdfString.Create("PLAT", "pc"));
            List<Blaze.Tdf> QOSS = new List<Blaze.Tdf>();
            List<Blaze.Tdf> BWPS = new List<Blaze.Tdf>();
            BWPS.Add(Blaze.TdfString.Create("PSA\0", "127.0.0.1"));
            BWPS.Add(Blaze.TdfInteger.Create("PSP\0", 20000));
            BWPS.Add(Blaze.TdfString.Create("SNA\0", "wv"));
            QOSS.Add(Blaze.TdfStruct.Create("BWPS", BWPS));
            QOSS.Add(Blaze.TdfInteger.Create("LNP\0", 0xA));
            List<Blaze.Tdf> LTPS1 = new List<Blaze.Tdf>();
            LTPS1.Add(Blaze.TdfString.Create("PSA\0", "127.0.0.1"));
            LTPS1.Add(Blaze.TdfInteger.Create("PSP\0", 21000));
            LTPS1.Add(Blaze.TdfString.Create("SNA\0", "wv"));
            List<Blaze.TdfStruct> LTPS = new List<Blaze.TdfStruct>();
            LTPS.Add(Blaze.CreateStructStub(LTPS1));
            t = Helper.ConvertStringList("{ams}");
            QOSS.Add(Blaze.TdfDoubleList.Create("LTPS", 1, 3, t, LTPS, 1));
            QOSS.Add(Blaze.TdfInteger.Create("SVID", 0x45410805));
            Result.Add(Blaze.TdfStruct.Create("QOSS", QOSS));
            Result.Add(Blaze.TdfString.Create("RSRC", "300294"));
            Result.Add(Blaze.TdfString.Create("SVER", "Monolith"));
            byte[] buff = Blaze.CreatePacket(p.Component, p.Command, 0, 0x1000, p.ID, Result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }

        private static void PostAuth(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Tdf> Result = new List<Blaze.Tdf>();
            List<Blaze.Tdf> PSSList = new List<Blaze.Tdf>();
            PSSList.Add(Blaze.TdfString.Create("ADRS", "127.0.0.1"));
            PSSList.Add(Blaze.TdfString.Create("PJID", "123071"));
            PSSList.Add(Blaze.TdfInteger.Create("PORT", 80));
            PSSList.Add(Blaze.TdfInteger.Create("RPRT", 9));
            Result.Add(Blaze.TdfStruct.Create("PSS\0", PSSList));
            List<Blaze.Tdf> TELEList = new List<Blaze.Tdf>();
            TELEList.Add(Blaze.TdfString.Create("ADRS", "127.0.0.1"));
            TELEList.Add(Blaze.TdfInteger.Create("ANON", 0));
            TELEList.Add(Blaze.TdfString.Create("DISA", "AD,AF,AG,AI,AL,AM,AN,AO,AQ,AR,AS,AW,AX,AZ,BA,BB,BD,BF,BH,BI,BJ,BM,BN,BO,BR,BS,BT,BV,BW,BY,BZ,CC,CD,CF,CG,CI,CK,CL,CM,CN,CO,CR,CU,CV,CX,DJ,DM,DO,DZ,EC,EG,EH,ER,ET,FJ,FK,FM,FO,GA,GD,GE,GF,GG,GH,GI,GL,GM,GN,GP,GQ,GS,GT,GU,GW,GY,HM,HN,HT,ID,IL,IM,IN,IO,IQ,IR,IS,JE,JM,JO,KE,KG,KH,KI,KM,KN,KP,KR,KW,KY,KZ,LA,LB,LC,LI,LK,LR,LS,LY,MA,MC,MD,ME,MG,MH,ML,MM,MN,MO,MP,MQ,MR,MS,MU,MV,MW,MY,MZ,NA,NC,NE,NF,NG,NI,NP,NR,NU,OM,PA,PE,PF,PG,PH,PK,PM,PN,PS,PW,PY,QA,RE,RS,RW,SA,SB,SC,SD,SG,SH,SJ,SL,SM,SN,SO,SR,ST,SV,SY,SZ,TC,TD,TF,TG,TH,TJ,TK,TL,TM,TN,TO,TT,TV,TZ,UA,UG,UM,UY,UZ,VA,VC,VE,VG,VN,VU,WF,WS,YE,YT,ZM,ZW,ZZ"));
            TELEList.Add(Blaze.TdfString.Create("FILT", ""));
            TELEList.Add(Blaze.TdfInteger.Create("LOC\0", 0x656E5553));
            TELEList.Add(Blaze.TdfString.Create("NOOK", "US, CA, MX"));
            TELEList.Add(Blaze.TdfInteger.Create("PORT", 80));
            TELEList.Add(Blaze.TdfInteger.Create("SDLY", 0x3A98));
            TELEList.Add(Blaze.TdfString.Create("SESS", "tele_sess"));
            TELEList.Add(Blaze.TdfString.Create("SKEY", "some_tele_key"));
            TELEList.Add(Blaze.TdfInteger.Create("SPCT", 0x4B));
            TELEList.Add(Blaze.TdfString.Create("STIM", "Default"));
            Result.Add(Blaze.TdfStruct.Create("TELE", TELEList));
            List<Blaze.Tdf> TICKList = new List<Blaze.Tdf>();
            TICKList.Add(Blaze.TdfString.Create("ADRS", "127.0.0.1"));
            TICKList.Add(Blaze.TdfInteger.Create("PORT", 80));
            TICKList.Add(Blaze.TdfString.Create("SKEY", "1,127.0.0.1:80,battlefield-3-pc,10,50,50,50,50,0,0"));
            Result.Add(Blaze.TdfStruct.Create("TICK", TICKList));
            List<Blaze.Tdf> UROPList = new List<Blaze.Tdf>();
            UROPList.Add(Blaze.TdfInteger.Create("TMOP", 1));
            UROPList.Add(Blaze.TdfInteger.Create("UID\0", 1));
            Result.Add(Blaze.TdfStruct.Create("UROP", UROPList));
            byte[] buff = Blaze.CreatePacket(p.Component, p.Command, 0, 0x1000, p.ID, Result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }
        
        private static void UserSettingsLoadAll(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {

            List<Blaze.Tdf> Result = new List<Blaze.Tdf>();
            List<string> Keys = new List<string>();
            List<string> Data = new List<string>();
            //foreach (Player.PlayerInfo.SettingEntry set in player.Settings)
            //{
            //    Keys.Add(set.Key);
            //    Data.Add(set.Data);
            //}//TODO
            Result.Add(Blaze.TdfDoubleList.Create("SMAP", 1, 1, Keys, Data, Keys.Count));
            byte[] buff = Blaze.CreatePacket(p.Component, p.Command, 0, 0x1000, p.ID, Result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }
    }
}
