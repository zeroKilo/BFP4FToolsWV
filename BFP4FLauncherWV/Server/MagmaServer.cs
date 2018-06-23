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
        public static readonly object _sync = new object();
        public static bool _exit;
        public static RichTextBox box = null;
        public static TcpListener lMagma = null;

        public static void Start()
        {
            SetExit(false);
            Log("Starting Magma...");
            new Thread(tHTTPMain).Start();
            for (int i = 0; i < 50; i++)
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
                    Log("[MGMA] Client connected");
                    NetworkStream ns = client.GetStream();
                    byte[] data = Helper.ReadContentTCP(ns);
                    Log("[MGMA] Received " + data.Length + " bytes of data");
                    Log("[MGMA] Recvdump:\n" + Encoding.ASCII.GetString(data));
                    try
                    {
                        ProcessMagma(Encoding.ASCII.GetString(data), ns);
                    }
                    catch { }
                    client.Close();
                    Log("[MGMA] Client disconnected");
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
                        break;
                    case "/api/relationships/roster/nucleus":
                        Log("[MGMA] Sending Roster response");
                        ReplyWithXML(s, "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>\r\n<roster relationships=\"0\"/><success code=\"SUCCESS\"/>");
                        break;
                }
            }
            if (cmd == "POST")
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
            Log("[MGMA] Sending: \n" + sb.ToString());
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
                    box.Text += stamp + s + "\n";
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
