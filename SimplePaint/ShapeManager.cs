﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SimplePaint.Shapes;

namespace SimplePaint
{
	public class ShapeManager
	{
		/// <summary>
		/// Gets the list of all figures.
		/// </summary>
		public List<CustomFigure> Figures { get; }

		/// <summary>
		/// Gets the selected figure.
		/// </summary>
		public CustomFigure SelectedFigure => Figures.SingleOrDefault(f => f.IsSelected);

		/// <summary>
		/// Gets the selected figure for multisampling.
		/// </summary>
		public CustomFigure MultisamplingFigure => Figures.SingleOrDefault(f => f.MultisamplingLine != null);

		/// <summary>
		/// Gets the currently drawn figure.
		/// </summary>
		public CustomFigure CurrentFigure { get; private set; }

		public ShapeManager()
		{
			Figures = new List<CustomFigure>();
		}

		/// <summary>
		/// Selects the figure in given location.
		/// </summary>
		/// <param name="location">Location of the figure.</param>
		public void SelectFigure(Point location)
		{
			var figure = Figures.FirstOrDefault(f => f.ContainsPoint(location));
			figure?.Select();
		}

		/// <summary>
		/// Deselects the selected figure.
		/// </summary>
		public void DeselectFigures()
		{
			Figures.ForEach(f => f.Deselect());
		}

		/// <summary>
		/// Initializes drawing a new figure.
		/// </summary>
		/// <param name="point">Position of the first vertex of the figure.</param>
		/// <param name="color">Color of the figure.</param>
		/// <param name="strokeThickness">Stroke thickness.</param>
		public void StartDrawingFigure(Point point, Color color, int strokeThickness)
		{
			CurrentFigure = new CustomFigure(point, color, strokeThickness);
		}

		/// <summary>
		/// Tries to add a vertex at given position to the current figure.
		/// </summary>
		/// <param name="point">Position of the vertex.</param>
		public void TryAddVertexToCurrentFigure(Point point)
		{
			var added = CurrentFigure.TryAddVertex(point);
			if (added) return;

			Figures.Add(CurrentFigure);
			CurrentFigure = null;
		}

		/// <summary>
		/// Cancels drawing the currently drawn figure.
		/// </summary>
		public void CancelDrawing()
		{
			CurrentFigure = null;
		}

		/// <summary>
		/// Marks the line at containing given point to be drawn using multisampling.
		/// </summary>
		/// <param name="point">Point on the line.</param>
		public void SelectLineForMultisampling(Point point)
		{
			var multisamplingFigure = MultisamplingFigure;
			if (multisamplingFigure != null) multisamplingFigure.MultisamplingLine = null;

			foreach (var figure in Figures)
			{
				var line = figure.GetLineContainingPoint(point);
				if (line == null) continue;
				figure.MultisamplingLine = line;
				return;
			}
		}

		/// <summary>
		/// Tries to add a vertex at given position.
		/// </summary>
		/// <param name="point">Position of the new vertex.</param>
		public void TryAddVertexToFigure(Point point)
		{
			foreach (var figure in Figures)
			{
				var line = figure.GetLineContainingPoint(point);
				if (line == null) continue;

				figure.AddVertexOnLine(point, line);
				return;
			}
		}

		/// <summary>
		/// Searches for a figure at given point and changes its color to the given one.
		/// </summary>
		/// <param name="color">Color to change to.</param>
		/// <param name="point">Location of the figure.</param>
		public void ChangeFigureColor(Color color, Point point)
		{
			foreach (var figure in Figures)
			{
				var line = figure.GetLineContainingPoint(point);
				if (line == null) continue;

				figure.FigureColor = color;
				return;
			}
		}

		/// <summary>
		/// Searches for a figure at given point and changes its stroke thickness to the given one.
		/// </summary>
		/// <param name="thickness">Desired thickness.</param>
		/// <param name="point">Location of the figure.</param>
		public void ChangeFigureThickness(int thickness, Point point)
		{
			foreach (var figure in Figures)
			{
				var line = figure.GetLineContainingPoint(point);
				if (line == null) continue;

				figure.StrokeThickness = thickness;
				return;
			}
		}
	}
}
