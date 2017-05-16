using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using static FDesigner.Shape;

namespace FDesigner
{
    public partial class Form1 : Form
    {
        public enum Tool
        {
            SELECT,
            LINE,
            CONNECTOR,
            TEXT
        }

        Canvas canvas = new Canvas(1024, 768, 10);

        Shape tempShape = new Shape();
        Line tempLine = new Line();
        TextBlock tempTextBlock = new TextBlock();
        bool processTextBlockKeys = false;

        public Point mouseOffset;
        public Tool currentTool = Tool.SELECT;

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

            LoadShapes();
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

            if (currentTool == Tool.SELECT)
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

            if (currentTool == Tool.LINE)
            {
                tempLine.x1 = e.X;
                tempLine.y1 = e.Y;
            }

            if (currentTool == Tool.TEXT)
            {
                tempTextBlock.x1 = e.X;
                tempTextBlock.y1 = e.Y;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripLabel1.Text = "X=" + e.X + ", Y=" + e.Y;

            if (currentTool == Tool.LINE)
            {
                if (e.Button == MouseButtons.Left)
                {
                    canvas.Refresh();

                    pictureBox1.Image = canvas.bitmap;

                    tempLine.x2 = e.X;
                    tempLine.y2 = e.Y;

                    canvas.Draw(tempLine);

                    pictureBox1.Image = canvas.bitmap;
                }

            }

            if (currentTool == Tool.TEXT)
            {
                if (e.Button == MouseButtons.Left)
                {
                    canvas.Refresh();

                    pictureBox1.Image = canvas.bitmap;

                    tempTextBlock.x2 = e.X;
                    tempTextBlock.y2 = e.Y;

                    canvas.Draw(tempTextBlock);

                    pictureBox1.Image = canvas.bitmap;
                }

            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (currentTool == Tool.LINE)
            {
                if (e.Button == MouseButtons.Left)
                {
                    tempLine.x2 = e.X;
                    tempLine.y2 = e.Y;

                    Line newLine = tempLine.Clone();

                    canvas.Lines.Add(newLine);
                    canvas.Refresh();
                    pictureBox1.Image = canvas.bitmap;
                }
            }

            if (currentTool == Tool.TEXT)
            {
                processTextBlockKeys = true;

                TextBox textBox = new TextBox();
                textBox.Name = "tempTextBox";
                textBox.Location = new Point(tempTextBlock.x1, tempTextBlock.y1);
                textBox.KeyPress += TextBox_KeyPress;
                textBox.Font = new Font(FontFamily.GenericMonospace, 10);
                textBox.Multiline = true;
                textBox.Width = tempTextBlock.x2 - tempTextBlock.x1;
                textBox.Height = tempTextBlock.y2 - tempTextBlock.y1;
                
                pictureBox1.Controls.Add(textBox);
                textBox.Focus();
            }
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                tempTextBlock.Text = ((TextBox)sender).Text;
                pictureBox1.Controls.Remove(((TextBox)sender));
                ((TextBox)sender).Dispose();

                TextBlock tb = tempTextBlock.Clone();
                canvas.TextBlocks.Add(tb);
                canvas.Refresh();
                pictureBox1.Image = canvas.bitmap;

            }
            else
            {
                tempTextBlock.Text = ((TextBox)sender).Text;
            }
        }



        private void pictureBox1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void pictureBox1_DragDrop(object sender, DragEventArgs e)
        {
            int x = ((PictureBox)sender).PointToClient(new Point(e.X, e.Y)).X;
            int y = ((PictureBox)sender).PointToClient(new Point(e.X, e.Y)).Y;

            toolStripLabel1.Text = "X=" + x + ", Y=" + y;

            pictureBox1.Image = canvas.DropShape(x, y, tempShape, mouseOffset);
        }

        private void pictureBox1_DragOver(object sender, DragEventArgs e)
        {
            int x = ((PictureBox)sender).PointToClient(new Point(e.X, e.Y)).X;
            int y = ((PictureBox)sender).PointToClient(new Point(e.X, e.Y)).Y;

            toolStripLabel1.Text = "X=" + x + ", Y=" + y;

            if (e.Data.GetData(typeof(Canvas.ShapeSelection)) != null)
            {
                Canvas.ShapeSelection shapeSelection = (Canvas.ShapeSelection)e.Data.GetData(typeof(Canvas.ShapeSelection));

                if (shapeSelection.grabArea == Canvas.GrabArea.AREA || shapeSelection.grabArea == Canvas.GrabArea.HANDLE_MIDCENTER)
                {
                    // Moving a shape
                    Cursor.Current = Cursors.SizeAll;
                    pictureBox1.Image = canvas.DragShape(x, y, tempShape, mouseOffset);
                }
                else
                {
                    // Resizing a shape
                    Cursor.Current = shapeSelection.cursor;
                    mouseOffset = new Point(x - tempShape.x1, y - tempShape.y1);
                    tempShape = canvas.ResizeShape(x, y, tempShape, shapeSelection.grabArea);
                    pictureBox1.Image = canvas.bitmap;
                }
            }
            else
            {
                // Dragging from toolbox
                pictureBox1.Image = canvas.DragShape(x, y, tempShape, mouseOffset);
            }

            
        }

