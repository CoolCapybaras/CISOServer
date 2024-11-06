using CISOServer.Core;

namespace CISOServer.Net.Packets.Clientbound
{
	public class LobbyJoinedPacket : IPacket
	{
		public int id = 6;

		public List<Client> clients;

		public LobbyJoinedPacket(List<Client> clients)
		{
			this.clients = clients;
		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			throw new NotImplementedException();
		}
	}
}
