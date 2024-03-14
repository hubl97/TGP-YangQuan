using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace NewTGP
{
    public partial class PSWPage : Form
    {
        public string psw = "TGPSET";
        public bool bPass = false;
        public PSWPage()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterParent;
            textBox1.PasswordChar = '*';
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            bPass = false;
            
            if(textBox1.Text == string.Empty)
            {
                MessageBox.Show("未输入密码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }        
           
            else if(textBox1.Text == psw||textBox1.Text.ToUpper()==psw)
            {              
                textBox1.Text = string.Empty;             
                bPass = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("密码错误", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox1.Text = string.Empty;
            }

 
            /*
            try
            {
                if (textBox1.Text == "")
                {
                    MessageBox.Show("密码不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else
                {
                    string Ipsw = textBox1.Text.Trim();
                    if (Ipsw == psw)
                    {
                        this.Hide();
                        TGPDisp.TGPconfig.Show();
                    }
                    else
                    {
                        textBox1.Text = "";
                        MessageBox.Show("密码错误", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }*/
        }

    }
}
