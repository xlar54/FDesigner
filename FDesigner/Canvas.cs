using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FDesigner
{
    public class Canvas
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
            public Cursor cursor;
        }

        Bitmap canvas;
        Bitmap buffer;

        public List<Shape> Shapes = new List<Shape>();

        public int GridSize = 20;

        public Bitmap bitmap
        {
            get { return canvas; }
        }

        public Bitmap tempBuffer
        {
            get { return buffer; }
        }

        public Canvas(int width, int height, int gridSize)
        {
            canvas = new Bitmap(width, height);
            GridSize = gridSize;
            DrawGrid();

            buffer = (Bitmap)canvas.Clone(); 
        }

        public void Clear()
        {
            canvas = new Bitmap(canvas.Width, canvas.Height);
            Shapes.Clear();
            DrawGrid();
        }

        public void DeselectShapes()
        {
            foreach (Shape s in Shapes)
                s.selected = false;
        }

        public int SelectedShapeIndex
        {
            get
            {

                for (int x = 0; x < Shapes.Count; x++)
                    if (Shapes[x].selected == true)
                        return x;

                return -1;
            }
            set
            {

                DeselectShapes();

                if (value > -1)
                    Shapes[value].selected = true;
            }
        }

        public Shape SelectedShape
        {
            get
            {
                if (SelectedShapeIndex > -1)
                    return Shapes[SelectedShapeIndex];
                else
                    return null;
            }

        }

        public ShapeSelection ShapeAtPoint(Point p)
        {
            ShapeSelection shapeSelection = new ShapeSelection { index = -1, grabArea = GrabArea.AREA };

            for (int x = Shapes.Count - 1; x > -1; x--)
            {
                Shape s = Shapes[x];

                // Select the topmost selected shape
                if (p.X >= s.x1 && p.X <= s.x2 && p.Y >= s.y1 && p.Y <= s.y2)
                {
                    
                    shapeSelection.index = x;

                    if (p.X > s.x1 + s.bitmap.Width - 10 && p.Y > s.y1 + s.bitmap.Height - 10)
                    {
                        shapeSelection.grabArea = GrabArea.HANDLE_BTMRIGHT;
                        shapeSelection.cursor = Cursors.SizeNWSE;
                    }
                       

                    if (p.X > s.x1 + (s.bitmap.Width / 2) - 5
                        && p.X < s.x1 + (s.bitmap.Width / 2) + 5
                        && p.Y > s.y1 + (s.bitmap.Height / 2) - 5
                        && p.Y > s.y1 + (s.bitmap.Height / 2) + 5)
                    {
                        shapeSelection.grabArea = GrabArea.HANDLE_BTMCENTER;
                        shapeSelection.cursor = Cursors.SizeNS;
                    }

                    if (p.X > s.x1
                        && p.X < s.x1 + 10
                        && p.Y > s.y1 + (s.bitmap.Height - 10)
                        && p.Y < s.y1 + s.bitmap.Height)
                    {
                        shapeSelection.grabArea = GrabArea.HANDLE_BTMLEFT;
                        shapeSelection.cursor = Cursors.SizeNESW;
                    }

                    if (p.X > s.x1 && p.X < s.x1 + 10
                        && p.Y > s.y1 + (s.bitmap.Height / 2) - 5
                        && p.Y < s.y1 + (s.bitmap.Height / 2) + 5)
                    {
                        shapeSelection.grabArea = GrabArea.HANDLE_MIDLEFT;
                        shapeSelection.cursor = Cursors.SizeWE;
                    }

                    if (p.X > s.x1 + s.bitmap.Width - 10
                        && p.Y > s.y1 + (s.bitmap.Height / 2) - 5
                        && p.Y < s.y1 + (s.bitmap.Height / 2) + 5)
                    {
                        shapeSelection.grabArea = GrabArea.HANDLE_MIDRIGHT;
                        shapeSelection.cursor = Cursors.SizeWE;
                    }

                    if (p.X > s.x1 && p.X < s.x1 + 10
                        && p.Y > s.y1 && p.Y < s.y1 + 10)
                    {
                        shapeSelection.grabArea = GrabArea.HANDLE_TOPLEFT;
                        shapeSelection.cursor = Cursors.SizeNWSE;
                    }

                    if (p.X > s.x1 + s.bitmap.Width - 10 && p.Y > s.y1 && p.Y < s.y1 + 10)
                    {
                        shapeSelection.grabArea = GrabArea.HANDLE_TOPRIGHT;
                        shapeSelection.cursor = Cursors.SizeNESW;
                    }

                    if (p.X > s.x1 + (s.bitmap.Width / 2) - 5
                        && p.X < s.x1 + (s.bitmap.Width / 2) + 5
                        && p.Y > s.y1 && p.Y < s.y1 + 10)
                    {
                        shapeSelection.grabArea = GrabArea.HANDLE_TOPCENTER;
                        shapeSelection.cursor = Cursors.SizeNS;
                    }

                    break;
                }
            }

            return shapeSelection;
        }

        public void Draw(Shape s)
        {
            using (Graphics g = Graphics.FromImage(canvas))
            {
                s.bitmap.MakeTransparent(Color.White);
                g.DrawImage(s.bitmap, new Point(s.x1, s.y1));
            }
        }

        public void Refresh()
        {
            DrawGrid();

            foreach (Shape s in Shapes)
                Draw(s);

            buffer = (Bitmap)canvas.Clone();
        }

        public Bitmap DragShape(int x, int y, Shape s, Point mouseOffset)
        {
            s.selected = true;

            Point p = new Point();

            if (mouseOffset.X == 0 && mouseOffset.Y == 0)
            {
                mouseOffset.X = s.CenterPoint.X;
                mouseOffset.Y = s.CenterPoint.Y;
            }

            p.X = x - mouseOffset.X;
            p.Y = y - mouseOffset.Y;

            // Snap to grid
            p = SnapToGrid(p);

            ReplaceBitmapRegion(s.buffer, s.x1, s.y1);

            s.x1 = p.X;
            s.y1 = p.Y;
            s.buffer = GetBitmapRegion(s.rect);

            Draw(s);

            return bitmap;

        }

        public Bitmap DropShape(int x, int y, Shape s, Point mouseOffset)
        {
            s.selected = true;

            Point p = new Point();

            if (mouseOffset.X == 0 && mouseOffset.Y == 0)
            {
                mouseOffset.X = s.CenterPoint.X;
                mouseOffset.Y = s.CenterPoint.Y;
            }

            p.X = x - mouseOffset.X;
            p.Y = y - mouseOffset.Y;

            // Snap to grid
            p = SnapToGrid(p);

            s.x1 = p.X;
            s.y1 = p.Y;
            s.buffer = GetBitmapRegion(s.rect);

            Shapes.Add(s);
            Refresh();
            DrawHandles(s);

            return bitmap;

        }

        public Shape ResizeShape(int x, int y, Shape s, GrabArea grabArea)
        {
            s.selected = true;

            Point p = new Point(x,y);

            // Snap to grid
            p = SnapToGrid(p);

            ReplaceBitmapRegion(s.buffer, s.x1, s.y1);

            if (grabArea == GrabArea.HANDLE_BTMCENTER)
            {
                s = new Shape(s.Type, s.x1, s.y1, s.bitmap.Width, p.Y - s.y1);
                s.buffer = GetBitmapRegion(s.rect);
            }

            if (grabArea == GrabArea.HANDLE_BTMLEFT)
            {
                s = new Shape(s.Type, p.X, s.y1, s.bitmap.Width + (s.x1 - p.X), p.Y - s.y1);
                s.buffer = GetBitmapRegion(s.rect);
            }

            if (grabArea == GrabArea.HANDLE_BTMRIGHT)
            {
                s = new Shape(s.Type, s.x1, s.y1, p.X - s.x1, p.Y - s.y1);
                s.buffer = GetBitmapRegion(s.rect);
            }

            if (grabArea == GrabArea.HANDLE_MIDLEFT)
            {
                s = new Shape(s.Type, p.X, s.y1, s.bitmap.Width + (s.x1 - p.X), s.bitmap.Height);
                s.buffer = GetBitmapRegion(s.rect);
            }

            if (grabArea == GrabArea.HANDLE_MIDRIGHT)
            {
                s = new Shape(s.Type, s.x1, s.y1, p.X - s.x1, s.bitmap.Height);
                s.buffer = GetBitmapRegion(s.rect);
            }

            if (grabArea == GrabArea.HANDLE_TOPLEFT)
            {
                s = new Shape(s.Type, p.X, p.Y, s.bitmap.Width + (s.x1 - p.X), s.bitmap.Height + (s.y1 - p.Y));
                s.buffer = GetBitmapRegion(s.rect);
            }

            if (grabArea == GrabArea.HANDLE_TOPRIGHT)
            {
                s = new Shape(s.Type, s.x1, p.Y, p.X - s.x1, s.bitmap.Height + (s.y1 - p.Y));
                s.buffer = GetBitmapRegion(s.rect);
            }

            if (grabArea == GrabArea.HANDLE_TOPCENTER)
            {
                s = new Shape(s.Type, s.x1, p.Y, s.bitmap.Width, s.bitmap.Height + (s.y1 - p.Y));
                s.buffer = GetBitmapRegion(s.rect);
            }


            Draw(s);
            DrawHandles(s);

            return s;

        }

        public Bitmap GetBitmapRegion(Rectangle rect)
        {
            Bitmap tempBmp = new Bitmap(rect.Width, rect.Height);
            using (Graphics u = Graphics.FromImage(tempBmp))
                u.DrawImage(this.bitmap, 0, 0, rect, GraphicsUnit.Pixel);

            return tempBmp;
        }

        public void DrawHandles(Shape s)
        {
            using (Graphics g = Graphics.FromImage(canvas))
            {
                s.bitmap.MakeTransparent(Color.White);
                g.DrawImage(s.bitmap, new Point(s.x1, s.y1));

                SolidBrush br = new SolidBrush(Color.Black);
                Pen p = new Pen(Color.Black);
                //p.DashPattern = new float[] { 9.0F, 2.0F, 1.0F, 3.0F };

                // Draw bounding box
                g.DrawRectangle(p, s.x1, s.y1, s.bitmap.Width - 1, s.bitmap.Height - 1);

                //Draw handles

                // Top Left
                g.FillRectangle(br, new Rectangle(s.x1, s.y1, 10, 10));
                // Top middle
                g.FillRectangle(br, new Rectangle(s.x1 + (s.bitmap.Width / 2) - 5, s.y1, 10, 10));
                // Top right
                g.FillRectangle(br, new Rectangle(s.x1 + s.bitmap.Width - 10, s.y1, 10, 10));

                // Bottom Left
                g.FillRectangle(br, new Rectangle(s.x1, s.y1 + s.bitmap.Height - 10, 10, 10));
                // Bottom middle
                g.FillRectangle(br, new Rectangle(s.x1 + (s.bitmap.Width / 2) - 5, s.y1 + s.bitmap.Height - 10, 10, 10));
                // Bottom right
                g.FillRectangle(br, new Rectangle(s.x1 + s.bitmap.Width - 10, s.y1 + s.bitmap.Height - 10, 10, 10));

                // Center Left
                g.FillRectangle(br, new Rectangle(s.x1, s.y1 + (s.bitmap.Height / 2) - 5, 10, 10));
                // Center Middle
                g.FillRectangle(br, new Rectangle(s.x1 + (s.bitmap.Width / 2) - 5, s.y1 + (s.bitmap.Height / 2) - 5, 10, 10));
                // Center Right
                g.FillRectangle(br, new Rectangle(s.x1 + s.bitmap.Width - 10, s.y1 + (s.bitmap.Height / 2) - 5, 10, 10));
            }
        }

        public void ReplaceBitmapRegion(Image srcImage, int x, int y)
        {
            // Draw bitmap to canvas
            using (Graphics g = Graphics.FromImage(this.bitmap))
            {
                g.DrawImage(srcImage, x, y);
            }
        }

        private void DrawGrid()
        {
            using (Graphics gb = Graphics.FromImage(canvas))
            {
                gb.Clear(Color.White);

                // Draw gridlines
                for (int x = 0; x < canvas.Width; x += GridSize)
                {
                    gb.DrawLine(new Pen(Color.WhiteSmoke), x, 0, x, canvas.Height);
                }

                for (int x = 0; x < canvas.Height; x += GridSize)
                {
                    gb.DrawLine(new Pen(Color.WhiteSmoke), 0, x, canvas.Width, x);
                }
            }
        }

        public Point SnapToGrid(Point p)
        {
            if (p.X % GridSize < GridSize / 2)
                p.X = p.X - p.X % GridSize;
            else
                p.X = p.X + (GridSize - p.X % GridSize);

            if (p.Y % GridSize < GridSize / 2)
                p.Y = p.Y - p.Y % GridSize;
            else
                p.Y = p.Y + (GridSize - p.Y % GridSize);

            return p;
        }

    }
}
