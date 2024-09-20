using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace TCP_ServerLib
{
    internal class JsonConverter
    {
        public static string? GetOneMessage(ref string data)
        {
            int startIndex = data.IndexOf('{');
            int endIndex = data.IndexOf('}')+1;
            if (startIndex < 0 || endIndex < 0) return null;

            string message = data.Substring(startIndex, endIndex);

            data = data.Substring(endIndex);
            return message;
        }

        public static JsonDocument? StringToJson(string data)
        {
            JsonDocument jsonDocument;
            JsonElement jsonElement;

            try
            {
                jsonDocument = JsonDocument.Parse(data);
                jsonElement = jsonDocument.RootElement;

                // json이 올바른 형식인지 검사
                if ((jsonElement.ValueKind != JsonValueKind.Undefined) && (jsonElement.ValueKind != JsonValueKind.Null))
                {
                    return jsonDocument;
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Invalid JSON received: {ex.Message}");
            }

            return null;
        }

        public static string JsonToString(JsonDocument json, DateTime sendAt, DateTime receiveAt)
        {
            JsonElement root = json.RootElement;
            TimeSpan duration = sendAt - receiveAt;
            string time_text = $"{duration.Seconds}.{0:D2}{duration.Milliseconds}";

            string message;
            if (root.TryGetProperty("ID", out JsonElement identity))
            {
                var jsonObject = new JsonObject
                {
                    ["ID"] = identity.GetString(),
                    ["process_time"] = time_text
                };

                message = jsonObject.ToJsonString();
            }
            else
            {
                var jsonObject = new JsonObject
                {
                    ["ID"] = "ERROR",
                    ["process_time"] = time_text
                };

                message = jsonObject.ToJsonString();
            }

            return message;
        }
    }
}
