using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.Settings.JsonConverters;

public class IntPtrJsonConverter : JsonConverter<IntPtr?>
{

    public class NonNullable : JsonConverter<IntPtr>
    {
        static readonly IntPtrJsonConverter converter = new();
        public override IntPtr Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => converter.Read(ref reader, typeToConvert, options) ?? default;
        public override void Write(Utf8JsonWriter writer, IntPtr value, JsonSerializerOptions options) => converter.Write(writer, value, options);
    }

    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert == typeof(IntPtr))
            return true;
        return base.CanConvert(typeToConvert);
    }

    public override IntPtr? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.Number
        ? (IntPtr)reader.GetInt32()
        : default;

    public override void Write(Utf8JsonWriter writer, IntPtr? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.ToString());
    }

}
