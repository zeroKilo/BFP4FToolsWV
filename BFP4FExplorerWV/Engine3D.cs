using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;

using Device = SharpDX.Direct3D11.Device;

namespace BFP4FExplorerWV
{
    public class Engine3D
    {
        public Device device;
        public DeviceContext context;
        public SwapChain swapChain;
        public Texture2D backBuffer;
        public RenderTargetView renderTargetView;
        public DepthStencilView depthStencilView;
        public DepthStencilState depthStencilState;
        public float CamRot = 3.1415f / 180f, CamDis = 3f;
        public List<RenderObject> objects;
        public PixelShader psWired, psTextured;

        public InputElement[] inputElementsWired = new InputElement[] 
        { 
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0) 
        };

        public InputElement[] inputElementsTextured = new InputElement[]
        {
            new InputElement("SV_Position", 0, Format.R32G32B32A32_Float, 0, 0),
            new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0),
            new InputElement("TEXCOORD", 0, Format.R32G32_Float, 32, 0),
        };
        public VertexShader vsWired, vsTextured;
        public ShaderSignature inputSignatureWired, inputSignatureTextured;
        public InputLayout inputLayoutWired, inputLayoutTextured;
        public SamplerState sampler;
        public SamplerStateDescription samplerStateDescription;
        private SharpDX.Direct3D11.Buffer worldViewProjectionBuffer;
        public Texture2D defaultTexture;
        public RasterizerState rasterStateWired, rasterStateTextured;
        private RawViewportF viewport;

        private RawMatrix world;
        private RawMatrix view;
        private RawMatrix proj;
        private RawVector3 camPos;

        public Engine3D(PictureBox f)
        {
            InitDevice(f);
            InitShaders();
            Resize(f);
            objects = new List<RenderObject>();
            RenderObject o = new RenderObject(device, RenderObject.RenderType.TriListTextured, defaultTexture, this);
            o.InitGeometry();
            objects.Add(o);
            o = new RenderObject(device, RenderObject.RenderType.TriListWired, defaultTexture, this);
            o.InitGeometry();
            objects.Add(o);
        }

        ~Engine3D()
        {
            Cleanup();
        }

        private void InitDevice(PictureBox f)
        {
            Device.CreateWithSwapChain(
                SharpDX.Direct3D.DriverType.Hardware,
                DeviceCreationFlags.None,
                new[] {
                    SharpDX.Direct3D.FeatureLevel.Level_11_1,
                    SharpDX.Direct3D.FeatureLevel.Level_11_0,
                    SharpDX.Direct3D.FeatureLevel.Level_10_1,
                    SharpDX.Direct3D.FeatureLevel.Level_10_0,
                },
                new SwapChainDescription()
                {
                    ModeDescription =
                        new ModeDescription(
                            f.ClientSize.Width,
                            f.ClientSize.Height,
                            new Rational(60, 1),
                            Format.R8G8B8A8_UNorm
                        ),
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = SharpDX.DXGI.Usage.BackBuffer | Usage.RenderTargetOutput,
                    BufferCount = 1,
                    Flags = SwapChainFlags.AllowModeSwitch,
                    IsWindowed = true,
                    OutputHandle = f.Handle,
                    SwapEffect = SwapEffect.Discard,                     
                },
                out device, out swapChain
            );
            context = device.ImmediateContext;    
        }

        private void InitShaders()
        {
            defaultTexture = TextureLoader.CreateTexture2DFromBitmap(device, TextureLoader.LoadBitmap(new ImagingFactory2(), "res\\default.png"));
            //Wireframe shaders
            ShaderBytecode vertexShaderByteCodeWired = ShaderBytecode.Compile(Properties.Resources.vertexShaderWired, "main", "vs_4_0", ShaderFlags.Debug);
            vsWired = new VertexShader(device, vertexShaderByteCodeWired);
            inputSignatureWired = ShaderSignature.GetInputSignature(vertexShaderByteCodeWired);
            inputLayoutWired = new InputLayout(device, inputSignatureWired, inputElementsWired);
            psWired = new PixelShader(device, ShaderBytecode.Compile(Properties.Resources.pixelShaderWired, "main", "ps_4_0", ShaderFlags.Debug));

            //Texture shaders
            ShaderBytecode vertexShaderByteCodeTextured = ShaderBytecode.Compile(Properties.Resources.vertexShaderTextured, "main", "vs_4_0", ShaderFlags.Debug);
            vsTextured = new VertexShader(device, vertexShaderByteCodeTextured);
            inputSignatureTextured = ShaderSignature.GetInputSignature(vertexShaderByteCodeTextured);
            inputLayoutTextured = new InputLayout(device, inputSignatureTextured, inputElementsTextured);
            psTextured = new PixelShader(device, ShaderBytecode.Compile(Properties.Resources.pixelShaderTextured, "main", "ps_4_0", ShaderFlags.Debug));

            RasterizerStateDescription renderStateDescWired = RasterizerStateDescription.Default();
            renderStateDescWired.FillMode = FillMode.Wireframe;
            renderStateDescWired.CullMode = CullMode.None;
            rasterStateWired = new RasterizerState(device, renderStateDescWired);
            RasterizerStateDescription renderStateDescTextured = RasterizerStateDescription.Default();
            renderStateDescTextured.IsFrontCounterClockwise = false;
            renderStateDescTextured.FillMode = FillMode.Solid;
            renderStateDescTextured.CullMode = CullMode.None;
            renderStateDescTextured.IsDepthClipEnabled = true;            
            rasterStateTextured = new RasterizerState(device, renderStateDescTextured);

            samplerStateDescription = new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.MinMagMipLinear
            };
            sampler = new SamplerState(device, samplerStateDescription);

            worldViewProjectionBuffer = new SharpDX.Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }

        public void Resize(PictureBox f)
        {
            if(renderTargetView != null) renderTargetView.Dispose();
            if(depthStencilView != null) depthStencilView.Dispose();
            if(backBuffer != null) backBuffer.Dispose();
            swapChain.ResizeBuffers(1, f.ClientSize.Width, f.ClientSize.Height, SharpDX.DXGI.Format.Unknown, SwapChainFlags.AllowModeSwitch);
            viewport = new RawViewportF();
            viewport.X = 0;
            viewport.Y = 0;
            viewport.Width = f.ClientSize.Width;
            viewport.Height = f.ClientSize.Height;
            viewport.MinDepth = 0;
            viewport.MaxDepth = 1;
            backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);            
            renderTargetView = new RenderTargetView(device, backBuffer);
            Texture2D depthBuffer = new Texture2D(device, new Texture2DDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = f.ClientSize.Width,
                Height = f.ClientSize.Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });
            depthStencilView = new DepthStencilView(device, depthBuffer);
            context.Rasterizer.SetViewport(viewport);
            context.OutputMerger.SetTargets(depthStencilView, renderTargetView);
            context.OutputMerger.SetRenderTargets(depthStencilView, renderTargetView);
            proj = Matrix.PerspectiveFovLH((float)Math.PI / 3f, f.ClientSize.Width / (float)f.ClientSize.Height, 0.5f, 100000f);
        }

        public void Render()
        {
            context.ClearRenderTargetView(renderTargetView, new RawColor4(0, 128, 255, 255));
            context.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
            camPos = new RawVector3((float)Math.Sin(CamRot) * CamDis, 0, (float)Math.Cos(CamRot) * CamDis);
            view = Matrix.LookAtLH(camPos, Vector3.Zero, Vector3.UnitY);
            world = Matrix.Multiply(view , proj);
            world = Matrix.Transpose(world);
            context.VertexShader.SetConstantBuffer(0, worldViewProjectionBuffer);
            context.UpdateSubresource(ref world, worldViewProjectionBuffer);
            foreach (RenderObject ro in objects)
                ro.Render(context);
            swapChain.Present(0, PresentFlags.None);
        }

        public void ClearScene()
        {
            foreach (RenderObject ro in objects)
                ro.Dispose();
            objects.Clear();
        }

        public void ResetCameraDistance()
        {
            float dis = 0.001f;
            foreach (RenderObject o in objects)
            {
                foreach (RawVector3 v in o.vertices)
                {
                    float l = (float)Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
                    if (l > dis)
                        dis = l;
                }
                foreach(RenderObject.Vertex v in o.verticesTextured)
                {
                    Vector4 p = v.Position;
                    float l = (float)Math.Sqrt(p.X* p.X + p.Y * p.Y + p.Z * p.Z);
                    if (l > dis)
                        dis = l;
                }
            }
            CamDis = dis * 2;
        }

        public void Cleanup()
        {
            renderTargetView.Dispose();
            backBuffer.Dispose();
            device.Dispose();
            swapChain.Dispose();
            if (objects != null)
                foreach (RenderObject ro in objects)
                    ro.Dispose();
            inputLayoutWired.Dispose();
            inputSignatureWired.Dispose();
        }

        public Texture2D FindTextureByPath(string path)
        {
            BF2FileSystem.BF2FSEntry e = BF2FileSystem.FindEntryFromIngamePath(path.Replace("/","\\"));
            if (e == null)
                return null;
            byte[] data = BF2FileSystem.GetFileFromZip(e.zipFile, e.inZipPath);
            if (data == null)
                return null;
            string ext = Path.GetExtension(e.inFSPath).ToLower();
            string tmpfile = "tmp" + ext;
            string tmpfile2 = "tmp.png";
            if (File.Exists(tmpfile2))
                File.Delete(tmpfile2);
            File.WriteAllBytes(tmpfile, data);
            Texture2D result = null;
            switch (ext)
            {
                case ".dds":
                    Helper.DDS2PNG("tmp.dds");
                    if (File.Exists(tmpfile2))
                    {
                        System.Drawing.Bitmap bmp = Helper.LoadBitmapUnlocked(tmpfile2);
                        result = TextureLoader.CreateTexture2DFromBitmap(device, TextureLoader.CreateWICBitmapFromGDI(bmp));
                        bmp.Dispose();
                        File.Delete(tmpfile2);
                    }
                    break;
            }
            File.Delete(tmpfile);
            return result;
        }
    }

    public static class TextureLoader
    {
        public static BitmapSource LoadBitmap(ImagingFactory2 factory, string filename)
        {
            var bitmapDecoder = new BitmapDecoder(
                factory,
                filename,
                DecodeOptions.CacheOnDemand
                );

            var formatConverter = new FormatConverter(factory);

            formatConverter.Initialize(
                bitmapDecoder.GetFrame(0),
                PixelFormat.Format32bppPRGBA,
                BitmapDitherType.None,
                null,
                0.0,
                BitmapPaletteType.Custom);

            return formatConverter;
        }
        public static SharpDX.Direct3D11.Texture2D CreateTexture2DFromBitmap(SharpDX.Direct3D11.Device device, BitmapSource bitmapSource)
        {
            int stride = bitmapSource.Size.Width * 4;
            using (var buffer = new SharpDX.DataStream(bitmapSource.Size.Height * stride, true, true))
            {
                bitmapSource.CopyPixels(stride, buffer);
                return new SharpDX.Direct3D11.Texture2D(device, new SharpDX.Direct3D11.Texture2DDescription()
                {
                    Width = bitmapSource.Size.Width,
                    Height = bitmapSource.Size.Height,
                    ArraySize = 1,
                    BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Immutable,
                    CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                    Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                    MipLevels = 1,
                    OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                    SampleDescription = new SampleDescription(1, 0),
                }, new DataRectangle(buffer.DataPointer, stride));
            }
        }

        public static unsafe SharpDX.WIC.Bitmap CreateWICBitmapFromGDI(System.Drawing.Bitmap gdiBitmap)
        {
            var wicFactory = new ImagingFactory();
            var wicBitmap = new SharpDX.WIC.Bitmap(
                wicFactory, gdiBitmap.Width, gdiBitmap.Height,
                SharpDX.WIC.PixelFormat.Format32bppBGRA,
                BitmapCreateCacheOption.CacheOnLoad);

            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(
                0, 0, gdiBitmap.Width, gdiBitmap.Height);
            var btmpData = gdiBitmap.LockBits(rect,
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            byte* pGDIData = (byte*)btmpData.Scan0;
            using (BitmapLock bl = wicBitmap.Lock(BitmapLockFlags.Write))
            {
                byte* pWICData = (byte*)bl.Data.DataPointer;
                for (int y = 0; y < gdiBitmap.Height; y++)
                {
                    int offsetWIC = y * bl.Stride;
                    int offsetGDI = y * btmpData.Stride;
                    for (int x = 0; x < gdiBitmap.Width; x++)
                    {
                        pWICData[offsetWIC + 0] = pGDIData[offsetGDI + 0];  //R
                        pWICData[offsetWIC + 1] = pGDIData[offsetGDI + 1];  //G
                        pWICData[offsetWIC + 2] = pGDIData[offsetGDI + 2];  //B
                        pWICData[offsetWIC + 3] = pGDIData[offsetGDI + 3];      //A
                        offsetWIC += 4;
                        offsetGDI += 4;
                    }
                }
            }

            gdiBitmap.UnlockBits(btmpData);

            return wicBitmap;
        }
    }

    public class RenderObject
    { 
        public enum RenderType
        {
            LineList,
            TriListWired,
            TriListTextured,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex
        {
            public Vertex(Vector4 position, Color4 color, Vector2 textureUV)
            {
                Position = position;
                Color = color;
                TextureUV = textureUV;
            }

            public Vector4 Position;
            public Color4 Color;
            public Vector2 TextureUV;
        }

        public Device device;
        public Engine3D engine;
        public RenderType type;
        public RawVector3[] vertices = new RawVector3[] { new RawVector3(-0.5f, 0.5f, 0.0f),
                                                          new RawVector3(0.5f, 0.5f, 0.0f),
                                                          new RawVector3(0.0f, -0.5f, 0.0f) };
        public Vertex[] verticesTextured = new Vertex[] { new Vertex(new Vector4(-0.5f, 0.5f, 0.0f, 1.0f), Color4.White, new Vector2(0, 0)),
                                                          new Vertex(new Vector4(0.0f, -0.5f, 0.0f, 1.0f), Color4.White, new Vector2(0.5f, 1f)),
                                                          new Vertex(new Vector4(0.5f, 0.5f, 0.0f, 1.0f), Color4.White, new Vector2(1, 0))};
        public SharpDX.Direct3D11.Buffer triangleVertexBufferWired;
        public SharpDX.Direct3D11.Buffer triangleVertexBufferTextured;
        public SharpDX.Direct3D11.Texture2D texture;
        public ShaderResourceView textureView;

        public RenderObject(Device d, RenderType t, SharpDX.Direct3D11.Texture2D tex, Engine3D e)
        {
            device = d;
            type = t;
            engine = e;
            texture = tex;
            if (tex != null)
                textureView = new ShaderResourceView(device, tex);
        }

        public void InitGeometry()
        {
            if(vertices != null)
                triangleVertexBufferWired = SharpDX.Direct3D11.Buffer.Create<RawVector3>(device, BindFlags.VertexBuffer, vertices);
            if(verticesTextured != null)
                triangleVertexBufferTextured = SharpDX.Direct3D11.Buffer.Create<Vertex>(device, BindFlags.VertexBuffer, verticesTextured);
        }

        public void Render(DeviceContext context)
        {
            switch (type)
            {
                case RenderType.LineList:
                    context.Rasterizer.State = engine.rasterStateWired;
                    context.VertexShader.Set(engine.vsWired);
                    context.PixelShader.Set(engine.psWired);
                    context.InputAssembler.InputLayout = engine.inputLayoutWired;
                    context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
                    context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(triangleVertexBufferWired, Utilities.SizeOf<RawVector3>(), 0));
                    context.Draw(vertices.Count(), 0);
                    break;
                case RenderType.TriListWired:
                    context.Rasterizer.State = engine.rasterStateWired;
                    context.VertexShader.Set(engine.vsWired);
                    context.PixelShader.Set(engine.psWired);
                    context.InputAssembler.InputLayout = engine.inputLayoutWired;
                    context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                    context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(triangleVertexBufferWired, Utilities.SizeOf<RawVector3>(), 0));
                    context.Draw(vertices.Count(), 0);
                    break;
                case RenderType.TriListTextured:
                    context.Rasterizer.State = engine.rasterStateTextured;
                    context.VertexShader.Set(engine.vsTextured);
                    context.InputAssembler.InputLayout = engine.inputLayoutTextured;
                    context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                    context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(triangleVertexBufferTextured, Utilities.SizeOf<Vertex>(), 0));
                    context.PixelShader.Set(engine.psTextured);
                    context.PixelShader.SetShaderResource(0, textureView);
                    context.PixelShader.SetSampler(0, engine.sampler);
                    context.Draw(verticesTextured.Count(), 0);
                    break;
            }
        }

        public void Dispose()
        {
            if (triangleVertexBufferWired != null)
                triangleVertexBufferWired.Dispose();
            if (triangleVertexBufferTextured != null)
                triangleVertexBufferTextured.Dispose();
        }
    }
}
