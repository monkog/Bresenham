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
		public LinkedListNode<CustomEllipse> FirstNode => FigureVertices.First;

		/// <summary>
		/// Gets the last node in the LinkedList.
		/// </summary>
		public LinkedListNode<CustomEllipse> LastNode => FigureVertices.Last;

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
		/// Gets or sets the vertex number.
		/// </summary>
		public int VertexNumber { get; set; }

		/// <summary>
		/// The figure shapes
		/// </summary>
		public LinkedList<IShape> FigureShapes { get; set; }
		/// <summary>
		/// Gets or sets the figure vertices.
		/// </summary>
		public LinkedList<CustomEllipse> FigureVertices { get; set; }

		/// <summary>
		/// Gets or sets the multi-sampling line.
		/// </summary>
		public CustomLine MultisamplingLine { get; set; }

		/// <summary>
		/// Gets or sets the color of the multi-sampling.
		/// </summary>
		public Color MultisamplingColor { get; set; }
		/// <summary>
		/// Gets the size of the vertex.
		/// </summary>
		public int VertexSize { get; private set; }

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
			VertexSize = StrokeThickness + 6;
			FigureVertices = new LinkedList<CustomEllipse>();
			FigureVertices.AddFirst(new CustomEllipse(point));
			FigureShapes = new LinkedList<IShape>();
			FigureShapes.AddFirst(new CustomEllipse(point));
			MultisamplingLine = null;
			MultisamplingColor = Color.Azure;
			MaxX = point.X + 5;
			MinX = point.X - 5;
			MaxY = point.Y + 5;
			MinY = point.Y - 5;
			VertexNumber++;
		}

		/// <summary>
		/// Adds the vertex on the specified line.
		/// </summary>
		/// <param name="vertex">The vertex.</param>
		/// <param name="line">The line.</param>
		public void AddInbetweenVertex(CustomEllipse vertex, CustomLine line)
		{
			var previousNode = FigureShapes.Find(line).Previous;
			var nextNode = FigureShapes.Find(line).Next;

			var previousEllipse = previousNode.Value as CustomEllipse;
			CustomEllipse nextEllipse;

			if (nextNode == null)
				nextEllipse = FigureShapes.First.Value as CustomEllipse;
			else
				nextEllipse = nextNode.Value as CustomEllipse;

			if (((Math.Abs(vertex.Position.X - previousEllipse.Position.X) < 10 && Math.Abs(vertex.Position.Y - previousEllipse.Position.Y) < 10)
				|| (Math.Abs(vertex.Position.X - nextEllipse.Position.X) < 10) && Math.Abs(vertex.Position.Y - nextEllipse.Position.Y) < 10))
				return;

			FigureShapes.AddAfter(previousNode, new CustomLine(previousEllipse.Position, vertex.Position));
			FigureShapes.AddAfter(previousNode.Next, new CustomEllipse(vertex.Position));
			FigureShapes.AddAfter(previousNode.Next.Next, new CustomLine(vertex.Position, nextEllipse.Position));
			FigureShapes.Remove(line);

			FigureVertices.AddAfter(FigureVertices.Find(FindVertexAtPoint(previousEllipse.Position)), vertex);

			VertexNumber++;

			if (vertex.Position.X + 5 > MaxX) MaxX = vertex.Position.X + 5;
			if (vertex.Position.X - 5 < MinX) MinX = vertex.Position.X - 5;
			if (vertex.Position.Y + 5 > MaxY) MaxY = vertex.Position.Y + 5;
			if (vertex.Position.Y - 5 < MinY) MinY = vertex.Position.Y - 5;
		}

		/// <summary>
		/// Adds the vertex to the newly constructed figure.
		/// </summary>
		/// <param name="point">The point.</param>
		public void AddVertex(Point point)
		{
			// We don't want to add next vertex too close to an existing one
			if (FigureVertices.Any(
				vertex => Math.Abs(vertex.Position.X - point.X) < 10 && Math.Abs(vertex.Position.Y - point.Y) < 10))
				return;

			FigureVertices.AddLast(new CustomEllipse(point));
			FigureShapes.AddLast(new CustomEllipse(point));
			var delta = VertexSize / 2;
			if (point.X + delta > MaxX) MaxX = point.X + delta;
			if (point.X - delta < MinX) MinX = point.X - delta;
			if (point.Y + delta > MaxY) MaxY = point.Y + delta;
			if (point.Y - delta < MinY) MinY = point.Y - delta;
			VertexNumber++;
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

			for (var i = 0; i < VertexNumber; i++)
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
			return vertex !=null;
		}

		private bool IsOutsideBoundingBox(Point point)
		{
			return point.X < MinX - Delta || point.X > MaxX + Delta || point.Y < MinY - Delta || point.Y > MaxY + Delta;
		}

		private CustomEllipse FindVertexAtPoint(Point point)
		{
			return FigureVertices.FirstOrDefault(v => 
				point.X < v.Position.X + Delta && point.X > v.Position.X - Delta && point.Y < v.Position.Y + Delta && point.Y > v.Position.Y - Delta);
		}
	}
}
