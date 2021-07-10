using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CADawid.DxModule;
using CADawid.Serialization.Model;
using CADawid.Utils;
using CADawid.ViewModel;
using SharpDX;

namespace CADawid.Model
{
    public delegate void Transformed(IGeometryObject transformedObject);
    public delegate void SelectionChanged(IGeometryObject geometryObject);
    public interface IGeometryObject : ITransformable
    {
        string Name { get; set; }
        Matrix CurrentModel { get; }
        Vector3 CurrentPosition { get; }
        bool IsSelected { get; set; }
        bool IsRemovable { get; set; }
        void NotifyTransformed();
        void ResetGeometry();
        MyVector3 GetScreenPosition(DxCamera camera);
        GeometryViewModel GetViewModel();
        SerializationModel GetSerializationModel();
        IGeometryObject Copy();
        List<IGeometryObject> Remove(Scene scene);
        void Render(DxRenderer dxRenderer, Matrix worldTransform, bool isSelected, Func<Vector4, Vector4> modifier);

        event Transformed OnTransformed;
        event SelectionChanged OnSelectionChanged;
    }
}
