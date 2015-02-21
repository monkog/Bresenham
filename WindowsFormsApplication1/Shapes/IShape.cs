using System.Drawing;

namespace WindowsFormsApplication1.Shapes
{
    /// <summary>
    /// Interface representing all shapes that can be drawn
    /// </summary>
    public interface IShape
    {
        void Draw(Graphics graphics, Color color, int thickness);
    }
}
