using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Mathematics.Interop;

namespace BFP4FExplorerWV
{
    public static class ExporterObj
    {
        public static void Export(BF2StaticMesh mesh, string filename, int geoMatIdx)
        {
            StringBuilder sb = new StringBuilder();
            Helper.BF2MeshSTMGeometryMaterial lod0 = mesh.geomat[geoMatIdx];
            int vertexcounter = 1;
            for (int i = 0; i < lod0.numMaterials; i++)
            {
                Helper.BF2MeshSTMMaterial mat = lod0.materials[i];
                List<RenderObject.VertexTextured> list = new List<RenderObject.VertexTextured>();
                int m = mesh.geometry.vertices.Count / (int)mesh.geometry.numVertices;
                for (int j = 0; j < mat.numIndicies; j++)
                {
                    int pos = (mesh.geometry.indices[(int)mat.indiciesStartIndex + j] + (int)mat.vertexStartIndex) * m;
                    list.Add(mesh.GetVertex(pos));
                }
                if (mat.numIndicies != 0)
                {
                    WriteObject(sb, list, "Material" + i, vertexcounter);
                    vertexcounter += list.Count();
                }
            }
            File.WriteAllText(filename, sb.ToString());
        }

        public static void Export(BF2BundledMesh mesh, string filename, int geoMatIdx)
        {
            StringBuilder sb = new StringBuilder();
            Helper.BF2MeshBMGeometryMaterial lod0 = mesh.geomat[geoMatIdx];
            int vertexcounter = 1;
            for (int i = 0; i < lod0.numMaterials; i++)
            {
                Helper.BF2MeshBMMaterial mat = lod0.materials[i];
                List<RenderObject.VertexTextured> list = new List<RenderObject.VertexTextured>();
                int m = mesh.geometry.vertices.Count / (int)mesh.geometry.numVertices;
                for (int j = 0; j < mat.numIndicies; j++)
                {
                    int pos = (mesh.geometry.indices[(int)mat.indiciesStartIndex + j] + (int)mat.vertexStartIndex) * m;
                    list.Add(mesh.GetVertex(pos));
                }
                if (mat.numIndicies != 0)
                {
                    WriteObject(sb, list, "Material" + i, vertexcounter);
                    vertexcounter += list.Count();
                }
            }
            File.WriteAllText(filename, sb.ToString());
        }

        public static void Export(BF2SkinnedMesh mesh, string filename, int geoMatIdx)
        {
            StringBuilder sb = new StringBuilder();
            Helper.BF2MeshSKMGeometryMaterial lod0 = mesh.geomat[geoMatIdx];
            int vertexcounter = 1;
            for (int i = 0; i < lod0.numMaterials; i++)
            {
                Helper.BF2MeshSKMMaterial mat = lod0.materials[i];
                List<RenderObject.VertexTextured> list = new List<RenderObject.VertexTextured>();
                int m = mesh.geometry.vertices.Count / (int)mesh.geometry.numVertices;
                for (int j = 0; j < mat.numIndicies; j++)
                {
                    int pos = (mesh.geometry.indices[(int)mat.indiciesStartIndex + j] + (int)mat.vertexStartIndex) * m;
                    list.Add(mesh.GetVertex(pos));
                }
                if (mat.numIndicies != 0)
                {
                    WriteObject(sb, list, "Material" + i, vertexcounter);
                    vertexcounter += list.Count();
                }
            }
            File.WriteAllText(filename, sb.ToString());
        }

        public static void Export(BF2CollisionMesh mesh, string filename)
        {
            StringBuilder sb = new StringBuilder();
            int count = 1;
            int vertexcounter = 1;
            foreach(Helper.BF2CollisionMeshGeometry geom in mesh.geometry)
                foreach(Helper.BF2CollisionMeshSubGeometry subgeo in geom.subGeom)
                    foreach (Helper.BF2CollisionMeshColData col in subgeo.colData)
                    {
                        List<RawVector3> list = new List<RawVector3>();
                        for (int i = 0; i < col.numFaces; i++)
                        {
                            ushort[] face = col.faces[i];
                            list.Add(col.vertices[face[0]].ToRawVec3());
                            list.Add(col.vertices[face[1]].ToRawVec3());
                            list.Add(col.vertices[face[2]].ToRawVec3());
                        }
                        WriteObject(sb, list, "Geometry" + (count++), vertexcounter);
                        vertexcounter += list.Count;
                    }
            File.WriteAllText(filename, sb.ToString());
        }

        private static void WriteObject(StringBuilder sb, List<RenderObject.VertexTextured> list, string name, int vertexCounter)
        {
            sb.AppendLine("o " + name);
            foreach (RenderObject.VertexTextured v in list)
            {
                Vector4 p = v.Position;
                Vector2 uv = v.TextureUV;
                sb.AppendLine("v " + PF(p.X) + PF(p.Y) + PF(p.Z));
                sb.AppendLine("vt " + PF(uv.X) + PF(uv.Y));
            }
            for (int j = 0; j < list.Count; j += 3)
            {
                sb.Append("f ");
                int n = vertexCounter + j;
                for (int k = 0; k < 3; k++)
                    sb.Append((n + k) + "/" + (n + k) + " ");
                sb.AppendLine();
            }
        }

        private static void WriteObject(StringBuilder sb, List<RawVector3> list, string name, int vertexCounter)
        {
            sb.AppendLine("o " + name);
            foreach (RawVector3 v in list)
                sb.AppendLine("v " + PF(v.X) + PF(v.Y) + PF(v.Z));
            for (int j = 0; j < list.Count; j += 3)
            {
                sb.Append("f ");
                int n = vertexCounter + j;
                for (int k = 0; k < 3; k++)
                    sb.Append((n + k) + " ");
                sb.AppendLine();
            }
        }

        private static string PF(float f)
        {
            return f.ToString().Replace(",", ".") + " ";
        }
    }
}
