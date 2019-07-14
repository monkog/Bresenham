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
			_mouseDownPosition = e.Location;
			_mouseLastPosition = _mouseDownPosition;

			if (_formState == FormState.Multisampling)
			{
				_shapeManager.SelectLineForMultisampling(e.Location);
				drawingArea.Refresh();
				return;
			}

			if (_formState != FormState.Default) return;

			_shapeManager.SelectFigure(e.Location);
			if (_shapeManager.SelectedFigure == null) return;

			if (_shapeManager.SelectedFigure.IsVertex(e.Location, out var vertex))
			{
				vertex.Select();
				Cursor = Cursors.Hand;
				return;
			}

			Cursor = Cursors.SizeAll;
		}

		private void MouseMoveOccured(object sender, MouseEventArgs e)
		{
			var selectedFigure = _shapeManager.SelectedFigure;
			if (selectedFigure != null)
			{
				var selectedVertex = selectedFigure.SelectedVertex;
				if (selectedVertex == null) MoveFigure(e.Location);
				else MoveVertex(e.Location);

				_mouseLastPosition = e.Location;
				drawingArea.Refresh();
				return;
			}

			if (_formState == FormState.DrawFigure)
			{
				_shapeManager.CurrentFigure?.DrawTemporaryLine(e.Location);
				drawingArea.Refresh();
				return;
			}

			SetCursorImage(e.Location);
		}

		private void MouseUpOccured(object sender, MouseEventArgs e)
		{
			_mouseUpPosition = e.Location;
			_shapeManager.DeselectFigures();
			
			switch (_formState)
			{
				case FormState.AddVertex:
					_shapeManager.TryAddVertexToFigure(e.Location);
					drawingArea.Refresh();
					break;
				case FormState.DrawFigure:
					AddNewVertex();
					break;
				case FormState.ChangeColor:
					ChangeFigureColor(e.Location);
					break;
				case FormState.ChangeThickness:
					ChangeFigureThickness(e.Location);
					break;
			}
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

		private void SetCursorImage(Point location)
		{
			var isCursorOverFigure = _shapeManager.Figures.Any(figure => figure.ContainsPoint(location));

			if (!isCursorOverFigure)
			{
				Cursor = Cursors.Default;
				return;
			}

			var isCursorOverLine = _shapeManager.Figures.Any(figure => figure.GetLineContainingPoint(location) != null);

			switch (_formState)
			{
				case FormState.ChangeColor when isCursorOverLine:
					Cursor = _cursorManager.BucketCursor;
					break;
				case FormState.Multisampling when isCursorOverLine:
				case FormState.ChangeThickness when isCursorOverLine:
					Cursor = _cursorManager.PenCursor;
					break;
				case FormState.AddVertex when isCursorOverLine:
					Cursor = Cursors.Cross;
					break;
				case FormState.ChangeColor:
				case FormState.ChangeThickness:
				case FormState.AddVertex:
				case FormState.Multisampling:
					Cursor = Cursors.Default;
					break;
				case FormState.Default when _shapeManager.Figures.Any(figure => figure.IsVertex(location, out _)):
					Cursor = Cursors.Hand;
					break;
				case FormState.Default:
					Cursor = Cursors.SizeAll;
					break;
			}
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

		private void MoveVertex(Point point)
		{
			var deltaX = point.X - _mouseLastPosition.X;
			var deltaY = point.Y - _mouseLastPosition.Y;

			var selectedFigure = _shapeManager.SelectedFigure;
			var vertex = selectedFigure.SelectedVertex;
			if (!vertex.CanDrag(deltaX, deltaY, drawingArea.Bounds.Width, drawingArea.Bounds.Height)) return;

			selectedFigure.MoveSelectedVertex(deltaX, deltaY);
		}

		private void MoveFigure(Point point)
		{
			var deltaX = point.X - _mouseLastPosition.X;
			var deltaY = point.Y - _mouseLastPosition.Y;
			var selectedFigure = _shapeManager.SelectedFigure;

			if (!selectedFigure.CanDrag(deltaX, deltaY, drawingArea.Bounds.Width, drawingArea.Bounds.Height)) return;

			selectedFigure.Move(deltaX, deltaY);
		}
	}
}
