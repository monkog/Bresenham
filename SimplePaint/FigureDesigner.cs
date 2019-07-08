using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SimplePaint.Shapes;

namespace SimplePaint
{
	public partial class FigureDesigner : Form
	{
		private readonly ShapeManager _shapeManager = new ShapeManager();

		private readonly CursorManager _cursorManager = new CursorManager();

		public FigureDesigner()
		{
			InitializeComponent();
		}

		#region Private Members

		/// <summary>
		/// The currently drawn figure
		/// </summary>
		private CustomFigure _currentFigure;
		/// <summary>
		/// The current stroke thickness
		/// </summary>
		private int _strokeThickness;
		/// <summary>
		/// Is the user currently adding a vertex
		/// </summary>
		private bool _doAddVertex;
		/// <summary>
		/// The last mouse down position
		/// </summary>
		private Point _mouseDownPosition;
		/// <summary>
		/// The last mouse up position
		/// </summary>
		private Point _mouseUpPosition;
		/// <summary>
		/// The last mouse position
		/// </summary>
		private Point _mouseLastPosition;
		/// <summary>
		/// Is the user drawing a figure
		/// </summary>
		private bool _doDrawFigure;

		/// <summary>
		/// Is the user changing color
		/// </summary>
		private bool _doChangeColor;

		/// <summary>
		/// Is the user changing thickness
		/// </summary>
		private bool _doChangeThickness;
		/// <summary>
		/// The selected line
		/// </summary>
		private CustomLine _selectedLine;
		/// <summary>
		/// The multisampling figure
		/// </summary>
		private CustomFigure _multisamplingFigure;
		/// <summary>
		/// Is the user doing multisample
		/// </summary>
		private bool _doMultisample;
		#endregion Private Members

		private void FigureDesigner_Load(object sender, EventArgs e)
		{
			InitializeDefaultValues();
			SetDefaultSettings();

			drawFigureButton.PerformClick();
		}

		private void MouseDownOccured(object sender, MouseEventArgs e)
		{
			_mouseDownPosition = new Point(e.X, e.Y);
			_mouseLastPosition = _mouseDownPosition;

			if (_shapeManager.SelectedFigure != null || _doDrawFigure || _doAddVertex || _doChangeColor || _doChangeThickness)
				return;

			_shapeManager.SelectFigure(e.Location);

			if (_doMultisample && SelectLineForMultisampling(e.Location)) return;

			if (_shapeManager.SelectedFigure == null) return;
			Cursor = Cursors.SizeAll;
			if (_shapeManager.SelectedFigure.IsVertex(e.Location, out var vertex)) vertex.Select();
		}

		private void MouseMoveOccured(object sender, MouseEventArgs e)
		{
			Cursor = (_doAddVertex || _doDrawFigure) ? Cursors.Cross : Cursors.Default;

			if (_shapeManager.SelectedFigure == null && !_doDrawFigure && !_doAddVertex && SetCursorImage(e.Location)) return;
			if (_shapeManager.SelectedFigure != null && _shapeManager.SelectedFigure.Vertices.Any(v => v.IsSelected) && MoveVertex(e.Location)) return;
			if (_shapeManager.SelectedFigure != null && !MoveFigure(e.Location)) return;

			// Proceed when drawing current figure is in progress. 
			// Draws a temporary line between the last vertex and current mouse position.
			if (_doDrawFigure && _currentFigure != null)
			{
				if (_currentFigure.FigureShapes.Last() is CustomLine)
					_currentFigure.FigureShapes.Remove(_currentFigure.FigureShapes.Last());
				_currentFigure.FigureShapes.AddLast(new CustomLine(_currentFigure.LastVertex.Position, new Point(e.X, e.Y)));
			}

			drawingArea.Refresh();
		}

