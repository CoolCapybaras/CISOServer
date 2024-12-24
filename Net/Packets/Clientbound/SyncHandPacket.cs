using CISOServer.Core;
using CISOServer.Gamelogic;

namespace CISOServer.Net.Packets.Clientbound
{
	public class SyncHandPacket : IPacket
	{
		public int id = 19;

		public List<Card> cards;

		public SyncHandPacket(List<Card> cards)
		{
			this.cards = cards;
		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			throw new NotImplementedException();
		}
	}
}
