using SimTheme_Park_Online.Data.Primitive;
using static SimTheme_Park_Online.Data.TPWConstants;

namespace SimTheme_Park_Online.Data.Packets
{
    public class TPWChatPacket : TPWPacket
    {
        public void EmplaceBlock(TPWUnicodeString StringValue)
        {
            EmplaceBody((byte)TPWChatTypeCodes.UNI);
            EmplaceBody(StringValue, false);
            EmplaceBody(0xFF, 0xFF);
        }
        public void EmplaceBlock(TPWZeroTerminatedString StringValue)
        {
            EmplaceBody((byte)TPWChatTypeCodes.ASCII);
            EmplaceBody(StringValue, false);
            EmplaceBody(0xFF);
        }
        public TPWChatPacket(uint ChatResponseCode, params ITPWBOSSSerializable[] Objects)
        {
            EmplaceBody((uint)ChatResponseCode);
            foreach(var obj in Objects)
            {
                var serializableType = obj;
                if (obj is DWORD dWORD)
                    serializableType = dWORD.UInt32.ToSZ();
                if (serializableType is TPWZeroTerminatedString @string)
                    EmplaceBlock(@string);
                if (obj is TPWUnicodeString)
                    EmplaceBlock((TPWUnicodeString)serializableType);
            }
            EmplaceBody(00);
        }
    }
}
