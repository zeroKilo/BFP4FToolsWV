using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.Mathematics.Interop;

namespace BFP4FExplorerWV
{
    public static class Helper
    {
        public static void RunShell(string file, string command)
        {
            Process process = new System.Diagnostics.Process();
            ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = file;
            startInfo.Arguments = command;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = Path.GetDirectoryName(file) + "\\";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }

        public static void DDS2PNG(string filedds)
        {
            RunShell(Path.GetDirectoryName(Application.ExecutablePath) + "\\texconv.exe", "-ft png " + filedds);
        }

        public static ushort ReadU16(Stream s)
        {
            byte[] buff = new byte[2];
            s.Read(buff, 0, 2);
            return BitConverter.ToUInt16(buff, 0);
        }

        public static uint ReadU32(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            return BitConverter.ToUInt32(buff, 0);
        }

        public static ulong ReadU64(Stream s)
        {
            byte[] buff = new byte[8];
            s.Read(buff, 0, 8);
            return BitConverter.ToUInt64(buff, 0);
        }

        public static float ReadFloat(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            return BitConverter.ToSingle(buff, 0);
        }

        public static string ReadString(Stream s)
        {
            uint len = ReadU32(s);
            byte[] data = new byte[len];
            s.Read(data, 0, (int)len);
            return Encoding.ASCII.GetString(data);
        }

        public static TreeNode FindNext(TreeNode t, string text)
        {
            foreach (TreeNode t2 in t.Nodes)
            {
                if (t2.Text.ToLower().Contains(text))
                    return t2;
                if (t2.Nodes.Count != 0)
                {
                    TreeNode t3 = FindNext(t2, text);
                    if (t3 != null)
                        return t3;
                }
            }
            return null;
        }

        public static void SelectNext(string text, TreeView tree)
        {
            text = text.ToLower();
            TreeNode t = tree.SelectedNode;
            if (t == null && tree.Nodes.Count != 0)
                t = tree.Nodes[0];
            while (true)
            {
                TreeNode t2 = FindNext(t, text);
                if (t2 != null)
                {
                    tree.SelectedNode = t2;
                    return;
                }
                else if (t.NextNode != null)
                    t = t.NextNode;
                else if (t.Parent != null && t.Parent.NextNode != null)
                    t = t.Parent.NextNode;
                else if (t.Parent != null && t.Parent.NextNode == null)
                    while (t.Parent != null)
                    {
                        t = t.Parent;
                        if (t != null && t.NextNode != null)
                        {
                            t = t.NextNode;
                            break;
                        }
                    }
                else
                    return;
                if (t.Text.ToLower().Contains(text))
                {
                    tree.SelectedNode = t;
                    return;
                }
            }
        }


        public class BF2MeshHeader
        {
            public uint u1;
            public uint version;
            public uint u2;
            public uint u3;
            public uint u4;
            public char u5;
            public BF2MeshHeader(Stream s)
            {
                u1 = Helper.ReadU32(s);
                version = Helper.ReadU32(s);
                u2 = Helper.ReadU32(s);
                u3 = Helper.ReadU32(s);
                u4 = Helper.ReadU32(s);
                u5 = (char)s.ReadByte();
            }
        }

        public class BF2CollisionMeshHeader
        {
            public uint head;
            public uint version;
            public uint numChunks;
            public BF2CollisionMeshHeader(Stream s)
            {
                head = Helper.ReadU32(s);
                version = Helper.ReadU32(s);
                numChunks = Helper.ReadU32(s);
            }
        }

        public class BF2CollisionMeshGeometry
        {
            public uint numSubGeom;
            public List<BF2CollisionMeshSubGeometry> subGeom;
            public BF2CollisionMeshGeometry(Stream s, uint version)
            {
                numSubGeom = Helper.ReadU32(s);
                subGeom = new List<BF2CollisionMeshSubGeometry>();
                for (int i = 0; i < numSubGeom; i++)
                    subGeom.Add(new BF2CollisionMeshSubGeometry(s, version));
            }
        }

        public class BF2CollisionMeshSubGeometry
        {
            public uint numCols;
            public List<BF2CollisionMeshColData> colData;
            public BF2CollisionMeshSubGeometry(Stream s, uint version)
            {
                numCols = Helper.ReadU32(s);
                colData = new List<BF2CollisionMeshColData>();
                for (int i = 0; i < numCols; i++)
                    colData.Add(new BF2CollisionMeshColData(s, version));
            }
        }

