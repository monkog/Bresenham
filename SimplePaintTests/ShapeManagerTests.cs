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
			Assert.IsTrue(_figure.IsSelected);
		}

		[TestMethod]
		public void DeselectFigures_NoParams_SelectedFigureNull()
		{
			_unitUnderTest.Figures.Add(_figure);
			_unitUnderTest.SelectFigure(new Point(0, 0));

			_unitUnderTest.DeselectFigures();

			Assert.IsNull(_unitUnderTest.SelectedFigure);
			Assert.IsFalse(_figure.IsSelected);
		}

		[TestMethod]
		public void MultisamplingFigure_FigureWithLineSelected_Figure()
		{
			_unitUnderTest.Figures.Add(_figure);
			_figure.MultisamplingLine = new CustomLine(new Point(0, 0), new Point(100, 100));

			Assert.AreEqual(_figure, _unitUnderTest.MultisamplingFigure);
		}

		[TestMethod]
		public void MultisamplingFigure_NoFigureWithLineSelected_Null()
		{
			_unitUnderTest.Figures.Add(_figure);

			Assert.IsNull(_unitUnderTest.MultisamplingFigure);
		}
	}
}
