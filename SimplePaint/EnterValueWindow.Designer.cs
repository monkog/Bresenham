namespace SimplePaint
{
	partial class EnterValueWindow
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
			this.LineThicknessLabel = new System.Windows.Forms.Label();
			this.LineThicknessTextbox = new System.Windows.Forms.TextBox();
			this.OkButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// LineThicknessLabel
			// 
			this.LineThicknessLabel.Location = new System.Drawing.Point(20, 10);
			this.LineThicknessLabel.Name = "LineThicknessLabel";
			this.LineThicknessLabel.Size = new System.Drawing.Size(260, 35);
			this.LineThicknessLabel.TabIndex = 0;
			this.LineThicknessLabel.Text = "Determine the thickness of line. For best results use value lower than 10 px.";
			this.LineThicknessLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// LineThicknessTextbox
			// 
			this.LineThicknessTextbox.Location = new System.Drawing.Point(10, 50);
			this.LineThicknessTextbox.Name = "LineThicknessTextbox";
			this.LineThicknessTextbox.Size = new System.Drawing.Size(270, 20);
			this.LineThicknessTextbox.TabIndex = 1;
			this.LineThicknessTextbox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.KeyPressed);
			// 
			// OkButton
			// 
			this.OkButton.Location = new System.Drawing.Point(125, 75);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = new System.Drawing.Size(50, 23);
			this.OkButton.TabIndex = 2;
			this.OkButton.Text = "OK";
			this.OkButton.UseVisualStyleBackColor = true;
			this.OkButton.Click += new System.EventHandler(this.OkClicked);
			// 
			// EnterValueWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 111);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.LineThicknessTextbox);
			this.Controls.Add(this.LineThicknessLabel);
			this.Name = "EnterValueWindow";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Choose line thickness";
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.KeyPressed);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label LineThicknessLabel;
		private System.Windows.Forms.TextBox LineThicknessTextbox;
		private System.Windows.Forms.Button OkButton;
	}
}