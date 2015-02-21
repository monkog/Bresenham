using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WindowsFormsApplication1.Helpers;
using WindowsFormsApplication1.Shapes;

namespace WindowsFormsApplication1
{
    public partial class FigureDesigner : Form
    {
        #region .ctor
        /// <summary>
        /// Initializes the window instance
        /// </summary>
        public FigureDesigner()
        {
            InitializeComponent();
        }
        #endregion .ctor
        #region variables
        private bool _isFirstVertex;
        private CustomFigure _currentFigure;
        private Color _color;
        private int _strokeThickness;
        private bool _doAddVertex;
        private List<CustomFigure> _figures;
        private Point _mouseDownPosition;
        private Point _mouseUpPosition;
        private Point _mouseLastPosition;
        private bool _doDrawFigure;
        private CustomFigure _selectedFigure;
        private bool _doChangeColor;
        private CustomVertex _selectedVertex;
        private bool _doChangeThickness;
        private Cursor _bucketCursor;
        private Cursor _dotCursor;
        private Cursor _penCursor;
        private Cursor _handCursor;
        private CustomLine _selectedLine;
        private CustomFigure _multisamplingFigure;
        private bool _doMultisample;
        #endregion

        /// <summary>
        /// Used for importing cursor images
        /// </summary>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);
        
