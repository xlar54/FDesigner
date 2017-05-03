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

        public int x2 { get { return x1 + bitmap.Width;  } }
        public int y2 { get { return y1 + bitmap.Height; } }

        public int z;
        public Bitmap bitmap;
        public bool selected = false;

        public Rectangle rect
        {
            get { return new Rectangle(x1, y1, bitmap.Width, bitmap.Height);  }
        }

        public void Draw(Image canvas)
        {
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.DrawImage(this.bitmap, new Point(this.x1, this.y1));
            }
        }

    }
}
