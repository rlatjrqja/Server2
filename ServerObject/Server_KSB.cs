using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Text;
using Protocols;

namespace ServerObject
{
    public class Server_KSB
    {
        public static Server_KSB instance = null;
        Socket server_host;
        public List<UserHandler> users = new List<UserHandler>();
        public Action<byte[]> MessageReceive;

        public Server_KSB(string address, int port)
        {
            if(instance == null) instance = this;

            server_host = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // 이미 점유중인 포트인 경우 해결 필요함
            IPEndPoint serverIP = new IPEndPoint(IPAddress.Parse(address), port);
            server_host.Bind(serverIP);
        }

        public void StartServer()
        {
            server_host.Listen();
            Task.Run(AcceptUser);
            //MessageReceive += SendToAll;

            while (true)
            {
                List<UserHandler> disconnectedUsers = new List<UserHandler>();

                foreach (var user in users)
                {
                    if (!user.client.Connected) 
                        disconnectedUsers.Add(user);
                }
                foreach (var user in disconnectedUsers)
                {
                    user.client.Close();
                    users.Remove(user);
                }

                Console.WriteLine($"현재 접속자 수: {users.Count}");
                Thread.Sleep(5000);
            }
        }

        void AcceptUser()
        {
            while (true)
            {
                Socket host = server_host.Accept();
                UserHandler newUser = new(host);
                byte[] response = newUser.WaitRequest();
                newUser.client.Send(response);
                newUser.Listen();
            }
        }

        void SendToAll(byte[] data)
        {
            foreach (var user in users)
            {
                if(user.client.Connected)
                    user.client.Send(data);
            }
        }
    }
}
