using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDesigner
{
    public class Line
    {
        public int x1;
        public int y1;
        public int x2;
        public int y2;

        public Bitmap buffer;

        public Line Clone()
        {
            Line l = new Line();
            l.x1 = x1;
            l.y1 = y1;
            l.x2 = x2;
            l.y2 = y2;

            return l;
        }
    }
}
