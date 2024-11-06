using CISOServer.Core;

namespace CISOServer.Net.Packets.Clientbound
{
	public class SearchLobbyResultPacket : IPacket
	{
		public int id = 9;

		public IEnumerable<GameLobby> lobbies;

		public SearchLobbyResultPacket(IEnumerable<GameLobby> lobbies)
		{
			this.lobbies = lobbies;
		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			throw new NotImplementedException();
		}
	}
}
