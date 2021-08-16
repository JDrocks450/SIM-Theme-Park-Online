using MiscUtil.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimTheme_Park_Online
{
    public class TPWPacket
    {
           /* PACKET FORMAT FOUND:
            * REMEMBER THESE ARE INDICES OF BYTES IN ARRAY!
            * 42'B' 73's' [1 0 WORD] [2] [3] [5 4 WORD ESI + 1E]
            * [6..9 DWORD ESI+20] [10..13 DWORD ESI+24]
            * [14..17 DWORD ESI+28] 
            * BODY:
            * [0] PlayerID [1] CustomerID
            */
           /*
           * 1 0 WORD: 0x012E, 9 SUCCESS, 2 SERV ERROR, 1 AUTH ERROR
           */

        public DateTime Received { get; set; } 
        public DateTime Sent { get; set; }

        /// <summary>
        /// The header of the packet, two bytes, ASCII
        /// <para>Bs, Bc, etc.</para>
        /// </summary>
        public byte[] ResponseCode { get; set; }
        /// <summary>
        /// The length of the header, readonly
        /// </summary>
        public ushort HeaderLength => (ushort)Header.Length;
        public uint DataLength => HeaderLength + BodyLength;
        /// <summary>
        /// WORD -- Indicates the type of the message
        /// </summary>
        public ushort MsgType { get; set; }
        /// <summary>
        /// WORD
        /// </summary>
        public ushort Param1 { get; set; }
        /// <summary>
        /// WORD
        /// </summary>
        public ushort Param2 { get; set; }
        /// <summary>
        /// [DWORD] The length of the body of this packet, needed for TPW packets, this is calculated for you.
        /// <para>See: <see cref="Body"/></para>
        /// </summary>
        public UInt32 BodyLength => (UInt32)(Body?.Length ?? 0 + Footer?.Length ?? 0);
        /// <summary>
        /// [DWORD]
        /// </summary>
        public UInt32 Param3 { get; set; }
        /// <summary>
        /// [DWORD] The priority of this packet?
        /// </summary>
        public UInt32 PacketQueue { get; set; } = 0x000A;

        public byte[] Header => GetHeader();
        public byte[] Body { get; set; }
        public byte[] Footer { get; set; }

        public TPWPacket()
        {

        }
        public TPWPacket(ushort MsgType, ushort Param1, ushort Param2, UInt32 Param3, UInt32 PacketPriority)
        {
            this.MsgType = MsgType;
            this.Param1 = Param1;
            this.Param2 = Param2;
            this.Param3 = Param3;
            PacketQueue = PacketPriority;
        }

        private byte[] GetHeader()
        {
            byte[] buffer = new byte[20];
            ResponseCode.CopyTo(buffer, 0);
            EndianBitConverter converter = EndianBitConverter.Big;
            converter.CopyBytes(MsgType, buffer, 2);
            converter.CopyBytes(Param1, buffer, 4);
            converter.CopyBytes(Param2, buffer, 6);
            converter.CopyBytes(BodyLength, buffer, 8);
            converter.CopyBytes(Param3, buffer, 12);
            EndianBitConverter.Little.CopyBytes(PacketQueue, buffer, 16);
            return buffer;
        }

        public byte[] GetBytes()
        {
            byte[] buffer = new byte[DataLength];
            Header.CopyTo(buffer,0);
            Body?.CopyTo(buffer, HeaderLength);
            Footer?.CopyTo(buffer, Body.Length + HeaderLength - Footer.Length);
            return buffer;
        }

        public static TPWPacket Parse(byte[] buffer)
        {
            if (buffer.Length < 20) throw new ArgumentException("The submitted buffer was not at least 20 bytes long.");
            EndianBitConverter converter = EndianBitConverter.Big;
            int offset = 2;
            TPWPacket packet = new TPWPacket()
            {
                ResponseCode = new byte[2] { buffer[0], buffer[1] },
                MsgType = converter.ToUInt16(buffer, offset + 0),
                Param1 = converter.ToUInt16(buffer, offset + 2),
                Param2 = converter.ToUInt16(buffer, offset + 4),
                Param3 = converter.ToUInt32(buffer, offset + 10),
                PacketQueue = EndianBitConverter.Little.ToUInt32(buffer, offset + 14),
                Body = new byte[converter.ToUInt32(buffer, offset + 6)]
            };
            int dataEnd = 0, index = 0;
            foreach (var entry in buffer)
            {
                if (entry != 00)
                    dataEnd = index;
                index++;
            }
            dataEnd -= 20; // header auto correct
            if (dataEnd > packet.BodyLength) throw new FormatException($"Packet size discrepency detected. " +
                $"Body size has data that extends to {dataEnd} when we expected {packet.BodyLength} by BodyLength parameter value.");
            offset = 20;
            buffer.Skip(20).Take((int)packet.BodyLength).ToArray().CopyTo(packet.Body, 0);
            return packet;
        }
    }
}
