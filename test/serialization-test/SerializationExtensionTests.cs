using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Telefrek.Serialization
{
    /// <summary>
    /// Test serialization extension methods
    /// </summary>
    [TestClass]
    public class SerializationExtensionTests
    {
        [TestMethod]
        public void TestDefaultOrNull()
        {
            Assert.IsTrue(0.IsNullOrDefault(), "0 should be the default value for an int");
            Assert.IsFalse("".IsNullOrDefault(), "Empty string is not the default");
            Assert.IsTrue(false.IsNullOrDefault(), "False is default for boolean");
            Assert.IsTrue(default(string).IsNullOrDefault(), "Lol, null is definitely null");
            Assert.IsTrue(default(decimal).IsNullOrDefault(), "should be the default for decimal");
            var i = (int?)null;
            Assert.IsTrue(i.IsNullOrDefault(), "Nullable value should be null");
        }
    }
}