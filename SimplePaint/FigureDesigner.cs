using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SimplePaint.Properties;
using SimplePaint.Shapes;

namespace SimplePaint
{
	public partial class FigureDesigner : Form
	{
		private readonly ShapeManager _shapeManager = new ShapeManager();

		private readonly CursorManager _cursorManager = new CursorManager();

		private FormState _formState;

		private Point _mouseDownPosition;

		private Point _mouseUpPosition;

		private Point _mouseLastPosition;

		private int _strokeThickness = 2;

		public FigureDesigner()
		{
			InitializeComponent();
			SetFormState(FormState.DrawFigure);
		}

		private void MouseDownOccured(object sender, MouseEventArgs e)
		{
			_mouseDownPosition = new Point(e.X, e.Y);
			_mouseLastPosition = _mouseDownPosition;

			if (_shapeManager.SelectedFigure != null || !(_formState == FormState.Default || _formState == FormState.Multisampling))
				return;

			_shapeManager.SelectFigure(e.Location);

			if (_formState == FormState.Multisampling && SelectLineForMultisampling(e.Location)) return;

			if (_shapeManager.SelectedFigure == null) return;
			Cursor = Cursors.SizeAll;
			if (_shapeManager.SelectedFigure.IsVertex(e.Location, out var vertex)) vertex.Select();
		}

		private void MouseMoveOccured(object sender, MouseEventArgs e)
		{
			Cursor = (_formState == FormState.AddVertex || _formState == FormState.DrawFigure) ? Cursors.Cross : Cursors.Default;

			if (_shapeManager.SelectedFigure == null && _formState != FormState.DrawFigure && _formState != FormState.AddVertex && SetCursorImage(e.Location)) return;
			if (_shapeManager.SelectedFigure != null && _shapeManager.SelectedFigure.Vertices.Any(v => v.IsSelected) && MoveVertex(e.Location)) return;
			if (_shapeManager.SelectedFigure != null && !MoveFigure(e.Location)) return;

			// Proceed when drawing current figure is in progress. 
			// Draws a temporary line between the last vertex and current mouse position.
			if (_formState == FormState.DrawFigure && _shapeManager.CurrentFigure != null)
			{
				if (_shapeManager.CurrentFigure.FigureShapes.Last() is CustomLine)
					_shapeManager.CurrentFigure.FigureShapes.Remove(_shapeManager.CurrentFigure.FigureShapes.Last());
				_shapeManager.CurrentFigure.FigureShapes.AddLast(new CustomLine(_shapeManager.CurrentFigure.LastVertex.Position, new Point(e.X, e.Y)));
			}

			drawingArea.Refresh();
		}

		private void MouseUpOccured(object sender, MouseEventArgs e)
		{
			_mouseUpPosition = new Point(e.X, e.Y);
			_shapeManager.DeselectFigures();

			// Add vertex to existing figure.
			if (_formState == FormState.AddVertex)
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

			if (_formState == FormState.DrawFigure)
			{
				AddNewVertex();
				return;
			}

			if (_formState == FormState.ChangeColor && ChangeFigureColor(e.Location)) return;

			if (_formState == FormState.ChangeThickness) ChangeFigureThickness(e.Location);
		}

