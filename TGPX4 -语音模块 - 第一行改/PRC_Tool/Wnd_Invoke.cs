using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace PRC_Tool
{
    public class Wnd_Invoke
    {
        delegate void ControlText_ChangeCallback(Control parent, Control c, string s);
        public static void UpdateText(Control parent, Control c, string s)
        {
            if (c.IsDisposed) return;
            try
            {
                if (c.InvokeRequired)
                {
                    ControlText_ChangeCallback cc = new ControlText_ChangeCallback(UpdateText);
                    parent.Invoke(cc, new object[] { parent, c, s });
                }
                else
                {
                    c.Text = s;
                }
            }
            catch
            {
            }
        }

        public static void AddItem(Control parent, ListBox c, string s)
        {
            if (c.IsDisposed) return;
            try
            {
                if (c.InvokeRequired)
                {
                    ControlText_ChangeCallback cc = new ControlText_ChangeCallback(UpdateText);
                    parent.Invoke(cc, new object[] { parent, c, s });
                }
                else
                {
                    c.Items.Add(s);
                }
            }
            catch
            {
            }

        }
    }
}
