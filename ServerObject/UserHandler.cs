using System.Net.Sockets;
using System.Text;

namespace ServerObject
{
    public class UserHandler
    {
        public Socket client;
        string UserName = "Unknown";


        public UserHandler(Socket socket)
        {
            client = socket;
            Task.Run(ReceiveString);
        }

        public void ReceiveString()
        {
            while (client.Connected)
            {
                byte[] data = new byte[1024];
                int length = client.Receive(data);
                if (length <= 0)
                {
                    Console.WriteLine($"{client.RemoteEndPoint} 접속 종료");
                    client.Disconnect(false);
                    continue;
                }

                string msg = Encoding.UTF8.GetString(data);
                Console.Write(msg);
            }
        }
    }
}
