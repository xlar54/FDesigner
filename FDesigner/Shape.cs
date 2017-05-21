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
        public int x1 = 0;
        public int y1 = 0;

        public ShapeDef ShapeDef;

        public int x2 { get { return x1 + bitmap.Width;  } }
        public int y2 { get { return y1 + bitmap.Height; } }

        public int z;
        public Bitmap bitmap;
        public Bitmap buffer;
        public bool selected = false;

        private SimpleExpressionEvaluator.ExpressionIterator parser = new SimpleExpressionEvaluator.ExpressionIterator();

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

        public Rectangle rect
        {
            get { return new Rectangle(x1, y1, bitmap.Width, bitmap.Height);  }
        }

        public Shape()
        {

        }

        public Shape(ShapeDef shapeDefinition, int x, int y, int width, int height)
        {
            this.ShapeDef = shapeDefinition;

            SolidBrush fillBrush = new SolidBrush(Color.WhiteSmoke);
            Pen pen = new Pen(Color.Black, 1);
            
            x1 = x;
            y1 = y;
            bitmap = new Bitmap(width, height);
            buffer = (Bitmap)bitmap.Clone();

            using (Graphics g = Graphics.FromImage(this.bitmap))
            {
                int h = this.bitmap.Height - 1;
                int w = this.bitmap.Width - 1;

                if (ShapeDef.Filledpolygon != null)
                {
                    Point[] points = new Point[ShapeDef.Filledpolygon.Points.Point.Count];
                    for (int zz=0; zz < ShapeDef.Filledpolygon.Points.Point.Count; zz++)
                    {
                        PointDef p = ShapeDef.Filledpolygon.Points.Point[zz];

                        points[zz].X = Convert.ToInt32(parser.EvaluateStringExpression(p.X.Replace("w", w.ToString()).Replace("h", h.ToString())));
                        points[zz].Y = Convert.ToInt32(parser.EvaluateStringExpression(p.Y.Replace("w", w.ToString()).Replace("h", h.ToString())));
                    }
                    g.FillPolygon(fillBrush, points);
                    g.DrawPolygon(pen, points);
                }

                if (ShapeDef.Ellipse != null)
                {
                    g.FillEllipse(fillBrush, 0, 0, w, h);
                    g.DrawEllipse(pen, 0, 0, w, h);
                }
                
            }
        }

        public Shape Clone()
        {
            Shape newShape = new Shape();

            newShape.ShapeDef = this.ShapeDef;
            newShape.x1 = x1;
            newShape.y1 = y1;
            newShape.bitmap = (Bitmap)this.bitmap.Clone();
            newShape.buffer = (Bitmap)this.buffer.Clone();

            return newShape;
        }

    }
}
