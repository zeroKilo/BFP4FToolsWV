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
    public class BF2Terrain
    {
        public uint version;
        public Vector3 primaryWorldScale;
        public Vector3 secondaryWorldScale;
        public float u1;
        public float u2;
        public float u3;
        public uint patchSize;
        public bool subdividePatches;
        public uint numPatches;
        public uint patchColormapSize;
        public uint lowDetailmapSize;
        public string colorMapbaseName; 
        public string detailMapbaseName;
        public string lowDetailMapbaseName;
        public string lightmapBaseName;
        public Vector2 farSideTiling;
        public float farSideTilingHi;
        public float farSideTilingLow;
        public float farYOffset;
        public Vector3 terrainSunColor;
        public Vector3 terrainSkyColor;
        public Vector3 terrainWaterColor;
        public uint numLayers;
        public List<BF2TerrainLayer> layers;
        public uint[] u4;
        public uint[] u5;
        public uint numLodIndices;
        public ushort[] lodIndices;
        public bool vertexFormat;
        public List<BF2TerrainQuadPatch> quads;

        public RenderObject ro;

        public BF2Terrain(byte[] data)
        {
            MemoryStream m = new MemoryStream(data);
            version = Helper.ReadU32(m);
            primaryWorldScale = Helper.ReadVector3(m);
            secondaryWorldScale = Helper.ReadVector3(m);
            u1 = Helper.ReadFloat(m);
            u2 = Helper.ReadFloat(m);
            u3 = Helper.ReadFloat(m);
            patchSize = Helper.ReadU32(m);
            subdividePatches = m.ReadByte() == 0x1;
            numPatches = Helper.ReadU32(m);
            patchColormapSize = Helper.ReadU32(m);
            lowDetailmapSize = Helper.ReadU32(m);
            colorMapbaseName = Helper.ReadTString(m);
            detailMapbaseName = Helper.ReadTString(m);
            lowDetailMapbaseName = Helper.ReadTString(m);
            lightmapBaseName = Helper.ReadTString(m);
            farSideTiling = Helper.ReadVector2(m);
            farSideTilingHi = Helper.ReadFloat(m);
            farSideTilingLow = Helper.ReadFloat(m);
            farYOffset = Helper.ReadFloat(m);
            terrainSunColor = Helper.ReadVector3(m);
            terrainSkyColor = Helper.ReadVector3(m);
            terrainWaterColor = Helper.ReadVector3(m);
            numLayers = Helper.ReadU32(m);
            layers = new List<BF2TerrainLayer>();
            for (int i = 0; i < numLayers; i++)
                layers.Add(new BF2TerrainLayer(m));
            u4 = new uint[798];
            for (int i = 0; i < 798; i++)
                u4[i] = Helper.ReadU32(m);
            u5 = new uint[384];
            for (int i = 0; i < 384; i++)
                u5[i] = Helper.ReadU32(m);
            numLodIndices = Helper.ReadU32(m);
            lodIndices = new ushort[numLodIndices];
            for (int i = 0; i < numLodIndices; i++)
                lodIndices[i] = Helper.ReadU16(m);
            vertexFormat = m.ReadByte() == 0x1;
            quads = new List<BF2TerrainQuadPatch>();
            uint count = numPatches * numPatches;
            for (uint i = 0; i < count; i++)
                quads.Add(new BF2TerrainQuadPatch(m, patchSize, primaryWorldScale.Y));   
        }

        public void ConvertForEngine(Engine3D engine)
        {
            ro = new RenderObject(engine.device, RenderObject.RenderType.TriListWired, null, engine);
            List<RenderObject.VertexWired> result = new List<RenderObject.VertexWired>();
            foreach (BF2TerrainQuadPatch quad in quads)
            {
                List<RenderObject.VertexWired> list = new List<RenderObject.VertexWired>();
                Vector3 p = quad.position - new Vector3(patchSize, 0, patchSize);
                p.Y = 0;
                float tx, tz, dx = primaryWorldScale.X, dz = primaryWorldScale.Z;
                for (uint j = 0; j < patchSize; j++)
                    for (uint i = 0; i < patchSize; i++)
                    {
                        tx = i * dx;
                        tz = j * dz;
                        list.Add(new RenderObject.VertexWired(p + new Vector3(tx, quad.GetHeight(i, j), tz), Color4.Black));
                        list.Add(new RenderObject.VertexWired(p + new Vector3(tx + dx, quad.GetHeight(i + 1, j), tz), Color4.Black));
                        list.Add(new RenderObject.VertexWired(p + new Vector3(tx, quad.GetHeight(i, j + 1), tz + dz), Color4.Black));
                        list.Add(new RenderObject.VertexWired(p + new Vector3(tx, quad.GetHeight(i, j + 1), tz + dz), Color4.Black));
                        list.Add(new RenderObject.VertexWired(p + new Vector3(tx + dx, quad.GetHeight(i + 1, j), tz), Color4.Black));
                        list.Add(new RenderObject.VertexWired(p + new Vector3(tx + dx, quad.GetHeight(i + 1, j + 1), tz + dz), Color4.Black));
                    }
                for (int i = 0; i < list.Count; i++)
                {
                    RenderObject.VertexWired v = list[i];
                    v.Position.Y *= primaryWorldScale.Y;
                    list[i] = v;
                }
                result.AddRange(list);               
            }
            ro.verticesWired = result.ToArray();
            GC.Collect();
            ro.InitGeometry();
        }

        public class BF2TerrainLayer
        {
            public string detailTexturePath;
            public byte planeMap;
            public Vector2 sideTiling;
            public float topTiling;
            public float yOffset;
            public bool envMap;
            
            public BF2TerrainLayer(Stream s)
            {
                detailTexturePath = Helper.ReadTString(s);
                planeMap = (byte)s.ReadByte();
                sideTiling = Helper.ReadVector2(s);
                topTiling = Helper.ReadFloat(s);
                yOffset = Helper.ReadFloat(s);
                envMap = s.ReadByte() == 0x1;
            }
        }

        public class BF2TerrainQuadPatch
        {
            public int x;
            public int y;
            public uint u3;
            public uint u4;
            public Vector3 position;
            public uint u6;
            public BF2TerrainCompactVertexData[,] compactVertexData;
            public List<BF2TerrainDetailChart> detailCharts;
            public float scaleY;

            public float GetHeight(uint i, uint j)
            {
                return compactVertexData[i, j].pos1;
            }

            public BF2TerrainQuadPatch(Stream s, uint patchSize, float sY)
            {
                scaleY = sY;
                x = Helper.ReadS32(s);
                y = Helper.ReadS32(s);
                u3 = Helper.ReadU32(s);
                u4 = Helper.ReadU32(s);
                position = Helper.ReadVector3(s);
                u6 = Helper.ReadU32(s);
                compactVertexData = new BF2TerrainCompactVertexData[patchSize + 1, patchSize + 1];
                for (uint j = 0; j < patchSize + 1; j++)
                    for (uint i = 0; i < patchSize + 1; i++)
                        compactVertexData[i, j] = new BF2TerrainCompactVertexData(s);
                detailCharts = new List<BF2TerrainDetailChart>();
                while (true)
                {
                    int test = Helper.ReadS32(s);
                    if (test == -1)
                        break;
                    s.Seek(-4, SeekOrigin.Current);
                    detailCharts.Add(new BF2TerrainDetailChart(s));
                }
            }
        }

        public class BF2TerrainCompactVertexData
        {
            public Color4 pos0;
            public float pos1;
            public Vector3 morphData;
            public Color4 normal;
            public BF2TerrainCompactVertexData(Stream s)
            {
                pos0 = new Color4(Helper.ReadU32(s));
                pos1 = Helper.ReadFloat(s);
                morphData = Helper.ReadVector3(s);
                normal = new Color4(Helper.ReadU32(s));
            }
        }

        public class BF2TerrainDetailChart
        {
            public uint magic;
            public uint index;
            public Vector3 zero1;
            public float zero2;
            public uint numChartIndicies;
            public uint numChartFaces;
            public uint u1;
            public uint numSubIndexQuads;
            public List<uint[]> SubIndexQuad;
            public ushort[] chartIndicies;

            public BF2TerrainDetailChart(Stream s)
            {
                magic = Helper.ReadU32(s);
                index = Helper.ReadU32(s);
                zero1 = Helper.ReadVector3(s);
                zero2 = Helper.ReadFloat(s);
                numChartIndicies = Helper.ReadU32(s);
                numChartFaces = Helper.ReadU32(s);
                u1 = Helper.ReadU32(s);
                numSubIndexQuads = Helper.ReadU32(s);
                SubIndexQuad = new List<uint[]>();
                for (int i = 0; i < numSubIndexQuads; i++)
                {
                    uint[] tmp = new uint[6];
                    for (int j = 0; j < 6; j++)
                        tmp[j] = Helper.ReadU32(s);
                    SubIndexQuad.Add(tmp);
                }
                chartIndicies = new ushort[numChartIndicies];
                for (int i = 0; i < numChartIndicies; i++)
                    chartIndicies[i] = Helper.ReadU16(s);
            }
        }
    }
}
