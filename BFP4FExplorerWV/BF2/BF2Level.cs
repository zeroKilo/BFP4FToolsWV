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
        public static BF2Terrain terrain;
        public static string name = "";

        public static void Load()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            objects = new List<BF2LevelObject>();            
            engine.ClearScene();
            foreach (BF2LevelObject lo in objects)
            {
                lo.Free();
                GC.Collect();
            }
            objects.Clear();
            engine.textureManager.ClearCache();
            LoadTerrain();
            LoadRoadObjects();
            LoadStaticObjects();
            Log.WriteLine("[BF2 LL] Loaded level in " + sw.ElapsedMilliseconds + "ms");
        }

        public static List<string> MakeList()
        {
            List<string> result = new List<string>();
            int count = 0;
            foreach (BF2LevelObject lo in objects)
                result.Add((count++).ToString("D4") + " : " + lo._name);
            return result;
        }

        public static void SelectIndex(int idx)
        {
            for (int i = 0; i < objects.Count; i++)
                objects[i].SetSelected(i == idx);
        }

        public static void CloneEntry(int n)
        {
            BF2LevelObject lo = objects[n];
            BF2LevelObject nlo;
            switch (lo.type)
            {
                case BF2LevelObject.BF2LOTYPE.StaticObject:
                    nlo = new BF2LevelObject(lo.position, lo.rotation, BF2LevelObject.BF2LOTYPE.StaticObject);
                    nlo._data = lo._data.ToArray();
                    nlo._template = lo._template;
                    nlo._name = lo._name;
                    nlo.properties = new List<string>(lo.properties.ToArray());
                    BF2StaticMesh stm = new BF2StaticMesh(nlo._data);
                    if (stm == null) return;
                    nlo.staticMeshes = stm.ConvertForEngine(engine, true, 0);
                    foreach (RenderObject ro in nlo.staticMeshes)
                        nlo.transform = lo.transform;
                    nlo._valid = true;
                    objects.Add(nlo);
                    break;
                case BF2LevelObject.BF2LOTYPE.Road:
                    nlo = new BF2LevelObject(lo.position, lo.rotation, BF2LevelObject.BF2LOTYPE.Road);
                    nlo._data = lo._data.ToArray();
                    nlo._template = lo._template;
                    nlo._name = lo._name;
                    nlo.properties = new List<string>(lo.properties.ToArray());
                    BF2Mesh mesh = new BF2Mesh(nlo._data);
                    if (mesh == null) return;
                    Texture2D tex = FindRoadTexture(nlo._name);
                    nlo.meshes = mesh.ConvertForEngine(engine, tex);
                    foreach (RenderObject ro in nlo.meshes)
                        ro.transform = nlo.transform;
                    nlo._valid = true;
                    objects.Add(nlo);
                    break;
            }
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
            SaveStaticObjects();
            SaveRoadObjects();
        }

        public static void SaveStaticObjects()
        {
            BF2FileSystem.BF2FSEntry e = BF2FileSystem.FindEntryFromIngamePath("Levels\\" + name + "\\StaticObjects.con");
            if (e == null)
                return;
            byte[] data = BF2FileSystem.GetFileFromZip(e.zipFile, e.inZipPath);
            if (data == null)
                return;
            StringBuilder sb = new StringBuilder();
            foreach (BF2LevelObject lo in objects)
                if (lo.type == BF2LevelObject.BF2LOTYPE.StaticObject)
                {
                    bool foundPosition = false;
                    bool foundRotation = false;
                    foreach (string line in lo.properties)
                    {
                        if (line.StartsWith("Object.absolutePosition"))
                        {
                            string s = "Object.absolutePosition ";
                            s += lo.position.X.ToString().Replace(',', '.') + "/";
                            s += lo.position.Y.ToString().Replace(',', '.') + "/";
                            s += lo.position.Z.ToString().Replace(',', '.');
                            sb.AppendLine(s);
                            foundPosition = true;
                        }
                        else if (line.StartsWith("Object.absolutePosition"))
                        {
                            string s = "Object.rotation ";
                            s += lo.rotation.X.ToString().Replace(',', '.') + "/";
                            s += lo.rotation.Y.ToString().Replace(',', '.') + "/";
                            s += lo.rotation.Z.ToString().Replace(',', '.');
                            sb.AppendLine(s);
                            foundRotation = true;
                        }
                        else
                            sb.AppendLine(line);
                    }
                    if (!foundPosition)
                    {
                        string s = "Object.absolutePosition ";
                        s += lo.position.X.ToString().Replace(',', '.') + "/";
                        s += lo.position.Y.ToString().Replace(',', '.') + "/";
                        s += lo.position.Z.ToString().Replace(',', '.');
                        sb.AppendLine(s);
                    }
                    if (!foundRotation)
                    {
                        string s = "Object.rotation ";
                        s += lo.rotation.X.ToString().Replace(',', '.') + "/";
                        s += lo.rotation.Y.ToString().Replace(',', '.') + "/";
                        s += lo.rotation.Z.ToString().Replace(',', '.');
                        sb.AppendLine(s);
                    }
                    sb.AppendLine();
                }
            sb.AppendLine();
            byte[] dataNew = Encoding.ASCII.GetBytes(sb.ToString());
            BF2FileSystem.SetFileFromEntry(e, dataNew);
        }

        public static void SaveRoadObjects()
        {
            BF2FileSystem.BF2FSEntry e = BF2FileSystem.FindEntryFromIngamePath("Levels\\" + name + "\\CompiledRoads.con");
            if (e == null)
                return;
            byte[] data = BF2FileSystem.GetFileFromZip(e.zipFile, e.inZipPath);
            if (data == null)
                return;
            StringBuilder sb = new StringBuilder();
            foreach (BF2LevelObject lo in objects)
                if (lo.type == BF2LevelObject.BF2LOTYPE.Road)
                {
                    bool foundPosition = false;
                    foreach (string line in lo.properties)
                    {
                        if (line.StartsWith("object.absoluteposition"))
                        {
                            string s = "object.absoluteposition ";
                            s += lo.position.X.ToString().Replace(',', '.') + "/";
                            s += lo.position.Y.ToString().Replace(',', '.') + "/";
                            s += lo.position.Z.ToString().Replace(',', '.');
                            sb.AppendLine(s);
                            foundPosition = true;
                        }
                        else
                            sb.AppendLine(line);
                    }
                    if (!foundPosition)
                    {
                        string s = "object.absoluteposition ";
                        s += lo.position.X.ToString().Replace(',', '.') + "/";
                        s += lo.position.Y.ToString().Replace(',', '.') + "/";
                        s += lo.position.Z.ToString().Replace(',', '.');
                        sb.AppendLine(s);
                    }
                    sb.AppendLine();
                }
            sb.AppendLine();
            byte[] dataNew = Encoding.ASCII.GetBytes(sb.ToString());
            BF2FileSystem.SetFileFromEntry(e, dataNew);
        }

        private static void LoadTerrain()
        {
            Log.WriteLine("[BF2 LL] Loading terrain...");
            BF2FileSystem.BF2FSEntry e = BF2FileSystem.FindEntryFromIngamePath("Levels\\" + name + "\\terraindata.raw");
            if (e == null)
                return;
            byte[] data = BF2FileSystem.GetFileFromZip(e.zipFile, e.inZipPath);
            if (data == null)
                return;
            terrain = new BF2Terrain(data);
            terrain.ConvertForEngine(engine);
            engine.terrain = terrain.ro;
        }

        private static void LoadStaticObjects()
        {
            Log.WriteLine("[BF2 LL] Loading static objects...");
            BF2FileSystem.BF2FSEntry e = BF2FileSystem.FindEntryFromIngamePath("Levels\\" + name + "\\StaticObjects.con");
            if (e == null)
                return;
            byte[] data = BF2FileSystem.GetFileFromZip(e.zipFile, e.inZipPath);
            if (data == null)
                return;
            string[] lines = Encoding.ASCII.GetString(data).Split('\n');
            int pos = 0;
            int count = 0;
            while(pos < lines.Length)
            {
                Log.SetProgress(0, lines.Length, pos);
                List<string> objectInfos = new List<string>();
                while (lines[pos].Trim() != "")
                    objectInfos.Add(lines[pos++].Trim());
                LoadStaticObject(objectInfos);
                pos++;
                if (count++ > 10)
                {
                    count = 0;
                    GC.Collect();
                }
            }
            Vector3 center = Vector3.Zero;
            foreach (BF2LevelObject lo in objects)
                center += lo.position;
            center /= objects.Count();
            engine.CamPos = center;
            Log.SetProgress(0, lines.Length, 0);
        }

        private static void LoadRoadObjects()
        {
            Log.WriteLine("[BF2 LL] Loading roads...");
            BF2FileSystem.BF2FSEntry e = BF2FileSystem.FindEntryFromIngamePath("Levels\\" + name + "\\CompiledRoads.con");
            if (e == null)
                return;
            byte[] data = BF2FileSystem.GetFileFromZip(e.zipFile, e.inZipPath);
            if (data == null)
                return;
            string[] lines = Encoding.ASCII.GetString(data).Split('\n');
            int pos = 0;
            int count = 0;
            while (pos < lines.Length)
            {
                Log.SetProgress(0, lines.Length, pos);
                List<string> objectInfos = new List<string>();
                while (lines[pos].Trim() != "")
                    objectInfos.Add(lines[pos++].Trim());
                LoadRoadObject(objectInfos);
                pos++;
                if (count++ > 10)
                {
                    count = 0;
                    GC.Collect();
                }
            }
            Vector3 center = Vector3.Zero;
            foreach (BF2LevelObject lo in objects)
                center += lo.position;
            center /= objects.Count();
            engine.CamPos = center;
            Log.SetProgress(0, lines.Length, 0);
        }

        private static void LoadStaticObject(List<string> infos)
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
                if (obj._template == templateName && obj.type == BF2LevelObject.BF2LOTYPE.StaticObject)
                {
                    lo = new BF2LevelObject(pos, rot, obj.type);
                    lo._template = templateName;
                    lo._name = templateName;
                    lo._data = obj._data.ToArray();
                    lo.properties = infos;
                    switch (obj.type)
                    {
                        case BF2LevelObject.BF2LOTYPE.StaticObject:
                            BF2StaticMesh stm = new BF2StaticMesh(lo._data);
                            if (stm == null) return;
                            lo.staticMeshes = stm.ConvertForEngine(engine, true, 0);
                            foreach (RenderObject ro in lo.staticMeshes)
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
                        lo = new BF2LevelObject(pos, rot, BF2LevelObject.BF2LOTYPE.StaticObject);
                        lo._template = templateName;
                        lo._name = templateName;
                        lo.properties = infos;
                        BF2StaticMesh stm = LoadStaticMesh(infosObject, lo);
                        if (stm == null) return;
                        lo.staticMeshes = stm.ConvertForEngine(engine, true, 0);
                        foreach (RenderObject ro in lo.staticMeshes)
                            ro.transform = lo.transform;
                        lo._valid = true;
                        break;
                }
            }
            if (lo != null && lo._valid)
                objects.Add(lo);
        }

        private static void LoadRoadObject(List<string> infos)
        {
            string objectName = Helper.FindLineStartingWith(infos, "object.create");
            if(objectName == null) return;
            string meshName = Helper.FindLineStartingWith(infos, "object.geometry.loadMesh");
            if (meshName == null) return;
            string position = Helper.FindLineStartingWith(infos, "object.absolutePosition");
            Vector3 pos = Vector3.Zero;
            Vector3 rot = Vector3.Zero;
            if (position != null) pos = Helper.ParseVector3(position.Split(' ')[1]);
            objectName = objectName.Split(' ')[1];
            meshName = meshName.Split(' ')[1];
            BF2LevelObject lo = null;
            bool foundCached = false;
            foreach (BF2LevelObject obj in objects)
                if (obj._template == meshName && obj.type == BF2LevelObject.BF2LOTYPE.Road)
                {
                    lo = new BF2LevelObject(pos, rot, obj.type);
                    lo._template = meshName;
                    lo._name = objectName;
                    lo._data = obj._data.ToArray();
                    lo.properties = infos;
                    switch (obj.type)
                    {
                        case BF2LevelObject.BF2LOTYPE.Road:
                            BF2Mesh mesh = new BF2Mesh(lo._data);
                            if (mesh == null) return;
                            Texture2D tex = FindRoadTexture(lo._name);
                            lo.meshes = mesh.ConvertForEngine(engine, tex);
                            foreach (RenderObject ro in lo.meshes)
                                ro.transform = lo.transform;
                            lo._valid = true;
                            foundCached = true;
                            break;
                    }
                    break;
                }
            if (!foundCached)
            {
                lo = new BF2LevelObject(pos, rot, BF2LevelObject.BF2LOTYPE.Road);
                BF2FileSystem.BF2FSEntry entry = BF2FileSystem.FindFirstEntry(meshName);
                lo._data = BF2FileSystem.GetFileFromEntry(entry);
                if (lo._data == null)
                    return;
                lo._template = meshName;
                lo._name = objectName;
                lo.properties = infos;
                BF2Mesh mesh = new BF2Mesh(lo._data);
                if (mesh == null) return;
                Texture2D tex = FindRoadTexture(lo._name);
                lo.meshes = mesh.ConvertForEngine(engine, tex);
                foreach (RenderObject ro in lo.meshes)
                    ro.transform = lo.transform;
                lo._valid = true;
            }
            if (lo != null && lo._valid)
                objects.Add(lo);
        }

        private static Texture2D FindRoadTexture(string templateName)
        {
            Texture2D result = null;
            BF2FileSystem.BF2FSEntry e = BF2FileSystem.FindEntryFromIngamePath("objects\\roads\\Splines\\" + templateName + ".con");
            if (e == null)
                return result;
            byte[] data = BF2FileSystem.GetFileFromZip(e.zipFile, e.inZipPath);
            if (data == null)
                return result;
            List<string> lines = new List<string>(Encoding.ASCII.GetString(data).Split('\n'));
            string texName = Helper.FindLineStartingWith(lines, "RoadTemplateTexture.SetTextureFile");
            if (texName == null) return result;
            texName = texName.Split(' ')[1].Replace("\"", "").Trim() + ".dds";
            result = engine.textureManager.FindTextureByPath(texName);
            if(result == null)
                result = engine.defaultTexture;
            return result;
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