        public class BF2CollisionMeshColData
        {
            public uint colType;
            public uint numFaces;
            public List<ushort[]> faces;
            public uint numVertices;
            public List<BF2MeshVector3> vertices;
            public ushort[] u1;
            public BF2MeshVector3 boundsMin1;
            public BF2MeshVector3 boundsMax1;
            public byte u2;
            public BF2MeshVector3 boundsMin2;
            public BF2MeshVector3 boundsMax2;
            public uint numY;
            public List<uint[]> dataY;
            public uint numZ;
            public ushort[] dataZ;
            public uint numA;
            public uint[] dataA;
            public BF2CollisionMeshColData(Stream s, uint version)
            {
                if (version >= 9)
                    colType = Helper.ReadU32(s);
                numFaces = Helper.ReadU32(s);
                faces = new List<ushort[]>();
                for (int i = 0; i < numFaces; i++)
                {
                    ushort[] tmp = new ushort[4];
                    for (int j = 0; j < 4; j++)
                        tmp[j] = Helper.ReadU16(s);
                    faces.Add(tmp);
                }
                numVertices = Helper.ReadU32(s);
                vertices = new List<BF2MeshVector3>();
                for (int i = 0; i < numVertices; i++)
                    vertices.Add(new BF2MeshVector3(s));
                u1 = new ushort[numVertices];
                for (int i = 0; i < numVertices; i++)
                    u1[i] = Helper.ReadU16(s);
                boundsMin1 = new BF2MeshVector3(s);
                boundsMax1 = new BF2MeshVector3(s);
                u2 = (byte)s.ReadByte();
                boundsMin2 = new BF2MeshVector3(s);
                boundsMax2 = new BF2MeshVector3(s);
                numY = Helper.ReadU32(s);
                dataY = new List<uint[]>();
                for (int i = 0; i < numY; i++)
                {
                    uint[] tmp = new uint[4];
                    for (int j = 0; j < 4; j++)
                        tmp[j] = Helper.ReadU32(s);
                    dataY.Add(tmp);
                }
                numZ = Helper.ReadU32(s);
                dataZ = new ushort[numZ];
                for (int i = 0; i < numZ; i++)
                    dataZ[i] = Helper.ReadU16(s);
                if (version >= 10)
                {
                    numA = Helper.ReadU32(s);
                    dataA = new uint[numA];
                    for (int i = 0; i < numA; i++)
                        dataA[i] = Helper.ReadU32(s);
                }
            }
        }

        public class BF2MeshGeometry
        {
            public uint numGeom;
            public List<uint> numLods;
            public uint numVertexElements;
            public List<BF2MeshVertexElement> vertexElements;
            public uint vertexFormat;
            public uint vertexStride;
            public uint numVertices;
            public List<float> vertices;
            public uint numIndices;
            public List<ushort> indices;
            public BF2MeshGeometry(Stream s)
            {
                numGeom = Helper.ReadU32(s);
                numLods = new List<uint>();
                for (int i = 0; i < numGeom; i++)
                    numLods.Add(Helper.ReadU32(s));
                numVertexElements = Helper.ReadU32(s);
                vertexElements = new List<BF2MeshVertexElement>();
                for (int i = 0; i < numVertexElements; i++)
                    vertexElements.Add(new BF2MeshVertexElement(s));
                vertexFormat = Helper.ReadU32(s);
                vertexStride = Helper.ReadU32(s);
                numVertices = Helper.ReadU32(s);
                vertices = new List<float>();
                uint count = numVertices * (vertexStride / vertexFormat);
                for (int i = 0; i < count; i++)
                    vertices.Add(Helper.ReadFloat(s));
                numIndices = Helper.ReadU32(s);
                indices = new List<ushort>();
                for (int i = 0; i < numIndices; i++)
                    indices.Add(Helper.ReadU16(s));
            }

            public uint GetSumOfLODs()
            {
                uint result = 0;
                foreach (uint u in numLods)
                    result += u;
                return result;
            }
        }

        public class BF2MeshVertexElement
        {
            ushort flag;
            ushort offset;
            ushort varType;
            ushort usage;
            public BF2MeshVertexElement(Stream s)
            {
                flag = Helper.ReadU16(s);
                offset = Helper.ReadU16(s);
                varType = Helper.ReadU16(s);
                usage = Helper.ReadU16(s);
            }
        }

