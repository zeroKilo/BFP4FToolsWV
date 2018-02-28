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

        private static void PostAuth(Blaze.Packet p, PlayerInfo pi, NetworkStream ns)
        {
            List<Blaze.Tdf> Result = new List<Blaze.Tdf>();
            List<Blaze.Tdf> PSSList = new List<Blaze.Tdf>();
            PSSList.Add(Blaze.TdfString.Create("ADRS", "127.0.0.1"));
            PSSList.Add(Blaze.TdfBlob.Create("CSIG"));
            PSSList.Add(Blaze.TdfString.Create("PJID", "303107"));
            PSSList.Add(Blaze.TdfInteger.Create("PORT", 80));
            PSSList.Add(Blaze.TdfInteger.Create("RPRT", 15));
            PSSList.Add(Blaze.TdfInteger.Create("TIID", 0));
            Result.Add(Blaze.TdfStruct.Create("PSS\0", PSSList));
            List<Blaze.Tdf> TELEList = new List<Blaze.Tdf>();
            TELEList.Add(Blaze.TdfString.Create("ADRS", "127.0.0.1"));
            TELEList.Add(Blaze.TdfInteger.Create("ANON", 0));
            TELEList.Add(Blaze.TdfString.Create("DISA", "AD,AF,AG,AI,AL,AM,AN,AO,AQ,AR,AS,AW,AX,AZ,BA,BB,BD,BF,BH,BI,BJ,BM,BN,BO,BR,BS,BT,BV,BW,BY,BZ,CC,CD,CF,CG,CI,CK,CL,CM,CN,CO,CR,CU,CV,CX,DJ,DM,DO,DZ,EC,EG,EH,ER,ET,FJ,FK,FM,FO,GA,GD,GE,GF,GG,GH,GI,GL,GM,GN,GP,GQ,GS,GT,GU,GW,GY,HM,HN,HT,ID,IL,IM,IN,IO,IQ,IR,IS,JE,JM,JO,KE,KG,KH,KI,KM,KN,KP,KR,KW,KY,KZ,LA,LB,LC,LI,LK,LR,LS,LY,MA,MC,MD,ME,MG,MH,ML,MM,MN,MO,MP,MQ,MR,MS,MU,MV,MW,MY,MZ,NA,NC,NE,NF,NG,NI,NP,NR,NU,OM,PA,PE,PF,PG,PH,PK,PM,PN,PS,PW,PY,QA,RE,RS,RW,SA,SB,SC,SD,SG,SH,SJ,SL,SM,SN,SO,SR,ST,SV,SY,SZ,TC,TD,TF,TG,TH,TJ,TK,TL,TM,TN,TO,TT,TV,TZ,UA,UG,UM,UY,UZ,VA,VC,VE,VG,VN,VU,WF,WS,YE,YT,ZM,ZW,ZZ"));
            TELEList.Add(Blaze.TdfString.Create("FILT", "-UION/****"));
            TELEList.Add(Blaze.TdfInteger.Create("LOC\0", 0x656E4445));
            TELEList.Add(Blaze.TdfString.Create("NOOK", "US, CA, MX"));
            TELEList.Add(Blaze.TdfInteger.Create("PORT", 80));
            TELEList.Add(Blaze.TdfInteger.Create("SDLY", 0x3A98));
            TELEList.Add(Blaze.TdfString.Create("SESS", "JMhnT9dXSED"));
            byte[] skey = { 0x5E, 0x8A, 0xCB, 0xDD, 0xF8, 0xEC, 0xC1, 0x95, 0x98, 0x99, 0xF9, 0x94, 0xC0, 0xAD, 0xEE, 0xFC, 0xCE, 0xA4, 0x87, 0xDE, 0x8A, 0xA6, 0xCE, 0xDC, 0xB0, 0xEE, 0xE8, 0xE5, 0xB3, 0xF5, 0xAD, 0x9A, 0xB2, 0xE5, 0xE4, 0xB1, 0x99, 0x86, 0xC7, 0x8E, 0x9B, 0xB0, 0xF4, 0xC0, 0x81, 0xA3, 0xA7, 0x8D, 0x9C, 0xBA, 0xC2, 0x89, 0xD3, 0xC3, 0xAC, 0x98, 0x96, 0xA4, 0xE0, 0xC0, 0x81, 0x83, 0x86, 0x8C, 0x98, 0xB0, 0xE0, 0xCC, 0x89, 0x93, 0xC6, 0xCC, 0x9A, 0xE4, 0xC8, 0x99, 0xE3, 0x82, 0xEE, 0xD8, 0x97, 0xED, 0xC2, 0xCD, 0x9B, 0xD7, 0xCC, 0x99, 0xB3, 0xE5, 0xC6, 0xD1, 0xEB, 0xB2, 0xA6, 0x8B, 0xB8, 0xE3, 0xD8, 0xC4, 0xA1, 0x83, 0xC6, 0x8C, 0x9C, 0xB6, 0xF0, 0xD0, 0xC1, 0x93, 0x87, 0xCB, 0xB2, 0xEE, 0x88, 0x95, 0xD2, 0x80, 0x80 };
            string skeys = "";
            foreach (byte b in skey)
                skeys += (char)b;
            TELEList.Add(Blaze.TdfString.Create("SKEY", skeys));
            TELEList.Add(Blaze.TdfInteger.Create("SPCT", 0x4B));
            TELEList.Add(Blaze.TdfString.Create("STIM", ""));
            Result.Add(Blaze.TdfStruct.Create("TELE", TELEList));
            List<Blaze.Tdf> TICKList = new List<Blaze.Tdf>();
            TICKList.Add(Blaze.TdfString.Create("ADRS", "127.0.0.1"));
            TICKList.Add(Blaze.TdfInteger.Create("PORT", 80));
            TICKList.Add(Blaze.TdfString.Create("SKEY", "823287263,127.0.0.1:80,battlefield-p4f-pc,10,50,50,50,50,0,12"));
            Result.Add(Blaze.TdfStruct.Create("TICK", TICKList));
            List<Blaze.Tdf> UROPList = new List<Blaze.Tdf>();
            UROPList.Add(Blaze.TdfInteger.Create("TMOP", 1));
            UROPList.Add(Blaze.TdfInteger.Create("UID\0", pi.userId));
            Result.Add(Blaze.TdfStruct.Create("UROP", UROPList));
            byte[] buff = Blaze.CreatePacket(p.Component, p.Command, 0, 0x1000, p.ID, Result);
            ns.Write(buff, 0, buff.Length);
            ns.Flush();
        }
    }
}
