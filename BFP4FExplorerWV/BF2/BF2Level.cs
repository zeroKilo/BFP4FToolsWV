using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D11;

using Device = SharpDX.Direct3D11.Device;

namespace BFP4FExplorerWV
{
    public static class BF2Level
    {
        public static List<BF2LevelObject> objects = new List<BF2LevelObject>();
        public static Engine3D engine;
        public static void Load()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            objects = new List<BF2LevelObject>();
            BF2FileSystem.BF2FSEntry e = BF2FileSystem.FindEntryFromIngamePath("level\\StaticObjects.con");
            if (e == null)
                return;
            byte[] data = BF2FileSystem.GetFileFromZip(e.zipFile, e.inZipPath);
            if (data == null)
                return;
            engine.ClearScene();
            foreach (BF2LevelObject lo in objects)
                lo.Free();
            objects.Clear();
            engine.textureManager.ClearCache();
            GC.Collect();
            LoadStaticObjects(data);
            Log.WriteLine("[BF2 LL] Loaded level in " + sw.ElapsedMilliseconds + "ms");
        }

        public static List<string> MakeList()
        {
            List<string> result = new List<string>();
            int count = 0;
            foreach (BF2LevelObject lo in objects)
                result.Add((count++).ToString("D4") + " : " + lo._template);
            return result;
        }

        public static void SelectIndex(int idx)
        {
            for (int i = 0; i < objects.Count; i++)
                objects[i].SetSelected(i == idx);
        }

        
        public static int Process3DClick(int x, int y)
        {
            Ray ray = engine.UnprojectClick(x, y);
            int idx = -1;
            float minDist = 100000;
            for (int i = 0; i < objects.Count; i++)
            {
                BF2LevelObject lo = objects[i];
                BoundingSphere s = lo.CalcBoundingSphere();
                s.Center = lo.position;
                float dist = 0;
                if (Collision.RayIntersectsSphere(ref ray, ref s, out dist))
                {
                    if (lo.CheckRayHit(ray, out dist))
                    {
                        if (dist < minDist)
                        {
                            minDist = dist;
                            idx = i;
                        }
                    }
                }
            }
            return idx;
        }

        public static void Save()
        {
            BF2FileSystem.BF2FSEntry e = BF2FileSystem.FindEntryFromIngamePath("level\\StaticObjects.con");
            if (e == null)
                return;
            byte[] data = BF2FileSystem.GetFileFromZip(e.zipFile, e.inZipPath);
            if (data == null)
                return;
            StringBuilder sb = new StringBuilder();
            foreach (BF2LevelObject lo in objects)
            {
                foreach (string line in lo.properties)
                {
                    if (line.StartsWith("Object.absolutePosition"))
                    {
                        string s = "Object.absolutePosition ";
                        s += lo.position.X.ToString().Replace(',', '.') + "/";
                        s += lo.position.Y.ToString().Replace(',', '.') + "/";
                        s += lo.position.Z.ToString().Replace(',', '.');
                        sb.AppendLine(s);
                    }
                    else if (line.StartsWith("Object.absolutePosition"))
                    {
                        string s = "Object.rotation ";
                        s += lo.rotation.X.ToString().Replace(',', '.') + "/";
                        s += lo.rotation.Y.ToString().Replace(',', '.') + "/";
                        s += lo.rotation.Z.ToString().Replace(',', '.');
                        sb.AppendLine(s);
                    }
                    else
                        sb.AppendLine(line);
                }
                sb.AppendLine();
            }
            sb.AppendLine();
            byte[] dataNew = Encoding.ASCII.GetBytes(sb.ToString());
            BF2FileSystem.SetFileFromEntry(e, dataNew);
        }

        private static void LoadStaticObjects(byte[] data)
        {
            string[] lines = Encoding.ASCII.GetString(data).Split('\n');
            int pos = 0;
            while(pos < lines.Length)
            {
                Log.SetProgress(0, lines.Length, pos);
                List<string> objectInfos = new List<string>();
                while (lines[pos].Trim() != "")
                    objectInfos.Add(lines[pos++].Trim());
                LoadObject(objectInfos);
                pos++;
            }
            Vector3 center = Vector3.Zero;
            foreach (BF2LevelObject lo in objects)
                center += lo.position;
            center /= objects.Count();
            engine.CamPos = center;
            Log.SetProgress(0, lines.Length, 0);
        }

        private static void LoadObject(List<string> infos)
        {
            string templateName = Helper.FindLineStartingWith(infos, "Object.create");
            if (templateName == null) return;
            string position = Helper.FindLineStartingWith(infos, "Object.absolutePosition");
            string rotation = Helper.FindLineStartingWith(infos, "Object.rotation");
            Vector3 pos = Vector3.Zero;
            Vector3 rot = Vector3.Zero;
            if (position != null) pos = Helper.ParseVector3(position.Split(' ')[1]);
            if (rotation != null) rot = Helper.ParseVector3(rotation.Split(' ')[1]);
            templateName = templateName.Split(' ')[1] + ".con";
            BF2LevelObject lo = null;
            bool foundCached = false;
            foreach(BF2LevelObject obj in objects)
                if (obj._template == templateName)
                {
                    lo = new BF2LevelObject(pos, rot, obj.type);
                    lo._template = templateName;
                    lo._data = obj._data.ToArray();
                    lo.properties = infos;
                    switch (obj.type)
                    {
                        case BF2LevelObject.BF2LOTYPE.StaticMesh:
                            BF2StaticMesh stm = new BF2StaticMesh(lo._data);
                            if (stm == null) return;
                            lo.stm = stm.ConvertForEngine(engine, true);
                            foreach (RenderObject ro in lo.stm)
                                ro.transform = lo.transform;
                            lo._valid = true;
                            foundCached = true;
                            break;
                    }
                    break;
                }
            if (!foundCached)
            {
                BF2FileSystem.BF2FSEntry entry = BF2FileSystem.FindFirstEntry(templateName);
                byte[] data = BF2FileSystem.GetFileFromEntry(entry);
                if (data == null)
                    return;
                List<string> infosObject = new List<string>(Encoding.ASCII.GetString(data).Split('\n'));
                string geoTemplate = Helper.FindLineStartingWith(infosObject, "GeometryTemplate.create");
                if (geoTemplate == null) return;
                string[] parts = geoTemplate.Split(' ');
                switch (parts[1].ToLower())
                {
                    case "staticmesh":
                        lo = new BF2LevelObject(pos, rot, BF2LevelObject.BF2LOTYPE.StaticMesh);
                        lo._template = templateName;
                        lo.properties = infos;
                        BF2StaticMesh stm = LoadStaticMesh(infosObject, lo);
                        if (stm == null) return;
                        lo.stm = stm.ConvertForEngine(engine, true);
                        foreach (RenderObject ro in lo.stm)
                            ro.transform = lo.transform;
                        lo._valid = true;
                        break;
                }
            }
            if (lo != null && lo._valid)
                objects.Add(lo);
        }

        private static BF2StaticMesh LoadStaticMesh(List<string> infos, BF2LevelObject lo)
        {
            string geoTemplate = Helper.FindLineStartingWith(infos, "GeometryTemplate.create");
            if (geoTemplate == null) return null;
            string[] parts = geoTemplate.Split(' ');
            string templateName = parts[2].Trim() + ".staticmesh";
            BF2FileSystem.BF2FSEntry entry = BF2FileSystem.FindFirstEntry(templateName);
            byte[] data = BF2FileSystem.GetFileFromEntry(entry);
            if (data == null)
                return null;
            lo._data = data;
            return new BF2StaticMesh(data);
        }
    }   
}
