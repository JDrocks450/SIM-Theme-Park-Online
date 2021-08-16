using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using QuazarAPI.Networking.Standard;
using System.Text.Json;

namespace QuazarAPI.Networking.Data
{
    [Serializable]
    public class Datagram
    {
        public const string HEADER = "OTP#DG BEGIN";
        public static int HEADER_LENGTH => HEADER.Length;
        public static int ReceiveAmount
        {
            get; set;
        } = 250;
        public int Sender
        {
            get; set;
        }        
        public SIMThemeParkWaypoints SenderWaypoint => (SIMThemeParkWaypoints)Sender;

        public int Receiver
        {
            get; set;
        }
        public SIMThemeParkWaypoints ReceiverWaypoint => (SIMThemeParkWaypoints)Receiver;

        public string Command
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets whether the body of the Datagram is definitely serialized JSON
        /// </summary>
        public bool IsSerializedBody
        {
            get; set;
        } = false;

        public string Data
        {
            get; set;
        }

        private static JsonSerializerOptions Default => new JsonSerializerOptions()
                                                            {
                                                                IgnoreReadOnlyProperties = true,
                                                                WriteIndented = false
                                                            };

        public Datagram()
        {

        }

        public Datagram(SIMThemeParkWaypoints From, SIMThemeParkWaypoints To, string Command, string Data) : this()
        {
            Sender = (int)From;
            Receiver = (int)To;
            this.Command = Command;
            this.Data = Data;
        }

        public Datagram(SIMThemeParkWaypoints From, SIMThemeParkWaypoints To, string Command, object Data, Type DataType) : 
            this(From, To, Command, SerializeObject(Data, DataType))
        {
            IsSerializedBody = true;
        }        
       
        public static bool CheckHeader(byte[] header)
        {
            if (header[0] != 'O') return false;
            if (header.Length < HEADER_LENGTH) return false;
            header = header.Take(HEADER_LENGTH).ToArray();
            var headerStr = Encoding.ASCII.GetString(header);
            return headerStr == HEADER;
        }

        public static Datagram Deserialize(byte[] source)
        {
            if (!CheckHeader(source))
                return null;
            source = source.Skip(HEADER_LENGTH).ToArray();
            string json = Encoding.ASCII.GetString(source);
            json = json.TrimEnd('\0');
            return JsonSerializer.Deserialize<Datagram>(json);
        }

        public static string SerializeObject(object Object, Type type) => JsonSerializer.Serialize(Object, type, Default);
        public static string SerializeObject<T>(T Object) => JsonSerializer.Serialize<T>(Object, Default);
        public string Serialize() => HEADER + JsonSerializer.Serialize(this, Default);

        public string GetBody() => Data;

        public T GetBody<T>()
        {
            if (!IsSerializedBody) return default(T);
            string json = Data;
            var serializedValue = JsonSerializer.Deserialize<T>(json);
            return serializedValue;
        }

        public override string ToString()
        {
            return $"[OTPDatagram] {Command} | {Data}";
        }
    }
}
