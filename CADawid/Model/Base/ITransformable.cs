using CADawid.Utils;
using SharpDX;

namespace CADawid.Model
{
    public interface ITransformable
    {
        MyVector3 TranslationV { get; set; }
        MyVector3 ScaleV { get; set; }
        MyVector3 RotationV { get; set; }

        void Translate(Vector3 translation);
        void Scale(Vector3 scale, Vector3 pivot);
        void ScaleLocally(Vector3 scale);
        void Rotate(Vector3 rotation, Vector3 pivot);
        void RotateLocally(Vector3 rotation);
    }
}
