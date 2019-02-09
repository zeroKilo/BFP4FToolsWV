using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BFP4FLauncherWV
{
    public static class MagmaServer
    {
        public static bool basicMode = false;
        public static readonly object _sync = new object();
        public static bool _exit;
        public static RichTextBox box = null;
        public static TcpListener lMagma = null;

        public static void Start()
        {
            SetExit(false);
            Log("Starting Magma...");
            new Thread(tHTTPMain).Start();
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(10);
                Application.DoEvents();
            }
        }

        public static void Stop()
        {
            Log("Magma stopping...");
            if (lMagma != null) lMagma.Stop();
            SetExit(true);
            Log("Done.");
        }

        public static void tHTTPMain(object obj)
        {
            try
            {
                Log("[MGMA] starting...");
                lMagma = new TcpListener(IPAddress.Parse(ProviderInfo.backendIP), 80);
                Log("[MGMA] bound to  " + ProviderInfo.backendIP + ":80");
                lMagma.Start();
                Log("[MGMA] listening...");
                TcpClient client;
                while (!GetExit())
                {
                    client = lMagma.AcceptTcpClient();
                    NetworkStream ns = client.GetStream();
                    byte[] data = Helper.ReadContentTCP(ns);
                    if(!basicMode)
                        Log("[MGMA] Recvdump:\n" + Encoding.ASCII.GetString(data));
                    try
                    {
                        ProcessMagma(Encoding.ASCII.GetString(data), ns);
                    }
                    catch { }
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                LogError("MGMA", ex);
            }
        }

        public static void ProcessMagma(string data, Stream s)
        {
            string[] lines = data.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Log("[MGMA] Request: " + lines[0]);
            string cmd = lines[0].Split(' ')[0];
            string url = lines[0].Split(' ')[1].Split(':')[0];
            if (cmd == "GET")
            {
                switch (url)
                {
                    case "/api/nucleus/authToken":
                        Log("[MGMA] Sending AuthToken");
                        if (lines.Length > 5 && lines[5].StartsWith("x-server-key"))
                            ReplyWithXML(s, "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>\r\n<success><token>" + lines[5].Split(':')[1].Trim() + "</token></success>");
                        else
                            ReplyWithXML(s, "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>\r\n<success><token code=\"NEW_TOKEN\">" + lines[4].Split('=')[1] + "</token></success>");
                        return;
                    case "/api/relationships/roster/nucleus":
                        Log("[MGMA] Sending Roster response");
                        ReplyWithXML(s, "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>\r\n<roster relationships=\"0\"/><success code=\"SUCCESS\"/>");
                        return;
                    case "/wv/getProfiles":
                        Log("[MGMA] Sending Player Profiles");
                        StringBuilder sb = new StringBuilder();
                        Profiles.Refresh();
                        sb.Append("<profiles>\r\n");
                        foreach (Profile p in Profiles.profiles)
                            sb.Append("<profile name='" + Profiles.getProfilePath(p.id) + "'>" + Convert.ToBase64String(Encoding.Unicode.GetBytes(p._raw)) + "</profile>\r\n");
                        sb.Append("</profiles>\r\n");
                        ReplyWithXML(s, "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>\r\n" + sb.ToString());
                        break;
                }
                if (url.StartsWith("/api/nucleus/name/"))
                {
                    int id = Convert.ToInt32(url.Substring(18));
                    Log("[MGMA] Sending name response for PID " + id);
                    PlayerInfo p = null;
                    foreach(PlayerInfo pi in BlazeServer.allClients)
                        if (pi.userId == id)
                        {
                            p = pi;
                            break;
                        }
                    if (p == null)
                    {
                        Log("[MGMA] Cant find player id!");
                        return;
                    }
                    ReplyWithXML(s, "<name>" + p.profile.name + "</name>");
                }
                if (url.StartsWith("/api/nucleus/entitlements/"))
                {
                    int id = Convert.ToInt32(url.Substring(26));
                    Log("[MGMA] Sending entitlement response for PID " + id);
                    PlayerInfo p = null;
                    foreach (PlayerInfo pi in BlazeServer.allClients)
                        if (pi.userId == id)
                        {
                            p = pi;
                            break;
                        }
                    if (p == null)
                    {
                        Log("[MGMA] Cant find player id!");
                        return;
                    }
                    string response = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><entitlements count=\"123\">";
                    string[] ids = "2004 2005 2006 2007 2008 2017 2018 2021 2023 2054 2168 3000 3001 3002 3003 3004 3005 3006 3007 3008 3009 3011 3012 3013 3014 3015 3016 3017 3018 3019 3020 3021 3022 3023 3024 3025 3026 3027 3029 3038 3041 3043 3044 3045 3047 3048 3050 3051 3052 3061 3062 3063 3064 3065 3066 3067 3068 3069 3070 3071 3072 3073 3074 3075 3076 3077 3078 3079 3080 3081 3082 3084 3085 3086 3087 3088 3089 3090 3091 3092 3094 3095 3096 3097 3099 3100 3101 3102 3103 3104 3105 3106 3107 3108 3109 3110 3111 3112 3113 3114 3115 3116 3117 3118 3119 3120 3121 3122 3124 3126 3127 3128 3129 3130 3131 3132 3133 3134 3135 3136 3137 3138 3139".Split(' ');
                    int i = 1;
                    foreach (var entitlement in ids)
                    {
                        response += "<entitlement><entitlementId>" 
                                 + Convert.ToString(i) 
                                 + "</entitlementId><entitlementTag>" 
                                 + entitlement 
                                 + "-UNLM-</entitlementTag><useCount>1</useCount><grantDate>" 
                                 + DateTime.UtcNow.ToString("MMM-dd-yyyy HH:mm:ss UTC") 
                                 + "</grantDate><terminationDate></terminationDate><status>ACTIVE</status></entitlement>";
                        i++;
                    }
                    response += "</entitlements>";
                    ReplyWithXML(s, response);
                }
            }
            if (cmd == "POST" && !basicMode)
            {
                int pos = data.IndexOf("\r\n\r\n");
                if (pos != -1)
                    Log("[MGMA] Content: \n" + data.Substring(pos + 4));
            }
        }

        public static void ReplyWithXML(Stream s, string c)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("HTTP/1.1 200 OK");
            sb.AppendLine("Date: " + DateTime.Now.ToUniversalTime().ToString("r"));
            sb.AppendLine("Server: Warranty Voiders");
            sb.AppendLine("Content-Length: " + c.Length);
            sb.AppendLine("Keep-Alive: timeout=5, max=100");
            sb.AppendLine("Connection: Keep-Alive");
            sb.AppendLine();
            sb.Append(c);
            if (!basicMode)
            {
                Log("[MGMA] Sending: \n" + sb.ToString());
            }
            byte[] buf = Encoding.ASCII.GetBytes(sb.ToString());
            s.Write(buf, 0, buf.Length);
        }

        public static void SetExit(bool state)
        {
            lock (_sync)
            {
                _exit = state;
            }
        }

        public static bool GetExit()
        {
            bool result;
            lock (_sync)
            {
                result = _exit;
            }
            return result;
        }

        public static void Log(string s)
        {
            if (box == null) return;
            try
            {
                box.Invoke(new Action(delegate
                {
                    string stamp = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " : ";
                    box.AppendText(stamp + s + "\n");
                    BackendLog.Write(stamp + s + "\n");
                    box.SelectionStart = box.Text.Length;
                    box.ScrollToCaret();
                }));
            }
            catch { }
        }

        public static void LogError(string who, Exception e, string cName = "")
        {
            string result = "";
            if (who != "") result = "[" + who + "] " + cName + " ERROR: ";
            result += e.Message;
            if (e.InnerException != null)
                result += " - " + e.InnerException.Message;
            Log(result);
        }
    }
}
