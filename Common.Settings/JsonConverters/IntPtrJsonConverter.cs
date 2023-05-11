using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.Settings.JsonConverters;

/// <summary>A json converter that will serialize / deserialize <see cref="IntPtr"/>.</summary>
public class IntPtrJsonConverter : JsonConverter<IntPtr?>
{

    /// <summary>Json serializer does, at this time, require separate converters for nullable and non-nullable, this is the non-nullable version of <see cref="IntPtrJsonConverter"/>.</summary>
    public class NonNullable : JsonConverter<IntPtr>
    {

        static readonly IntPtrJsonConverter converter = new();

        /// <inheritdoc/>
        public override IntPtr Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => converter.Read(ref reader, typeToConvert, options) ?? default;

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, IntPtr value, JsonSerializerOptions options) => converter.Write(writer, value, options);

    }

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert == typeof(IntPtr) || base.CanConvert(typeToConvert);

    /// <inheritdoc/>
    public override IntPtr? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.Number
        ? (IntPtr)reader.GetInt32()
        : default;

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, IntPtr? value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value?.ToString());

}
