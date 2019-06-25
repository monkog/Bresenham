using System.Drawing;

namespace SimplePaint.Shapes
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
