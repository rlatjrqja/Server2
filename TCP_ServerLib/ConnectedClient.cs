using System.Net.Sockets;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text;

namespace TCP_ServerLib
{
    public class ConnectedClient
    {
        Socket client;
        DateTime time1;
        DateTime time2;
        string? message_queue = null;

        public ConnectedClient(Socket socket)
        {
            client = socket;
            Thread th = new Thread(MulithreadingClient);
            th.Start(socket);
        }

        private void MulithreadingClient()
        {
            try
            {
                while (client.Connected)
                {
                    JsonElement json = Receive(client);
                    if (!IsValidJson(json)) continue;

                    Print(json);
                    if (!Send(client, json)) break;
                }
            }
            finally
            {
                client.Close();
                TCP_Server_ksb.Disconnect(this);
            }
        }

        private bool IsValidJson(JsonElement json)
        {
            // JSON 데이터가 올바른 형식인지 확인
            return json.ValueKind != JsonValueKind.Undefined && json.ValueKind != JsonValueKind.Null;
        }

        JsonElement Receive(Socket socket)
        {
            byte[] buffer = new byte[1024];
            string? data = null;

            do
            {
                int length = socket.Receive(buffer);
                time1 = DateTime.Now;
                message_queue = Encoding.UTF8.GetString(buffer, 0, length);
                data = JsonConverter.GetOneJson(message_queue);
            } while (data == null);

            try
            {
                JsonDocument jsonDocument = JsonDocument.Parse(data);
                JsonElement rootElement = jsonDocument.RootElement;

                return rootElement;
            }
            catch (JsonException ex)
            {
                string errorJson = $"{{\"error\": \"Invalid JSON received\", " +
                    $"\"message\": \"{ex.Message}\"}}";
                JsonDocument errorDoc = JsonDocument.Parse(errorJson);
                JsonElement rootElement = errorDoc.RootElement;

                return rootElement;
            }
        }

        private void Print(JsonElement json)
        {
            foreach (JsonProperty prop in json.EnumerateObject())
            {
                Console.WriteLine($"{prop.Name}: {prop.Value}");
            }
        }

        private string Record()
        {
            time2 = DateTime.Now;
            TimeSpan duration = time2 - time1;
            string time = $"{duration.Seconds}.{0:D2}{duration.Milliseconds}";

            return time;
        }

        private bool Send(Socket socket, JsonElement json)
        {
            if (!json.TryGetProperty("ID", out JsonElement identity))
            {
                var jsonObject = new JsonObject
                {
                    ["ID"] = "ERROR",
                    ["process_time"] = Record()
                };

                string jsonString = jsonObject.ToJsonString();
                byte[] buffer = Encoding.UTF8.GetBytes(jsonString);

                socket.Send(buffer);
                Console.WriteLine("에러메세지 반환 완료.");

                return false;
            }
            else
            {
                var jsonObject = new JsonObject
                {
                    ["ID"] = identity.GetString(),
                    ["process_time"] = Record()
                };

                string jsonString = jsonObject.ToJsonString();
                byte[] buffer = Encoding.UTF8.GetBytes(jsonString);

                socket.Send(buffer);
                Console.WriteLine("JSON 데이터 전송 완료.");
                return true;
            }
        }
    }
}