using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using SharpDX;
using CADawid.DxModule;
using CADawid.Utils;
using CADawid.ViewModel;
using System.Windows.Controls;
using CADawid.View;
using CADawid.Model;
using System.Collections.Generic;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using CADawid.Enums;
using CADawid.View.AddPanels;
using CADawid.Serialization.Model;
using System.Xml.Serialization;
using System.IO;
using CADawid.Serialization;
using Microsoft.Win32;

namespace CADawid
{
    public partial class MainWindow : Window, IGeometryPanel, IGeometryAddPanel
    {
        private TimeSpan lastRender;
        private bool lastVisible;
        private System.Windows.Point mousePosition;
        private DxRenderer dxRenderer { get; set; }
        private SceneSerializer sceneSerializer { get; set; } 
        private GeometryViewModel currentViewModel;

        private Mode mode;
        private Mode Mode
        {
            get => mode;
            set
            {
                mode = value;
                if(mode == Mode.None)
                {
                    dxRenderer.modeLabel = null;
                }
                else
                {
                    dxRenderer.modeLabel = "Mode: " + mode.ToString();
                }
            }
        }

        public MainWindow()
        {
            dxRenderer = new DxRenderer();
            sceneSerializer = new SceneSerializer();
            Mode = Mode.None;
            InitializeComponent();
            this.host.Loaded += new RoutedEventHandler(this.Host_Loaded);
            this.host.SizeChanged += new SizeChangedEventHandler(this.Host_SizeChanged);
            HierarchyList.ItemsSource = dxRenderer.Scene.AllGeometries;

            foreach (GeometryType type in (GeometryType[])Enum.GetValues(typeof(GeometryType)))
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = type;
                AddType.Items.Add(comboBoxItem);
            }
            AddType.SelectedIndex = 0;
            AddType.DataContext = dxRenderer;

            TransformCenter.DataContext = dxRenderer;
            CursorPositon.DataContext = dxRenderer.Scene;
            DockButton.DataContext = dxRenderer;
            CameraSection.DataContext = dxRenderer.Scene.Camera;
            ImageActions.DataContext = this;
        }

        #region DirectX
        private void Host_Loaded(object sender, RoutedEventArgs e)
        {
            this.InitializeRendering();
        }
        private void Host_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double dpiScale = 1.0;

            var hwndTarget = PresentationSource.FromVisual(this).CompositionTarget as HwndTarget;
            //if (hwndTarget != null)
            //{
            //    dpiScale = hwndTarget.TransformToDevice.M11;
            //}

            int surfWidth = (int)(host.ActualWidth < 0 ? 0 : Math.Ceiling(host.ActualWidth * dpiScale));
            int surfHeight = (int)(host.ActualHeight < 0 ? 0 : Math.Ceiling(host.ActualHeight * dpiScale));

            InteropImage.SetPixelSize(surfWidth, surfHeight);

