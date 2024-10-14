using System.Reflection.Emit;
using System;
using System.Text;

namespace Protocols
{
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
            MakeHeader(1, 000, 0, 0, 0);

            // 각 필드를 바이트 배열로 변환
            byte[] verBytes = new byte[] { proto_VER };
            byte[] opcodeBytes = BitConverter.GetBytes(OPCODE);
            byte[] seqNoBytes = BitConverter.GetBytes(SEQ_NO);
            byte[] lengthBytes = BitConverter.GetBytes(LENGTH);
            byte[] crcBytes = BitConverter.GetBytes(CRC);

            // Little-endian 시스템이라면 바이트 순서 반전 필요
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(opcodeBytes);
                Array.Reverse(seqNoBytes);
                Array.Reverse(lengthBytes);
                Array.Reverse(crcBytes);
            }

            // 모든 바이트 배열을 결합하여 헤더 생성
            byte[] header = new byte[verBytes.Length + opcodeBytes.Length + seqNoBytes.Length + lengthBytes.Length + crcBytes.Length];
            Buffer.BlockCopy(verBytes, 0, header, 0, verBytes.Length);
            Buffer.BlockCopy(opcodeBytes, 0, header, verBytes.Length, opcodeBytes.Length);
            Buffer.BlockCopy(seqNoBytes, 0, header, verBytes.Length + opcodeBytes.Length, seqNoBytes.Length);
            Buffer.BlockCopy(lengthBytes, 0, header, verBytes.Length + opcodeBytes.Length + seqNoBytes.Length, lengthBytes.Length);
            Buffer.BlockCopy(crcBytes, 0, header, verBytes.Length + opcodeBytes.Length + seqNoBytes.Length + lengthBytes.Length, crcBytes.Length);

            return header;
        }
        public Byte[] StartConnectionResponse(bool ok) 
        {
            MakeBody(4096);
            BODY = Encoding.UTF8.GetBytes("Hello World!");
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
