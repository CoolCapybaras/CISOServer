using CISOServer.Core;

namespace CISOServer.Net.Packets.Clientbound
{
	public class LobbyJoinedPacket : IPacket
	{
		public int id = 10;

		public int clientId;
		public GameLobby lobby;

		public LobbyJoinedPacket(int clientId, GameLobby lobby)
		{
			this.clientId = clientId;
			this.lobby = lobby;
		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			throw new NotImplementedException();
		}
	}
}
