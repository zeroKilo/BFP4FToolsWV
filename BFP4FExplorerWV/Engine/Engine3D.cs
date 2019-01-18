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
        public bool renderLevel = false;
        public TextureManager textureManager = new TextureManager();
        public Device device;
        public DeviceContext context;
        public SwapChain swapChain;
        public Texture2D backBuffer;
        public RenderTargetView renderTargetView;
        public DepthStencilView depthStencilView;
        public DepthStencilState depthStencilState;
        public float CamRot = 3.1415f / 180f, CamDis = 3f, CamHeight = 0;
        public Vector3 CamPos = Vector3.Zero;
        public List<RenderObject> objects;
        public RenderObject terrain;
        public PixelShader psWired, psTextured;
        public InputElement[] inputElementsWired = new InputElement[]
        {
            new InputElement("SV_Position", 0, Format.R32G32B32A32_Float, 0, 0),
            new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
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
        public SharpDX.Direct3D11.Buffer worldViewProjectionBuffer;
        public Texture2D defaultTexture;
        public RasterizerState rasterStateWired, rasterStateTextured;
        private RawViewportF viewport;
        
        private RawMatrix view;
        private RawMatrix proj;
        private RawVector3 camPosRel;

        public Engine3D(PictureBox f)
        {
            textureManager.engine = this;
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
                DriverType.Hardware,
                DeviceCreationFlags.None,
                new[] {
                    FeatureLevel.Level_11_1,
                    FeatureLevel.Level_11_0,
                    FeatureLevel.Level_10_1,
                    FeatureLevel.Level_10_0,
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
                    Usage = Usage.BackBuffer | Usage.RenderTargetOutput,
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
            defaultTexture = textureManager.CreateTexture2DFromBitmap(device, textureManager.LoadBitmap(new ImagingFactory2(), "res\\default.png"));
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
            swapChain.ResizeBuffers(1, f.ClientSize.Width, f.ClientSize.Height, Format.Unknown, SwapChainFlags.AllowModeSwitch);
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
            camPosRel = new RawVector3((float)Math.Sin(CamRot) * CamDis, CamHeight, (float)Math.Cos(CamRot) * CamDis);
            camPosRel += CamPos;
            view = Matrix.LookAtLH(camPosRel, CamPos, Vector3.UnitY);            
            if (renderLevel)
                foreach (BF2LevelObject lo in BF2Level.objects)
                    lo.Render(context, view, proj);
            if (terrain != null)
                terrain.Render(context, view, proj);
            foreach (RenderObject ro in objects)
                    ro.Render(context, view, proj);
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
                foreach (RenderObject.VertexWired vw in o.verticesWired)
                {
                    Vector4 v = vw.Position;
                    float l = (float)Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
                    if (l > dis)
                        dis = l;
                }
                foreach(RenderObject.VertexTextured v in o.verticesTextured)
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

        public Ray UnprojectClick(int x, int y)
        {
            Matrix temp = Matrix.Multiply(view, proj);
            return Ray.GetPickRay(x, y, viewport, temp);
        }
    }
}