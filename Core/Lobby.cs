using CISOServer.Net.Packets;
using CISOServer.Net.Packets.Clientbound;
using CISOServer.Utilities;
using System.Text.Json.Serialization;

namespace CISOServer.Core
{
	public class Lobby
	{
		private Server server;
		private Client host;
		private List<Client> clients = [];

		public int Id { get; set; }
		public string Name { get; set; }
		[JsonIgnore]
		public bool IsStarted { get; set; }

		public Lobby(Server server, int id)
		{
			this.server = server;
			this.Id = id;
		}

		public void OnClientJoin(Client client)
		{
			lock (clients)
			{
				if (clients.Count >= 5)
				{
					client.SendMessage("Нет свободных мест");
					return;
				}

				client.Lobby = this;

				Broadcast(new ClientJoinedPacket(client));

				clients.Add(client);

				client.SendPacket(new LobbyJoinedPacket(clients));

				if (clients.Count == 1)
				{
					host = client;
					Logger.LogInfo($"{client.Name} created new room #{Id}");
				}
			}
		}

		public void OnClientLeave(Client client)
		{
			lock (clients)
			{
				client.Lobby = null;

				if (clients.Count == 1)
				{
					server.Lobbies.Remove(Id, out _);
					Logger.LogInfo($"Room #{Id} was destroyed");
				}

				clients.Remove(client);

				Broadcast(new ClientLeavedPacket(client.Id));

				if (client == host)
				{
					host = clients[0];
					host.SendPacket(new BecomeHostPacket());
				}
			}
		}

		public void Broadcast(IPacket packet)
		{
			foreach (var client in clients)
			{
				client.SendPacket(packet);
			}
		}
	}
}
