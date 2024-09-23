using System.Net.Sockets;
using System.Net;

namespace TCP_ServerLib
{
    public class TCP_Server_ksb
    {
        public static readonly object lockObj = new object();
        static List<ConnectedClient> users = new List<ConnectedClient>();
        Socket server_host;

        public TCP_Server_ksb(string address,int port)
        {
            IPEndPoint serverIP = new IPEndPoint(IPAddress.Parse(address), port);
            server_host = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            StartServer(serverIP);
        }

        private void StartServer(IPEndPoint ip)
        {
            Console.WriteLine("서버를 시작합니다.");
            server_host.Bind(ip);
            server_host.Listen();

            while (true)
            {
                Socket socket = server_host.Accept();
                Connect(socket);
            }
        }

        void Connect(Socket host)
        {
            Console.WriteLine($"새로운 클라이언트 연결.");
            ConnectedClient client = new ConnectedClient(host);
            users.Add(client);
            CountUsers();
        }

        public static void Disconnect(ConnectedClient socket)
        {
            Console.WriteLine($"클라이언트 연결 종료.");
            users.Remove(socket);
            CountUsers();
        }

        public static void CountUsers()
        {
            Console.WriteLine($"현재 접속자 수: {users.Count}");
        }
    }
}
