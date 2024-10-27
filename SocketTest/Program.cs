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
            string filename = @"..\..\..\..\SendDir\Dummy2.txt";
            string fullPath = Path.GetFullPath(filename);
            // 서버에 접속한다.
            string ip = "172.18.27.199";
            int port = 50000;
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ip), port);
            // FileInfo 생성
            var file = new FileInfo(fullPath);

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
                        {
                            byte[] request = protocol.StartConnectionRequest();
                            client.Send(request);
                            Console.WriteLine($"서버 접속 요청 [Length]:{request.Length}");

                            byte[] response = new byte[1024];
                            client.Receive(response);

                            
                            string response_s = Encoding.UTF8.GetString(response);
                            Console.WriteLine($"서버 응답 수신 완료! {response_s}");

                            uint OPCODE = BitConverter.ToUInt32(response, 1);
                            if (OPCODE != 000)
                            {
                                Console.WriteLine("서버 접속 거부");
                                return;
                            }
                        }


                        {
                            // 파일을 IO로 읽어온다.
                            stream.Read(binary, 0, binary.Length);
                            byte[] request = protocol.TransmitFileRequest(file.Name.Length, file.Name);
                            client.Send(request);
                            Console.WriteLine("파일 전송 가능 상태 확인");
                        }

                        
                        
                        // 파일 이름 크기를 보낸다.
                        client.Send(BitConverter.GetBytes(file.Name.Length));
                        Console.WriteLine(file.Name.Length);

                        // 파일 이름을 보낸다.
                        client.Send(Encoding.UTF8.GetBytes(file.Name));
                        Console.WriteLine(file.Name);

                        // 파일 크기를 보낸다.
                        client.Send(BitConverter.GetBytes((long)binary.Length));
                        Console.WriteLine(binary.Length);

                        // 파일을 보낸다.
                        client.Send(binary);
                        Console.WriteLine(binary.Length);

                        while (true)
                        {
                            byte[] response = new byte[1024];
                            client.Receive(response);
                            Console.WriteLine(BitConverter.ToInt32(response));
                            switch (BitConverter.ToInt32(response))
                            {
                                
                                case 100:
                                    Console.WriteLine("파일 전송 성공");
                                    client.Send(BitConverter.GetBytes(400));
                                    break;
                                case 101:
                                    Console.WriteLine("파일명으로 인한 전송 실패");
                                    break;
                                case 102:
                                    Console.WriteLine("파일로 인한 전송 실패");
                                    break;
                                case 500:                                    
                                    return;
                                default:
                                    Console.WriteLine("디버깅");
                                    break;
                            }

                        }
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