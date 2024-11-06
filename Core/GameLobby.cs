using CISOServer.Gamelogic;
using CISOServer.Net.Packets;
using CISOServer.Net.Packets.Clientbound;
using CISOServer.Utilities;
using System.Text.Json;

namespace CISOServer.Core
{
	public class GameLobby
	{
		private HashSet<int> availableIds = [0, 1, 2, 3, 4];

		private Server server;
		private Player host;

		public int Id { get; }
		public string Name { get; }
		public int MaxClients { get; }
		public bool IsStarted { get; }
		public List<Player> Players { get; } = [];

		public GameLobby(Server server, int id, int maxClients)
		{
			this.server = server;
			this.Id = id;
			this.MaxClients = maxClients;
		}

		public void OnClientJoin(Client client)
		{
			lock (Players)
			{
				if (Players.Count == MaxClients)
				{
					client.SendMessage("Нет свободных мест");
					return;
				}

				int id = availableIds.Min();
				availableIds.Remove(id);

				var player = new Player(client, id);
				client.Lobby = this;
				client.Player = player;

				Players.Add(player);
				client.SendPacket(new LobbyJoinedPacket(id, this));
				BroadcastOther(player, new ClientJoinedPacket(player));

				if (Players.Count == 1)
				{
					host = player;
					Logger.LogInfo($"{client.Name} created new room #{Id}");
				}
			}
		}

		public void OnClientLeave(Client client)
		{
			lock (Players)
			{
				var player = client.Player!;
				client.Lobby = null;
				client.Player = null;

				if (Players.Count == 1)
				{
					Destroy(true);
					return;
				}

				Players.Remove(player);
				availableIds.Add(player.Id);

				Broadcast(new ClientLeavedPacket(player.Id));

				if (player == host)
				{
					host = Players[0];
					host.SendPacket(new BecomeHostPacket());
				}
			}
		}

		public void Broadcast(IPacket packet)
		{
			var bytes = JsonSerializer.SerializeToUtf8Bytes(packet, packet.GetType(), Misc.JsonSerializerOptions);
			foreach (var player in Players)
			{
				player.SendPacket(bytes);
			}
		}

		public void BroadcastOther(Player player, IPacket packet)
		{
			var bytes = JsonSerializer.SerializeToUtf8Bytes(packet, packet.GetType(), Misc.JsonSerializerOptions);
			foreach (var _player in Players)
			{
				if (_player != player)
					_player.SendPacket(bytes);
			}
		}

		public void Destroy(bool noClients = false)
		{
			foreach (var player in Players)
			{
				player.Client.Lobby = null;
				player.Client.Player = null;
			}

			server.Lobbies.TryRemove(Id, out _);
			Logger.LogInfo($"Room #{Id} was destroyed" + (noClients ? " due to lack of clients" : ""));
		}
	}
}
