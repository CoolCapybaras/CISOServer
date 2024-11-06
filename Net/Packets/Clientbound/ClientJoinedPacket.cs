using CISOServer.Core;

namespace CISOServer.Net.Packets.Clientbound
{
	public class ClientJoinedPacket : IPacket
	{
		public int id = 7;

		public Client client;

		public ClientJoinedPacket(Client client)
		{
			this.client = client;
		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			throw new NotImplementedException();
		}
	}
}
