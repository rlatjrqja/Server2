using TCP_ServerLib;

namespace Lecture02
{
    internal class Root
    {
        static void Main(string[] args)
        {
            _ = new TCP_Server_ksb("192.168.45.5", 50000);
        }
    }
}