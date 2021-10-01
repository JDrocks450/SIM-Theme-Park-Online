using Cassandra;
using MiscUtil.Conversion;
using SimTheme_Park_Online.Data.Primitive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SimTheme_Park_Online.Data.TPWConstants.TPWServerListConstants;

namespace SimTheme_Park_Online.Data
{
    /// <summary>
    /// An interface for creating lists that the server makes for the client.
    /// </summary>
    public class TPWServersideList
    {
        /// <summary>
        /// When making packets with multiple lists, use the <see cref="TPWServersideListSeparator"/> to separate the lists for use in one continuous packet.
        /// </summary>
        public class TPWServersideListSeparator : TPWServersideListDefinition
        {
            public override byte[] GetBytes()
            {
                return new byte[0];
            }
        }

        [Serializable]
        public class TPWServersideListDefinition
        {
            public string DataType { get; set; } = XX;
            public object Data { get; }

            public TPWServersideListDefinition()
            {

            }

            public TPWServersideListDefinition(object Data) :
                this(GetDataTypeByType(Data), Data)
            {

            }

            public TPWServersideListDefinition(
                string DataType, object Data) : this()
            {
                this.DataType = DataType;
                this.Data = Data;
            }

            public Templating.TPWSystemTypes GetClosestSystemType()
            {
                switch (DataType)
                {
                    case I4: return Templating.TPWSystemTypes.DWORD;
                    case F4: return Templating.TPWSystemTypes.DWORD;
                    case UZ: return Templating.TPWSystemTypes.UNI_STR;
                    case SZ: return Templating.TPWSystemTypes.UNI_STR;
                    case DT: return Templating.TPWSystemTypes.UNI_STR;
                    case BG: return Templating.TPWSystemTypes.BYTE_ARR;
                    default: return Templating.TPWSystemTypes.ASCII_STR;
                }
            }

            public virtual byte[] GetBytes()
            {
                switch (DataType)
                {
                    case I4: return EndianBitConverter.Big.GetBytes((uint)Data);
                    case UZ:
                        {
                            return ((TPWUnicodeString)Data).GetBytes();
                        }
                    case F4: return EndianBitConverter.Big.GetBytes((float)Data);
                    case SZ:
                        {
                            return ((TPWZeroTerminatedString)Data).GetBytes();
                        }
                    case DT:
                        {
                            return ((TPWDTStruct)Data).GetBytes();
                        }
                    case BG:
                        {
                            byte[] source = ((TimeUuid)Data).ToByteArray();
                            byte[] buffer = new byte[source.Length + 2];
                            EndianBitConverter.Big.CopyBytes((ushort)source.Length, buffer, 0);
                            source.CopyTo(buffer, 2);
                            return buffer;
                        }
                    default:
                        throw new NotImplementedException("This data type hasn't been implemented yet.");
                }
            }

            public int Write(Stream Buffer)
            {
                byte[] buffer = GetBytes();
                Buffer.Write(buffer);
                return buffer.Length;
            }
        }

        public Templating.TPWDataTemplate GetTemplate() => _template;

        private Templating.TPWDataTemplate _template = new Templating.TPWDataTemplate();

        public bool IsEmptyList
        {
            get; set;
        } = false;

        /// <summary>
        /// The type of list, please check <see cref="TPWConstants.TPWCityServerListType"/> to see if the list is already known.
        /// <para>This list type is specific to each function in the game</para>
        /// </summary>
        public uint ListType { get; set; }
        /// <summary>
        /// The number of items in this list, this is populated for you and cannot/should not be altered.
        /// </summary>
        public uint NumberOfItems => (uint)Definitions.OfType<TPWServersideListDefinition>().Count();
        public uint Param { get; set; } = 0x01;

        public HashSet<TPWServersideListDefinition> Definitions { get; set; } = new HashSet<TPWServersideListDefinition>();

        public TPWServersideList()
        {

        }

        /// <summary>
        /// Creates an array of packets being sent from the server that have their data split.
        /// </summary>
        /// <param name="PrimaryPacket">The first packet in the list -- the one with all the information about the current transaction.</param>
        /// <returns></returns>
        public void CreatePacket(ref TPWPacket PrimaryPacket)
        {
            int index = 0;
            var packets = getPackets(ref PrimaryPacket, false);
            if (packets[index] == PrimaryPacket)
                index++;
            PrimaryPacket.AppendChildPackets(packets);
        }

