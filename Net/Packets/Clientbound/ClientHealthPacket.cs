using CISOServer.Core;

namespace CISOServer.Net.Packets.Clientbound
{
	public class ClientHealthPacket : IPacket
	{
		public int id = 24;

		public int clientId;
		public int health;

		public ClientHealthPacket(int clientId, int health)
		{
			this.clientId = clientId;
			this.health = health;
		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			throw new NotImplementedException();
		}
	}
}
