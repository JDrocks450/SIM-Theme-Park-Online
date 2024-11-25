using MiscUtil.Conversion;
using QuazarAPI;
using QuazarAPI.Networking.Data;
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
    public class TPWPacket : PacketBase
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

        public bool IsHeaderless { get; set; } = false;        

        /// <summary>
        /// The header of the packet, two bytes, ASCII
        /// <para>Bs, Bc, etc.</para>
        /// </summary>
        public byte[] OriginCode { get; set; } = Data.TPWConstants.Bs_Header;
        /// <summary>
        /// The length of the header, readonly
        /// </summary>
        public ushort HeaderLength => IsHeaderless ? (ushort)0 : (ushort)Header.Length;
        public override uint GetHeaderSize() => 20;
        public uint DataLength => HeaderLength + BodyLength;
        /// <summary>
        /// WORD -- Indicates the type of the message
        /// </summary>
        public ushort MessageType { get; set; }
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
        public override UInt32 BodyLength => (UInt32)(Body?.Length ?? 0 + Footer?.Length ?? 0);
        /// <summary>
        /// [DWORD]
        /// </summary>
        public UInt32 Param3 { get; set; }
        /// <summary>
        /// [DWORD] The priority of this packet?
        /// </summary>
        public UInt32 PacketQueue { get; set; } = 0x000A;

        public byte[] Header => GetHeader();
        public override bool TryGetHeaderData(in byte[] Buffer, out uint ReadSize)
        {
            ReadSize = 0;
            if (Buffer.Length < 20) return false;
            ReadSize = 20;
            return true;
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
            this.MessageType = MsgType;
            this.Language = LanguageCode;
            this.Param2 = Param2;
            this.Param3 = Param3;
            PacketQueue = PacketPriority;
        }
        ~TPWPacket()
        {
            Dispose();
        }        

        private byte[] GetHeader()
        {
            byte[] buffer = new byte[20];
            OriginCode.CopyTo(buffer, 0);
            EndianBitConverter converter = EndianBitConverter.Big;
            converter.CopyBytes(MessageType, buffer, 2);
            converter.CopyBytes(Language, buffer, 4);
            converter.CopyBytes(Param2, buffer, 6);
            converter.CopyBytes(BodyLength + GetChildPacketBodyLength(), buffer, 8);
            converter.CopyBytes(Param3, buffer, 12);
            EndianBitConverter.Little.CopyBytes(PacketQueue, buffer, 16);
            return buffer;
        }

        public override byte[] GetBytes()
        {
            byte[] buffer = new byte[DataLength];
            if (!IsHeaderless)
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

        public override T ParsePacket<T>(byte[] bytes, out int endIndex) => Parse(bytes, out endIndex) as T;
        public override IEnumerable<T> ParseAllPackets<T>(ref byte[] Data) => ParseAll(ref Data).Select(x => x as T);

        public static TPWPacket Parse(in byte[] buffer, out int EndIndex)
        {
            if (buffer.Length < 20) throw new ArgumentException("The submitted buffer was not at least 20 bytes long.");
            EndianBitConverter converter = EndianBitConverter.Big;
            int offset = 2;
            TPWPacket packet = new TPWPacket()
            {
                OriginCode = new byte[2] { buffer[0], buffer[1] },
                MessageType = converter.ToUInt16(buffer, offset + 0),
                Language = converter.ToUInt16(buffer, offset + 2),
                Param2 = converter.ToUInt16(buffer, offset + 4),
                Param3 = converter.ToUInt32(buffer, offset + 10),
                PacketQueue = EndianBitConverter.Little.ToUInt32(buffer, offset + 14),
                Body = new byte[converter.ToUInt32(buffer, offset + 6)]
            };
            if (packet.OriginCode[0] == 0 && packet.OriginCode[1] == 0)
                throw new FormatException($"{nameof(packet.OriginCode)} was not recognized: {packet.OriginCode.Select(x => x.ToString())}");         
            offset = 20;
            if (buffer.Length < offset + (int)packet.BodyLength)
                throw new ArgumentException("The submitted buffer is too short to contain all the body data, you should wait until there's more data received.");
            packet.Body = buffer.Skip(offset).Take((int)packet.BodyLength).ToArray();
            EndIndex = (int)(offset + packet.BodyLength);

            return packet;
        }
        public void EmplaceBody(ITPWBOSSSerializable Serializable, bool FullFormat = true) => EmplaceBody(Serializable.GetBytes(FullFormat));
        public void EmplaceBodyAt(int Position, ITPWBOSSSerializable Type, bool FullFormat = true)
        {
            SetPosition(Position);
            EmplaceBody(Type, FullFormat);
        }
        public TPWZeroTerminatedString ReadBodyTerminatedString(byte Terminator = 00)
        {            
            var retVal = Parsers.TPWParsedData.ReadBodyTerminatedString(Body, (int)_bodyBuffer.Position, out var newPos, Terminator);
            _bodyBuffer.Position = newPos;
            return retVal;
        }
        public TPWUnicodeString ReadBodyKnownLengthUnicodeString()
        {
            var retVal = Parsers.TPWParsedData.ReadBodyFullFormattedUZ(Body, (int)_bodyBuffer.Position, out var newPos);
            _bodyBuffer.Position = newPos;
            return retVal;
        }
        public TPWUnicodeString ReadBodyUnicodeString(ushort Terminator = 0x0000)
        {
            var retVal = Parsers.TPWParsedData.ReadBodyUnicodeString(Body, (int)_bodyBuffer.Position, out var newPos, Terminator);
            _bodyBuffer.Position = newPos;
            return retVal;
        }
        public TPWDTStruct ReadBodyDateTime()
        {
            ushort Year = ReadBodyUshort();
            ushort Month = ReadBodyUshort();
            ushort Day = ReadBodyUshort();
            return new TPWDTStruct(Year,Month,Day);
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
    }
}
