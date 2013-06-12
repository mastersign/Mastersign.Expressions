using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace de.mastersign.expressions.demo
{
    public partial class MainForm : Form
    {
        private readonly EvaluationContext evalContext = new EvaluationContext(EvaluationContext.Default);
        private Func<int, int, double, double, double> f;

        private ColorPalette bwPalette;
        private Bitmap canvas;

        private readonly Stopwatch watch = new Stopwatch();

        public MainForm()
        {
            InitializeComponent();
            InitializeCanvas();
            evalContext.SetParameters(
                new ParameterInfo("X", typeof(int)),
                new ParameterInfo("Y", typeof(int)),
                new ParameterInfo("x", typeof(double)),
                new ParameterInfo("y", typeof(double)));
            watch.Start();
            UpdateContext();
            CompileFunction();
            RepaintCanvas();
        }

        private void display_SizeChanged(object sender, EventArgs e)
        {
            InitializeCanvas();
        }

        private void cmbExpr_TextChanged(object sender, EventArgs e)
        {
            CompileFunction();
            RepaintCanvas();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            RepaintCanvas();
        }

        private void InitializeCanvas()
        {
            var w = display.Width;
            var h = display.Height;
            if (canvas != null && canvas.Width == w && canvas.Height == h)
            {
                return;
            }
            evalContext.SetVariable("W", w);
            evalContext.SetVariable("H", h);
            canvas = new Bitmap(w, h, PixelFormat.Format8bppIndexed);
            if (bwPalette == null)
            {
                var palette = canvas.Palette;
                for (var i = 0; i < palette.Entries.Length; i++)
                {
                    palette.Entries[i] = Color.FromArgb(i, i, i);
                }
                bwPalette = palette;
            }
            canvas.Palette = bwPalette;
        }

        private void CompileFunction()
        {
            var expr = cmbExpr.Text;
            if (expr == string.Empty)
            {
                lblMessage.Text = "";
                cmbExpr.BackColor = SystemColors.Window;
                expr = "0";
            }
            try
            {
                f = evalContext.CompileExpression<int, int, double, double, double>(expr);
                lblMessage.Text = "";
                cmbExpr.BackColor = SystemColors.Window;
            }
            catch (SyntaxErrorException syntaxErr)
            {
                lblMessage.Text = "Syntax: "
                    + syntaxErr.Message
                    + (syntaxErr.InnerException != null ? syntaxErr.InnerException.Message : "");
                cmbExpr.BackColor = SystemColors.Info;
            }
            catch (SemanticErrorException semanticErr)
            {
                lblMessage.Text = "Fehler: "
                    + semanticErr.Message
                    + (semanticErr.InnerException != null ? semanticErr.InnerException.Message : "");
                cmbExpr.BackColor = SystemColors.Info;
            }
        }

        private void UpdateContext()
        {
            var t = Environment.TickCount;
            evalContext.SetVariable("T", t);
            var phase = (t % 10000) / 10000.0;
            evalContext.SetVariable("t", phase);
        }

        private void RepaintCanvas()
        {
            if (f == null) return;
            InitializeCanvas();

            var w = canvas.Width;
            var h = canvas.Height;
            var bmpData = canvas.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            UpdateContext();

            var line = bmpData.Scan0;
            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var v = f(x, y, (double)x / w - 0.5, (double)y / h - 0.5);
                    Marshal.WriteByte(line + x,
                        (byte)(Math.Min(1.0, Math.Max(0.0, v)) * 255.0));
                }
                line += bmpData.Stride;
            }
            canvas.UnlockBits(bmpData);
            display.Picture = canvas;
        }
    }
}
