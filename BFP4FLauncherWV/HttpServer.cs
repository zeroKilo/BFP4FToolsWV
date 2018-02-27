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
using BlazeLibWV;

namespace BFP4FLauncherWV
{
    public static class HttpServer
    {
        public static readonly object _sync = new object();
        public static bool _exit;
        public static RichTextBox box = null;
        public static TcpListener lHttp = null;

        public static void Start()
        {
            SetExit(false);
            Log("Starting Http...");
            new Thread(tHTTPMain).Start();
            for (int i = 0; i < 50; i++)
            {
                Thread.Sleep(10);
                Application.DoEvents();
            }
        }

        public static void Stop()
        {
            Log("Http stopping...");
            if (lHttp != null) lHttp.Stop();
            SetExit(true);
            Log("Done.");
        }

        public static void tHTTPMain(object obj)
        {
            try
            {
                Log("[HTTP] starting...");
                lHttp = new TcpListener(IPAddress.Parse("127.0.0.1"), 80);
                Log("[HTTP] bound to port 80");
                lHttp.Start();
                Log("[HTTP] listening...");
                TcpClient client;
                while (!GetExit())
                {
                    client = lHttp.AcceptTcpClient();
                    Log("[HTTP] Client connected");
                    NetworkStream ns = client.GetStream();
                    byte[] data = Helper.ReadContentTCP(ns);
                    Log("[HTTP] Received " + data.Length + " bytes of data");
                    try
                    {
                        ProcessHttp(Encoding.ASCII.GetString(data), ns);
                    }
                    catch { }
                    client.Close();
                    Log("[HTTP] Client disconnected");
                }
            }
            catch (Exception ex)
            {
                LogError("HTTP", ex);
            }
        }

        public static void ProcessHttp(string data, Stream s)
        {
            string[] lines = data.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Log("[HTTP] Request: " + lines[0]);
            string cmd = lines[0].Split(' ')[0];
            string url = lines[0].Split(' ')[1];
            if (cmd == "GET")
            {
                switch (url)
                {
                    case "/api/nucleus/authToken":
                        Log("[HTTP] Sending AuthToken");
                        ReplyWithXML(s, "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>\r\n<success><token code=\"NEW_TOKEN\">SIxmvSLJSOwKPq5WZ3FL5KIRNJVCLp4Jgs_3mJcY2yJahXxR5mTRGUsi6PKhA4X1jpuVMxHJQv3WQ3HnQfvKeG60hRugA</token></success>");
                        break;
                }
            }
            if (cmd == "POST")
            {
                int pos = data.IndexOf("\r\n\r\n");
                if (pos != -1)
                    Log("[HTTP] Content: \n" + data.Substring(pos + 4));
            }
        }

        public static void ReplyWithXML(Stream s, string c)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("HTTP/1.1 200 OK");
            sb.AppendLine("Date: " + DateTime.Now.ToUniversalTime().ToString("r"));
            sb.AppendLine("Server: Warranty Voiders");
            sb.AppendLine("Content-Length: 189");
            sb.AppendLine("Keep-Alive: timeout=5, max=100");
            sb.AppendLine("Connection: Keep-Alive");
            sb.AppendLine();
            sb.Append(c);
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
