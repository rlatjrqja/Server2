using System.Net;
using System.Net.Sockets;
using System.Text;
using Protocols;

namespace ServerObject
{
    public class UserHandler
    {
        public Socket client;
        string UserName = "Unknown";
        Protocol protocol;
        int count = 0;

        public UserHandler(Socket socket)
        {
            client = socket;
            Task.Run(Listen);
            protocol = new Protocol();
        }

        public byte[] WaitConnectRequest()
        {
            bool connection = false;
            
            if (Server_KSB.instance.users.Count < 5)
            {
                connection = true;
            }

            byte[] response = protocol.StartConnectionResponse(connection);

            return response;
        }

        public byte[] WaitFileRequest(int size)
        {
            byte[] response = protocol.TransmitFileResponse(size);

            return response;
        }

        public void Listen()
        {
            while(true)
            {
                byte[] request = new byte[1024];
                client.Receive(request);
                int OPCODE = BitConverter.ToInt32(request, 1);

                /// request 없으면 종료? 예외처리

                switch (OPCODE)
                {
                    // 접속 요청
                    case 000:
                        //ReceiveString();
                        //ReceiveFile();
                        //byte[] imageArray = File.ReadAllBytes(@"C:\YUHAN\FTPtest.xlsx");
                        byte[] response = WaitConnectRequest();
                        client.Send(response);
                        Server_KSB.instance.users.Add(this);
                        Console.WriteLine($"접속 요청 [Length]:{request.Length}");
                        break;
                    case 100:

                        /*int headerSize = protocol.GetSizeHeader();
                        int offset = headerSize + 1;

                        byte[] body = new byte[request[3]];
                        request.CopyTo(body, headerSize);

                        byte[] fileNameSize;
                        body.CopyTo(fileNameSize, 0);
                        int fileNameSize = BitConverter.ToInt32(body[0]);
                        string fileNameSize = body[0];*/

                        /*int headerSize = protocol.GetSizeHeader();
                        byte[] headerBuffer = new byte[headerSize];
                        client.Receive(headerBuffer);
                        WaitFileRequest(headerBuffer.Length);
                        ReceiveFile();*/

                        /*byte[] fileName = new byte[1024];
                        client.Receive(fileName);
                        protocol.TransmitFileResponse();*/

                        Console.WriteLine("파일 전송 요청");
                        while (client.Connected)
                        {
                            int state = ReceiveFile();
                            if(state == 300 || state == 301) break;
                            byte[] opcode = new byte[4];
                            client.Send(BitConverter.GetBytes(state));
                        }

                        break;
                    case 400:
                        Console.WriteLine("정상 처리 완료");
                        client.Send(BitConverter.GetBytes(500));
                        break;
                    default:
                        Console.WriteLine($"Request error: {request[1]}");
                        break;
                }
            }
            
        }

        void ReceiveString()
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

                string receivedMSG = Encoding.UTF8.GetString(data, 0, length);
                string tempName = client.RemoteEndPoint.ToString();
                string combinedMSG = $"{tempName}: {receivedMSG}";
                byte[] msg = Encoding.UTF8.GetBytes(combinedMSG);

                Server_KSB.instance.MessageReceive.Invoke(msg);
                //Console.Write(msg);
            }
        }

        public int ReceiveFile()
        {
            try
            {
                if (client.Connected)
                {
                    

                    // 2. 파일 이름 길이 수신 (4바이트 - int형)
                    byte[] fileNameLengthBuffer = new byte[4];
                    int bytesRead = client.Receive(fileNameLengthBuffer, 0, fileNameLengthBuffer.Length, SocketFlags.None);
                    if (bytesRead <= 0)
                    {
                        Console.WriteLine("파일 이름 길이를 수신하는 중 연결이 끊겼습니다.");
                        return 101;
                    }
                    if (!BitConverter.IsLittleEndian) Array.Reverse(fileNameLengthBuffer);
                    int fileNameLength = BitConverter.ToInt32(fileNameLengthBuffer, 0);

                    // 3. 파일 이름 수신
                    byte[] fileNameBuffer = new byte[fileNameLength];
                    bytesRead = client.Receive(fileNameBuffer, 0, fileNameBuffer.Length, SocketFlags.None);
                    if (bytesRead <= 0)
                    {
                        Console.WriteLine("파일 이름을 수신하는 중 연결이 끊겼습니다.");
                        return 101;
                    }
                    //if (BitConverter.IsLittleEndian) Array.Reverse(fileNameLengthBuffer);

                    string fileName = Encoding.UTF8.GetString(fileNameBuffer);
                    Console.WriteLine($"수신할 파일 이름: {fileName}");

                    // 1. 파일 크기 수신 (8바이트 - long형)
                    byte[] fileSizeBuffer = new byte[8];
                    bytesRead = client.Receive(fileSizeBuffer, 0, fileSizeBuffer.Length, SocketFlags.None);
                    if (bytesRead <= 0)
                    {
                        Console.WriteLine("파일 크기를 수신하는 중 연결이 끊겼습니다.");
                        return 102;
                    }
                    if (!BitConverter.IsLittleEndian) Array.Reverse(fileSizeBuffer);
                    long fileSize = BitConverter.ToInt64(fileSizeBuffer, 0);
                    Console.WriteLine($"수신할 파일 크기: {fileSize} 바이트");

                    // 4. 파일 데이터 수신
                    string filePath = Path.Combine(@"..\..\..\..\ReceiveDir", fileName); // 저장 경로 설정
                    using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        long totalBytesReceived = 0;
                        byte[] fileBuffer = new byte[1024];

                        while (totalBytesReceived < fileSize)
                        {
                            int bytesToRead = (int)Math.Min(fileBuffer.Length, fileSize - totalBytesReceived);
                            int bytesReceived = client.Receive(fileBuffer, 0, bytesToRead, SocketFlags.None);

                            if (bytesReceived <= 0)
                            {
                                Console.WriteLine("파일 수신 중 연결이 끊겼습니다.");
                                return 102;
                            }

                            fs.Write(fileBuffer, 0, bytesReceived);

                            count += bytesToRead;
                            //Console.WriteLine(Encoding.UTF8.GetString(fileBuffer)+ "[count]"+ count);
                            totalBytesReceived += bytesReceived;
                        }

                    }

                    Console.WriteLine($"파일 {fileName} 수신 완료.");
                    return 100;
                }

                return 300;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ReceiveFile 오류: {ex.Message}");
                return 301;
            }
        }

    }
}
