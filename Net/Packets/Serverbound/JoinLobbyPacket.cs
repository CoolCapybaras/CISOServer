using CISOServer.Core;

namespace CISOServer.Net.Packets.Serverbound
{
	public class JoinLobbyPacket : IPacket
	{
		public int id = 5;

		public int lobbyId;

		public JoinLobbyPacket(int lobbyId)
		{
			this.lobbyId = lobbyId;
		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			if (client.IsAuthed && client.Lobby == null)
				client.JoinLobby(lobbyId);
			return ValueTask.CompletedTask;
		}
	}
}
