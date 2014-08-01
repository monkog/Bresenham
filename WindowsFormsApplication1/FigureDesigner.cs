using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class FigureDesigner : Form
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

        #region variables
        private bool m_isFirstVertex;
        private CustomFigure m_currentFigure;
        private Color m_color;
        private int m_strokeThickness;
        private bool m_doAddVertex;
        private List<CustomFigure> m_figures;
        private Point m_mouseDownPosition;
        private Point m_mouseUpPosition;
        private Point m_mouseLastPosition;
        private bool m_doDrawFigure;
        private CustomFigure m_selectedFigure;
        private bool m_doChangeColor;
        private CustomVertex m_selectedVertex;
        private bool m_doChangeThickness;
        private Cursor m_bucketCursor;
        private Cursor m_dotCursor;
        private Cursor m_penCursor;
        private Cursor m_handCursor;
        private CustomLine m_selectedLine;
        private CustomFigure m_multisamplingFigure;
        private bool m_doMultisample;
        #endregion

        public FigureDesigner()
        {
            InitializeComponent();
        }

        #region event methods
        private void FigureDesigner_Load(object sender, EventArgs e)
        {
            // Window state.
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.SaddleBrown;

            // Local variables initialisation.
            m_isFirstVertex = true;
            m_color = Color.Red;
            m_strokeThickness = 2;
            m_doAddVertex = false;
            m_doDrawFigure = false;
            m_figures = new List<CustomFigure>();
            m_mouseDownPosition = Point.Empty;
            m_mouseUpPosition = Point.Empty;
            m_doDrawFigure = false;
            m_selectedFigure = null;
            m_doChangeColor = false;
            m_selectedVertex = null;
            m_doChangeThickness = false;
            m_selectedLine = null;
            m_multisamplingFigure = null;
            m_doMultisample = false;

            // Layout.
            buttonsPanel.BackColor = Color.Bisque;
            buttonsPanel.BorderStyle = BorderStyle.Fixed3D;

            drawingArea.BackColor = Color.Bisque;
            drawingArea.BorderStyle = BorderStyle.Fixed3D;

            colorLabel.BackColor = Color.Bisque;
            sizeLabel.BackColor = Color.Bisque;

            colorPictureBox.BackColor = m_color;
            colorPictureBox.BorderStyle = BorderStyle.Fixed3D;

            // Functionality.
            drawingArea.Paint += drawingArea_Paint;

            // Cursors.
            const int IMAGE_CURSOR = 2;
            const uint LR_LOADFROMFILE = 0x00000010;
            //IntPtr ipImage = LoadImage(IntPtr.Zero,
            //    @"d:\Studia\Semestr_5\Grafika_Komputerowa_I\Laboratoria\01-Edytor_Grafiki\WindowsFormsApplication1\WindowsFormsApplication1\Bucket.cur ",
            //    IMAGE_CURSOR, 0, 0, LR_LOADFROMFILE);

            //m_bucketCursor = new Cursor(ipImage);

            //ipImage = LoadImage(IntPtr.Zero,
            //    @"d:\Studia\Semestr_5\Grafika_Komputerowa_I\Laboratoria\01-Edytor_Grafiki\WindowsFormsApplication1\WindowsFormsApplication1\Dot.cur ",
            //    IMAGE_CURSOR, 0, 0, LR_LOADFROMFILE);

            //m_dotCursor = new Cursor(ipImage);

            //ipImage = LoadImage(IntPtr.Zero,
            //    @"d:\Studia\Semestr_5\Grafika_Komputerowa_I\Laboratoria\01-Edytor_Grafiki\WindowsFormsApplication1\WindowsFormsApplication1\Pen.cur ",
            //    IMAGE_CURSOR, 0, 0, LR_LOADFROMFILE);

            //m_penCursor = new Cursor(ipImage);

            //ipImage = LoadImage(IntPtr.Zero,
            //    @"d:\Studia\Semestr_5\Grafika_Komputerowa_I\Laboratoria\01-Edytor_Grafiki\WindowsFormsApplication1\WindowsFormsApplication1\Hand.cur ",
            //    IMAGE_CURSOR, 0, 0, LR_LOADFROMFILE);

            //m_handCursor = new Cursor(ipImage);
            m_bucketCursor = m_dotCursor = m_penCursor = m_handCursor = Cursors.Hand;

            drawFigureButton.PerformClick();
        }

        private void drawingArea_Paint(object sender, PaintEventArgs e)
        {
            if (m_currentFigure != null)
                foreach (CustomShape shape in m_currentFigure.m_figureShapes)
                    shape.draw(e.Graphics, m_currentFigure.m_figureColor, m_currentFigure.m_strokeThickness);
            if (m_figures.Count != 0)
                foreach (CustomFigure figure in m_figures)
                {
                    if (m_doMultisample && m_multisamplingFigure == figure && m_selectedLine != null)
                    {
                        foreach (CustomShape shape in figure.m_figureShapes)
                            if (shape.GetType() == typeof(CustomLine))
                            {
                                CustomLine line = shape as CustomLine;
                                if (line == m_selectedLine)
                                    shape.draw(e.Graphics, figure.m_multisamplingColor, figure.m_strokeThickness);
                                else
                                    shape.draw(e.Graphics, figure.m_figureColor, figure.m_strokeThickness);
                            }
                            else
                                shape.draw(e.Graphics, figure.m_figureColor, figure.m_strokeThickness);
                    }
                    else
                        foreach (CustomShape shape in figure.m_figureShapes)
                            shape.draw(e.Graphics, figure.m_figureColor, figure.m_strokeThickness);
                }
        }

        private void drawingArea_MouseDown(object sender, MouseEventArgs e)
        {
            m_mouseDownPosition = new Point(e.X - 5, e.Y - 5);
            m_mouseLastPosition = m_mouseDownPosition;

            // Selects the figure to move around the drawingArea.
            if (m_selectedFigure == null && !m_doDrawFigure && !m_doAddVertex && !m_doChangeColor && !m_doChangeThickness)
            {
                CustomVertex outVertex;
                CustomLine outLine;
                CustomFigure outFigure;

                foreach (CustomFigure figure in m_figures)
                    if (isPointVertice(e.Location, figure, out outVertex))
                    {
                        m_selectedFigure = figure;
                        m_selectedVertex = outVertex;
                        return;
                    }

                if (m_doMultisample)
                    foreach (CustomFigure figure in m_figures)
                        if (isPointOnBorder(e.Location, out outFigure, out outLine))
                        {
                            if (m_multisamplingFigure != null)
                                m_multisamplingFigure.m_multisamplingLine = null;

                            m_multisamplingFigure = outFigure;
                            m_selectedLine = outLine;
                            figure.m_multisamplingLine = outLine;
                            drawingArea.Refresh();
                            return;
                        }

                foreach (CustomFigure figure in m_figures)
                    if (isPointInFigure(e.Location, figure))
                    {
                        m_selectedFigure = figure;
                        this.Cursor = Cursors.SizeAll;
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

            if (m_doAddVertex || m_doDrawFigure)
                Cursor = m_dotCursor;

            // Changes the cursor if the drawing mode is off and mouse is above any of the previously drawn shapes.
            if (m_selectedFigure == null && !m_doDrawFigure && !m_doAddVertex)
                if (m_doChangeColor && isPointOnBorder(e.Location, out outFigure, out outLine))
                {
                    this.Cursor = m_bucketCursor;
                }
                else if (m_doChangeThickness)
                {
                    foreach (CustomFigure figure in m_figures)
                        if (isPointOnBorder(e.Location, out outFigure, out outLine))
                        {
                            this.Cursor = m_penCursor;
                            return;
                        }
                }
                else if (!m_doChangeColor && !m_doChangeThickness)
                {
                    foreach (CustomFigure figure in m_figures)
                        if (isPointVertice(e.Location, figure, out outVertex))
                        {
                            this.Cursor = m_handCursor;
                            return;
                        }

                    foreach (CustomFigure figure in m_figures)
                        if (isPointInFigure(e.Location, figure))
                        {
                            this.Cursor = Cursors.SizeAll;
                            return;
                        }
                }

            // Move the vertex between the bounds of drawingArea.
            if (m_selectedVertex != null)
            {
                int deltaX = m_mouseLastPosition.X - e.X;
                int deltaY = m_mouseLastPosition.Y - e.Y;

                // Prevent draging the figure outside the drawing area, so that it won't loose it's vertices and edges.
                if (m_selectedVertex.m_point.X - deltaX > drawingArea.Bounds.Width - 10
                    || m_selectedVertex.m_point.Y - deltaY > drawingArea.Bounds.Height - 10
                    || m_selectedVertex.m_point.X - deltaX < 5
                    || m_selectedVertex.m_point.Y - deltaY < 5)
                    return;

                this.Cursor = m_handCursor;
                LinkedListNode<CustomShape> node;

                foreach (CustomShape shape in m_selectedFigure.m_figureShapes)
                    if (shape.GetType() == typeof(CustomEllipse))
                    {
                        CustomEllipse ellipse = shape as CustomEllipse;
                        if (ellipse.m_point == m_selectedVertex.m_point)
                        {
                            node = m_selectedFigure.m_figureShapes.Find(shape);

                            try
                            {
                                CustomLine line = node.Previous.Value as CustomLine;
                                line.m_point2 = new Point(line.m_point2.X - deltaX, line.m_point2.Y - deltaY);
                            }
                            catch (Exception)
                            {
                                CustomLine line = m_selectedFigure.m_figureShapes.Last.Value as CustomLine;
                                line.m_point2 = new Point(line.m_point2.X - deltaX, line.m_point2.Y - deltaY);
                            }

                            try
                            {
                                CustomLine line = node.Next.Value as CustomLine;
                                line.m_point1 = new Point(line.m_point1.X - deltaX, line.m_point1.Y - deltaY);
                            }
                            catch (Exception)
                            {
                                CustomLine line = m_selectedFigure.m_figureShapes.First.Value as CustomLine;
                                line.m_point1 = new Point(line.m_point1.X - deltaX, line.m_point1.Y - deltaY);
                            }

                            ellipse.m_point = new Point(ellipse.m_point.X - deltaX, ellipse.m_point.Y - deltaY);

                            m_selectedFigure.m_figureVertices.Find(m_selectedVertex).Value.m_point = new Point(m_selectedVertex.m_point.X - deltaX, m_selectedVertex.m_point.Y - deltaY);

                            drawingArea.Refresh();

                            m_selectedFigure.m_maxX = int.MinValue;
                            m_selectedFigure.m_maxY = int.MinValue;
                            m_selectedFigure.m_minX = int.MaxValue;
                            m_selectedFigure.m_minY = int.MaxValue;

                            foreach (CustomVertex vertex in m_selectedFigure.m_figureVertices)
                            {
                                if (vertex.m_point.X < m_selectedFigure.m_minX) m_selectedFigure.m_minX = vertex.m_point.X;
                                if (vertex.m_point.Y < m_selectedFigure.m_minY) m_selectedFigure.m_minY = vertex.m_point.Y;
                                if (vertex.m_point.X > m_selectedFigure.m_maxX) m_selectedFigure.m_maxX = vertex.m_point.X;
                                if (vertex.m_point.Y > m_selectedFigure.m_maxY) m_selectedFigure.m_maxY = vertex.m_point.Y;
                            }

                            m_mouseLastPosition = e.Location;
                            return;
                        }
                    }
            }

            // Move the figure between the bounds of the drawingArea.
            if (m_selectedFigure != null)
            {
                int deltaX = m_mouseLastPosition.X - e.X;
                int deltaY = m_mouseLastPosition.Y - e.Y;

                // Prevent draging the figure outside the drawing area, so that it won't loose it's vertices and edges.
                if (m_selectedFigure.m_maxX - deltaX > drawingArea.Bounds.Width - 10
                    || m_selectedFigure.m_maxY - deltaY > drawingArea.Bounds.Height - 10
                    || m_selectedFigure.m_minX - deltaX < 5
                    || m_selectedFigure.m_minY - deltaY < 5)
                    return;

                this.Cursor = Cursors.SizeAll;

                foreach (CustomShape shape in m_selectedFigure.m_figureShapes)
                    if (shape.GetType() == typeof(CustomEllipse))
                    {
                        CustomEllipse ellipse = shape as CustomEllipse;
                        ellipse.m_point = new Point(ellipse.m_point.X - deltaX, ellipse.m_point.Y - deltaY);
                    }
                    else
                    {
                        CustomLine line = shape as CustomLine;
                        line.m_point1 = new Point(line.m_point1.X - deltaX, line.m_point1.Y - deltaY);
                        line.m_point2 = new Point(line.m_point2.X - deltaX, line.m_point2.Y - deltaY);
                    }

                foreach (CustomVertex vertex in m_selectedFigure.m_figureVertices)
                    vertex.m_point = new Point(vertex.m_point.X - deltaX, vertex.m_point.Y - deltaY);

                m_selectedFigure.m_maxX -= deltaX;
                m_selectedFigure.m_minX -= deltaX;
                m_selectedFigure.m_maxY -= deltaY;
                m_selectedFigure.m_minY -= deltaY;

                m_mouseLastPosition = e.Location;
            }

            // Proceed when drawing current figure is in progress. 
            // Draws a temporary ine between the last vertex and current mouse position.
            if (m_doDrawFigure && m_currentFigure != null)
            {
                if (m_currentFigure.m_figureShapes.Last<CustomShape>().GetType() == typeof(CustomLine))
                    m_currentFigure.m_figureShapes.Remove(m_currentFigure.m_figureShapes.Last<CustomShape>());
                m_currentFigure.m_figureShapes.AddLast(new CustomLine(m_currentFigure.getLastVertex().m_point, new Point(e.X, e.Y)));
            }

            drawingArea.Refresh();
        }

        private void drawingArea_MouseUp(object sender, MouseEventArgs e)
        {
            m_mouseUpPosition = new Point(e.X, e.Y);
            m_selectedFigure = null;
            m_selectedVertex = null;

            CustomFigure figure;
            CustomLine line;

            // Add vertex to existing figure.
            if (!m_doDrawFigure && m_doAddVertex && m_figures.Count != 0 && isPointOnBorder(m_mouseUpPosition, out figure, out line))
            {
                figure.addInbetweenVertex(new CustomVertex(m_mouseUpPosition), line);
                drawingArea.Refresh();
                return;
            }

            // Draw figure.
            if (m_doDrawFigure)
            {
                if (m_isFirstVertex)
                {
                    m_isFirstVertex = false;
                    m_currentFigure = new CustomFigure(m_mouseUpPosition, m_color, m_strokeThickness);
                }
                else
                {
                    if (isFigureComplete(m_mouseUpPosition))
                    {
                        if (m_currentFigure.m_vertexNumber < 3) return;
                        m_isFirstVertex = true;
                        m_figures.Add(m_currentFigure);
                        m_currentFigure.m_figureShapes.Remove(m_currentFigure.m_figureShapes.Last<CustomShape>());
                        m_currentFigure.m_figureShapes.AddLast(new CustomLine(m_currentFigure.getLastVertex().m_point, m_currentFigure.getFirstVertex().m_point));
                        drawingArea.Refresh();
                        m_currentFigure = null;
                        return;
                    }
                    m_currentFigure.addVertex(m_mouseUpPosition);
                }
                drawingArea.Refresh();
            }

            // Changes the color of the chosen figure.
            if (m_doChangeColor)
            {
                CustomFigure outFigure;
                CustomLine outLine;

                if (isPointOnBorder(e.Location, out outFigure, out outLine))
                {
                    outFigure.m_figureColor = m_color;
                    drawingArea.Refresh();
                    return;
                }
            }

            // Changes the line thickness of the chosen figure.
            if (m_doChangeThickness)
            {
                CustomFigure outFigure;
                CustomLine outLine;

                if (isPointOnBorder(e.Location, out outFigure, out outLine))
                {
                    outFigure.m_strokeThickness = m_strokeThickness;
                    drawingArea.Refresh();
                    return;
                }
            }
        }

        private void drawFigureButton_Click(object sender, EventArgs e)
        {
            if (m_doDrawFigure)
                if (drawFigureButton.Text != "CANCEL")
                {
                    MessageBox.Show("You have to finish drawing to use this functionality.", "Don't do that!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    Cursor = Cursors.Default;
                    drawFigureButton.Text = "DRAW FIGURE";
                    m_doDrawFigure = false;
                    return;
                }
            else
            {
                drawFigureButton.Text = "CANCEL";
                m_doDrawFigure = true;
            }
            m_doAddVertex = false;
            m_doChangeColor = false;
            m_doChangeThickness = false;

            changeColorButton.Text = "CHANGE COLOR";
            addVertexButton.Text = "ADD VERTEX";
            changeSizeButton.Text = "CHANGE SIZE";
            Cursor = m_dotCursor;
        }

        private void addVertexButton_Click(object sender, EventArgs e)
        {
            if (m_doAddVertex)
            {
                Cursor = Cursors.Default;
                m_doAddVertex = false;
                addVertexButton.Text = "ADD VERTEX";
                return;
            }

            if (m_doDrawFigure && m_currentFigure != null)
            {
                MessageBox.Show("You have to finish drawing to use this functionality.", "Don't do that!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (m_figures.Count == 0)
            {
                MessageBox.Show("You have to draw at least one figure to use this functionality.", "Don't do that!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            m_doAddVertex = true;
            m_doDrawFigure = false;
            m_doChangeColor = false;
            m_doChangeThickness = false;
            changeColorButton.Text = "CHANGE COLOR";
            addVertexButton.Text = "CANCEL";
            drawFigureButton.Text = "DRAW FIGURE";
            changeSizeButton.Text = "CHANGE SIZE";

            Cursor = m_dotCursor;
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;

            changeColorButton.Text = "CHANGE COLOR";
            m_doChangeColor = false;
            addVertexButton.Text = "ADD VERTEX";
            m_doAddVertex = false;
            drawFigureButton.Text = "DRAW FIGURE";
            m_doDrawFigure = false;
            changeSizeButton.Text = "CHANGE SIZE";
            m_doChangeThickness = false;

            m_isFirstVertex = true;
            m_figures.Clear();
            m_currentFigure = null;
            drawingArea.Refresh();
        }

        private void changeColorButton_Click(object sender, EventArgs e)
        {
            if (m_doDrawFigure && m_currentFigure != null && m_currentFigure.m_vertexNumber != 0)
            {
                MessageBox.Show("You have to finish drawing to use this functionality.", "Don't do that!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (m_doChangeColor && !m_doDrawFigure)
            {
                changeColorButton.Text = "CHANGE COLOR";
                m_doChangeColor = false;
                return;
            }

            addVertexButton.Text = "ADD VERTEX";
            m_doAddVertex = false;
            changeSizeButton.Text = "CHANGE SIZE";
            m_doChangeThickness = false;

            ColorDialog colorDialog = new ColorDialog();
            colorDialog.AllowFullOpen = true;

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                m_color = colorDialog.Color;
                colorPictureBox.BackColor = m_color;
                drawingArea.Refresh();

                if (!m_doDrawFigure)
                {
                    m_doChangeColor = true;
                    changeColorButton.Text = "CANCEL";
                }
            }

            colorDialog.Dispose();
        }

        private void changeSizeButton_Click(object sender, EventArgs e)
        {
            if (m_doDrawFigure && m_currentFigure != null && m_currentFigure.m_vertexNumber != 0)
            {
                MessageBox.Show("You have to finish drawing to use this functionality.", "Don't do that!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!m_doDrawFigure)
            {
                changeColorButton.Text = "CHANGE COLOR";
                m_doChangeColor = false;
                addVertexButton.Text = "ADD VERTEX";
                m_doAddVertex = false;
                drawFigureButton.Text = "DRAW FIGURE";
                m_doDrawFigure = false;
            }

            if (m_doChangeThickness)
            {
                m_doChangeThickness = false;
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

            if (!m_doDrawFigure)
            {
                changeSizeButton.Text = "CANCEL";
                m_doChangeThickness = true; ;
            }
            m_strokeThickness = size;
            sizeLabel.Text = "CURRENT SIZE: " + size.ToString() + " px";
        }
        #endregion

        #region methods
        private bool isFigureComplete(Point point)
        {
            CustomVertex vertex = m_currentFigure.getFirstVertex();

            return (Math.Abs(point.X - vertex.m_point.X) < 10 && Math.Abs(point.Y - vertex.m_point.Y) < 10);
        }

        private bool isPointOnBorder(Point point, out CustomFigure outFigure, out CustomLine outLine)
        {
            outFigure = null;
            outLine = null;

            foreach (CustomFigure figure in m_figures)
                foreach (CustomShape shape in figure.m_figureShapes)
                    if (shape.GetType() == typeof(CustomLine))
                    {
                        CustomLine line = shape as CustomLine;

                        double AB = Math.Sqrt((line.m_point1.X - line.m_point2.X) * (line.m_point1.X - line.m_point2.X) + (line.m_point1.Y - line.m_point2.Y) * (line.m_point1.Y - line.m_point2.Y));
                        double AC = Math.Sqrt((point.X - line.m_point1.X) * (point.X - line.m_point1.X) + (point.Y - line.m_point1.Y) * (point.Y - line.m_point1.Y));
                        double BC = Math.Sqrt((point.X - line.m_point2.X) * (point.X - line.m_point2.X) + (point.Y - line.m_point2.Y) * (point.Y - line.m_point2.Y));

                        if (AC + BC < AB + 2 && AC + BC > AB - 2)
                        {
                            outFigure = figure;
                            outLine = line;
                            return true;
                        }
                    }

            return false;
        }

        private bool isPointInFigure(Point point, CustomFigure figure)
        {
            if (point.X < figure.m_minX - 5 || point.X > figure.m_maxX + 5 || point.Y < figure.m_minY - 5 || point.Y > figure.m_maxY + 5)
                return false;

            LinkedListNode<CustomVertex> currentFirstVertex = figure.getFirstNode();
            LinkedListNode<CustomVertex> currentLastVertex = figure.getLastNode();

            bool isInFigure = false;

            for (int i = 0, j = figure.m_vertexNumber - 1; i < figure.m_vertexNumber; j = i++)
            {
                int iX = currentFirstVertex.Value.m_point.X;
                int iY = currentFirstVertex.Value.m_point.Y;
                int jX = currentLastVertex.Value.m_point.X;
                int jY = currentLastVertex.Value.m_point.Y;

                if ((iY > point.Y) != (jY > point.Y)
                    && (point.X < (jX - iX) * (point.Y - iY) / (jY - iY) + iX))
                    isInFigure = !isInFigure;
                currentLastVertex = currentFirstVertex;
                currentFirstVertex = currentFirstVertex.Next;
            }
            return isInFigure;
        }

        private bool isPointVertice(Point point, CustomFigure figure, out CustomVertex outVertex)
        {
            outVertex = null;

            if (point.X < figure.m_minX - 10 || point.X > figure.m_maxX + 10 || point.Y < figure.m_minY - 10 || point.Y > figure.m_maxY + 10)
                return false;

            foreach (CustomVertex vertex in figure.m_figureVertices)
                if (point.X < vertex.m_point.X + 10 && point.X > vertex.m_point.X - 10
                    && point.Y < vertex.m_point.Y + 10 && point.Y > vertex.m_point.Y - 10)
                {
                    outVertex = vertex;
                    return true;
                }

            return false;
        }

        private void multisamplingButton_Click(object sender, EventArgs e)
        {
            m_doMultisample = true;
        }
        #endregion
    }

    #region classes
    public class CustomVertex
    {
        public CustomVertex(Point point)
        {
            m_point = point;
        }

        public Point m_point { get; set; }
    }

    public class CustomFigure
    {
        public CustomFigure(Point point, Color color, int strokeThickness)
        {
            m_figureColor = color;
            m_strokeThickness = strokeThickness;
            m_figureVertices = new LinkedList<CustomVertex>();
            m_figureVertices.AddFirst(new CustomVertex(point));
            m_figureShapes = new LinkedList<CustomShape>();
            m_figureShapes.AddFirst(new CustomEllipse(point));
            m_multisamplingLine = null;
            m_multisamplingColor = Color.Azure;
            m_maxX = point.X + 5;
            m_minX = point.X - 5;
            m_maxY = point.Y + 5;
            m_minY = point.Y - 5;
            m_vertexNumber++;
        }

        public void addInbetweenVertex(CustomVertex vertex, CustomLine line)
        {
            LinkedListNode<CustomShape> previousNode = m_figureShapes.Find(line).Previous;
            LinkedListNode<CustomShape> nextNode = m_figureShapes.Find(line).Next;

            CustomEllipse previousEllipse = previousNode.Value as CustomEllipse;
            CustomEllipse nextEllipse;

            if (nextNode == null)
                nextEllipse = m_figureShapes.First.Value as CustomEllipse;
            else
                nextEllipse = nextNode.Value as CustomEllipse;

            if (((Math.Abs(vertex.m_point.X - previousEllipse.m_point.X) < 10 && Math.Abs(vertex.m_point.Y - previousEllipse.m_point.Y) < 10)
                || (Math.Abs(vertex.m_point.X - nextEllipse.m_point.X) < 10) && Math.Abs(vertex.m_point.Y - nextEllipse.m_point.Y) < 10))
                return;

            m_figureShapes.AddAfter(previousNode, new CustomLine(previousEllipse.m_point, vertex.m_point));
            m_figureShapes.AddAfter(previousNode.Next, new CustomEllipse(vertex.m_point));
            m_figureShapes.AddAfter(previousNode.Next.Next, new CustomLine(vertex.m_point, nextEllipse.m_point));
            m_figureShapes.Remove(line);

            m_figureVertices.AddAfter(m_figureVertices.Find(findVertexWithValue(previousEllipse.m_point)), vertex);

            m_vertexNumber++;

            if (vertex.m_point.X + 5 > m_maxX) m_maxX = vertex.m_point.X + 5;
            if (vertex.m_point.X - 5 < m_minX) m_minX = vertex.m_point.X - 5;
            if (vertex.m_point.Y + 5 > m_maxY) m_maxY = vertex.m_point.Y + 5;
            if (vertex.m_point.Y - 5 < m_minY) m_minY = vertex.m_point.Y - 5;
        }

        public void addVertex(Point point)
        {
            foreach (CustomVertex vertex in m_figureVertices)
                if (Math.Abs(vertex.m_point.X - point.X) < 10 && Math.Abs(vertex.m_point.Y - point.Y) < 10)
                    return;

            m_figureVertices.AddLast(new CustomVertex(point));
            m_figureShapes.AddLast(new CustomEllipse(point));
            if (point.X + 5 > m_maxX) m_maxX = point.X + 5;
            if (point.X - 5 < m_minX) m_minX = point.X - 5;
            if (point.Y + 5 > m_maxY) m_maxY = point.Y + 5;
            if (point.Y - 5 < m_minY) m_minY = point.Y - 5;
            m_vertexNumber++;
        }

        public void deleteVertex(CustomVertex vertex)
        {
            m_figureVertices.Remove(vertex);
            m_vertexNumber--;

            if (m_figureShapes.Last.GetType() == typeof(CustomEllipse))
                m_figureShapes.Remove(m_figureShapes.Last);
        }

        private CustomVertex findVertexWithValue(Point point)
        {
            foreach (CustomVertex vertex in m_figureVertices)
                if (vertex.m_point == point)
                    return vertex;
            return null;
        }

        public LinkedListNode<CustomVertex> getFirstNode()
        {
            return m_figureVertices.First;
        }

        public LinkedListNode<CustomVertex> getLastNode()
        {
            return m_figureVertices.Last;
        }

        public CustomVertex getFirstVertex()
        {
            return m_figureVertices.First.Value;
        }

        public CustomVertex getLastVertex()
        {
            return m_figureVertices.Last.Value;
        }

        public CustomVertex getPreviousVertex(CustomVertex vertex)
        {
            return m_figureVertices.Find(vertex).Previous.Value;
        }

        public Color m_figureColor { get; set; }
        public int m_strokeThickness { get; set; }
        public int m_maxX { get; set; }
        public int m_maxY { get; set; }
        public int m_minX { get; set; }
        public int m_minY { get; set; }
        public int m_vertexNumber { get; set; }
        public LinkedList<CustomShape> m_figureShapes;
        public LinkedList<CustomVertex> m_figureVertices;
        public CustomLine m_multisamplingLine { get; set; }
        public Color m_multisamplingColor { get; set; }
    }

    abstract public class CustomShape
    {
        abstract public void draw(Graphics graphics, Color color, int thickness);
    }

    public class CustomLine : CustomShape
    {
        public CustomLine(Point p1, Point p2)
        {
            m_point1 = p1;
            m_point2 = p2;
        }

        public override void draw(Graphics graphics, Color color, int thickness)
        {
            //multisampling(graphics, color, thickness);
            symmetricBresenham(graphics, color, thickness);
        }

        private void symmetricBresenham(Graphics graphics, Color color, int thickness)
        {
            int upPixels = (thickness - 1) / 2;
            int downPixels = thickness - 1 - upPixels;

            int x1 = m_point1.X, x2 = m_point2.X, y1 = m_point1.Y, y2 = m_point2.Y;
            int incrX, incrY, d, dx, dy, incrNE, incrE;

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
                incrNE = 2 * (dy - dx);
                d = 2 * dy - dx;

                while (xf != xb && xf - 1 != xb && xf + 1 != xb)
                {
                    xf += incrX;
                    xb -= incrX;
                    if (d < 0) //Choose E and W
                        d += incrE;
                    else //Choose NE and SW
                    {
                        d += incrNE;
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
                incrNE = 2 * (dx - dy);
                d = 2 * dx - dy;

                while (yf != yb && yf - 1 != yb && yf + 1 != yb)
                {
                    yf += incrY;
                    yb -= incrY;

                    if (d < 0) //Choose E and W
                        d += incrE;
                    else //Choose NE and SW
                    {
                        d += incrNE;
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

        private void multisampling(Graphics graphics, Color color, int thickness)
        {
            Bitmap bmp = new Bitmap(Math.Abs(m_point1.X - m_point2.X), Math.Abs(m_point1.Y - m_point2.Y));

            Pen pen = new Pen(new SolidBrush(Color.Blue));
            pen.EndCap = System.Drawing.Drawing2D.LineCap.Square;
            pen.StartCap = System.Drawing.Drawing2D.LineCap.Square;
            Point[] points = { new Point(0, 0), new Point(0, 2), new Point(bmp.Width - 2, bmp.Height), new Point(bmp.Width, bmp.Height) };
            graphics.DrawPolygon(pen, points);

            bmp = new Bitmap(Math.Abs(m_point1.X - m_point2.X), Math.Abs(m_point1.Y - m_point2.Y), graphics);
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

        public Point m_point1 { get; set; }
        public Point m_point2 { get; set; }
    }

    public class CustomEllipse : CustomShape
    {
        public CustomEllipse(Point p)
        {
            m_point = p;
        }

        public override void draw(Graphics graphics, Color color, int thickness)
        {
            graphics.FillEllipse(new SolidBrush(color), new Rectangle(new Point(m_point.X - 5, m_point.Y - 5), new Size(10, 10)));
        }
        public Point m_point { get; set; }
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
