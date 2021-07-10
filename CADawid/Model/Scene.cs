using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using CADawid.DxModule;
using CADawid.Utils;
using SharpDX;

namespace CADawid.Model
{
    public class Scene : INotifyPropertyChanged
    {
        public ObservableCollection<IGeometryObject> AllGeometries
        {
            get
            {
                ObservableCollection<IGeometryObject> compositeCollection = new ObservableCollection<IGeometryObject>(Points);
                foreach(IGeometryObject g in Geometries)
                {
                    compositeCollection.Add(g);
                }
                return compositeCollection;
            }
        }
        public ObservableCollection<IGeometryObject> Geometries { get; set; }

        public ObservableCollection<IGeometryObject> VirtualPoints
        {
            get
            {
                ObservableCollection<IGeometryObject> compositeCollection = new ObservableCollection<IGeometryObject>();
                foreach (KeyValuePair<IGeometryObject, List<Point>> virtualPoints in virtualPoints)
                {
                    foreach(IGeometryObject virtualPoint in virtualPoints.Value)
                    {
                        compositeCollection.Add(virtualPoint);
                    }
                }
                return compositeCollection;
            }
        }
        public ObservableCollection<IGeometryObject> Points { get; set; }
        private Dictionary<IGeometryObject, List<Point>> virtualPoints { get; set; }

        public List<IGeometryObject> SelectedObjects { get; set; }

        public List<IGeometryObject> ConstantGeometries { get; set; }
        public IGeometryObject CenterOfSelection { get; set; }
        public IGeometryObject Cursor { get; set; }
        public int currentSelectionCount = 0;
        public DxCamera Camera { get; set; }

        #region ScreenWorldPosition
        public IGeometryObject ScreenWorldObject { get; set; }

        public float WorldPositionX
        {
            get => ScreenWorldObject.TranslationV.X;
            set
            {
                ScreenWorldObject.TranslationV.X = value;
                NotifyPropertyChanged(nameof(WorldPositionX));
                UpdatePositions();
            }
        }
        public float WorldPositionY
        {
            get => ScreenWorldObject.TranslationV.Y;
            set
            {
                ScreenWorldObject.TranslationV.Y = value;
                NotifyPropertyChanged(nameof(WorldPositionY));
                UpdatePositions();
            }
        }
        public float WorldPositionZ
        {
            get => ScreenWorldObject.TranslationV.Z;
            set
            {
                ScreenWorldObject.TranslationV.Z = value;
                NotifyPropertyChanged(nameof(WorldPositionZ));
                UpdatePositions();
            }
        }
        public MyVector3 ScreenPosition;
        public float ScreenPositionX
        {
            get => ScreenPosition == null ? 0 : ScreenPosition.X;
            set
            {
                ScreenPosition.X = value;
                NotifyPropertyChanged(nameof(ScreenPositionX));
                UpdateWorldPosition();
            }
        }
        public float ScreenPositionY
        {
            get => ScreenPosition == null ? 0 : ScreenPosition.Y;
            set
            {
                ScreenPosition.Y = value;
                NotifyPropertyChanged(nameof(ScreenPositionY));
                UpdateWorldPosition();
            }
        }

        public void UpdatePositions()
        {
            Camera.UpdateVPMatrix();
            UpdateScreenPosition();
            NotifyPropertyChanged(nameof(WorldPositionX));
            NotifyPropertyChanged(nameof(WorldPositionY));
            NotifyPropertyChanged(nameof(WorldPositionZ));
        }
        private void UpdateScreenPosition()
        {
            ScreenPosition = ScreenWorldObject.GetScreenPosition(Camera);
            NotifyPropertyChanged(nameof(ScreenPositionX));
            NotifyPropertyChanged(nameof(ScreenPositionY));
        }
        private void UpdateWorldPosition()
        {
            Vector4 worldPos = Camera.ScreenToWorld(ScreenPosition.X,
                   ScreenPosition.Y, ScreenPosition.Z, false);
            ScreenWorldObject.TranslationV = new Vector3(worldPos.X / worldPos.W,
                worldPos.Y / worldPos.W, worldPos.Z / worldPos.W);
            NotifyPropertyChanged(nameof(WorldPositionX));
            NotifyPropertyChanged(nameof(WorldPositionY));
            NotifyPropertyChanged(nameof(WorldPositionZ));
        }
        #endregion

