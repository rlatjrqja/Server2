using ServerObject;

namespace newKSBserver
{
    internal class MainLogic
    {
        static void Main(string[] args)
        {
            string ip = "0.0.0.0";
            int port = 50000;

            Server_KSB server_ = new Server_KSB(ip, port);
            server_.StartServer();
        }
    }
}
