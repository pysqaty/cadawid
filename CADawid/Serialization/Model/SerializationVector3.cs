using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CADawid.Utils;

namespace CADawid.Serialization.Model
{
    public class SerializationVector3
    {
        [XmlAttribute]
        public float X { get; set; }
        [XmlAttribute]
        public float Y { get; set; }
        [XmlAttribute]
        public float Z { get; set; }

        public SerializationVector3(SharpDX.Vector3 v3)
        {
            X = v3.X;
            Y = v3.Y;
            Z = v3.Z;
        }

        public SerializationVector3() { }

        public static implicit operator SharpDX.Vector3(SerializationVector3 myVector3) => new SharpDX.Vector3(myVector3.X, myVector3.Y, myVector3.Z);
        public static implicit operator SerializationVector3(SharpDX.Vector3 vector3) => new SerializationVector3(vector3);

        public static implicit operator MyVector3(SerializationVector3 myVector3) => new MyVector3(myVector3.X, myVector3.Y, myVector3.Z);
        public static implicit operator SerializationVector3(MyVector3 vector3) => new SerializationVector3(vector3);
    }
}
