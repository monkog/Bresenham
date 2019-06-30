using System.Collections.Generic;
using SimplePaint.Shapes;

namespace SimplePaint
{
	public class ShapeManager
	{
		/// <summary>
		/// List of all figures.
		/// </summary>
		public List<CustomFigure> Figures { get; }

		public ShapeManager()
		{
			Figures = new List<CustomFigure>();
		}
	}
}
