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

		[DataTestMethod]
		[DataRow(100, 100, DisplayName = "Outside bounding box")]
		[DataRow(0, 10, DisplayName = "Outside figure")]
		[DataRow(0, -10, DisplayName = "On the edge of the shape")]
		public void IsVertex_NotVertexPoint_False(int x, int y)
		{
			var result = _unitUnderTest.IsVertex(new Point(x, y), out var vertex);

			Assert.IsFalse(result);
			Assert.IsNull(vertex);
		}

		[DataTestMethod]
		[DataRow(-10, 10)]
		[DataRow(-11, 11)]
		public void IsVertex_VertexPoint_True(int x, int y)
		{
			var result = _unitUnderTest.IsVertex(new Point(x, y), out var vertex);

			Assert.IsTrue(result);
			Assert.AreEqual(_unitUnderTest.FirstNode.Value, vertex);
		}

		[DataTestMethod]
		[DataRow(-10, 10, DisplayName = "Already existing vertex - not added")]
		[DataRow(-11, 11, DisplayName = "Close to already existing vertex - not added")]
		public void AddVertex_VertexPoint_HasNotBeenAdded(int x, int y)
		{
			var vertexCount = _unitUnderTest.Vertices.Count;

			_unitUnderTest.AddVertex(new Point(x, y));

			Assert.AreEqual(vertexCount, _unitUnderTest.Vertices.Count);
		}

		[TestMethod]
		public void AddVertex_VertexPoint_HasBeenAdded()
		{
			var vertexCount = _unitUnderTest.Vertices.Count;

			_unitUnderTest.AddVertex(new Point(20, 20));

			Assert.AreEqual(vertexCount + 1, _unitUnderTest.Vertices.Count);
		}

		[TestMethod]
		public void AddVertex_VertexPoint_BoundingBoxUpdated()
		{
			var minX = _unitUnderTest.MinX;
			var minY = _unitUnderTest.MinY;

			_unitUnderTest.AddVertex(new Point(-20, -40));

			Assert.AreNotEqual(minX, _unitUnderTest.MinX);
			Assert.AreEqual(-20, _unitUnderTest.MinX);
			Assert.AreNotEqual(minY, _unitUnderTest.MinY);
			Assert.AreEqual(-40, _unitUnderTest.MinY);
		}
	}
}
