using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimplePaint.Shapes;
using System.Drawing;

namespace SimplePaintTests.Shapes
{
	[TestClass]
	public class CustomEllipseTests
	{
		private CustomEllipse _unitUnderTest;

		[TestInitialize]
		public void Initialize()
		{
			_unitUnderTest = new CustomEllipse(new Point(12, 15));
		}

		[TestMethod]
		public void Ctor_ValidParameters_PropertiesAssigned()
		{
			var position = new Point(1, 3);

			var unitUnderTest = new CustomEllipse(position);

			Assert.AreEqual(position, unitUnderTest.Position);
		}

		[TestMethod]
		public void Select_NoParams_FigureSelected()
		{
			_unitUnderTest.Select();

			Assert.IsTrue(_unitUnderTest.IsSelected);
		}

		[TestMethod]
		public void Deselect_NoParams_SelectedFigureNull()
		{
			_unitUnderTest.Select();

			_unitUnderTest.Deselect();

			Assert.IsFalse(_unitUnderTest.IsSelected);
		}

		[DataTestMethod]
		[DataRow(1, 1, 100, 100, true)]
		[DataRow(-1, -1, 100, 100, true)]
		[DataRow(-10, -1, 100, 100, false)]
		[DataRow(100, -1, 100, 100, false)]
		public void CanDrag_Deltas_CanDrag(int deltaX, int deltaY, int xBound, int yBound, bool expected)
		{
			var canDrag = _unitUnderTest.CanDrag(deltaX, deltaY, xBound, yBound);

			Assert.AreEqual(expected, canDrag);
		}
	}
}
