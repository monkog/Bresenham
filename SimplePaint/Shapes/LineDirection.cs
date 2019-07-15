using System;

namespace SimplePaint.Shapes
{
	public class LineDirection
	{
		/// <summary>
		/// Gets the abs difference in X coordinate between the end point and the start point.
		/// </summary>
		public int Dx { get; }

		/// <summary>
		/// Gets the abs difference in Y coordinate between the end point and the start point.
		/// </summary>
		public int Dy { get; }

		/// <summary>
		/// Gets the difference sign in X coordinate between the end point and the start point.
		/// </summary>
		public int IncrementX { get; }

		/// <summary>
		/// Gets the difference sign in Y coordinate between the end point and the start point.
		/// </summary>
		public int IncrementY { get; }

		public LineDirection(CustomLine line)
		{
			var dx = line.EndPoint.Position.X - line.StartPoint.Position.X;
			IncrementX = Math.Sign(dx);
			Dx = Math.Abs(dx);

			var dy = line.EndPoint.Position.Y - line.StartPoint.Position.Y;
			IncrementY = Math.Sign(dy);
			Dy = Math.Abs(dy);
		}
	}
}
