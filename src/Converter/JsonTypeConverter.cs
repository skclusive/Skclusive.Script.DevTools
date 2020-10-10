using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Skclusive.Script.DevTools
{
    public class JsonTypeConverter<I, T> : JsonConverter<I>
        where T : class, I
    {
        public override I Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<T>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, I value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, typeof(T), options);
        }
    }
}
