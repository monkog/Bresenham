using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WindowsFormsApplication1.Shapes
{
    /// <summary>
    /// Represents a figure composed of lines and ellipses
    /// </summary>
    public class CustomFigure
    {
        #region Public Properties
        /// <summary>
        /// Gets the first node in the LinkedList.
        /// </summary>
        /// <value>
        /// The first node in the LinkedList.
        /// </value>
        public LinkedListNode<CustomVertex> FirstNode { get { return FigureVertices.First; } }
        /// <summary>
        /// Gets the last node in the LinkedList.
        /// </summary>
        /// <value>
        /// The last node in the LinkedList.
        /// </value>
        public LinkedListNode<CustomVertex> LastNode { get { return FigureVertices.Last; } }
        /// <summary>
        /// Gets the first vertex.
        /// </summary>
        /// <value>
        /// The first vertex.
        /// </value>
        public CustomVertex FirstVertex { get { return FirstNode.Value; } }
        /// <summary>
        /// Gets the last vertex.
        /// </summary>
        /// <value>
        /// The last vertex.
        /// </value>
        public CustomVertex LastVertex { get { return LastNode.Value; } }
        /// <summary>
        /// Gets or sets the color of the figure.
        /// </summary>
        /// <value>
        /// The color of the figure.
        /// </value>
        public Color FigureColor { get; set; }
        /// <summary>
        /// Gets or sets the stroke thickness.
        /// </summary>
        /// <value>
        /// The stroke thickness.
        /// </value>
        public int StrokeThickness { get; set; }
        /// <summary>
        /// Gets or sets the maximum x coordinate of the figure.
        /// </summary>
        /// <value>
        /// The maximum x coordinate of the figure.
        /// </value>
        public int MaxX { get; set; }
        /// <summary>
        /// Gets or sets the maximum y coordinate of the figure.
        /// </summary>
        /// <value>
        /// The maximum y coordinate of the figure.
        /// </value>
        public int MaxY { get; set; }
        /// <summary>
        /// Gets or sets the minimum x coordinate of the figure.
        /// </summary>
        /// <value>
        /// The minimum x coordinate of the figure.
        /// </value>
        public int MinX { get; set; }
        /// <summary>
        /// Gets or sets the minimum y coordinate of the figure.
        /// </summary>
        /// <value>
        /// The minimum y coordinate of the figure.
        /// </value>
        public int MinY { get; set; }
        /// <summary>
        /// Gets or sets the vertex number.
        /// </summary>
        /// <value>
        /// The vertex number.
        /// </value>
        public int VertexNumber { get; set; }
        /// <summary>
        /// The figure shapes
        /// </summary>
        public LinkedList<IShape> FigureShapes { get; set; }
        /// <summary>
        /// Gets or sets the figure vertices.
        /// </summary>
        /// <value>
        /// The figure vertices.
        /// </value>
        public LinkedList<CustomVertex> FigureVertices { get; set; }
        /// <summary>
        /// Gets or sets the multisampling line.
        /// </summary>
        /// <value>
        /// The multisampling line.
        /// </value>
        public CustomLine MultisamplingLine { get; set; }
        /// <summary>
        /// Gets or sets the color of the multisampling.
        /// </summary>
        /// <value>
        /// The color of the multisampling.
        /// </value>
        public Color MultisamplingColor { get; set; }
        /// <summary>
        /// Gets the size of the vertex.
        /// </summary>
        /// <value>
        /// The size of the vertex.
        /// </value>
        public int VertexSize { get; private set; }
        #endregion Public Properties
        #region .ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomFigure"/> class.
        /// </summary>
        /// <param name="point">The position point.</param>
        /// <param name="color">The color.</param>
        /// <param name="strokeThickness">The stroke thickness.</param>
        /// <param name="vertexSize">Size of the vertex</param>
        public CustomFigure(Point point, Color color, int strokeThickness)
        {
            FigureColor = color;
            StrokeThickness = strokeThickness;
            VertexSize = StrokeThickness + 6;
            FigureVertices = new LinkedList<CustomVertex>();
            FigureVertices.AddFirst(new CustomVertex(point));
            FigureShapes = new LinkedList<IShape>();
            FigureShapes.AddFirst(new CustomEllipse(point));
            MultisamplingLine = null;
            MultisamplingColor = Color.Azure;
            MaxX = point.X + 5;
            MinX = point.X - 5;
            MaxY = point.Y + 5;
            MinY = point.Y - 5;
            VertexNumber++;
        }
        #endregion .ctor
        #region Public Methods
        /// <summary>
        /// Adds the vertex on the specified line.
        /// </summary>
        /// <param name="vertex">The vertex.</param>
        /// <param name="line">The line.</param>
        public void AddInbetweenVertex(CustomVertex vertex, CustomLine line)
        {
            var previousNode = FigureShapes.Find(line).Previous;
            var nextNode = FigureShapes.Find(line).Next;

            CustomEllipse previousEllipse = previousNode.Value as CustomEllipse;
            CustomEllipse nextEllipse;

            if (nextNode == null)
                nextEllipse = FigureShapes.First.Value as CustomEllipse;
            else
                nextEllipse = nextNode.Value as CustomEllipse;

            if (((Math.Abs(vertex.Point.X - previousEllipse.Position.X) < 10 && Math.Abs(vertex.Point.Y - previousEllipse.Position.Y) < 10)
                || (Math.Abs(vertex.Point.X - nextEllipse.Position.X) < 10) && Math.Abs(vertex.Point.Y - nextEllipse.Position.Y) < 10))
                return;

            FigureShapes.AddAfter(previousNode, new CustomLine(previousEllipse.Position, vertex.Point));
            FigureShapes.AddAfter(previousNode.Next, new CustomEllipse(vertex.Point));
            FigureShapes.AddAfter(previousNode.Next.Next, new CustomLine(vertex.Point, nextEllipse.Position));
            FigureShapes.Remove(line);

            FigureVertices.AddAfter(FigureVertices.Find(FindVertexWithValue(previousEllipse.Position)), vertex);

            VertexNumber++;

            if (vertex.Point.X + 5 > MaxX) MaxX = vertex.Point.X + 5;
            if (vertex.Point.X - 5 < MinX) MinX = vertex.Point.X - 5;
            if (vertex.Point.Y + 5 > MaxY) MaxY = vertex.Point.Y + 5;
            if (vertex.Point.Y - 5 < MinY) MinY = vertex.Point.Y - 5;
        }
        /// <summary>
        /// Gets the previous vertex.
        /// </summary>
        /// <param name="vertex">The vertex.</param>
        /// <returns></returns>
        public CustomVertex GetPreviousVertex(CustomVertex vertex)
        {
            return FigureVertices.Find(vertex).Previous.Value;
        }
        /// <summary>
        /// Adds the vertex to the newly constructed figure.
        /// </summary>
        /// <param name="point">The point.</param>
        public void AddVertex(Point point)
        {
            // We don't want to add next vertex too close to an existing one
            if (FigureVertices.Any(
                vertex => Math.Abs(vertex.Point.X - point.X) < 10 && Math.Abs(vertex.Point.Y - point.Y) < 10))
                return;

            FigureVertices.AddLast(new CustomVertex(point));
            FigureShapes.AddLast(new CustomEllipse(point));
            int delta = VertexSize / 2;
            if (point.X + delta > MaxX) MaxX = point.X + delta;
            if (point.X - delta < MinX) MinX = point.X - delta;
            if (point.Y + delta > MaxY) MaxY = point.Y + delta;
            if (point.Y - delta < MinY) MinY = point.Y - delta;
            VertexNumber++;
        }
        /// <summary>
        /// Deletes the vertex.
        /// </summary>
        /// <param name="vertex">The vertex.</param>
        public void DeleteVertex(CustomVertex vertex)
        {
            FigureVertices.Remove(vertex);
            VertexNumber--;

            if (FigureShapes.Last.GetType() == typeof(CustomEllipse))
                FigureShapes.Remove(FigureShapes.Last);
        }
        #endregion Public Methods
        #region Private Methods
        /// <summary>
        /// Finds the vertex with value.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>First vertes with the specified position or null if one does not exist</returns>
        private CustomVertex FindVertexWithValue(Point point)
        {
            return FigureVertices.FirstOrDefault(x => x.Point == point);
        }
        #endregion Private Methods
    }
}
