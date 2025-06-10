using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HoaDonDienTu.Helper
{
    /// <summary>
    /// Handles flexible decimal conversion from API - supports scientific notation, strings, numbers, nulls
    /// </summary>
    public class FlexibleDecimalConverter : JsonConverter<decimal?>
    {
        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                    return reader.GetDecimal();

                case JsonTokenType.String:
                    var stringValue = reader.GetString();
                    if (string.IsNullOrEmpty(stringValue))
                        return null;

                    // Handle scientific notation like "2.0230749E8"
                    if (decimal.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                        return result;
                    return null;

                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.True:
                    return 1;

                case JsonTokenType.False:
                    return 0;

                default:
                    return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteNumberValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }

    /// <summary>
    /// Handles flexible integer conversion from API
    /// </summary>
    public class FlexibleIntConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                    return reader.GetInt32();

                case JsonTokenType.String:
                    var stringValue = reader.GetString();
                    if (string.IsNullOrEmpty(stringValue))
                        return null;

                    if (int.TryParse(stringValue, out var result))
                        return result;
                    return null;

                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.True:
                    return 1;

                case JsonTokenType.False:
                    return 0;

                default:
                    return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteNumberValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }

    /// <summary>
    /// Serializes complex objects/arrays to JSON string for database storage
    /// </summary>
    public class JsonStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString();

                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.StartObject:
                case JsonTokenType.StartArray:
                    // For complex objects/arrays, serialize them to JSON string
                    using (var document = JsonDocument.ParseValue(ref reader))
                    {
                        return document.RootElement.GetRawText();
                    }

                case JsonTokenType.Number:
                    return reader.GetDecimal().ToString(CultureInfo.InvariantCulture);

                case JsonTokenType.True:
                    return "true";

                case JsonTokenType.False:
                    return "false";

                default:
                    return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            if (value == null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value);
        }
    }

    /// <summary>
    /// Handles flexible string conversion - converts any JSON type to string
    /// </summary>
    public class FlexibleStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString();

                case JsonTokenType.Number:
                    return reader.GetDecimal().ToString(CultureInfo.InvariantCulture);

                case JsonTokenType.True:
                    return "true";

                case JsonTokenType.False:
                    return "false";

                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.StartObject:
                case JsonTokenType.StartArray:
                    // For complex objects, return JSON string
                    using (var document = JsonDocument.ParseValue(ref reader))
                    {
                        return document.RootElement.GetRawText();
                    }

                default:
                    return reader.GetString();
            }
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            if (value == null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value);
        }
    }

    /// <summary>
    /// Converts boolean values to string representation for database storage
    /// </summary>
    public class BoolToStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.True:
                    return "true";

                case JsonTokenType.False:
                    return "false";

                case JsonTokenType.String:
                    var stringValue = reader.GetString();
                    if (bool.TryParse(stringValue, out var boolResult))
                        return boolResult.ToString().ToLower();
                    return stringValue;

                case JsonTokenType.Number:
                    var numberValue = reader.GetInt32();
                    return (numberValue != 0).ToString().ToLower();

                case JsonTokenType.Null:
                    return null;

                default:
                    return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            if (value == null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value);
        }
    }
}
