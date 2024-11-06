using CISOServer.Database;
using CISOServer.Managers;
using CISOServer.Net.Sockets;
using CISOServer.Services;
using CISOServer.Utilities;
using CISOServer.Utilities.Collections;
using System.Collections.Concurrent;

namespace CISOServer.Core
{
	public class Server
	{
		private SocketListener socketListener;
		private CancellationTokenSource cancellationSource = new();

		public AuthTokenManager AuthTokenManager { get; } = new();
		public ConcurrentHashSet<Client> Clients { get; } = [];
		public ConcurrentDictionary<int, GameLobby> Lobbies { get; } = [];
		public VkAuthService VkAuthService { get; private set; }
		public TelegramBotService TelegramBotService { get; private set; }

		public async Task Start()
		{
			if (!Setup())
				return;

			VkAuthService = new VkAuthService(this);
			TelegramBotService = new TelegramBotService(this, cancellationSource.Token);
			_ = Task.Run(TelegramBotService.Start);

#if DEBUG_EDITOR || RELEASE_EDITOR
			int port = Config.Get<int>("serverTcpPort");
			socketListener = new SocketListener(port);
			socketListener.Start();
			Logger.LogInfo($"Tcp listener started on port {port}");
#else
			string hostname = Config.Get<string>("serverWebSocketHostname");
			socketListener = new SocketListener(hostname);
			socketListener.Start();
			Logger.LogInfo($"WebSocket listener started on {hostname}");
#endif

			socketListener.AuthRequest += VkAuthService.AuthRequest;

			while (true)
			{
				ClientSocket socket;
				try
				{
					socket = await socketListener.AcceptSocketAsync(cancellationSource.Token);
				}
				catch (OperationCanceledException)
				{
					break;
				}

				var client = new Client(this, socket);
				Clients.Add(client);
				_ = Task.Run(client.Start);
			}

			Logger.LogInfo("Shutting down server...");

			foreach (var client in Clients)
				await client.Disconnect();
		}

		private static bool Setup()
		{
			if (!Config.Load())
			{
				Logger.LogError("Необходимо заполнить config.json");
				return false;
			}

			Directory.CreateDirectory("profileImages");

			if (!File.Exists("profileImages/default.jpg"))
			{
				Logger.LogError("Отсутствует стандартная аватарка по пути profileImages/default.jpg");
				return false;
			}

			Misc.Init(Config.Get<string>("appHostname"));
			HMACToken.Init(Config.Get<string>("secretKey"));
			ApplicationDbContext.Init(Config.Get<string>("dbConnectionString"));

			return true;
		}

		public void Stop()
		{
			cancellationSource.Cancel();
		}
	}
}
