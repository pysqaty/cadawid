
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using CADawid.Model;

namespace CADawid.Serialization.Model
{
    public abstract class PatchSerializationModel : SerializationModel
    {
        [XmlArray(ElementName = "Points")]
        [XmlArrayItem(ElementName = "PointRef")]
        public List<GridPointRef> PointsRef { get; set; }

        [XmlIgnore]
        public int uNodes { get; set; }
        [XmlIgnore]
        public int vNodes { get; set; }
        [XmlIgnore]
        protected int PatchesU { get; set; }
        [XmlIgnore]
        protected int PatchesV { get; set; }

        [XmlAttribute(AttributeName = "ColumnSlices")]
        public int PrecisionU { get; set; }
        [XmlAttribute(AttributeName = "RowSlices")] 
        public int PrecisionV { get; set; }
        [XmlAttribute(AttributeName = "ShowControlPolygon")]
        public bool DisplayBezierGrid { get; set; }

        [XmlAttribute]
        public SurfaceCylinderDirection WrapDirection { get; set; }

        public PatchSerializationModel(IGeometryObject model) : base(model) {}

        public PatchSerializationModel() { }
    }
}
