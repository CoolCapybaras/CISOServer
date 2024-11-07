using CISOServer.Core;

namespace CISOServer.Net.Packets.Clientbound
{
	public class GameEndedPacket : IPacket
	{
		public int id = 18;

		public int clientId;

		public GameEndedPacket(int clientId)
		{
			this.clientId = clientId;
		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			throw new NotImplementedException();
		}
	}
}
