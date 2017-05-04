using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDesigner
{
    public class ShapesManager
    {
        public List<Shape> shapes = new List<Shape>();
        public int selected;

        public ShapesManager()
        {

        }

        public int SelectedIndex
        {
            get {

                for (int x = 0; x < shapes.Count; x++)
                    if (shapes[x].selected == true)
                        return x;

                return -1;
            }
            set {

                DeselectAll();

                if (value > -1)
                    shapes[value].selected = true;
            }
        }

        public void Deselect(int x)
        {
            shapes[x].selected = false;
        }

        public void DeselectAll()
        {
            foreach (Shape s in shapes)
                s.selected = false;
        }

        public Shape SelectedShape
        {
            get
            {
                return shapes[SelectedIndex];
            }

        }

        public void RemoveAt(int index)
        {
            shapes.RemoveAt(index);
        }

        public void MoveToBottom(int index)
        {
            shapes.Add(shapes[index]);
            shapes.RemoveAt(index);
        }

        public void DrawAll(Image canvas)
        {
            Pen p = new Pen(Color.Green);
            p.DashPattern = new float[] { 9.0F, 2.0F, 1.0F, 3.0F };

            using (Graphics g = Graphics.FromImage(canvas))
            {
                foreach (Shape s in shapes)
                {
                    g.DrawImage(s.bitmap, new Point(s.x1, s.y1));

                    if (s.selected)
                    {
                        SolidBrush br = new SolidBrush(Color.Black);
                        //g.DrawRectangle(p, s.x1 - 5, s.y1 - 5, s.bitmap.Width + 10, s.bitmap.Height + 10);
                        
                        // Draw bounding box
                        g.DrawRectangle(p, s.x1, s.y1, s.bitmap.Width, s.bitmap.Height);

                        //Draw handles

                        // Top Left
                        g.FillRectangle(br, new Rectangle(s.x1, s.y1, 10, 10));
                        // Top middle
                        g.FillRectangle(br, new Rectangle(s.x1 + (s.bitmap.Width/2) - 5, s.y1, 10, 10));
                        // Top right
                        g.FillRectangle(br, new Rectangle(s.x1 + s.bitmap.Width - 10, s.y1, 10, 10));

                        // Bottom Left
                        g.FillRectangle(br, new Rectangle(s.x1, s.y1 + s.bitmap.Height - 10, 10, 10));
                        // Bottom middle
                        g.FillRectangle(br, new Rectangle(s.x1 + (s.bitmap.Width / 2) - 5, s.y1 + s.bitmap.Height - 10, 10, 10));
                        // Bottom right
                        g.FillRectangle(br, new Rectangle(s.x1 + s.bitmap.Width - 10, s.y1 + s.bitmap.Height - 10, 10, 10));

                        // Center Left
                        g.FillRectangle(br, new Rectangle(s.x1, s.y1 + (s.bitmap.Height/2) - 5, 10, 10));
                        // Center Middle
                        g.FillRectangle(br, new Rectangle(s.x1 + (s.bitmap.Width / 2) - 5, s.y1 + (s.bitmap.Height / 2) - 5, 10, 10));
                        // Center Right
                        g.FillRectangle(br, new Rectangle(s.x1 + s.bitmap.Width - 10, s.y1 + (s.bitmap.Height / 2) - 5, 10, 10));
                    }
                }
            }
        }

        public int ShapeIndexAtPoint(Point p)
        {
            for (int x = shapes.Count - 1; x > -1; x--)
            {
                Shape s = shapes[x];

                // Select the topmost selected shape
                if (p.X >= s.x1 && p.X <= s.x2 && p.Y >= s.y1 && p.Y <= s.y2)
                    return x;
            }

            return -1;
        }

        
    }
}
