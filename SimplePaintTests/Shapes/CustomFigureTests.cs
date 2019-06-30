﻿using System.Drawing;
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
			_unitUnderTest.AddVertex(new Point(-20, -20));
			_unitUnderTest.AddVertex(new Point(20, -20));
			_unitUnderTest.AddVertex(new Point(20, 20));
			_unitUnderTest.AddVertex(new Point(0, 0));
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
		[DataRow(-20, 20)]
		[DataRow(-21, 21)]
		public void IsVertex_VertexPoint_True(int x, int y)
		{
			var result = _unitUnderTest.IsVertex(new Point(x, y), out var vertex);

			Assert.IsTrue(result);
			Assert.AreEqual(_unitUnderTest.FirstNode.Value, vertex);
		}

		[DataTestMethod]
		[DataRow(-20, 20, DisplayName = "Already existing vertex - not added")]
		[DataRow(-21, 21, DisplayName = "Close to already existing vertex - not added")]
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

			_unitUnderTest.AddVertex(new Point(40, 40));

			Assert.AreEqual(vertexCount + 1, _unitUnderTest.Vertices.Count);
		}

		[TestMethod]
		public void AddVertex_VertexPoint_BoundingBoxUpdated()
		{
			var minX = _unitUnderTest.MinX;
			var minY = _unitUnderTest.MinY;

			_unitUnderTest.AddVertex(new Point(-50, -40));

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
			var point = new Point(20,20);
			var vertexCount = _unitUnderTest.Vertices.Count;

			_unitUnderTest.AddVertexOnLine(point, new CustomLine(point, point));

			Assert.AreEqual(vertexCount, _unitUnderTest.Vertices.Count);
		}
	}
}