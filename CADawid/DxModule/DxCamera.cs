using System;
using System.ComponentModel;
using CADawid.Utils;
using SharpDX;

namespace CADawid.DxModule
{
    public class DxCamera : Model.ITransformable, INotifyPropertyChanged
    {
        #region Matrices / Parameters
        private int height = 0;
        private int width = 0;
        private float aspectRatio => width / (float)height;
        private float fov = (float)Math.PI / 4f;
        private float far = 100.0f;
        private float near = 0.01f;
        private float f = 50.01f;
        private float d = 3f;

        public float F
        {
            get => f;
            set
            {
                f = value;
                UpdateProjectionMatrices();
                NotifyPropertyChanged("F");
            }
        }
        public float D
        {
            get => d;
            set
            {
                d = value;
                UpdateProjectionMatrices();
                NotifyPropertyChanged("D");
            }
        }
        public float Far
        {
            get => far;
            set
            {
                far = value;
                UpdateProjectionMatrices();
                NotifyPropertyChanged("Far");
            }
        }
        public float Near
        {
            get => near;
            set
            {
                near = value;
                UpdateProjectionMatrices();
                NotifyPropertyChanged("Near");
            }
        }
        public int Width
        {
            get => width;
            set
            {
                width = value;
                UpdateProjectionMatrices();
                NotifyPropertyChanged("Width");
            }
        }
        public int Height
        {
            get => height;
            set
            {
                height = value;
                UpdateProjectionMatrices();
                NotifyPropertyChanged("Height");
            }
        }

        public bool isStereoscopyEnabled;

        public bool IsStereoscopyEnabled
        {
            get => isStereoscopyEnabled;
            set
            {
                isStereoscopyEnabled = value;
                NotifyPropertyChanged(nameof(IsStereoscopyEnabled));
            }
        }
        public bool IsLeftEye = false;



        public Matrix ViewMatrix;
        public Matrix ProjectionMatrix;
        public Matrix ProjectionMatrix_LE;
        public Matrix ProjectionMatrix_RE;
        public Matrix VP;
        public Matrix Ry => Matrix.RotationY(RotationV.Y);
        public Matrix Rx => Matrix.RotationX(RotationV.X);
        public Matrix S => Matrix.Scaling(ScaleV);
        public Matrix T => Matrix.Translation(TranslationV);
        public Matrix R => Ry * Rx;

        public Vector3 Eye;
        public Vector3 At;
        public Vector3 Up;
        #endregion

        public DxCamera()
        {
            Reset();
        }
        public void Reset()
        {
            height = 0;
            width = 0;
            ViewMatrix = Matrix.Identity;
            ProjectionMatrix = Matrix.Identity;
            Eye = new Vector3(0.0f, 0.0f, -20.0f);
            At = new Vector3(0.0f, 0.0f, 0.0f);
            Up = new Vector3(0.0f, 1.0f, 0.0f);

            ScaleV = new Vector3(1f, 1f, 1f);
            RotationV = new Vector3(0f, 0f, 0f);
            TranslationV = new Vector3(0f, 0f, 0f);

            Update();
        }
        public void SetUpViewPort(SharpDX.Direct3D11.Device device)
        {
            SharpDX.Mathematics.Interop.RawViewportF vp = new SharpDX.Mathematics.Interop.RawViewportF();
            vp.Width = width;
            vp.Height = height;
            vp.MinDepth = 0f;
            vp.MaxDepth = 1f;
            vp.X = 0;
            vp.Y = 0;
            device.ImmediateContext.Rasterizer.SetViewport(vp);

            UpdateProjectionMatrices();
            Update();
        }

