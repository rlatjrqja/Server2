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

        public UserHandler(Socket socket)
        {
            client = socket;
            protocol = new Protocol();
        }

        public byte[] WaitRequest()
        {
            //byte[] head = protocol.StartConnectionRequest();
            bool connection=false;
            byte[] request = new byte[1024];
            client.Receive(request);
            if(request[1] == 000)
            {
                connection = true;
            }

            byte[] response = protocol.StartConnectionResponse(connection);

            /*switch (protocol.OPCODE)
            {
                case 000:
                    client.Send(head);
                    byte[] body = protocol.StartConnectionResponse(true);
                    client.Send(body);
                    Server_KSB.instance.users.Add(this);
                    connection = true;
                    break;
                case 001:
                    break;
            }*/

            return response;
        }

        public void Listen()
        {
            switch (protocol.OPCODE)
            {
                default:
                    //ReceiveString();
                    ReceiveFile();
                    //byte[] imageArray = File.ReadAllBytes(@"C:\YUHAN\FTPtest.xlsx");

                    //Console.WriteLine(Encoding.UTF8.GetString(imageArray));
                    break;
                case 101:
                    break;
                case 102:
                    break;
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

        void ReceiveFile()
        {
            try
            {
                while (client.Connected)
                {
                    

                    // 2. 파일 이름 길이 수신 (4바이트 - int형)
                    byte[] fileNameLengthBuffer = new byte[4];
                    int bytesRead = client.Receive(fileNameLengthBuffer, 0, fileNameLengthBuffer.Length, SocketFlags.None);
                    if (bytesRead <= 0)
                    {
                        Console.WriteLine("파일 이름 길이를 수신하는 중 연결이 끊겼습니다.");
                        return;
                    }
                    if (!BitConverter.IsLittleEndian) Array.Reverse(fileNameLengthBuffer);
                    int fileNameLength = BitConverter.ToInt32(fileNameLengthBuffer, 0);

                    // 3. 파일 이름 수신
                    byte[] fileNameBuffer = new byte[fileNameLength];
                    bytesRead = client.Receive(fileNameBuffer, 0, fileNameBuffer.Length, SocketFlags.None);
                    if (bytesRead <= 0)
                    {
                        Console.WriteLine("파일 이름을 수신하는 중 연결이 끊겼습니다.");
                        return;
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
                        return;
                    }
                    if (!BitConverter.IsLittleEndian) Array.Reverse(fileSizeBuffer);
                    long fileSize = BitConverter.ToInt64(fileSizeBuffer, 0);
                    Console.WriteLine($"수신할 파일 크기: {fileSize} 바이트");

                    // 4. 파일 데이터 수신
                    string filePath = Path.Combine(@"C:\YUHAN\ReceivedFiles", fileName); // 저장 경로 설정
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
                                return;
                            }

                            fs.Write(fileBuffer, 0, bytesReceived);
                            totalBytesReceived += bytesReceived;
                        }
                    }

                    Console.WriteLine($"파일 {fileName} 수신 완료.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ReceiveFile 오류: {ex.Message}");
            }
        }

    }
}
