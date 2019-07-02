using System.Drawing;

namespace SimplePaint.Shapes
{
    /// <summary>
    /// Ellipse representing the vertex
    /// </summary>
    public class CustomEllipse : IShape
	{
		private const int Delta = 10;

		/// <summary>
		/// Gets or sets the position of the ellipse.
		/// </summary>
		public Point Position { get; set; }

        /// <summary>
        /// Gets the value determining whether this vertex is selected.
        /// </summary>
        public bool IsSelected { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CustomEllipse"/> class.
		/// </summary>
		/// <param name="position">The position.</param>
		public CustomEllipse(Point position)
        {
            Position = position;
		}

		/// <summary>
		/// Selects the vertex.
		/// </summary>
		public void Select()
		{
			IsSelected = true;
		}

		/// <summary>
		/// Deselects the vertex.
		/// </summary>
		public void Deselect()
		{
			IsSelected = false;
		}

		/// <summary>
		/// Determines whether this vertex can be dragged by the given deltas.
		/// </summary>
		/// <param name="deltaX">Change in x coordinate.</param>
		/// <param name="deltaY">Change in y coordinate.</param>
		/// <param name="xBound">Max x position that can be set.</param>
		/// <param name="yBound">Max y position that can be set.</param>
		/// <returns>True if this vertex can be dragged, false otherwise.</returns>
		public bool CanDrag(int deltaX, int deltaY, int xBound, int yBound)
		{
			return Position.X + deltaX > Delta
			       && Position.X + deltaX < xBound - Delta
			       && Position.Y + deltaY < yBound - Delta
			       && deltaY + Position.Y > Delta;
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