        private TPWPacket[] getPackets(ref TPWPacket ReferencePacket, bool isEmpty)
        {
            int group = 0;
            int offset = 0;
            uint totalSize = 0;
            uint templateOffsetEnd = 0;
            var encoder = EndianBitConverter.Big;
            _template.Definitions.Clear();
            Dictionary<int, (string format, byte[] data)> formattedDataMap = new Dictionary<int, (string format, byte[] data)>();
            Queue<Templating.TPWTemplateDefinition> definitions = new Queue<Templating.TPWTemplateDefinition>();
            int formatStrLeng = 0;
            while (Definitions.Skip(offset).Any())
            {
                using (MemoryStream definitionBuffer = new MemoryStream())
                {
                    StringBuilder formatStr = new StringBuilder();
                    foreach (var def in Definitions.Skip(offset))
                    {
                        if (def is TPWServersideListSeparator)
                        {
                            group++;
                            offset++;
                            break;
                        }
                        formatStr.Append(def.DataType + ',');
                        int written = def.Write(definitionBuffer);
                        definitions.Enqueue(new Templating.TPWTemplateDefinition(
                                    templateOffsetEnd, (uint)written, def.DataType + " Value", "A formatted value.", def.GetClosestSystemType()));
                        templateOffsetEnd += (uint)written;
                        offset++;
                    }
                    formatStrLeng = formatStr.Length;
                    totalSize += (uint)(definitionBuffer.Length);
                    formattedDataMap.Add(group - 1, (formatStr.ToString().TrimEnd(','), definitionBuffer.ToArray()));
                }
            }
            group = 0;
            TPWPacket[] outboundPackets = new TPWPacket[formattedDataMap.Count];
            foreach (var mapItem in formattedDataMap.Values)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    if (group == 0)
                    {
                        //emplace header
                        stream.Write(encoder.GetBytes(ListType));
                        stream.Write(encoder.GetBytes(isEmpty ? 0 : (uint)formattedDataMap.Count));
                        stream.Write(encoder.GetBytes(isEmpty ? 0 : totalSize)); // DataLength

                        //emplace the format string
                        Param = (uint)0;// (uint)(mapItem.data.Length + mapItem.format.Length + 1);
                        stream.Write(encoder.GetBytes(Param));
                        stream.Write(Encoding.ASCII.GetBytes(mapItem.format));
                        stream.WriteByte(00);
                        _TemplateHeader(mapItem.format.Length);
                    }
                    if (!isEmpty)
                    {
                        stream.Write(mapItem.data);
                    }

                    TPWPacket target = null;
                    if (group == 0)
                        target = ReferencePacket;
                    else
                    {
                        target = new TPWPacket()
                        {
                            MessageType = ReferencePacket.MessageType,
                            Language = ReferencePacket.Language,
                            PacketQueue = ReferencePacket.PacketQueue,
                            OriginCode = ReferencePacket.OriginCode,
                            IsHeaderless = true
                        };
                        outboundPackets[group-1] = target;
                    }
                    target.Body = stream.ToArray();                    
                }
                group++;
            }

            while (definitions.Any())
            {
                var def = definitions.Dequeue();
                uint width = def.Length;
                def.StartOffset += (uint)(formatStrLeng + 16);
                def.EndOffset = def.StartOffset + width;
                _template.Add(def);
            }

