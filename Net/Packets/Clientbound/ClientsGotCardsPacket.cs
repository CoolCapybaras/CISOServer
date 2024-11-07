using CISOServer.Core;

namespace CISOServer.Net.Packets.Clientbound
{
	public class ClientsGotCardsPacket : IPacket
	{
		public int id = 20;

		public List<int> clientIds;

		public ClientsGotCardsPacket(List<int> clientIds)
		{
			this.clientIds = clientIds;
		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			throw new NotImplementedException();
		}
	}
}
