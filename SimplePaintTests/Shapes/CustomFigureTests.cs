using System.Drawing;
using System.Linq;
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
			_unitUnderTest = new CustomFigure(new Point(-20, 20), Color.Black, 2);
			_unitUnderTest.TryAddVertex(new Point(-20, -20));
			_unitUnderTest.TryAddVertex(new Point(20, -20));
			_unitUnderTest.TryAddVertex(new Point(20, 20));
			_unitUnderTest.TryAddVertex(new Point(0, 0));
		}

		[DataTestMethod]
		[DataRow(-20, -20, true)]
		[DataRow(0, 0, true)]
		[DataRow(100, 100, false)]
		[DataRow(0, 20, false)]
		[DataRow(0, -20, true)]
		public void IsPointInFigure_Point_IsInFigure(int x, int y, bool isInFigure)
		{
			var result = _unitUnderTest.ContainsPoint(new Point(x, y));

			Assert.AreEqual(isInFigure, result);
		}

		[DataTestMethod]
		[DataRow(100, 100, DisplayName = "Outside bounding box")]
		[DataRow(0, 20, DisplayName = "Outside figure")]
		[DataRow(0, -20, DisplayName = "On the edge of the shape")]
		public void IsVertex_NotVertexPoint_False(int x, int y)
		{
			var result = _unitUnderTest.IsVertex(new Point(x, y), out var vertex);

			Assert.IsFalse(result);
			Assert.IsNull(vertex);
		}

		[DataTestMethod]
		[DataRow(-20, 20, DisplayName = "First vertex")]
		[DataRow(-21, 21, DisplayName = "Close to first vertex")]
		public void IsVertex_VertexPoint_True(int x, int y)
		{
			var result = _unitUnderTest.IsVertex(new Point(x, y), out var vertex);

			Assert.IsTrue(result);
			Assert.AreEqual(_unitUnderTest.FirstNode.Value, vertex);
		}

		[DataTestMethod]
		[DataRow(-20, 20, DisplayName = "Already existing vertex - not added")]
		[DataRow(-21, 21, DisplayName = "Close to already existing vertex - not added")]
		public void TryAddVertex_VertexPoint_HasNotBeenAdded(int x, int y)
		{
			var vertexCount = _unitUnderTest.Vertices.Count;

			_unitUnderTest.TryAddVertex(new Point(x, y));

			Assert.AreEqual(vertexCount, _unitUnderTest.Vertices.Count);
		}

		[TestMethod]
		public void TryAddVertex_VertexPoint_HasBeenAdded()
		{
			var vertexCount = _unitUnderTest.Vertices.Count;

			_unitUnderTest.TryAddVertex(new Point(40, 40));

			Assert.AreEqual(vertexCount + 1, _unitUnderTest.Vertices.Count);
		}

		[TestMethod]
		public void TryAddVertex_VertexPoint_BoundingBoxUpdated()
		{
			var minX = _unitUnderTest.MinX;
			var minY = _unitUnderTest.MinY;

			_unitUnderTest.TryAddVertex(new Point(-50, -40));

			Assert.AreNotEqual(minX, _unitUnderTest.MinX);
			Assert.AreEqual(-50, _unitUnderTest.MinX);
			Assert.AreNotEqual(minY, _unitUnderTest.MinY);
			Assert.AreEqual(-40, _unitUnderTest.MinY);
		}

		[TestMethod]
		public void AddVertexOnLine_PointAndLine_VertexAdded()
		{
			Assert.Fail("Adjust class responsibilities and add respective tests");
			var line = _unitUnderTest.FigureShapes.OfType<CustomLine>().First();
			var vertexCount = _unitUnderTest.Vertices.Count;

			_unitUnderTest.AddVertexOnLine(new Point(-20, 0), line);

			Assert.AreEqual(vertexCount + 1, _unitUnderTest.Vertices.Count);
		}

		[TestMethod]
		public void AddVertexOnLine_ExistingVertex_VertexNotAdded()
		{
			var point = new Point(20, 20);
			var vertexCount = _unitUnderTest.Vertices.Count;

			_unitUnderTest.AddVertexOnLine(point, new CustomLine(point, point));

			Assert.AreEqual(vertexCount, _unitUnderTest.Vertices.Count);
		}

		[TestMethod]
		public void Move_Delta_BoundingBoxUpdated()
		{
			var minX = _unitUnderTest.MinX;
			var maxX = _unitUnderTest.MaxX;
			var minY = _unitUnderTest.MinY;
			var maxY = _unitUnderTest.MaxY;

			_unitUnderTest.Move(-10, -19);

			Assert.AreNotEqual(minX, _unitUnderTest.MinX);
			Assert.AreEqual(-30, _unitUnderTest.MinX);
			Assert.AreNotEqual(maxX, _unitUnderTest.MaxX);
			Assert.AreEqual(10, _unitUnderTest.MaxX);
			Assert.AreNotEqual(minY, _unitUnderTest.MinY);
			Assert.AreEqual(-39, _unitUnderTest.MinY);
			Assert.AreNotEqual(maxY, _unitUnderTest.MaxY);
			Assert.AreEqual(1, _unitUnderTest.MaxY);
		}

		[TestMethod]
		public void Move_Delta_VerticesUpdated()
		{
			var firstVertex = new Point(0, 0);
			var secondVertex = new Point(10, 10);

			var unitUnderTest = new CustomFigure(firstVertex, Color.AliceBlue, 1);
			unitUnderTest.TryAddVertex(secondVertex);

			unitUnderTest.Move(-10, -19);

			Assert.AreEqual(2, unitUnderTest.Vertices.Count);
			Assert.AreEqual(1, unitUnderTest.Vertices.Count(v => v.Position.X == -10 && v.Position.Y == -19));
			Assert.AreEqual(1, unitUnderTest.Vertices.Count(v => v.Position.X == 0 && v.Position.Y == -9));
		}

		[TestMethod]
		public void Move_Delta_LinesUpdated()
		{
			Assert.Fail();
		}

		[TestMethod]
		public void MoveVertex_Delta_LinesUpdated()
		{
			Assert.Fail();
		}

		[TestMethod]
		public void Select_NoParams_Selected()
		{
			_unitUnderTest.Select();

			Assert.IsTrue(_unitUnderTest.IsSelected);
		}

		[TestMethod]
		public void Deselect_NoParams_NotSelected()
		{
			_unitUnderTest.Select();

			_unitUnderTest.Deselect();

			Assert.IsFalse(_unitUnderTest.IsSelected);
		}

		[TestMethod]
		public void Deselect_NoParams_AllVerticesDeselected()
		{
			_unitUnderTest.Select();
			var vertex = _unitUnderTest.Vertices.First.Value;
			vertex.Select();

			_unitUnderTest.Deselect();

			Assert.IsFalse(vertex.IsSelected);
		}

		[TestMethod]
		public void Deselect_NoParams_SelectedVertexNull()
		{
			_unitUnderTest.Select();
			var vertex = _unitUnderTest.Vertices.First.Value;
			vertex.Select();

			_unitUnderTest.Deselect();

			Assert.IsNull(_unitUnderTest.SelectedVertex);
		}

		[TestMethod]
		public void SelectedVertex_NoParams_SelectedVertex()
		{
			var vertex = _unitUnderTest.Vertices.First.Value;
			vertex.Select();

			var result = _unitUnderTest.SelectedVertex;

			Assert.AreEqual(vertex, result);
			Assert.IsTrue(result.IsSelected);
		}

		[DataTestMethod]
		[DataRow(-10, -10, false)]
		[DataRow(10, 10, false)]
		[DataRow(40, 40, true)]
		[DataRow(400, 400, false)]
		public void CanDrag_Deltas_CanDrag(int deltaX, int deltaY, bool expected)
		{
			var result = _unitUnderTest.CanDrag(deltaX, deltaY, 100, 100);

			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void GetLineContainingPoint_NoLineContainsPoint_Null()
		{
			var line = new CustomLine(new Point(1, 2), new Point(1, 4));
			_unitUnderTest.FigureShapes.AddLast(line);

			var result = _unitUnderTest.GetLineContainingPoint(new Point(7, 9));

			Assert.IsNull(result);
		}

		[TestMethod]
		public void GetLineContainingPoint_LineContainsPoint_Line()
		{
			var line = new CustomLine(new Point(1, 2), new Point(1, 4));
			_unitUnderTest.FigureShapes.AddLast(line);

			var result = _unitUnderTest.GetLineContainingPoint(new Point(1, 3));

			Assert.IsNotNull(result);
			Assert.AreEqual(line, result);
		}

		[TestMethod]
		public void GetLineContainingPoint_PointCloseToLine_Line()
		{
			var line = new CustomLine(new Point(1, 2), new Point(1, 4));
			_unitUnderTest.FigureShapes.AddLast(line);

			var result = _unitUnderTest.GetLineContainingPoint(new Point(2, 3));

			Assert.IsNotNull(result);
			Assert.AreEqual(line, result);
		}

		[TestMethod]
		public void TryAddFigure_1Vertex_True()
		{
			var point = new Point(2, 20);
			var unitUnderTest = new CustomFigure(new Point(1, 3), Color.AntiqueWhite, 2);

			var result = unitUnderTest.TryAddVertex(point);

			Assert.IsTrue(result);
			Assert.IsTrue(unitUnderTest.FigureShapes.OfType<CustomEllipse>().Any(e => e.Position == point));
		}

		[TestMethod]
		public void TryAddFigure_2Vertices_True()
		{
			var point = new Point(2, 20);
			var unitUnderTest = new CustomFigure(new Point(1, 3), Color.AntiqueWhite, 2);
			unitUnderTest.TryAddVertex(new Point(10, 100));

			var result = unitUnderTest.TryAddVertex(point);

			Assert.IsTrue(result);
			Assert.IsTrue(unitUnderTest.FigureShapes.OfType<CustomEllipse>().Any(e => e.Position == point));
		}

		[TestMethod]
		public void TryAddFigure_3VerticesNotClosingVertex_True()
		{
			var point = new Point(2, 200);
			var unitUnderTest = new CustomFigure(new Point(1, 3), Color.AntiqueWhite, 2);
			unitUnderTest.TryAddVertex(new Point(10, 100));
			unitUnderTest.TryAddVertex(new Point(20, 100));

			var result = unitUnderTest.TryAddVertex(point);

			Assert.IsTrue(result);
			Assert.IsTrue(unitUnderTest.FigureShapes.OfType<CustomEllipse>().Any(e => e.Position == point));
		}

		[TestMethod]
		public void TryAddFigure_ClosingVertex_False()
		{
			var point = new Point(2, 3);
			var unitUnderTest = new CustomFigure(new Point(1, 3), Color.AntiqueWhite, 2);
			unitUnderTest.TryAddVertex(new Point(10, 100));
			unitUnderTest.TryAddVertex(new Point(20, 100));

			var result = unitUnderTest.TryAddVertex(point);

			Assert.IsFalse(result);
			Assert.IsFalse(unitUnderTest.FigureShapes.OfType<CustomEllipse>().Any(e => e.Position == point));
		}
	}
}
