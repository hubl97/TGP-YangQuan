namespace NewTGP
{
    partial class Quit
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button_quit = new System.Windows.Forms.Button();
            this.button_hide = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_quit
            // 
            this.button_quit.Location = new System.Drawing.Point(13, 78);
            this.button_quit.Name = "button_quit";
            this.button_quit.Size = new System.Drawing.Size(75, 23);
            this.button_quit.TabIndex = 0;
            this.button_quit.Text = "退出";
            this.button_quit.UseVisualStyleBackColor = true;
            this.button_quit.Click += new System.EventHandler(this.button_quit_Click);
            // 
            // button_hide
            // 
            this.button_hide.Location = new System.Drawing.Point(106, 77);
            this.button_hide.Name = "button_hide";
            this.button_hide.Size = new System.Drawing.Size(75, 23);
            this.button_hide.TabIndex = 1;
            this.button_hide.Text = "隐藏";
            this.button_hide.UseVisualStyleBackColor = true;
            this.button_hide.Visible = false;
            this.button_hide.Click += new System.EventHandler(this.button_hide_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.Location = new System.Drawing.Point(204, 77);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(75, 23);
            this.button_cancel.TabIndex = 2;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // Quit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(290, 131);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_hide);
            this.Controls.Add(this.button_quit);
            this.Name = "Quit";
            this.Text = "退出确认";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_quit;
        private System.Windows.Forms.Button button_hide;
        private System.Windows.Forms.Button button_cancel;
    }
}