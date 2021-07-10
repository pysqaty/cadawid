using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CADawid.Utils
{
    public class MyVector3 : INotifyPropertyChanged
    {
        private SharpDX.Vector3 vec3;

        public float X
        {
            get => vec3.X;
            set
            {
                vec3 = new SharpDX.Vector3(value, vec3.Y, vec3.Z);
                NotifyPropertyChanged("X");
            }
        }

        public float Y
        {
            get => vec3.Y;
            set
            {
                vec3 = new SharpDX.Vector3(vec3.X, value, vec3.Z);
                NotifyPropertyChanged("Y");
            }
        }

        public float Z
        {
            get => vec3.Z;
            set
            {
                vec3 = new SharpDX.Vector3(vec3.X, vec3.Y, value);
                NotifyPropertyChanged("Z");
            }
        }

        public MyVector3(SharpDX.Vector3 v3)
        {
            vec3 = v3;
        }

        public MyVector3(float x, float y, float z)
        {
            vec3 = new SharpDX.Vector3(x, y, z);
        }

        public static implicit operator SharpDX.Vector3(MyVector3 myVector3) => myVector3.vec3;
        public static implicit operator MyVector3(SharpDX.Vector3 vector3) => new MyVector3(vector3);

        public static MyVector3 operator *(MyVector3 vector3, float val) => new MyVector3(val * vector3.vec3);
        public static MyVector3 operator *(float val, MyVector3 vector3) => new MyVector3(val * vector3.vec3);
        public static MyVector3 operator -(MyVector3 a, MyVector3 b) => new MyVector3(a.vec3 - b.vec3);

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
