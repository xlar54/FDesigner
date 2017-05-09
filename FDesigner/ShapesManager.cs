using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDesigner
{
    public enum GrabArea
    {
        AREA,
        HANDLE_TOPLEFT,
        HANDLE_TOPCENTER,
        HANDLE_TOPRIGHT,
        HANDLE_MIDLEFT,
        HANDLE_MIDCENTER,
        HANDLE_MIDRIGHT,
        HANDLE_BTMLEFT,
        HANDLE_BTMCENTER,
        HANDLE_BTMRIGHT
    }

    public struct ShapeSelection
    {
        public int index;
        public GrabArea grabArea;
    }

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

        public Shape SelectedShape
        {
            get
            {
                if (SelectedIndex > -1)
                    return shapes[SelectedIndex];
                else
                    return null;
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

        public void MoveToBottom(int index)
        {
            shapes.Add(shapes[index]);
            shapes.RemoveAt(index);
        }

        public void RemoveAt(int index)
        {
            shapes.RemoveAt(index);
        }

        public ShapeSelection ShapeAtPoint(Point p)
        {
            ShapeSelection shapeSelection = new ShapeSelection{ index = -1, grabArea = GrabArea.AREA };

            for (int x = shapes.Count - 1; x > -1; x--)
            {
                Shape s = shapes[x];

                // Select the topmost selected shape
                if (p.X >= s.x1 && p.X <= s.x2 && p.Y >= s.y1 && p.Y <= s.y2)
                {
                    shapeSelection.index = x;

                    if (p.X > s.x1 + s.bitmap.Width - 10 && p.Y > s.y1 + s.bitmap.Height - 10)
                        shapeSelection.grabArea = GrabArea.HANDLE_BTMRIGHT;

                    if (p.X > s.x1 + (s.bitmap.Width / 2) - 5
                        && p.X < s.x1 + (s.bitmap.Width / 2) + 5
                        && p.Y > s.y1 + (s.bitmap.Height / 2) - 5
                        && p.Y > s.y1 + (s.bitmap.Height / 2) + 5)
                        shapeSelection.grabArea = GrabArea.HANDLE_BTMCENTER;

                    if (p.X > s.x1
                        && p.X < s.x1 + 10
                        && p.Y > s.y1 + (s.bitmap.Height - 10)
                        && p.Y < s.y1 + s.bitmap.Height)
                        shapeSelection.grabArea = GrabArea.HANDLE_BTMLEFT;
                }               
            }

            return shapeSelection;
        }
    }
}
