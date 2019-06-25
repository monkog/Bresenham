using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimplePaint.Shapes;
using System.Drawing;

namespace SimplePaintTests.Shapes
{
	[TestClass]
	public class CustomEllipseTests
	{
		[TestMethod]
		public void Ctor_ValidParameters_PropertiesAssigned()
		{
			var position = new Point(1, 3);

			var unitUnderTest = new CustomEllipse(position);

			Assert.AreEqual(position, unitUnderTest.Position);
		}
	}
}
