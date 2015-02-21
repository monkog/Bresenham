using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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
                foreach (CustomShape shape in _currentFigure.MFigureShapes)
                    shape.Draw(e.Graphics, _currentFigure.MFigureColor, _currentFigure.MStrokeThickness);
            if (_figures.Count != 0)
                foreach (CustomFigure figure in _figures)
                {
                    if (_doMultisample && _multisamplingFigure == figure && _selectedLine != null)
                    {
                        foreach (CustomShape shape in figure.MFigureShapes)
                            if (shape.GetType() == typeof(CustomLine))
                            {
                                CustomLine line = shape as CustomLine;
                                if (line == _selectedLine)
                                    shape.Draw(e.Graphics, figure.MMultisamplingColor, figure.MStrokeThickness);
                                else
                                    shape.Draw(e.Graphics, figure.MFigureColor, figure.MStrokeThickness);
                            }
                            else
                                shape.Draw(e.Graphics, figure.MFigureColor, figure.MStrokeThickness);
                    }
                    else
                        foreach (CustomShape shape in figure.MFigureShapes)
                            shape.Draw(e.Graphics, figure.MFigureColor, figure.MStrokeThickness);
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
                                _multisamplingFigure.MMultisamplingLine = null;

                            _multisamplingFigure = outFigure;
                            _selectedLine = outLine;
                            figure.MMultisamplingLine = outLine;
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
                LinkedListNode<CustomShape> node;

                foreach (CustomShape shape in _selectedFigure.MFigureShapes)
                    if (shape.GetType() == typeof(CustomEllipse))
                    {
                        CustomEllipse ellipse = shape as CustomEllipse;
                        if (ellipse.MPoint == _selectedVertex.Point)
                        {
                            node = _selectedFigure.MFigureShapes.Find(shape);

                            try
                            {
                                CustomLine line = node.Previous.Value as CustomLine;
                                line.MPoint2 = new Point(line.MPoint2.X - deltaX, line.MPoint2.Y - deltaY);
                            }
                            catch (Exception)
                            {
                                CustomLine line = _selectedFigure.MFigureShapes.Last.Value as CustomLine;
                                line.MPoint2 = new Point(line.MPoint2.X - deltaX, line.MPoint2.Y - deltaY);
                            }

                            try
                            {
                                CustomLine line = node.Next.Value as CustomLine;
                                line.MPoint1 = new Point(line.MPoint1.X - deltaX, line.MPoint1.Y - deltaY);
                            }
                            catch (Exception)
                            {
                                CustomLine line = _selectedFigure.MFigureShapes.First.Value as CustomLine;
                                line.MPoint1 = new Point(line.MPoint1.X - deltaX, line.MPoint1.Y - deltaY);
                            }

                            ellipse.MPoint = new Point(ellipse.MPoint.X - deltaX, ellipse.MPoint.Y - deltaY);

                            _selectedFigure.MFigureVertices.Find(_selectedVertex).Value.Point = new Point(_selectedVertex.Point.X - deltaX, _selectedVertex.Point.Y - deltaY);

                            drawingArea.Refresh();

                            _selectedFigure.MMaxX = int.MinValue;
                            _selectedFigure.MMaxY = int.MinValue;
                            _selectedFigure.MMinX = int.MaxValue;
                            _selectedFigure.MMinY = int.MaxValue;

                            foreach (CustomVertex vertex in _selectedFigure.MFigureVertices)
                            {
                                if (vertex.Point.X < _selectedFigure.MMinX) _selectedFigure.MMinX = vertex.Point.X;
                                if (vertex.Point.Y < _selectedFigure.MMinY) _selectedFigure.MMinY = vertex.Point.Y;
                                if (vertex.Point.X > _selectedFigure.MMaxX) _selectedFigure.MMaxX = vertex.Point.X;
                                if (vertex.Point.Y > _selectedFigure.MMaxY) _selectedFigure.MMaxY = vertex.Point.Y;
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
                if (_selectedFigure.MMaxX - deltaX > drawingArea.Bounds.Width - 10
                    || _selectedFigure.MMaxY - deltaY > drawingArea.Bounds.Height - 10
                    || _selectedFigure.MMinX - deltaX < 5
                    || _selectedFigure.MMinY - deltaY < 5)
                    return;

                Cursor = Cursors.SizeAll;

                foreach (CustomShape shape in _selectedFigure.MFigureShapes)
                    if (shape.GetType() == typeof(CustomEllipse))
                    {
                        CustomEllipse ellipse = shape as CustomEllipse;
                        ellipse.MPoint = new Point(ellipse.MPoint.X - deltaX, ellipse.MPoint.Y - deltaY);
                    }
                    else
                    {
                        CustomLine line = shape as CustomLine;
                        line.MPoint1 = new Point(line.MPoint1.X - deltaX, line.MPoint1.Y - deltaY);
                        line.MPoint2 = new Point(line.MPoint2.X - deltaX, line.MPoint2.Y - deltaY);
                    }

                foreach (CustomVertex vertex in _selectedFigure.MFigureVertices)
                    vertex.Point = new Point(vertex.Point.X - deltaX, vertex.Point.Y - deltaY);

                _selectedFigure.MMaxX -= deltaX;
                _selectedFigure.MMinX -= deltaX;
                _selectedFigure.MMaxY -= deltaY;
                _selectedFigure.MMinY -= deltaY;

                _mouseLastPosition = e.Location;
            }

            // Proceed when drawing current figure is in progress. 
            // Draws a temporary ine between the last vertex and current mouse position.
            if (_doDrawFigure && _currentFigure != null)
            {
                if (_currentFigure.MFigureShapes.Last<CustomShape>().GetType() == typeof(CustomLine))
                    _currentFigure.MFigureShapes.Remove(_currentFigure.MFigureShapes.Last<CustomShape>());
                _currentFigure.MFigureShapes.AddLast(new CustomLine(_currentFigure.GetLastVertex().Point, new Point(e.X, e.Y)));
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
                        if (_currentFigure.MVertexNumber < 3) return;
                        _isFirstVertex = true;
                        _figures.Add(_currentFigure);
                        _currentFigure.MFigureShapes.Remove(_currentFigure.MFigureShapes.Last<CustomShape>());
                        _currentFigure.MFigureShapes.AddLast(new CustomLine(_currentFigure.GetLastVertex().Point, _currentFigure.GetFirstVertex().Point));
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
                    outFigure.MFigureColor = _color;
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
                    outFigure.MStrokeThickness = _strokeThickness;
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
            if (_doDrawFigure && _currentFigure != null && _currentFigure.MVertexNumber != 0)
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
            if (_doDrawFigure && _currentFigure != null && _currentFigure.MVertexNumber != 0)
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

            string result = Prompt.ShowDialog("Determine the thickness of line. For best results use value lower than 10 px.", "Choose line thickness");
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
                foreach (CustomShape shape in figure.MFigureShapes)
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
            if (point.X < figure.MMinX - 5 || point.X > figure.MMaxX + 5 || point.Y < figure.MMinY - 5 || point.Y > figure.MMaxY + 5)
                return false;

            LinkedListNode<CustomVertex> currentFirstVertex = figure.GetFirstNode();
            LinkedListNode<CustomVertex> currentLastVertex = figure.GetLastNode();

            bool isInFigure = false;

            for (int i = 0, j = figure.MVertexNumber - 1; i < figure.MVertexNumber; j = i++)
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

            if (point.X < figure.MMinX - 10 || point.X > figure.MMaxX + 10 || point.Y < figure.MMinY - 10 || point.Y > figure.MMaxY + 10)
                return false;

            foreach (CustomVertex vertex in figure.MFigureVertices)
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

    #region classes
    public class CustomVertex
    {
        public CustomVertex(Point point)
        {
            Point = point;
        }

        public Point Point { get; set; }
    }

    public class CustomFigure
    {
        public CustomFigure(Point point, Color color, int strokeThickness)
        {
            MFigureColor = color;
            MStrokeThickness = strokeThickness;
            MFigureVertices = new LinkedList<CustomVertex>();
            MFigureVertices.AddFirst(new CustomVertex(point));
            MFigureShapes = new LinkedList<CustomShape>();
            MFigureShapes.AddFirst(new CustomEllipse(point));
            MMultisamplingLine = null;
            MMultisamplingColor = Color.Azure;
            MMaxX = point.X + 5;
            MMinX = point.X - 5;
            MMaxY = point.Y + 5;
            MMinY = point.Y - 5;
            MVertexNumber++;
        }

        public void AddInbetweenVertex(CustomVertex vertex, CustomLine line)
        {
            LinkedListNode<CustomShape> previousNode = MFigureShapes.Find(line).Previous;
            LinkedListNode<CustomShape> nextNode = MFigureShapes.Find(line).Next;

            CustomEllipse previousEllipse = previousNode.Value as CustomEllipse;
            CustomEllipse nextEllipse;

            if (nextNode == null)
                nextEllipse = MFigureShapes.First.Value as CustomEllipse;
            else
                nextEllipse = nextNode.Value as CustomEllipse;

            if (((Math.Abs(vertex.Point.X - previousEllipse.MPoint.X) < 10 && Math.Abs(vertex.Point.Y - previousEllipse.MPoint.Y) < 10)
                || (Math.Abs(vertex.Point.X - nextEllipse.MPoint.X) < 10) && Math.Abs(vertex.Point.Y - nextEllipse.MPoint.Y) < 10))
                return;

            MFigureShapes.AddAfter(previousNode, new CustomLine(previousEllipse.MPoint, vertex.Point));
            MFigureShapes.AddAfter(previousNode.Next, new CustomEllipse(vertex.Point));
            MFigureShapes.AddAfter(previousNode.Next.Next, new CustomLine(vertex.Point, nextEllipse.MPoint));
            MFigureShapes.Remove(line);

            MFigureVertices.AddAfter(MFigureVertices.Find(FindVertexWithValue(previousEllipse.MPoint)), vertex);

            MVertexNumber++;

            if (vertex.Point.X + 5 > MMaxX) MMaxX = vertex.Point.X + 5;
            if (vertex.Point.X - 5 < MMinX) MMinX = vertex.Point.X - 5;
            if (vertex.Point.Y + 5 > MMaxY) MMaxY = vertex.Point.Y + 5;
            if (vertex.Point.Y - 5 < MMinY) MMinY = vertex.Point.Y - 5;
        }

        public void AddVertex(Point point)
        {
            foreach (CustomVertex vertex in MFigureVertices)
                if (Math.Abs(vertex.Point.X - point.X) < 10 && Math.Abs(vertex.Point.Y - point.Y) < 10)
                    return;

            MFigureVertices.AddLast(new CustomVertex(point));
            MFigureShapes.AddLast(new CustomEllipse(point));
            if (point.X + 5 > MMaxX) MMaxX = point.X + 5;
            if (point.X - 5 < MMinX) MMinX = point.X - 5;
            if (point.Y + 5 > MMaxY) MMaxY = point.Y + 5;
            if (point.Y - 5 < MMinY) MMinY = point.Y - 5;
            MVertexNumber++;
        }

        public void DeleteVertex(CustomVertex vertex)
        {
            MFigureVertices.Remove(vertex);
            MVertexNumber--;

            if (MFigureShapes.Last.GetType() == typeof(CustomEllipse))
                MFigureShapes.Remove(MFigureShapes.Last);
        }

        private CustomVertex FindVertexWithValue(Point point)
        {
            foreach (CustomVertex vertex in MFigureVertices)
                if (vertex.Point == point)
                    return vertex;
            return null;
        }

        public LinkedListNode<CustomVertex> GetFirstNode()
        {
            return MFigureVertices.First;
        }

        public LinkedListNode<CustomVertex> GetLastNode()
        {
            return MFigureVertices.Last;
        }

        public CustomVertex GetFirstVertex()
        {
            return MFigureVertices.First.Value;
        }

        public CustomVertex GetLastVertex()
        {
            return MFigureVertices.Last.Value;
        }

        public CustomVertex GetPreviousVertex(CustomVertex vertex)
        {
            return MFigureVertices.Find(vertex).Previous.Value;
        }

        public Color MFigureColor { get; set; }
        public int MStrokeThickness { get; set; }
        public int MMaxX { get; set; }
        public int MMaxY { get; set; }
        public int MMinX { get; set; }
        public int MMinY { get; set; }
        public int MVertexNumber { get; set; }
        public LinkedList<CustomShape> MFigureShapes;
        public LinkedList<CustomVertex> MFigureVertices;
        public CustomLine MMultisamplingLine { get; set; }
        public Color MMultisamplingColor { get; set; }
    }

    abstract public class CustomShape
    {
        abstract public void Draw(Graphics graphics, Color color, int thickness);
    }

    public class CustomLine : CustomShape
    {
        public CustomLine(Point p1, Point p2)
        {
            MPoint1 = p1;
            MPoint2 = p2;
        }

        public override void Draw(Graphics graphics, Color color, int thickness)
        {
            //multisampling(graphics, color, thickness);
            SymmetricBresenham(graphics, color, thickness);
        }

        private void SymmetricBresenham(Graphics graphics, Color color, int thickness)
        {
            int upPixels = (thickness - 1) / 2;
            int downPixels = thickness - 1 - upPixels;

            int x1 = MPoint1.X, x2 = MPoint2.X, y1 = MPoint1.Y, y2 = MPoint2.Y;
            int incrX, incrY, d, dx, dy, incrNe, incrE;

            if (x1 < x2)
            {
                incrX = 1;
                dx = x2 - x1;
            }
            else
            {
                incrX = -1;
                dx = x1 - x2;
            }

            if (y1 < y2)
            {
                incrY = 1;
                dy = y2 - y1;
            }
            else
            {
                incrY = -1;
                dy = y1 - y2;
            }

            int xf = x1;
            int yf = y1;
            int xb = x2;
            int yb = y2;

            graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xf, yf), new Size(1, 1)));
            graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xb, yb), new Size(1, 1)));

            if (dx > dy)
            {
                incrE = 2 * dy;
                incrNe = 2 * (dy - dx);
                d = 2 * dy - dx;

                while (xf != xb && xf - 1 != xb && xf + 1 != xb)
                {
                    xf += incrX;
                    xb -= incrX;
                    if (d < 0) //Choose E and W
                        d += incrE;
                    else //Choose NE and SW
                    {
                        d += incrNe;
                        yf += incrY;
                        yb -= incrY;
                    }
                    graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xf, yf), new Size(1, 1)));
                    graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xb, yb), new Size(1, 1)));

                    for (int i = 1; i <= upPixels; i++)
                    {
                        graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xf, yf + i), new Size(1, 1)));
                        graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xb, yb + i), new Size(1, 1)));
                    }
                    for (int i = 1; i <= downPixels; i++)
                    {
                        graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xf, yf - i), new Size(1, 1)));
                        graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xb, yb - i), new Size(1, 1)));
                    }
                }
            }
            else
            {
                incrE = 2 * dx;
                incrNe = 2 * (dx - dy);
                d = 2 * dx - dy;

                while (yf != yb && yf - 1 != yb && yf + 1 != yb)
                {
                    yf += incrY;
                    yb -= incrY;

                    if (d < 0) //Choose E and W
                        d += incrE;
                    else //Choose NE and SW
                    {
                        d += incrNe;
                        xf += incrX;
                        xb -= incrX;
                    }
                    graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xf, yf), new Size(1, 1)));
                    graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xb, yb), new Size(1, 1)));

                    for (int i = 1; i <= upPixels; i++)
                    {
                        graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xf + i, yf), new Size(1, 1)));
                        graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xb + i, yb), new Size(1, 1)));
                    }
                    for (int i = 1; i <= downPixels; i++)
                    {
                        graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xf - i, yf), new Size(1, 1)));
                        graphics.FillRectangle(new SolidBrush(color), new Rectangle(new Point(xb - i, yb), new Size(1, 1)));
                    }
                }
            }
        }

        private void Multisampling(Graphics graphics, Color color, int thickness)
        {
            Bitmap bmp = new Bitmap(Math.Abs(MPoint1.X - MPoint2.X), Math.Abs(MPoint1.Y - MPoint2.Y));

            Pen pen = new Pen(new SolidBrush(Color.Blue));
            pen.EndCap = LineCap.Square;
            pen.StartCap = LineCap.Square;
            Point[] points = { new Point(0, 0), new Point(0, 2), new Point(bmp.Width - 2, bmp.Height), new Point(bmp.Width, bmp.Height) };
            graphics.DrawPolygon(pen, points);

            bmp = new Bitmap(Math.Abs(MPoint1.X - MPoint2.X), Math.Abs(MPoint1.Y - MPoint2.Y), graphics);
            graphics.Dispose();
            Point point;
            double alpha;
            for (int i = 0; i < bmp.Width; i += 2)
                for (int j = 0; j < bmp.Height; i += 2)
                {
                    alpha = 0;
                    point = new Point(i / 2, j / 2);
                    for (int k = 0; k < 2; k++)
                        for (int l = 0; l < 2; l++)
                            if (bmp.GetPixel(k, l) == Color.Blue)
                                alpha++;
                    alpha /= 4;
                    graphics.FillRectangle(new SolidBrush(Color.FromArgb((int)(alpha * 255), color)), new Rectangle(point, new Size(1, 1)));
                }
        }

        void IntensifyPixel(int x, int y, double distance, Graphics graphics, Color color)
        {
            double intensity = Math.Round(Math.Abs(distance));
            int intIntensity = (int)(intensity * 255);
            graphics.FillRectangle(new SolidBrush(Color.FromArgb(intIntensity, color)), new Rectangle(new Point(x, y), new Size(1, 1)));
            //WritePixel(x, y, intensity);
        }

        public Point MPoint1 { get; set; }
        public Point MPoint2 { get; set; }
    }

    public class CustomEllipse : CustomShape
    {
        public CustomEllipse(Point p)
        {
            MPoint = p;
        }

        public override void Draw(Graphics graphics, Color color, int thickness)
        {
            graphics.FillEllipse(new SolidBrush(color), new Rectangle(new Point(MPoint.X - 5, MPoint.Y - 5), new Size(10, 10)));
        }
        public Point MPoint { get; set; }
    }

    public static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form();
            prompt.StartPosition = FormStartPosition.CenterScreen;
            prompt.Width = 300;
            prompt.Height = 150;
            prompt.Text = caption;
            Label textLabel = new Label() { Left = 20, Top = 10, Height = 35, Width = 260, Text = text };
            TextBox textBox = new TextBox() { Left = 10, Top = 50, Width = 270 };
            Button confirmation = new Button() { Text = "OK", Left = 125, Width = 50, Top = 75 };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.ShowDialog();
            return textBox.Text;
        }
    }
    #endregion
}
