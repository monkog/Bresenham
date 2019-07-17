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
			_figure.TryAddVertex(new Point(-10, 10));
			_figure.TryAddVertex(new Point(-10, -10));

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
			var from = new CustomEllipse(new Point(0, 0));
			var to = new CustomEllipse(new Point(100, 100));
			_figure.MultisamplingLine = new CustomLine(from, to);

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
			var firstVertex = currentFigure.Vertices.First.Value;

			Assert.IsNotNull(currentFigure);
			Assert.AreEqual(point, firstVertex.Position);
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
			_unitUnderTest.Figures.Add(_figure);

			_unitUnderTest.SelectLineForMultisampling(new Point(5, -10));

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
			var verticesCount = _figure.Vertices.Count;

			_unitUnderTest.TryAddVertexToFigure(new Point(0, -10));

			Assert.AreEqual(verticesCount + 1, _figure.Vertices.Count);
		}

		[TestMethod]
		public void TryAddVertexToFigure_PointNotOnFigureEdge_VertexNotAdded()
		{
			_unitUnderTest.Figures.Add(_figure);
			var verticesCount = _figure.Vertices.Count;

			_unitUnderTest.TryAddVertexToFigure(new Point(100, -10));

			Assert.AreEqual(verticesCount, _figure.Vertices.Count);
		}

		[TestMethod]
		public void ChangeFigureColor_PointOnFigureEdge_ColorChanged()
		{
			_unitUnderTest.Figures.Add(_figure);

			var newColor = Color.White;
			_unitUnderTest.ChangeFigureColor(newColor, new Point(0, -10));

			Assert.AreEqual(newColor, _figure.FigureColor);
		}

		[TestMethod]
		public void ChangeFigureColor_PointNotOnFigureEdge_ColorNotChanged()
		{
			_unitUnderTest.Figures.Add(_figure);
			var color = _figure.FigureColor;

			_unitUnderTest.ChangeFigureColor(Color.White, new Point(100, -10));

			Assert.AreEqual(color, _figure.FigureColor);
		}

		[TestMethod]
		public void ChangeFigureThickness_PointOnFigureEdge_ThicknessChanged()
		{
			_unitUnderTest.Figures.Add(_figure);

			const int newThickness = 22;
			_unitUnderTest.ChangeFigureThickness(newThickness, new Point(0, -10));

			Assert.AreEqual(newThickness, _figure.StrokeThickness);
		}

		[TestMethod]
		public void ChangeFigureThickness_PointNotOnFigureEdge_ThicknessNotChanged()
		{
			_unitUnderTest.Figures.Add(_figure);
			var thickness = _figure.StrokeThickness;

			_unitUnderTest.ChangeFigureThickness(22, new Point(100, -10));

			Assert.AreEqual(thickness, _figure.StrokeThickness);
		}
	}
}
