using SimTheme_Park_Online;
using SimTheme_Park_Online.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuazarAPI.Networking.Standard
{
    /// <summary>
    /// A wrapper for the <see cref="TcpClient"/> class
    /// </summary>
    public abstract class QuazarClient : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public bool IsConnected => _client != null && _client.Connected;
        protected TcpClient _client;
        protected Task _recvTask;
        protected bool _recvStop = true;
        protected readonly List<ArraySegment<byte>> _recvQueue;
        private ManualResetEvent _recvInvoke;

        public QuazarClient(string Name, IPAddress Address, int Port)
        {
            if (Address is null)
            {
                throw new ArgumentNullException(nameof(Address));
            }

            this.Name = Name;
            this.Address = Address;
            this.Port = Port;
            _recvQueue = new List<ArraySegment<byte>>();
            _recvInvoke = new ManualResetEvent(false);

            _client = new TcpClient();
        }

        public string Name { get; }
        public IPAddress Address { get; }
        public int Port { get; }
        public uint ReceiveAmount => TPWConstants.TPWSendLimit;

        public async Task Connect()
        {
            await _client.ConnectAsync(Address, Port);
            _recvTask = new Task(StartReceiveAsync);
            _recvStop = false;
            _recvTask.Start();            
        }
        public async Task<int> Send(byte[] Data) => 
            await _client.Client.SendAsync(new ArraySegment<byte>(Data, 0, Data.Length), SocketFlags.None);
        public async Task SendPacket(TPWPacket Packet) => await Send(Packet.GetBytes());
        public async Task SendPackets(params TPWPacket[] Packets)
        {
            foreach (TPWPacket packet in Packets)
                await SendPacket(packet);
        }
        public async Task<TPWPacket> AwaitPacket()
        {
            retry:
            var Data = await AwaitResponse();            
            using (MemoryStream networkData = new MemoryStream())
            {
                await networkData.WriteAsync(Data.Array, 0, Data.Count);                    
                while (true)
                {
                    try
                    {
                        var packet = TPWPacket.Parse(networkData.ToArray(), out int EndIndex);
                        networkData.Position = EndIndex;
                        byte[] remaining = new byte[networkData.Length - EndIndex];
                        if (remaining.Length > 0)
                        {
                            networkData.Read(remaining, 0, remaining.Length);
                            _recvQueue.Insert(0, new ArraySegment<byte>(remaining));
                            _recvInvoke.Set();
                        }
                        return packet;
                    }
                    catch(ArgumentException)
                    {
                        QConsole.WriteLine(Name, "Partial data received...");
                    }
                    catch (FormatException ex)
                    {
                        QConsole.WriteLine(Name, ex.ToString());
                    }
                    Data = await AwaitResponse();
                    await networkData.WriteAsync(Data.Array, 0, Data.Count);
                }                
            }
        }
        public Task<ArraySegment<byte>> AwaitResponse()
        {
            return Task.Run(delegate
            {
                if (!_recvQueue.Any())
                {
                    _recvInvoke.WaitOne();
                    _recvInvoke.Reset();
                }
                while (!_recvQueue.Any()) { }
                ArraySegment<byte> Data = _recvQueue.First();
                _recvQueue.RemoveAt(0);
                return Data;
            });
        }

        private async void StartReceiveAsync()
        {
            while (!_recvStop)
            {
                try
                {
                    ArraySegment<byte> Data = await awaitData();
                    _recvQueue.Add(Data);
                    _recvInvoke.Set();
                }
                catch (SocketException e) // Connection Error
                {
                    QConsole.WriteLine(Name, "A connection error occured: " + e.Message);
                    break;
                }                
            }
        }
        private async Task<ArraySegment<byte>> awaitData()
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(new byte[ReceiveAmount]);
            int readValue = await _client.Client.ReceiveAsync(segment, SocketFlags.None);
            byte[] data = segment.Array;
            Array.Resize(ref data, readValue);
            return new ArraySegment<byte>(data);
        }

        public void Dispose()
        {
            if (IsDisposed) return;
            _recvStop = true;
            _recvTask?.Wait();
            _client.Dispose();
            IsDisposed = true;
        }
    }
}
