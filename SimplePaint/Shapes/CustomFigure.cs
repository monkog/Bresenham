﻿using System;
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

		private readonly Color _multisamplingColor = Color.Azure;

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
		/// Gets the figure lines.
		/// </summary>
		public List<CustomLine> Lines { get; }

		/// <summary>
		/// Gets or sets the figure vertices.
		/// </summary>
		public LinkedList<CustomEllipse> Vertices { get; }

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
			Lines = new List<CustomLine>();
			MultisamplingLine = null;
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

			var newVertex = new CustomEllipse(point);
			Vertices.AddAfter(Vertices.Find(line.StartPoint), newVertex);
			Lines.Remove(line);

			var firstLine = new CustomLine(line.StartPoint, newVertex);
			var secondLine = new CustomLine(newVertex, line.EndPoint);
			Lines.Add(firstLine);
			Lines.Add(secondLine);

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

			var currentVertex = Vertices.First;
			var previousVertex = Vertices.Last;

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
			foreach (var line in Lines)
			{
				var startX = line.StartPoint.Position.X;
				var startY = line.StartPoint.Position.Y;
				var endX = line.EndPoint.Position.X;
				var endY = line.EndPoint.Position.Y;

				var lineLength = Math.Sqrt((startX - endX) * (startX - endX) + (startY - endY) * (startY - endY));
				var distanceFromStart = Math.Sqrt((point.X - startX) * (point.X - startX) + (point.Y - startY) * (point.Y - startY));
				var distanceFromEnd = Math.Sqrt((point.X - endX) * (point.X - endX) + (point.Y - endY) * (point.Y - endY));

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

			UpdateBoundingBox();
		}

		/// <summary>
		/// Moves the vertex by the given delta.
		/// </summary>
		/// <param name="deltaX">Change in X coordinate.</param>
		/// <param name="deltaY">Change in Y coordinate.</param>
		public void MoveSelectedVertex(int deltaX, int deltaY)
		{
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

			var firstVertex = Vertices.First.Value;
			var lastVertex = Vertices.Last.Value;
			if (!Vertices.Contains(Lines.Last().EndPoint)) Lines.Remove(Lines.Last());
			Lines.Add(new CustomLine(lastVertex, firstVertex));
			UpdateBoundingBox();

			return false;
		}

		/// <summary>
		/// Draws a line between last vertex and the given location.
		/// </summary>
		/// <param name="point">The line's end point.</param>
		public void DrawTemporaryLine(Point point)
		{
			if (Lines.Any() && !Vertices.Contains(Lines.Last().EndPoint)) Lines.Remove(Lines.Last());
			var lastVertex = Vertices.Last.Value;
			Lines.Add(new CustomLine(lastVertex, new CustomEllipse(point)));
		}

		/// <summary>
		/// Draws the custom figure.
		/// </summary>
		/// <param name="graphics">Graphics instance.</param>
		/// <param name="withMultisampling">Determines whether multisampling algorithm should be used.</param>
		public void Draw(Graphics graphics, bool withMultisampling = false)
		{
			foreach (var vertex in Vertices)
				vertex.Draw(graphics, FigureColor, StrokeThickness);

			foreach (var line in Lines.Where(line => line != MultisamplingLine))
				line.Draw(graphics, FigureColor, StrokeThickness);

			var color = withMultisampling ? _multisamplingColor : FigureColor;
			MultisamplingLine?.Draw(graphics, color, StrokeThickness);
		}

		private bool WillCloseFigure(Point point)
		{
			return FindVertexAtPoint(point) == Vertices.First.Value;
		}

		private void AddVertex(Point point)
		{
			if (IsVertex(point, out _)) return;

			var vertex = new CustomEllipse(point);
			var lastLine = Lines.LastOrDefault();
			if (lastLine != null && !Vertices.Contains(lastLine.EndPoint)) Lines.Remove(Lines.LastOrDefault());
			Lines.Add(new CustomLine(Vertices.Last.Value, vertex));
			Vertices.AddLast(vertex);

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
