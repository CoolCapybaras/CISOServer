using CISOServer.Core;
using CISOServer.Gamelogic;

namespace CISOServer.Net.Packets.Clientbound
{
	public class ClientJoinedPacket : IPacket
	{
		public int id = 11;

		public Player client;

		public ClientJoinedPacket(Player client)
		{
			this.client = client;
		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			throw new NotImplementedException();
		}
	}
}
