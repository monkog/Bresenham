using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimplePaint.Shapes;

namespace SimplePaintTests.Shapes
{
	[TestClass]
	public class CustomFigureTests
	{
		/// <summary>
		/// Figure shape looks like that:
		/// 
		/// |\  /|
		/// | \/ |
		/// |	 |
		/// |____|
		/// </summary>
		private CustomFigure _unitUnderTest;

		[TestInitialize]
		public void Initialize()
		{
			_unitUnderTest = new CustomFigure(new Point(-10, 10), Color.Black, 2);
			_unitUnderTest.AddVertex(new Point(-10, -10));
			_unitUnderTest.AddVertex(new Point(10, -10));
			_unitUnderTest.AddVertex(new Point(10, 10));
			_unitUnderTest.AddVertex(new Point(0, 0));
		}

		[DataTestMethod]
		[DataRow(-10, -10, true)]
		[DataRow(0, 0, true)]
		[DataRow(100, 100, false)]
		[DataRow(0, 10, false)]
		[DataRow(0, -10, true)]
		public void IsPointInFigure_Point_IsInFigure(int x, int y, bool isInFigure)
		{
			var result = _unitUnderTest.ContainsPoint(new Point(x, y));

			Assert.AreEqual(isInFigure, result);
		}
	}
}
