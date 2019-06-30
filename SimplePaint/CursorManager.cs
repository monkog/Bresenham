using System.Windows.Forms;
using SimplePaint.Properties;

namespace SimplePaint
{
	public class CursorManager
	{
		/// <summary>
		/// Gets the bucket cursor.
		/// </summary>
		public Cursor BucketCursor { get; }

		/// <summary>
		/// Gets the pen cursor.
		/// </summary>
		public Cursor PenCursor { get; }

		public CursorManager()
		{
			BucketCursor = new Cursor(Resources.bucket.GetHicon());
			PenCursor = new Cursor(Resources.pen.GetHicon());
		}
	}
}
