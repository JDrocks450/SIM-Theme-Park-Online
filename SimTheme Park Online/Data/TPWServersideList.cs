﻿using MiscUtil.Conversion;
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
                            var str = (string)Data;                                                    
                            var strBuffer = Encoding.Unicode.GetBytes(str);
                            byte[] buffer = new byte[strBuffer.Length + 2];    
                            EndianBitConverter.Big.CopyBytes((ushort)strBuffer.Length, buffer, 0);
                            strBuffer.CopyTo(buffer, 2);
                            return buffer;
                        }
                    case F4: return EndianBitConverter.Big.GetBytes((float)Data);
                    case SZ: 
                    case DT: return new byte[] { 00, 00, 00, 00 };
                    case BG: return (byte[])Data;
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

        /// <summary>
        /// The type of list, please check <see cref="TPWConstants.TPWServerListType"/> to see if the list is already known.
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

        public byte[] GetBytes()
        {
            int group = 0;
            int offset = 0;
            uint totalSize = 0;
            uint templateOffsetEnd = 0;
            var encoder = EndianBitConverter.Big;
            _template.Definitions.Clear();
            Dictionary<int, (string format, byte[] data)> formattedDataMap = new Dictionary<int, (string format, byte[] data)>();
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
                        _template.Add(
                                new Templating.TPWTemplateDefinition(
                                    templateOffsetEnd, (uint)written, def.DataType + " Value", "A formatted value.", def.GetClosestSystemType()));                        
                        offset++;
                    }
                    totalSize += (uint)(definitionBuffer.Length);
                    formattedDataMap.Add(group - 1, (formatStr.ToString().TrimEnd(','), definitionBuffer.ToArray()));
                }
            }
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(encoder.GetBytes(ListType));
                stream.Write(encoder.GetBytes((uint)formattedDataMap.Count));
                stream.Write(encoder.GetBytes(totalSize)); // DataLength                
                group = 0;
                uint prevDataLeng = 0;
                foreach (var mapItem in formattedDataMap.Values)
                {
                    if (group == 0)
                    {
                        Param = prevDataLeng = (uint)(mapItem.data.Length + mapItem.format.Length + 1);
                        stream.Write(encoder.GetBytes(Param));
                        stream.Write(Encoding.ASCII.GetBytes(mapItem.format));
                        stream.WriteByte(00);
                        templateOffsetEnd = _TemplateHeader(mapItem.format.Length, templateOffsetEnd);
                    }
                    //else Param += (uint)prevDataLeng;                                                                                 
                    stream.Write(mapItem.data);
                    templateOffsetEnd += (uint)(mapItem.data.Length + mapItem.format.Length + 1);                    
                    group++;
                }
                //stream.Write(new byte[90]); // add a bit incase too short
                return stream.ToArray();
            }
        }        

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
    }
}