using System.Text.Json;
using System.Text.Json.Serialization;

namespace CateringEcommerce.API.Converter
{
    public class NullableDecimalConverter : JsonConverter<decimal?>
    {
        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                if (string.IsNullOrWhiteSpace(value))
                    return null;

                if (decimal.TryParse(value, out var result))
                    return result;
            }

            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetDecimal();

            return null;
        }

        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value ?? 0);
        }
    }
}
