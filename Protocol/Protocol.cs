using System.Reflection.Emit;
using System;
using System.Text;

namespace Protocols
{
    static class Constants
    {
        //public const Byte FTP = 
    }

    public class Protocol
    {
        public Byte proto_VER { get; private set; }
        public ushort OPCODE { get; private set;}
        public uint SEQ_NO { get; private set;}
        public uint LENGTH { get; private set;}
        public ushort CRC { get; private set;}
        Byte[] BODY;

        public Protocol()
        {
            
        }

        // 반환시 헤더 크기 반환
        private int MakeHeader(int ver, int op, int seq, int len, int crc) 
        {
            proto_VER = (Byte)ver;
            OPCODE = (ushort)op;
            SEQ_NO = (uint)seq;
            LENGTH = (uint)len;
            CRC = (ushort)crc;

            return sizeof(Byte) + sizeof(ushort) + sizeof(uint) + sizeof(uint) + sizeof(ushort);
        }
        public int GetSizeHeader() 
        {
            return sizeof(Byte) + sizeof(ushort) + sizeof(uint) + sizeof(uint) + sizeof(ushort);
        }
        private int MakeBody(int size) 
        {
            BODY = new Byte[size];

            return size;
        }

        public Byte[] GetLastPacket() 
        {
            return null;   
        }
        public Byte[] StartConnectionRequest() 
        {
            BODY = Encoding.UTF8.GetBytes("Request Connection");
            MakeHeader(1, 000, 0, BODY.Length, 0);

            List<byte> packet = new List<byte>();
            packet.Add(proto_VER);
            packet.AddRange(BitConverter.GetBytes(OPCODE));
            packet.AddRange(BitConverter.GetBytes(SEQ_NO));
            packet.AddRange(BitConverter.GetBytes(LENGTH));
            packet.AddRange(BitConverter.GetBytes(CRC));
            packet.AddRange(BODY);

            return packet.ToArray();
        }
        public Byte[] StartConnectionResponse(bool ok) 
        {
            if (ok)
            {
                OPCODE = 000;
                BODY = Encoding.UTF8.GetBytes("000 OK");
            }
            else
            {
                OPCODE = 001;
                BODY = Encoding.UTF8.GetBytes("001 reject");
            }

            byte[] response = new byte[GetSizeHeader()+ BODY.Length];
            return BODY;
        }
        public Byte[] TransmitFileRequest(string filename, int size) 
        {
            return null;
        }
        public Byte[] TransmitFileResponse(string filename, int size) 
        {
            return null;
        }

        ///....... 이런 식으로 작성하면 프로토콜 프레임이 작성되고
        ///.......  바이트배열이 만들어 지면 전송객체에 전달해주면 된다.
        // ......  주의 사항으로 바이트오더에 주의
    }
}
