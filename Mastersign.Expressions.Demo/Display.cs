using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace de.mastersign.expressions.demo
{
    public partial class Display : UserControl
    {
        private Bitmap picture;

        public Display()
        {
            InitializeComponent();
        }

        public Bitmap Picture
        {
            get { return picture; }
            set
            {
                picture = value;
                Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // nothing
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Picture != null)
            {
                e.Graphics.DrawImage(Picture, Point.Empty);
            }
            else
            {
                e.Graphics.Clear(BackColor);
            }
        }
    }
}
