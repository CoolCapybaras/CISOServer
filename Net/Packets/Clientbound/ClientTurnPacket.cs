using CISOServer.Core;

namespace CISOServer.Net.Packets.Clientbound
{
	public class ClientTurnPacket : IPacket
	{
		public int id = 21;

		public int clientId;

		public ClientTurnPacket(int clientId)
		{
			this.clientId = clientId;
		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			throw new NotImplementedException();
		}
	}
}