            bool isVisible = (surfWidth != 0 && surfHeight != 0);
            if (lastVisible != isVisible)
            {
                lastVisible = isVisible;
                if (lastVisible)
                {
                    CompositionTarget.Rendering += CompositionTarget_Rendering;
                }
                else
                {
                    CompositionTarget.Rendering -= CompositionTarget_Rendering;
                }
            }
        }
        private void InitializeRendering()
        {
            IntPtr windowHandle = new WindowInteropHelper(this).Handle;
            var d3d = new SharpDX.Direct3D9.Direct3D();
            var presentparams = new SharpDX.Direct3D9.PresentParameters(1, 1);
            SharpDX.Direct3D9.Device d3d9Device = new SharpDX.Direct3D9.Device(d3d, 0,
                SharpDX.Direct3D9.DeviceType.Hardware, windowHandle, SharpDX.Direct3D9.CreateFlags.HardwareVertexProcessing, presentparams);
            var surface = d3d9Device.GetBackBuffer(0, 0);
            InteropImage.Lock();
            InteropImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
            InteropImage.Unlock();

            InteropImage.WindowOwner = (new System.Windows.Interop.WindowInteropHelper(this)).Handle;
            InteropImage.OnRender = this.DoRender;
            InteropImage.RequestRender();
        }
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            RenderingEventArgs args = (RenderingEventArgs)e;

            if (this.lastRender != args.RenderingTime)
            {
                InteropImage.RequestRender();
                this.lastRender = args.RenderingTime;
            }
        }
        private void UninitializeRendering()
        {
            CompositionTarget.Rendering -= this.CompositionTarget_Rendering;
        }
        private void DoRender(IntPtr surface, bool isNewSurface)
        {
            dxRenderer.Render(surface, isNewSurface);
        }
        #endregion

        #region Image events / Selection


        public delegate void SceneDisplayChanged(DxCamera camera);
        public event SceneDisplayChanged OnSceneDisplayChanged;

        private void ImageHost_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            dxRenderer.Scene.Camera.ScaleLocally(e.Delta > 0 ? new Vector3(1f) : new Vector3(-1f));
            dxRenderer.Scene.UpdatePositions();
            OnSceneDisplayChanged?.Invoke(dxRenderer.Scene.Camera);
        }
        private void ImageHost_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point currentPosition = e.GetPosition(this);
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                Vector3 rotation = new Vector3((float)(currentPosition.Y - mousePosition.Y), (float)(currentPosition.X - mousePosition.X), 0f);
                dxRenderer.Scene.Camera.Rotate(rotation, new Vector3(0, 0, 0));
                dxRenderer.Scene.UpdatePositions();
                OnSceneDisplayChanged?.Invoke(dxRenderer.Scene.Camera);
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                if(Mode == Mode.BoxSelection)
                {
                    dxRenderer.SetEndBoxSelection(new Vector2((float)currentPosition.X, (float)currentPosition.Y));
                    mousePosition = currentPosition;
                    return;
                }
                Vector4 difference = new Vector4((float)(currentPosition.X - mousePosition.X), (float)(-currentPosition.Y + mousePosition.Y), 0f, 1f);
                Vector3 pivot = new Vector3(0f, 0f, 0f);
                if (dxRenderer.Scene.CenterOfSelection != null && dxRenderer.CurrentTransformCenter == Model.TransformCenter.SelectionCenter)
                {
                    pivot = dxRenderer.Scene.CenterOfSelection.TranslationV;
                }
                else if (dxRenderer.Scene.Cursor != null && dxRenderer.CurrentTransformCenter == Model.TransformCenter.Cursor)
                {
                    pivot = dxRenderer.Scene.Cursor.TranslationV;
                }

                if (Mode == Mode.Location)
                {
                    Vector3 translationVector = default;
                    if (Keyboard.IsKeyDown(Key.X))
                    {
                        translationVector = new Vector3(difference.X, 0.0f, 0.0f);
                    }
                    else if (Keyboard.IsKeyDown(Key.Y))
                    {
                        translationVector = new Vector3(0.0f, difference.X, 0.0f);
                    }
                    else if (Keyboard.IsKeyDown(Key.Z))
                    {
                        translationVector = new Vector3(0.0f, 0.0f, difference.X);
                    }
                    else
                    {
                        translationVector = MathExt.RotateVector(difference, dxRenderer.Scene.Camera.R);
                    }
                    foreach (IGeometryObject ob in HierarchyList.SelectedItems)
                    {
                        ob.Translate(translationVector);
                    }
                    foreach (IGeometryObject ob in dxRenderer.Scene.SelectedObjects)
                    {
                        ob.Translate(translationVector);
                    }
                }
                else if(Mode == Mode.Scale)
                {
                    Vector3 scaleVector = default;
                    if (Keyboard.IsKeyDown(Key.X))
                    {
                        scaleVector = new Vector3(difference.X, 0.0f, 0.0f);
                    }
                    else if (Keyboard.IsKeyDown(Key.Y))
                    {
                        scaleVector = new Vector3(0.0f, difference.X, 0.0f);
                    }
                    else if (Keyboard.IsKeyDown(Key.Z))
                    {
                        scaleVector = new Vector3(0.0f, 0.0f, difference.X);
                    }
                    else
                    {
                        scaleVector = new Vector3(difference.X);

                    }
                    foreach (IGeometryObject ob in HierarchyList.SelectedItems)
                    {
                        ob.Scale(scaleVector, pivot);
                    }
                }
                else if(Mode == Mode.Rotation)
                {
                    Vector3 rotationVector = default;
                    if (Keyboard.IsKeyDown(Key.X))
                    {
                        rotationVector = new Vector3(difference.X, 0f, 0f);
                    }
                    else if (Keyboard.IsKeyDown(Key.Y))
                    {
                        rotationVector = new Vector3(0f, difference.X, 0f);
                    }
                    else if (Keyboard.IsKeyDown(Key.Z))
                    {
                        rotationVector = new Vector3(0f, 0f, difference.X);
                    }
                    foreach (IGeometryObject ob in HierarchyList.SelectedItems)
                    {
                        ob.Rotate(rotationVector, pivot);
                    }
                }
                OnSceneDisplayChanged?.Invoke(dxRenderer.Scene.Camera);
            }
            else if(e.RightButton == MouseButtonState.Pressed)
            {
                Vector4 difference = new Vector4((float)(currentPosition.X - mousePosition.X), (float)(-currentPosition.Y + mousePosition.Y), 0f, 1f);
                dxRenderer.Scene.Camera.Translate(MathExt.RotateVector(difference, dxRenderer.Scene.Camera.R));
            }
            mousePosition = currentPosition;
        }
        private void ImageHost_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                if(Mode == Mode.BoxSelection)
                {
                    HierarchyList.SelectedItems.Clear();
                    dxRenderer.Scene.ClearSelection();
                    List<IGeometryObject> selectedObjects = dxRenderer.CheckBoxSelection();
                    foreach (IGeometryObject selected in selectedObjects)
                    {
                        if(!selected.IsSelected)
                        {
                            HierarchyList.SelectedItems.Add(selected);
                        }
                    }
                    dxRenderer.Scene.UpdateCenterOfSelection();
                    dxRenderer.SetStartBoxSelection(null);
                    dxRenderer.SetEndBoxSelection(null);
                }
            }
        }
        private void ImageHost_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point screenCoords = e.GetPosition(ImageHost);
                if (Keyboard.IsKeyDown(Key.LeftAlt))
                {
                    Vector4 worldPos = dxRenderer.Scene.Camera.ScreenToWorld((float)screenCoords.X,
                        (float)screenCoords.Y, dxRenderer.Scene.ScreenPosition.Z, false);
                    Vector3 cursorWorldPosition = new Vector3(worldPos.X / worldPos.W, worldPos.Y / worldPos.W, worldPos.Z / worldPos.W);

                    dxRenderer.Scene.ScreenWorldObject.TranslationV = cursorWorldPosition;
                    dxRenderer.Scene.UpdatePositions();
                    return;
                }
                dxRenderer.SetStartBoxSelection(new Vector2((float)screenCoords.X, (float)screenCoords.Y));
                if(Mode == Mode.None)
                {
                    (Vector4 cameraPos, Vector3 ray) = dxRenderer.Scene.Camera.ScreenSpaceToRay(screenCoords);
                    Vector3 from = new Vector3(cameraPos.X, cameraPos.Y, cameraPos.Z);

                    (IGeometryObject selected, bool isVirtual) = dxRenderer.SelectPoint(cameraPos, ray);

                    if (selected != null)
                    {
                        if (isVirtual)
                        {
                            HierarchyList.SelectedItems.Clear();
                            dxRenderer.Scene.SelectedObjects.Add(selected);
                            selected.IsSelected = true;
                        }
                        else
                        {
                            dxRenderer.Scene.ClearSelection();
                            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                            {
                                if (selected.IsSelected)
                                {
                                    HierarchyList.SelectedItems.Remove(selected);
                                }
                                else
                                {
                                    HierarchyList.SelectedItems.Add(selected);
                                }
                            }
                            else
                            {
                                HierarchyList.SelectedItems.Clear();
                                HierarchyList.SelectedItem = selected;
                            }
                        }
                    }
                    else
                    {
                        HierarchyList.SelectedItems.Clear();
                        dxRenderer.Scene.ClearSelection();
                    }
                    dxRenderer.Scene.UpdateCenterOfSelection();
                }
            }
            else if(e.RightButton == MouseButtonState.Pressed)
            {
                Mode = Mode.None;
            }
        }
        private void HierarchyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(dxRenderer.IsDocked)
            {
                return;
            }
            UpdatePanel();
        }
        #endregion

        #region Add/Delete

        public void AddBezierCurveC0(BezierCurveC0 bezierCurveC0, List<Model.Point> pointsForBezier)
        {
            bezierCurveC0.AddNodes(pointsForBezier);
            bezierCurveC0.OnEmptyNodes += PointBasedGeometryEmpty;
            OnGeometriesRemoved += bezierCurveC0.RemoveNodes;
            OnSceneDisplayChanged += bezierCurveC0.SetPrecision;
            dxRenderer.Scene.AddGeometry(bezierCurveC0);
        }
        public void AddBezierCurveC2(BezierCurveC2 bezierCurveC2, List<Model.Point> pointsForBezier)
        {
            bezierCurveC2.AddNodes(pointsForBezier);
            bezierCurveC2.OnEmptyNodes += PointBasedGeometryEmpty;
            bezierCurveC2.OnVirtualPointsChanged += dxRenderer.Scene.UpdateVirtualPoints;
            OnGeometriesRemoved += bezierCurveC2.RemoveNodes;
            dxRenderer.Scene.AddGeometry(bezierCurveC2);
        }
        public void AddInterpolationBezierCurveC2(InterpolationBezierCurveC2 interpolationBezierCurveC2, List<Model.Point> pointsForBezier)
        {
            interpolationBezierCurveC2.AddNodes(pointsForBezier);
            interpolationBezierCurveC2.OnEmptyNodes += PointBasedGeometryEmpty;
            OnGeometriesRemoved += interpolationBezierCurveC2.RemoveNodes;
            //OnSceneDisplayChanged += bezierCurveC0.SetPrecision;
            dxRenderer.Scene.AddGeometry(interpolationBezierCurveC2);
        }

        public void AddGeometry(GeometryType type, IGeometryObject geometryObject, bool buttonAdd = false)
        {
            switch (type)
            {
                case GeometryType.Torus:
                    {
                        Torus torus = geometryObject as Torus;
                        if(buttonAdd)
                        {
                            torus.TranslationV = new MyVector3(dxRenderer.Scene.Cursor.TranslationV.X,
                                dxRenderer.Scene.Cursor.TranslationV.Y,
                                dxRenderer.Scene.Cursor.TranslationV.Z);
                        }
                        dxRenderer.Scene.AddGeometry(torus);
                        HierarchyList.ItemsSource = dxRenderer.Scene.AllGeometries;
                        break;
                    }
                case GeometryType.Point:
                    {
                        Model.Point point = geometryObject as Model.Point;
                        if(buttonAdd)
                        {
                            point.TranslationV = new MyVector3(dxRenderer.Scene.Cursor.TranslationV.X,
                                dxRenderer.Scene.Cursor.TranslationV.Y,
                                dxRenderer.Scene.Cursor.TranslationV.Z);
                        }
                        dxRenderer.Scene.AddGeometry(point);
                        if (HierarchyList.SelectedItems.Count == 1 && HierarchyList.SelectedItem is ICurve pointsBased)
                        {
                            pointsBased.AddNode(point);
                        }
                        HierarchyList.ItemsSource = dxRenderer.Scene.AllGeometries;
                        break;
                    }
                case GeometryType.BezierCurveC0:
                    {
                        List<Model.Point> pointsForBezier = new List<Model.Point>();
                        if(buttonAdd)
                        {
                            foreach (IGeometryObject g in HierarchyList.SelectedItems)
                            {
                                if (g is Model.Point node)
                                {
                                    pointsForBezier.Add(node);
                                }
                                else
                                {
                                    MessageBox.Show("Only points can be selected to create this object", "Information");
                                    return;
                                }
                            }
                            if (pointsForBezier.Count == 0)
                            {
                                MessageBox.Show("Select at least one point", "Information");
                                return;
                            }
                        }
                        
                        BezierCurveC0 bezierCurveC0 = geometryObject as BezierCurveC0;
                        AddBezierCurveC0(bezierCurveC0, pointsForBezier);
                        HierarchyList.ItemsSource = dxRenderer.Scene.AllGeometries;
                        break;
                    }
                case GeometryType.BezierCurveC2:
                    {
                        List<Model.Point> pointsForBezier = new List<Model.Point>();
                        if (buttonAdd)
                        {
                            foreach (IGeometryObject g in HierarchyList.SelectedItems)
                            {
                                if (g is Model.Point node)
                                {
                                    pointsForBezier.Add(node);
                                }
                                else
                                {
                                    MessageBox.Show("Only points can be selected to create this object", "Information");
                                    return;
                                }
                            }
                            if (pointsForBezier.Count == 0)
                            {
                                MessageBox.Show("Select at least one point", "Information");
                                return;
                            }
                        }  
                        BezierCurveC2 bezierCurveC2 = geometryObject as BezierCurveC2;
                        AddBezierCurveC2(bezierCurveC2, pointsForBezier);
                        HierarchyList.ItemsSource = dxRenderer.Scene.AllGeometries;
                        break;
                    }
                case GeometryType.InterpolationBezierCurveC2:
                    {
                        List<Model.Point> pointsForBezier = new List<Model.Point>();
                        if (buttonAdd)
                        {
                            foreach (IGeometryObject g in HierarchyList.SelectedItems)
                            {
                                if (g is Model.Point node)
                                {
                                    pointsForBezier.Add(node);
                                }
                                else
                                {
                                    MessageBox.Show("Only points can be selected to create this object", "Information");
                                    return;
                                }
                            }
                            if (pointsForBezier.Count == 0)
                            {
                                MessageBox.Show("Select at least one point", "Information");
                                return;
                            }
                        }  
                        InterpolationBezierCurveC2 interpolationBezierCurveC2 = geometryObject as InterpolationBezierCurveC2;
                        AddInterpolationBezierCurveC2(interpolationBezierCurveC2, pointsForBezier);
                        HierarchyList.ItemsSource = dxRenderer.Scene.AllGeometries;
                        break;
                    }
                case GeometryType.BicubicBezierPatchC0:
                    {
                        BicubicBezierPatchC0 bicubicBezierPatch = geometryObject as BicubicBezierPatchC0;
                        if(buttonAdd)
                        {
                            bicubicBezierPatch.SetNodes(
                                bicubicBezierPatch.GenerateNodes(dxRenderer.Scene.Cursor.CurrentPosition)
                            );
                        }
                        dxRenderer.Scene.AddGeometry(bicubicBezierPatch);
                        if(buttonAdd)
                        {
                            foreach (Model.Point p in bicubicBezierPatch.Nodes)
                            {
                                dxRenderer.Scene.AddGeometry(p);
                            }
                        }
                        HierarchyList.ItemsSource = dxRenderer.Scene.AllGeometries;
                        break;
                    }
                case GeometryType.BicubicBsplinePatchC2:
                    {
                        BicubicBsplinePatchC2 bicubicBezierPatch = geometryObject as BicubicBsplinePatchC2;
                        if (buttonAdd)
                        {
                            bicubicBezierPatch.SetNodes(
                                bicubicBezierPatch.GenerateNodes(dxRenderer.Scene.Cursor.CurrentPosition)
                            );
                        }
                        dxRenderer.Scene.AddGeometry(bicubicBezierPatch);
                        if (buttonAdd)
                        {
                            foreach (Model.Point p in bicubicBezierPatch.Nodes)
                            {
                                dxRenderer.Scene.AddGeometry(p);
                            }
                        }
                        HierarchyList.ItemsSource = dxRenderer.Scene.AllGeometries;
                        break;
                    }
            }
        }

        public delegate void GeometriesRemoved(IGeometryObject[] removedGeometries);
        public event GeometriesRemoved OnGeometriesRemoved;

        private void AddType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GeometryType type = (GeometryType)Enum.Parse(typeof(GeometryType), (AddType.SelectedItem as ComboBoxItem).Content.ToString());
            dxRenderer.UpdateObjectToAdd(type);
            dxRenderer.ObjectToAdd.GetViewModel().VisitToAdd(this);
        }
        private void AddObject_Click(object sender, RoutedEventArgs e)
        {
            GeometryType type = (GeometryType)Enum.Parse(typeof(GeometryType), (AddType.SelectedItem as ComboBoxItem).Content.ToString());
            AddGeometry(type, dxRenderer.ObjectToAdd.Copy(), true);
            OnSceneDisplayChanged?.Invoke(dxRenderer.Scene.Camera);
        }
        private void DelGeometryBtn_Click(object sender, RoutedEventArgs e)
        {
            IGeometryObject[] geometriesToDelete = new IGeometryObject[HierarchyList.SelectedItems.Count];
            HierarchyList.SelectedItems.CopyTo(geometriesToDelete, 0);
            List<IGeometryObject> allToDelete = new List<IGeometryObject>();
            foreach (IGeometryObject geometry in geometriesToDelete)
            {
                allToDelete.AddRange(geometry.Remove(dxRenderer.Scene));
            }
            OnGeometriesRemoved?.Invoke(allToDelete.ToArray());
            if (currentViewModel != null && !dxRenderer.Scene.AllGeometries.Contains(currentViewModel.SelectedObject))
            {
                dxRenderer.IsDocked = false;
            }
            HierarchyList.ItemsSource = dxRenderer.Scene.AllGeometries;
            dxRenderer.Scene.UpdateCenterOfSelection();
            OnSceneDisplayChanged?.Invoke(dxRenderer.Scene.Camera);
        }
        private void PointBasedGeometryEmpty(IGeometryObject p)
        {
            dxRenderer.Scene.Geometries.Remove(p);
            HierarchyList.ItemsSource = dxRenderer.Scene.AllGeometries;
            UpdatePanel();
        }
        #endregion

        #region IGeometryPanel interface
        public void Accept(TorusViewModel torus)
        {
            ObjectPanel.Children.Clear();
            TorusPanel torusPanel = new TorusPanel(torus);
            ObjectPanel.Children.Add(torusPanel);
            Transform.DataContext = currentViewModel;

            Transform.Translation.IsEnabled = true;
            Transform.Rotation.IsEnabled = true;
            Transform.Scale.IsEnabled = true;
        }
        public void Accept(PointViewModel point)
        {
            ObjectPanel.Children.Clear();
            Transform.DataContext = currentViewModel;

            Transform.Translation.IsEnabled = true;
            Transform.Rotation.IsEnabled = false;
            Transform.Scale.IsEnabled = false;
        }
        public void Accept(BezierCurveC0ViewModel curve)
        {
            ObjectPanel.Children.Clear();
            BezierCurveC0Panel bezierCurveC0Panel = new BezierCurveC0Panel(curve, HierarchyList.SelectedItems);
            ObjectPanel.Children.Add(bezierCurveC0Panel);
            Transform.DataContext = currentViewModel;
            Transform.Translation.IsEnabled = false;
            Transform.Rotation.IsEnabled = false;
            Transform.Scale.IsEnabled = false;
        }
        public void Accept(BezierCurveC2ViewModel curve)
        {
            ObjectPanel.Children.Clear();
            BezierCurveC2Panel bezierCurveC2Panel = new BezierCurveC2Panel(curve, HierarchyList.SelectedItems);
            ObjectPanel.Children.Add(bezierCurveC2Panel);
            Transform.DataContext = currentViewModel;
            Transform.Translation.IsEnabled = false;
            Transform.Rotation.IsEnabled = false;
            Transform.Scale.IsEnabled = false;
        }
        public void Accept(InterpolationBezierCurveC2ViewModel curve)
        {
            ObjectPanel.Children.Clear();
            InterpolationBezierCurveC2Panel bezierCurveC2Panel = new InterpolationBezierCurveC2Panel(curve, HierarchyList.SelectedItems);
            ObjectPanel.Children.Add(bezierCurveC2Panel);
            Transform.DataContext = currentViewModel;
            Transform.Translation.IsEnabled = false;
            Transform.Rotation.IsEnabled = false;
            Transform.Scale.IsEnabled = false;
        }
        public void Accept(PatchViewModel surface)
        {
            ObjectPanel.Children.Clear();
            BicubicBezierPatchC0Panel bicubicBezierPatchC0Panel = new BicubicBezierPatchC0Panel(surface);
            ObjectPanel.Children.Add(bicubicBezierPatchC0Panel);
            Transform.DataContext = currentViewModel;
            Transform.Translation.IsEnabled = false;
            Transform.Rotation.IsEnabled = false;
            Transform.Scale.IsEnabled = false;
        }

        public void Accept(GregoryPatchViewModel surface)
        {
            ObjectPanel.Children.Clear();
            GregoryPatchPanel bicubicBezierPatchC0Panel = new GregoryPatchPanel(surface);
            ObjectPanel.Children.Add(bicubicBezierPatchC0Panel);
            Transform.DataContext = currentViewModel;
            Transform.Translation.IsEnabled = false;
            Transform.Rotation.IsEnabled = false;
            Transform.Scale.IsEnabled = false;
        }
        #endregion

        #region IGeometryAddPanel interface

        public void AcceptToAdd(TorusViewModel torus)
        {
            ObjectToAddPanel.Children.Clear();
        }

        public void AcceptToAdd(PointViewModel point)
        {
            ObjectToAddPanel.Children.Clear();
        }

        public void AcceptToAdd(BezierCurveC0ViewModel curve)
        {
            ObjectToAddPanel.Children.Clear();
        }

        public void AcceptToAdd(BezierCurveC2ViewModel curve)
        {
            ObjectToAddPanel.Children.Clear();
        }

        public void AcceptToAdd(InterpolationBezierCurveC2ViewModel curve)
        {
            ObjectToAddPanel.Children.Clear();
        }

        public void AcceptToAdd(PatchViewModel surface)
        {
            ObjectToAddPanel.Children.Clear();
            BicubicBezierPatchC0AddPanel bicubicBezierPatchC0Panel = new BicubicBezierPatchC0AddPanel(surface);
            ObjectToAddPanel.Children.Add(bicubicBezierPatchC0Panel);
        }

        #endregion

        #region Dock
        private void Dock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            dxRenderer.IsDocked = !dxRenderer.IsDocked;
            if(!dxRenderer.IsDocked)
            {
                UpdatePanel();
            }
        }
        private void UpdatePanel()
        {
            if (HierarchyList.SelectedItems.Count == 1)
            {
                currentViewModel = (HierarchyList.SelectedItem as IGeometryObject).GetViewModel();
                currentViewModel.Visit(this);
                Transform.Visibility = Visibility.Visible;
            }
            else if (HierarchyList.SelectedItems.Count == 0)
            {
                Transform.Visibility = Visibility.Hidden;
                ObjectPanel.Children.Clear();
            }
            else
            {
                currentViewModel = dxRenderer.Scene.CenterOfSelection?.GetViewModel();
                currentViewModel?.Visit(this);
                Transform.Visibility = Visibility.Visible;
                Transform.Translation.IsEnabled = false;
                Transform.Rotation.IsEnabled = false;
                Transform.Scale.IsEnabled = false;
            }
        }
        #endregion

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(Mode == Mode.None)
            {
                if (e.Key == Key.R)
                {
                    Mode = Mode.Rotation;
                }
                else if (e.Key == Key.G)
                {
                    Mode = Mode.Location;
                }
                else if (e.Key == Key.S)
                {
                    Mode = Mode.Scale;
                }
                else if (e.Key == Key.B)
                {
                    Mode = Mode.BoxSelection;
                }
                else
                {
                    Mode = Mode.None;
                }
            }
            else if(Mode == Mode.Rotation || Mode == Mode.Location || Mode == Mode.Scale)
            {
                if(e.Key != Key.X && e.Key != Key.Y && e.Key != Key.Z)
                {
                    Mode = Mode.None;
                }
            }
            else
            {
                Mode = Mode.None;
            }
        }

        #region Serialization
        private void SaveScene_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML Files|*.xml";
            if (saveFileDialog.ShowDialog() == true)
            {
                sceneSerializer.Serialize(dxRenderer.Scene, saveFileDialog.FileName);
            }
        }

        private void LoadScene_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\..\\");
            openFileDlg.InitialDirectory = System.IO.Path.GetFullPath(path);
            openFileDlg.Multiselect = false;
            openFileDlg.Filter = "XML Files|*.xml";
            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result == true)
            {
                sceneSerializer.Deserialize(dxRenderer.Scene, this, openFileDlg.FileName);
                HierarchyList.ItemsSource = dxRenderer.Scene.AllGeometries;
            }

        }
        #endregion

        private void MergePointsItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (HierarchyList.SelectedItems.Count == 2)
                {
                    if (HierarchyList.SelectedItems[0] is Model.Point p1)
                    {
                        if (HierarchyList.SelectedItems[1] is Model.Point p2)
                        {
                            IPointBased[] copy = new IPointBased[p1.PointBased.Count];
                            p1.PointBased.CopyTo(copy);
                            foreach (IPointBased pointBased in copy)
                            {
                                pointBased.SwapPoint(p1, p2);
                            }
                            Func<float, float, float> avg = delegate (float x, float y)
                            {
                                return (x + y) / 2.0f;
                            };
                            p2.SetPosition(new Vector3(
                                avg(p1.TranslationV.X, p2.TranslationV.X),
                                avg(p1.TranslationV.Y, p2.TranslationV.Y),
                                avg(p1.TranslationV.Z, p2.TranslationV.Z)));
                            p1.IsRemovable = true;
                            p1.Remove(dxRenderer.Scene);
                            HierarchyList.ItemsSource = dxRenderer.Scene.AllGeometries;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                //TODO: message box
            }
            
        }

        private void FillInItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (HierarchyList.SelectedItems.Count == 3)
                {
                    if (HierarchyList.SelectedItems[0] is Model.Point p1)
                    {
                        if (HierarchyList.SelectedItems[1] is Model.Point p2)
                        {
                            if (HierarchyList.SelectedItems[2] is Model.Point p3)
                            {
                                var corners = new List<Model.Point> { p1, p2, p3 };
                                var patches = GregoryPatch.CreateFillInPatch(corners);
                                if (patches == null)
                                {
                                    return;
                                }
                                foreach (var p in patches)
                                {
                                    dxRenderer.Scene.AddGeometry(p);
                                }
                                HierarchyList.ItemsSource = dxRenderer.Scene.AllGeometries;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                //TODO: message box
            }
            
            
        }
    }
}
