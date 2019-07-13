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
		/// Gets the value determining whether this figure is selected.
		/// </summary>
		public bool IsSelected { get; private set; }

		/// <summary>
		/// Gets the selected vertex.
		/// </summary>
		public CustomEllipse SelectedVertex => Vertices.SingleOrDefault(f => f.IsSelected);

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
			var firstVertex = new CustomEllipse(point);
			Vertices.AddFirst(firstVertex);
			FigureShapes = new LinkedList<IShape>();
			FigureShapes.AddFirst(firstVertex);
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
		/// Gets the line which contains the given point.
		/// </summary>
		/// <param name="point">The point.</param>
		/// <returns>The line containing the point or null if no line contains this point.</returns>
		public CustomLine GetLineContainingPoint(Point point)
		{
			foreach (var line in FigureShapes.OfType<CustomLine>())
			{
				var lineLength = Math.Sqrt((line.StartPoint.X - line.EndPoint.X) * (line.StartPoint.X - line.EndPoint.X)
								   + (line.StartPoint.Y - line.EndPoint.Y) * (line.StartPoint.Y - line.EndPoint.Y));
				var distanceFromStart = Math.Sqrt((point.X - line.StartPoint.X) * (point.X - line.StartPoint.X)
								   + (point.Y - line.StartPoint.Y) * (point.Y - line.StartPoint.Y));
				var distanceFromEnd = Math.Sqrt((point.X - line.EndPoint.X) * (point.X - line.EndPoint.X)
								   + (point.Y - line.EndPoint.Y) * (point.Y - line.EndPoint.Y));

				if (!(distanceFromStart + distanceFromEnd < lineLength + 2) || !(distanceFromStart + distanceFromEnd > lineLength - 2)) continue;
				return line;
			}

			return null;
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
		/// Selects the figure.
		/// </summary>
		public void Select()
		{
			IsSelected = true;
		}

		/// <summary>
		/// Deselects the figure.
		/// </summary>
		public void Deselect()
		{
			IsSelected = false;
			foreach (var vertex in Vertices.Where(v => v.IsSelected))
				vertex.Deselect();
		}

		/// <summary>
		/// Determines whether this vertex can be dragged by the given deltas.
		/// </summary>
		/// <param name="deltaX">Change in x coordinate.</param>
		/// <param name="deltaY">Change in y coordinate.</param>
		/// <param name="xBound">Max x position that can be set.</param>
		/// <param name="yBound">Max y position that can be set.</param>
		/// <returns>True if this vertex can be dragged, false otherwise.</returns>
		public bool CanDrag(int deltaX, int deltaY, int xBound, int yBound)
		{
			return MaxX + deltaX < xBound - Delta
				   && MinX + deltaX > Delta
				   && MaxY + deltaY < yBound - Delta
				   && MinY + deltaY > Delta;
		}

		/// <summary>
		/// Moves the figure by the given delta.
		/// </summary>
		/// <param name="deltaX">Change in X coordinate.</param>
		/// <param name="deltaY">Change in Y coordinate.</param>
		public void Move(int deltaX, int deltaY)
		{
			foreach (var vertex in Vertices)
				vertex.Position = new Point(vertex.Position.X + deltaX, vertex.Position.Y + deltaY);

			foreach (var line in FigureShapes.OfType<CustomLine>())
			{
				line.StartPoint = new Point(line.StartPoint.X + deltaX, line.StartPoint.Y + deltaY);
				line.EndPoint = new Point(line.EndPoint.X + deltaX, line.EndPoint.Y + deltaY);
			}

			UpdateBoundingBox();
		}

		/// <summary>
		/// Moves the vertex by the given delta.
		/// </summary>
		/// <param name="deltaX">Change in X coordinate.</param>
		/// <param name="deltaY">Change in Y coordinate.</param>
		public void MoveSelectedVertex(int deltaX, int deltaY)
		{
			var vertexNode = FigureShapes.First(v => v is CustomEllipse e && e.Position == SelectedVertex.Position);
			var vertex = FigureShapes.Find(vertexNode);
			UpdateLinesPositions(vertex, deltaX, deltaY);
			var newPosition = new Point(SelectedVertex.Position.X + deltaX, SelectedVertex.Position.Y + deltaY);
			SelectedVertex.Position = newPosition;

			UpdateBoundingBox();
		}

		/// <summary>
		/// Tries to add a new vertex to this figure.
		/// </summary>
		/// <param name="point">Position of the new vertex.</param>
		/// <returns>True if the vertex was successfully added, false otherwise.</returns>
		public bool TryAddVertex(Point point)
		{
			if (Vertices.Count < 3 || !WillCloseFigure(point))
			{
				AddVertex(point);
				return true;
			}

			FigureShapes.Remove(FigureShapes.OfType<CustomLine>().LastOrDefault());
			FigureShapes.AddLast(new CustomLine(LastVertex.Position, FirstVertex.Position));

			return false;
		}

		/// <summary>
		/// Draws a line between last vertex and the given location.
		/// </summary>
		/// <param name="point">The line's end point.</param>
		public void DrawTemporaryLine(Point point)
		{
			if (FigureShapes.Last.Value is CustomLine) FigureShapes.Remove(FigureShapes.Last());
			FigureShapes.AddLast(new CustomLine(LastVertex.Position, point));
		}

		private bool WillCloseFigure(Point point)
		{
			return FindVertexAtPoint(point) == FirstVertex;
		}

		private void AddVertex(Point point)
		{
			if (IsVertex(point, out _)) return;

			var vertex = new CustomEllipse(point);
			Vertices.AddLast(vertex);
			FigureShapes.AddLast(vertex);

			UpdateBoundingBox();
		}

		private void UpdateLinesPositions(LinkedListNode<IShape> node, int deltaX, int deltaY)
		{
			var previousLine = node.Previous?.Value as CustomLine ?? FigureShapes.Last.Value as CustomLine;
			previousLine.EndPoint = new Point(previousLine.EndPoint.X + deltaX, previousLine.EndPoint.Y + deltaY);

			var nextLine = node.Next?.Value as CustomLine ?? FigureShapes.First.Value as CustomLine;
			nextLine.StartPoint = new Point(nextLine.StartPoint.X + deltaX, nextLine.StartPoint.Y + deltaY);
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
