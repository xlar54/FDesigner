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

    }
}
