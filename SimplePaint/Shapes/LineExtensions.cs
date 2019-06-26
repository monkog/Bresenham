using System;
using System.Drawing;

namespace SimplePaint.Shapes
{
	public static class LineExtensions
	{
		/// <summary>
		/// Finds the direction of drawing in x and y coordinate.
		/// </summary>
		/// <param name="line">The line to find the direction for.</param>
		/// <returns>Point representing the direction of drawing.</returns>
		public static Point FindDrawingDirection(this CustomLine line)
		{
			// Find the direction of drawing in x coordinate
			var xDirection = line.StartPoint.X < line.EndPoint.X ? 1 : -1;
			var dx = Math.Abs(line.StartPoint.X - line.EndPoint.X);

			// Find the direction of drawing in y coordinate
			var yDirection = line.StartPoint.Y < line.EndPoint.Y ? 1 : -1;
			var dy = Math.Abs(line.StartPoint.Y - line.EndPoint.Y);

			return new Point(xDirection * dx, yDirection * dy);
		}
	}
}
