using System;
using System.ComponentModel;
using CADawid.DxModule;
using CADawid.Serialization.Model;
using CADawid.Utils;
using CADawid.ViewModel;
using SharpDX;

namespace CADawid.Model
{
    public abstract class GeometryObject<V, I> : INotifyPropertyChanged, IGeometryObject
        where V : unmanaged
        where I : unmanaged
    {
        public string Name { get; set; }
        public Matrix Model { get; set; }
        public Vector4 Color { get; set; }
        public Vector4 UnselectedColor { get; set; }
        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnSelectionChanged(this);
            }
        }
        public bool IsRemovable { get; set; }
        public GeometryObject()
        {
            Name = this.GetType().Name;
            Model = Matrix.Identity;
            Color = new Vector4(1f, 1f, 1f, 1f);

            ScaleV = new Vector3(1f, 1f, 1f);
            RotationV = new Vector3(0f, 0f, 0f);
            TranslationV = new Vector3(0f, 0f, 0f);
        }
        public MyVector3 GetScreenPosition(DxCamera camera)
        {
            Vector4 vec4 = Vector3.Transform(CurrentPosition, camera.VP);
            MyVector3 onScreen = new MyVector3(vec4.X / vec4.W, vec4.Y / vec4.W, vec4.Z / vec4.W);
            onScreen.X = (int)(((onScreen.X + 1f) / 2f) * camera.Width);
            onScreen.Y = (int)(((-onScreen.Y + 1f) / 2f) * camera.Height);
            return onScreen;
        }
        public abstract GeometryViewModel GetViewModel();
        public abstract IGeometryObject Copy();
        public virtual System.Collections.Generic.List<IGeometryObject> Remove(Scene scene)
        {
            scene.RemoveGeometry(this);
            return new System.Collections.Generic.List<IGeometryObject>() { this };
        }
        #region Buffers
        public SharpDX.Direct3D11.Buffer vertexBuffer { get; set; }
        public SharpDX.Direct3D11.Buffer indexBuffer { get; set; }
        #endregion

        #region Geometry
        protected Geometry<V, I> Geometry { get; set; }
        protected virtual Geometry<V, I> TryUpdateGeometry(DxRenderer dxRenderer)
        {
            if (Geometry.Equals(default(Geometry<V, I>)))
            {
                Geometry = GenerateGeometry();
                if(Geometry.Indices.Length != 0 && Geometry.Vertices.Length != 0)
                {
                    unsafe
                    {
                        vertexBuffer = dxRenderer.CreateVertexBuffer<V>(Geometry.Vertices, sizeof(V));
                        indexBuffer = dxRenderer.CreateIndexBuffer(Geometry.Indices, sizeof(I));
                    }
                }
            }
            return Geometry;
        }
        protected abstract Geometry<V, I> GenerateGeometry();
        public virtual void ResetGeometry()
        {
            Geometry = default(Geometry<V, I>);
        }
        #endregion

        #region Transformations

        public event Transformed OnTransformed = delegate { };

        public event SelectionChanged OnSelectionChanged = delegate { };

        public void NotifyTransformed()
        {
            OnTransformed(this);
        }

        public const float TranslationVar = 0.02f;
        public const float ScaleVar = 0.06f;
        public const float RotateVar = 0.02f;

        private MyVector3 translationV;
        public MyVector3 TranslationV
        {
            get => translationV;
            set
            {
                translationV = value;
                NotifyPropertyChanged("TranslationV");
            }
        }
        private MyVector3 scaleV;
        public MyVector3 ScaleV
        {
            get => scaleV;
            set
            {
                scaleV = value;
                NotifyPropertyChanged("ScaleV");
            }
        }
        private MyVector3 rotationV;
        public MyVector3 RotationV
        {
            get => rotationV;
            set
            {
                rotationV = value;
                NotifyPropertyChanged("RotationV");
            }
        }

        public virtual Matrix CurrentModel =>
            Matrix.Scaling(ScaleV) *
            Matrix.RotationX(RotationV.X) * Matrix.RotationY(RotationV.Y) * Matrix.RotationZ(RotationV.Z) *
            Matrix.Translation(TranslationV);

        public virtual Vector3 CurrentPosition => CurrentModel.TranslationVector;

        public virtual void Translate(Vector3 translation)
        {
            TranslationV += TranslationVar * translation;
            OnTransformed(this);
        }

        public virtual void Scale(Vector3 scale, Vector3 pivot)
        {
            if(scale.X < -10f)
            {
                scale = new Vector3(-10f);
            }
            if(scale.X > 10f)
            {
                scale = new Vector3(10f);
            }

            ScaleLocally(scale);
            Vector3 pivotToObject = CurrentPosition - pivot;
            TranslationV = pivot + MathExt.ScaleVector(new Vector4(pivotToObject, 1f), Matrix.Scaling(new Vector3(1f) + ScaleVar * scale));
            OnTransformed(this);
        }

        public virtual void ScaleLocally(Vector3 scale)
        {
            ScaleV *= new Vector3(1f) + ScaleVar * scale;
            OnTransformed(this);
        }

        public virtual void Rotate(Vector3 rotation, Vector3 pivot)
        {
            TranslationV = pivot + MathExt.RotateVector(new Vector4(CurrentPosition - pivot, 1f), Matrix.RotationX(RotateVar * rotation.X) *
                    Matrix.RotationY(RotateVar * rotation.Y) * Matrix.RotationZ(RotateVar * rotation.Z));
            OnTransformed(this);
        }

        public virtual void RotateLocally(Vector3 rotation)
        {
            RotationV += RotateVar * rotation;
            OnTransformed(this);
        }
        #endregion

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public virtual void Render(DxRenderer dxRenderer, Matrix worldTransform, bool isSelected, Func<Vector4, Vector4> modifier)
        {
            Geometry<V, I> g = TryUpdateGeometry(dxRenderer);
            if (g.Indices.Length == 0 || g.Vertices.Length == 0)
            {
                return;
            }
            unsafe
            {
                dxRenderer.SetVertexBuffer<V>(vertexBuffer, sizeof(V));
            }
            dxRenderer.SetIndexBuffer(indexBuffer);

            Matrix modelMatrix = Model * worldTransform;
            DxConstantBuffer cb = new DxConstantBuffer();
            cb.MVP = modelMatrix * dxRenderer.Scene.Camera.VP;
            cb.Color = modifier(isSelected ? Color : UnselectedColor);

            dxRenderer.UpdateConstantBuffer(ref dxRenderer.constantBuffer, cb);

            dxRenderer.device.ImmediateContext.DrawIndexed(g.Indices.Length, 0, 0);
        }

        public abstract SerializationModel GetSerializationModel();
    }
}
