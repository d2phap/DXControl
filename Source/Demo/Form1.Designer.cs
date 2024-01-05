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
            canvas = new DXCanvas();
            chkD2D = new CheckBox();
            chkAnimation = new CheckBox();
            SuspendLayout();
            // 
            // canvas
            // 
            canvas.AllowDrop = true;
            canvas.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            canvas.BackColor = Color.FromArgb(128, 128, 255);
            canvas.BaseDpi = 96F;
            canvas.Bitmap = null;
            canvas.CheckFPS = true;
            canvas.EnableAnimation = true;
            canvas.Location = new Point(20, 90);
            canvas.Margin = new Padding(6, 5, 6, 5);
            canvas.Name = "canvas";
            canvas.RequestUpdateFrame = true;
            canvas.Size = new Size(2114, 1293);
            canvas.TabIndex = 0;
            canvas.Text = "dxCanvas1";
            canvas.DragDrop += canvas_DragDrop;
            canvas.DragOver += canvas_DragOver;
            // 
            // chkD2D
            // 
            chkD2D.AutoSize = true;
            chkD2D.Checked = true;
            chkD2D.CheckState = CheckState.Checked;
            chkD2D.Location = new Point(20, 25);
            chkD2D.Margin = new Padding(6, 5, 6, 5);
            chkD2D.Name = "chkD2D";
            chkD2D.Size = new Size(372, 49);
            chkD2D.TabIndex = 1;
            chkD2D.Text = "Use Direct2D graphics";
            chkD2D.UseVisualStyleBackColor = true;
            chkD2D.CheckedChanged += chkD2D_CheckedChanged;
            // 
            // chkAnimation
            // 
            chkAnimation.AutoSize = true;
            chkAnimation.Checked = true;
            chkAnimation.CheckState = CheckState.Checked;
            chkAnimation.Location = new Point(484, 25);
            chkAnimation.Margin = new Padding(6, 5, 6, 5);
            chkAnimation.Name = "chkAnimation";
            chkAnimation.Size = new Size(303, 49);
            chkAnimation.TabIndex = 2;
            chkAnimation.Text = "Enable animation";
            chkAnimation.UseVisualStyleBackColor = true;
            chkAnimation.CheckedChanged += ChkAnimation_CheckedChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(18F, 45F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2154, 1403);
            Controls.Add(chkAnimation);
            Controls.Add(chkD2D);
            Controls.Add(canvas);
            Margin = new Padding(6, 5, 6, 5);
            Name = "Form1";
            Padding = new Padding(20);
            Text = "D2Phap.DXControl demo";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DXCanvas canvas;
        private CheckBox chkD2D;
        private CheckBox chkAnimation;
    }
}