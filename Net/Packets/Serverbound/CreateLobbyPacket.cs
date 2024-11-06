using CISOServer.Core;
using CISOServer.Utilities;

namespace CISOServer.Net.Packets.Serverbound
{
	public class CreateLobbyPacket : IPacket
	{
		public int id = 3;

		public int maxClients;

		public CreateLobbyPacket(int maxClients)
		{
			this.maxClients = maxClients;
		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			if (!client.IsAuthed || client.Lobby != null)
				return ValueTask.CompletedTask;

			maxClients = Math.Clamp(maxClients, 2, 5);

			int id = Misc.RandomId();
			var lobby = new GameLobby(server, id, maxClients);
			server.Lobbies.TryAdd(id, lobby);
			lobby.OnClientJoin(client);

			return ValueTask.CompletedTask;
		}
	}
}
