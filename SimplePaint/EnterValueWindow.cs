using System;
using System.Windows.Forms;
using SimplePaint.Properties;

namespace SimplePaint
{
	public partial class EnterValueWindow : Form
	{
		/// <summary>
		/// Gets the thickness of the line set by the user.
		/// </summary>
		public int LineThickness { get; private set; }

		public EnterValueWindow()
		{
			InitializeComponent();
		}

		private void OkClicked(object sender, EventArgs e)
		{
			ValidateLineThickness();
		}

		private void KeyPressed(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar != (char)Keys.Enter) return;
			ValidateLineThickness();
		}

		private void ValidateLineThickness()
		{
			if (!int.TryParse(LineThicknessTextbox.Text, out var lineThickness) || lineThickness < 0 || lineThickness > 10)
			{
				MessageBox.Show(Resources.ValueMustBeInRange, Resources.InvalidArgument, MessageBoxButtons.OK);
				return;
			}

			LineThickness = lineThickness;
			Close();
		}
	}
}