        public Scene()
        {
            Geometries = new ObservableCollection<IGeometryObject>();
            Points = new ObservableCollection<IGeometryObject>();
            virtualPoints = new Dictionary<IGeometryObject, List<Point>>();
            ConstantGeometries = new List<IGeometryObject>();
            SelectedObjects = new List<IGeometryObject>();

            Camera = new DxCamera();

            ConstantGeometries.Add(new Plane(200, 200, 200, 200, Matrix.Identity,
                new Vector4(72f / 255f, 73f / 255f, 84f / 255f, 1), new Vector4(72f / 255f, 73f / 255f, 84f / 255f, 0.3f)));
            float axisPoint = -100f;
            ConstantGeometries.Add(new Segment(new Vertex(-axisPoint, 0, 0, 1), new Vertex(axisPoint, 0, 0, 1), Matrix.Identity, 
                new Vector4(0.5f, 0, 0, 1), new Vector4(0.5f, 0, 0, 0.3f)));
            ConstantGeometries.Add(new Segment(new Vertex(0, -axisPoint, 0, 1), new Vertex(0, axisPoint, 0, 1), Matrix.Identity, 
                new Vector4(0, 0.5f, 0, 1), new Vector4(0, 0.5f, 0, 0.3f)));
            ConstantGeometries.Add(new Segment(new Vertex(0, 0, -axisPoint, 1), new Vertex(0, 0, axisPoint, 1), Matrix.Identity, 
                new Vector4(0, 0, 0.5f, 1), new Vector4(0, 0, 0.5f, 0.3f)));

            Cursor = new Cursor(0.7f, new Vector4(1f, 0f, 0.95f, 1f), new Vector4(1f, 0f, 0.3f, 1f));
            ScreenWorldObject = Cursor;
        }
        public void UpdateCenterOfSelection()
        {
            Vector3 center = new Vector3(0f);
            int counter = 0;
            foreach (IGeometryObject geometry in AllGeometries)
            {
                if (geometry.IsSelected)
                {
                    center += geometry.CurrentPosition;
                    counter++;
                }
            }
            currentSelectionCount = counter;
            if (counter == 0)
            {
                CenterOfSelection = null;
                return;
            }

            if(CenterOfSelection == null)
            {
                Model.Point point = new Model.Point(0.3f, new Vector4(0, 0.8f, 0, 0.6f), new Vector4(0, 0.8f, 0, 0.3f));
                point.TranslationV = center / (float)counter;
                CenterOfSelection = point;
            }
            else
            {
                CenterOfSelection.TranslationV = center / (float)counter;
            }

        }
        private void GeometryTransformed(IGeometryObject transformedObject)
        {
            if(transformedObject.IsSelected)
            {
                UpdateCenterOfSelection();
            }
        }
        private void GeometrySelectionChanged(IGeometryObject geometryObject)
        {
            UpdateCenterOfSelection();
        }

        public void UpdateVirtualPoints(IGeometryObject pointBased, List<Point> virtualPoints)
        {
            if(virtualPoints == null)
            {
                this.virtualPoints.Remove(pointBased);
                return;
            }
            if(this.virtualPoints.ContainsKey(pointBased))
            {
                this.virtualPoints[pointBased].Clear();
                this.virtualPoints[pointBased].AddRange(virtualPoints);
            }
            else
            {
                this.virtualPoints.Add(pointBased, virtualPoints);
            }
        }

        public void AddGeometry(IGeometryObject geometry)
        {
            geometry.OnTransformed += GeometryTransformed;
            geometry.OnSelectionChanged += GeometrySelectionChanged;
            if (geometry is Point)
            {
                Points.Add(geometry);
            }
            else
            {
                Geometries.Add(geometry);
            }
        }
        public void RemoveGeometry(IGeometryObject geometry)
        {
            if(!geometry.IsRemovable)
            {
                return;
            }
            if (geometry is Point)
            {
                Points.Remove(geometry);
            }
            else
            {
                Geometries.Remove(geometry);
            }
        }
        public void ClearSelection()
        {
            SelectedObjects.ForEach(p => p.IsSelected = false);
            SelectedObjects.Clear();
        }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
