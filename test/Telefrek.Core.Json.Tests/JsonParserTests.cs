using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Telefrek.Core.Json.Tests
{
    public class JsonParserTests
    {
        [Fact]
        public async Task TestParseSimpleObject()
        {
            var jsonValue = "{\"prop1\":\"test\"}";

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonValue)))
            {
                var element = await ms.AsJsonAsync().ConfigureAwait(false);
                Assert.NotNull(element);
                Assert.True(element.IsJsonObject());
                var props = new Dictionary<string, JsonElement>();
                foreach (var prop in element.AsJsonObject().Properties)
                    props.Add(prop.Name, prop.Value);

                Assert.Single(props);
                Assert.Equal("test", props["prop1"].AsString());
            }
        }

        [Fact]
        public async Task TestParseObjectWithNestedArray()
        {
            var jsonValue = "{\"prop1\":[\"test\"]}";

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonValue)))
            {
                var element = await ms.AsJsonAsync().ConfigureAwait(false);
                Assert.NotNull(element);
                Assert.True(element.IsJsonObject());
                var props = new Dictionary<string, JsonElement>();
                foreach (var prop in element.AsJsonObject().Properties)
                    props.Add(prop.Name, prop.Value);

                Assert.Single(props);
                Assert.True(props["prop1"].IsJsonArray());
                Assert.Single(props["prop1"].AsJsonArray().Items);
                Assert.Equal("test", props["prop1"].AsJsonArray().Items[0].AsString());
            }
        }

        [Fact]
        public async Task TestParseSimpleArray()
        {
            var jsonValue = "[\"test\"]";

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonValue)))
            {
                var element = await ms.AsJsonAsync().ConfigureAwait(false);
                Assert.NotNull(element);
                Assert.True(element is JsonArray);
                Assert.Equal("test", element.AsJsonArray().Items[0].AsString(), false, false, false);
            }
        }

        [Fact]
        public async Task TestParseMixedArray()
        {
            var jsonValue = "[\"test\", 10.1, null]";

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonValue)))
            {
                var element = await ms.AsJsonAsync().ConfigureAwait(false);
                Assert.NotNull(element);
                Assert.True(element.IsJsonArray());
                Assert.Equal(3, element.AsJsonArray().Items.Count);
                Assert.Equal("test", element.AsJsonArray().Items[0].AsString(), false, false, false);
                Assert.Equal(10.1d, element.AsJsonArray().Items[1].AsDouble());
                Assert.Equal(JsonNull.Instance, element.AsJsonArray().Items[2]);
            }
        }


        [Fact]
        public async Task TestParseNestedArray()
        {
            var jsonValue = "[\"test\"\t, 10.1, null     , [\"two\"]\r\n\n]";

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonValue)))
            {
                var element = await ms.AsJsonAsync().ConfigureAwait(false);
                Assert.NotNull(element);
                Assert.True(element.IsJsonArray());
                Assert.Equal(4, element.AsJsonArray().Items.Count);
                Assert.Equal("test", element.AsJsonArray().Items[0].AsString(), false, false, false);
                Assert.Equal(10.1d, element.AsJsonArray().Items[1].AsDouble());
                Assert.Equal(JsonNull.Instance, element.AsJsonArray().Items[2]);
                Assert.Single(element.AsJsonArray().Items[3].AsJsonArray().Items);
                Assert.Equal("two", element.AsJsonArray().Items[3].AsJsonArray().Items[0].AsString(), false, false, false);
            }
        }

        [Fact]
        public async Task TestParseString()
        {
            var jsonValue = "\"test\"";

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonValue)))
            {
                var element = await ms.AsJsonAsync().ConfigureAwait(false);
                Assert.NotNull(element);
                Assert.True(element is JsonString);
                Assert.Equal("test", element.AsString(), false, false, false);
            }
        }

        [Fact]
        public async Task TestParseLong()
        {
            var jsonValue = "100";

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonValue)))
            {
                var element = await ms.AsJsonAsync().ConfigureAwait(false);
                Assert.NotNull(element);
                Assert.True(element is JsonNumber);
                Assert.Equal(100L, element.AsLong());
            }
        }


        [Fact]
        public async Task TestParseBoolean()
        {
            var jsonValue = "true";

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonValue)))
            {
                var element = await ms.AsJsonAsync().ConfigureAwait(false);
                Assert.NotNull(element);
                Assert.True(element is JsonBool);
                Assert.True(element.AsBool());
            }

            jsonValue = "false";

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonValue)))
            {
                var element = await ms.AsJsonAsync().ConfigureAwait(false);
                Assert.NotNull(element);
                Assert.True(element is JsonBool);
                Assert.False(element.AsBool());
            }
        }

        [Fact]
        public async Task TestParseNull()
        {
            var jsonValue = "null";

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonValue)))
            {
                var element = await ms.AsJsonAsync().ConfigureAwait(false);
                Assert.NotNull(element);
                Assert.True(element is JsonNull);
                Assert.Equal(JsonNull.Instance, element);
            }
        }

        [Fact]
        public async Task TestParseDouble()
        {
            var jsonValue = "10.01";

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonValue)))
            {
                var element = await ms.AsJsonAsync().ConfigureAwait(false);
                Assert.NotNull(element);
                Assert.True(element is JsonDouble);
                Assert.Equal(10.01d, element.AsDouble());
            }
        }

        [Fact]
        public async Task TestParseStringWithEscaped()
        {
            var jsonValue = "\"\\\"test\\\"\"";

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonValue)))
            {
                var element = await ms.AsJsonAsync().ConfigureAwait(false);
                Assert.NotNull(element);
                Assert.True(element is JsonString);
                Assert.Equal("\\\"test\\\"", element.AsString(), false, false, false);
            }
        }


        [Fact]
        public async Task TestParseWhitespaceStringWithEscaped()
        {
            var jsonValue = "\t\"\\\"test\\\"\"";

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonValue)))
            {
                var element = await ms.AsJsonAsync().ConfigureAwait(false);
                Assert.NotNull(element);
                Assert.True(element is JsonString);
                Assert.Equal("\\\"test\\\"", element.AsString(), false, false, false);
            }
        }
    }
}