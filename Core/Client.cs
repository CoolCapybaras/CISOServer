using CISOServer.Gamelogic;
using CISOServer.Net.Packets;
using CISOServer.Net.Packets.Clientbound;
using CISOServer.Net.Packets.Serverbound;
using CISOServer.Net.Sockets;
using CISOServer.Utilities;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace CISOServer.Core
{
	public class Client : IDisposable
	{
		private Server server;
		private ClientSocket socket;
		private bool disposed;

		public int Id { get; set; }
		public bool IsAuthed => Id != 0;
		public string Name { get; set; }
		public string Avatar { get; set; }
		public GameLobby? Lobby { get; set; }
		public Player? Player { get; set; }
		public IPAddress Ip => socket.Ip;
		public HashSet<int> SearchedLobbyIds { get; } = [];

		public Client(Server server, ClientSocket socket)
		{
			this.socket = socket;
			this.server = server;
		}

		public async Task Start()
		{
			try
			{
				while (true)
				{
					using var stream = new MemoryStream();
					await socket.ReceiveAsync(stream);

					stream.Position = 0;
					Logger.LogInfo($"[{Name ?? socket.Ip.ToString()}]: {await new StreamReader(stream).ReadToEndAsync()}");

					stream.Position = 0;
					int id = JsonSerializer.Deserialize<BasePacket>(stream, Misc.JsonSerializerOptions).id;
					stream.Position = 0;
					switch (id)
					{
						case 2:
							await JsonSerializer.Deserialize<AuthPacket>(stream, Misc.JsonSerializerOptions).HandleAsync(server, this);
							break;
						case 3:
							await JsonSerializer.Deserialize<CreateLobbyPacket>(stream, Misc.JsonSerializerOptions).HandleAsync(server, this);
							break;
						case 4:
							await JsonSerializer.Deserialize<SearchLobbyPacket>(stream, Misc.JsonSerializerOptions).HandleAsync(server, this);
							break;
						case 5:
							await JsonSerializer.Deserialize<JoinLobbyPacket>(stream, Misc.JsonSerializerOptions).HandleAsync(server, this);
							break;
						case 6:
							await JsonSerializer.Deserialize<LeaveLobbyPacket>(stream, Misc.JsonSerializerOptions).HandleAsync(server, this);
							break;
						case 7:
							await JsonSerializer.Deserialize<UpdateProfilePacket>(stream, Misc.JsonSerializerOptions).HandleAsync(server, this);
							break;
					}
				}
			}
			catch (Exception ex) when (ex is SocketException || ex is JsonException) { }
			catch (Exception ex)
			{
				Logger.LogError(ex.ToString());
			}

			OnDisconnect();
			this.Dispose();
		}

		public void Auth(int id, string name, string? token = null)
		{
			Id = id;
			Name = name;
			Avatar = id > 0 ? $"{Misc.AppHostname}profileImages/{id}.jpg" : $"{Misc.AppHostname}profileImages/default.jpg";

			var client = server.Clients.FirstOrDefault(x => x.Id == id && x != this);
			SendPacket(new AuthResultPacket(Id, Name, Avatar, token));
			if (client != null)
			{
				Lobby = client.Lobby;
				Player = client.Player;
				Player.Client = this;
				server.Clients.TryRemove(client);

				Player.State = ClientState.Ok;
				SendPacket(new LobbyJoinedPacket(Player.Id, Lobby!));
				Lobby.BroadcastOther(Player, new ClientStatePacket(Player.Id, Player.State));
			}
		}

		public void JoinLobby(int lobbyId)
		{
			if (!server.Lobbies.TryGetValue(lobbyId, out var lobby))
			{
				SendMessage("Такого лобби не существует", 2);
				return;
			}

			if (lobby.IsStarted)
			{
				SendMessage("Лобби уже запущено", 2);
				return;
			}

			lobby.OnClientJoin(this);
		}

		public Task Disconnect()
		{
			return socket.CloseAsync();
		}

		private void OnDisconnect()
		{
			server.AuthTokenManager.RemoveToken(this);

			if (!IsAuthed)
				return;

			if (Lobby != null)
			{
				if (Lobby.IsStarted)
				{
					Player.State = ClientState.ConnectionError;
					Lobby.BroadcastOther(Player, new ClientStatePacket(Player.Id, Player.State));
				}
				else
				{
					Lobby.OnClientLeave(this);
					server.Clients.TryRemove(this);
				}
			}

			Logger.LogInfo($"{Name} has left the server");
		}

		public void SendPacket(IPacket packet)
		{
			socket.Send(JsonSerializer.SerializeToUtf8Bytes(packet, packet.GetType(), Misc.JsonSerializerOptions));
		}

		public void SendPacket(byte[] packet)
		{
			socket.Send(packet);
		}

		public void SendMessage(string text, int type)
		{
			socket.Send(JsonSerializer.SerializeToUtf8Bytes(new MessagePacket(type, text), Misc.JsonSerializerOptions));
		}

		public void Dispose()
		{
			if (disposed)
				return;

			disposed = true;

			socket.Dispose();

			GC.SuppressFinalize(this);
		}
	}
}
