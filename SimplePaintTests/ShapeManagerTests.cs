using System.Drawing;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimplePaint;
using SimplePaint.Shapes;

namespace SimplePaintTests
{
	[TestClass]
	public class ShapeManagerTests
	{
		private CustomFigure _figure;

		private ShapeManager _unitUnderTest;

		[TestInitialize]
		public void Initialize()
		{
			_figure = new CustomFigure(new Point(-10, 10), Color.Black, 2);
			_figure.AddVertex(new Point(-10, -10));
			_figure.AddVertex(new Point(10, -10));
			_figure.AddVertex(new Point(10, 10));

			_unitUnderTest = new ShapeManager();
		}

		[TestMethod]
		public void Ctor_NoParams_PropertiesInitialized()
		{
			var unitUnderTest = new ShapeManager();

			Assert.IsNotNull(unitUnderTest.Figures);
			Assert.IsFalse(unitUnderTest.Figures.Any());
			Assert.IsNull(_unitUnderTest.SelectedFigure);
		}

		[TestMethod]
		public void SelectFigure_PointInFigure_FigureSelected()
		{
			_unitUnderTest.Figures.Add(_figure);

			_unitUnderTest.SelectFigure(new Point(0, 0));
			
			Assert.AreEqual(_figure, _unitUnderTest.SelectedFigure);
		}

		[TestMethod]
		public void DeselectFigure_NoParams_SelectedFigureNull()
		{
			_unitUnderTest.Figures.Add(_figure);
			_unitUnderTest.SelectFigure(new Point(0, 0));

			_unitUnderTest.DeselectFigure();

			Assert.IsNull(_unitUnderTest.SelectedFigure);
		}
	}
}
