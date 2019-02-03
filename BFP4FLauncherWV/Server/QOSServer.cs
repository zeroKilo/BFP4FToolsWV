using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BFP4FLauncherWV
{
    public static class QOSServer
    {
        public static readonly object _sync = new object();
        public static TcpListener tcpQOS = null;
        public static RichTextBox box = null;
        public static bool _exit;
        private static uint _qosip = 0x7F000001;
        private static int _qosport = 17400;
        private static readonly object _sync2 = new object();

        public static void Start()
        {
            SetExit(false);
            Log("Starting QOS Server...");
            new Thread(tQOSMain).Start();
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(10);
                Application.DoEvents();
            }
        }

        public static void Stop()
        {
            Log("QOS Server stopping...");
            if (tcpQOS != null) 
                tcpQOS.Stop();
            SetExit(true);
            Log("Done.");
        }

        public static void tQOSMain(object obj)
        {
            try
            {
                IPAddress addr = IPAddress.Parse(ProviderInfo.QOS_IP);
                _qosip = BitConverter.ToUInt32(addr.GetAddressBytes().Reverse().ToArray(), 0);
                Log("[QOS ] QOS Server starting...");
                tcpQOS = new TcpListener(addr, 17502);
                Log("[QOS ] QOS Server TCP bound to  " + ProviderInfo.QOS_IP + ":17502");
                tcpQOS.Start();
                Log("[QOS ] QOS Server listening...");
                while (!GetExit())
                {
                    TcpClient client = tcpQOS.AcceptTcpClient();
                    new Thread(tQOSClientHandler).Start(client);
                }
            }
            catch (Exception ex)
            {
                LogError("QOS ", ex);
            }
        }

        public static void tQOSClientHandler(object obj)
        {
            TcpClient client = (TcpClient)obj;
            Log("[QOS ] Client connected...");
            NetworkStream ns = client.GetStream();
            ns.ReadTimeout = 10;
            string request = ReadToEnd(ns);
            if (request.StartsWith("GET /qos/qos?vers=1&qtyp=1"))
                QOSTest1(client, ns);
            else if (request.StartsWith("GET /qos/qos?vers=1&qtyp=2"))
                QOSTest2(client, ns);
            else if (request.StartsWith("GET /qos/firewall?vers=1"))
                QOSTest3(client, ns);
            else
                Log("[QOS ] Received unknown:\n" + request);
            GC.Collect();
        }

        private static void QOSTest1(TcpClient client, Stream ns)
        {
            int qPort = GetNextQOSPort();
            Log("[QOS ] Client requested test 1 got port " + qPort + "...");
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            string s = Resources.Resource1.template2.Replace("#QOSIP#", _qosip.ToString()).Replace("#QOSPORT#", qPort.ToString());
            s = Resources.Resource1.template1.Replace("#SIZE#", s.Length.ToString()) + s;
            WriteString(ns, s);
            client.Close();
            UdpClient server = new UdpClient(new IPEndPoint(IPAddress.Parse(ProviderInfo.QOS_IP), qPort));
            List<byte[]> msgs = new List<byte[]>();
            for (int i = 0; i < 10; i++)
            {
                byte[] data = WaitForUdp(server, ref remote);
                if (data != null)
                    msgs.Add(data);
            }
            Log("[QOS ] Request " + qPort + " : UDP Messages received");
            UdpClient udpclient = new UdpClient();
            if (msgs.Count != 0)
            {
                udpclient.Connect(remote);
                for (int i = 0; i < msgs.Count; i++)
                {
                    MemoryStream m = new MemoryStream();
                    m.Write(msgs[i], 0, msgs[i].Length);
                    m.Write(new byte[] { 0x4f, 0xf9, 0x68, 0x55, 0x48, 0x87, 0x00, 0x00, 0x00, 0x00 }, 0, 10);
                    byte[] data = m.ToArray();
                    udpclient.Send(data, data.Length);
                }
            }
            Log("[QOS ] Request " + qPort + " : UDP Messages send");
            udpclient.Close();
        }

        private static void QOSTest2(TcpClient client, Stream ns)
        {
            int qPort = GetNextQOSPort();
            Log("[QOS ] Client requested test 2 got port " + qPort + "...");
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            string s = Resources.Resource1.template4.Replace("#QOSIP#", _qosip.ToString()).Replace("#QOSPORT#", qPort.ToString());
            s = Resources.Resource1.template3.Replace("#SIZE#", s.Length.ToString()) + s;
            WriteString(ns, s);
            client.Close();
            UdpClient server = new UdpClient(new IPEndPoint(IPAddress.Parse(ProviderInfo.QOS_IP), qPort));
            List<byte[]> msgs = new List<byte[]>();
            for (int i = 0; i < 10; i++)
            {
                byte[] data = WaitForUdp(server, ref remote);
                if (data != null)
                    msgs.Add(data);
            }
            Log("[QOS ] Request " + qPort + " : UDP Messages received");
            UdpClient udpclient = new UdpClient();
            if (msgs.Count != 0)
            {
                udpclient.Connect(remote);
                for (int i = 0; i < msgs.Count; i++)
                {
                    MemoryStream m = new MemoryStream();
                    m.Write(msgs[0], 0, 15);
                    m.WriteByte((byte)(i + 1));
                    m.Write(new byte[] { 0x0A, 0x00, 0x00, 0x00, 0x04, 0x93, 0xE0, 0x00, 0x0E, 0x4B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, 16);
                    m.Write(new byte[1168], 0, 1168);
                    byte[] data = m.ToArray();
                    udpclient.Send(data, data.Length);
                }
            }
            Log("[QOS ] Request " + qPort + " : UDP Messages send");
            udpclient.Close();
        }

        private static void QOSTest3(TcpClient client, Stream ns)
        {
            int qPort = GetNextQOSPort();
            Log("[QOS ] Client requested test 3 got port " + qPort + "...");
            string s = Resources.Resource1.template6.Replace("#QOSIP#", _qosip.ToString()).Replace("#QOSPORT#", qPort.ToString());
            s = Resources.Resource1.template5.Replace("#SIZE#", s.Length.ToString()) + s;
            WriteString(ns, s);
            ns.ReadTimeout = 100;
            string request = ReadToEnd(ns);
            if (!request.StartsWith("GET /qos/firetype?vers=1"))
            {
                Log("[QOS ] Received unknown:\n" + request);
                return;
            }
            Log("[QOS ] Client requested test 4 got port " + qPort + "...");
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            UdpClient server = new UdpClient(new IPEndPoint(IPAddress.Parse(ProviderInfo.QOS_IP), qPort));
            for (int i = 0; i < 10; i++)
                WaitForUdp(server, ref remote);
            Log("[QOS ] Request " + qPort + " : UDP Messages received");
            WriteString(ns, Resources.Resource1.template7);
            client.Close();
        }

        private static int GetNextQOSPort()
        {
            int result = 0;
            lock (_sync2)
            {
                result = _qosport++;
                if (_qosport >= 17500)
                    _qosport = 17400;
            }
            return result;
        }

        private static byte[] WaitForUdp(UdpClient server, ref IPEndPoint remote)
        {
            var asyncResult = server.BeginReceive(null, null);
            asyncResult.AsyncWaitHandle.WaitOne(100);
            if (asyncResult.IsCompleted)
            {
                try
                {
                    return server.EndReceive(asyncResult, ref remote);
                }
                catch { }
            } 
            return null;
        }

        private static string ReadToEnd(Stream s)
        {
            MemoryStream m = new MemoryStream();
            int b;
            try
            {
                while ((b = s.ReadByte()) != -1)
                    m.WriteByte((byte)b);
            }
            catch { }
            return Encoding.ASCII.GetString(m.ToArray());
        }

        private static void WriteString(Stream s, string str)
        {
            byte[] data = Encoding.ASCII.GetBytes(str);
            s.Write(data, 0, data.Length);
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
