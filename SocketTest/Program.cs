using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Protocols;

namespace FileClient
{
    class Program
    {


        static void Main(string[] args)
        {
            Protocol protocol = new Protocol();
            // 업로드 할 파일
            string filename = @"C:\YUHAN\FTPTEST.txt";
            // 서버에 접속한다.
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("192.168.45.232"), 50000);
            // FileInfo 생성
            var file = new FileInfo(filename);

            // 파일이 존재하는지
            if (file.Exists)
            {
                // 바이너리 버퍼
                var binary = new byte[file.Length];
                // 파일 IO 생성
                using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                {
                    // 소켓 생성
                    using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))

                    {
                        // 접속
                        client.Connect(ipep);
                        // 비동기 수신 시작
                        // Task.Run(() => { ReceiveAlways(client); });

                        byte[] request = protocol.StartConnectionRequest();
                        client.Send(request);

                        byte[] response = new byte[1024];
                        client.Receive(response);
                        string opcode = Encoding.UTF8.GetString(response);
                        Console.WriteLine($"서버 응답 수신 완료! {opcode}");

                        if (opcode.Split(' ')[0] != "000") return;

                        // 파일을 IO로 읽어온다.
                        stream.Read(binary, 0, binary.Length);

                        // 파일 이름 크기를 보낸다.
                        client.Send(BitConverter.GetBytes(file.Name.Length));

                        // 파일 이름을 보낸다.
                        client.Send(Encoding.UTF8.GetBytes(file.Name));

                        // 파일 크기를 보낸다.
                        client.Send(BitConverter.GetBytes(binary.Length));

                        // 파일을 보낸다.
                        client.Send(binary);
                    }
                }
            }
            else
            {
                // 콘솔 출력
                Console.WriteLine("The file does not exist.");
            }
        }

        static void ReceiveAlways(Socket socket)
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (true)
                {
                    int bytesReceived = socket.Receive(buffer);
                    if (bytesReceived > 0)
                    {
                        var msg = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                        Console.Write(msg);
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"수신 중 오류 발생: {ex.Message}");
            }
        }

    }
}