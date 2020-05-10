using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JoyMoe.Common.Json
{
    /// <summary>
    /// ISO 8601 Zulu Time JsonConverter for DateTimeOffset
    /// </summary>
    public class IsoDateTimeConverter : JsonConverter<DateTimeOffset>
    {
        /// <inheritdoc/>
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(DateTimeOffset));
            return DateTimeOffset.Parse(reader.GetString());
        }
        
        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToUniversalTime().ToString("O"));
        }
    }
}
