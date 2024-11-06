using CISOServer.Core;

namespace CISOServer.Net.Packets.Serverbound
{
	public class JoinLobbyPacket : IPacket
	{
		public int id = 3;

		public int lobbyId;

		public JoinLobbyPacket(int lobbyId)
		{
			this.lobbyId = lobbyId;
		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			if (!client.IsAuthed || client.Lobby != null
				|| !server.Lobbies.TryGetValue(lobbyId, out var lobby))
				return ValueTask.CompletedTask;

			if (lobby.IsStarted)
			{
				client.SendMessage("Лобби уже запущено");
				return ValueTask.CompletedTask;
			}

			lobby.OnClientJoin(client);
			return ValueTask.CompletedTask;
		}
	}
}
