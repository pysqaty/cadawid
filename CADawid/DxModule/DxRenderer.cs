using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CADawid.Model;
using CADawid.Utils;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

namespace CADawid.DxModule
{
    public class DxRenderer : INotifyPropertyChanged
    {
        private const float SelectPrecision = 0.1f;

        private DriverType driverType;
        public Device device;
        private RenderTargetView renderTargetView;
        private DepthStencilView depthStencilView;
        private DepthStencilState depthStencilStateOn;
        private DepthStencilState depthStencilStateOff;
        private BlendState blendStereoscopy;

        private Vector2? boxSelectionStart;
        private Vector2? boxSelectionEnd;
        public string modeLabel;

        #region Shaders
        public VertexShader vertexShader;
        public PixelShader pixelShader;
        public GeometryShader geometryShader;
        public InputLayout inputLayout;
        
        public VertexShader vertexShaderPoint;
        public PixelShader pixelShaderPoint;
        public GeometryShader geometryShaderPoint;
        public InputLayout inputLayoutPoint;

        public VertexShader vertexShaderCursor;
        public PixelShader pixelShaderCursor;
        public GeometryShader geometryShaderCursor;
        public InputLayout inputLayoutCursor;

        public VertexShader vertexShaderCurve;
        public PixelShader pixelShaderCurve;
        public GeometryShader geometryShaderCurve;
        public InputLayout inputLayoutCurve;

        public VertexShader vertexShaderPatch;
        public PixelShader pixelShaderPatch;
        public GeometryShader geometryShaderPatch;
        public InputLayout inputLayoutPatch;

        public VertexShader vertexShaderBsplinePatch;
        public PixelShader pixelShaderBsplinePatch;
        public GeometryShader geometryShaderBsplinePatch;

        public VertexShader vertexShaderGregoryPatch;
        public PixelShader pixelShaderGregoryPatch;
        public GeometryShader geometryShaderGregoryPatch;
        #endregion

        public SharpDX.Direct3D11.Buffer constantBuffer;
        public SharpDX.Direct3D11.Buffer pointConstantBuffer;
        public SharpDX.Direct3D11.Buffer patchConstantBuffer;
        public SharpDX.Direct3D11.Buffer geometryConstantBuffer;

        public SharpDX.Direct2D1.RenderTarget renderTarget;

        #region Dock
        private bool isDocked;
        public bool IsDocked
        {
            get => isDocked;
            set
            {
                isDocked = value;
                NotifyPropertyChanged(nameof(IsDocked));
                NotifyPropertyChanged(nameof(IsUndocked));
            }
        }
        public bool IsUndocked
        {
            get => !isDocked;
        }
        #endregion

        public Scene Scene { get; set; }
        public TransformCenter CurrentTransformCenter { get; set; }

        public IGeometryObject ObjectToAdd { get; set; }

