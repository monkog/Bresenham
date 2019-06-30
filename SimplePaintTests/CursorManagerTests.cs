using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimplePaint;

namespace SimplePaintTests
{
	[TestClass]
	public class CursorManagerTests
	{
		[TestMethod]
		public void Ctor_NoParams_PropertiesInitialized()
		{
			var unitUnderTest = new CursorManager();

			Assert.IsNotNull(unitUnderTest.BucketCursor);
			Assert.IsNotNull(unitUnderTest.PenCursor);
		}
	}
}
