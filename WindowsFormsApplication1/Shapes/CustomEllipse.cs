using System.Drawing;

namespace WindowsFormsApplication1.Shapes
{
    public class CustomEllipse : IShape
    {
        public CustomEllipse(Point p)
        {
            Point = p;
        }

        public void Draw(Graphics graphics, Color color, int thickness)
        {
            graphics.FillEllipse(new SolidBrush(color), new Rectangle(new Point(Point.X - 5, Point.Y - 5), new Size(10, 10)));
        }
        public Point Point { get; set; }
    }
}
