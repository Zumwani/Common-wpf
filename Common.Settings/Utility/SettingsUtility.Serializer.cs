using Common.Settings.JsonConverters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.Settings.Utility;

public static partial class SettingsUtility
{

    /// <summary>Provides properties and methods relating to serialization.</summary>
    public static class Serializer
    {

        static Serializer()
        {

            EnableDefaultConverter<RectJsonConverter.NonNullable>();
            EnableDefaultConverter<BitmapSourceJsonConverter.NonNullable>();
            EnableDefaultConverter<IntPtrJsonConverter.NonNullable>();

            EnableDefaultConverter<RectJsonConverter>();
            EnableDefaultConverter<BitmapSourceJsonConverter>();
            EnableDefaultConverter<IntPtrJsonConverter>();

        }

        /// <summary>The <see cref="JsonSerializerOptions"/> to use when serializing.</summary>
        public static JsonSerializerOptions JsonOptions { get; } = new();

        /// <summary>Gets if a default converter is enabled.</summary>
        public static bool IsDefaultConverterEnabled<T>() where T : JsonConverter, new() =>
            JsonOptions.Converters.OfType<T>().Any();

        /// <summary>Enables a default converter.</summary>
        /// <param name="isEnabled">Determines whatever the converter should be enabled. Setting to <see langword="false"/> has same effect as <see cref="DisableDefaultConverter{T}"/>.</param>
        public static void EnableDefaultConverter<T>(bool isEnabled = true) where T : JsonConverter, new()
        {
            _ = JsonOptions.Converters.Remove(JsonOptions.GetConverter(typeof(T)));
            if (isEnabled)
                JsonOptions.Converters.Add(new T());
        }

        /// <summary>Disables a default converter.</summary>
        public static void DisableDefaultConverter<T>() where T : JsonConverter, new() =>
            EnableDefaultConverter<T>(isEnabled: false);

        /// <summary>Serialize <paramref name="value"/> into json.</summary>
        public static string Serialize<T>(T? value) =>
            value as string ?? JsonSerializer.Serialize(value, JsonOptions);

        /// <summary>Deserialize <paramref name="json"/> into <typeparamref name="T"/>.</summary>
        public static T? Deserialize<T>(string json) =>
            JsonSerializer.Deserialize<T>(json, JsonOptions);

    }

}
