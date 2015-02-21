using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace WindowsFormsApplication1.Shapes
{
    /// <summary>
    /// Represents a line
    /// </summary>
    public class CustomLine : IShape
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets the start point.
        /// </summary>
        /// <value>
        /// The start point.
        /// </value>
        public Point StartPoint { get; set; }
        /// <summary>
        /// Gets or sets the end point.
        /// </summary>
        /// <value>
        /// The end point.
        /// </value>
        public Point EndPoint { get; set; }
        #endregion Public Properties
        #region .ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomLine"/> class.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="endPoint">The end point.</param>
        public CustomLine(Point startPoint, Point endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
        }
        #endregion .ctor
        #region Public Methods
        /// <summary>
        /// Draws the line.
        /// </summary>
        /// <param name="graphics">The graphics.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">The thickness of the line.</param>
        public void Draw(Graphics graphics, Color color, int thickness)
        {
            SymmetricBresenham(graphics, color, thickness);
            Multisampling(graphics, color, thickness);
        }
        #endregion Public Methods
        #region Private Methods
        /// <summary>
        /// Draws the line using the Symmetric Bresenham algorythm.
        /// </summary>
        /// <param name="graphics">The graphics.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">The thickness of the line.</param>
        private void SymmetricBresenham(Graphics graphics, Color color, int thickness)
        {
            int upPixels = (thickness - 1) / 2;
            int downPixels = thickness - 1 - upPixels;

            int x1 = StartPoint.X, x2 = EndPoint.X, y1 = StartPoint.Y, y2 = EndPoint.Y;
            int incrX, incrY, dx, dy;

            FindDrawingStep(x1, x2, y1, y2, out incrX, out incrY, out dx, out dy);

            graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(x1, y1), new Size(1, 1)));
            graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(x2, y2), new Size(1, 1)));

            if (dx > dy)
                DrawLine(graphics, color, x1, y1, x2, y2, dy, dx, incrX, incrY, upPixels, downPixels, isHorizontal: true);
            else
                DrawLine(graphics, color, y1, x1, y2, x2, dx, dy, incrY, incrX, upPixels, downPixels, isHorizontal: false);
        }
        /// <summary>
        /// Draws the line.
        /// </summary>
        /// <param name="graphics">The graphics.</param>
        /// <param name="color">The color.</param>
        /// <param name="x1">The x coordinate of the start point.</param>
        /// <param name="x2">The x coordinate of the end point.</param>
        /// <param name="y1">The y coordinate of the start point.</param>
        /// <param name="y2">The y coordinate of the end point.</param>
        /// <param name="incrX">The direction x coordinate. Positive value is right.</param>
        /// <param name="incrY">The direction y coordinate. Positive value is up.</param>
        /// <param name="dx">The step in x coordinate.</param>
        /// <param name="dy">The step in y coordinate.</param>
        /// <param name="upPixels">Number of pixels to copy above the line.</param>
        /// <param name="downPixels">Number of pixels to copy below the line.</param>
        /// <param name="isHorizontal">Is the line horizontal</param>
        private static void DrawLine(Graphics graphics, Color color, int x1, int y1, int x2, int y2
            , int dy, int dx, int incrX, int incrY, int upPixels, int downPixels, bool isHorizontal)
        {
            int xf = x1;
            int yf = y1;
            int xb = x2;
            int yb = y2;
            var incrE = 2 * dy;
            var incrNe = 2 * (dy - dx);
            var d = 2 * dy - dx;

            while (xf != xb && xf - 1 != xb && xf + 1 != xb)
            {
                xf += incrX;
                xb -= incrX;
                if (d < 0) //Choose E and W
                    d += incrE;
                else //Choose NE and SW
                {
                    d += incrNe;
                    yf += incrY;
                    yb -= incrY;
                }

                if (isHorizontal)
                    DrawLineSegment(graphics, xf, yf, xb, yb, upPixels, downPixels, color, isHorizontal);
                else
                    DrawLineSegment(graphics, yf, xf, yb, xb, upPixels, downPixels, color, isHorizontal);
            }
        }
        /// <summary>
        /// Draws the line segment.
        /// </summary>
        /// <param name="graphics">The graphics.</param>
        /// <param name="x1">The x coordinate of the start point.</param>
        /// <param name="y1">The y coordinate of the start point.</param>
        /// <param name="x2">The x coordinate of the end point.</param>
        /// <param name="y2">The y coordinate of the end point.</param>
        /// <param name="upPixels">Number of pixels to copy above the line.</param>
        /// <param name="downPixels">Number of pixels to copy below the line.</param>
        /// <param name="color">The color.</param>
        /// <param name="isLineHorizontal">Is the line's steapness less than 45 deg</param>
        private static void DrawLineSegment(Graphics graphics, int x1, int y1, int x2, int y2
            , int upPixels, int downPixels, Color color, bool isLineHorizontal)
        {
            int horizontalMultiplier = isLineHorizontal ? 1 : 0;
            int verticalMultiplier = 1 - horizontalMultiplier;
            for (int i = 0; i <= upPixels; i++)
            {
                graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(
                    x1 + i * horizontalMultiplier, y1 + i * verticalMultiplier), new Size(1, 1)));
                graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(
                    x2 + i * horizontalMultiplier, y2 + i * verticalMultiplier), new Size(1, 1)));
            }
            for (int i = 0; i <= downPixels; i++)
            {
                graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(
                    x1 - i * horizontalMultiplier, y1 - i * verticalMultiplier), new Size(1, 1)));
                graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(
                    x2 - i * horizontalMultiplier, y2 - i * verticalMultiplier), new Size(1, 1)));
            }
        }
        /// <summary>
        /// Finds the drawing step.
        /// </summary>
        /// <remarks>
        /// Finds the direction of drawing in x and y coordinate and the step for each iteration
        /// </remarks>
        /// <param name="x1">The x coordinate of the start point.</param>
        /// <param name="x2">The x coordinate of the end point.</param>
        /// <param name="y1">The y coordinate of the start point.</param>
        /// <param name="y2">The y coordinate of the end point.</param>
        /// <param name="incrX">The direction x coordinate. Positive value is right.</param>
        /// <param name="incrY">The direction y coordinate. Positive value is up.</param>
        /// <param name="dx">The step in x coordinate.</param>
        /// <param name="dy">The step in y coordinate.</param>
        private void FindDrawingStep(int x1, int x2, int y1, int y2
            , out int incrX, out int incrY, out int dx, out int dy)
        {
            // Find the direction of drawing in x coordinate
            incrX = x1 < x2 ? 1 : -1;
            dx = Math.Abs(x1 - x2);

            // Find the direction of drawing in y coordinate
            incrY = y1 < y2 ? 1 : -1;
            dy = Math.Abs(y1 - y2);
        }

        private void Multisampling(Graphics graphics, Color color, int thickness)
        {
            return;
            Bitmap bmp ;
            //= new Bitmap(Math.Abs(StartPoint.X - EndPoint.X), Math.Abs(StartPoint.Y - EndPoint.Y));

            //Pen pen = new Pen(new SolidBrush(Color.Blue));
            //pen.EndCap = LineCap.Square;
            //pen.StartCap = LineCap.Square;
            //Point[] points = { new Point(0, 0), new Point(0, 2), new Point(bmp.Width - 2, bmp.Height), new Point(bmp.Width, bmp.Height) };
            //graphics.DrawPolygon(pen, points);
            if(StartPoint == EndPoint)return;
           
            bmp = new Bitmap(Math.Abs(StartPoint.X - EndPoint.X), Math.Abs(StartPoint.Y - EndPoint.Y), graphics);
            Point point;
            double alpha;
            for (int i = 0; i < bmp.Width; i += 2)
                for (int j = 0; j < bmp.Height; i += 2)
                {
                    alpha = 0;
                    point = new Point(i / 2, j / 2);
                    for (int k = 0; k < 2; k++)
                        for (int l = 0; l < 2; l++)
                            if (bmp.GetPixel(k, l) == Color.Blue)
                                alpha++;
                    alpha /= 4;
                    graphics.FillRectangle(new SolidBrush(Color.FromArgb((int)(alpha * 255), color)), new Rectangle(point, new Size(1, 1)));
                }
        }

        void IntensifyPixel(int x, int y, double distance, Graphics graphics, Color color)
        {
            double intensity = Math.Round(Math.Abs(distance));
            int intIntensity = (int)(intensity * 255);
            graphics.FillRectangle(new SolidBrush(Color.FromArgb(intIntensity, color)), new Rectangle(new Point(x, y), new Size(1, 1)));
            //WritePixel(x, y, intensity);
        }
        #endregion Private Methods
    }
}
