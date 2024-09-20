using System.Net.Sockets;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text;

namespace TCP_ServerLib
{
    public class ConnectedClient
    {
        Socket client;
        DateTime receiveAt;
        DateTime sendAt;
        string? message_queue = null;

        public ConnectedClient(Socket socket)
        {
            client = socket;
            Thread th = new Thread(MulithreadingClient);
            th.Start();
        }

        private void MulithreadingClient()
        {
            try
            {
                while (client.Connected)
                {
                    while(message_queue != null)
                    {
                        receiveAt = DateTime.Now;
                        string? data = JsonConverter.GetOneMessage(ref message_queue);

                        // json이 끝나지 않았을 경우 ( }가 없는 등 이유로 )
                        if (data == null) break;

                        JsonDocument? json = JsonConverter.StringToJson(data);

                        // json에 문제가 있는 경우 ( ,가 없는 등 이유로 )
                        if (json == null) break;
                        PrintConsole(json);

                        sendAt = DateTime.Now;
                        Send(json);
                    }

                    Receive();
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"Client Abnormal Termination");
            }
            finally
            {
                client.Close();
                TCP_Server_ksb.Disconnect(this);
            }
        }

        void Receive()
        {
            byte[] buffer = new byte[1024];
            int length = client.Receive(buffer);
            if(length <= 0) return;

            message_queue = Encoding.UTF8.GetString(buffer, 0, length);
        }

        private void PrintConsole(JsonDocument json)
        {
            JsonElement root = json.RootElement;
            foreach (JsonProperty prop in root.EnumerateObject())
            {
                Console.WriteLine($"{prop.Name}: {prop.Value}");
            }
        }

        private void Send(JsonDocument json)
        {
            string message = JsonConverter.JsonToString(json,sendAt,receiveAt);
            message += "\r\n";
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            client.Send(buffer);
            Console.WriteLine("JSON 데이터 전송 완료.");
        }
    }
}