            return outboundPackets;
        }

        public void CreateEmptyPacket(ref TPWPacket Packet) => getPackets(ref Packet, true);

        public static TPWServersideList MergeAll(params TPWServersideList[] Lists)
        {
            if (!Lists.Any()) return null;
            TPWServersideList list = new TPWServersideList()
            {
                ListType = Lists[0].ListType,
                Param = Lists[0].Param
            };
            foreach(var structure in Lists)
            {
                foreach (var def in structure.Definitions)
                    list.Definitions.Add(def);
                list.Definitions.Add(new TPWServersideListSeparator());
            }
            return list;
        }

        private uint _TemplateHeader(int formatStrLen, uint currentOffset = 0)
        {            
            _template.Add(
                new Templating.TPWTemplateDefinition(
                    currentOffset + 0, 4, "LIST TYPE", "The type of list being sent.", Templating.TPWSystemTypes.DWORD));
            _template.Add(
                new Templating.TPWTemplateDefinition(
                    currentOffset + 4, 4, "NUMBER OF ITEMS", "The number of items in the list.", Templating.TPWSystemTypes.DWORD));
            _template.Add(
                new Templating.TPWTemplateDefinition(
                    currentOffset + 8, 4, "DATA SIZE", "The length, in bytes, of the data.", Templating.TPWSystemTypes.DWORD));
            _template.Add(
                new Templating.TPWTemplateDefinition(
                    currentOffset + 12, 4, "PARAM", "An unknown parameter.", Templating.TPWSystemTypes.DWORD));
            uint formatLen = (uint)formatStrLen;
            _template.Add(
                new Templating.TPWTemplateDefinition(
                    currentOffset + 16, formatLen, "FORMAT STR", "The format of the proceeding data.", Templating.TPWSystemTypes.ASCII_STR));
            return currentOffset + 17 + formatLen;
        }

        /// <summary>
        /// Creates a new TPWServersideList with the given information
        /// </summary>
        /// <returns></returns>
        public static TPWServersideList Create(uint ListType, params TPWServersideListDefinition[] Definitions) => 
            new TPWServersideList()
            {
                ListType = ListType,
                Definitions = new HashSet<TPWServersideListDefinition>(Definitions)
            };

        public static TPWServersideList Create(uint ListType, params object[] Values)
        {
            return Create(ListType,
                Values.Select(x => new TPWServersideListDefinition(x)).ToArray());
        }

        public static IEnumerable<TPWServersideList> Parse(TPWPacket packet)
        {
            uint expectedEntries = 0, size = 0;
            string formatString = "";

            void ReadHeader(ref TPWServersideList TList)
            {
                packet.SetPosition(0);
                uint listType = packet.ReadBodyDword();
                expectedEntries = packet.ReadBodyDword();
                size = packet.ReadBodyDword();

                TList.ListType = listType;
                TList.Param = packet.ReadBodyDword();
                formatString = packet.ReadBodyTerminatedString();
                packet.SetPosition((int)packet.BodyPosition + 1);
            }

            TPWServersideList list = new TPWServersideList();
            ReadHeader(ref list);
            if (packet.BodyLength < size)
                throw new ArgumentException("The data is not of sufficient length: " + size + " bytes. This is likely an imcomplete packet.");
            string[] formatSpecifiers = formatString.Split(',');
            for (int i = 0; i < expectedEntries; i++)
            {
                foreach (var specifier in formatSpecifiers)
                {
                    switch (specifier)
                    {
                        case I4: list.Definitions.Add(new TPWServersideListDefinition(packet.ReadBodyDword())); break;
                        case UZ: 
                            list.Definitions.Add(new TPWServersideListDefinition(packet.ReadBodyKnownLengthUnicodeString()));
                            break;
                        case F4: list.Definitions.Add(new TPWServersideListDefinition(EndianBitConverter.Big.ToSingle(packet.ReadBodyByteArray(4), 0))); break;
                        case SZ: 
                            list.Definitions.Add(new TPWServersideListDefinition(packet.ReadBodyTerminatedString())); 
                            packet.Advance();
                            break;
                        case DT: list.Definitions.Add(new TPWServersideListDefinition(packet.ReadBodyDateTime())); break;
                        case BG:
                            {
                                ushort length = packet.ReadBodyUshort();
                                byte[] uuid = packet.ReadBodyByteArray(length);
                                var guid = new Guid(uuid);
                                var tuuid = TimeUuid.Parse(guid.ToString());
                                list.Definitions.Add(new TPWServersideListDefinition(tuuid));
                                break;
                            }
                        default:
                            throw new NotImplementedException("This data type hasn't been implemented yet.");
                    }
                }
                yield return list;
                list = new TPWServersideList()
                {
                    ListType = list.ListType,
                };
            }
        }

        public static IEnumerable<TPWServersideList> Parse(byte[] Array)
        {
            //Make a packet to use the body reading functions I wrote
            TPWPacket packet = new TPWPacket();
            packet.Body = Array;

            return Parse(packet);
        }
    }
}
