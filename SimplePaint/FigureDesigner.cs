﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SimplePaint.Shapes;

namespace SimplePaint
{
	public partial class FigureDesigner : Form
	{
		#region Extern Imports
		/// <summary>
		/// Used for importing cursor images
		/// </summary>
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		internal static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType
			, int cxDesired, int cyDesired, uint fuLoad);
		#endregion Extern Imports
		#region .ctor
		/// <summary>
		/// Initializes the window instance
		/// </summary>
		public FigureDesigner()
		{
			InitializeComponent();
		}
		#endregion .ctor
		#region Private Members

		private readonly ShapeManager _shapeManager = new ShapeManager();

		/// <summary>
		/// Is this the first vertex
		/// </summary>
		private bool _isFirstVertex;
		/// <summary>
		/// The currently drawn figure
		/// </summary>
		private CustomFigure _currentFigure;
		/// <summary>
		/// The current color
		/// </summary>
		private Color _color;
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
		/// The selected vertex
		/// </summary>
		private CustomEllipse _selectedVertex;
		/// <summary>
		/// Is the user changing thickness
		/// </summary>
		private bool _doChangeThickness;
		/// <summary>
		/// The bucket cursor
		/// </summary>
		private Cursor _bucketCursor;
		/// <summary>
		/// The dot cursor
		/// </summary>
		private Cursor _dotCursor;
		/// <summary>
		/// The pen cursor
		/// </summary>
		private Cursor _penCursor;
		/// <summary>
		/// The hand cursor
		/// </summary>
		private Cursor _handCursor;
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
		#region Event Methods
		/// <summary>
		/// Handles the Load event of the FigureDesigner control.
		/// </summary>
		/// <remarks>
		/// Sets up the layout and initializes default values
		/// </remarks>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void FigureDesigner_Load(object sender, EventArgs e)
		{
			InitializeDefaultValues();
			SetDefaultSettings();

			LoadCursorImages();
			drawFigureButton.PerformClick();
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
		/// <summary>
		/// Handles the MouseDown event of the drawingArea control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void drawingArea_MouseDown(object sender, MouseEventArgs e)
		{
			_mouseDownPosition = new Point(e.X, e.Y);
			_mouseLastPosition = _mouseDownPosition;

			if (_shapeManager.SelectedFigure != null || _doDrawFigure || _doAddVertex || _doChangeColor || _doChangeThickness)
				return;


			_shapeManager.SelectFigure(e.Location);

			if (_shapeManager.SelectedFigure != null) Cursor = Cursors.SizeAll;

			if (_doMultisample && SelectLineForMultisampling(e.Location)) return;

			_shapeManager.SelectFigure(e.Location);

			if (_shapeManager.SelectedFigure == null) return;

			Cursor = Cursors.SizeAll;
			if (_shapeManager.SelectedFigure.IsVertex(e.Location, out var vertex)) _selectedVertex = vertex;
		}
		/// <summary>
		/// Handles the MouseMove event of the drawingArea control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void drawingArea_MouseMove(object sender, MouseEventArgs e)
		{
			Cursor = Cursors.Default;

			if (_doAddVertex || _doDrawFigure)
				Cursor = _dotCursor;

			if (_shapeManager.SelectedFigure == null && !_doDrawFigure && !_doAddVertex && SetCursorImage(e.Location)) return;
			if (_selectedVertex != null && MoveVertex(e.X, e.Y, e.Location)) return;
			if (_shapeManager.SelectedFigure != null && MoveFigure(e.X, e.Y, e.Location)) return;

			// Proceed when drawing current figure is in progress. 
			// Draws a temporary line between the last vertex and current mouse position.
			if (_doDrawFigure && _currentFigure != null)
			{
				if (_currentFigure.FigureShapes.Last().GetType() == typeof(CustomLine))
					_currentFigure.FigureShapes.Remove(_currentFigure.FigureShapes.Last());
				_currentFigure.FigureShapes.AddLast(new CustomLine(_currentFigure.LastVertex.Position, new Point(e.X, e.Y)));
			}

			drawingArea.Refresh();
		}
		/// <summary>
		/// Handles the MouseUp event of the drawingArea control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void drawingArea_MouseUp(object sender, MouseEventArgs e)
		{
			_mouseUpPosition = new Point(e.X, e.Y);
			_shapeManager.DeselectFigure();
			_selectedVertex = null;

			CustomFigure figure;
			CustomLine line;

			// Add vertex to existing figure.
			if (!_doDrawFigure && _doAddVertex && _shapeManager.Figures.Any() && IsPointOnBorder(_mouseUpPosition, out figure, out line))
			{
				figure.AddInbetweenVertex(new CustomEllipse(_mouseUpPosition), line);
				drawingArea.Refresh();
				return;
			}

			if (_doDrawFigure && DrawFigure()) return;

			if (_doChangeColor && ChangeFigureColor(e.Location)) return;

			if (_doChangeThickness)
				ChangeFigureThickness(e.Location);
		}
		/// <summary>
		/// Handles the Click event of the drawFigureButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void drawFigureButton_Click(object sender, EventArgs e)
		{
			if (_doDrawFigure)
			{
				Cursor = Cursors.Default;
				SetButtonStates(addVertex: false, drawFigure: false, changeColor: false
					, changeThickness: false, doMultisampling: false);
				_currentFigure = null;
				_isFirstVertex = true;
				return;
			}

			SetButtonStates(addVertex: false, drawFigure: true, changeColor: false
				, changeThickness: false, doMultisampling: false);
			Cursor = _dotCursor;
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

			Cursor = _dotCursor;
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
			if (_doDrawFigure && _currentFigure != null && _currentFigure.VertexNumber != 0)
			{
				MessageBox.Show("You have to finish drawing to use this functionality.", "Don't do that!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (_doChangeColor && !_doDrawFigure)
			{
				SetButtonStates(addVertex: false, drawFigure: false, changeColor: false
					, changeThickness: false, doMultisampling: false);
				return;
			}

			ColorDialog colorDialog = new ColorDialog();
			colorDialog.AllowFullOpen = true;

			if (colorDialog.ShowDialog() == DialogResult.OK)
			{
				_color = colorDialog.Color;
				colorPictureBox.BackColor = _color;
				drawingArea.Refresh();

				if (!_doDrawFigure)
					SetButtonStates(addVertex: false, drawFigure: false, changeColor: true
						, changeThickness: false, doMultisampling: false);
			}
		}
		/// <summary>
		/// Handles the Click event of the changeSizeButton control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void changeSizeButton_Click(object sender, EventArgs e)
		{
			if (_doDrawFigure && _currentFigure != null && _currentFigure.VertexNumber != 0)
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
		#endregion Event Methods
		#region Private Methods
		/// <summary>
		/// Sets the default settings and clears the figures.
		/// </summary>
		private void SetDefaultSettings()
		{
			Cursor = Cursors.Default;

			SetButtonStates(addVertex: false, drawFigure: false, changeColor: false
				, changeThickness: false, doMultisampling: false);

			_isFirstVertex = true;
			_shapeManager.Figures.Clear();
			_currentFigure = null;
			drawingArea.Refresh();
		}
		/// <summary>
		/// Loads the cursor images.
		/// </summary>
		private void LoadCursorImages()
		{
			const int imageCursor = 2;
			const uint lrLoadfromfile = 0x00000010;
			try
			{
				IntPtr ipImage = LoadImage(IntPtr.Zero,
					@"d:\Studia\Semestr_5\Grafika_Komputerowa_I\Laboratoria\01-Edytor_Grafiki\WindowsFormsApplication1\WindowsFormsApplication1\Bucket.cur ",
					imageCursor, 0, 0, lrLoadfromfile);
				_bucketCursor = new Cursor(ipImage);

				ipImage = LoadImage(IntPtr.Zero,
					@"d:\Studia\Semestr_5\Grafika_Komputerowa_I\Laboratoria\01-Edytor_Grafiki\WindowsFormsApplication1\WindowsFormsApplication1\Dot.cur ",
					imageCursor, 0, 0, lrLoadfromfile);
				_dotCursor = new Cursor(ipImage);

				ipImage = LoadImage(IntPtr.Zero,
					@"d:\Studia\Semestr_5\Grafika_Komputerowa_I\Laboratoria\01-Edytor_Grafiki\WindowsFormsApplication1\WindowsFormsApplication1\Pen.cur ",
					imageCursor, 0, 0, lrLoadfromfile);
				_penCursor = new Cursor(ipImage);

				ipImage = LoadImage(IntPtr.Zero,
					@"d:\Studia\Semestr_5\Grafika_Komputerowa_I\Laboratoria\01-Edytor_Grafiki\WindowsFormsApplication1\WindowsFormsApplication1\Hand.cur ",
					imageCursor, 0, 0, lrLoadfromfile);
				_handCursor = new Cursor(ipImage);
			}
			catch (Exception)
			{
				_bucketCursor = _dotCursor = _penCursor = _handCursor = Cursors.Hand;
				Console.WriteLine("Could not load cursors.");
			}
		}

		private void InitializeDefaultValues()
		{
			_color = Color.Red;
			colorPictureBox.BackColor = _color;
			_strokeThickness = 2;

			_mouseDownPosition = Point.Empty;
			_mouseUpPosition = Point.Empty;

			_shapeManager.DeselectFigure();
			_selectedVertex = null;
			_selectedLine = null;
			_multisamplingFigure = null;
			_doMultisample = false;
		}
		/// <summary>
		/// Determines whether [is figure complete] after placing new vertex.
		/// </summary>
		/// <param name="point">Position of placed vertex.</param>
		/// <returns>True if the figure is closed, otherwise false</returns>
		private bool IsFigureComplete(Point point)
		{
			var vertex = _currentFigure.FirstVertex;

			return (Math.Abs(point.X - vertex.Position.X) < 10 && Math.Abs(point.Y - vertex.Position.Y) < 10);
		}
		/// <summary>
		/// Determines whether [is point on border].
		/// </summary>
		/// <param name="point">The point.</param>
		/// <param name="outFigure">The out figure.</param>
		/// <param name="outLine">The out line.</param>
		/// <returns>True if the hit point was on a line</returns>
		private bool IsPointOnBorder(Point point, out CustomFigure outFigure, out CustomLine outLine)
		{
			outFigure = null;
			outLine = null;

			foreach (CustomFigure figure in _shapeManager.Figures)
				foreach (CustomLine line in from shape in figure.FigureShapes
											where shape.GetType() == typeof(CustomLine)
											select shape as CustomLine)
				{
					double ab = Math.Sqrt((line.StartPoint.X - line.EndPoint.X) * (line.StartPoint.X - line.EndPoint.X)
						+ (line.StartPoint.Y - line.EndPoint.Y) * (line.StartPoint.Y - line.EndPoint.Y));
					double ac = Math.Sqrt((point.X - line.StartPoint.X) * (point.X - line.StartPoint.X)
						+ (point.Y - line.StartPoint.Y) * (point.Y - line.StartPoint.Y));
					double bc = Math.Sqrt((point.X - line.EndPoint.X) * (point.X - line.EndPoint.X)
						+ (point.Y - line.EndPoint.Y) * (point.Y - line.EndPoint.Y));

					if (!(ac + bc < ab + 2) || !(ac + bc > ab - 2)) continue;
					outFigure = figure;
					outLine = line;
					return true;
				}

			return false;
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
			CustomLine outLine;
			CustomFigure outFigure;

			foreach (CustomFigure figure in _shapeManager.Figures)
				if (IsPointOnBorder(location, out outFigure, out outLine))
				{
					if (_multisamplingFigure != null)
						_multisamplingFigure.MultisamplingLine = null;

					_multisamplingFigure = outFigure;
					_selectedLine = outLine;
					figure.MultisamplingLine = outLine;
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
			CustomFigure outFigure;
			CustomLine outLine;

			if (_doChangeColor && IsPointOnBorder(location, out outFigure, out outLine))
			{
				Cursor = _bucketCursor;
			}
			else if (_doChangeThickness && _shapeManager.Figures.Any(figure => IsPointOnBorder(location, out outFigure, out outLine)))
			{
				Cursor = _penCursor;
				return true;
			}
			else if (!_doChangeColor && !_doChangeThickness)
			{
				if (_shapeManager.Figures.Any(figure => figure.IsVertex(location, out _)))
				{
					Cursor = _handCursor;
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
			CustomFigure outFigure;
			CustomLine outLine;

			if (!IsPointOnBorder(location, out outFigure, out outLine)) return;

			outFigure.StrokeThickness = _strokeThickness;
			drawingArea.Refresh();
		}
		/// <summary>
		/// Changes the color of the chosen figure.
		/// </summary>
		/// <param name="location">The location.</param>
		/// <returns></returns>
		private bool ChangeFigureColor(Point location)
		{
			CustomFigure outFigure;
			CustomLine outLine;

			if (!IsPointOnBorder(location, out outFigure, out outLine)) return false;

			outFigure.FigureColor = _color;
			drawingArea.Refresh();
			return true;
		}
		/// <summary>
		/// Draws the figure.
		/// </summary>
		/// <returns>True if the figure is completed or can be completed but has less than 3 vertices</returns>
		private bool DrawFigure()
		{
			if (_isFirstVertex)
			{
				_isFirstVertex = false;
				_currentFigure = new CustomFigure(_mouseUpPosition, _color, _strokeThickness);
			}
			else
			{
				if (IsFigureComplete(_mouseUpPosition))
				{
					if (_currentFigure.VertexNumber < 3) return true;

					_isFirstVertex = true;
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
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <param name="location">The location.</param>
		/// <returns>True if there was a vertex to move and it was moved, false otherwise</returns>
		private bool MoveVertex(int x, int y, Point location)
		{
			int deltaX = _mouseLastPosition.X - x;
			int deltaY = _mouseLastPosition.Y - y;

			// Prevent draging the figure outside the drawing area, so that it won't loose it's vertices and edges.
			if (_selectedVertex.Position.X - deltaX > drawingArea.Bounds.Width - 10
				|| _selectedVertex.Position.Y - deltaY > drawingArea.Bounds.Height - 10
				|| _selectedVertex.Position.X - deltaX < 5
				|| _selectedVertex.Position.Y - deltaY < 5)
				return true;

			Cursor = _handCursor;

			foreach (CustomEllipse ellipse in
				from shape in _shapeManager.SelectedFigure.FigureShapes
				where shape.GetType() == typeof(CustomEllipse)
				select shape as CustomEllipse)
			{
				if (ellipse.Position != _selectedVertex.Position) continue;

				var node = _shapeManager.SelectedFigure.FigureShapes.Find(ellipse);

				SetLinePosition(node, deltaX, deltaY);
				ellipse.Position = new Point(ellipse.Position.X - deltaX, ellipse.Position.Y - deltaY);
				_shapeManager.SelectedFigure.FigureVertices.Find(_selectedVertex).Value.Position =
					new Point(_selectedVertex.Position.X - deltaX, _selectedVertex.Position.Y - deltaY);

				drawingArea.Refresh();
				SetFigureBounds();

				_mouseLastPosition = location;
				return true;
			}
			return false;
		}
		/// <summary>
		/// Sets the line position.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="deltaX">The delta x.</param>
		/// <param name="deltaY">The delta y.</param>
		private void SetLinePosition(LinkedListNode<IShape> node, int deltaX, int deltaY)
		{
			try
			{
				CustomLine line = node.Previous.Value as CustomLine;
				line.EndPoint = new Point(line.EndPoint.X - deltaX, line.EndPoint.Y - deltaY);
			}
			catch (Exception)
			{
				CustomLine line = _shapeManager.SelectedFigure.FigureShapes.Last.Value as CustomLine;
				line.EndPoint = new Point(line.EndPoint.X - deltaX, line.EndPoint.Y - deltaY);
			}

			try
			{
				CustomLine line = node.Next.Value as CustomLine;
				line.StartPoint = new Point(line.StartPoint.X - deltaX, line.StartPoint.Y - deltaY);
			}
			catch (Exception)
			{
				CustomLine line = _shapeManager.SelectedFigure.FigureShapes.First.Value as CustomLine;
				line.StartPoint = new Point(line.StartPoint.X - deltaX, line.StartPoint.Y - deltaY);
			}
		}
		/// <summary>
		/// Sets the figure's bounds.
		/// </summary>
		private void SetFigureBounds()
		{
			_shapeManager.SelectedFigure.MaxX = int.MinValue;
			_shapeManager.SelectedFigure.MaxY = int.MinValue;
			_shapeManager.SelectedFigure.MinX = int.MaxValue;
			_shapeManager.SelectedFigure.MinY = int.MaxValue;

			foreach (var vertex in _shapeManager.SelectedFigure.FigureVertices)
			{
				if (vertex.Position.X < _shapeManager.SelectedFigure.MinX) _shapeManager.SelectedFigure.MinX = vertex.Position.X;
				if (vertex.Position.Y < _shapeManager.SelectedFigure.MinY) _shapeManager.SelectedFigure.MinY = vertex.Position.Y;
				if (vertex.Position.X > _shapeManager.SelectedFigure.MaxX) _shapeManager.SelectedFigure.MaxX = vertex.Position.X;
				if (vertex.Position.Y > _shapeManager.SelectedFigure.MaxY) _shapeManager.SelectedFigure.MaxY = vertex.Position.Y;
			}
		}
		/// <summary>
		/// Move the figure between the bounds of the drawingArea.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <param name="location">The location.</param>
		/// <returns>True if the figure was moved</returns>
		private bool MoveFigure(int x, int y, Point location)
		{
			int deltaX = _mouseLastPosition.X - x;
			int deltaY = _mouseLastPosition.Y - y;

			// Prevent draging the figure outside the drawing area, so that it won't loose it's vertices and edges.
			if (_shapeManager.SelectedFigure.MaxX - deltaX > drawingArea.Bounds.Width - 10
				|| _shapeManager.SelectedFigure.MaxY - deltaY > drawingArea.Bounds.Height - 10
				|| _shapeManager.SelectedFigure.MinX - deltaX < 5
				|| _shapeManager.SelectedFigure.MinY - deltaY < 5)
				return true;

			foreach (IShape shape in _shapeManager.SelectedFigure.FigureShapes)
				if (shape.GetType() == typeof(CustomEllipse))
				{
					CustomEllipse ellipse = shape as CustomEllipse;
					ellipse.Position = new Point(ellipse.Position.X - deltaX, ellipse.Position.Y - deltaY);
				}
				else
				{
					CustomLine line = shape as CustomLine;
					line.StartPoint = new Point(line.StartPoint.X - deltaX, line.StartPoint.Y - deltaY);
					line.EndPoint = new Point(line.EndPoint.X - deltaX, line.EndPoint.Y - deltaY);
				}

			foreach (CustomEllipse vertex in _shapeManager.SelectedFigure.FigureVertices)
				vertex.Position = new Point(vertex.Position.X - deltaX, vertex.Position.Y - deltaY);

			_shapeManager.SelectedFigure.MaxX -= deltaX;
			_shapeManager.SelectedFigure.MinX -= deltaX;
			_shapeManager.SelectedFigure.MaxY -= deltaY;
			_shapeManager.SelectedFigure.MinY -= deltaY;

			_mouseLastPosition = location;
			Cursor = Cursors.SizeAll;
			return false;
		}
		#endregion Private Methods
	}
}
