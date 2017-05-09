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
        Canvas canvas = new Canvas(1024, 768, 10);
        Shape tempShape = new Shape();
        public Point mouseOffset;

        ContextMenuStrip popUpMenu = new ContextMenuStrip();

        public Form1()
        {
            InitializeComponent();

            pictureBox1.Image = canvas.bitmap;

            popUpMenu.Items.Add("Copy");
            popUpMenu.Items.Add("Cut");
            popUpMenu.Items.Add("Paste");
            popUpMenu.Items.Add("-");
            popUpMenu.Items.Add("Delete");
            popUpMenu.ItemClicked += popUpMenu_ItemClicked;
            pictureBox1.ContextMenuStrip = popUpMenu;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.AllowDrop = true;
        }

        private void btnSquare_MouseDown(object sender, MouseEventArgs e)
        {
            deselectAll();
            mouseOffset = new Point(0, 0);
            tempShape = new Shape(ShapeType.BOX, 0, 0, 100, 100);
            btnSquare.DoDragDrop(ShapeType.BOX, DragDropEffects.Move);
        }

        private void btnDiamond_MouseDown(object sender, MouseEventArgs e)
        {
            deselectAll();
            mouseOffset = new Point(0, 0);
            tempShape = new Shape(ShapeType.DIAMOND, 0, 0, 100, 100);
            btnDiamond.DoDragDrop(ShapeType.DIAMOND, DragDropEffects.Move);
        }

        private void btnCircle_MouseDown(object sender, MouseEventArgs e)
        {
            deselectAll();
            mouseOffset = new Point(0, 0);
            tempShape = new Shape(ShapeType.CIRCLE, 0, 0, 100, 100);
            btnCircle.DoDragDrop(ShapeType.CIRCLE, DragDropEffects.Move);
        }

        private void btnTriangle_MouseDown(object sender, MouseEventArgs e)
        {
            deselectAll();
            mouseOffset = new Point(0, 0);
            tempShape = new Shape(ShapeType.TRIANGLE, 0, 0, 100, 100);
            btnTriangle.DoDragDrop(ShapeType.TRIANGLE, DragDropEffects.Move);
        }
        

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = new Bitmap(640, 480);
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
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
            canvas.Clear();
        }


        private void popUpMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "Delete")
            {
                int x = canvas.SelectedShapeIndex;
                deselectAll();
                canvas.Shapes.RemoveAt(x);
                canvas.Refresh();
            }
        }


        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            Canvas.ShapeSelection shapeSelection = canvas.ShapeAtPoint(e.Location);

            // User clicks empty area
            if (shapeSelection.index == -1)
            {
                canvas.DeselectShapes();
                canvas.Refresh();
                pictureBox1.Image = canvas.bitmap;
            }

            foreach (ToolStripItem item in popUpMenu.Items)
                item.Enabled = (shapeSelection.index > -1 ? true : false);

            if (e.Button == MouseButtons.Left)
            {
                if (shapeSelection.index > -1)
                {
                    // Pick up the shape, and initiate a drag / drop
                    canvas.SelectedShapeIndex = shapeSelection.index;
                    tempShape = canvas.SelectedShape.Clone();
                    canvas.Shapes.RemoveAt(shapeSelection.index);
                    canvas.Refresh();

                    tempShape.buffer = canvas.GetBitmapRegion(tempShape.rect);
                    pictureBox1.Image = canvas.bitmap;

                    mouseOffset = new Point(e.X - tempShape.x1, e.Y - tempShape.y1);
                    pictureBox1.DoDragDrop(shapeSelection, DragDropEffects.Move);
                }
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            label1.Text = "X=" + e.X + ", Y=" + e.Y;
        }


        private void pictureBox1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void pictureBox1_DragDrop(object sender, DragEventArgs e)
        {
            int x = ((PictureBox)sender).PointToClient(new Point(e.X, e.Y)).X;
            int y = ((PictureBox)sender).PointToClient(new Point(e.X, e.Y)).Y;

            label1.Text = "X=" + x + ", Y=" + y;

            pictureBox1.Image = canvas.DropShape(x, y, tempShape, mouseOffset);
        }

        private void pictureBox1_DragOver(object sender, DragEventArgs e)
        {
            int x = ((PictureBox)sender).PointToClient(new Point(e.X, e.Y)).X;
            int y = ((PictureBox)sender).PointToClient(new Point(e.X, e.Y)).Y;

            label1.Text = "X=" + x + ", Y=" + y;

            if (e.Data.GetData(typeof(Canvas.ShapeSelection)) != null)
            {
                Canvas.ShapeSelection shapeSelection = (Canvas.ShapeSelection)e.Data.GetData(typeof(Canvas.ShapeSelection));

                if (shapeSelection.grabArea == Canvas.GrabArea.AREA || shapeSelection.grabArea == Canvas.GrabArea.HANDLE_MIDCENTER)
                {
                    pictureBox1.Image = canvas.DragShape(x, y, tempShape, mouseOffset);
                }
                else
                {
                    mouseOffset = new Point(x - tempShape.x1, y - tempShape.y1);
                    tempShape = canvas.ResizeShape(x, y, tempShape, shapeSelection.grabArea);
                    pictureBox1.Image = canvas.bitmap;
                }
            }
            else
            {
                pictureBox1.Image = canvas.DragShape(x, y, tempShape, mouseOffset);
            }

            
        }


        private void deselectAll()
        {
            if (canvas.SelectedShape != null)
            {
                canvas.SelectedShape.selected = false;
                canvas.Refresh();
                pictureBox1.Invalidate();
            }
        }


    }
}
