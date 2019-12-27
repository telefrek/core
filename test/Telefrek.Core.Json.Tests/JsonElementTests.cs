using System;
using Xunit;
using Telefrek.Core.Json;

namespace Telefrek.Core.Json.Tests
{
    public class JsonElementTests
    {
        [Fact]
        public void PrimitivesCreatedFromRawObjects()
        {
            JsonElement element = null;
            element = "hello world";
            Assert.NotNull(element);
            Assert.True(element is JsonString);
            element = 1;
            Assert.NotNull(element);
            Assert.True(element is JsonNumber);
            element = 2L;
            Assert.NotNull(element);
            Assert.True(element is JsonNumber);
            element = 3d;
            Assert.NotNull(element);
            Assert.True(element is JsonDouble);
            element = true;
            Assert.NotNull(element);
            Assert.True(element is JsonBool);
            Assert.NotNull(JsonNull.Instance);
            element = JsonNull.Instance;
            Assert.NotNull(element);
        }

        [Fact]
        public void JsonElementSerializationTests()
        {
            var sample = "{\"obj1\":{\"name\":\"test\",\"int\":2,\"float\":1.234},\"arr\":[-1,0,1,2,3],\"missing\":null}";
            var pretty = "{\n\t\"obj1\" : {\n\t\t\"name\" : \"test\",\n\t\t\"int\" : 2,\n\t\t\"float\" : 1.234\n\t},\n\t\"arr\" : [\n\t-1,\n\t0,\n\t1,\n\t2,\n\t3\n\t],\n\t\"missing\" : null\n}";

            var test1 = sample.AsJson();
            var test2 = pretty.AsJson();

            var samplePretty = test1.ToJson(true);
            var prettySimple = test2.ToJson();

            Assert.Equal(sample, prettySimple);
            Assert.Equal(pretty, samplePretty);
        }
    }
}
