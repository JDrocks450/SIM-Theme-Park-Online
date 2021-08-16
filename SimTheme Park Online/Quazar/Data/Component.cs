using QuazarAPI.Networking.Data;
using SimTheme_Park_Online;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace QuazarAPI.Networking.Standard
{
    public abstract class Component
    {
        public string Name
        {
            get; set;
        }

        protected static int ReceiveAmount => 1000;

        public int PORT
        {
            get; set;
        }

        public SIMThemeParkWaypoints ThisWaypoint
        {
            get; protected set;
        }

        public uint BACKLOG
        {
            get;set;
        }

        private TcpListener listener;
        private Dictionary<uint, TcpClient> _clients = new Dictionary<uint, TcpClient>();
        private Dictionary<uint, ClientInfo> _clientInfo = new Dictionary<uint, ClientInfo>();

        public ObservableCollection<TPWPacket> IncomingTrans = new ObservableCollection<TPWPacket>(),
                                            OutgoingTrans = new ObservableCollection<TPWPacket>();

        public event EventHandler<ClientInfo> OnConnectionsUpdated;

        protected Socket Host => listener.Server;
        protected IDictionary<uint, TcpClient> Connections => _clients;       

        protected Component(string name, SIMThemeParkWaypoints Waypoint, uint Backlog = 1) : this(name, (int)Waypoint, Waypoint, Backlog)
        {

        }

        protected Component(string name, int port, SIMThemeParkWaypoints Waypoint = SIMThemeParkWaypoints.External, uint backlog = 1)
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
                QConsole.WriteLine("Error Detected on Client: " + ID);
                Disconnect(ID);
                return false;
            }
            try
            {
                var packet = TPWPacket.Parse(dataBuffer);
                QConsole.WriteLine("Incoming packet was successfully parsed.");
                IncomingTrans.Add(packet);
                return true;
            }
            catch (Exception ParseException)
            {
                QConsole.WriteLine("Couldn't parse incoming TPWPacket! " + ParseException.Message);
            }
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
            void Ready()
            {
                dataBuffer = new byte[ReceiveAmount];
                try
                {                    
                    //This will handle the client forcibly closing connection by raising an exception the moment connection is lost.
                    //Therefore, this condition is handled here
                    connection.Client.BeginReceive(dataBuffer, 0, ReceiveAmount, SocketFlags.None, OnRecieve, null);
                }
                catch(Exception e)
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
            uint id = (uint)_clients.Count;
            _clients.Add(id, newConnection);
            OnClientConnect(newConnection, id);
            Ready();
        }

        public abstract void Start();
        public abstract void Stop();
        public IEnumerable<ClientInfo> GetAllConnectedClients()
        {
            foreach (var connection in _clientInfo)
                yield return connection.Value;
        }
        public IEnumerable<uint> GetAllWaypointClients(SIMThemeParkWaypoints wayPoint) => _clientInfo.Where(x => x.Value.Me == wayPoint).Select(x => x.Key);
        public ClientInfo GetClientInfoByID(uint ID) => _clientInfo[ID];

        public void Disconnect(uint id)
        {
            var client = Connections[id];
            client.Close();
            _clients.Remove(id);
            OnConnectionsUpdated?.Invoke(this, null);
            QConsole.WriteLine($"[{Name}] Disconnected Client {id}");
        }

        protected void StopListening()
        {
            listener.Stop();
        }

        /// <summary>
        /// Handles the command using a default response, if possible
        /// </summary>
        /// <param name="datagram"></param>
        protected bool HandleCommand(uint ID, TPWPacket datagram)
        {
            switch (datagram.ResponseCode)
            {
                default: return false;
            }
            return true;
        }

        protected void BeginListening()
        {            
            listener.Start();
            QConsole.WriteLine($"[{Name}] Listening\nIP: {listener.LocalEndpoint}\nPORT: {PORT}");
            Ready();
        }      

        protected virtual void OnClientConnect(TcpClient Connection, uint ID)
        {
            QConsole.WriteLine($"\n[{Name}] Client Connected\nIP: {Connection.Client.RemoteEndPoint}\nID: {ID}");
            //GetAllInformation(Connection);
            BeginReceive(Connection, ID);
        }

        protected virtual void OnConnected(TcpClient Connection, uint ID)
        {
            QConsole.WriteLine($"\n[{Name}] Connected to Server\nIP: {Connection.Client.RemoteEndPoint}\nID: {ID}");
            BeginReceive(Connection, ID);
        }

        protected abstract void OnIncomingPacket(Socket Sender, uint ID, TPWPacket Data);

        protected virtual void OnForceClose(TcpClient socket, uint ID)
        {
            OnConnectionsUpdated?.Invoke(this, null);
            QConsole.WriteLine($"[{Name}] Client forcefully disconnected: {ID}");
        }

        /// <summary>
        /// Connects to a server component
        /// </summary>
        public uint Connect(IPAddress address, int port)
        {
            var newConnection = ConnectionHelper.Connect(address, port);
            uint id = (uint)_clients.Count;
            _clients.Add(id, newConnection);
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
            //Send(ID, GetMyClientInfo(port));
            return ID;
        }

        /// <summary>
        /// Sends data to a connected client by ID
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Data"></param>
        public void Send(uint ID, string Data) => Send(ID, Encoding.ASCII.GetBytes(Data));

        protected void Send(uint ID, byte[] s_buffer)
        {
            try
            {
                var cxInfo = _clientInfo[ID];
                QConsole.WriteLine($"[{Name}] SENDING {s_buffer.Length} bytes TO {cxInfo.Me}");
            }
            catch
            {
                QConsole.WriteLine($"[{Name}] SENDING {s_buffer.Length} bytes TO {ID}: \n" +
                    $"{string.Join(' ', s_buffer)}\n" +
                    $"TRANSLATION: \n" +
                    $"{Encoding.ASCII.GetString(s_buffer)}");
            }            
            Connections[ID].GetStream().Write(s_buffer,0,s_buffer.Length);
        }

        protected void Send(uint ID, TPWPacket Data)
        {
            OutgoingTrans.Add(Data);
            Send(ID, Data.GetBytes());
        }
    }
}
