using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace FDesigner
{
    [XmlRoot(ElementName = "point")]
    public class PointDef
    {
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
    }

    [XmlRoot(ElementName = "points")]
    public class PointsDef
    {
        [XmlElement(ElementName = "point")]
        public List<PointDef> Point { get; set; }
    }

    [XmlRoot(ElementName = "filledpolygon")]
    public class FilledPolygonDef
    {
        [XmlElement(ElementName = "points")]
        public PointsDef Points { get; set; }
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
        [XmlAttribute(AttributeName = "fillcolor")]
        public string Fillcolor { get; set; }
    }

    [XmlRoot(ElementName = "shape")]
    public class ShapeDef
    {
        [XmlElement(ElementName = "filledpolygon")]
        public FilledPolygonDef Filledpolygon { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "ellipse")]
        public EllipseDef Ellipse { get; set; }
    }

    [XmlRoot(ElementName = "ellipse")]
    public class EllipseDef
    {
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
        [XmlAttribute(AttributeName = "fillcolor")]
        public string Fillcolor { get; set; }
    }

    [XmlRoot(ElementName = "group")]
    public class GroupDef
    {
        [XmlElement(ElementName = "shape")]
        public List<ShapeDef> Shape { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "groups")]
    public class GroupsDef
    {
        [XmlElement(ElementName = "group")]
        public List<GroupDef> Group { get; set; }
    }

}
