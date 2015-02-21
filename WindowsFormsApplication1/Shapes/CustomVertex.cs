using System.Drawing;

namespace WindowsFormsApplication1.Shapes
{
    public class CustomVertex
    {
        public CustomVertex(Point point)
        {
            Point = point;
        }

        public Point Point { get; set; }
    }
}
