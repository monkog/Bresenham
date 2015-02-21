using System;
using System.Collections.Generic;
using System.Drawing;

namespace WindowsFormsApplication1.Shapes
{

    public class CustomFigure
    {
        public CustomFigure(Point point, Color color, int strokeThickness)
        {
            FigureColor = color;
            StrokeThickness = strokeThickness;
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

        public void AddInbetweenVertex(CustomVertex vertex, CustomLine line)
        {
            LinkedListNode<IShape> previousNode = FigureShapes.Find(line).Previous;
            LinkedListNode<IShape> nextNode = FigureShapes.Find(line).Next;

            CustomEllipse previousEllipse = previousNode.Value as CustomEllipse;
            CustomEllipse nextEllipse;

            if (nextNode == null)
                nextEllipse = FigureShapes.First.Value as CustomEllipse;
            else
                nextEllipse = nextNode.Value as CustomEllipse;

            if (((Math.Abs(vertex.Point.X - previousEllipse.Point.X) < 10 && Math.Abs(vertex.Point.Y - previousEllipse.Point.Y) < 10)
                || (Math.Abs(vertex.Point.X - nextEllipse.Point.X) < 10) && Math.Abs(vertex.Point.Y - nextEllipse.Point.Y) < 10))
                return;

            FigureShapes.AddAfter(previousNode, new CustomLine(previousEllipse.Point, vertex.Point));
            FigureShapes.AddAfter(previousNode.Next, new CustomEllipse(vertex.Point));
            FigureShapes.AddAfter(previousNode.Next.Next, new CustomLine(vertex.Point, nextEllipse.Point));
            FigureShapes.Remove(line);

            FigureVertices.AddAfter(FigureVertices.Find(FindVertexWithValue(previousEllipse.Point)), vertex);

            VertexNumber++;

            if (vertex.Point.X + 5 > MaxX) MaxX = vertex.Point.X + 5;
            if (vertex.Point.X - 5 < MinX) MinX = vertex.Point.X - 5;
            if (vertex.Point.Y + 5 > MaxY) MaxY = vertex.Point.Y + 5;
            if (vertex.Point.Y - 5 < MinY) MinY = vertex.Point.Y - 5;
        }

        public void AddVertex(Point point)
        {
            foreach (CustomVertex vertex in FigureVertices)
                if (Math.Abs(vertex.Point.X - point.X) < 10 && Math.Abs(vertex.Point.Y - point.Y) < 10)
                    return;

            FigureVertices.AddLast(new CustomVertex(point));
            FigureShapes.AddLast(new CustomEllipse(point));
            if (point.X + 5 > MaxX) MaxX = point.X + 5;
            if (point.X - 5 < MinX) MinX = point.X - 5;
            if (point.Y + 5 > MaxY) MaxY = point.Y + 5;
            if (point.Y - 5 < MinY) MinY = point.Y - 5;
            VertexNumber++;
        }

        public void DeleteVertex(CustomVertex vertex)
        {
            FigureVertices.Remove(vertex);
            VertexNumber--;

            if (FigureShapes.Last.GetType() == typeof(CustomEllipse))
                FigureShapes.Remove(FigureShapes.Last);
        }

        private CustomVertex FindVertexWithValue(Point point)
        {
            foreach (CustomVertex vertex in FigureVertices)
                if (vertex.Point == point)
                    return vertex;
            return null;
        }

        public LinkedListNode<CustomVertex> GetFirstNode()
        {
            return FigureVertices.First;
        }

        public LinkedListNode<CustomVertex> GetLastNode()
        {
            return FigureVertices.Last;
        }

        public CustomVertex GetFirstVertex()
        {
            return FigureVertices.First.Value;
        }

        public CustomVertex GetLastVertex()
        {
            return FigureVertices.Last.Value;
        }

        public CustomVertex GetPreviousVertex(CustomVertex vertex)
        {
            return FigureVertices.Find(vertex).Previous.Value;
        }

        public Color FigureColor { get; set; }
        public int StrokeThickness { get; set; }
        public int MaxX { get; set; }
        public int MaxY { get; set; }
        public int MinX { get; set; }
        public int MinY { get; set; }
        public int VertexNumber { get; set; }
        public LinkedList<IShape> FigureShapes;
        public LinkedList<CustomVertex> FigureVertices;
        public CustomLine MultisamplingLine { get; set; }
        public Color MultisamplingColor { get; set; }
    }
}
