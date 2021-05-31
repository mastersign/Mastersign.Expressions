using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mastersign.Expressions.Demo
{
    public partial class MainForm : Form
    {
        private readonly EvaluationContext evalContext = new EvaluationContext();
        private Func<int, int, double, double, double> f;

        private ColorPalette bwPalette;
        private Bitmap frontbuffer, backbuffer;

        private readonly Stopwatch watch = new Stopwatch();

        public MainForm()
        {
            InitializeComponent();
            InitializeBackbuffer();
            evalContext.LoadAllPackages();
            evalContext.SetParameters(
                new ParameterInfo("X", typeof(int)),
                new ParameterInfo("Y", typeof(int)),
                new ParameterInfo("rx", typeof(double)),
                new ParameterInfo("ry", typeof(double)));
            watch.Start();
            UpdateContext();
            CompileFunction();
            RepaintCanvas();
        }

        private void display_SizeChanged(object sender, EventArgs e)
        {
            InitializeBackbuffer();
        }

        private void cmbExpr_TextChanged(object sender, EventArgs e)
        {
            CompileFunction();
            RepaintCanvas();
        }

        private void chkIgnoreCase_CheckedChanged(object sender, EventArgs e)
        {
            CompileFunction();
            RepaintCanvas();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            RepaintCanvas();
        }

        private void InitializeBackbuffer()
        {
            var w = display.Width;
            var h = display.Height;
            if (backbuffer != null && backbuffer.Width == w && backbuffer.Height == h)
            {
                return;
            }
            evalContext.SetVariable("W", w);
            evalContext.SetVariable("H", h);
            backbuffer = new Bitmap(w, h, PixelFormat.Format8bppIndexed);
            if (bwPalette == null)
            {
                var palette = backbuffer.Palette;
                for (var i = 0; i < palette.Entries.Length; i++)
                {
                    palette.Entries[i] = Color.FromArgb(i, i, i);
                }
                bwPalette = palette;
            }
            backbuffer.Palette = bwPalette;
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
            evalContext.SetIgnoreCase(chkIgnoreCase.Checked);
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
            evalContext.SetVariable("rt", phase);
        }

        private void RepaintCanvas()
        {
            if (f == null) return;
            InitializeBackbuffer();

            var w = backbuffer.Width;
            var h = backbuffer.Height;

            var bmpData = backbuffer.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            Action<int> lineProcessor =
                y =>
                    {
                        var line = bmpData.Scan0 + bmpData.Stride*y;
                        for (var x = 0; x < w; x++)
                        {
                            var v = f(x, y, (double) x/w - 0.5, (double) y/h - 0.5);
                            Marshal.WriteByte(
                                line + x,
                                (byte) (Math.Min(1.0, Math.Max(0.0, v))*255.0));
                        }
                    };

            UpdateContext();

            if (chkParallel.Checked)
            {
                Parallel.For(0, h, lineProcessor);
            }
            else
            {
                for (var y = 0; y < h; y++)
                {
                    lineProcessor(y);
                }
            }

            backbuffer.UnlockBits(bmpData);

            backbuffer = Interlocked.Exchange(ref frontbuffer, backbuffer);

            display.Picture = frontbuffer;
        }
    }
}