        private void pictureBox1_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = false;
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


        private void lineToolStripButton_Click(object sender, EventArgs e)
        {
            deselectAll();
            currentTool = Tool.LINE;
            textToolStripButton.Checked = false;
            lineToolStripButton.Checked = true;
            selectToolStripButton.Checked = false;

        }

        private void selectToolStripButton_Click(object sender, EventArgs e)
        {
            deselectAll();
            currentTool = Tool.SELECT;
            textToolStripButton.Checked = false;
            selectToolStripButton.Checked = true;
            lineToolStripButton.Checked = false;
        }

        private void textToolStripButton_Click(object sender, EventArgs e)
        {
            deselectAll();
            currentTool = Tool.TEXT;
            textToolStripButton.Checked = true;
            selectToolStripButton.Checked = false;
            lineToolStripButton.Checked = false;
        }


        public void LoadShapes()
        {
            XmlSerializer ser = new XmlSerializer(typeof(Xml2CSharp.Groups));

            using (FileStream fileStream = new FileStream("shapes.xml", FileMode.Open))
            {
                Xml2CSharp.Groups g = (Xml2CSharp.Groups)ser.Deserialize(fileStream);

                int buttonTop = 0;

                for(int x=0; x < g.Group.Count; x++)
                {
                    Xml2CSharp.Group group = g.Group[x];

                    Button b = new Button();
                    b.Name = "btnGroup" + group.Name.Trim().Replace(" ", "");
                    b.Text = group.Name;
                    b.BackColor = Color.White;
                    b.Width = panel1.Width;
                    b.Top = buttonTop;
                    b.Height = 25;
                    b.FlatStyle = FlatStyle.Flat;
                    b.FlatAppearance.BorderColor = Color.Black;
                    b.FlatAppearance.BorderSize = 1;
                    b.Click += GroupButton_Click;
                    buttonTop += b.Height + 5;
                    panel1.Controls.Add(b);
                }
            }

        }

        private void GroupButton_Click(object sender, EventArgs e)
        {
            int offset = panel1.Height;

            foreach (Control c in panel1.Controls)
            {
                if (c is Panel)
                {
                    c.Controls.Clear();
                    c.Dispose();
                }
                    
            }

            foreach (Control c in panel1.Controls)
            {
                if (c is Button)
                {
                    if (c.Name != ((Button)sender).Name)
                    {
                        // Move unselected group buttons to bottom
                        c.Top = offset - c.Height;
                        offset = c.Top;
                        c.Tag = "";
                    }
                    else
                    {
                        // This is the selected group button
                        c.Top = 0;
                        c.Tag = "selected";

                        // Inner panel to contain the shape buttons
                        Panel p = new Panel();
                        p.Width = panel1.Width;
                        p.Height = panel1.Height;
                        p.Top = 40;
                        
                        XmlSerializer ser = new XmlSerializer(typeof(Xml2CSharp.Groups));

                        // build the shape buttons and add them to the new inner panel
                        using (FileStream fileStream = new FileStream("shapes.xml", FileMode.Open))
                        {
                            Xml2CSharp.Groups g = (Xml2CSharp.Groups)ser.Deserialize(fileStream);

                            int buttonTop = 0;

                            for (int x = 0; x < g.Group.Count; x++)
                            {
                                if (g.Group[x].Name == c.Text)
                                {
                                    Xml2CSharp.Group group = g.Group[x];

                                    for (int z=0; z < group.Shape.Count; z++)
                                    {
                                        Xml2CSharp.Shape shape = group.Shape[z];

                                        Button b = new Button();
                                        b.Name = "btnShape" + shape.Name.Trim().Replace(" ", "");
                                        b.Text = shape.Name;
                                        b.BackColor = Color.Beige;
                                        b.Width = p.Width;
                                        b.Top = buttonTop;
                                        b.Height = 35;
                                        b.Tag = shape;
                                        b.MouseDown += ShapeButton_MouseDown;
                                        buttonTop += b.Height + 5;
                                        p.Controls.Add(b);
                                    }

                                    panel1.Controls.Add(p);
                                    
                                }
                            }
                        }

                    }
                    
                }
            }
        }

        private void ShapeButton_MouseDown(object sender, MouseEventArgs e)
        {
            Button b = ((Button)sender);
            Xml2CSharp.Shape shape = (Xml2CSharp.Shape)b.Tag;
            selectToolStripButton_Click(null, null);
            deselectAll();
            mouseOffset = new Point(0, 0);
            tempShape = new Shape(shape, 0, 0, 100, 100);
            ((Button)sender).DoDragDrop(ShapeType.BOX, DragDropEffects.Move);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            foreach (Control c in panel1.Controls)
            {
                if (c is Button)
                {
                    if ((string)c.Tag == "selected")
                    {
                        GroupButton_Click(c, null);
                    }
                }
            }
        }
    }
}
