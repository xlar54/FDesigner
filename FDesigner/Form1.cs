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

namespace FDesigner
{
    public partial class Form1 : Form
    {
        public enum ShapeType
        {
            NONE,
            BOX,
            DIAMOND,
            CIRCLE,
            TRIANGLE,
        }

        Bitmap mainCanvas = new Bitmap(1280, 960);
        Bitmap tempCanvas;

        ShapesManager shapeMgr = new ShapesManager();

        public ShapeType selectedShape;
        public bool drawingShape = false;
        public bool movingShape = false;
        public Point mouseOffset;
        public Shape tempShape = new Shape();

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
            selectedShape = ShapeType.BOX;

            shapeMgr.DeselectAll();
            drawShapesQueue();
        }

        private void btnDiamond_Click(object sender, EventArgs e)
        {
            selectedShape = ShapeType.DIAMOND;
            shapeMgr.DeselectAll();
            drawShapesQueue();
        }

        private void btnCircle_Click(object sender, EventArgs e)
        {
            selectedShape = ShapeType.CIRCLE;
            shapeMgr.DeselectAll();
            drawShapesQueue();
        }

        private void btnTriangle_Click(object sender, EventArgs e)
        {
            selectedShape = ShapeType.TRIANGLE;
            shapeMgr.DeselectAll();
            drawShapesQueue();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            Point m = e.Location;
            
            // User starts to draw a shape
            if (e.Button == MouseButtons.Left && !drawingShape && selectedShape != ShapeType.NONE)
            {
                this.Cursor = Cursors.SizeNWSE;
                drawShapesQueue();
                
                tempShape.x1 = m.X;
                tempShape.y1 = m.Y;
                drawingShape = true;
                tempCanvas = (Bitmap)mainCanvas.Clone();
                return;
            }

            // User moves a shape (or clicks canvas)
            if (e.Button == MouseButtons.Left && !drawingShape && selectedShape == ShapeType.NONE)
            {
                this.Cursor = Cursors.SizeAll;

                shapeMgr.DeselectAll();

                int shapeAtPoint = shapeMgr.ShapeIndexAtPoint(m);

                if (shapeAtPoint > -1)
                {
                    shapeMgr.SelectedIndex = shapeAtPoint;
                    drawShapesQueue();

                    movingShape = true;
                    mouseOffset = new Point(m.X - shapeMgr.SelectedShape.x1, m.Y - shapeMgr.SelectedShape.y1);
                    tempCanvas = (Bitmap)mainCanvas.Clone();
                }


                drawShapesQueue();
            }

        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            Point m = e.Location;
            SolidBrush whiteBrush = new SolidBrush(Color.White);
            SolidBrush fillBrush = new SolidBrush(Color.WhiteSmoke);
            Pen pen = new Pen(Color.Black, 1);

            label1.Text = "X=" + m.X.ToString() + ", Y=" + m.Y.ToString();

            if (e.Button == MouseButtons.Left && drawingShape)
            {
                // If inside the bounds of a rectangle from original start point
                if (m.X > tempShape.x1 && m.Y > tempShape.y1)
                {
                    // Create a bitmap of size start to current
                    int width = m.X + 1 - tempShape.x1;
                    int height = m.Y + 1 - tempShape.y1;

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
                        switch (selectedShape)
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
                                    gb.FillEllipse(fillBrush, 0, 0, tempShape.bitmap.Width-1, tempShape.bitmap.Height-1);
                                    gb.DrawEllipse(pen, 0, 0, tempShape.bitmap.Width-1, tempShape.bitmap.Height-1);
                                    break;
                                }
                                
                            case ShapeType.TRIANGLE:
                                {
                                    Point[] points = new Point[3];
                                    points[0] = new Point(0, tempShape.bitmap.Height-1);
                                    points[1] = new Point(tempShape.bitmap.Width / 2, 0);
                                    points[2] = new Point(tempShape.bitmap.Width-1, tempShape.bitmap.Height-1);
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
                    ReplaceBitmapRegion(tempCanvas, mainCanvas, new Rectangle(tempShape.x1, tempShape.y1, maxWidth, maxHeight));
                    tempShape.Draw(mainCanvas);

                    pictureBox1.Image = mainCanvas;
                }
            }

            if (e.Button == MouseButtons.Left && movingShape)
            {
                // Get underlying region, draw it, then draw the shape
                ReplaceBitmapRegion(tempCanvas, mainCanvas, shapeMgr.SelectedShape.rect);

                shapeMgr.SelectedShape.x1 = m.X - mouseOffset.X;
                shapeMgr.SelectedShape.y1 = m.Y - mouseOffset.Y;
                shapeMgr.SelectedShape.Draw(mainCanvas);

                pictureBox1.Image = mainCanvas;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (drawingShape)
            {
                this.Cursor = Cursors.Arrow;
                drawingShape = false;
                selectedShape = ShapeType.NONE;

                maxHeight = 0;
                maxWidth = 0;

                Shape newShape = new Shape();
                newShape.x1 = tempShape.x1;
                newShape.y1 = tempShape.y1;
                newShape.bitmap = (Bitmap)tempShape.bitmap.Clone();
                newShape.selected = true;
                shapeMgr.shapes.Add(newShape);

                drawShapesQueue();
            }

            if (movingShape)
            {
                this.Cursor = Cursors.Arrow;
                movingShape = false;
                shapeMgr.MoveToBottom(shapeMgr.SelectedIndex);
                drawShapesQueue();
            }
            
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
    }
}
