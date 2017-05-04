using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static FDesigner.Shape;

namespace FDesigner
{
    public partial class Form1 : Form
    {
        public enum CanvasMode
        {
            NONE,
            DRAW,
            MOVE
        }

        Bitmap mainCanvas = new Bitmap(1280, 960);
        Bitmap tempCanvas;

        ShapesManager shapeMgr = new ShapesManager();

        public bool drawingShape = false;
        public bool movingShape = false;
        public Point mouseOffset;
        public Shape tempShape;

        int maxHeight;
        int maxWidth;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            clearCanvas(mainCanvas);
        }

        private void clearCanvas(Image canvas)
        {
            using (Graphics gb = Graphics.FromImage(canvas))
            {
                gb.Clear(Color.White);

                // Draw gridlines
                for (int x = 0; x < canvas.Width; x += 20)
                {
                    gb.DrawLine(new Pen(Color.WhiteSmoke), x, 0, x, canvas.Height);
                }

                for (int x = 0; x < canvas.Height; x += 20)
                {
                    gb.DrawLine(new Pen(Color.WhiteSmoke), 0, x, canvas.Width, x);
                }
            }

            pictureBox1.Image = canvas;
        }

        private void btnSquare_Click(object sender, EventArgs e)
        {
            tempShape = new Shape();
            tempShape.shapeType = ShapeType.BOX;

            shapeMgr.DeselectAll();
            drawShapesQueue();
        }

        private void btnDiamond_Click(object sender, EventArgs e)
        {
            tempShape = new Shape();
            tempShape.shapeType = ShapeType.DIAMOND;
            shapeMgr.DeselectAll();
            drawShapesQueue();
        }

        private void btnCircle_Click(object sender, EventArgs e)
        {
            tempShape = new Shape();
            tempShape.shapeType = ShapeType.CIRCLE;
            shapeMgr.DeselectAll();
            drawShapesQueue();
        }

