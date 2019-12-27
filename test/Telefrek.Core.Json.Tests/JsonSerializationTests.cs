using System;
using Telefrek.Core.Json.Serialization;
using Xunit;

namespace Telefrek.Core.Json.Tests
{
    public class JsonSerializationTests
    {
        // Setup the serialization
        static JsonSerializationTests() { JsonSerializationFactory.TryRegister(new SimpleObjectSerializer()); }

        [Fact]
        public void SimpleObjectSerialized()
        {
            var obj = new SimpleObject { Name = "test", Created = new DateTime(2020, 01, 01, 0, 0, 0, DateTimeKind.Utc), IsActive = true };
            var expected = "{\"name\":\"test\",\"created\":\"2020-01-01T00:00:00.0000000Z\",\"active\":true}";

            var element = obj.AsJson();
            Assert.NotNull(element);
            Assert.True(element.IsJsonObject());
            Assert.Equal(3, element.AsJsonObject().Properties.Count);
            Assert.True(element.AsJsonObject().Has("name"));

            var str = obj.ToJson();
            Assert.Equal(expected, str);

            var obj2 = expected.FromJson<SimpleObject>();
            Assert.NotNull(obj2);
            Assert.Equal(obj.Name, obj2.Name);
            Assert.Equal(obj.Created, obj2.Created);
            Assert.Equal(obj.IsActive, obj2.IsActive);
        }
    }

    class SimpleObject
    {
        public string Name { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; }
    }

    class SimpleObjectSerializer : IJsonSerializable<SimpleObject>
    {
        public SimpleObject Deserialize(JsonElement element)
        {
            if (element != null && element.IsJsonObject())
            {
                var instance = new SimpleObject();

                foreach (var prop in element.AsJsonObject().Properties)
                {
                    if (prop.Value == null || prop.Value.IsJsonNull()) continue;
                    switch (prop.Name)
                    {
                        case "name":
                            instance.Name = prop.Value.AsString();
                            break;
                        case "created":
                            instance.Created = DateTime.Parse(prop.Value.AsString()).ToUniversalTime();
                            break;
                        case "active":
                            instance.IsActive = prop.Value.AsBool();
                            break;
                    }
                }

                return instance;
            }

            return null;
        }

        public JsonElement Serialize(SimpleObject instance)
        {
            if (instance == null) return JsonNull.Instance;

            var element = new JsonObject();
            element.Add("name", instance.Name);
            element.Add("created", instance.Created.ToString("o"));
            element.Add("active", instance.IsActive);

            return element;
        }
    }
}