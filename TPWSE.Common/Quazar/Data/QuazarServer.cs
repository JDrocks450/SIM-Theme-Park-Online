using QuazarAPI.Networking.Data;
using SimTheme_Park_Online;
using SimTheme_Park_Online.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuazarAPI.Networking.Standard
{
    public abstract class QuazarServer
    {        
        /// <summary>
        /// The name of this <see cref="QuazarServer"/>
        /// </summary>
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// The amount of data to receive per transmission
        /// </summary>
        public int ReceiveAmount { get; protected set; } = 1000;
        public int SendAmount { get; set; } = SimTheme_Park_Online.Data.TPWConstants.TPWSendLimit;

        /// <summary>
        /// The port of this <see cref="TcpListener"/>
        /// </summary>
        public int PORT
        {
            get; set;
        }

        /// <summary>
        /// This waypoint in the server cluster, not currently used.
        /// </summary>
        public SIMThemeParkWaypoints ThisWaypoint
        {
            get; protected set;
        }

        /// <summary>
        /// The amount of connections accepted.
        /// </summary>
        public uint BACKLOG
        {
            get;set;
        }

        /// <summary>
        /// All packets sent through <see cref="Send"/> functions will be ignored except when sent from  
        /// </summary>
        public bool ManualMode
        {
            get; set;
        } = false;

        /// <summary>
        /// The current packet queue, dont use this i fucked it.
        /// </summary>
        public uint PacketQueue
        {
            get; protected set;
        } = 0x0A;

        /// <summary>
        /// Sets whether or not incoming / outgoing packets are cached.
        /// </summary>
        /// <param name="Enabled"></param>
        protected void SetPacketCaching(bool Enabled) =>
            _packetCache =
#if DEBUG 
            true;
#else
            Enabled;
#endif

        private TcpListener listener;
        private Dictionary<uint, TcpClient> _clients = new Dictionary<uint, TcpClient>();
        private Dictionary<uint, ClientInfo> _clientInfo = new Dictionary<uint, ClientInfo>();

        public ObservableCollection<TPWPacket> IncomingTrans = new ObservableCollection<TPWPacket>(),
                                            OutgoingTrans = new ObservableCollection<TPWPacket>();
        public ObservableCollection<ClientInfo> ConnectionHistory = new ObservableCollection<ClientInfo>();
        private bool _packetCache = true;

        public event EventHandler<ClientInfo> OnConnectionsUpdated;

        protected Socket Host => listener.Server;
        protected IDictionary<uint, TcpClient> Connections => _clients;
        protected Dictionary<SIMThemeParkWaypoints, uint> KnownConnectionMap = new Dictionary<SIMThemeParkWaypoints, uint>();

        protected Queue<(uint ID, byte[] Buffer)> SendQueue = new Queue<(uint ID, byte[] Buffer)>();
        protected Thread SendThread;
        protected ManualResetEvent SendThreadInvoke;

        /// <summary>
        /// Creates a <see cref="QuazarServer"/> with the specified parameters.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="port"></param>
        /// <param name="Waypoint"></param>
        /// <param name="backlog"></param>
        protected QuazarServer(string name, SIMThemeParkWaypoints Waypoint, uint Backlog = 1) : this(name, (int)Waypoint, Waypoint, Backlog)
        {

        }

        /// <summary>
        /// Creates a <see cref="QuazarServer"/> with the specified parameters.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="port"></param>
        /// <param name="Waypoint"></param>
        /// <param name="backlog"></param>
        protected QuazarServer(string name, int port, SIMThemeParkWaypoints Waypoint = SIMThemeParkWaypoints.External, uint backlog = 1)
        {
            PORT = port;
            BACKLOG = backlog;
            Name = name;
            ThisWaypoint = Waypoint;
            Init();            
        }

        private void Init()
        {
            listener = new TcpListener(IPAddress.Loopback, PORT);
            listener.Server.SendBufferSize = SendAmount;

            SendThreadInvoke = new ManualResetEvent(false);
            SendThread = new Thread(doSendLoop);
            SendThread.Start();
        }

        private void doSendLoop()
        {
            while (true)
            {
                SendThreadInvoke.WaitOne();
                while (SendQueue.Count > 0)
                {
                    var data = SendQueue.Dequeue();
                    uint ID = data.ID;
                    byte[] s_buffer = data.Buffer;

                    //send data
                    if (Connections.ContainsKey(ID))
                    {
                        try
                        {
                            var connection = Connections[ID];
                            byte[] network_buffer = new byte[SendAmount];
                            using (var buffer = new MemoryStream(s_buffer)) {
                                int sentAmount = 0;
                                do
                                {
                                    sentAmount = buffer.Read(network_buffer, 0, SendAmount);
                                    connection.GetStream().Write(network_buffer, 0, sentAmount);
                                    if (s_buffer.Length > SendAmount)
                                        QConsole.WriteLine("System", $"{Name} - An outgoing packet was large ({s_buffer.Length} bytes) sent a chunk of {sentAmount}");
                                }
                                while (sentAmount == SendAmount);
                            }
                        }
                        catch (IOException exc)
                        {
                            QConsole.WriteLine(Name, $"ERROR: {exc.Message}");
                        }
                        catch (InvalidOperationException invalid)
                        {
                            QConsole.WriteLine(Name, $"ERROR: {invalid.Message}");
                        }
                    }
                    else QConsole.WriteLine(Name, $"Tried to send data to a disposed connection.");
                }
                SendThreadInvoke.Reset();
            }
        }

        private void Ready() => listener.BeginAcceptTcpClient(AcceptConnection, listener);

        private void GetAllInformation(TcpClient client)
        {
            foreach(var str in Enum.GetNames(typeof(SocketOptionName))) {                
                Console.Write(str + ": ");
                try
                {
                    Console.WriteLine(
                        client.Client.GetSocketOption(SocketOptionLevel.Tcp, (SocketOptionName)Enum.Parse(typeof(SocketOptionName), str)));                        
                }
                catch
                {
                    Console.WriteLine("ERROR");
                }
            }
        }

        protected virtual bool OnReceive(uint ID, byte[] dataBuffer)
        {
            if (dataBuffer.Where(x => x == 0).Count() == dataBuffer.Length)
            {
                QConsole.WriteLine(Name, $"Error Detected on Client: " + ID);
                Disconnect(ID);
                return false;
            }
            int amount = 0;

            byte[] readBuffer = new byte[dataBuffer.Length];
            dataBuffer.CopyTo(readBuffer, 0);
            var packets = TPWPacket.ParseAll(ref readBuffer);
            foreach (var packet in packets)
            {
                QConsole.WriteLine("System", $"{Name}: Incoming packet was successfully parsed.");
                packet.Received = DateTime.Now;
                if (_packetCache)
                    IncomingTrans.Add(packet);
                OnIncomingPacket(ID, packet);
            }
            QConsole.WriteLine("System", $"{Name}: Found {packets.Count()} Packets...");
            return true;
        }

        private void BeginReceive(TcpClient connection, uint ID)
        {            
            byte[] dataBuffer = null;
            void OnRecieve(object state)
            {
                if (this.OnReceive(ID, dataBuffer))
                    Ready();
                else return;
            }

            if (connection.Client.Available != 0)
            {
                dataBuffer = new byte[connection.Client.Available];
                connection.Client.Receive(dataBuffer);
                OnRecieve(null);
                return;
            }            
            void Ready()
            {
                dataBuffer = new byte[ReceiveAmount];
                try
                {                    
                    //This will handle the client forcibly closing connection by raising an exception the moment connection is lost.
                    //Therefore, this condition is handled here
                    connection?.Client.BeginReceive(dataBuffer, 0, ReceiveAmount, SocketFlags.None, OnRecieve, null);
                }
                catch(SocketException e)
                {
                  //Raise event and disconnect problem client
                  OnForceClose(connection, ID);
                  Disconnect(ID);
                  return;
                }
            }
            Ready();
        }        
        
        private void AcceptConnection(IAsyncResult ar)
        {
            var newConnection = listener.EndAcceptTcpClient(ar);
            uint id = awardID();
            _clients.Add(id, newConnection);            
            OnClientConnect(newConnection, id);
            Ready();
        }

        public abstract void Start();
        public abstract void Stop();
        public IEnumerable<(uint ID, TcpClient Client)> GetAllConnectedClients()
        {
            foreach (var connection in _clients)
                yield return (connection.Key, connection.Value);
        }
        public IEnumerable<uint> GetAllWaypointClients(SIMThemeParkWaypoints wayPoint) => _clientInfo.Where(x => x.Value.Me == wayPoint).Select(x => x.Key);
        public ClientInfo GetClientInfoByID(uint ID) => _clientInfo[ID];

        public void Disconnect(uint id)
        {
            try
            {
                var client = Connections[id];
                client.Close();
            }
            catch (SocketException exc)
            {

            }
            _clients.Remove(id);
            _clientInfo.Remove(id);
            OnConnectionsUpdated?.Invoke(this, null);
            QConsole.WriteLine(Name, $"Disconnected Client {id}");
        }

        protected void StopListening()
        {
            listener.Stop();
        }

        protected void BeginListening()
        {            
            listener.Start();
            QConsole.WriteLine(Name, $"Listening\nIP: {listener.LocalEndpoint}\nPORT: {PORT}");
            Ready();
        }      

        /// <summary>
        /// Called when a client connects to the server and has a <see cref="ClientInfo"/> struct associated.
        /// </summary>
        /// <param name="Connection"></param>
        /// <param name="ID"></param>
        protected virtual void OnClientConnect(TcpClient Connection, uint ID)
        {
            QConsole.WriteLine(Name, $"\nClient Connected\nIP: {Connection.Client.RemoteEndPoint}\nID: {ID}");
            //GetAllInformation(Connection);   
            ClientInfo info = new ClientInfo()
            {
                ID = ID,
                Me = SIMThemeParkWaypoints.External,
                Name = "CLIENT",
                ConnectTime = DateTime.Now
            };
            _clientInfo.Add(ID, info);
            ConnectionHistory.Add(info);
            OnConnectionsUpdated?.Invoke(this, info);
            BeginReceive(Connection, ID);
        }

        protected virtual void OnConnected(TcpClient Connection, uint ID)
        {
            QConsole.WriteLine(Name, $"\nConnected to Server\nIP: {Connection.Client.RemoteEndPoint}\nID: {ID}");
            BeginReceive(Connection, ID);
        }

        protected abstract void OnIncomingPacket(uint ID, TPWPacket Data);
        protected abstract void OnOutgoingPacket(uint ID, TPWPacket Data);   

        protected virtual void OnForceClose(TcpClient socket, uint ID)
        {
            OnConnectionsUpdated?.Invoke(this, null);
            QConsole.WriteLine(Name, $"Client forcefully disconnected: {ID}");
        }

        /// <summary>
        /// Gets an ID that isn't taken. This is not a GUID.
        /// </summary>
        /// <returns></returns>
        private uint awardID() =>        
            UniqueNumber.Generate(_clients.Keys);        

        /// <summary>
        /// Connects to a server component
        /// </summary>
        public uint Connect(IPAddress address, int port)
        {
            var newConnection = ConnectionHelper.Connect(address, port);
            uint id = awardID();
            OnConnected(newConnection, id);
            Ready();
            return id;
        }

        /// <summary>
        /// Connects to a server using the same IP as this server component was created
        /// </summary>
        /// <param name="port"></param>
        protected uint Connect(int port) => Connect(IPAddress.Loopback, port);

        /// <summary>
        /// Connects to a server using the same IP as this server component was created
        /// </summary>
        /// <param name="port"></param>
        protected uint Connect(SIMThemeParkWaypoints port)
        {
            uint ID = Connect(IPAddress.Loopback, (int)port);
            KnownConnectionMap.Add(port, ID);
            return ID;
        }

        protected uint GetIDByWaypoint(SIMThemeParkWaypoints waypoint) => KnownConnectionMap[waypoint];

        /// <summary>
        /// Sends the packet(s) to every connected client
        /// </summary>
        /// <param name="Packets"></param>
        public void Broadcast(params TPWPacket[] Packets)
        {
            var clients = new uint[_clients.Count];
            _clients.Keys.CopyTo(clients, 0);
            foreach(var client in clients)            
                Send(client, Packets);            
        }
        /// <summary>
        /// Sends the packet(s) to the client by ID.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Packets"></param>
        public void Send(uint ID, params TPWPacket[] Packets)
        {
            foreach(var packet in Packets)            
                Send(ID, packet);            
        }
        /// <summary>
        /// Sends raw data to a client by ID.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="s_buffer"></param>
        /// <param name="ManualModeOverride"></param>
        protected void Send(uint ID, byte[] s_buffer, bool ManualModeOverride = false)
        {
            if (ManualMode && !ManualModeOverride)
            {
                QConsole.WriteLine(Name, "ManualMode is Enabled. Use Manual Controls to send the required data.");
                return;
            }
            if (!_clientInfo.TryGetValue(ID, out var cxInfo))
            {
                QConsole.WriteLine(Name, "Couldn't find the client: " + ID);
                return;
            }            
            SendQueue.Enqueue((ID, s_buffer));
            SendThreadInvoke.Set();
        }

        /// <summary>
        /// Sends a packet to a client by ID.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Data"></param>
        /// <param name="ManualModeOverride"></param>
        protected void Send(uint ID, TPWPacket Data, bool ManualModeOverride = false)
        {
            void _doSend(TPWPacket packet)
            {
                packet.Sent = DateTime.Now;
                OnOutgoingPacket(ID, packet);
                PacketQueue++;
                if (_packetCache)
                    OutgoingTrans.Add(packet);
                Send(ID, packet.GetBytes(), ManualModeOverride);
            }
            _doSend(Data);
            if (Data.HasChildPackets)
            {
                foreach (var child in Data.splitPackets)
                    _doSend(child);
                QConsole.WriteLine("System", $"{Name}: Found an outgoing transmission with {Data.ChildPacketAmount} child packets, those have been sent too.");
            }
        }
        protected virtual void OnManualSend(uint ID, ref TPWPacket Data)
        {

        }
        /// <summary>
        /// Sends a packet irrespective of the server being in <see cref="ManualMode"/>
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Data"></param>
        public void ManualSend(uint ID, TPWPacket Data)
        {
            OnManualSend(ID, ref Data);
            Send(ID, Data, true);            
        }
        /// <summary>
        /// Sends packet(s) irrespective of the server being in <see cref="ManualMode"/>
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Data"></param>
        public void ManualSend(uint ID, params TPWPacket[] Packets)
        {
            foreach(var packet in Packets)            
                ManualSend(ID, packet); 
        }
    }
}
