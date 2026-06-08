using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nocturne.Core.Models.Serializers;

/// <summary>
/// JSON converter that handles flexible long (Int64) serialization for Nightscout compatibility.
/// Nightscout may send numeric values as either numbers or strings depending on the context.
/// </summary>
/// <remarks>
/// Also handles string values containing decimal notation (e.g., "1234567890.0") by parsing
/// as double and truncating. Returns 0 for null or unrecognized values.
/// </remarks>
/// <seealso cref="FlexibleNullableLongConverter"/>
public class FlexibleLongConverter : JsonConverter<long>
{
    public override long Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                if (reader.TryGetInt64(out var longVal))
                    return longVal;
                return (long)reader.GetDouble();

            case JsonTokenType.String:
                var stringValue = reader.GetString();
                if (string.IsNullOrWhiteSpace(stringValue))
                    return 0;

                if (long.TryParse(stringValue, out var result))
                    return result;

                // Try parsing as double and truncating (for values like "1234567890.0")
                if (double.TryParse(stringValue, out var doubleResult))
                    return (long)doubleResult;

                return 0;

            case JsonTokenType.Null:
                return 0;

            default:
                return 0;
        }
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}

/// <summary>
/// JSON converter that handles flexible nullable long (Int64?) serialization for Nightscout compatibility.
/// Nightscout may send numeric values as either numbers or strings depending on the context.
/// </summary>
/// <seealso cref="FlexibleLongConverter"/>
public class FlexibleNullableLongConverter : JsonConverter<long?>
{
    public override long? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                if (reader.TryGetInt64(out var longVal))
                    return longVal;
                return (long)reader.GetDouble();

            case JsonTokenType.String:
                var stringValue = reader.GetString();
                if (string.IsNullOrWhiteSpace(stringValue))
                    return null;

                if (long.TryParse(stringValue, out var result))
                    return result;

                // Try parsing as double and truncating (for values like "1234567890.0")
                if (double.TryParse(stringValue, out var doubleResult))
                    return (long)doubleResult;

                return null;

            case JsonTokenType.Null:
                return null;

            default:
                return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteNumberValue(value.Value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
