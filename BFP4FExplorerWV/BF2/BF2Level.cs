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

        public static TreeNode MakeTree()
        {
            TreeNode t = new TreeNode("Level");
            foreach (BF2LevelObject lo in objects)
                t.Nodes.Add(lo.MakeNode());
            t.Expand();
            return t;
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

    public class BF2LevelObject
    {
        public enum BF2LOTYPE
        {
            StaticMesh = 0,

        }
        public bool _valid = false;
        public string _template;
        public byte[] _data;
        public Vector3 position;
        public Vector3 rotation;
        public Matrix transform;
        public BF2LOTYPE type;
        public List<RenderObject> stm = null;
        public BF2LevelObject(Vector3 pos, Vector3 rot, BF2LOTYPE t)
        {
            position = pos;
            rotation = rot;
            type = t;
            float f = 3.1415f / 180f;
            transform = Matrix.RotationYawPitchRoll(rot.X * f, rot.Y * f, rot.Z * f) *
                        Matrix.Translation(pos);
        }

        public void Render(DeviceContext context, Matrix view, Matrix proj)
        {
            if (stm != null)
                foreach (RenderObject ro in stm)
                    ro.Render(context, view, proj);
        }

        public TreeNode MakeNode()
        {
            TreeNode t = new TreeNode(_template);
            t.Nodes.Add(new TreeNode("Position = " + position.ToString()));
            t.Nodes.Add(new TreeNode("Rotation= " + position.ToString()));
            return t;
        }

        public void Free()
        {
            stm.Clear();
            stm = null;
        }
    }
}
