using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFP4FLauncherWV
{
    public static class BackendLog
    {
        public static string logFile = "BackendLog.txt";
        public static void Clear()
        {
            if (File.Exists(logFile))
                File.Delete(logFile);
        }

        public static void Write(string s)
        {
            File.AppendAllText(logFile, s);
        }
    }
}