        private void btnTriangle_Click(object sender, EventArgs e)
        {
            tempShape = new Shape();
            tempShape.shapeType = ShapeType.TRIANGLE;
            shapeMgr.DeselectAll();
            drawShapesQueue();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            Point m = e.Location;

            // Are we clicking on an existing shape, or empty area
            int shapeAtPoint = shapeMgr.ShapeIndexAtPoint(m);

            // User clicks empty area
            if (e.Button == MouseButtons.Left && shapeAtPoint == -1 && tempShape == null)
            {
                shapeMgr.DeselectAll();
                drawShapesQueue();
                return;
            }

            // User starts to draw a new shape
            if (e.Button == MouseButtons.Left && shapeAtPoint == -1 && tempShape != null)
            {
                shapeMgr.DeselectAll();

                this.Cursor = Cursors.SizeNWSE;
                drawShapesQueue();
                
                tempShape.x1 = m.X;
                tempShape.y1 = m.Y;
                drawingShape = true;
                tempCanvas = (Bitmap)mainCanvas.Clone();
                return;
            }

            // User clicks a shape, go to move mode
            if (e.Button == MouseButtons.Left && shapeAtPoint > -1)
            {
                shapeMgr.DeselectAll();

                this.Cursor = Cursors.SizeAll;
                shapeMgr.SelectedIndex = shapeAtPoint;
                drawShapesQueue();

                // Check if mouse is over a handle
                if (m.X > shapeMgr.SelectedShape.x1 + shapeMgr.SelectedShape.bitmap.Width - 10 
                    && m.Y > shapeMgr.SelectedShape.y1 + shapeMgr.SelectedShape.bitmap.Height - 10)
                {
                    tempShape = shapeMgr.SelectedShape.Clone();

                    shapeMgr.RemoveAt(shapeMgr.SelectedIndex);

                    drawingShape = true;
                    tempCanvas = (Bitmap)mainCanvas.Clone();
                }
                else
                {
                    movingShape = true;
                    mouseOffset = new Point(m.X - shapeMgr.SelectedShape.x1, m.Y - shapeMgr.SelectedShape.y1);
                    tempCanvas = (Bitmap)mainCanvas.Clone();
                }

                drawShapesQueue();
            }

        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            Point location = e.Location;
            label1.Text = "X=" + location.X.ToString() + ", Y=" + location.Y.ToString();

            if (e.Button == MouseButtons.Left && drawingShape)
            {
                drawShape(location);
            }

            if (e.Button == MouseButtons.Left && movingShape)
            {
                moveSelectedShape(location);
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;

            if (drawingShape)
            {
                drawingShape = false;

                maxHeight = 0;
                maxWidth = 0;

                Shape newShape = tempShape.Clone();
                newShape.selected = true;
                shapeMgr.shapes.Add(newShape);
                tempShape = null;

                drawShapesQueue();
            }

            if (movingShape)
            {
                movingShape = false;
                shapeMgr.MoveToBottom(shapeMgr.SelectedIndex);
                drawShapesQueue();
            }
            
        }

        private void drawShape(Point location)
        {
            SolidBrush whiteBrush = new SolidBrush(Color.White);
            SolidBrush fillBrush = new SolidBrush(Color.WhiteSmoke);
            Pen pen = new Pen(Color.Black, 1);

            // If inside the bounds of a rectangle from original start point
            if (location.X > tempShape.x1 && location.Y > tempShape.y1)
            {
                // Create a bitmap of size start to current
                int width = location.X + 1 - tempShape.x1;
                int height = location.Y + 1 - tempShape.y1;

                // We have to stop any crazy sizing for performance reasons
                if (width > 200 || height > 200)
                    return;

                tempShape.bitmap = new Bitmap(width, height);

                // Track the max size so when user retracts, we can copy underlying back in
                if (tempShape.bitmap.Width > maxWidth) maxWidth = tempShape.bitmap.Width;
                if (tempShape.bitmap.Height > maxHeight) maxHeight = tempShape.bitmap.Height;

                using (Graphics gb = Graphics.FromImage(tempShape.bitmap))
                {
                    gb.SmoothingMode = SmoothingMode.AntiAlias;

                    // Draw shape to temp bitmap 
                    switch (tempShape.shapeType)
                    {
                        case ShapeType.BOX:
                            {
                                gb.FillRectangle(fillBrush, 0, 0, tempShape.bitmap.Width - 1, tempShape.bitmap.Height - 1);
                                gb.DrawRectangle(pen, 0, 0, tempShape.bitmap.Width - 1, tempShape.bitmap.Height - 1);
                                break;
                            }

                        case ShapeType.DIAMOND:
                            {
                                Point[] points = new Point[4];
                                points[0] = new Point(0, tempShape.bitmap.Height / 2);
                                points[1] = new Point(tempShape.bitmap.Width / 2, 0);
                                points[2] = new Point(tempShape.bitmap.Width - 1, tempShape.bitmap.Height / 2);
                                points[3] = new Point(tempShape.bitmap.Width / 2, tempShape.bitmap.Height - 1);
                                gb.FillPolygon(fillBrush, points);
                                gb.DrawPolygon(pen, points);
                                break;
                            }

                        case ShapeType.CIRCLE:
                            {
                                gb.FillEllipse(fillBrush, 0, 0, tempShape.bitmap.Width - 1, tempShape.bitmap.Height - 1);
                                gb.DrawEllipse(pen, 0, 0, tempShape.bitmap.Width - 1, tempShape.bitmap.Height - 1);
                                break;
                            }

                        case ShapeType.TRIANGLE:
                            {
                                Point[] points = new Point[3];
                                points[0] = new Point(0, tempShape.bitmap.Height - 1);
                                points[1] = new Point(tempShape.bitmap.Width / 2, 0);
                                points[2] = new Point(tempShape.bitmap.Width - 1, tempShape.bitmap.Height - 1);
                                gb.FillPolygon(fillBrush, points);
                                gb.DrawPolygon(pen, points);
                                break;
                            }

                        default:
                            throw new Exception("you done goofed");
                    }
                }

                tempShape.bitmap.MakeTransparent(Color.White);

                // Get underlying region, draw it, then draw the shape
                Rectangle r = new Rectangle(tempShape.x1, tempShape.y1, maxWidth, maxHeight);
                ReplaceBitmapRegion(tempCanvas, mainCanvas, r);
                pictureBox1.Invalidate(r);

                tempShape.Draw(mainCanvas);
                pictureBox1.Invalidate(tempShape.rect);
            }
        }

        private void moveSelectedShape(Point location)
        {
            // Get underlying region, draw it
            ReplaceBitmapRegion(tempCanvas, mainCanvas, shapeMgr.SelectedShape.rect);
            pictureBox1.Invalidate(shapeMgr.SelectedShape.rect);

            // draw at new location
            shapeMgr.SelectedShape.x1 = location.X - mouseOffset.X;
            shapeMgr.SelectedShape.y1 = location.Y - mouseOffset.Y;
            shapeMgr.SelectedShape.Draw(mainCanvas);
            pictureBox1.Invalidate(shapeMgr.SelectedShape.rect);
        }

        private void drawShapesQueue()
        {
            clearCanvas(mainCanvas);
            shapeMgr.DrawAll(mainCanvas);
            pictureBox1.Image = mainCanvas;
        }

        private bool checkForOverlap(Shape shape)
        {
           /* foreach (Shape s in shapes)
            {
                if (shape.x1 >= s.x1 && shape.x1 <= s.x1 + s.bitmap.Width)
                {
                    return true;
                }

                if (shape.y1 + shape.bitmap.Height > s.y1)
                {
                    return true;
                }

            }*/

            return false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = new Bitmap(640, 480);
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;

        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (shapeMgr.shapes.Count > 0)
            {
                shapeMgr.shapes.RemoveAt(shapeMgr.shapes.Count - 1);
                drawShapesQueue();
            }
            
        }

        private void ReplaceBitmapRegion(Image srcImage, Image destImage, Rectangle rect)
        {
            Bitmap tempBmp = new Bitmap(rect.Width, rect.Height);
            using (Graphics u = Graphics.FromImage(tempBmp))
                u.DrawImage(srcImage, 0, 0, rect, GraphicsUnit.Pixel);

            // Draw temp bitmap to canvas
            using (Graphics g = Graphics.FromImage(destImage))
            {
                g.DrawImage(tempBmp, rect.X, rect.Y);

                //Pen p = new Pen(Color.Green);
                //p.DashPattern = new float[] { 9.0F, 6.0F, 4.0F, 3.0F };
                //g.DrawRectangle(p, shapes[selectedShapeIndex].x1 - 5, shapes[selectedShapeIndex].y1 - 5, shapes[selectedShapeIndex].bitmap.Width + 10, shapes[selectedShapeIndex].bitmap.Height + 10);
            }
        }

        private void printToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            PrintDocument printDocument1 = new PrintDocument();
            printDocument1.PrintPage += new PrintPageEventHandler(printDocument1_PrintPage);
            printDocument1.Print();
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(pictureBox1.Image, 0, 0);
        }

        private void newToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            clearCanvas(mainCanvas);
            tempShape = new Shape();
            shapeMgr.shapes.Clear();

        }
    }
}
