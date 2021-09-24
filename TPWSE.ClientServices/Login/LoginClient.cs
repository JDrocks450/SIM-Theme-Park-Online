using QuazarAPI;
using QuazarAPI.Networking.Standard;
using SimTheme_Park_Online;
using SimTheme_Park_Online.Data.Primitive;
using SimTheme_Park_Online.Data.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace TPWSE.ClientServices.Login
{
    public class LoginClient : QuazarClient
    {
        public LoginClient(IPAddress Connection, int Port) : base("LoginClient", Connection, Port)
        {

        }

        private TPWPacket GenerateLoginPacket(string Username, string Password)
        {
            TPWPacket packet = new TPWPacket()
            {
                MsgType = 0x0,
            };
            packet.AllocateBody(130);
            packet.EmplaceBody((TPWUnicodeString)Username, false);
            packet.SetPosition((int)packet.BodyPosition + 0x30);
            packet.EmplaceBody(Password.ToUZ(), false);
            return packet;
        }

        public async Task<TPWLoginAuthPacket> AttemptLogin(string Username, string Password)
        {
            try
            {
                await Connect();
            }
            catch (Exception e)
            {
                QConsole.WriteLine(Name, "Could not connect to the server.");
                return null;
            }
            await SendPackets(GenerateLoginPacket(Username, Password));
            using (TPWPacket packet = await AwaitPacket())
            {
                return new TPWLoginAuthPacket(packet);
            }
        }
    }
}
