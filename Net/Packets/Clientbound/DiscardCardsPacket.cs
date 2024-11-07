using CISOServer.Core;

namespace CISOServer.Net.Packets.Clientbound
{
	public class DiscardCardsPacket : IPacket
	{
		public int id = 23;

		public ValueTask HandleAsync(Server server, Client client)
		{
			throw new NotImplementedException();
		}
	}
}
