using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BFP4FExplorerWV
{
    public static class BF2HUDLoader
    {
        public static List<BF2HUDElement> elements;

        public static void Init()
        {
            elements = new List<BF2HUDElement>();
            elements.Add(new BF2HUDElement("Global", null));
            elements.Add(new BF2HUDElement("TopLayer", null));
            elements.Add(new BF2HUDElement("TopLeft", null));
            elements.Add(new BF2HUDElement("TopRight", null));
            elements.Add(new BF2HUDElement("BottomLeftStatic", null));
            elements.Add(new BF2HUDElement("BottomRightStatic", null));
            elements.Add(new BF2HUDElement("BottomLeftAnimate", null));
            elements.Add(new BF2HUDElement("BottomRightAnimate", null));
            BF2HUDManager.parameter = new List<string>();
            BF2HUDBuilder.current = null;
            Log.WriteLine("[BF2 HL] Loading HUD...");
            Load("menu\\HUD\\HudSetup\\HudSetupMain.con");
        }

        public static void Load(string filename)
        {
            Log.WriteLine("[BF2 HL]  running \"" + filename + "\"...");
            BF2FileSystem.BF2FSEntry e = BF2FileSystem.FindEntryFromIngamePath(filename);
            if (e == null)
                return;
            byte[] data = BF2FileSystem.GetFileFromZip(e.zipFile, e.inZipPath);
            if (data == null)
            {
                Log.WriteLine("[BF2 HL]  ERROR file not found!");
                return;
            }
            string basePath = Path.GetDirectoryName(filename) + "\\";
            string[] lines = SplitBinText(data);
            int count = 0;
            foreach (string line in lines)
            {
                count++;
                string[] parts;
                string tmp = line.ToLower();
                if (tmp.Trim() == "") continue;
                if (tmp.StartsWith("rem")) continue;
                if (tmp.StartsWith("run"))
                {
                    parts = line.Split(' ');
                    Load(basePath + parts[1].Replace("\"", "").Replace("/", "\\"));
                }
                else if (tmp.StartsWith("hudbuilder"))
                    BF2HUDBuilder.ProcessLine(line, count);
                else if (tmp.StartsWith("hudmanager"))
                    BF2HUDManager.ProcessLine(line, count);
            }
        }

        private static string[] SplitBinText(byte[] data)
        {
            List<string> result = new List<string>();
            string s = Encoding.ASCII.GetString(data);
            string line;
            StringReader sr = new StringReader(s);
            while ((line = sr.ReadLine()) != null)
                result.Add(line);
            return result.ToArray();
        }

        public static TreeNode MakeTree()
        {
            TreeNode result = new TreeNode("HUD");
            foreach (BF2HUDElement e in elements)
                if (e.parent == null)
                    result.Nodes.Add(e.name, e.name);
                else
                {
                    TreeNode[] t = result.Nodes.Find(e.parent.name, true);
                    if (t != null && t.Length != 0)
                        t[0].Nodes.Add(e.name, e.name);
                }
            result.Expand();
            return result;
        }
    }
}
