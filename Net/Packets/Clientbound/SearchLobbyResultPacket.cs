using CISOServer.Core;

namespace CISOServer.Net.Packets.Clientbound
{
	public class SearchLobbyResultPacket : IPacket
	{
		public int id = 9;

		public List<GameLobby> lobbies;

		public SearchLobbyResultPacket(List<GameLobby> lobbies)
		{
			this.lobbies = lobbies;
		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			throw new NotImplementedException();
		}
	}
}
