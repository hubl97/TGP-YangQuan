using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NewTGP
{
    public partial class Quit : Form
    {
        public int select = 0;
        public static int quit = 1;
        public static int hide = 2;
        public static int cancel = 3;
        public Quit()
        {
            InitializeComponent();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            select = cancel;
            this.Close();
        }

        private void button_hide_Click(object sender, EventArgs e)
        {
            select = hide;
            this.Close();
        }

        private void button_quit_Click(object sender, EventArgs e)
        {
            select = quit;
            this.Close();
        }
    }
}
