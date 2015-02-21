using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace WindowsFormsApplication1.Shapes
{

    public class CustomLine : IShape
    {
        public CustomLine(Point p1, Point p2)
        {
            MPoint1 = p1;
            MPoint2 = p2;
        }

        public void Draw(Graphics graphics, Color color, int thickness)
        {
            //multisampling(graphics, color, thickness);
            SymmetricBresenham(graphics, color, thickness);
        }

        private void SymmetricBresenham(Graphics graphics, Color color, int thickness)
        {
            int upPixels = (thickness - 1) / 2;
            int downPixels = thickness - 1 - upPixels;

            int x1 = MPoint1.X, x2 = MPoint2.X, y1 = MPoint1.Y, y2 = MPoint2.Y;
            int incrX, incrY, d, dx, dy, incrNe, incrE;

            if (x1 < x2)
            {
                incrX = 1;
                dx = x2 - x1;
            }
            else
            {
                incrX = -1;
                dx = x1 - x2;
            }

            if (y1 < y2)
            {
                incrY = 1;
                dy = y2 - y1;
            }
            else
            {
                incrY = -1;
                dy = y1 - y2;
            }

            int xf = x1;
            int yf = y1;
            int xb = x2;
            int yb = y2;

            graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xf, yf), new Size(1, 1)));
            graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xb, yb), new Size(1, 1)));

            if (dx > dy)
            {
                incrE = 2 * dy;
                incrNe = 2 * (dy - dx);
                d = 2 * dy - dx;

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
                    graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xf, yf), new Size(1, 1)));
                    graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xb, yb), new Size(1, 1)));

                    for (int i = 1; i <= upPixels; i++)
                    {
                        graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xf, yf + i), new Size(1, 1)));
                        graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xb, yb + i), new Size(1, 1)));
                    }
                    for (int i = 1; i <= downPixels; i++)
                    {
                        graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xf, yf - i), new Size(1, 1)));
                        graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xb, yb - i), new Size(1, 1)));
                    }
                }
            }
            else
            {
                incrE = 2 * dx;
                incrNe = 2 * (dx - dy);
                d = 2 * dx - dy;

                while (yf != yb && yf - 1 != yb && yf + 1 != yb)
                {
                    yf += incrY;
                    yb -= incrY;

                    if (d < 0) //Choose E and W
                        d += incrE;
                    else //Choose NE and SW
                    {
                        d += incrNe;
                        xf += incrX;
                        xb -= incrX;
                    }
                    graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xf, yf), new Size(1, 1)));
                    graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xb, yb), new Size(1, 1)));

                    for (int i = 1; i <= upPixels; i++)
                    {
                        graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xf + i, yf), new Size(1, 1)));
                        graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xb + i, yb), new Size(1, 1)));
                    }
                    for (int i = 1; i <= downPixels; i++)
                    {
                        graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xf - i, yf), new Size(1, 1)));
                        graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xb - i, yb), new Size(1, 1)));
                    }
                }
            }
        }

        private void Multisampling(Graphics graphics, Color color, int thickness)
        {
            Bitmap bmp = new Bitmap(Math.Abs(MPoint1.X - MPoint2.X), Math.Abs(MPoint1.Y - MPoint2.Y));

            Pen pen = new Pen(new SolidBrush(Color.Blue));
            pen.EndCap = LineCap.Square;
            pen.StartCap = LineCap.Square;
            Point[] points = { new Point(0, 0), new Point(0, 2), new Point(bmp.Width - 2, bmp.Height), new Point(bmp.Width, bmp.Height) };
            graphics.DrawPolygon(pen, points);

            bmp = new Bitmap(Math.Abs(MPoint1.X - MPoint2.X), Math.Abs(MPoint1.Y - MPoint2.Y), graphics);
            graphics.Dispose();
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

        public Point MPoint1 { get; set; }
        public Point MPoint2 { get; set; }
    }
}
