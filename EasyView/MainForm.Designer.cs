namespace EasyView
{
    partial class MainForm
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
            this.ThePictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.ThePictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // ThePictureBox
            // 
            this.ThePictureBox.BackColor = System.Drawing.Color.Black;
            this.ThePictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ThePictureBox.Location = new System.Drawing.Point(0, 0);
            this.ThePictureBox.Name = "ThePictureBox";
            this.ThePictureBox.Size = new System.Drawing.Size(524, 383);
            this.ThePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ThePictureBox.TabIndex = 0;
            this.ThePictureBox.TabStop = false;
            this.ThePictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.MainForm_Paint);
            this.ThePictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ThePictureBox_MouseDown);
            // 
            // targetForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 383);
            this.Controls.Add(this.ThePictureBox);
            this.Name = "targetForm";
            this.Text = "Form1";
            this.TopMost = true;
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.ThePictureBox_DragEnter);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.ThePictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox ThePictureBox;
    }
}

