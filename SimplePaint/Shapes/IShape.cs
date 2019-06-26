using System.Drawing;

namespace SimplePaint.Shapes
{
    /// <summary>
    /// Interface representing all shapes that can be drawn
    /// </summary>
    public interface IShape
    {
		/// <summary>
		/// Draws the current shape.
		/// </summary>
		/// <param name="graphics">Graphics instance.</param>
		/// <param name="color">Color of the shape.</param>
		/// <param name="thickness">Thickness of the shape.</param>
        void Draw(Graphics graphics, Color color, int thickness);
    }
}