		private void MouseUpOccured(object sender, MouseEventArgs e)
		{
			_mouseUpPosition = new Point(e.X, e.Y);
			_shapeManager.DeselectFigures();

			// Add vertex to existing figure.
			if (!_doDrawFigure && _doAddVertex)
			{
				foreach (var figure in _shapeManager.Figures)
				{
					var line = figure.GetLineContainingPoint(_mouseUpPosition);
					if (line == null) continue;

					figure.AddVertexOnLine(_mouseUpPosition, line);
					drawingArea.Refresh();
					return;
				}
			}

			if (_doDrawFigure && DrawFigure()) return;

			if (_doChangeColor && ChangeFigureColor(e.Location)) return;

			if (_doChangeThickness) ChangeFigureThickness(e.Location);
		}

		/// <summary>
		/// Handles the Paint event of the drawingArea control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
		private void drawingArea_Paint(object sender, PaintEventArgs e)
		{
			if (_currentFigure != null)
				foreach (IShape shape in _currentFigure.FigureShapes)
					shape.Draw(e.Graphics, _currentFigure.FigureColor, _currentFigure.StrokeThickness);
			if (_shapeManager.Figures.Any())
				foreach (CustomFigure figure in _shapeManager.Figures)
				{
					if (_doMultisample && _multisamplingFigure == figure && _selectedLine != null)
					{
						foreach (IShape shape in figure.FigureShapes)
							if (shape.GetType() == typeof(CustomLine))
							{
								CustomLine line = shape as CustomLine;
								var color = line == _selectedLine ? figure.MultisamplingColor : figure.FigureColor;
								shape.Draw(e.Graphics, color, figure.StrokeThickness);
							}
							else
								shape.Draw(e.Graphics, figure.FigureColor, figure.StrokeThickness);
					}
					else
						foreach (IShape shape in figure.FigureShapes)
							shape.Draw(e.Graphics, figure.FigureColor, figure.StrokeThickness);
				}
		}

