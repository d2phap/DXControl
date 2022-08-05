namespace Demo
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.canvas = new Demo.DXCanvas();
            this.chkD2D = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // canvas
            // 
            this.canvas.AllowDrop = true;
            this.canvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.canvas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.canvas.BaseDpi = 96F;
            this.canvas.CheckFPS = true;
            this.canvas.Location = new System.Drawing.Point(10, 46);
            this.canvas.Name = "canvas";
            this.canvas.RequestUpdateFrame = true;
            this.canvas.Size = new System.Drawing.Size(780, 394);
            this.canvas.TabIndex = 0;
            this.canvas.Text = "dxCanvas1";
            this.canvas.DragDrop += new System.Windows.Forms.DragEventHandler(this.canvas_DragDrop);
            this.canvas.DragOver += new System.Windows.Forms.DragEventHandler(this.canvas_DragOver);
            // 
            // chkD2D
            // 
            this.chkD2D.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkD2D.AutoSize = true;
            this.chkD2D.Checked = true;
            this.chkD2D.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkD2D.Location = new System.Drawing.Point(590, 13);
            this.chkD2D.Name = "chkD2D";
            this.chkD2D.Size = new System.Drawing.Size(200, 27);
            this.chkD2D.TabIndex = 1;
            this.chkD2D.Text = "Use Direct2D graphics";
            this.chkD2D.UseVisualStyleBackColor = true;
            this.chkD2D.CheckedChanged += new System.EventHandler(this.chkD2D_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.chkD2D);
            this.Controls.Add(this.canvas);
            this.Name = "Form1";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Text = "D2Phap.DXControl demo";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DXCanvas canvas;
        private CheckBox chkD2D;
    }
}