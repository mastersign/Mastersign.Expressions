using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace de.mastersign.expressions.demo
{
    partial class MainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.lblHints = new System.Windows.Forms.Label();
            this.lblMessage = new System.Windows.Forms.Label();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.cmbExpr = new System.Windows.Forms.ComboBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.display = new de.mastersign.expressions.demo.Display();
            this.chkParallel = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lblHints
            // 
            this.lblHints.AutoSize = true;
            this.lblHints.Location = new System.Drawing.Point(12, 9);
            this.lblHints.Name = "lblHints";
            this.lblHints.Size = new System.Drawing.Size(151, 13);
            this.lblHints.TabIndex = 1;
            this.lblHints.Text = "Variables: W, H, X, Y, x, y, T, t";
            this.toolTip.SetToolTip(this.lblHints, resources.GetString("lblHints.ToolTip"));
            // 
            // lblMessage
            // 
            this.lblMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMessage.Location = new System.Drawing.Point(12, 51);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(512, 42);
            this.lblMessage.TabIndex = 2;
            // 
            // timer
            // 
            this.timer.Enabled = true;
            this.timer.Interval = 10;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // cmbExpr
            // 
            this.cmbExpr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbExpr.FormattingEnabled = true;
            this.cmbExpr.Items.AddRange(new object[] {
            "sin((x + t) * pi * 3) * sin((y - t) * pi * 3) * 0.5 + 0.5",
            "cos((sqrt(x^2 + y^2) + t) * pi * 4) * 0.5 + 0.5",
            "sin(t * pi * 10) * 0.5 + 0.5",
            "rand()",
            "if(X = 100 or Y = 100, 1, 0)",
            "if(sqrt(x^2 + y^2) > 0.3, 1, 0)",
            "if(mod(X + Y + (T / 50), 50) = 0 or mod(X - Y - (T / 50), 50) = 0, sin(pi * mod(T" +
                "/500f, 2f)) * 0.5 + 0.5, sin(pi * mod(T/2000f, 2f)) * 0.5 + 0.5)"});
            this.cmbExpr.Location = new System.Drawing.Point(12, 27);
            this.cmbExpr.Name = "cmbExpr";
            this.cmbExpr.Size = new System.Drawing.Size(513, 21);
            this.cmbExpr.TabIndex = 4;
            this.cmbExpr.TextChanged += new System.EventHandler(this.cmbExpr_TextChanged);
            // 
            // display
            // 
            this.display.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.display.BackColor = System.Drawing.Color.Black;
            this.display.Location = new System.Drawing.Point(12, 96);
            this.display.Name = "display";
            this.display.Picture = null;
            this.display.Size = new System.Drawing.Size(512, 512);
            this.display.TabIndex = 5;
            this.display.Resize += new System.EventHandler(this.display_SizeChanged);
            // 
            // chkParallel
            // 
            this.chkParallel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkParallel.AutoSize = true;
            this.chkParallel.Location = new System.Drawing.Point(446, 8);
            this.chkParallel.Name = "chkParallel";
            this.chkParallel.Size = new System.Drawing.Size(78, 17);
            this.chkParallel.TabIndex = 6;
            this.chkParallel.Text = "Multithread";
            this.chkParallel.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(537, 620);
            this.Controls.Add(this.chkParallel);
            this.Controls.Add(this.display);
            this.Controls.Add(this.cmbExpr);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.lblHints);
            this.Name = "MainForm";
            this.Text = "Mastersign.Expressions Demo";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblHints;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.ComboBox cmbExpr;
        private Display display;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.CheckBox chkParallel;
    }
}

