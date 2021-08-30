using MiscUtil.Conversion;
using QuazarAPI;
using SimTheme_Park_Online.Data.Primitive;
using SimTheme_Park_Online.Data.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SimTheme_Park_Online
{
    [Serializable]
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

        public DateTime Received, Sent;
        public string ReceivedTime => Received == default ? "Not Received" : Received.ToString();
        public string SentTime => Sent == default ? "Not Sent" : Sent.ToString();

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
        /// WORD - Language ID, 0409 [US]
        /// </summary>
        public ushort Language { get; set; } = 0x0809;
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
        private MemoryStream _bodyBuffer = new MemoryStream();
        public byte[] Body
        {
            get => _bodyBuffer.ToArray();
            set
            {
                AllocateBody((uint)value.Length);
                EmplaceBody(value);
            }
        }
        public byte[] Footer { get; set; }
        //[JsonIgnore]
        /// <summary>
        /// The file this packet is stored in on the computer, if one exists.
        /// </summary>
        public string FileName { get; set; }
        
        private TPWDataTemplate _dataTemplate;
        /// <summary>
        /// Represents whether a <see cref="TPWDataTemplate"/> is added to this packet or not.
        /// </summary>
        public bool HasDataTemplate => _dataTemplate != null;
        /// <summary>
        /// Gets the current <see cref="TPWDataTemplate"/> that represents the data contained in this packet.
        /// </summary>
        public TPWDataTemplate GetTemplate() => _dataTemplate;
        public TPWDataTemplate SetTemplate(TPWDataTemplate value) => _dataTemplate = value;

        public TPWPacket()
        {

        }
        public TPWPacket(ushort MsgType, ushort LanguageCode, ushort Param2, UInt32 Param3, UInt32 PacketPriority)
        {
            this.MsgType = MsgType;
            this.Language = LanguageCode;
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
            converter.CopyBytes(Language, buffer, 4);
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
            if (Body != default)
            {
                byte[] fooBody = Body;
                //Array.Resize(ref fooBody, Body.Length + Footer?.Length ?? 0);
                //Footer?.CopyTo(fooBody, Body.Length - (Footer?.Length ?? 0));
                fooBody?.CopyTo(buffer, HeaderLength);
            }
            return buffer;
        }

        public static TPWPacket Parse(in byte[] buffer, out int EndIndex)
        {
            if (buffer.Length < 20) throw new ArgumentException("The submitted buffer was not at least 20 bytes long.");
            EndianBitConverter converter = EndianBitConverter.Big;
            int offset = 2;
            TPWPacket packet = new TPWPacket()
            {
                ResponseCode = new byte[2] { buffer[0], buffer[1] },
                MsgType = converter.ToUInt16(buffer, offset + 0),
                Language = converter.ToUInt16(buffer, offset + 2),
                Param2 = converter.ToUInt16(buffer, offset + 4),
                Param3 = converter.ToUInt32(buffer, offset + 10),
                PacketQueue = EndianBitConverter.Little.ToUInt32(buffer, offset + 14),
                Body = new byte[converter.ToUInt32(buffer, offset + 6)]
            };
            if (packet.ResponseCode[0] == 0 && packet.ResponseCode[1] == 0)
                throw new FormatException("ResponseCode was not recognized: 00 00");
            int dataEnd = 0, index = 0;
            foreach (var entry in buffer)
            {
                if (entry != 00)
                    dataEnd = index;
                index++;
            }
            dataEnd -= 20; // header auto correct
            offset = 20;
            packet.Body = buffer.Skip(offset).Take((int)packet.BodyLength).ToArray();
            EndIndex = (int)(offset + packet.BodyLength);

            return packet;
        }

        public void AllocateBody(uint BodySize) { 
            _bodyBuffer = new MemoryStream(new byte[BodySize]);
            SetPosition(0);
        }
        public void SetPosition(int Position) => _bodyBuffer.Position = Position;
        public void EmplaceBody(params byte[] Bytes) => _bodyBuffer.Write(Bytes);        
        public void EmplaceBody(uint DWORD, Endianness Endian = Endianness.BigEndian) {
            if (Endian == Endianness.BigEndian)
                EmplaceBody(EndianBitConverter.Big.GetBytes(DWORD));
            else EmplaceBody(EndianBitConverter.Little.GetBytes(DWORD));
        }
        public void EmplaceBody(ITPWBOSSSerializable Serializable, bool FullFormat = true) => EmplaceBody(Serializable.GetBytes(FullFormat));
        public void EmplaceBodyAt(int Position, byte[] Buffer)
        {
            SetPosition(Position);
            EmplaceBody(Buffer);
        }
        public void EmplaceBodyAt(int Position, uint DWORD, Endianness Endian = Endianness.BigEndian)
        {
            SetPosition(Position);
            EmplaceBody(DWORD, Endian);
        }      
        public void EmplaceBodyAt(int Position, byte Byte)
        {
            SetPosition(Position);
            EmplaceBody(Byte);
        }      
        public void EmplaceBodyAt(int Position, ITPWBOSSSerializable Byte)
        {

        }

        public static IEnumerable<TPWPacket> ParseAll(params string[] Files)
        {
            foreach(var file in Files)
            {
                byte[] buffer = File.ReadAllBytes(file);
                foreach (var packet in ParseAll(ref buffer))
                    yield return packet;
            }
        }

        public static IEnumerable<TPWPacket> ParseAll(ref byte[] readBuffer)
        {
            int amount = 0;
            List<TPWPacket> packets = new List<TPWPacket>();
            while (readBuffer.Length > 0)
            {
                int endIndex = 0;
                try
                {
                    packets.Add(Parse(readBuffer, out endIndex));                    
                }
                catch (Exception ParseException)
                {
                    QConsole.WriteLine("System", $"Couldn't parse incoming TPWPacket! " + ParseException.Message);
                    break;
                }

                if (endIndex < readBuffer.Length)
                {
                    int nLength = readBuffer.Length - endIndex;
                    if (nLength == 0)
                        break;
                    readBuffer = readBuffer.Skip(endIndex).ToArray();
                }
                else break;
                amount++;
            }
            return packets;
        }

        public void MergeBody(TPWPacket packet, int index)
        {
            MergeBody(packet.Body, index);
        }
        public void MergeBody(byte[] source, int index = 0)
        {
            byte[] buffer = Body;
            int inputLeng = buffer.Length;
            Array.Resize(ref buffer, Body.Length + (source.Length - index));
            source.Skip(index).ToArray().CopyTo(buffer, inputLeng);
            Body = buffer;
        }

        public int Write(in Stream Buffer)
        {
            byte[] buffer = GetBytes();
            Buffer.Write(buffer);
            return buffer.Length;
        }
    }
}
