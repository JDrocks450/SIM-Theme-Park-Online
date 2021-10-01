using MiscUtil.Conversion;
using SimTheme_Park_Online.Data.Primitive;
using System.Collections.Generic;
using System.Text;

namespace SimTheme_Park_Online.Parsers
{
    /// <summary>
    /// Provides a basis for different types of formatted data that are found in Theme Park World / SIM Theme Park Online.
    /// </summary>
    public abstract class TPWParsedData
    {
        public static TPWZeroTerminatedString ReadBodyTerminatedString(byte[] Buffer, int index, out long NewPosition, byte Terminator = 00)
        {
            byte c = Buffer[index];
            List<byte> value = new List<byte>();
            while (c != Terminator)
            {
                value.Add(c);
                index++;
                c = Buffer[index];
            }
            NewPosition = index;
            return System.Text.Encoding.ASCII.GetString(value.ToArray());
        }
        public static TPWUnicodeString ReadBodyFullFormattedUZ(byte[] Buffer, int index, out long NewPosition)
        {
            ushort byteLength = EndianBitConverter.Big.ToUInt16(Buffer, index);
            index += 2;
            return ReadBodyUnicodeString(Buffer, index, out NewPosition, 0x00, byteLength);
        }
        public static TPWUnicodeString ReadBodyUnicodeString(byte[] Buffer, int index, out long NewPosition, ushort Terminator = 00, ushort readAmount = 00)
        {           
            var converter = EndianBitConverter.Big;
            ushort currentChar = converter.ToUInt16(Buffer, index);
            List<byte> value = new List<byte>();
            string currentText = "";
            ushort alreadyRead = 0;
            while (true)
            {
                if (readAmount == 0)
                {
                    if (currentChar == Terminator)
                        break;
                }
                else if (alreadyRead >= readAmount)
                    break;
                value.Add(Buffer[index]);
                value.Add(Buffer[index + 1]);
                currentText = System.Text.Encoding.Unicode.GetString(value.ToArray());
                index+=2;
                alreadyRead += 2;
                currentChar = converter.ToUInt16(Buffer, index);
            }
            NewPosition = index;
            return currentText;
        }        
    }
    /// <summary>
    /// A parser that takes in a <see cref="TPWPacket"/> and returns parsed data types.
    /// </summary>
    /// <typeparam name=""></typeparam>
    public interface ITPWParser<T> where T : TPWParsedData
    {
        /// <summary>
        /// Attempts to parse the incoming data into the data type provided by the implementing <see langword="class"/> and <see langword="T"/>
        /// </summary>
        /// <param name="Data">The incoming data</param>
        /// <param name="ParsedValues">The returned data types.</param>
        /// <returns></returns>
        bool TryParse(TPWPacket Data, out List<T> ParsedValues);        
    }
}