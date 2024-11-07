using CISOServer.Core;
using CISOServer.Gamelogic;

namespace CISOServer.Net.Packets.Clientbound
{
	public class SyncHandPacket : IPacket
	{
		public int id = 19;

		public HashSet<Card> cards;

		public SyncHandPacket(HashSet<Card> cards)
		{
			this.cards = cards;
		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			throw new NotImplementedException();
		}
	}
}
