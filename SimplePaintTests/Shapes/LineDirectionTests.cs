using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimplePaint.Shapes;

namespace SimplePaintTests.Shapes
{
	[TestClass]
	public class LineDirectionTests
	{
		[DataTestMethod]
		[DataRow(1, 2, 3, 4, 2, 2, 1, 1)]
		[DataRow(3, 4, 1, 2, 2, 2, -1, -1)]
		[DataRow(1, 1, 1, 1, 0, 0, 0, 0)]
		public void Ctor_Line_PropertiesAssigned(int x1, int y1, int x2, int y2, int dx, int dy, int incrementX, int incrementY)
		{
			var line = new CustomLine(new Point(x1, y1), new Point(x2, y2));

			var lineDirection = new LineDirection(line);

			Assert.AreEqual(dx, lineDirection.Dx);
			Assert.AreEqual(dy, lineDirection.Dy);
			Assert.AreEqual(incrementX, lineDirection.IncrementX);
			Assert.AreEqual(incrementY, lineDirection.IncrementY);
		}
	}
}
