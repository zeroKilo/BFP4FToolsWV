using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BFP4FExplorerWV
{
    public static class BF2FileSystem
    {
        public class BF2FSEntry
        {
            public string zipFile;
            public string inZipPath;
            public string inFSPath;
            public BF2FSEntry(string _zipFile, string _inZipFile, string _inFSPath)
            {
                zipFile = _zipFile;
                inZipPath = _inZipFile;
                inFSPath = _inFSPath;
            }
        }

        public static List<BF2FSEntry> clientFS = new List<BF2FSEntry>();
        public static List<BF2FSEntry> serverFS = new List<BF2FSEntry>();
        public static string basepath;

        public static void Load()
        {
            Log.WriteLine("[BF2_FS] Loading client filesystem...");
            clientFS = MountZipFiles(basepath + "ClientArchives.con");
            Log.WriteLine("[BF2_FS] Loading server filesystem...");
            serverFS = MountZipFiles(basepath + "ServerArchives.con");
        }

        public static void LoadLevel(string level)
        {
            Log.WriteLine("[BF2_FS] Loading level client data...");
            clientFS.AddRange(MountZipFile(basepath + "Levels\\" + level + "\\client.zip", "Levels\\" + level));
            Log.WriteLine("[BF2_FS] Loading level server data...");
            serverFS.AddRange(MountZipFile(basepath + "Levels\\" + level + "\\server.zip", "Levels\\" + level));
        }

        private static List<BF2FSEntry> MountZipFiles(string filename)
        {
            List<BF2FSEntry> result = new List<BF2FSEntry>();
            string[] lines = File.ReadAllLines(filename);
            int count = 0;
            foreach (string line in lines)
            {
                Log.SetProgress(0, lines.Length, count++);
                if (line.Trim().ToLower().StartsWith("filemanager.mountarchive"))
                {

                    string[] parts = line.Trim().ToLower().Split(' ');
                    Log.WriteLine("[BF2_FS]  mounting \"" + parts[1].Replace("/","\\") + "\" to \"" + parts[2] + "\"");
                    result.AddRange(MountZipFile(basepath + parts[1].Replace("/", "\\"), parts[2]));
                }
            }
            Log.SetProgress(0, 0, 0);
            return result;
        }

        private static List<BF2FSEntry> MountZipFile(string zipFile, string mountPoint)
        {
            List<BF2FSEntry> result = new List<BF2FSEntry>();
            if (!File.Exists(zipFile))
            {
                Log.WriteLine("[BF2_FS]  error \"" + zipFile + "\" not found");
                return result;
            }
            ZipArchive zip = ZipFile.OpenRead(zipFile);
            foreach (ZipArchiveEntry entry in zip.Entries)
                result.Add(new BF2FSEntry(zipFile, entry.FullName, mountPoint + "\\" + entry.FullName.Replace("/", "\\")));
            zip.Dispose();
            return result;
        }

        public static TreeNode MakeFSTree()
        {
            TreeNode t = new TreeNode("File System");
            TreeNode tC = new TreeNode("Client");
            foreach (BF2FSEntry entry in clientFS)
                AddPath(tC, entry.inFSPath.Split('\\'));
            tC.Expand();
            t.Nodes.Add(tC);
            TreeNode tS = new TreeNode("Server");
            foreach (BF2FSEntry entry in serverFS)
                AddPath(tS, entry.inFSPath.Split('\\'));
            tS.Expand();
            t.Nodes.Add(tS);
            t.Expand();
            return t;
        }

        public static TreeNode MakeFSTreeFiltered(string[] endings)
        {
            TreeNode t = new TreeNode("File System");
            TreeNode tC = new TreeNode("Client");
            foreach (BF2FSEntry entry in clientFS)
            {
                string ext = Path.GetExtension(entry.inFSPath).Trim().ToLower();
                foreach(string ending in endings)
                    if (ending == ext)
                    {
                        AddPath(tC, entry.inFSPath.Split('\\'));
                        break;
                    }
            }
            tC.Expand();
            t.Nodes.Add(tC);
            TreeNode tS = new TreeNode("Server");
            foreach (BF2FSEntry entry in serverFS)
            {
                string ext = Path.GetExtension(entry.inFSPath).Trim().ToLower();
                foreach (string ending in endings)
                    if (ending == ext)
                    {
                        AddPath(tS, entry.inFSPath.Split('\\'));
                        break;
                    }
            }
            tS.Expand();
            t.Nodes.Add(tS);
            t.Expand();
            return t;
        }

        private static void AddPath(TreeNode root, string[] pathParts)
        {
            if (pathParts.Length > 1)
            {
                List<string> tmp = new List<string>();
                for (int i = 1; i < pathParts.Length; i++)
                    tmp.Add(pathParts[i]);
                bool found = false;
                foreach (TreeNode t in root.Nodes)
                    if (t.Text == pathParts[0])
                    {
                        found = true;
                        AddPath(t, tmp.ToArray());
                        break;
                    }
                if (!found)
                {
                    TreeNode t = new TreeNode(pathParts[0]);
                    AddPath(t, tmp.ToArray());
                    root.Nodes.Add(t);
                }
            }
            else
            {
                bool found = false;
                foreach (TreeNode t in root.Nodes)
                    if (t.Text == pathParts[0])
                    {
                        found = true;
                        break;
                    }
                if (!found)
                {
                    TreeNode t = new TreeNode(pathParts[0]);
                    root.Nodes.Add(t);
                }
            }
        }

        private static BF2FSEntry FindEntryFromNode(TreeNode t)
        {
            string path = GetPathFromNode(t);
            BF2FSEntry entry = null;
            if (path.StartsWith("\\File System\\Client\\"))
            {
                path = path.Substring(20);
                foreach (BF2FSEntry e in clientFS)
                    if (e.inFSPath == path)
                    {
                        entry = e;
                        break;
                    }
            }
            if (path.StartsWith("\\File System\\Server\\"))
            {
                path = path.Substring(20);
                foreach (BF2FSEntry e in serverFS)
                    if (e.inFSPath == path)
                    {
                        entry = e;
                        break;
                    }
            }
            return entry;
        }

        public static BF2FSEntry FindEntryFromIngamePath(string path)
        {
            return FindFirstEntry(path);
        }

        public static BF2FSEntry FindFirstEntry(string path)
        {
            path = path.ToLower();
            BF2FSEntry entry = null;
            foreach (BF2FSEntry e in clientFS)
                if (e.inFSPath.ToLower().Contains(path))
                {
                    entry = e;
                    break;
                }
            if (entry == null)
                foreach (BF2FSEntry e in serverFS)
                    if (e.inFSPath.ToLower().Contains(path))
                    {
                        entry = e;
                        break;
                    }
            return entry;
        }

        public static void SetFileFromNode(TreeNode t, byte[] datanew)
        {
            SetFileFromEntry(FindEntryFromNode(t), datanew);
        }

        public static void SetFileFromEntry(BF2FSEntry entry, byte[] datanew)
        {
            if (entry != null)
                SetFileFromZip(entry.zipFile, entry.inZipPath, datanew);
        }

        public static byte[] GetFileFromNode(TreeNode t)
        {
            return GetFileFromEntry(FindEntryFromNode(t));
        }

        public static byte[] GetFileFromEntry(BF2FSEntry entry)
        {
            if (entry != null)
                return GetFileFromZip(entry.zipFile, entry.inZipPath);
            return null;
        }

        public static byte[] GetFileFromZip(string zipFile, string inZipName)
        {
            string tmpname = "temp.bin";
            if (File.Exists(tmpname))
                File.Delete(tmpname);
            ZipArchive zip = ZipFile.OpenRead(zipFile);
            byte[] result = null;
            foreach (ZipArchiveEntry entry in zip.Entries)
                if (entry.FullName == inZipName)
                {
                    entry.ExtractToFile(tmpname);
                    if (File.Exists(tmpname))
                    {
                        result = File.ReadAllBytes(tmpname);
                        File.Delete(tmpname);
                    }
                }
            zip.Dispose();
            return result;
        }

        public static void SetFileFromZip(string zipFile, string inZipName, byte[] datanew)
        {
            string tmpname = "temp.bin";
            File.WriteAllBytes(tmpname, datanew);
            ZipArchive zip = ZipFile.Open(zipFile, ZipArchiveMode.Update);
            foreach (ZipArchiveEntry entry in zip.Entries)
                if (entry.FullName == inZipName)
                {
                    entry.Delete();
                    break;
                }
            zip.CreateEntryFromFile(tmpname, inZipName, CompressionLevel.Optimal);
            zip.Dispose();
            File.Delete(tmpname);
        }

        public static string GetPathFromNode(TreeNode t)
        {
            string result = "";
            while (t != null)
            {
                result = "\\" + t.Text + result;
                t = t.Parent;
            }
            return result;
        }
    }
}
