using System.Buffers;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace Common.Settings.JsonConverters;

public class BitmapSourceJsonConverter : JsonConverter<BitmapSource?>
{

    public class NonNullable : JsonConverter<BitmapSource>
    {
        static readonly BitmapSourceJsonConverter converter = new();
        public override BitmapSource Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => converter.Read(ref reader, typeToConvert, options) ?? throw new NullReferenceException("BitmapSource cannot be null.");
        public override void Write(Utf8JsonWriter writer, BitmapSource value, JsonSerializerOptions options) => converter.Write(writer, value, options);
    }

    public override BitmapSource? Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {

        if (reader.TokenType == JsonTokenType.String)
        {

            var span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
            if (span.Length == 0)
                return null;

            var base64 = Regex.Unescape(Encoding.UTF8.GetString(span));
            var bytes = Convert.FromBase64String(base64);
            using var stream = new MemoryStream(bytes);

            var img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.StreamSource = stream;
            img.EndInit();
            img.Freeze();

            return img;

        }

        return null;

    }

    public override void Write(Utf8JsonWriter writer, BitmapSource? bitmap, JsonSerializerOptions options)
    {

        if (bitmap is null)
            return;

        using var stream = new MemoryStream();
        var encoder = new PngBitmapEncoder();

        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(stream);

        var bytes = stream.ToArray();
        var base64 = Convert.ToBase64String(bytes);

        writer.WriteStringValue(base64);

    }

}
