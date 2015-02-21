using System.Windows.Forms;

namespace WindowsFormsApplication1.Helpers
{
    public static class PromptWindow
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form();
            prompt.StartPosition = FormStartPosition.CenterScreen;
            prompt.Width = 300;
            prompt.Height = 150;
            prompt.Text = caption;
            Label textLabel = new Label() { Left = 20, Top = 10, Height = 35, Width = 260, Text = text };
            TextBox textBox = new TextBox() { Left = 10, Top = 50, Width = 270 };
            Button confirmation = new Button() { Text = "OK", Left = 125, Width = 50, Top = 75 };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.ShowDialog();
            return textBox.Text;
        }
    }
}