        #region Init
        public DxRenderer()
        {
            IsDocked = false;
            device = null;
            renderTargetView = null;
            depthStencilView = null;
            inputLayout = null;
            vertexShader = null;
            pixelShader = null;
            constantBuffer = null;
            pointConstantBuffer = null;
            patchConstantBuffer = null;
            Scene = new Scene();
            InitDevice();
        }
        private void InitDevice()
        {
            DeviceCreationFlags createDeviceFlags = DeviceCreationFlags.BgraSupport;
            DriverType[] driverTypes = new DriverType[]
            {
                DriverType.Hardware,
                DriverType.Warp,
                DriverType.Reference
            };
            int numDriverTypes = driverTypes.Length;

            FeatureLevel[] featureLevels = new FeatureLevel[]
            {
                FeatureLevel.Level_11_1,
                FeatureLevel.Level_11_0,
                FeatureLevel.Level_10_1,
                FeatureLevel.Level_10_0
            };
            int numFeatureLevels = featureLevels.Length;

            for (int i = 0; i < numDriverTypes; i++)
            {
                device = new SharpDX.Direct3D11.Device(driverTypes[i],
                    createDeviceFlags, featureLevels);
                if (device != null)
                {
                    driverType = driverTypes[i];
                    break;
                }
            }
            (vertexShader, pixelShader, geometryShader, inputLayout) = LoadShaders("../../../Shader/basicShader.fx", new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                0, 0, InputClassification.PerVertexData, 0),
            });
            (vertexShaderPoint, pixelShaderPoint, geometryShaderPoint, inputLayoutPoint) = LoadShaders("../../../Shader/pointShader.fx", new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                0, 0, InputClassification.PerVertexData, 0),
            });
            (vertexShaderCursor, pixelShaderCursor, geometryShaderCursor, inputLayoutCursor) = LoadShaders("../../../Shader/crossShader.fx", new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                0, 0, InputClassification.PerVertexData, 0),
            });
            (vertexShaderCurve, pixelShaderCurve, geometryShaderCurve, inputLayoutCurve) = LoadShaders("../../../Shader/curveShader.fx", new InputElement[]
            {
                new InputElement("P0p", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                        InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElement("P1p", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                        InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElement("P2p", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                        InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElement("P3p", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                        InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
            });

            var inputElements = new InputElement[]
            {
                new InputElement("P00p", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                        InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElement("P10p", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                        InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElement("P20p", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                        InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElement("P30p", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                        InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElement("P01p", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                        InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElement("P11p", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                        InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElement("P21p", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                        InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElement("P31p", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                        InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElement("P02p", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                        InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElement("P12p", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                        InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElement("P22p", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                        InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElement("P32p", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                        InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElement("P03p", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                        InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElement("P13p", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                        InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElement("P23p", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                        InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
                new InputElement("P33p", 0, SharpDX.DXGI.Format.R32G32B32A32_Float,
                        InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
            };

            (vertexShaderPatch, pixelShaderPatch, geometryShaderPatch, inputLayoutPatch) = LoadShaders("../../../Shader/patchShader.fx", inputElements);
            (vertexShaderGregoryPatch, pixelShaderGregoryPatch, geometryShaderGregoryPatch, _) = LoadShaders("../../../Shader/gregoryPatchShader.fx", inputElements);

            (vertexShaderBsplinePatch, pixelShaderBsplinePatch, geometryShaderBsplinePatch, _) = LoadShaders("../../../Shader/bsplinePatchShader.fx", inputElements);

            unsafe
            {
                CreateConstantBuffer(ref constantBuffer, sizeof(DxConstantBuffer), 0, DxShaderType.VertexShader);
                CreateConstantBuffer(ref pointConstantBuffer, 16, 1, DxShaderType.PixelShader);
                CreateConstantBuffer(ref patchConstantBuffer, 16, 2, DxShaderType.GeometryShader);
                CreateConstantBuffer(ref geometryConstantBuffer, sizeof(DxConstantBuffer), 0, DxShaderType.GeometryShader);
            }

        }
        private void InitRenderTarget(IntPtr resourcePointer)
        {
            ComObject pUnk = new ComObject(resourcePointer);
            SharpDX.DXGI.Resource pDXGIResource = pUnk.QueryInterface<SharpDX.DXGI.Resource>();
            IntPtr sharedHandle = pDXGIResource.SharedHandle;

            SharpDX.Direct3D11.Resource tempResource11 = device.OpenSharedResource<SharpDX.Direct3D11.Resource>(sharedHandle);
            Texture2D pOutputResource = tempResource11.QueryInterface<Texture2D>();

            using (var surface = pOutputResource.QueryInterface<SharpDX.DXGI.Surface>())
            {
                var d2dFactory = new SharpDX.Direct2D1.Factory();
                var dpi = d2dFactory.DesktopDpi;
                SharpDX.Direct2D1.RenderTargetProperties props = new SharpDX.Direct2D1.RenderTargetProperties(SharpDX.Direct2D1.RenderTargetType.Hardware,
                    new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                    96, 96, SharpDX.Direct2D1.RenderTargetUsage.None, SharpDX.Direct2D1.FeatureLevel.Level_DEFAULT);
                renderTarget = new SharpDX.Direct2D1.RenderTarget(d2dFactory, surface, props);
            }


            RenderTargetViewDescription rtDesc = new RenderTargetViewDescription();
            rtDesc.Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm;
            rtDesc.Dimension = RenderTargetViewDimension.Texture2D;
            rtDesc.Texture2D = new RenderTargetViewDescription.Texture2DResource();
            rtDesc.Texture2D.MipSlice = 0;

            renderTargetView = new RenderTargetView(device, pOutputResource, rtDesc);

            var blendDescription = new BlendStateDescription();
            blendDescription.RenderTarget[0].IsBlendEnabled = true;
            blendDescription.RenderTarget[0].SourceBlend = BlendOption.One;
            blendDescription.RenderTarget[0].DestinationBlend = BlendOption.One;
            blendDescription.RenderTarget[0].BlendOperation = BlendOperation.Add;
            blendDescription.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            blendDescription.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
            blendDescription.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            blendDescription.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            blendStereoscopy = new BlendState(device, blendDescription);

            device.ImmediateContext.OutputMerger.SetBlendState(null);

            Texture2DDescription outputResourceDesc = pOutputResource.Description;

            if (outputResourceDesc.Width != Scene.Camera.Width || outputResourceDesc.Height != Scene.Camera.Height)
            {
                Scene.Camera.Width = outputResourceDesc.Width;
                Scene.Camera.Height = outputResourceDesc.Height;
                Scene.Camera.SetUpViewPort(device);
                Scene.UpdatePositions();
            }

            Texture2DDescription depthTexture = new Texture2DDescription();
            depthTexture.Width = outputResourceDesc.Width;
            depthTexture.Height = outputResourceDesc.Height;
            depthTexture.ArraySize = 1;
            depthTexture.Format = SharpDX.DXGI.Format.D24_UNorm_S8_UInt;
            depthTexture.SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0);
            depthTexture.Usage = ResourceUsage.Default;
            depthTexture.BindFlags = BindFlags.DepthStencil;
            depthTexture.MipLevels = 1;
            depthTexture.CpuAccessFlags = CpuAccessFlags.None;
            depthTexture.OptionFlags = ResourceOptionFlags.None;
            depthTexture.MipLevels = 0;

            Texture2D depthStencilBuffer = new Texture2D(device, depthTexture);

            DepthStencilStateDescription depthStencilDescOn = new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,
                IsStencilEnabled = true,
                StencilReadMask = 0xFF,
                StencilWriteMask = 0xFF,
                FrontFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Increment,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always
                },
                BackFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Decrement,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always
                }
            };
            DepthStencilStateDescription depthStencilDescOff = new DepthStencilStateDescription()
            {
                IsDepthEnabled = false,
            };

            DepthStencilViewDescription depthStencilViewDesc = new DepthStencilViewDescription()
            {
                Format = SharpDX.DXGI.Format.D24_UNorm_S8_UInt,
                Dimension = DepthStencilViewDimension.Texture2D,
                Texture2D = new DepthStencilViewDescription.Texture2DResource()
                {
                    MipSlice = 0
                }
            };

            depthStencilStateOn = new DepthStencilState(device, depthStencilDescOn);
            depthStencilStateOff = new DepthStencilState(device, depthStencilDescOff);
            device.ImmediateContext.OutputMerger.SetDepthStencilState(depthStencilStateOn, 1);

            depthStencilView = new DepthStencilView(device, depthStencilBuffer, depthStencilViewDesc);

            device.ImmediateContext.OutputMerger.SetRenderTargets(depthStencilView, renderTargetView);

            

            pOutputResource?.Dispose();
            tempResource11?.Dispose();
            pDXGIResource?.Dispose();
        }
        private (VertexShader, PixelShader, GeometryShader, InputLayout) LoadShaders(string shaderPath, InputElement[] layout)
        {
            try
            {
                string fullPath = System.IO.Path.GetFullPath(shaderPath);
                var vSByteCode = ShaderBytecode.CompileFromFile(fullPath, "VS", "vs_5_0");
                var pSByteCode = ShaderBytecode.CompileFromFile(fullPath, "PS", "ps_5_0");
                var gSByteCode = ShaderBytecode.CompileFromFile(fullPath, "GS", "gs_5_0", ShaderFlags.Debug);
                VertexShader vs = new VertexShader(device, vSByteCode);
                PixelShader ps = new PixelShader(device, pSByteCode);
                GeometryShader gs = new GeometryShader(device, gSByteCode);

                InputLayout il = new InputLayout(device, vSByteCode, layout);
                device.ImmediateContext.InputAssembler.InputLayout = inputLayout;

                vSByteCode.Dispose();
                gSByteCode.Dispose();
                pSByteCode.Dispose();

                return (vs, ps, gs, il);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return (null, null, null, null);
        }
        #endregion

        #region Buffers
        public SharpDX.Direct3D11.Buffer CreateVertexBuffer<T>(T[] vertices, int tSize) where T : struct
        {
            BufferDescription bd = new BufferDescription();
            bd.Usage = ResourceUsage.Immutable;
            bd.CpuAccessFlags = CpuAccessFlags.None;
            bd.BindFlags = BindFlags.VertexBuffer;
            unsafe
            {
                bd.SizeInBytes = tSize * vertices.Length;
            }
            bd.CpuAccessFlags = 0;
            return SharpDX.Direct3D11.Buffer.Create<T>(device, vertices, bd);
        }
        public void SetVertexBuffer<T>(SharpDX.Direct3D11.Buffer vertexBuffer, int tSize)
        {
            int stride = 0;
            unsafe
            {
                stride = tSize;
            }
            int offset = 0;
            var vBB = new VertexBufferBinding(vertexBuffer, stride, offset);
            device.ImmediateContext.InputAssembler.SetVertexBuffers(0, vBB);
        }
        public SharpDX.Direct3D11.Buffer CreateIndexBuffer<T>(T[] indices, int tSize) where T : struct
        {
            BufferDescription bd = new BufferDescription();
            bd.Usage = ResourceUsage.Default;
            unsafe
            {
                bd.SizeInBytes = tSize * indices.Length;
            }

            bd.BindFlags = BindFlags.IndexBuffer;
            bd.CpuAccessFlags = CpuAccessFlags.None;
            return SharpDX.Direct3D11.Buffer.Create(device, indices, bd);
        }
        public void SetIndexBuffer(SharpDX.Direct3D11.Buffer indicesBuffer)
        {
            device.ImmediateContext.InputAssembler.SetIndexBuffer(indicesBuffer, SharpDX.DXGI.Format.R16_UInt, 0);
        }
        private void CreateConstantBuffer(ref SharpDX.Direct3D11.Buffer buffer, int sizeInBytes, int slot, DxShaderType shaderType)
        {
            try
            {
                BufferDescription bd = new BufferDescription();
                bd.Usage = ResourceUsage.Dynamic;
                unsafe
                {
                    bd.SizeInBytes = sizeInBytes;
                }
                bd.BindFlags = BindFlags.ConstantBuffer;
                bd.CpuAccessFlags = CpuAccessFlags.Write;
                buffer = new SharpDX.Direct3D11.Buffer(device, bd);
                if (shaderType == DxShaderType.VertexShader)
                {
                    device.ImmediateContext.VertexShader.SetConstantBuffer(slot, buffer);
                }
                else if(shaderType == DxShaderType.GeometryShader)
                {
                    device.ImmediateContext.GeometryShader.SetConstantBuffer(slot, buffer);
                }
                else if (shaderType == DxShaderType.PixelShader)
                {
                    device.ImmediateContext.PixelShader.SetConstantBuffer(slot, buffer);
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

        }
        public void UpdateConstantBuffer<T>(ref SharpDX.Direct3D11.Buffer buffer, T cb) where T : struct
        {
            DataStream dataStream;
            device.ImmediateContext.MapSubresource(buffer, MapMode.WriteDiscard, MapFlags.None, out dataStream);
            dataStream?.Write(cb);
            dataStream?.Dispose();
            device.ImmediateContext.UnmapSubresource(buffer, 0);
        }
        #endregion

        #region Render

        public void Render(IntPtr resourcePointer, bool isNewSurface)
        {
            if (isNewSurface)
            {
                renderTargetView = null;
                device.ImmediateContext.OutputMerger.SetRenderTargets(renderTargetView: null);
                InitRenderTarget(resourcePointer);
            }

            RawColor4 clearColor = new RawColor4(0.0f, 0.0f, 0.0f, 1.0f);
            device.ImmediateContext.ClearRenderTargetView(renderTargetView, clearColor);
            device.ImmediateContext.ClearDepthStencilView(depthStencilView,
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
            Scene.Camera.Update();
            if (!Scene.Camera.isStereoscopyEnabled)
            {
                device.ImmediateContext.OutputMerger.SetBlendState(null);
                Scene.Camera.UpdateVPMatrix();
                Render3D((color) => color);
            }
            else
            {
                device.ImmediateContext.OutputMerger.SetBlendState(blendStereoscopy);
                Scene.Camera.IsLeftEye = false;
                Scene.Camera.UpdateVPMatrix();
                Render3D((color) => Scene.Camera.GetEyeColor(color, false));
                device.ImmediateContext.ClearDepthStencilView(depthStencilView,
                    DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
                Scene.Camera.IsLeftEye = true;
                Scene.Camera.UpdateVPMatrix();
                Render3D((color) => Scene.Camera.GetEyeColor(color, true));
            }
            RenderBoxSelect();
            RenderModeLabel(modeLabel);

            if (device.ImmediateContext != null)
            {
                device.ImmediateContext.Flush();
            }
        }
        public void RenderGeometries(Func<Vector4, Vector4> colorModifier)
        {
            foreach (IGeometryObject geometry in Scene.Geometries)
            {
                geometry.Render(this, geometry.CurrentModel, geometry.IsSelected, colorModifier);
            }
        }
        public void RenderConstantGeometries(Func<Vector4, Vector4> colorModifier)
        {
            device.ImmediateContext.OutputMerger.SetDepthStencilState(depthStencilStateOff, 1);
            foreach (IGeometryObject geometry in Scene.ConstantGeometries)
            {
                geometry.Render(this, Matrix.Identity, true, colorModifier);
            }
            device.ImmediateContext.OutputMerger.SetDepthStencilState(depthStencilStateOn, 1);
        }
        public void RenderPoints(Func<Vector4, Vector4> colorModifier)
        {
            Matrix faceIt = Matrix.RotationX(-Scene.Camera.RotationV.X) * Matrix.RotationY(-Scene.Camera.RotationV.Y) * Matrix.RotationZ(-Scene.Camera.RotationV.Z);
            foreach (IGeometryObject geometry in Scene.Points)
            {
                geometry.Render(this, faceIt * Matrix.Translation(geometry.TranslationV), geometry.IsSelected, colorModifier);
            }
            if (Scene.CenterOfSelection != null)
            {
                Scene.CenterOfSelection.Render(this, faceIt * Matrix.Translation(Scene.CenterOfSelection.TranslationV), true, colorModifier);
            }
        }
        public void RenderCursor(Func<Vector4, Vector4> colorModifier)
        {
            Matrix faceIt = Matrix.RotationX(-Scene.Camera.RotationV.X) * Matrix.RotationY(-Scene.Camera.RotationV.Y) * Matrix.RotationZ(-Scene.Camera.RotationV.Z);
            DxPointConstantBuffer pcb = new DxPointConstantBuffer();
            pcb.parameter = 0.06f;
            Scene.Cursor.Render(this, faceIt * Matrix.Translation(Scene.Cursor.TranslationV), true, colorModifier);
        }
        public void RenderModeLabel(string mode)
        {
            if(modeLabel == null)
            {
                return;
            } 
            renderTarget.BeginDraw();
            renderTarget.DrawText(mode, 
                new SharpDX.DirectWrite.TextFormat(
                    new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared), 
                    "Arial", 14), 
                new RawRectangleF(10, 10, 200, 10), 
                new SharpDX.Direct2D1.SolidColorBrush(renderTarget, new RawColor4(0.6f, 0.6f, 0.0f, 1)));
            renderTarget.EndDraw();
        }
        public void Render3D(Func<Vector4, Vector4> colorModifier)
        {
            RenderConstantGeometries(colorModifier);
            RenderGeometries(colorModifier);
            RenderPoints(colorModifier);
            RenderCursor(colorModifier);
        }
        #endregion

        #region Selection

        public void RenderBoxSelect()
        {
            if(boxSelectionStart == null || boxSelectionEnd == null)
            {
                return;
            }
            renderTarget.BeginDraw();
            renderTarget.FillRectangle(new SharpDX.Mathematics.Interop.RawRectangleF(boxSelectionStart.Value.X, boxSelectionStart.Value.Y, 
                boxSelectionEnd.Value.X, boxSelectionEnd.Value.Y),
                new SharpDX.Direct2D1.SolidColorBrush(renderTarget,
                new SharpDX.Mathematics.Interop.RawColor4(0.2f, 0.2f, 0.2f, 0.4f)));
            renderTarget.EndDraw();
        }
        public void SetStartBoxSelection(Vector2? startPosition)
        {
            boxSelectionStart = startPosition;
        }
        public void SetEndBoxSelection(Vector2? endPosition)
        {
            boxSelectionEnd = endPosition;
        }

        public List<IGeometryObject> CheckBoxSelection()
        {
            List<IGeometryObject> selectedObjects = new List<IGeometryObject>();
            if (boxSelectionStart == null || boxSelectionEnd == null)
            {
                return selectedObjects;
            }
            float minX, minY, maxX, maxY;
            if(boxSelectionEnd.Value.X < boxSelectionStart.Value.X)
            {
                minX = boxSelectionEnd.Value.X;
                maxX = boxSelectionStart.Value.X;
            }
            else
            {
                minX = boxSelectionStart.Value.X;
                maxX = boxSelectionEnd.Value.X;
            }
            if (boxSelectionEnd.Value.Y < boxSelectionStart.Value.Y)
            {
                minY = boxSelectionEnd.Value.Y;
                maxY = boxSelectionStart.Value.Y;
            }
            else
            {
                minY = boxSelectionStart.Value.Y;
                maxY = boxSelectionEnd.Value.Y;
            }
            foreach (IGeometryObject g in Scene.Points)
            {
                if(g is Model.Point point)
                {
                    MyVector3 sPos = point.GetScreenPosition(Scene.Camera);
                    if(sPos.X <= maxX && sPos.X >= minX &&
                        sPos.Y <= maxY && sPos.Y >= minY)
                    {
                        selectedObjects.Add(g);
                    }
                }
            }
            return selectedObjects;
        }

        public (IGeometryObject selectedObject, bool isVirtual) SelectPoint(Vector4 cameraPos, Vector3 ray)
        {
            bool hasSelection = false;
            bool isVirtual = false;
            Vector3 from = new Vector3(cameraPos.X, cameraPos.Y, cameraPos.Z);
            float min = float.MaxValue;
            IGeometryObject selected = null;
            foreach (IGeometryObject g in Scene.Points)
            {
                if (g is Model.Point point)
                {
                    hasSelection = CheckSelection(from, cameraPos, point, ray, ref min, ref selected);
                    if (hasSelection)
                    {
                        isVirtual = false;
                    }
                }
            }
            foreach (IGeometryObject g in Scene.VirtualPoints)
            {
                if (g is Model.Point point)
                {
                    hasSelection = CheckSelection(from, cameraPos, point, ray, ref min, ref selected);
                    if (hasSelection)
                    {
                        isVirtual = true;
                    }
                }
            }
            return (selected, isVirtual);
        }
        private bool CheckSelection(Vector3 from, Vector4 cameraPos, Model.Point point, Vector3 ray, ref float min, ref IGeometryObject selected)
        {
            bool isSelected = false;
            Vector4 normal4 = cameraPos - new Vector4(point.TranslationV, 1);
            Vector3 normal = new Vector3(normal4.X, normal4.Y, normal4.Z);
            normal.Normalize();

            float t = (Vector3.Dot((Vector3)point.TranslationV
                - from, normal)) /
                Vector3.Dot(ray, normal);

            Vector3 intersection = from + t * ray;

            float dist = Vector3.Distance(intersection, point.TranslationV);
            if (dist < point.Size / 2 + SelectPrecision)
            {
                if (dist < min)
                {
                    isSelected = true;
                    min = dist;
                    selected = point;
                }
            }
            return isSelected;
        }
        #endregion

        #region Property changed
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Add

        public void UpdateObjectToAdd(GeometryType type)
        {
            ObjectToAdd = ObjectsStaticFactory.CreateObject(type);
        }

        #endregion
    }
}
