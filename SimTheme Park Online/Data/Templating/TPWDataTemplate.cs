using MiscUtil.Conversion;
using QuazarAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Data.Templating
{
    public enum TPWSystemTypes : int
    {
        NONE = -999,
        /// <summary>
        /// A word, 16 bit unsigned integer
        /// </summary>
        WORD = 2,
        /// <summary>
        /// A double-word, 32 bit unsigned integer
        /// </summary>
        DWORD = 4,
        /// <summary>
        /// A byte
        /// </summary>
        BYTE = 1,
        /// <summary>
        /// ASCII string
        /// </summary>
        ASCII_STR = -1,
        /// <summary>
        /// Unicode String
        /// </summary>
        UNI_STR = -2,
        BYTE_ARR = 5
    }
    /// <summary>
    /// The individual template definition that represents a piece of information in a packet.
    /// </summary>
    [Serializable]
    public class TPWTemplateDefinition
    {
        /// <summary>
        /// The type of information we're representing
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TPWSystemTypes DataType
        {
            get; set;
        }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string Color { get; set; } = "#FFFFFFFF";
        /// <summary>
        /// The start of the templated data
        /// </summary>
        public uint StartOffset { get; set; }
        /// <summary>
        /// The end of the templated data
        /// </summary>
        public uint EndOffset { get; set; }
        /// <summary>
        /// The length, in bytes, of the templated data
        /// </summary>
        public uint Length => EndOffset - StartOffset;

        public TPWTemplateDefinition()
        {

        }
        public TPWTemplateDefinition(uint Start, uint Length, string Name, string Desc, TPWSystemTypes Type, string Color = default)
        {
            StartOffset = Start;
            this.Name = Name;
            this.Desc = Desc;
            this.Color = Color ?? "#FFFFFFFF";
            EndOffset = Start + Length;
            DataType = Type;
        }

        /// <summary>
        /// Judging by length, returns possible recognizable data types this object could be
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TPWSystemTypes> GetPossibleTypes() => GetPossibleTypes(Length);

        public static IEnumerable<TPWSystemTypes> GetPossibleTypes(uint Length)
        {
            foreach (int value in Enum.GetValues(typeof(TPWSystemTypes)))
            {
                if (value == -999) continue;
                if (value < 0)
                {
                    int tempVal = value * -1;
                    if (Length >= tempVal)
                        yield return (TPWSystemTypes)value;
                    continue;
                }
                if (Length < value) continue;
                if (value == Length)// || Length % value == 0)
                {
                    yield return (TPWSystemTypes)value;
                    continue;
                }
            }
        }

        /// <summary>
        /// Converts a <see cref="TPWSystemTypes"/> value into a <see cref="Type"/> object.
        /// </summary>
        /// <returns><see langword="object"/> by <see langword="default"/></returns>
        public static Type GetTypeForSystemType(TPWSystemTypes SysType)
        {
            switch (SysType)
            {
                case TPWSystemTypes.BYTE:
                    return typeof(byte);
                case TPWSystemTypes.WORD:
                    return typeof(ushort);
                case TPWSystemTypes.DWORD:
                    return typeof(uint);
                case TPWSystemTypes.ASCII_STR:
                case TPWSystemTypes.UNI_STR:
                    return typeof(string);
                default: return typeof(object);
            }
        }

        /// <summary>
        /// Finds this data in the source, and reflects it into a data value.
        /// </summary>
        /// <typeparam name="T">Destination Data Type</typeparam>
        /// <param name="Source"></param>
        /// <returns></returns>
        public T Reflect<T>(byte[] Source) => (T)Reflect(Source);
        public object Reflect(byte[] Source) => Reflect(DataType, Source, (int)StartOffset, (int)Length);

        public static object Reflect(TPWSystemTypes DataType, byte[] Source, int StartOffset, int Length = 0)
        {
            try
            {
                switch (DataType)
                {
                    default:
                    case TPWSystemTypes.NONE:
                        return string.Join(' ', Source);
                    case TPWSystemTypes.BYTE:
                        return Source[0];
                    case TPWSystemTypes.WORD:
                        return EndianBitConverter.Big.ToUInt16(Source, StartOffset);
                    case TPWSystemTypes.DWORD:
                        return EndianBitConverter.Big.ToUInt32(Source, StartOffset);
                    case TPWSystemTypes.ASCII_STR:
                        return Encoding.ASCII.GetString(Source.Skip(StartOffset).Take(Length).ToArray());
                    case TPWSystemTypes.UNI_STR:
                        return Encoding.Unicode.GetString(Source.Skip(StartOffset).Take(Length).ToArray());
                }
            }
            catch(Exception e)
            {
                QConsole.WriteLine("TPWDataTemplate.cs", $"An error occured reflecting bytes into an object: {e.Message}");
            }
            return null;
        }

        /// <summary>
        /// Displays the data contained in this definition using the source provided.
        /// </summary>
        /// <returns></returns>
        public string Represent(byte[] Source, string Parameter = default)
        {
            var value = Reflect(Source);
            if (value == null)
                return "ERROR";
            if (Parameter == "HEX")
            {
                switch(DataType)
                {
                    case TPWSystemTypes.BYTE:
                        return "0x" + ((byte)value).ToString("X2");
                    case TPWSystemTypes.WORD:
                        return "0x" + ((ushort)value).ToString("X4");
                    case TPWSystemTypes.DWORD:
                        return "0x" + ((uint)value).ToString("X8");
                }
            }
            return value.ToString();
        }
    }
    /// <summary>
    /// Represents a means of templating the raw data of a packet into something easily recognizable and debuggable.
    /// </summary>
    [Serializable]
    public class TPWDataTemplate
    {
        /// <summary>
        /// Be smart with this, set accessor is open because serialization and other reasons 
        /// </summary>
        public HashSet<TPWTemplateDefinition> Definitions { get; set; } = new HashSet<TPWTemplateDefinition>();
        public event EventHandler OnDefinitionsUpdated;

        public string FileName { get; set; }

        public TPWDataTemplate()
        {

        }

        public static TPWDataTemplate Load(string FilePath)
        {
            TPWDataTemplate returnValue = null;
            using (var fs = File.OpenText(FilePath))
                returnValue = JsonSerializer.Deserialize<TPWDataTemplate>(fs.ReadToEnd());
            if (returnValue == null) throw new Exception("Template Load Failure, I don't really know why." +
                " I have a guess, but programming a check for this is useless sorry.");
            returnValue.FileName = FilePath;
            return returnValue;
        }

        public void Save(string FileName = default)
        {
            if (FileName == default)
                FileName = this.FileName;
            using (var fs = File.Create(FileName))
                fs.Write(JsonSerializer.SerializeToUtf8Bytes(this));
            this.FileName = FileName;
        }

        public bool Add(TPWTemplateDefinition Definition)
        {
            if (TryGetByOffsets(Definition.StartOffset, Definition.EndOffset, out var definition))
                Definitions.Remove(definition);
            Definitions.Add(Definition);
            OnDefinitionsUpdated?.Invoke(this, null);
            return true;
        }

        public bool Remove(TPWTemplateDefinition Definition)
        {
            bool val = Definitions.Remove(Definition);
            OnDefinitionsUpdated?.Invoke(this, null);
            return val;
        }

        public bool TryGetByOffsets(uint Start, uint End, out TPWTemplateDefinition definition)
        {
            var data = Definitions.Where(x => x.StartOffset == Start || x.EndOffset == End || (Start < x.EndOffset && Start >= x.StartOffset));
            definition = data.FirstOrDefault();
            if (data.Any())
                return true;
            return false;
        }
    }
}
