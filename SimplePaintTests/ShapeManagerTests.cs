using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimplePaint;

namespace SimplePaintTests
{
	[TestClass]
	public class ShapeManagerTests
	{
		[TestMethod]
		public void Ctor_NoParams_PropertiesInitialized()
		{
			var unitUnderTest = new ShapeManager();

			Assert.IsNotNull(unitUnderTest.Figures);
			Assert.IsFalse(unitUnderTest.Figures.Any());
		}
	}
}
