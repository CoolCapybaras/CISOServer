using CISOServer.Gamelogic;
using CISOServer.Net.Packets;
using CISOServer.Net.Packets.Clientbound;
using CISOServer.Net.Packets.Serverbound;
using CISOServer.Utilities;
using System.Text.Json;

namespace CISOServer.Core
{
	public enum GameState
	{
		None,
		Attack,
		Defense
	}

	public class GameLobby
	{
		private HashSet<int> availableIds = [0, 1, 2, 3, 4];

		private Server server;
		private Player host;

		private Stack<Card> dealDeck;

		private Player mainTurnPlayer;
		private Player turnPlayer;
		private Player previousPlayer;
		private TaskCompletionSource actionTask;

		private GameState state;
		private bool actionAllowed;
		private bool attackPlayed;
		private int deadPlayersCount;

		public int Id { get; }
		public string Name { get; set; }
		public int MaxClients { get; }
		public bool IsStarted { get; set; }
		public List<Player> Players { get; } = [];
		public List<Card> Table { get; } = [];

		public GameLobby(Server server, int id, int maxClients)
		{
			this.server = server;
			this.Id = id;
			this.MaxClients = maxClients;
		}

		private async Task Start()
		{
			try
			{
				await ProcessGame();
			}
			catch (Exception e)
			{
				Logger.LogError(e.ToString());
			}
		}

		private async Task ProcessGame()
		{
			Broadcast(new GameStartedPacket());

			InitDealDeck();
			await DealCards();

			mainTurnPlayer = Players[Random.Shared.Next(Players.Count)];
			turnPlayer = mainTurnPlayer;
			Broadcast(new ClientTurnPacket(turnPlayer.Id));
			state = GameState.Attack;

			while (true)
			{
				await WaitForAction();

				if (previousPlayer.Health <= 0)
				{
					if (Players.All(x => x.State != ClientState.InGame))
					{
						Destroy(true);
						return;
					}

					Broadcast(new ClientDiedPacket(previousPlayer.Id));
					deadPlayersCount++;
					await Task.Delay(500);

					if (deadPlayersCount == Players.Count - 1)
					{
						Broadcast(new GameEndedPacket(Players.Find(x => x.Health > 0).Id));
						break;
					}
				}

				if (state == GameState.None)
				{
					await DealCards();
					state = GameState.Attack;
				}

				Broadcast(new ClientTurnPacket(turnPlayer.Id));
			}

			Destroy();
		}

		private void InitDealDeck()
		{
			var deck = new List<Card>();
			deck.AddRange(Enumerable.Repeat(new Card(CardType.Attack), 12));
			deck.AddRange(Enumerable.Repeat(new Card(CardType.Defense), 8));
			deck.AddRange(Enumerable.Repeat(new Card(CardType.Counterattack), 8));
			deck.AddRange(Enumerable.Repeat(new Card(CardType.Reflection), 8));
			deck.Shuffle();

			dealDeck = new Stack<Card>(deck);
		}

		private async Task DealCards()
		{
			var ids = new List<int>();
			foreach (var player in Players)
			{
				while (player.Cards.Count < 5 && dealDeck.TryPop(out var card))
				{
					player.Cards.Add(card);
					ids.Add(player.Id);
				}
				player.SendPacket(new SyncHandPacket(player.Cards));
			}
			Broadcast(new ClientsGotCardsPacket(ids));
			if (ids.Count > 0)
				await Task.Delay(ids.Count * 100);
		}

		public void OnClientAction(Player player, GameAction type, Card card, int targetId)
		{
			if (!actionAllowed || player != turnPlayer)
			{
				player.SendMessage("Сейчас не ваш ход", 3);
				return;
			}

			if (type == GameAction.PlayCard)
			{
				if (!player.Cards.Contains(card))
				{
					player.SendMessage("Попытка сыграть несуществующую карту", 3);
					return;
				}
				var target = Players.Find(x => x.Id == targetId);
				if (target == null)
				{
					player.SendMessage("Цель хода не найдена", 3);
					return;
				}

				if (card.Type == CardType.Attack)
				{
					if (state != GameState.Attack)
					{
						player.SendMessage("Сейчас нельзя сыграть карту атаки", 3);
						return;
					}
					if (attackPlayed)
					{
						player.SendMessage("За один ход можно сыграть только одну карту атаки", 3);
						return;
					}

					attackPlayed = true;
					UpdateTurn(target, player, GameState.Defense);
				}
				else if (card.Type == CardType.Defense)
				{
					if (state != GameState.Defense)
					{
						player.SendMessage("Сейчас нельзя сыграть карту защиты", 3);
						return;
					}
					if (target != previousPlayer)
					{
						player.SendMessage("Цель хода не является атакующим", 3);
						return;
					}

					UpdateTurn(mainTurnPlayer, player, GameState.Attack);
				}
				else if (card.Type == CardType.Counterattack)
				{
					if (state != GameState.Defense || player.Health <= 1)
					{
						player.SendMessage("Сейчас нельзя сыграть карту контратаки", 3);
						return;
					}
					if (target != previousPlayer)
					{
						player.SendMessage("Цель хода не является атакующим", 3);
						return;
					}

					Broadcast(new ClientHealthPacket(player.Id, --player.Health));
					UpdateTurn(target, player, GameState.Defense);
				}
				else if (card.Type == CardType.Reflection)
				{
					if (state != GameState.Defense)
					{
						player.SendMessage("Сейчас нельзя сыграть карту отражения", 3);
						return;
					}

					UpdateTurn(target, player, GameState.Defense);
				}

				player.Cards.Remove(card);
				Table.Add(card);
				BroadcastOther(player, new ClientPlayedCardPacket(player.Id, card, target.Id));
			}
			else if (type == GameAction.EndTurn)
			{
				if (player == mainTurnPlayer)
				{
					if (state == GameState.Attack)
					{
						NextTurn();
					}
					else if (state == GameState.Defense)
					{
						Broadcast(new ClientHealthPacket(player.Id, --player.Health));
						NextTurn();
					}
					attackPlayed = false;
				}
				else
				{
					if (state == GameState.Defense)
					{
						Broadcast(new ClientHealthPacket(player.Id, --player.Health));
						UpdateTurn(mainTurnPlayer, player, GameState.Attack);
					}
				}
				Broadcast(new DiscardCardsPacket());
			}
		}

