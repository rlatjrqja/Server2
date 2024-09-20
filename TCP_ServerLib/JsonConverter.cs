using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TCP_ServerLib
{
    internal class JsonConverter
    {
        public static string? GetOneJson(string data)
        {
            int startIndex = data.IndexOf('{');
            int endIndex = data.IndexOf('}');
            if (startIndex < 0 || endIndex < 0) return null;

            string message = data.Substring(startIndex, endIndex);
            return message;
        }

        public static JsonDocument? StringToJson(string data)
        {
            try
            {
                JsonDocument jsonDocument = JsonDocument.Parse(data);
                return jsonDocument;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Invalid JSON received: {ex.Message}");
                return null;
            }
        }
    }
}
