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
		public CustomFigure SelectedFigure { get; private set; }

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
			SelectedFigure = Figures.FirstOrDefault(f => CustomFigure.IsPointInFigure(location, f));
		}

		/// <summary>
		/// Deselects the selected figure.
		/// </summary>
		public void DeselectFigure()
		{
			SelectedFigure = null;
		}
	}
}
