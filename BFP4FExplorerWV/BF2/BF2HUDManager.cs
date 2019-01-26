using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFP4FExplorerWV
{
    public static class BF2HUDManager
    {
        public static List<string> parameter = new List<string>();
        public static void ProcessLine(string s, int line)
        {
            parameter.Add(s.Substring(11));
        }
    }
}
