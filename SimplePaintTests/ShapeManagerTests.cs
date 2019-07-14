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
			_figure.TryAddVertex(new Point(-10, -10));
			_figure.TryAddVertex(new Point(10, -10));
			_figure.TryAddVertex(new Point(10, 10));

			_unitUnderTest = new ShapeManager();
		}

		[TestMethod]
		public void Ctor_NoParams_PropertiesInitialized()
		{
			var unitUnderTest = new ShapeManager();

			Assert.IsNotNull(unitUnderTest.Figures);
			Assert.IsFalse(unitUnderTest.Figures.Any());
			Assert.IsNull(_unitUnderTest.SelectedFigure);
			Assert.IsNull(_unitUnderTest.CurrentFigure);
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

		[TestMethod]
		public void StartDrawingFigure_Always_CurrentFigureAssigned()
		{
			var point = new Point(2, 10);
			var color = Color.AliceBlue;
			const int strokeThickness = 5;

			_unitUnderTest.StartDrawingFigure(point, color, strokeThickness);
			var currentFigure = _unitUnderTest.CurrentFigure;

			Assert.IsNotNull(currentFigure);
			Assert.AreEqual(point, currentFigure.FirstVertex.Position);
			Assert.AreEqual(color, currentFigure.FigureColor);
			Assert.AreEqual(strokeThickness, currentFigure.StrokeThickness);
		}

		[TestMethod]
		public void CancelDrawing_Always_CurrentFigureNull()
		{
			_unitUnderTest.StartDrawingFigure(new Point(2, 10), Color.AliceBlue, 5);

			_unitUnderTest.CancelDrawing();

			Assert.IsNull(_unitUnderTest.CurrentFigure);
		}

		[TestMethod]
		public void TryAddVertexToCurrentFigure_CanAdd_CurrentFigureNotNull()
		{
			_unitUnderTest.StartDrawingFigure(new Point(2, 10), Color.AliceBlue, 5);

			_unitUnderTest.TryAddVertexToCurrentFigure(new Point(100, 100));

			Assert.IsNotNull(_unitUnderTest.CurrentFigure);
		}

		[TestMethod]
		public void TryAddVertexToCurrentFigure_CanAdd_CurrentFigureNotInFiguresCollection()
		{
			_unitUnderTest.StartDrawingFigure(new Point(2, 10), Color.AliceBlue, 5);

			_unitUnderTest.TryAddVertexToCurrentFigure(new Point(100, 100));

			CollectionAssert.DoesNotContain(_unitUnderTest.Figures, _unitUnderTest.CurrentFigure);
		}

		[TestMethod]
		public void TryAddVertexToCurrentFigure_CannotAdd_CurrentFigureNull()
		{
			_unitUnderTest.StartDrawingFigure(new Point(2, 10), Color.AliceBlue, 5);
			var figure = _unitUnderTest.CurrentFigure;
			figure.TryAddVertex(new Point(20, 20));
			figure.TryAddVertex(new Point(20, 40));

			_unitUnderTest.TryAddVertexToCurrentFigure(new Point(2, 10));

			Assert.IsNull(_unitUnderTest.CurrentFigure);
		}

		[TestMethod]
		public void TryAddVertexToCurrentFigure_CannotAdd_CurrentFigureInFiguresCollection()
		{
			_unitUnderTest.StartDrawingFigure(new Point(2, 10), Color.AliceBlue, 5);
			var figure = _unitUnderTest.CurrentFigure;
			figure.TryAddVertex(new Point(20, 20));
			figure.TryAddVertex(new Point(20, 40));

			_unitUnderTest.TryAddVertexToCurrentFigure(new Point(2, 10));

			CollectionAssert.Contains(_unitUnderTest.Figures, figure);
		}

		[TestMethod]
		public void SelectLineForMultisampling_PointOnFigureEdge_LineSelected()
		{
			_unitUnderTest.SelectLineForMultisampling(new Point(5, -10));

			Assert.Fail("Fix after adding lines will work properly.");

			Assert.IsNotNull(_unitUnderTest.MultisamplingFigure);
			Assert.IsNotNull(_unitUnderTest.MultisamplingFigure.MultisamplingLine);
		}

		[TestMethod]
		public void SelectLineForMultisampling_PointNotOnFigureEdge_MultisamplingFigureNull()
		{
			_unitUnderTest.SelectLineForMultisampling(new Point(100, -10));

			Assert.IsNull(_unitUnderTest.MultisamplingFigure);
		}

		[TestMethod]
		public void TryAddVertexToFigure_PointOnFigureEdge_VertexAdded()
		{
			_unitUnderTest.Figures.Add(_figure);
			var verticesCount = _figure.FigureShapes.Count;

			_unitUnderTest.TryAddVertexToFigure(new Point(0, -10));

			Assert.Fail("Fix after adding lines will work properly.");
			Assert.AreEqual(verticesCount + 1, _figure.FigureShapes.Count);
		}

		[TestMethod]
		public void TryAddVertexToFigure_PointNotOnFigureEdge_VertexNotAdded()
		{
			_unitUnderTest.Figures.Add(_figure);
			var verticesCount = _figure.FigureShapes.Count;

			_unitUnderTest.TryAddVertexToFigure(new Point(100, -10));

			Assert.AreEqual(verticesCount, _figure.FigureShapes.Count);
		}
	}
}
