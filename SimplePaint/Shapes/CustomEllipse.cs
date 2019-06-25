using System.Drawing;

namespace SimplePaint.Shapes
{
    /// <summary>
    /// Ellipse representing the vertex
    /// </summary>
    public class CustomEllipse : IShape
    {
        /// <summary>
        /// Gets or sets the position of the ellipse.
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomEllipse"/> class.
        /// </summary>
        /// <param name="position">The position.</param>
        public CustomEllipse(Point position)
        {
            Position = position;
        }

        /// <inheritdoc/>
        public void Draw(Graphics graphics, Color color, int size)
        {
            size += 6;
            var delta = size / 2;
            graphics.FillEllipse(new SolidBrush(color), new Rectangle(new Point(Position.X - delta, Position.Y - delta), new Size(size, size)));
        }
    }
}