        #region event methods
        private void FigureDesigner_Load(object sender, EventArgs e)
        {
            // Window state.
            WindowState = FormWindowState.Maximized;
            BackColor = Color.SaddleBrown;

            // Local variables initialisation.
            _isFirstVertex = true;
            _color = Color.Red;
            _strokeThickness = 2;
            _doAddVertex = false;
            _doDrawFigure = false;
            _figures = new List<CustomFigure>();
            _mouseDownPosition = Point.Empty;
            _mouseUpPosition = Point.Empty;
            _doDrawFigure = false;
            _selectedFigure = null;
            _doChangeColor = false;
            _selectedVertex = null;
            _doChangeThickness = false;
            _selectedLine = null;
            _multisamplingFigure = null;
            _doMultisample = false;

            // Layout.
            buttonsPanel.BackColor = Color.Bisque;
            buttonsPanel.BorderStyle = BorderStyle.Fixed3D;

            drawingArea.BackColor = Color.Bisque;
            drawingArea.BorderStyle = BorderStyle.Fixed3D;

            colorLabel.BackColor = Color.Bisque;
            sizeLabel.BackColor = Color.Bisque;

            colorPictureBox.BackColor = _color;
            colorPictureBox.BorderStyle = BorderStyle.Fixed3D;

            // Functionality.
            drawingArea.Paint += drawingArea_Paint;

            // Cursors.
            const int imageCursor = 2;
            const uint lrLoadfromfile = 0x00000010;
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
            _bucketCursor = _dotCursor = _penCursor = _handCursor = Cursors.Hand;

            drawFigureButton.PerformClick();
        }

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
                                if (line == _selectedLine)
                                    shape.Draw(e.Graphics, figure.MultisamplingColor, figure.StrokeThickness);
                                else
                                    shape.Draw(e.Graphics, figure.FigureColor, figure.StrokeThickness);
                            }
                            else
                                shape.Draw(e.Graphics, figure.FigureColor, figure.StrokeThickness);
                    }
                    else
                        foreach (IShape shape in figure.FigureShapes)
                            shape.Draw(e.Graphics, figure.FigureColor, figure.StrokeThickness);
                }
        }

        private void drawingArea_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseDownPosition = new Point(e.X - 5, e.Y - 5);
            _mouseLastPosition = _mouseDownPosition;

            // Selects the figure to move around the drawingArea.
            if (_selectedFigure == null && !_doDrawFigure && !_doAddVertex && !_doChangeColor && !_doChangeThickness)
            {
                CustomVertex outVertex;
                CustomLine outLine;
                CustomFigure outFigure;

                foreach (CustomFigure figure in _figures)
                    if (IsPointVertice(e.Location, figure, out outVertex))
                    {
                        _selectedFigure = figure;
                        _selectedVertex = outVertex;
                        return;
                    }

                if (_doMultisample)
                    foreach (CustomFigure figure in _figures)
                        if (IsPointOnBorder(e.Location, out outFigure, out outLine))
                        {
                            if (_multisamplingFigure != null)
                                _multisamplingFigure.MultisamplingLine = null;

                            _multisamplingFigure = outFigure;
                            _selectedLine = outLine;
                            figure.MultisamplingLine = outLine;
                            drawingArea.Refresh();
                            return;
                        }

                foreach (CustomFigure figure in _figures)
                    if (IsPointInFigure(e.Location, figure))
                    {
                        _selectedFigure = figure;
                        Cursor = Cursors.SizeAll;
                        return;
                    }
            }
        }

        private void drawingArea_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Default;

            CustomFigure outFigure;
            CustomLine outLine;
            CustomVertex outVertex;

            if (_doAddVertex || _doDrawFigure)
                Cursor = _dotCursor;

            // Changes the cursor if the drawing mode is off and mouse is above any of the previously drawn shapes.
            if (_selectedFigure == null && !_doDrawFigure && !_doAddVertex)
                if (_doChangeColor && IsPointOnBorder(e.Location, out outFigure, out outLine))
                {
                    Cursor = _bucketCursor;
                }
                else if (_doChangeThickness)
                {
                    foreach (CustomFigure figure in _figures)
                        if (IsPointOnBorder(e.Location, out outFigure, out outLine))
                        {
                            Cursor = _penCursor;
                            return;
                        }
                }
                else if (!_doChangeColor && !_doChangeThickness)
                {
                    foreach (CustomFigure figure in _figures)
                        if (IsPointVertice(e.Location, figure, out outVertex))
                        {
                            Cursor = _handCursor;
                            return;
                        }

                    foreach (CustomFigure figure in _figures)
                        if (IsPointInFigure(e.Location, figure))
                        {
                            Cursor = Cursors.SizeAll;
                            return;
                        }
                }

            // Move the vertex between the bounds of drawingArea.
            if (_selectedVertex != null)
            {
                int deltaX = _mouseLastPosition.X - e.X;
                int deltaY = _mouseLastPosition.Y - e.Y;

                // Prevent draging the figure outside the drawing area, so that it won't loose it's vertices and edges.
                if (_selectedVertex.Point.X - deltaX > drawingArea.Bounds.Width - 10
                    || _selectedVertex.Point.Y - deltaY > drawingArea.Bounds.Height - 10
                    || _selectedVertex.Point.X - deltaX < 5
                    || _selectedVertex.Point.Y - deltaY < 5)
                    return;

                Cursor = _handCursor;
                LinkedListNode<IShape> node;

                foreach (IShape shape in _selectedFigure.FigureShapes)
                    if (shape.GetType() == typeof(CustomEllipse))
                    {
                        CustomEllipse ellipse = shape as CustomEllipse;
                        if (ellipse.Point == _selectedVertex.Point)
                        {
                            node = _selectedFigure.FigureShapes.Find(shape);

                            try
                            {
                                CustomLine line = node.Previous.Value as CustomLine;
                                line.MPoint2 = new Point(line.MPoint2.X - deltaX, line.MPoint2.Y - deltaY);
                            }
                            catch (Exception)
                            {
                                CustomLine line = _selectedFigure.FigureShapes.Last.Value as CustomLine;
                                line.MPoint2 = new Point(line.MPoint2.X - deltaX, line.MPoint2.Y - deltaY);
                            }

                            try
                            {
                                CustomLine line = node.Next.Value as CustomLine;
                                line.MPoint1 = new Point(line.MPoint1.X - deltaX, line.MPoint1.Y - deltaY);
                            }
                            catch (Exception)
                            {
                                CustomLine line = _selectedFigure.FigureShapes.First.Value as CustomLine;
                                line.MPoint1 = new Point(line.MPoint1.X - deltaX, line.MPoint1.Y - deltaY);
                            }

                            ellipse.Point = new Point(ellipse.Point.X - deltaX, ellipse.Point.Y - deltaY);

                            _selectedFigure.FigureVertices.Find(_selectedVertex).Value.Point = new Point(_selectedVertex.Point.X - deltaX, _selectedVertex.Point.Y - deltaY);

                            drawingArea.Refresh();

                            _selectedFigure.MaxX = int.MinValue;
                            _selectedFigure.MaxY = int.MinValue;
                            _selectedFigure.MinX = int.MaxValue;
                            _selectedFigure.MinY = int.MaxValue;

                            foreach (CustomVertex vertex in _selectedFigure.FigureVertices)
                            {
                                if (vertex.Point.X < _selectedFigure.MinX) _selectedFigure.MinX = vertex.Point.X;
                                if (vertex.Point.Y < _selectedFigure.MinY) _selectedFigure.MinY = vertex.Point.Y;
                                if (vertex.Point.X > _selectedFigure.MaxX) _selectedFigure.MaxX = vertex.Point.X;
                                if (vertex.Point.Y > _selectedFigure.MaxY) _selectedFigure.MaxY = vertex.Point.Y;
                            }

                            _mouseLastPosition = e.Location;
                            return;
                        }
                    }
            }

            // Move the figure between the bounds of the drawingArea.
            if (_selectedFigure != null)
            {
                int deltaX = _mouseLastPosition.X - e.X;
                int deltaY = _mouseLastPosition.Y - e.Y;

                // Prevent draging the figure outside the drawing area, so that it won't loose it's vertices and edges.
                if (_selectedFigure.MaxX - deltaX > drawingArea.Bounds.Width - 10
                    || _selectedFigure.MaxY - deltaY > drawingArea.Bounds.Height - 10
                    || _selectedFigure.MinX - deltaX < 5
                    || _selectedFigure.MinY - deltaY < 5)
                    return;

                Cursor = Cursors.SizeAll;

                foreach (IShape shape in _selectedFigure.FigureShapes)
                    if (shape.GetType() == typeof(CustomEllipse))
                    {
                        CustomEllipse ellipse = shape as CustomEllipse;
                        ellipse.Point = new Point(ellipse.Point.X - deltaX, ellipse.Point.Y - deltaY);
                    }
                    else
                    {
                        CustomLine line = shape as CustomLine;
                        line.MPoint1 = new Point(line.MPoint1.X - deltaX, line.MPoint1.Y - deltaY);
                        line.MPoint2 = new Point(line.MPoint2.X - deltaX, line.MPoint2.Y - deltaY);
                    }

                foreach (CustomVertex vertex in _selectedFigure.FigureVertices)
                    vertex.Point = new Point(vertex.Point.X - deltaX, vertex.Point.Y - deltaY);

                _selectedFigure.MaxX -= deltaX;
                _selectedFigure.MinX -= deltaX;
                _selectedFigure.MaxY -= deltaY;
                _selectedFigure.MinY -= deltaY;

                _mouseLastPosition = e.Location;
            }

            // Proceed when drawing current figure is in progress. 
            // Draws a temporary ine between the last vertex and current mouse position.
            if (_doDrawFigure && _currentFigure != null)
            {
                if (_currentFigure.FigureShapes.Last<IShape>().GetType() == typeof(CustomLine))
                    _currentFigure.FigureShapes.Remove(_currentFigure.FigureShapes.Last<IShape>());
                _currentFigure.FigureShapes.AddLast(new CustomLine(_currentFigure.GetLastVertex().Point, new Point(e.X, e.Y)));
            }

            drawingArea.Refresh();
        }

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

            // Draw figure.
            if (_doDrawFigure)
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
                        if (_currentFigure.VertexNumber < 3) return;
                        _isFirstVertex = true;
                        _figures.Add(_currentFigure);
                        _currentFigure.FigureShapes.Remove(_currentFigure.FigureShapes.Last<IShape>());
                        _currentFigure.FigureShapes.AddLast(new CustomLine(_currentFigure.GetLastVertex().Point, _currentFigure.GetFirstVertex().Point));
                        drawingArea.Refresh();
                        _currentFigure = null;
                        return;
                    }
                    _currentFigure.AddVertex(_mouseUpPosition);
                }
                drawingArea.Refresh();
            }

            // Changes the color of the chosen figure.
            if (_doChangeColor)
            {
                CustomFigure outFigure;
                CustomLine outLine;

                if (IsPointOnBorder(e.Location, out outFigure, out outLine))
                {
                    outFigure.FigureColor = _color;
                    drawingArea.Refresh();
                    return;
                }
            }

            // Changes the line thickness of the chosen figure.
            if (_doChangeThickness)
            {
                CustomFigure outFigure;
                CustomLine outLine;

                if (IsPointOnBorder(e.Location, out outFigure, out outLine))
                {
                    outFigure.StrokeThickness = _strokeThickness;
                    drawingArea.Refresh();
                    return;
                }
            }
        }

        private void drawFigureButton_Click(object sender, EventArgs e)
        {
            if (_doDrawFigure)
                if (drawFigureButton.Text != "CANCEL")
                {
                    MessageBox.Show("You have to finish drawing to use this functionality.", "Don't do that!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    Cursor = Cursors.Default;
                    drawFigureButton.Text = "DRAW FIGURE";
                    _doDrawFigure = false;
                    return;
                }
            else
            {
                drawFigureButton.Text = "CANCEL";
                _doDrawFigure = true;
            }
            _doAddVertex = false;
            _doChangeColor = false;
            _doChangeThickness = false;

            changeColorButton.Text = "CHANGE COLOR";
            addVertexButton.Text = "ADD VERTEX";
            changeSizeButton.Text = "CHANGE SIZE";
            Cursor = _dotCursor;
        }

        private void addVertexButton_Click(object sender, EventArgs e)
        {
            if (_doAddVertex)
            {
                Cursor = Cursors.Default;
                _doAddVertex = false;
                addVertexButton.Text = "ADD VERTEX";
                return;
            }

            if (_doDrawFigure && _currentFigure != null)
            {
                MessageBox.Show("You have to finish drawing to use this functionality.", "Don't do that!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_figures.Count == 0)
            {
                MessageBox.Show("You have to draw at least one figure to use this functionality.", "Don't do that!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _doAddVertex = true;
            _doDrawFigure = false;
            _doChangeColor = false;
            _doChangeThickness = false;
            changeColorButton.Text = "CHANGE COLOR";
            addVertexButton.Text = "CANCEL";
            drawFigureButton.Text = "DRAW FIGURE";
            changeSizeButton.Text = "CHANGE SIZE";

            Cursor = _dotCursor;
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;

            changeColorButton.Text = "CHANGE COLOR";
            _doChangeColor = false;
            addVertexButton.Text = "ADD VERTEX";
            _doAddVertex = false;
            drawFigureButton.Text = "DRAW FIGURE";
            _doDrawFigure = false;
            changeSizeButton.Text = "CHANGE SIZE";
            _doChangeThickness = false;

            _isFirstVertex = true;
            _figures.Clear();
            _currentFigure = null;
            drawingArea.Refresh();
        }

        private void changeColorButton_Click(object sender, EventArgs e)
        {
            if (_doDrawFigure && _currentFigure != null && _currentFigure.VertexNumber != 0)
            {
                MessageBox.Show("You have to finish drawing to use this functionality.", "Don't do that!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_doChangeColor && !_doDrawFigure)
            {
                changeColorButton.Text = "CHANGE COLOR";
                _doChangeColor = false;
                return;
            }

            addVertexButton.Text = "ADD VERTEX";
            _doAddVertex = false;
            changeSizeButton.Text = "CHANGE SIZE";
            _doChangeThickness = false;

            ColorDialog colorDialog = new ColorDialog();
            colorDialog.AllowFullOpen = true;

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                _color = colorDialog.Color;
                colorPictureBox.BackColor = _color;
                drawingArea.Refresh();

                if (!_doDrawFigure)
                {
                    _doChangeColor = true;
                    changeColorButton.Text = "CANCEL";
                }
            }

            colorDialog.Dispose();
        }

        private void changeSizeButton_Click(object sender, EventArgs e)
        {
            if (_doDrawFigure && _currentFigure != null && _currentFigure.VertexNumber != 0)
            {
                MessageBox.Show("You have to finish drawing to use this functionality.", "Don't do that!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_doDrawFigure)
            {
                changeColorButton.Text = "CHANGE COLOR";
                _doChangeColor = false;
                addVertexButton.Text = "ADD VERTEX";
                _doAddVertex = false;
                drawFigureButton.Text = "DRAW FIGURE";
                _doDrawFigure = false;
            }

            if (_doChangeThickness)
            {
                _doChangeThickness = false;
                changeSizeButton.Text = "CHANGE SIZE";
                return;
            }

            string result = PromptWindow.ShowDialog("Determine the thickness of line. For best results use value lower than 10 px.", "Choose line thickness");
            int size = int.MaxValue;

            if (!int.TryParse(result, out size) || size < 0 || size > 10)
            {
                MessageBox.Show("Invalid argument!", "Don't do that!", MessageBoxButtons.OK);
                return;
            }

            if (!_doDrawFigure)
            {
                changeSizeButton.Text = "CANCEL";
                _doChangeThickness = true; ;
            }
            _strokeThickness = size;
            sizeLabel.Text = "CURRENT SIZE: " + size.ToString() + " px";
        }
        #endregion

        #region methods
        private bool IsFigureComplete(Point point)
        {
            CustomVertex vertex = _currentFigure.GetFirstVertex();

            return (Math.Abs(point.X - vertex.Point.X) < 10 && Math.Abs(point.Y - vertex.Point.Y) < 10);
        }

        private bool IsPointOnBorder(Point point, out CustomFigure outFigure, out CustomLine outLine)
        {
            outFigure = null;
            outLine = null;

            foreach (CustomFigure figure in _figures)
                foreach (IShape shape in figure.FigureShapes)
                    if (shape.GetType() == typeof(CustomLine))
                    {
                        CustomLine line = shape as CustomLine;

                        double ab = Math.Sqrt((line.MPoint1.X - line.MPoint2.X) * (line.MPoint1.X - line.MPoint2.X) + (line.MPoint1.Y - line.MPoint2.Y) * (line.MPoint1.Y - line.MPoint2.Y));
                        double ac = Math.Sqrt((point.X - line.MPoint1.X) * (point.X - line.MPoint1.X) + (point.Y - line.MPoint1.Y) * (point.Y - line.MPoint1.Y));
                        double bc = Math.Sqrt((point.X - line.MPoint2.X) * (point.X - line.MPoint2.X) + (point.Y - line.MPoint2.Y) * (point.Y - line.MPoint2.Y));

                        if (ac + bc < ab + 2 && ac + bc > ab - 2)
                        {
                            outFigure = figure;
                            outLine = line;
                            return true;
                        }
                    }

            return false;
        }

        private bool IsPointInFigure(Point point, CustomFigure figure)
        {
            if (point.X < figure.MinX - 5 || point.X > figure.MaxX + 5 || point.Y < figure.MinY - 5 || point.Y > figure.MaxY + 5)
                return false;

            LinkedListNode<CustomVertex> currentFirstVertex = figure.GetFirstNode();
            LinkedListNode<CustomVertex> currentLastVertex = figure.GetLastNode();

            bool isInFigure = false;

            for (int i = 0, j = figure.VertexNumber - 1; i < figure.VertexNumber; j = i++)
            {
                int iX = currentFirstVertex.Value.Point.X;
                int iY = currentFirstVertex.Value.Point.Y;
                int jX = currentLastVertex.Value.Point.X;
                int jY = currentLastVertex.Value.Point.Y;

                if ((iY > point.Y) != (jY > point.Y)
                    && (point.X < (jX - iX) * (point.Y - iY) / (jY - iY) + iX))
                    isInFigure = !isInFigure;
                currentLastVertex = currentFirstVertex;
                currentFirstVertex = currentFirstVertex.Next;
            }
            return isInFigure;
        }

        private bool IsPointVertice(Point point, CustomFigure figure, out CustomVertex outVertex)
        {
            outVertex = null;

            if (point.X < figure.MinX - 10 || point.X > figure.MaxX + 10 || point.Y < figure.MinY - 10 || point.Y > figure.MaxY + 10)
                return false;

            foreach (CustomVertex vertex in figure.FigureVertices)
                if (point.X < vertex.Point.X + 10 && point.X > vertex.Point.X - 10
                    && point.Y < vertex.Point.Y + 10 && point.Y > vertex.Point.Y - 10)
                {
                    outVertex = vertex;
                    return true;
                }

            return false;
        }

        private void multisamplingButton_Click(object sender, EventArgs e)
        {
            _doMultisample = true;
        }
        #endregion
    }
}
