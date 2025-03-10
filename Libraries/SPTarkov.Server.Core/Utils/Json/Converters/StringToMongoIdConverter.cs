using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Utils.Json.Converters
{
    public class StringToMongoIdConverter : JsonConverter<MongoId>
    {
        public override MongoId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return new MongoId(reader.GetString());
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, MongoId mongoId, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, mongoId.ToString(), options);
        }

        // Deserialize MongoId as a dictionary key
        public override MongoId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {

            return new MongoId(reader.GetString());
        }

        // Serialize MongoId as a dictionary key
        public override void WriteAsPropertyName(Utf8JsonWriter writer, MongoId value, JsonSerializerOptions options)
        {
            writer.WritePropertyName(value.ToString());
        }
    }
}
