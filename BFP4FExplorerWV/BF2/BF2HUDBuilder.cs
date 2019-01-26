using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFP4FExplorerWV
{
    public static class BF2HUDBuilder
    {
        public static BF2HUDElement current = null;

        public static void ProcessLine(string s, int line)
        {
            string tmp = s.ToLower();
            if (tmp.StartsWith("hudbuilder.create"))
            {
                if (current != null)
                    BF2HUDLoader.elements.Add(current);
                tmp = s.Replace("\t", " ");
                while (tmp.Contains("  "))
                    tmp = tmp.Replace("  ", " ");
                string[] parts = tmp.Split(' ');
                string name = parts[2].Trim();
                string parentname = parts[1].Trim();
                BF2HUDElement parent = null;
                foreach (BF2HUDElement e in BF2HUDLoader.elements)
                    if (e.name == parentname)
                    {
                        parent = e;
                        break;
                    }
                if (parent == null)
                {
                    Log.WriteLine("[BF2 HB]   ERROR line " + line + " : cannot find parent \"" + parentname + "\"!");
                    current = null;
                    return;
                }
                current = new BF2HUDElement(name, parent);
                current.parameter.Add(s.Substring(11));
            }
            else if (current != null)
                current.parameter.Add(s.Substring(11));
        }
    }
}
