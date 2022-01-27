using QuazarAPI.Networking.Standard;
using SimTheme_Park_Online;
using SimTheme_Park_Online.Data;
using SimTheme_Park_Online.Data.Primitive;
using SimTheme_Park_Online.Data.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TPWSE.ClientServices.Clients
{
    /// <summary>
    /// A standardized client of the TPW-SE CityServer
    /// </summary>
    public sealed class CityClient : QuazarClient
    {
        public enum StatusCodes
        {
            READY,
            CONNECTING,
            CONNECTED,
            DISCONNECTED,
            SENDING_DATA,
            THEMEINFO,
            LOGICALINFO,
            RIDEINFO,
            CHATINFO,
            CITIESINFO
        }
        /// <summary>
        /// The current status of this server
        /// </summary>
        public StatusCodes Status { get; private set; }
        /// <summary>
        /// Invoked on status update
        /// </summary>
        public event EventHandler<StatusCodes> StatusChanged;

        /// <summary>
        /// The city responses from the server. See: <see cref="AttemptConnectionAsync"/>
        /// </summary>
        public IEnumerable<TPWCityInfo> Cities { get; private set; }
        public IEnumerable<TPWParkInfo> OnlineParks { get; private set; }              

        public uint CurrentPacketQueue = 0x0A;

        public CityClient(IPAddress Connection, int Port) : base("CityClient", Connection, Port)
        {

        }

        private void changeStatus(StatusCodes status)
        {
            Status = status;
            StatusChanged?.DynamicInvoke(this, status);
        }

        public async Task AttemptConnectionAsync()
        {
            changeStatus(StatusCodes.CONNECTING);
            try
            {
                await Connect();
            }
            catch (Exception ex)
            {
                changeStatus(StatusCodes.DISCONNECTED);
                throw;
            }
            changeStatus(StatusCodes.CONNECTED);

            await doCityHandshake();
        }

        /// <summary>
        /// Requests the server to supply all the parks in a city.
        /// </summary>
        /// <param name="City">The city to get information on</param>
        /// <returns></returns>
        /// <exception cref="SocketException">If the server isn't connected with this client, an exception will be thrown.</exception>
        public async Task<IEnumerable<TPWParkInfo>> GetParksByCity(TPWCityInfo City)
        {
            if (!IsConnected)
                throw new SocketException(404);
            TPWPacket packet = new TPWPacket()
            {
                OriginCode = TPWConstants.Bc_Header,
                MessageType = 0,
                PacketQueue = CurrentPacketQueue
            };
            packet.EmplaceBody((TPWZeroTerminatedString)("CITYID=" + City.CityID), true);
            await SendPacket(packet);
            packet.Dispose();
            packet = await AwaitPacket();
            return TPWParkResponseStructure.FromPacket(packet).Select(x => x.ToParkInfo());
        }

        /// <summary>
        /// Requests the server to supply all the parks by a user.
        /// </summary>
        /// <param name="username">The name of the user. 
        /// <para>
        /// TPW-SE's search algorithm is improved over the original game's strategy. 
        /// In TPW-SE, the search term can be partial, complete, all caps, lowercase. 
        /// For the most accurate results, make sure to spell the name entirely correctly with capitalization.
        /// </para> </param>
        /// <returns></returns>
        /// <exception cref="SocketException"></exception>
        public async Task<IEnumerable<TPWParkInfo>> GetParksByUser(TPWUnicodeString username)
        {
            if (!IsConnected)
                throw new SocketException(404);
            TPWPacket packet = new TPWPacket()
            {
                OriginCode = TPWConstants.Bc_Header,
                MessageType = 0,
                PacketQueue = CurrentPacketQueue
            };
            packet.EmplaceBody((TPWZeroTerminatedString)("OWNERNAME=" + string.Join('\0', username.GetBytes(false))), true);
            await SendPacket(packet);
            packet.Dispose();
            packet = await AwaitPacket();
            return TPWParkResponseStructure.FromPacket(packet).Select(x => x.ToParkInfo());
        }

        private async Task doCityHandshake()
        {
            changeStatus(StatusCodes.SENDING_DATA);
            //Make a request of all data the server has to send before valid connection can be established
            await SendPackets(
                new TPWPacket() { OriginCode = TPWConstants.Bc_Header, PacketQueue = 0x0A }, // theme info
                new TPWPacket() { OriginCode = TPWConstants.Bc_Header, PacketQueue = 0x0B }, // logical servers
                new TPWPacket() { OriginCode = TPWConstants.Bc_Header, PacketQueue = 0x0C }, // ride info
                new TPWPacket() { OriginCode = TPWConstants.Bc_Header, PacketQueue = 0x0D }, // chat servers
                new TPWPacket() { OriginCode = TPWConstants.Bc_Header, PacketQueue = 0x0E }); // cities
            TPWPacket themeInfo = null;
            TPWPacket logicalInfo = null;
            TPWPacket rideInfo = null;
            TPWPacket chatInfo = null;
            TPWPacket citiesInfo = null;   
            changeStatus(StatusCodes.THEMEINFO);
            for (int i = 0; i < 5; i++)
            {                       
                var packet = await AwaitPacket();
                switch (packet.PacketQueue)
                {
                    case 0x0A: themeInfo = packet; break;
                    case 0x0B: logicalInfo = packet; break;
                    case 0x0C: rideInfo = packet; break;
                    case 0x0D: chatInfo = packet; break;
                    case 0x0E: citiesInfo = packet; break;
                }
                changeStatus(Status + 1);
            }
            CurrentPacketQueue = 0x0F;
            OnlineParks = TPWParkResponseStructure.FromPacket(chatInfo).Select(x => x.ToParkInfo());
            Cities = TPWCityInfoStructure.FromPacket(citiesInfo).Select(x => x.ToCityInfo());
            changeStatus(StatusCodes.READY);
        }
    }
}