        public class BF2MeshSTMLod
        {
            public BF2MeshVector3 u1;
            public BF2MeshVector3 u2;
            public BF2MeshVector3 u3;
            public uint numNodes;
            public List<BF2MeshMatrix> nodes;
            public BF2MeshSTMLod(Stream s, BF2MeshHeader header)
            {
                u1 = new BF2MeshVector3(s);
                u2 = new BF2MeshVector3(s);
                if(header.version == 4)
                    u3 = new BF2MeshVector3(s);
                numNodes = Helper.ReadU32(s);
                nodes = new List<BF2MeshMatrix>();
                for (int i = 0; i < numNodes; i++)
                    nodes.Add(new BF2MeshMatrix(s));
            }
        }

        public class BF2MeshBMLod
        {
            public BF2MeshVector3 u1;
            public BF2MeshVector3 u2;
            public BF2MeshVector3 u3;
            public uint u4;
            public List<BoneEntry> u5;
            public class BoneEntry
            {
                public BF2MeshMatrix mat;
                public string name;
                public BoneEntry(BF2MeshMatrix m, string n)
                {
                    mat = m;
                    name = n;
                }
            }
            public BF2MeshBMLod(Stream s, BF2MeshHeader header)
            {
                if (header.version == 6)
                {
                    u1 = new BF2MeshVector3(s);
                    u2 = new BF2MeshVector3(s);
                    u3 = new BF2MeshVector3(s);
                    u4 = Helper.ReadU32(s);
                }
                if (header.version == 10)
                {
                    u1 = new BF2MeshVector3(s);
                    u2 = new BF2MeshVector3(s);
                    u4 = Helper.ReadU32(s);
                    if (header.u5 == 1)
                    {
                        u5 = new List<BoneEntry>();
                        for (int i = 0; i < u4; i++)
                            u5.Add(new BoneEntry(new BF2MeshMatrix(s), Helper.ReadString(s)));
                    }
                }
            }
        }

        public class BF2MeshSKMLod
        {
            public BF2MeshVector3 u1;
            public BF2MeshVector3 u2;
            public BF2MeshVector3 u3;
            public uint numRigs;
            public List<BF2MeshRig> rigs;
            public BF2MeshSKMLod(Stream s, uint version)
            {
                u1 = new BF2MeshVector3(s);
                u2 = new BF2MeshVector3(s);
                if(version == 4)
                    u3 = new BF2MeshVector3(s);
                numRigs = Helper.ReadU32(s);
                rigs = new List<BF2MeshRig>();
                for (int i = 0; i < numRigs; i++)
                    rigs.Add(new BF2MeshRig(s));
            }
        }

        public class BF2MeshSTMMaterial
        {
            public uint alphaMode;
            public string shaderFile;
            public string technique;
            public uint numTextureMaps;
            public List<string> textureMapFiles;
            public uint vertexStartIndex;
            public uint indiciesStartIndex;
            public uint numIndicies;
            public uint numVertices;
            public uint u1;
            public ushort u2;
            public ushort u3;
            public BF2MeshVector3 u4;
            public BF2MeshVector3 u5;
            public BF2MeshSTMMaterial(Stream s, BF2MeshHeader header)
            {
                alphaMode = Helper.ReadU32(s);
                shaderFile = Helper.ReadString(s);
                technique = Helper.ReadString(s);
                numTextureMaps = Helper.ReadU32(s);
                textureMapFiles = new List<string>();
                for (int i = 0; i < numTextureMaps; i++)
                    textureMapFiles.Add(Helper.ReadString(s));
                vertexStartIndex = Helper.ReadU32(s);
                indiciesStartIndex = Helper.ReadU32(s);
                numIndicies = Helper.ReadU32(s);
                numVertices = Helper.ReadU32(s);
                u1 = Helper.ReadU32(s);
                u2 = Helper.ReadU16(s);
                u3 = Helper.ReadU16(s);
                if (header.version == 0xB)
                {
                    u4 = new BF2MeshVector3(s);
                    u5 = new BF2MeshVector3(s);
                }
            }
        }