		private void drawingArea_Paint(object sender, PaintEventArgs e)
		{
			if (_shapeManager.CurrentFigure != null)
				foreach (IShape shape in _shapeManager.CurrentFigure.FigureShapes)
					shape.Draw(e.Graphics, _shapeManager.CurrentFigure.FigureColor, _shapeManager.CurrentFigure.StrokeThickness);
			if (!_shapeManager.Figures.Any()) return;
			var multisamplingFigure = _shapeManager.MultisamplingFigure;

			foreach (CustomFigure figure in _shapeManager.Figures)
			{
				if (_formState == FormState.Multisampling && multisamplingFigure == figure)
				{
					foreach (IShape shape in figure.FigureShapes)
						if (shape.GetType() == typeof(CustomLine))
						{
							CustomLine line = shape as CustomLine;
							var color = line == multisamplingFigure.MultisamplingLine ? figure.MultisamplingColor : figure.FigureColor;
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
			if (_formState == FormState.DrawFigure)
			{
				SetFormState(FormState.Default);
				_shapeManager.CancelDrawing();
				drawingArea.Refresh();
				return;
			}

			SetFormState(FormState.DrawFigure);
		}

		private void addVertexButton_Click(object sender, EventArgs e)
		{
			if (_formState == FormState.AddVertex)
			{
				SetFormState(FormState.Default);
				return;
			}

			if (!_shapeManager.Figures.Any() || (_formState == FormState.DrawFigure && _shapeManager.CurrentFigure != null))
			{
				MessageBox.Show("You have to finish drawing at least one figure to use this functionality.", "Don't do that!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			SetFormState(FormState.AddVertex);
			Cursor = Cursors.Cross;
		}

		private void clearButton_Click(object sender, EventArgs e)
		{
			SetDefaultSettings();
		}

		private void changeColorButton_Click(object sender, EventArgs e)
		{
			if (_formState == FormState.DrawFigure && _shapeManager.CurrentFigure != null && _shapeManager.CurrentFigure.Vertices.Any())
			{
				MessageBox.Show("You have to finish drawing to use this functionality.", "Don't do that!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (_formState == FormState.ChangeColor)
			{
				SetFormState(FormState.Default);
				return;
			}

			var colorDialog = new ColorDialog { AllowFullOpen = true };

			if (colorDialog.ShowDialog() != DialogResult.OK) return;
			colorPictureBox.BackColor = colorDialog.Color;
			drawingArea.Refresh();

			if (_formState != FormState.DrawFigure) SetFormState(FormState.ChangeColor);
		}

		private void changeSizeButton_Click(object sender, EventArgs e)
		{
			if (_formState == FormState.DrawFigure && _shapeManager.CurrentFigure != null && _shapeManager.CurrentFigure.Vertices.Any())
			{
				MessageBox.Show("You have to finish drawing to use this functionality.", "Don't do that!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (_formState == FormState.ChangeThickness)
			{
				SetFormState(FormState.Default);
				return;
			}

			var enterValueWindow = new EnterValueWindow();
			enterValueWindow.ShowDialog();

			if (_formState != FormState.DrawFigure) SetFormState(FormState.ChangeThickness);

			_strokeThickness = enterValueWindow.LineThickness;
			sizeLabel.Text = "CURRENT SIZE: " + enterValueWindow.LineThickness + " px";
		}

		private void multisamplingButton_Click(object sender, EventArgs e)
		{
			var state = _formState == FormState.Multisampling ? FormState.Default : FormState.Multisampling;
			SetFormState(state);
		}

		private void SetDefaultSettings()
		{
			SetFormState(FormState.Default);

			_shapeManager.Figures.Clear();
			_shapeManager.CancelDrawing();
			drawingArea.Refresh();
		}

		private void SetFormState(FormState state)
		{
			_formState = state;
			changeColorButton.Text = Resources.ChangeColor;
			addVertexButton.Text = Resources.AddVertex;
			drawFigureButton.Text = Resources.DrawFigure;
			changeSizeButton.Text = Resources.ChangeThickness;
			multisamplingButton.Text = Resources.Multisampling;

			switch (state)
			{
				case FormState.DrawFigure:
					drawFigureButton.Text = Resources.Cancel;
					Cursor = Cursors.Cross;
					break;
				case FormState.AddVertex:
					addVertexButton.Text = Resources.Cancel;
					break;
				case FormState.ChangeColor:
					changeColorButton.Text = Resources.Cancel;
					break;
				case FormState.Multisampling:
					multisamplingButton.Text = Resources.Cancel;
					break;
				case FormState.ChangeThickness:
					changeSizeButton.Text = Resources.Cancel;
					break;
				case FormState.Default:
					Cursor = Cursors.Default;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(state), state, null);
			}
		}

		private bool SelectLineForMultisampling(Point location)
		{
			var multisamplingFigure = _shapeManager.MultisamplingFigure;
			if (multisamplingFigure != null) multisamplingFigure.MultisamplingLine = null;

			foreach (var figure in _shapeManager.Figures)
			{
				var line = figure.GetLineContainingPoint(location);
				if (line == null) continue;
				figure.MultisamplingLine = line;
				drawingArea.Refresh();
				return true;
			}

			return false;
		}

		private bool SetCursorImage(Point location)
		{
			var isCursorOverFigure = _shapeManager.Figures.Any(figure => figure.GetLineContainingPoint(location) != null);

			if (_formState == FormState.ChangeColor && isCursorOverFigure)
			{
				Cursor = _cursorManager.BucketCursor;
			}
			else if (_formState == FormState.ChangeThickness && isCursorOverFigure)
			{
				Cursor = _cursorManager.PenCursor;
				return true;
			}
			else if (_formState != FormState.ChangeColor && _formState != FormState.ChangeThickness)
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

		private void AddNewVertex()
		{
			if (_shapeManager.CurrentFigure == null)
			{
				_shapeManager.StartDrawingFigure(_mouseUpPosition, colorPictureBox.BackColor, _strokeThickness);
			}

			_shapeManager.TryAddVertexToCurrentFigure(_mouseUpPosition);
			drawingArea.Refresh();
		}
		
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
	}
}
