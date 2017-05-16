/* 
 Licensed under the Apache License, Version 2.0

 http://www.apache.org/licenses/LICENSE-2.0
 */
using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Xml2CSharp
{
    [XmlRoot(ElementName = "point")]
    public class Point
    {
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
    }

    [XmlRoot(ElementName = "points")]
    public class Points
    {
        [XmlElement(ElementName = "point")]
        public List<Point> Point { get; set; }
    }

    [XmlRoot(ElementName = "filledpolygon")]
    public class Filledpolygon
    {
        [XmlElement(ElementName = "points")]
        public Points Points { get; set; }
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
        [XmlAttribute(AttributeName = "fillcolor")]
        public string Fillcolor { get; set; }
    }

    [XmlRoot(ElementName = "shape")]
    public class Shape
    {
        [XmlElement(ElementName = "filledpolygon")]
        public Filledpolygon Filledpolygon { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "ellipse")]
        public Ellipse Ellipse { get; set; }
    }

    [XmlRoot(ElementName = "ellipse")]
    public class Ellipse
    {
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
        [XmlAttribute(AttributeName = "fillcolor")]
        public string Fillcolor { get; set; }
    }

    [XmlRoot(ElementName = "group")]
    public class Group
    {
        [XmlElement(ElementName = "shape")]
        public List<Shape> Shape { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "groups")]
    public class Groups
    {
        [XmlElement(ElementName = "group")]
        public List<Group> Group { get; set; }
    }

}
