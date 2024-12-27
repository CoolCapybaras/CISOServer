using CISOServer.Core;

namespace CISOServer.Net.Packets.Clientbound
{
	public class ClientDiedPacket : IPacket
	{
		public int id = 25;

		public int clientId;

		public ClientDiedPacket(int clientId)
		{
			this.clientId = clientId;
		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			throw new NotImplementedException();
		}
	}
}
