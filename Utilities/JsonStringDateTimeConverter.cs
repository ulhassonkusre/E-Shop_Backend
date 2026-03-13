using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EcommerceBackend.Utilities;

/// <summary>
/// Custom JSON converter that ensures DateTime is serialized as UTC with 'Z' suffix.
/// This ensures the frontend receives timestamps in UTC format and can convert to local time.
/// </summary>
public class JsonStringDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            return DateTime.MinValue;
        }

        // Parse the date and ensure it's in UTC
        if (DateTime.TryParse(value, out var result))
        {
            return result.Kind == DateTimeKind.Utc ? result : result.ToUniversalTime();
        }

        return DateTime.MinValue;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Always write as UTC with 'Z' suffix
        if (value.Kind != DateTimeKind.Utc)
        {
            value = value.ToUniversalTime();
        }
        
        writer.WriteStringValue(value.ToString("O")); // ISO 8601 format with 'Z'
    }
}