        public class BF2MeshBMMaterial
        {
            public uint alphaMode;
            public string shaderFile;
            public string technique;
            public uint numTextureMaps;
            public List<string> textureMapFiles;
            public uint vertexStartIndex;
            public uint indiciesStartIndex;
            public uint numIndicies;
            public uint numVertices;
            public uint u1;
            public ushort u2;
            public ushort u3;
            public BF2MeshBMMaterial(Stream s)
            {
                alphaMode = Helper.ReadU32(s);
                shaderFile = Helper.ReadString(s);
                technique = Helper.ReadString(s);
                numTextureMaps = Helper.ReadU32(s);
                textureMapFiles = new List<string>();
                for (int i = 0; i < numTextureMaps; i++)
                    textureMapFiles.Add(Helper.ReadString(s));
                vertexStartIndex = Helper.ReadU32(s);
                indiciesStartIndex = Helper.ReadU32(s);
                numIndicies = Helper.ReadU32(s);
                numVertices = Helper.ReadU32(s);
                u1 = Helper.ReadU32(s);
                u2 = Helper.ReadU16(s);
                u3 = Helper.ReadU16(s);
            }
        }

        public class BF2MeshSKMMaterial
        {
            public string shaderFile;
            public string technique;
            public uint numTextureMaps;
            public List<string> textureMapFiles;
            public uint vertexStartIndex;
            public uint indiciesStartIndex;
            public uint numIndicies;
            public uint numVertices;
            public uint u1;
            public ushort u2;
            public ushort u3;
            public BF2MeshSKMMaterial(Stream s)
            {
                shaderFile = Helper.ReadString(s);
                technique = Helper.ReadString(s);
                numTextureMaps = Helper.ReadU32(s);
                textureMapFiles = new List<string>();
                for (int i = 0; i < numTextureMaps; i++)
                    textureMapFiles.Add(Helper.ReadString(s));
                vertexStartIndex = Helper.ReadU32(s);
                indiciesStartIndex = Helper.ReadU32(s);
                numIndicies = Helper.ReadU32(s);
                numVertices = Helper.ReadU32(s);
                u1 = Helper.ReadU32(s);
                u2 = Helper.ReadU16(s);
                u3 = Helper.ReadU16(s);
            }
        }

        public class BF2MeshSTMGeometryMaterial
        {
            public uint numMaterials;
            public List<BF2MeshSTMMaterial> materials;
            public BF2MeshSTMGeometryMaterial(Stream s, BF2MeshHeader header)
            {
                numMaterials = Helper.ReadU32(s);
                materials = new List<BF2MeshSTMMaterial>();
                for (int i = 0; i < numMaterials; i++)
                    materials.Add(new BF2MeshSTMMaterial(s, header));
            }
        }

        public class BF2MeshBMGeometryMaterial
        {
            public uint numMaterials;
            public List<BF2MeshBMMaterial> materials;
            public BF2MeshBMGeometryMaterial(Stream s)
            {
                numMaterials = Helper.ReadU32(s);
                materials = new List<BF2MeshBMMaterial>();
                for (int i = 0; i < numMaterials; i++)
                    materials.Add(new BF2MeshBMMaterial(s));
            }
        }

        public class BF2MeshSKMGeometryMaterial
        {
            public uint numMaterials;
            public List<BF2MeshSKMMaterial> materials;
            public BF2MeshSKMGeometryMaterial(Stream s)
            {
                numMaterials = Helper.ReadU32(s);
                materials = new List<BF2MeshSKMMaterial>();
                for (int i = 0; i < numMaterials; i++)
                    materials.Add(new BF2MeshSKMMaterial(s));
            }
        }

        public class BF2MeshVector3
        {
            public float x, y, z;
            public BF2MeshVector3(Stream s)
            {
                x = Helper.ReadFloat(s);
                y = Helper.ReadFloat(s);
                z = Helper.ReadFloat(s);
            }

            public RawVector3 ToRawVec3()
            {
                return new RawVector3(x, y, z);
            }
        }

        public class BF2MeshMatrix
        {
            public float[,] m;
            public BF2MeshMatrix(Stream s)
            {
                m = new float[4, 4];
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                        m[i, j] = Helper.ReadFloat(s);
            }
        }

        public class BF2MeshRig
        {
            public uint numBones;
            public List<BF2MeshBone> bones;
            public BF2MeshRig(Stream s)
            {
                numBones = Helper.ReadU32(s);
                bones = new List<BF2MeshBone>();
                for (int i = 0; i < numBones; i++)
                    bones.Add(new BF2MeshBone(s));
            }
        }

        public class BF2MeshBone
        {
            public uint id;
            public BF2MeshMatrix mat;

            public BF2MeshBone(Stream s)
            {
                id = Helper.ReadU32(s);
                mat = new BF2MeshMatrix(s);
            }
        }
    }
}
