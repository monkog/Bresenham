using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimplePaint.Shapes;

namespace SimplePaintTests.Shapes
{
	[TestClass]
	public class LineExtensionsTests
	{
		[DataTestMethod]
		[DataRow(0, 0, 5, 4, 5, 4)]
		[DataRow(-5, -4, 0, 0, 5, 4)]
		[DataRow(-5, -4, 5, 4, 10, 8)]
		[DataRow(5, 4, -5, -4, -10, -8)]
		public void FindLineDirection_LineDescription_DirectionFound(int startX, int startY, int endX, int endY, int xDirection, int yDirection)
		{
			var line = new CustomLine(new Point(startX, startY), new Point(endX, endY));

			var direction = line.FindDrawingDirection();

			Assert.AreEqual(xDirection, direction.X);
			Assert.AreEqual(yDirection, direction.Y);
		}
	}
}
