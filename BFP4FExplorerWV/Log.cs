using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BFP4FExplorerWV
{
    public static class Log
    {
        public static RichTextBox box = null;
        public static ToolStripProgressBar pb = null;
        public static readonly object sync = new object();

        public static void WriteLine(string s)
        {
            Write(s + "\n");
        }

        public static void Write(string s)
        {
            if (box == null)
                return;
            box.BeginInvoke((MethodInvoker)delegate()
            {
                box.AppendText(s);
                box.SelectionStart = box.Text.Length;
                box.ScrollToCaret();
            });
            Application.DoEvents();
        }

        public static void SetProgress(int min, int max, int value)
        {
            if (pb == null)
                return;
            pb.GetCurrentParent().BeginInvoke((MethodInvoker)delegate()
            {
                pb.Minimum = min;
                pb.Maximum = max;
                pb.Value = value;
            });
            Application.DoEvents();
        }
    }
}
