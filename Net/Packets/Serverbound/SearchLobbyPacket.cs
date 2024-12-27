using CISOServer.Core;
using CISOServer.Net.Packets.Clientbound;
using CISOServer.Utilities;
using System.Text.Json;

namespace CISOServer.Net.Packets.Serverbound
{
	public enum SearchLobbyType
	{
		Search,
		Clear
	}

	public class SearchLobbyPacket : IPacket
	{
		public int id = 4;

		public SearchLobbyType type;
		public int count;

		public SearchLobbyPacket(SearchLobbyType type, int count)
		{
			this.type = type;
			this.count = count;
		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			if (!client.IsAuthed || client.Lobby != null)
				return ValueTask.CompletedTask;

			if (type == SearchLobbyType.Search)
			{
				var lobbies = server.Lobbies.Where(x => !client.SearchedLobbyIds.Contains(x.Key)).Select(x => x.Value).Take(count).ToList();
				foreach (var lobby in lobbies)
					client.SearchedLobbyIds.Add(lobby.Id);
				client.SendPacket(JsonSerializer.SerializeToUtf8Bytes(new SearchLobbyResultPacket(lobbies), Misc.JsonLobbySerializerOptions));
			}
			else if (type == SearchLobbyType.Clear)
			{
				client.SearchedLobbyIds.Clear();
			}

			return ValueTask.CompletedTask;
		}
	}
}
