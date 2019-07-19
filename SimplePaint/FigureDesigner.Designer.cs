namespace SimplePaint
{
    partial class FigureDesigner
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
			this.drawingArea = new System.Windows.Forms.PictureBox();
			this.buttonsPanel = new System.Windows.Forms.Panel();
			this.multisamplingButton = new System.Windows.Forms.Button();
			this.sizeLabel = new System.Windows.Forms.Label();
			this.changeSizeButton = new System.Windows.Forms.Button();
			this.colorPictureBox = new System.Windows.Forms.PictureBox();
			this.colorLabel = new System.Windows.Forms.Label();
			this.changeColorButton = new System.Windows.Forms.Button();
			this.clearButton = new System.Windows.Forms.Button();
			this.drawFigureButton = new System.Windows.Forms.Button();
			this.addVertexButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.drawingArea)).BeginInit();
			this.buttonsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.colorPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// drawingArea
			// 
			this.drawingArea.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.drawingArea.BackColor = System.Drawing.SystemColors.Window;
			this.drawingArea.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.drawingArea.Location = new System.Drawing.Point(114, 6);
			this.drawingArea.Name = "drawingArea";
			this.drawingArea.Size = new System.Drawing.Size(793, 604);
			this.drawingArea.TabIndex = 0;
			this.drawingArea.TabStop = false;
			this.drawingArea.Paint += new System.Windows.Forms.PaintEventHandler(this.RedrawDrawingArea);
			this.drawingArea.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MouseDownOccured);
			this.drawingArea.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseMoveOccured);
			this.drawingArea.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUpOccured);
			// 
			// buttonsPanel
			// 
			this.buttonsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonsPanel.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.buttonsPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.buttonsPanel.Controls.Add(this.multisamplingButton);
			this.buttonsPanel.Controls.Add(this.sizeLabel);
			this.buttonsPanel.Controls.Add(this.changeSizeButton);
			this.buttonsPanel.Controls.Add(this.colorPictureBox);
			this.buttonsPanel.Controls.Add(this.colorLabel);
			this.buttonsPanel.Controls.Add(this.changeColorButton);
			this.buttonsPanel.Controls.Add(this.clearButton);
			this.buttonsPanel.Controls.Add(this.drawFigureButton);
			this.buttonsPanel.Controls.Add(this.addVertexButton);
			this.buttonsPanel.Location = new System.Drawing.Point(5, 6);
			this.buttonsPanel.Name = "buttonsPanel";
			this.buttonsPanel.Size = new System.Drawing.Size(103, 604);
			this.buttonsPanel.TabIndex = 1;
			// 
			// multisamplingButton
			// 
			this.multisamplingButton.Font = new System.Drawing.Font("Copperplate Gothic Bold", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.multisamplingButton.Location = new System.Drawing.Point(3, 286);
			this.multisamplingButton.Name = "multisamplingButton";
			this.multisamplingButton.Size = new System.Drawing.Size(94, 50);
			this.multisamplingButton.TabIndex = 8;
			this.multisamplingButton.Text = "MULTISAMPLING";
			this.multisamplingButton.UseVisualStyleBackColor = true;
			this.multisamplingButton.Click += new System.EventHandler(this.ToMultisamplingState);
			// 
			// sizeLabel
			// 
			this.sizeLabel.AutoSize = true;
			this.sizeLabel.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.sizeLabel.Font = new System.Drawing.Font("Copperplate Gothic Bold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.sizeLabel.Location = new System.Drawing.Point(5, 515);
			this.sizeLabel.MaximumSize = new System.Drawing.Size(100, 200);
			this.sizeLabel.Name = "sizeLabel";
			this.sizeLabel.Size = new System.Drawing.Size(90, 38);
			this.sizeLabel.TabIndex = 7;
			this.sizeLabel.Text = "CURRENT SIZE: 2 px";
			this.sizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// changeSizeButton
			// 
			this.changeSizeButton.Font = new System.Drawing.Font("Copperplate Gothic Bold", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.changeSizeButton.Location = new System.Drawing.Point(4, 230);
			this.changeSizeButton.Name = "changeSizeButton";
			this.changeSizeButton.Size = new System.Drawing.Size(94, 50);
			this.changeSizeButton.TabIndex = 6;
			this.changeSizeButton.Text = "CHANGE SIZE";
			this.changeSizeButton.UseVisualStyleBackColor = true;
			this.changeSizeButton.Click += new System.EventHandler(this.ToChangeThicknessState);
			// 
			// colorPictureBox
			// 
			this.colorPictureBox.BackColor = System.Drawing.Color.OliveDrab;
			this.colorPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.colorPictureBox.Location = new System.Drawing.Point(4, 398);
			this.colorPictureBox.Name = "colorPictureBox";
			this.colorPictureBox.Size = new System.Drawing.Size(94, 94);
			this.colorPictureBox.TabIndex = 5;
			this.colorPictureBox.TabStop = false;
			this.colorPictureBox.Click += new System.EventHandler(this.ToChangeColorState);
			// 
			// colorLabel
			// 
			this.colorLabel.AutoSize = true;
			this.colorLabel.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.colorLabel.Font = new System.Drawing.Font("Copperplate Gothic Bold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.colorLabel.Location = new System.Drawing.Point(5, 354);
			this.colorLabel.MaximumSize = new System.Drawing.Size(100, 200);
			this.colorLabel.Name = "colorLabel";
			this.colorLabel.Size = new System.Drawing.Size(90, 38);
			this.colorLabel.TabIndex = 4;
			this.colorLabel.Text = "CURRENT COLOR:";
			this.colorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// changeColorButton
			// 
			this.changeColorButton.Font = new System.Drawing.Font("Copperplate Gothic Bold", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.changeColorButton.Location = new System.Drawing.Point(4, 174);
			this.changeColorButton.Name = "changeColorButton";
			this.changeColorButton.Size = new System.Drawing.Size(94, 50);
			this.changeColorButton.TabIndex = 3;
			this.changeColorButton.Text = "CHANGE COLOR";
			this.changeColorButton.UseVisualStyleBackColor = true;
			this.changeColorButton.Click += new System.EventHandler(this.ToChangeColorState);
			// 
			// clearButton
			// 
			this.clearButton.Font = new System.Drawing.Font("Copperplate Gothic Bold", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.clearButton.Location = new System.Drawing.Point(4, 118);
			this.clearButton.Name = "clearButton";
			this.clearButton.Size = new System.Drawing.Size(94, 50);
			this.clearButton.TabIndex = 2;
			this.clearButton.Text = "CLEAR";
			this.clearButton.UseVisualStyleBackColor = true;
			this.clearButton.Click += new System.EventHandler(this.SetDefaultSettings);
			// 
			// drawFigureButton
			// 
			this.drawFigureButton.Font = new System.Drawing.Font("Copperplate Gothic Bold", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.drawFigureButton.Location = new System.Drawing.Point(4, 62);
			this.drawFigureButton.Name = "drawFigureButton";
			this.drawFigureButton.Size = new System.Drawing.Size(94, 50);
			this.drawFigureButton.TabIndex = 1;
			this.drawFigureButton.Text = "DRAW FIGURE";
			this.drawFigureButton.UseVisualStyleBackColor = true;
			this.drawFigureButton.Click += new System.EventHandler(this.ToDrawFigureState);
			// 
			// addVertexButton
			// 
			this.addVertexButton.Font = new System.Drawing.Font("Copperplate Gothic Bold", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.addVertexButton.Location = new System.Drawing.Point(4, 6);
			this.addVertexButton.Name = "addVertexButton";
			this.addVertexButton.Size = new System.Drawing.Size(94, 50);
			this.addVertexButton.TabIndex = 0;
			this.addVertexButton.Text = "ADD VERTEX";
			this.addVertexButton.UseVisualStyleBackColor = true;
			this.addVertexButton.Click += new System.EventHandler(this.ToAddVertexState);
			// 
			// FigureDesigner
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.ClientSize = new System.Drawing.Size(913, 616);
			this.Controls.Add(this.buttonsPanel);
			this.Controls.Add(this.drawingArea);
			this.Name = "FigureDesigner";
			this.Text = "Form1";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			((System.ComponentModel.ISupportInitialize)(this.drawingArea)).EndInit();
			this.buttonsPanel.ResumeLayout(false);
			this.buttonsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.colorPictureBox)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox drawingArea;
        private System.Windows.Forms.Panel buttonsPanel;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.Button drawFigureButton;
        private System.Windows.Forms.Button addVertexButton;
        private System.Windows.Forms.Button changeColorButton;
        private System.Windows.Forms.PictureBox colorPictureBox;
        private System.Windows.Forms.Label colorLabel;
        private System.Windows.Forms.Button changeSizeButton;
        private System.Windows.Forms.Label sizeLabel;
        private System.Windows.Forms.Button multisamplingButton;


    }
}

