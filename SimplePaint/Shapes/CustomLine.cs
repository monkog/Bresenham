using System;
using System.Drawing;

namespace SimplePaint.Shapes
{
	/// <summary>
	/// Represents a line
	/// </summary>
	public class CustomLine : IShape
	{
		private static readonly Size PixelSize = new Size(1, 1);

		/// <summary>
		/// Gets or sets the start point.
		/// </summary>
		public Point StartPoint { get; set; }

		/// <summary>
		/// Gets or sets the end point.
		/// </summary>
		public Point EndPoint { get; set; }

		public CustomLine(Point startPoint, Point endPoint)
		{
			StartPoint = startPoint;
			EndPoint = endPoint;
		}

		/// <inheritdoc/>
		public void Draw(Graphics graphics, Color color, int thickness)
		{
			var brush = new SolidBrush(color);
			SymmetricBresenham(graphics, brush, thickness);
			Multisampling(graphics, color, thickness);
		}

		private void SymmetricBresenham(Graphics graphics, Brush brush, int thickness)
		{
			var upPixels = thickness / 2;
			var downPixels = thickness - upPixels;

			var direction = new LineDirection(this);
			DrawPixel(graphics, brush, StartPoint);
			DrawPixel(graphics, brush, EndPoint);

			if (direction.Dx > direction.Dy)
				DrawHorizontalLine(graphics, brush, direction, upPixels, downPixels);
			else
				DrawVerticalLine(graphics, brush, direction, upPixels, downPixels);
		}

		private void DrawHorizontalLine(Graphics graphics, Brush brush, LineDirection direction, int upPixels, int downPixels)
		{
			var x = StartPoint.X;
			var y = StartPoint.Y;

			// The precise step delta
			var d0 = 2 * direction.Dy - direction.Dx;
			var dE = 2 * direction.Dy;
			var dNe = 2 * (direction.Dy - direction.Dx);

			while (x != EndPoint.X)
			{
				if (d0 < 0) //Choose E and W
				{
					d0 += dE;
					x += direction.IncrementX;
				}
				else //Choose NE and SW
				{
					d0 += dNe;
					y += direction.IncrementY;
					x += direction.IncrementX;
				}

				DrawLineSegment(graphics, x, y, upPixels, downPixels, brush, true);
			}
		}

		private void DrawVerticalLine(Graphics graphics, Brush brush, LineDirection direction, int upPixels, int downPixels)
		{
			var x = StartPoint.X;
			var y = StartPoint.Y;

			// The precise step delta
			var d0 = 2 * direction.Dx - direction.Dy;
			var dE = 2 * direction.Dx;
			var dNe = 2 * (direction.Dx - direction.Dy);

			while (y != EndPoint.Y)
			{
				if (d0 < 0) //Choose E and W
				{
					d0 += dE;
					y += direction.IncrementY;
				}
				else //Choose NE and SW
				{
					d0 += dNe;
					y += direction.IncrementY;
					x += direction.IncrementX;
				}

				DrawLineSegment(graphics, x, y, upPixels, downPixels, brush, false);
			}
		}

		private static void DrawLineSegment(Graphics graphics, int x1, int y1, int upPixels, int downPixels, Brush brush, bool isLineHorizontal)
		{
			var horizontalMultiplier = isLineHorizontal ? 0 : 1;
			var verticalMultiplier = 1 - horizontalMultiplier;

			for (var i = 0; i <= upPixels; i++)
				DrawPixel(graphics, brush, new Point(x1 + i * horizontalMultiplier, y1 + i * verticalMultiplier));

			for (var i = 0; i <= downPixels; i++)
				DrawPixel(graphics, brush, new Point(x1 - i * horizontalMultiplier, y1 - i * verticalMultiplier));
		}

		private void Multisampling(Graphics graphics, Color color, int thickness)
		{
			return;
			Bitmap bmp;
			//= new Bitmap(Math.Abs(StartPoint.X - EndPoint.X), Math.Abs(StartPoint.Y - EndPoint.Y));

			//Pen pen = new Pen(new SolidBrush(Color.Blue));
			//pen.EndCap = LineCap.Square;
			//pen.StartCap = LineCap.Square;
			//Point[] points = { new Point(0, 0), new Point(0, 2), new Point(bmp.Width - 2, bmp.Height), new Point(bmp.Width, bmp.Height) };
			//graphics.DrawPolygon(pen, points);
			if (StartPoint == EndPoint) return;

			bmp = new Bitmap(Math.Abs(StartPoint.X - EndPoint.X), Math.Abs(StartPoint.Y - EndPoint.Y), graphics);
			Point point;
			double alpha;
			for (var i = 0; i < bmp.Width; i += 2)
				for (var j = 0; j < bmp.Height; i += 2)
				{
					alpha = 0;
					point = new Point(i / 2, j / 2);
					for (var k = 0; k < 2; k++)
						for (var l = 0; l < 2; l++)
							if (bmp.GetPixel(k, l) == Color.Blue)
								alpha++;
					alpha /= 4;
					graphics.FillRectangle(new SolidBrush(Color.FromArgb((int)(alpha * 255), color)), new Rectangle(point, PixelSize));
				}
		}

		void IntensifyPixel(int x, int y, double distance, Graphics graphics, Color color)
		{
			var intensity = Math.Round(Math.Abs(distance));
			var intIntensity = (int)(intensity * 255);
			graphics.FillRectangle(new SolidBrush(Color.FromArgb(intIntensity, color)), new Rectangle(new Point(x, y), PixelSize));
			//WritePixel(x, y, intensity);
		}

		private static void DrawPixel(Graphics graphics, Brush brush, Point point)
		{
			graphics.FillRectangle(brush, new Rectangle(point, new Size(1, 1)));
		}
	}
}
