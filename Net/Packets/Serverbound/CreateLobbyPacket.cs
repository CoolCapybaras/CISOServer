using CISOServer.Core;
using CISOServer.Utilities;

namespace CISOServer.Net.Packets.Serverbound
{
	public class CreateLobbyPacket : IPacket
	{
		public int id = 2;

		public CreateLobbyPacket()
		{

		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			if (!client.IsAuthed || client.Lobby != null)
				return ValueTask.CompletedTask;

			int id = Misc.RandomId();
			var lobby = new Lobby(server, id);
			server.Lobbies.TryAdd(id, lobby);
			lobby.OnClientJoin(client);

			return ValueTask.CompletedTask;
		}
	}
}
