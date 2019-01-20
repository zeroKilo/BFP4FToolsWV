using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace BFP4FExplorerWV
{
    public class BF2Mesh
    {
        public uint version;
        public Vector3 u1;
        public uint u2;
        public Vector3 u3;
        public Vector3 u4;
        public uint u5;
        public uint numVertices;
        public List<BF2MeshVertex> vertices;
        public uint numIndicies;
        public ushort[] indicies;
        public uint numUnknown;
        public List<BF2MeshUnknown> unk;
        public BF2Mesh(byte[] data)
        {
            MemoryStream m = new MemoryStream(data);
            version = Helper.ReadU32(m);
            u1 = Helper.ReadVector3(m);
            u2 = Helper.ReadU32(m);
            u3 = Helper.ReadVector3(m);
            u4 = Helper.ReadVector3(m);
            u5 = Helper.ReadU32(m);
            numVertices = Helper.ReadU32(m);
            vertices = new List<BF2MeshVertex>();
            for (int i = 0; i < numVertices; i++)
                vertices.Add(new BF2MeshVertex(m));
            numIndicies = Helper.ReadU32(m);
            indicies = new ushort[numIndicies];
            for (int i = 0; i < numIndicies; i++)
                indicies[i] = Helper.ReadU16(m);
            numUnknown = Helper.ReadU32(m);
            unk = new List<BF2MeshUnknown>();
            for (int i = 0; i < numUnknown; i++)
                unk.Add(new BF2MeshUnknown(m));
        }

        public List<RenderObject> ConvertForEngine(Engine3D engine, Texture2D texture)
        {
            List<RenderObject> result = new List<RenderObject>();
            List<RenderObject.VertexTextured> list = new List<RenderObject.VertexTextured>();
            List<RenderObject.VertexWired> list2 = new List<RenderObject.VertexWired>();
            for (int i = 0; i < numIndicies; i++)
            {
                list.Add(vertices[indicies[i]].ToTextured());
                list2.Add(vertices[indicies[i]].ToWired());               
            }
            RenderObject textured = new RenderObject(engine.device, RenderObject.RenderType.TriListTextured, texture, engine);
            RenderObject wired = new RenderObject(engine.device, RenderObject.RenderType.TriListWired, null, engine);
            textured.verticesTextured = list.ToArray();
            wired.verticesWired = list2.ToArray();
            textured.InitGeometry();
            wired.InitGeometry();
            result.Add(textured);
            result.Add(wired);
            return result;
        }

        public class BF2MeshVertex
        {
            public Vector3 pos;
            public Vector2 tex0;
            public Vector2 tex1;
            public uint alpha;
            public BF2MeshVertex(Stream s)
            {
                pos = Helper.ReadVector3(s);
                tex0 = Helper.ReadVector2(s);
                tex1 = Helper.ReadVector2(s);
                alpha = Helper.ReadU32(s);
            }

            public RenderObject.VertexTextured ToTextured()
            {
                return new RenderObject.VertexTextured(Helper.ToV4(pos), Color.White, tex0);
            }

            public RenderObject.VertexWired ToWired()
            {
                return new RenderObject.VertexWired(pos, Color4.Black);
            }
        }

        public class BF2MeshUnknown
        {
            public uint u1;
            public uint u2;
            public uint u3;
            public Vector3 u4;
            public BF2MeshUnknown(Stream s)
            {
                u1 = Helper.ReadU32(s);
                u2 = Helper.ReadU32(s);
                u3 = Helper.ReadU32(s);
                u4 = Helper.ReadVector3(s);
            }
        }
    }
}
