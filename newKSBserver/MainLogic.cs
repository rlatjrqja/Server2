using ServerObject;

namespace newKSBserver
{
    internal class MainLogic
    {
        static void Main(string[] args)
        {
            string ip = "192.168.45.232";
            int port = 50000;

            Server_KSB server_ = new Server_KSB(ip, port);
            server_.StartServer();
        }
    }
}