		private void NextTurn()
		{
			var previousPlayer = mainTurnPlayer;
			do mainTurnPlayer = Players[(Players.IndexOf(mainTurnPlayer) + 1) % Players.Count];
			while (mainTurnPlayer.Health <= 0 && mainTurnPlayer != previousPlayer);
			UpdateTurn(mainTurnPlayer, previousPlayer, GameState.None);
		}

		private void UpdateTurn(Player turnPlayer, Player previousPlayer, GameState state)
		{
			this.turnPlayer = turnPlayer;
			this.previousPlayer = previousPlayer;
			this.state = state;
			actionTask.SetResult();
		}

		private async Task WaitForAction()
		{
			actionTask = new TaskCompletionSource();
			var timeoutTask = Task.Delay(60000);
			actionAllowed = true;
			var completedTask = await Task.WhenAny(actionTask.Task, timeoutTask);
			if (completedTask == timeoutTask)
			{
				if (turnPlayer.State == ClientState.ConnectionError)
					turnPlayer.Health = 0;
				OnClientAction(turnPlayer, GameAction.EndTurn, null!, 0);
			}
			actionAllowed = false;
		}

		public void RestorePlayer(Client client, Client oldClient)
		{
			client.Lobby = oldClient.Lobby;
			client.Player = oldClient.Player;
			client.Player.Client = client;
			server.Clients.TryRemove(oldClient);

			var player = client.Player;

			player.State = ClientState.InGame;
			player.SendPacket(new LobbyJoinedPacket(player.Id, this));
			player.SendPacket(new SyncHandPacket(player.Cards));
			player.SendPacket(new ClientsGotCardsPacket(Enumerable.Repeat(player.Id, player.Cards.Count).ToList()));
			if (player == turnPlayer)
				player.SendPacket(new ClientTurnPacket(player.Id));
			BroadcastOther(player, new ClientStatePacket(player.Id, player.State));
		}

		public void OnClientJoin(Client client)
		{
			lock (Players)
			{
				if (Players.Count == MaxClients)
				{
					client.SendMessage("Нет свободных мест", 2);
					return;
				}

				int id = availableIds.Min();
				availableIds.Remove(id);

				var player = new Player(client, id);
				client.Lobby = this;
				client.Player = player;

				Players.Add(player);
				Name = string.Join(", ", Players.Select(x => x.Client.Name));

				client.SendPacket(new LobbyJoinedPacket(id, this));
				BroadcastOther(player, new ClientJoinedPacket(player));

				if (Players.Count == 1)
				{
					host = player;
					Logger.LogInfo($"{client.Name} created new room #{Id}");
				}
			}
		}

		public void OnClientLeave(Client client, bool connectionError = false)
		{
			var player = client.Player!;

			if (IsStarted)
			{
				if (connectionError)
				{
					player.State = ClientState.ConnectionError;
				}
				else
				{
					client.Lobby = null;
					client.Player = null;

					player.Health = 0;
					player.State = ClientState.Disconnected;
				}
				Broadcast(new ClientStatePacket(player.Id, player.State));
				return;
			}

			lock (Players)
			{
				client.Lobby = null;
				client.Player = null;

				if (Players.Count == 1)
				{
					Destroy(true);
					return;
				}

				Players.Remove(player);
				availableIds.Add(player.Id);
				Name = string.Join(", ", Players.Select(x => x.Client.Name));

				Broadcast(new ClientLeavedPacket(player.Id));

				if (player == host)
				{
					host = Players[0];
					host.SendPacket(new BecomeHostPacket());
				}
			}
		}

		public void OnGameStart(Client client)
		{
			if (client.Player != host || IsStarted)
				return;

			Task.Run(Start);
			IsStarted = true;

			Logger.LogInfo($"Room #{Id} was started with {Players.Count} clients");
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
				if (player.State == ClientState.ConnectionError)
					server.Clients.TryRemove(player.Client);
			}

			server.Lobbies.TryRemove(Id, out _);
			Logger.LogInfo($"Room #{Id} was destroyed" + (noClients ? " due to lack of clients" : ""));
		}
	}
}
