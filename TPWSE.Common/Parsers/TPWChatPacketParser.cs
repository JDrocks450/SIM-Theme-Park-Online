using QuazarAPI;
using SimTheme_Park_Online.Data;
using SimTheme_Park_Online.Data.Primitive;
using SimTheme_Park_Online.Data.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Parsers
{
    /// <summary>
    /// A parsed data type for a ChatServer packet
    /// </summary>
    public class TPWChatParsedData : TPWParsedData
    {
        public TPWTemplateDefinition TemplateDef => new TPWTemplateDefinition(StartIndex, Length, Name, "A parsed data type.",
                TypeCode == SimTheme_Park_Online.Data.TPWConstants.TPWChatTypeCodes.ASCII ? TPWSystemTypes.ASCII_STR : TPWSystemTypes.UNI_STR
            );

        internal readonly uint StartIndex, Length;
        internal readonly string Name;

        public readonly Data.TPWConstants.TPWChatTypeCodes TypeCode;
        public readonly ITPWBOSSSerializable Data;        

        public TPWChatParsedData(Data.TPWConstants.TPWChatTypeCodes TypeCode, ITPWBOSSSerializable Data, string Name, uint StartIndex, uint Length)
        {
            this.TypeCode = TypeCode;
            this.Data = Data;
            this.Name = Name;
            this.StartIndex = StartIndex;
            this.Length = Length;
        }

        public override string ToString()
        {
            return $"[{this.GetType().Name}] {Name} | {TypeCode} | {Data}";
        }
    }
    /// <summary>
    /// Parses incoming / outgoing <see cref="ChatServer"/> packets.
    /// </summary>
    public class TPWChatPacketParser : ITPWParser<TPWChatParsedData>
    {        
        public bool TryParse(TPWPacket Data, out List<TPWChatParsedData> ParsedValues) {
            return Parse(Data, out ParsedValues);
        }
        public static bool Parse(TPWPacket Data, out List<TPWChatParsedData> ParsedValues)
        {
            var list = new List<TPWChatParsedData>();
            ParsedValues = list;
            Data.SetPosition(0);
            uint ChatMsgType = Data.ReadBodyDword();
            string name = "ChatMsgType";
            var msgType = (TPWConstants.TPWChatServerCommand)ChatMsgType;
            if (Enum.IsDefined(typeof(TPWConstants.TPWChatServerCommand), ChatMsgType))
                name += " " + Enum.GetName(typeof(TPWConstants.TPWChatServerCommand),
                   msgType);
            else QConsole.WriteLine("New Findings", $"Found a new data type -- Jeremy, you need to add this to the source collection now. Type: {ChatMsgType}");
            list.Add(new TPWChatParsedData(TPWConstants.TPWChatTypeCodes.ASCII, (TPWZeroTerminatedString)ChatMsgType.ToString(), name, 0, 4));
            int count = 1;
            while (!Data.IsBodyEOF)
            {
                int chatValueType = Data.ReadBodyByte();
                if (chatValueType == -1 || chatValueType == 0)
                    break;
                if (chatValueType == 255)
                    continue;
                TPWConstants.TPWChatTypeCodes typeCode = (TPWConstants.TPWChatTypeCodes)(byte)chatValueType;
                if (typeCode == TPWConstants.TPWChatTypeCodes.NONE)
                    throw new Exception($"That should not happen -- the type code: {chatValueType} is not recognized.");
                ITPWBOSSSerializable value = default;
                long position = Data.BodyPosition;
                switch (typeCode)
                {
                    case TPWConstants.TPWChatTypeCodes.ASCII:
                        value = Data.ReadBodyTerminatedString(0xFF);
                        break;
                    case TPWConstants.TPWChatTypeCodes.UNI:
                        value = Data.ReadBodyUnicodeString(0xFFFF);
                        break;
                }
                if (chatValueType == 255)
                    name = "Separator";
                else
                    name = "Parameter: " + count;
                long length = Data.BodyPosition - position;
                list.Add(new TPWChatParsedData(typeCode, value,  name, (uint)position, (uint)length));
                count++;
            }
            return true;
        }
    }
}
