using System.Drawing;

namespace WindowsFormsApplication1.Shapes
{
    /// <summary>
    /// Ellipse representing the vertex
    /// </summary>
    public class CustomEllipse : IShape
    {
        #region Private Members
        /// <summary>
        /// The size of the ellipse
        /// </summary>
        private int _size;
        #endregion Private Members
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
        /// <param name="size">The size.</param>
        public CustomEllipse(Point position, int size)
        {
            Position = position;
            _size = size;
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
            graphics.FillEllipse(new SolidBrush(color), new Rectangle(new Point(Position.X - 5, Position.Y - 5), new Size(_size, _size)));
        }
        #endregion IShape
    }
}
