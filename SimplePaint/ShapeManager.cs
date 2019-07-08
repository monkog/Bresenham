using System.Collections.Generic;
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
	}
}
