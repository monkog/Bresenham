using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SimplePaint.Shapes
{
	/// <summary>
	/// Represents a figure composed of lines and ellipses
	/// </summary>
	public class CustomFigure
	{
		private const int Delta = 10;

		/// <summary>
		/// Gets the first node in the LinkedList.
		/// </summary>
		public LinkedListNode<CustomEllipse> FirstNode => Vertices.First;

		/// <summary>
		/// Gets the last node in the LinkedList.
		/// </summary>
		public LinkedListNode<CustomEllipse> LastNode => Vertices.Last;

		/// <summary>
		/// Gets the first vertex.
		/// </summary>
		public CustomEllipse FirstVertex => FirstNode.Value;

		/// <summary>
		/// Gets the last vertex.
		/// </summary>
		public CustomEllipse LastVertex => LastNode.Value;

		/// <summary>
		/// Gets or sets the color of the figure.
		/// </summary>
		public Color FigureColor { get; set; }

		/// <summary>
		/// Gets or sets the stroke thickness.
		/// </summary>
		public int StrokeThickness { get; set; }

		/// <summary>
		/// Gets or sets the maximum x coordinate of the figure.
		/// </summary>
		public int MaxX { get; set; }

		/// <summary>
		/// Gets or sets the maximum y coordinate of the figure.
		/// </summary>
		public int MaxY { get; set; }

		/// <summary>
		/// Gets or sets the minimum x coordinate of the figure.
		/// </summary>
		public int MinX { get; set; }

		/// <summary>
		/// Gets or sets the minimum y coordinate of the figure.
		/// </summary>
		public int MinY { get; set; }

		/// <summary>
		/// The figure shapes
		/// </summary>
		public LinkedList<IShape> FigureShapes { get; set; }
		/// <summary>
		/// Gets or sets the figure vertices.
		/// </summary>
		public LinkedList<CustomEllipse> Vertices { get; set; }

		/// <summary>
		/// Gets or sets the multi-sampling line.
		/// </summary>
		public CustomLine MultisamplingLine { get; set; }

		/// <summary>
		/// Gets or sets the color of the multi-sampling.
		/// </summary>
		public Color MultisamplingColor { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CustomFigure"/> class.
		/// </summary>
		/// <param name="point">The position point.</param>
		/// <param name="color">The color.</param>
		/// <param name="strokeThickness">The stroke thickness.</param>
		public CustomFigure(Point point, Color color, int strokeThickness)
		{
			FigureColor = color;
			StrokeThickness = strokeThickness;
			Vertices = new LinkedList<CustomEllipse>();
			Vertices.AddFirst(new CustomEllipse(point));
			FigureShapes = new LinkedList<IShape>();
			FigureShapes.AddFirst(new CustomEllipse(point));
			MultisamplingLine = null;
			MultisamplingColor = Color.Azure;
			UpdateBoundingBox();
		}

		/// <summary>
		/// Adds the vertex on the specified line.
		/// </summary>
		/// <param name="point">Position of the new vertex.</param>
		/// <param name="line">The line to add the vertex on.</param>
		public void AddVertexOnLine(Point point, CustomLine line)
		{
			if (IsVertex(point, out _)) return;

			var lineToSplit = FigureShapes.Find(line);
			var lineStart = lineToSplit.Previous;
			var lineEnd = lineToSplit.Next ?? FigureShapes.First;

			var previousEllipse = lineStart.Value as CustomEllipse;
			var nextEllipse = lineEnd.Value as CustomEllipse;

			FigureShapes.Remove(line);

			FigureShapes.AddAfter(lineStart, new CustomLine(previousEllipse.Position, point));
			FigureShapes.AddAfter(lineStart.Next, new CustomEllipse(point));
			FigureShapes.AddAfter(lineStart.Next.Next, new CustomLine(point, nextEllipse.Position));

			Vertices.AddAfter(Vertices.Find(FindVertexAtPoint(previousEllipse.Position)), new CustomEllipse(point));
			UpdateBoundingBox();
		}

		/// <summary>
		/// Adds the vertex to the newly constructed figure.
		/// </summary>
		/// <param name="point">The vertex coordinates.</param>
		public void AddVertex(Point point)
		{
			if (IsVertex(point, out _)) return;

			Vertices.AddLast(new CustomEllipse(point));
			FigureShapes.AddLast(new CustomEllipse(point));

			UpdateBoundingBox();
		}

		/// <summary>
		/// Determines whether this figure contains the given point.
		/// </summary>
		/// <param name="point">The point to look for.</param>
		/// <returns>True if the hit point is in a figure</returns>
		public bool ContainsPoint(Point point)
		{
			if (IsOutsideBoundingBox(point)) return false;

			var currentVertex = FirstNode;
			var previousVertex = LastNode;

			var isInFigure = false;

			for (var i = 0; i < Vertices.Count; i++)
			{
				var x1 = previousVertex.Value.Position.X;
				var y1 = previousVertex.Value.Position.Y;
				var x2 = currentVertex.Value.Position.X;
				var y2 = currentVertex.Value.Position.Y;

				if ((y2 > point.Y) != (y1 > point.Y) && (point.X < (x1 - x2) * (point.Y - y2) / (y1 - y2) + x2)) isInFigure = !isInFigure;
				previousVertex = currentVertex;
				currentVertex = currentVertex.Next;
			}

			return isInFigure;
		}

		/// <summary>
		/// Determines whether the specified point is vertex.
		/// </summary>
		/// <param name="point">The point.</param>
		/// <param name="vertex">The found vertex.</param>
		/// <returns>True if the hit point is vertex, false otherwise.</returns>
		public bool IsVertex(Point point, out CustomEllipse vertex)
		{
			vertex = null;
			if (IsOutsideBoundingBox(point)) return false;

			vertex = FindVertexAtPoint(point);
			return vertex != null;
		}

		/// <summary>
		/// Moves the figure by the given delta.
		/// </summary>
		/// <param name="deltaX">Change in X coordinate.</param>
		/// <param name="deltaY">Change in Y coordinate.</param>
		public void Move(int deltaX, int deltaY)
		{
			foreach (var vertex in FigureShapes.OfType<CustomEllipse>())
				vertex.Position = new Point(vertex.Position.X + deltaX, vertex.Position.Y + deltaY);

			foreach (var line in FigureShapes.OfType<CustomLine>())
			{
				line.StartPoint = new Point(line.StartPoint.X + deltaX, line.StartPoint.Y + deltaY);
				line.EndPoint = new Point(line.EndPoint.X + deltaX, line.EndPoint.Y + deltaY);
			}

			foreach (var vertex in Vertices)
				vertex.Position = new Point(vertex.Position.X + deltaX, vertex.Position.Y + deltaY);

			UpdateBoundingBox();
		}

		private void UpdateBoundingBox()
		{
			MinX = Vertices.Min(v => v.Position.X);
			MaxX = Vertices.Max(v => v.Position.X);
			MinY = Vertices.Min(v => v.Position.Y);
			MaxY = Vertices.Max(v => v.Position.Y);
		}

		private bool IsOutsideBoundingBox(Point point)
		{
			return point.X < MinX - Delta || point.X > MaxX + Delta || point.Y < MinY - Delta || point.Y > MaxY + Delta;
		}

		private CustomEllipse FindVertexAtPoint(Point point)
		{
			return Vertices.FirstOrDefault(v =>
				point.X < v.Position.X + Delta && point.X > v.Position.X - Delta && point.Y < v.Position.Y + Delta && point.Y > v.Position.Y - Delta);
		}
	}
}
