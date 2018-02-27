using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BFP4FLauncherWV
{
    public static class Helper
    {
        public static List<string> ConvertStringList(string data)
        {
            List<string> res = new List<string>();
            string t = data.Replace("{", "");
            string[] t2 = t.Split('}');
            foreach (string line in t2)
                if (line.Trim() != "")
                    res.Add(line.Trim());
            return res;
        }
        public static void ConvertDoubleStringList(string data, out List<string> list1, out List<string> list2)
        {
            List<string> res1 = new List<string>();
            List<string> res2 = new List<string>();
            string t = data.Replace("{", "");
            string[] t2 = t.Split('}');
            foreach (string line in t2)
                if (line.Trim() != "")
                {
                    string[] t3 = line.Trim().Split(';');
                    res1.Add(t3[0].Trim());
                    res2.Add(t3[1].Trim());
                }
            list1 = res1;
            list2 = res2;
        }

        public static byte[] ReadContentSSL(SslStream sslStream)
        {
            MemoryStream res = new MemoryStream();
            byte[] buff = new byte[0x10000];
            sslStream.ReadTimeout = 100;
            int bytesRead;
            try
            {
                while ((bytesRead = sslStream.Read(buff, 0, 0x10000)) > 0)
                    res.Write(buff, 0, bytesRead);
            }
            catch { }
            sslStream.Flush();
            return res.ToArray();
        }

        public static byte[] ReadContentTCP(NetworkStream Stream)
        {
            MemoryStream res = new MemoryStream();
            byte[] buff = new byte[0x10000];
            Stream.ReadTimeout = 100;
            int bytesRead;
            try
            {
                while ((bytesRead = Stream.Read(buff, 0, 0x10000)) > 0)
                    res.Write(buff, 0, bytesRead);
            }
            catch { }
            Stream.Flush();
            return res.ToArray();
        }
    }
}
