using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace de.mastersign.expressions.demo
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