        public void UpdateProjectionMatrices()
        {
            float offsetX_R = d / 2.0f;
            float offsetX_L = -d / 2.0f;

            float t = far * (float)Math.Tan(fov / 2.0f);
            float b = -t;

            float rFarBasic = t * aspectRatio;
            float lFarBasic = -rFarBasic;
            float r = f * rFarBasic / Far;
            float l = -r;

            float rFar_R = far * (r - offsetX_R) / f;
            float lFar_R = far * (l - offsetX_R) / f;

            float rFar_L = far * (r - offsetX_L) / f;
            float lFar_L = far * (l - offsetX_L) / f;

            ProjectionMatrix = new Matrix(
                2 * far / (rFarBasic - lFarBasic), 0.0f, 0.0f, 0.0f,
                0.0f, 2 * far / (t - b), 0.0f, 0.0f,
                (rFarBasic + lFarBasic) / (rFarBasic - lFarBasic), (t + b) / (t - b), (far + near) / (far - near), 1.0f,
                0.0f, 0.0f, -2.0f * far * near / (far - near), 0.0f);

            ProjectionMatrix_RE = Matrix.Translation(offsetX_R, 0, 0) * new Matrix(
                2 * far / (rFar_R - lFar_R), 0.0f, 0.0f, 0.0f,
                0.0f, 2 * far / (t - b), 0.0f, 0.0f,
                (rFar_R + lFar_R) / (rFar_R - lFar_R), (t + b) / (t - b), (far + near) / (far - near), 1.0f,
                0.0f, 0.0f, -2.0f * far * near / (far - near), 0.0f);

            ProjectionMatrix_LE = Matrix.Translation(offsetX_L, 0, 0) * new Matrix(
                2 * far / (rFar_L - lFar_L), 0.0f, 0.0f, 0.0f,
                0.0f, 2 * far / (t - b), 0.0f, 0.0f,
                (rFar_L + lFar_L) / (rFar_L - lFar_L), (t + b) / (t - b), (far + near) / (far - near), 1.0f,
                0.0f, 0.0f, -2.0f * far * near / (far - near), 0.0f);
        }

        public Vector4 GetEyeColor(Vector4 color, bool isLeft)
        {
            if(isLeft)
            {
                return new Vector4(color.X, 0, 0, color.W);
            }
            else
            {
                return new Vector4(0, color.Y, color.Z, color.W);
            }
        }

        #region Updates
        public void Update()
        {
            Matrix.LookAtLH(ref Eye, ref At, ref Up, out ViewMatrix);
            ViewMatrix = T * R * S * ViewMatrix;
        }

        public void UpdateVPMatrix()
        {
            if(isStereoscopyEnabled)
            {
                if(IsLeftEye)
                {
                    VP = ViewMatrix * ProjectionMatrix_LE;
                }
                else
                {
                    VP = ViewMatrix * ProjectionMatrix_RE;
                }
            }
            else
            {
                VP = ViewMatrix * ProjectionMatrix;
            }
        }
        #endregion

        #region Transformations
        public const float TranslationVar = 0.02f;
        public const float ScaleVar = 0.06f;
        public const float RotateVar = 0.02f;

        public MyVector3 ScaleV { get; set; }
        public MyVector3 RotationV { get; set; }
        public MyVector3 TranslationV { get; set; }

        public void Rotate(Vector3 rotation, Vector3 pivot)
        {
            //At = pivot;
            RotationV += RotateVar * new Vector3((float)rotation.X, (float)rotation.Y, 0f);
            Update();
        }
        public void Translate(Vector3 translation)
        {
            TranslationV += TranslationVar * translation;
            Update();
        }
        public void RotateLocally(Vector3 rotation)
        {
            throw new NotImplementedException();
        }
        public void Scale(Vector3 scale, Vector3 pivot)
        {
            throw new NotImplementedException();
        }
        public void ScaleLocally(Vector3 scale)
        {
            ScaleV += new Vector3(scale.X * ScaleVar, scale.Y * ScaleVar, scale.Z * ScaleVar);
            if (ScaleV.X <= 0.01f)
            {
                ScaleV = new Vector3(0.01f, 0.01f, 0.01f);
            }
            Update();
        }
        #endregion

        #region Screen/World
        public (Vector4 from, Vector3 ray) ScreenSpaceToRay(System.Windows.Point screenCoords)
        {
            Vector4 worldPos = ScreenToWorld((float)screenCoords.X, (float)screenCoords.Y);
            Vector3 ray = new Vector3(worldPos.X, worldPos.Y, worldPos.Z);
            ray.Normalize();
            Vector4 cameraPos = Vector4.Transform(new Vector4(0, 0, 0, 1), Matrix.Invert(ViewMatrix));
            return (cameraPos, ray);
        }
        public Vector4 ScreenToWorld(float screenX, float screenY, float screenZ = 1f, bool isW0 = true)
        {
            float x = (float)(screenX / width) * 2f - 1f;
            float y = (float)(screenY / height) * 2f - 1f;
            Vector4 screenPos = new Vector4(x, -y, screenZ, 1);
            screenPos = Vector4.Transform(screenPos, Matrix.Invert(ProjectionMatrix));
            if (isW0)
            {
                screenPos = new Vector4(screenPos.X, screenPos.Y, screenPos.Z, 0);
            }
            Vector4 worldPos = Vector4.Transform(screenPos, SharpDX.Matrix.Invert(ViewMatrix));
            return worldPos;
        }
        #endregion

        #region Property changed
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
