using CISOServer.Core;
using CISOServer.Database;
using CISOServer.Managers;
using CISOServer.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;

namespace CISOServer.Services
{
	public class TelegramBotService : IDisposable
	{
		private Server server;
		private HttpClient httpClient = new();
		private CancellationToken cancellationToken;
		private int updateId;

		private string username;
		private string token;
		private bool disposed;

		public TelegramBotService(Server server, CancellationToken cancellationToken)
		{
			this.server = server;
			this.cancellationToken = cancellationToken;

			username = Config.Get<string>("tgUsername");
			token = Config.Get<string>("tgToken");
		}

		public async Task Start()
		{
			try
			{
				await ProcessService();
			}
			catch (Exception e)
			{
				Logger.LogError(e.ToString());
			}
		}

		private async Task ProcessService()
		{
			Logger.LogInfo("Telegram bot service started");

			while (true)
			{
				HttpResponseMessage httpMessage;
				try
				{
					httpMessage = await httpClient.GetAsync($"https://api.telegram.org/bot{token}/getUpdates?offset={updateId}&timeout=25&allowed_updates=[\"message\"]", cancellationToken);
				}
				catch (HttpRequestException)
				{
					Logger.LogError("HttpRequestException");
					continue;
				}
				catch (OperationCanceledException e) when (e.InnerException is TimeoutException)
				{
					Logger.LogError("TimeoutException");
					continue;
				}
				catch (OperationCanceledException)
				{
					break;
				}

				if (httpMessage.StatusCode != HttpStatusCode.OK)
				{
					httpMessage.Dispose();
					Logger.LogError("Wrong status code");
					continue;
				}

				string response = await httpMessage.Content.ReadAsStringAsync();

				foreach (JsonObject update in JsonNode.Parse(response)["result"].AsArray())
				{
					updateId = update["update_id"].GetValue<int>() + 1;
					if (!update.ContainsKey("message"))
						continue;

					var message = update["message"].AsObject();
					if (!message.ContainsKey("text"))
						continue;

					var args = message["text"].GetValue<string>().Split();
					if (args.Length != 2
						|| args[0] != "/start"
						|| !server.AuthTokenManager.TryGetToken(args[1], out var authToken)
						|| authToken.ExpirationTime < DateTimeOffset.UtcNow)
						continue;

					var client = authToken.Client;
					var authId = message["from"]["id"].GetValue<long>();
					var ip = client.Ip.ToString();
					var timestamp = DateTimeOffset.UtcNow;

					using var db = new ApplicationDbContext();
					var user = await db.users.FirstOrDefaultAsync(x => x.authId == authId && x.authType);
					if (user == null)
					{
						user = new DbUser()
						{
							username = message["from"]["first_name"].GetValue<string>(),
							authId = authId,
							ip = ip,
							lastlogin = timestamp,
							regip = ip,
							regdate = timestamp,
							authType = true
						};
						await db.users.AddAsync(user);
						await db.SaveChangesAsync();

						using var stream = await httpClient.GetStreamAsync(await GetUserPhoto(authId));
						await Misc.SaveProfileImage(stream, user.id);
						string token = HMACToken.Create(user.id, (int)timestamp.AddMonths(1).ToUnixTimeSeconds());
						client.Auth(user.id, user.username, token);
					}
					else
					{
						user.ip = ip;
						user.lastlogin = timestamp;
						await db.SaveChangesAsync();

						string token = HMACToken.Create(user.id, (int)timestamp.AddMonths(1).ToUnixTimeSeconds());
						client.Auth(user.id, user.username, token);
					}

					if (authToken.LobbyId != 0)
						client.JoinLobby(authToken.LobbyId);

					await SendMessage(message["chat"]["id"].GetValue<int>(), "✅ Авторизация успешна");
					Logger.LogInfo($"{ip} authed as {client.Name} using telegram");
				}

				httpMessage.Dispose();
			}

			Logger.LogInfo("Shutting down telegram bot service...");
			this.Dispose();
		}

		private async Task<string> GetUserPhoto(long realname)
		{
			var response = JsonNode.Parse(await httpClient.GetStringAsync($"https://api.telegram.org/bot{token}/getUserProfilePhotos?user_id={realname}&limit=1"));
			if (response["result"]["total_count"].GetValue<int>() == 0)
				return "https://i.imgur.com/99Zx2sI.jpeg";
			response = JsonNode.Parse(await httpClient.GetStringAsync($"https://api.telegram.org/bot{token}/getFile?file_id={response["result"]["photos"][0][1]["file_id"].GetValue<string>()}"));
			return $"https://api.telegram.org/file/bot{token}/{response["result"]["file_path"].GetValue<string>()}";
		}

		private async Task SendMessage(int chatId, string text)
		{
			var content = new StringContent($"chat_id={chatId}&text={text}", Encoding.UTF8, "application/x-www-form-urlencoded");
			using var _ = await httpClient.PostAsync($"https://api.telegram.org/bot{token}/sendMessage", content);
		}

		public string GetAuthUrl(string token) => $"https://t.me/{username}?start={token}";

		public void Dispose()
		{
			if (disposed)
				return;

			disposed = true;

			httpClient.Dispose();

			GC.SuppressFinalize(this);
		}
	}
}
