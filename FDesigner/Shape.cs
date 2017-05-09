using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDesigner
{
    public class Shape
    {
        public enum ShapeType
        {
            NONE,
            BOX,
            DIAMOND,
            CIRCLE,
            TRIANGLE,
        }

        public int x1 = 0;
        public int y1 = 0;
        private ShapeType type;

        public int x2 { get { return x1 + bitmap.Width;  } }
        public int y2 { get { return y1 + bitmap.Height; } }

        public int z;
        public Bitmap bitmap;
        public Bitmap buffer;
        public bool selected = false;

        public Point CenterPoint
        {
            get
            {
                Point p = new Point();

                p.X = bitmap.Width / 2;
                p.Y = bitmap.Height / 2;

                return p;
            }
        }

        public ShapeType Type
        {
            get { return type; }
        }

        public Rectangle rect
        {
            get { return new Rectangle(x1, y1, bitmap.Width, bitmap.Height);  }
        }

        public Shape()
        {

        }

        public Shape(ShapeType type, int x, int y, int width, int height)
        {
            SolidBrush whiteBrush = new SolidBrush(Color.White);
            SolidBrush fillBrush = new SolidBrush(Color.WhiteSmoke);
            Pen pen = new Pen(Color.Black, 1);

            x1 = x;
            y1 = y;
            bitmap = new Bitmap(width, height);
            buffer = (Bitmap)bitmap.Clone();
            this.type = type;

            using (Graphics g = Graphics.FromImage(this.bitmap))
            {
                int h = this.bitmap.Height - 1;
                int w = this.bitmap.Width - 1;

                switch (type)
                {
                    case ShapeType.BOX:
                        {
                            g.FillRectangle(fillBrush, 0, 0, w, h);
                            g.DrawRectangle(pen, 0, 0, w, h);
                            break;
                        }

                    case ShapeType.DIAMOND:
                        {
                            Point[] points = new Point[4];
                            points[0] = new Point(0, h / 2);
                            points[1] = new Point(w / 2, 0);
                            points[2] = new Point(w, h / 2);
                            points[3] = new Point(w / 2, h);
                            g.FillPolygon(fillBrush, points);
                            g.DrawPolygon(pen, points);
                            break;
                        }

                    case ShapeType.CIRCLE:
                        {
                            g.FillEllipse(fillBrush, 0, 0, w, h);
                            g.DrawEllipse(pen, 0, 0, w, h);
                            break;
                        }

                    case ShapeType.TRIANGLE:
                        {
                            Point[] points = new Point[3];
                            points[0] = new Point(0, h);
                            points[1] = new Point(w / 2, 0);
                            points[2] = new Point(w, h);
                            g.FillPolygon(fillBrush, points);
                            g.DrawPolygon(pen, points);
                            break;
                        }

                }
            }
        }

        public Shape Clone()
        {
            Shape newShape = new Shape();

            newShape.x1 = x1;
            newShape.y1 = y1;
            newShape.type = type;
            newShape.bitmap = (Bitmap)this.bitmap.Clone();
            newShape.buffer = (Bitmap)this.buffer.Clone();

            return newShape;
        }

    }
}
