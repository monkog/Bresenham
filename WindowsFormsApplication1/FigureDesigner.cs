using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WindowsFormsApplication1.Helpers;
using WindowsFormsApplication1.Shapes;

namespace WindowsFormsApplication1
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
        /// List of all figures
        /// </summary>
        private List<CustomFigure> _figures;
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
        /// The selected figure
        /// </summary>
        private CustomFigure _selectedFigure;
        /// <summary>
        /// Is the user changing color
        /// </summary>
        private bool _doChangeColor;
        /// <summary>
        /// The selected vertex
        /// </summary>
        private CustomVertex _selectedVertex;
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
            // Window state.
            WindowState = FormWindowState.Maximized;
            BackColor = Color.SaddleBrown;

            InitializeDefaultValues();
            SetupLayout();
            SetDefaultSettings();

            // Functionality.
            drawingArea.Paint += drawingArea_Paint;

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
            if (_figures.Count != 0)
                foreach (CustomFigure figure in _figures)
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

            if (_selectedFigure != null || _doDrawFigure || _doAddVertex || _doChangeColor || _doChangeThickness)
                return;


            if (SelectFigureToMove(e.Location)) return;

            if (_doMultisample && SelectLineForMultisampling(e.Location)) return;

            foreach (CustomFigure figure in _figures.Where(figure => CustomFigure.IsPointInFigure(e.Location, figure)))
            {
                _selectedFigure = figure;
                Cursor = Cursors.SizeAll;
                return;
            }
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

            if (_selectedFigure == null && !_doDrawFigure && !_doAddVertex && SetCursorImage(e.Location)) return;
            if (_selectedVertex != null && MoveVertex(e.X, e.Y, e.Location)) return;
            if (_selectedFigure != null && MoveFigure(e.X, e.Y, e.Location)) return;

            // Proceed when drawing current figure is in progress. 
            // Draws a temporary line between the last vertex and current mouse position.
            if (_doDrawFigure && _currentFigure != null)
            {
                if (_currentFigure.FigureShapes.Last().GetType() == typeof(CustomLine))
                    _currentFigure.FigureShapes.Remove(_currentFigure.FigureShapes.Last());
                _currentFigure.FigureShapes.AddLast(new CustomLine(_currentFigure.LastVertex.Point, new Point(e.X, e.Y)));
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
            _selectedFigure = null;
            _selectedVertex = null;

            CustomFigure figure;
            CustomLine line;

            // Add vertex to existing figure.
            if (!_doDrawFigure && _doAddVertex && _figures.Count != 0 && IsPointOnBorder(_mouseUpPosition, out figure, out line))
            {
                figure.AddInbetweenVertex(new CustomVertex(_mouseUpPosition), line);
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
                SetButtonStates(addVertex: false, drawFigure: false, changeColor: false, changeThickness: false);
                _currentFigure = null;
                _isFirstVertex = true;
                return;
            }

            SetButtonStates(addVertex: false, drawFigure: true, changeColor: false, changeThickness: false);
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
                SetButtonStates(addVertex: false, drawFigure: false, changeColor: false, changeThickness: false);
                return;
            }

            if (_figures.Count == 0 || (_doDrawFigure && _currentFigure != null))
            {
                MessageBox.Show("You have to finish drawing at least one figure to use this functionality.", "Don't do that!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetButtonStates(addVertex: true, drawFigure: false, changeColor: false, changeThickness: false);

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
                SetButtonStates(addVertex: false, drawFigure: false, changeColor: false, changeThickness: false);
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
                    SetButtonStates(addVertex: false, drawFigure: false, changeColor: true, changeThickness: false);
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
                SetButtonStates(addVertex: false, changeColor: false, changeThickness: false, drawFigure: false);
                return;
            }

            string result = PromptWindow.ShowDialog("Determine the thickness of line. For best results use value lower than 10 px.", "Choose line thickness");
            int size;

            if (!int.TryParse(result, out size) || size < 0 || size > 10)
            {
                MessageBox.Show("Invalid argument!", "Don't do that!", MessageBoxButtons.OK);
                return;
            }

            if (!_doDrawFigure)
                SetButtonStates(addVertex: false, drawFigure: false, changeColor: false, changeThickness: true);

            _strokeThickness = size;
            sizeLabel.Text = "CURRENT SIZE: " + size + " px";
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
            _doMultisample = true;
        }
        #endregion Event Methods
        #region Private Methods
        /// <summary>
        /// Sets the default settings and clears the figures.
        /// </summary>
        private void SetDefaultSettings()
        {
            Cursor = Cursors.Default;

            SetButtonStates(addVertex: false, drawFigure: false, changeColor: false, changeThickness: false);

            _isFirstVertex = true;
            _figures.Clear();
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
        /// <summary>
        /// Setups the layout.
        /// </summary>
        private void SetupLayout()
        {
            buttonsPanel.BackColor = Color.Bisque;
            buttonsPanel.BorderStyle = BorderStyle.Fixed3D;

            drawingArea.BackColor = Color.Bisque;
            drawingArea.BorderStyle = BorderStyle.Fixed3D;

            colorLabel.BackColor = Color.Bisque;
            sizeLabel.BackColor = Color.Bisque;

            colorPictureBox.BackColor = _color;
            colorPictureBox.BorderStyle = BorderStyle.Fixed3D;
        }
        /// <summary>
        /// Initializes the default values for local variables.
        /// </summary>
        private void InitializeDefaultValues()
        {
            _color = Color.Red;
            _strokeThickness = 2;
            _figures = new List<CustomFigure>();

            _mouseDownPosition = Point.Empty;
            _mouseUpPosition = Point.Empty;

            _selectedFigure = null;
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
            CustomVertex vertex = _currentFigure.FirstVertex;

            return (Math.Abs(point.X - vertex.Point.X) < 10 && Math.Abs(point.Y - vertex.Point.Y) < 10);
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

            foreach (CustomFigure figure in _figures)
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
        private void SetButtonStates(bool addVertex, bool drawFigure, bool changeColor, bool changeThickness)
        {
            _doAddVertex = addVertex;
            _doDrawFigure = drawFigure;
            _doChangeColor = changeColor;
            _doChangeThickness = changeThickness;
            changeColorButton.Text = changeColor ? "CANCEL" : "CHANGE COLOR";
            addVertexButton.Text = addVertex ? "CANCEL" : "ADD VERTEX";
            drawFigureButton.Text = drawFigure ? "CANCEL" : "DRAW FIGURE";
            changeSizeButton.Text = changeThickness ? "CANCEL" : "CHANGE SIZE";
        }
        /// <summary>
        /// Selects the figure to move around the drawingArea.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>True if such a figure exists, false otherwise</returns>
        private bool SelectFigureToMove(Point location)
        {
            CustomVertex vertex;
            foreach (CustomFigure figure in _figures)
                if (CustomFigure.IsVertex(location, figure, out vertex))
                {
                    _selectedFigure = figure;
                    _selectedVertex = vertex;
                    return true;
                }
            return false;
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

            foreach (CustomFigure figure in _figures)
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
            else if (_doChangeThickness && _figures.Any(figure => IsPointOnBorder(location, out outFigure, out outLine)))
            {
                Cursor = _penCursor;
                return true;
            }
            else if (!_doChangeColor && !_doChangeThickness)
            {
                CustomVertex outVertex;
                if (_figures.Any(figure => CustomFigure.IsVertex(location, figure, out outVertex)))
                {
                    Cursor = _handCursor;
                    return true;
                }

                if (_figures.Any(figure => CustomFigure.IsPointInFigure(location, figure)))
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
                    _figures.Add(_currentFigure);
                    _currentFigure.FigureShapes.Remove(_currentFigure.FigureShapes.Last());
                    _currentFigure.FigureShapes.AddLast(new CustomLine(_currentFigure.LastVertex.Point,
                        _currentFigure.FirstVertex.Point));
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
            if (_selectedVertex.Point.X - deltaX > drawingArea.Bounds.Width - 10
                || _selectedVertex.Point.Y - deltaY > drawingArea.Bounds.Height - 10
                || _selectedVertex.Point.X - deltaX < 5
                || _selectedVertex.Point.Y - deltaY < 5)
                return true;

            Cursor = _handCursor;

            foreach (CustomEllipse ellipse in
                from shape in _selectedFigure.FigureShapes
                where shape.GetType() == typeof(CustomEllipse)
                select shape as CustomEllipse)
            {
                if (ellipse.Position != _selectedVertex.Point) continue;

                var node = _selectedFigure.FigureShapes.Find(ellipse);

                SetLinePosition(node, deltaX, deltaY);
                ellipse.Position = new Point(ellipse.Position.X - deltaX, ellipse.Position.Y - deltaY);
                _selectedFigure.FigureVertices.Find(_selectedVertex).Value.Point =
                    new Point(_selectedVertex.Point.X - deltaX, _selectedVertex.Point.Y - deltaY);

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
                CustomLine line = _selectedFigure.FigureShapes.Last.Value as CustomLine;
                line.EndPoint = new Point(line.EndPoint.X - deltaX, line.EndPoint.Y - deltaY);
            }

            try
            {
                CustomLine line = node.Next.Value as CustomLine;
                line.StartPoint = new Point(line.StartPoint.X - deltaX, line.StartPoint.Y - deltaY);
            }
            catch (Exception)
            {
                CustomLine line = _selectedFigure.FigureShapes.First.Value as CustomLine;
                line.StartPoint = new Point(line.StartPoint.X - deltaX, line.StartPoint.Y - deltaY);
            }
        }
        /// <summary>
        /// Sets the figure's bounds.
        /// </summary>
        private void SetFigureBounds()
        {
            _selectedFigure.MaxX = int.MinValue;
            _selectedFigure.MaxY = int.MinValue;
            _selectedFigure.MinX = int.MaxValue;
            _selectedFigure.MinY = int.MaxValue;

            foreach (var vertex in _selectedFigure.FigureVertices)
            {
                if (vertex.Point.X < _selectedFigure.MinX) _selectedFigure.MinX = vertex.Point.X;
                if (vertex.Point.Y < _selectedFigure.MinY) _selectedFigure.MinY = vertex.Point.Y;
                if (vertex.Point.X > _selectedFigure.MaxX) _selectedFigure.MaxX = vertex.Point.X;
                if (vertex.Point.Y > _selectedFigure.MaxY) _selectedFigure.MaxY = vertex.Point.Y;
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
            if (_selectedFigure.MaxX - deltaX > drawingArea.Bounds.Width - 10
                || _selectedFigure.MaxY - deltaY > drawingArea.Bounds.Height - 10
                || _selectedFigure.MinX - deltaX < 5
                || _selectedFigure.MinY - deltaY < 5)
                return true;

            foreach (IShape shape in _selectedFigure.FigureShapes)
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

            foreach (CustomVertex vertex in _selectedFigure.FigureVertices)
                vertex.Point = new Point(vertex.Point.X - deltaX, vertex.Point.Y - deltaY);

            _selectedFigure.MaxX -= deltaX;
            _selectedFigure.MinX -= deltaX;
            _selectedFigure.MaxY -= deltaY;
            _selectedFigure.MinY -= deltaY;

            _mouseLastPosition = location;
            Cursor = Cursors.SizeAll;
            return false;
        }
        #endregion Private Methods
    }
}
