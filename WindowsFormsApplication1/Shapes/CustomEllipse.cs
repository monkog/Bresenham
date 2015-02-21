using System.Drawing;

namespace WindowsFormsApplication1.Shapes
{
    /// <summary>
    /// Ellipse representing the vertex
    /// </summary>
    public class CustomEllipse : IShape
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets the position of the ellipse.
        /// </summary>
        /// <value>
        /// The position of the ellipse.
        /// </value>
        public Point Position { get; set; }
        #endregion Public Properties
        #region .ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomEllipse"/> class.
        /// </summary>
        /// <param name="position">The position.</param>
        public CustomEllipse(Point position)
        {
            Position = position;
        }
        #endregion .ctor
        #region IShape
        /// <summary>
        /// Draws the ellipse.
        /// </summary>
        /// <param name="graphics">The graphics.</param>
        /// <param name="color">The color.</param>
        /// <param name="size">The size of ellipse.</param>
        public void Draw(Graphics graphics, Color color, int size)
        {
            size = size + 6;
            int delta = size / 2;
            graphics.FillEllipse(new SolidBrush(color), new Rectangle(new Point(Position.X - delta, Position.Y - delta), new Size(size, size)));
        }
        #endregion IShape
    }
}
