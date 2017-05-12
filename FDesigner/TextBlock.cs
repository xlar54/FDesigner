using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDesigner
{
    public class TextBlock
    {
        public int x1;
        public int y1;
        public int x2;
        public int y2;
        public string Text;

        public Bitmap buffer;

        public TextBlock Clone()
        {
            TextBlock l = new TextBlock();
            l.Text = Text;
            l.x1 = x1;
            l.y1 = y1;
            l.x2 = x2;
            l.y2 = y2;

            return l;
        }

    }
}
