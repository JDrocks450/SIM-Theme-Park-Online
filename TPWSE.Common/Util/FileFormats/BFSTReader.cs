using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimTheme_Park_Online.Util.FileFormats
{
    // bullfrog string
    public class BFSTReader
    {
        public string[] Extensions => new[] { ".str" };
        public string AssetName => "BFST (Bullfrog String)";

        /// <summary>
        /// Gets the strings of a BFST file
        /// </summary>
        /// <param name="data">The bytes that make up the file this is from</param>
        /// <param name="conversionTable">The URI of the file: MBtoUni.dat</param>
        /// <returns></returns>
        public static List<string> GetStrings(byte[] data, Uri conversionTable)
        {
            var strings = new List<string>();
            var characters = new BFMUReader();
            // TODO: Move over to Asset-based solution that loads this with the requested data
            characters.LoadAsset(conversionTable.OriginalString);

            using (var memoryStream = new MemoryStream(data))
            {
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    if (Encoding.ASCII.GetString(binaryReader.ReadBytes(4)) != "BFST")
                        throw new Exception("Not a valid BFST file!");

                    /* 
                     * 4 bytes - magic number - BFST 
                     * 4 bytes - ???
                     * 4 bytes - string count
                     * for each string:
                     *    - 4 bytes - string offset (from end of string count bytes, so add 12)
                     */

                    binaryReader.ReadUInt32();
                    var stringCount = binaryReader.ReadUInt32();

                    var offsets = new List<uint>();
                    for (var i = 0; i < stringCount; ++i)
                        offsets.Add(binaryReader.ReadUInt32());

                    /* 
                     * for each string, we then have:
                     * 1 byte - ALWAYS 0x01? maybe encoding
                     * 3 bytes - length of string
                     * n bytes - characters
                     * each character comes from the BFMU file "MBToUni.dat"
                     */

                    foreach (uint offset in offsets)
                    {
                        binaryReader.BaseStream.Seek(offset + 12, SeekOrigin.Begin);
                        byte byte0;
                        do
                        {
                            byte0 = binaryReader.ReadByte();
                        } while (byte0 != 0x01);
                        var byte1 = binaryReader.ReadByte();
                        var byte2 = binaryReader.ReadByte();
                        var byte3 = binaryReader.ReadByte();
                        var stringLength = ((uint)byte3 << 16 | (uint)byte2 << 8 | byte1);
                        var str = "";
                        for (var i = 0; i < stringLength; ++i)
                        {
                            var b = binaryReader.ReadByte();

                            str += characters.GetChar(b);
                        }
                        binaryReader.ReadInt32();
                        strings.Add(str);
                    }
                }
            }
            return new List<string>(strings);
        }
    }
}
