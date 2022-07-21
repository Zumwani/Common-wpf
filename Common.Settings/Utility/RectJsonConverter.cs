﻿using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace Common.Settings.Utility;

/// <summary>
/// <para>Ensures that only the following properties are serialized for <see cref="Rect"/>:</para>
/// <para><see cref="Rect.Left"/></para>
/// <para><see cref="Rect.Top"/></para>
/// <para><see cref="Rect.Width"/></para>
/// <para><see cref="Rect.Height"/></para>
/// </summary>
public class RectJsonConverter : JsonConverter<Rect>
{

    public override Rect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("JSON payload expected to start with StartObject token.");

        var left = double.NaN;
        var top = double.NaN;
        var width = double.NaN;
        var height = double.NaN;

        var property = "";
        while (reader.Read())
        {

            if (reader.TokenType == JsonTokenType.EndObject)
                return double.IsNaN(left) || double.IsNaN(top) || double.IsNaN(width) || double.IsNaN(height)
                ? default
                : (new(left, top, width, height));

            if (reader.TokenType == JsonTokenType.PropertyName)
                property = reader.GetString();
            else if (reader.TokenType == JsonTokenType.Number)
            {
                if (property == nameof(Rect.Left)) left = reader.GetDouble();
                else if (property == nameof(Rect.Top)) top = reader.GetDouble();
                else if (property == nameof(Rect.Width)) width = reader.GetDouble();
                else if (property == nameof(Rect.Height)) height = reader.GetDouble();
            }
        }

        throw new JsonException();

    }

    public override void Write(Utf8JsonWriter writer, Rect value, JsonSerializerOptions options)
    {

        writer.WriteStartObject();

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(value));
        foreach (var prop in document.RootElement.EnumerateObject())
            if (prop.Name is nameof(Rect.Left) or nameof(Rect.Top) or nameof(Rect.Width) or nameof(Rect.Height))
                prop.WriteTo(writer);

        writer.WriteEndObject();

    }

}