		private void drawFigureButton_Click(object sender, EventArgs e)
		{
			if (_doDrawFigure)
			{
				Cursor = Cursors.Default;
				SetButtonStates(addVertex: false, drawFigure: false, changeColor: false
					, changeThickness: false, doMultisampling: false);
				_currentFigure = null;
				return;
			}

			SetButtonStates(addVertex: false, drawFigure: true, changeColor: false
				, changeThickness: false, doMultisampling: false);
			Cursor = Cursors.Cross;
		}
		/// <summary>
		/// Handles the Click event of the addVertexButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void addVertexButton_Click(object sender, EventArgs e)
		{
			if (_doAddVertex)
			{
				Cursor = Cursors.Default;
				SetButtonStates(addVertex: false, drawFigure: false, changeColor: false
					, changeThickness: false, doMultisampling: false);
				return;
			}

			if (!_shapeManager.Figures.Any() || (_doDrawFigure && _currentFigure != null))
			{
				MessageBox.Show("You have to finish drawing at least one figure to use this functionality.", "Don't do that!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			SetButtonStates(addVertex: true, drawFigure: false, changeColor: false
				, changeThickness: false, doMultisampling: false);

			Cursor = Cursors.Cross;
		}
		/// <summary>
		/// Clears all figures and restores the default settings.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void clearButton_Click(object sender, EventArgs e)
		{
			SetDefaultSettings();
		}
		/// <summary>
		/// Handles the Click event of the changeColorButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void changeColorButton_Click(object sender, EventArgs e)
		{
			if (_doDrawFigure && _currentFigure != null && _currentFigure.Vertices.Any())
			{
				MessageBox.Show("You have to finish drawing to use this functionality.", "Don't do that!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (_doChangeColor && !_doDrawFigure)
			{
				SetButtonStates(false, false, false, false, false);
				return;
			}

			var colorDialog = new ColorDialog { AllowFullOpen = true };

			if (colorDialog.ShowDialog() != DialogResult.OK) return;
			colorPictureBox.BackColor = colorDialog.Color;
			drawingArea.Refresh();

			if (!_doDrawFigure) SetButtonStates(false, false, true, false, false);
		}
		/// <summary>
		/// Handles the Click event of the changeSizeButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void changeSizeButton_Click(object sender, EventArgs e)
		{
			if (_doDrawFigure && _currentFigure != null && _currentFigure.Vertices.Any())
			{
				MessageBox.Show("You have to finish drawing to use this functionality.", "Don't do that!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (_doChangeThickness)
			{
				SetButtonStates(addVertex: false, changeColor: false, changeThickness: false
					, drawFigure: false, doMultisampling: false);
				return;
			}

			var enterValueWindow = new EnterValueWindow();
			enterValueWindow.ShowDialog();

			if (!_doDrawFigure)
				SetButtonStates(addVertex: false, drawFigure: false, changeColor: false
					, changeThickness: true, doMultisampling: false);

			_strokeThickness = enterValueWindow.LineThickness;
			sizeLabel.Text = "CURRENT SIZE: " + enterValueWindow.LineThickness + " px";
		}
		/// <summary>
		/// Handles the Click event of the multisamplingButton control.
		/// </summary>
		/// <remarks>
		/// Draws the figure using multisampling
		/// </remarks>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void multisamplingButton_Click(object sender, EventArgs e)
		{
			if (_doMultisample)
			{
				SetButtonStates(addVertex: false, drawFigure: false, changeColor: false
					, changeThickness: false, doMultisampling: false);
				return;
			}
			SetButtonStates(addVertex: false, drawFigure: false, changeColor: false
				, changeThickness: false, doMultisampling: true);
		}

		#region Private Methods
		/// <summary>
		/// Sets the default settings and clears the figures.
		/// </summary>
		private void SetDefaultSettings()
		{
			Cursor = Cursors.Default;

			SetButtonStates(addVertex: false, drawFigure: false, changeColor: false
				, changeThickness: false, doMultisampling: false);

			_shapeManager.Figures.Clear();
			_currentFigure = null;
			drawingArea.Refresh();
		}

		private void InitializeDefaultValues()
		{
			_strokeThickness = 2;

			_mouseDownPosition = Point.Empty;
			_mouseUpPosition = Point.Empty;

			_shapeManager.DeselectFigures();
			_selectedLine = null;
			_multisamplingFigure = null;
			_doMultisample = false;
		}

		/// <summary>
		/// Sets the button states.
		/// </summary>
		/// <param name="addVertex">if set to <c>true</c> [add vertex].</param>
		/// <param name="drawFigure">if set to <c>true</c> [draw figure].</param>
		/// <param name="changeColor">if set to <c>true</c> [change color].</param>
		/// <param name="changeThickness">if set to <c>true</c> [change thickness].</param>
		/// <param name="doMultisampling">if set to <c>true</c> [do multisampling].</param>
		private void SetButtonStates(bool addVertex, bool drawFigure, bool changeColor
			, bool changeThickness, bool doMultisampling)
		{
			_doAddVertex = addVertex;
			_doDrawFigure = drawFigure;
			_doChangeColor = changeColor;
			_doChangeThickness = changeThickness;
			_doMultisample = doMultisampling;
			changeColorButton.Text = changeColor ? "CANCEL" : "CHANGE COLOR";
			addVertexButton.Text = addVertex ? "CANCEL" : "ADD VERTEX";
			drawFigureButton.Text = drawFigure ? "CANCEL" : "DRAW FIGURE";
			changeSizeButton.Text = changeThickness ? "CANCEL" : "CHANGE SIZE";
			multisamplingButton.Text = doMultisampling ? "CANCEL" : "MULTISAMPLING";
		}

		/// <summary>
		/// Selects the line for multisampling.
		/// </summary>
		/// <param name="location">The location.</param>
		/// <returns>True if such a line was found, false otherwise</returns>
		private bool SelectLineForMultisampling(Point location)
		{
			foreach (var figure in _shapeManager.Figures)
			{
				var line = figure.GetLineContainingPoint(location);
				if (line == null) continue;

				if (_multisamplingFigure != null) _multisamplingFigure.MultisamplingLine = null;

				_multisamplingFigure = figure;
				_selectedLine = line;
				figure.MultisamplingLine = line;
				drawingArea.Refresh();
				return true;
			}

			return false;
		}
		/// <summary>
		/// Changes the cursor image if the drawing mode is off and mouse is above any of the previously drawn shapes.
		/// </summary>
		/// <param name="location">The location.</param>
		/// <returns>True if the cursor has been changed, false otherwise</returns>
		private bool SetCursorImage(Point location)
		{
			var isCursorOverFigure = _shapeManager.Figures.Any(figure => figure.GetLineContainingPoint(location) != null);

			if (_doChangeColor && isCursorOverFigure)
			{
				Cursor = _cursorManager.BucketCursor;
			}
			else if (_doChangeThickness && isCursorOverFigure)
			{
				Cursor = _cursorManager.PenCursor;
				return true;
			}
			else if (!_doChangeColor && !_doChangeThickness)
			{
				if (_shapeManager.Figures.Any(figure => figure.IsVertex(location, out _)))
				{
					Cursor = Cursors.Hand;
					return true;
				}

				if (_shapeManager.Figures.Any(figure => figure.ContainsPoint(location)))
				{
					Cursor = Cursors.SizeAll;
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// Changes the line thickness of the chosen figure.
		/// </summary>
		/// <param name="location">The location.</param>
		private void ChangeFigureThickness(Point location)
		{
			foreach (var figure in _shapeManager.Figures)
			{
				var line = figure.GetLineContainingPoint(location);
				if (line == null) continue;

				figure.StrokeThickness = _strokeThickness;
				drawingArea.Refresh();
				return;
			}
		}
		/// <summary>
		/// Changes the color of the chosen figure.
		/// </summary>
		/// <param name="location">The location.</param>
		/// <returns></returns>
		private bool ChangeFigureColor(Point location)
		{
			foreach (var figure in _shapeManager.Figures)
			{
				var line = figure.GetLineContainingPoint(location);
				if (line == null) continue;

				figure.FigureColor = colorPictureBox.BackColor;
				drawingArea.Refresh();
				return true;
			}

			return false;
		}
		/// <summary>
		/// Draws the figure.
		/// </summary>
		/// <returns>True if the figure is completed or can be completed but has less than 3 vertices</returns>
		private bool DrawFigure()
		{
			if (_currentFigure == null)
			{
				_currentFigure = new CustomFigure(_mouseUpPosition, colorPictureBox.BackColor, _strokeThickness);
			}
			else
			{
				if (_currentFigure.WillCloseFigure(_mouseUpPosition))
				{
					if (_currentFigure.Vertices.Count < 3) return true;

					_shapeManager.Figures.Add(_currentFigure);
					_currentFigure.FigureShapes.Remove(_currentFigure.FigureShapes.Last());
					_currentFigure.FigureShapes.AddLast(new CustomLine(_currentFigure.LastVertex.Position,
						_currentFigure.FirstVertex.Position));
					drawingArea.Refresh();

					_currentFigure = null;
					return true;
				}
				_currentFigure.AddVertex(_mouseUpPosition);
			}
			drawingArea.Refresh();
			return false;
		}
		/// <summary>
		/// Move the vertex in the bounds of drawingArea.
		/// </summary>
		/// <param name="point">The mouse position.</param>
		/// <returns>True if there was a vertex to move and it was moved, false otherwise</returns>
		private bool MoveVertex(Point point)
		{
			var deltaX = point.X - _mouseLastPosition.X;
			var deltaY = point.Y - _mouseLastPosition.Y;

			var selectedFigure = _shapeManager.SelectedFigure;
			var vertex = selectedFigure.SelectedVertex;
			if (!vertex.CanDrag(deltaX, deltaY, drawingArea.Bounds.Width, drawingArea.Bounds.Height)) return true;

			Cursor = Cursors.Hand;
			selectedFigure.MoveSelectedVertex(deltaX, deltaY);
			drawingArea.Refresh();

			_mouseLastPosition = point;
			return false;
		}

		private bool MoveFigure(Point point)
		{
			var deltaX = point.X - _mouseLastPosition.X;
			var deltaY = point.Y - _mouseLastPosition.Y;
			var selectedFigure = _shapeManager.SelectedFigure;

			if (!selectedFigure.CanDrag(deltaX, deltaY, drawingArea.Bounds.Width, drawingArea.Bounds.Height)) return false;

			selectedFigure.Move(deltaX, deltaY);

			_mouseLastPosition = point;
			Cursor = Cursors.SizeAll;
			return true;
		}

		#endregion Private Methods
	}
}
