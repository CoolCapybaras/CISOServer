using CISOServer.Core;
using CISOServer.Database;
using CISOServer.Managers;
using CISOServer.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

namespace CISOServer.Services
{
	public class VkAuthService : IDisposable
	{
		private Server server;
		private HttpClient httpClient = new();

		private int clientId;
		private string codeVerifier;
		private string codeChallenge;
		private string redirectUri;
		private bool disposed;

		public VkAuthService(Server server)
		{
			this.server = server;

			clientId = Config.Get<int>("vkClientId");
			codeVerifier = Config.Get<string>("vkCodeVerifier");
			redirectUri = $"{Config.Get<string>("appHostname")}auth";

			codeChallenge = GetCodeChallenge(codeVerifier);

			Logger.LogInfo("Vk auth service initiated");
		}

		public async void AuthRequest(HttpListenerContext context)
		{
			var queryParams = context.Request.QueryString;
			var code = queryParams["code"];
			var deviceId = queryParams["device_id"];
			var state = queryParams["state"];
			if (code == null || deviceId == null || state == null
				|| !server.AuthTokenManager.TryGetToken(state, out var authToken)
				|| authToken.ExpirationTime < DateTimeOffset.UtcNow)
			{
				context.Response.Close();
				return;
			}

			var vkResponse = await GetAccessToken(code, deviceId);
			if (vkResponse.ContainsKey("error"))
			{
				await SendMessage(context, vkResponse.ToJsonString());
				context.Response.Close();
				return;
			}

			var client = authToken.Client;
			var authId = vkResponse["user_id"].GetValue<long>();
			var ip = client.Ip.ToString();
			var timestamp = DateTimeOffset.UtcNow;

			using var db = new ApplicationDbContext();
			var user = await db.users.FirstOrDefaultAsync(x => x.authId == authId && !x.authType);
			if (user == null)
			{
				vkResponse = await GetUserInfo(vkResponse["access_token"].GetValue<string>());

				user = new DbUser()
				{
					username = vkResponse["first_name"].GetValue<string>(),
					authId = authId,
					ip = ip,
					lastlogin = timestamp,
					regip = ip,
					regdate = timestamp,
					authType = false
				};
				await db.users.AddAsync(user);
				await db.SaveChangesAsync();

				using var stream = await httpClient.GetStreamAsync($"{vkResponse["avatar"].GetValue<string>()[..^5]}200x200");
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

			await SendMessage(context, "Авторизация успешна, можете закрыть страницу");
			context.Response.Close();
			Logger.LogInfo($"{ip} authed as {client.Name} using vk");
		}

		public async Task SendMessage(HttpListenerContext context, string message)
		{
			var data = Encoding.UTF8.GetBytes(message);
			context.Response.ContentType = "text/plain; charset=utf-8";
			await context.Response.OutputStream.WriteAsync(data);
		}

		public string GetAuthUrl(string token) => $"https://id.vk.com/authorize?response_type=code&client_id={clientId}&code_challenge={codeChallenge}&code_challenge_method=s256&redirect_uri={redirectUri}&state={token}";

		private async Task<JsonObject> GetAccessToken(string code, string deviceId)
		{
			var response = await PostAsync("https://id.vk.com/oauth2/auth", $"grant_type=authorization_code&code={code}&code_verifier={codeVerifier}&client_id={clientId}&device_id={deviceId}&redirect_uri={redirectUri}");
			return JsonNode.Parse(response).AsObject();
		}

		private async Task<JsonObject> GetUserInfo(string accessToken)
		{
			var response = await PostAsync("https://id.vk.com/oauth2/user_info", $"access_token={accessToken}&client_id={clientId}");
			return JsonNode.Parse(response)["user"].AsObject();
		}

		public async Task<string> PostAsync(string url, string args)
		{
			var content = new StringContent(args, Encoding.UTF8, "application/x-www-form-urlencoded");
			using var response = await httpClient.PostAsync(url, content);
			return await response.Content.ReadAsStringAsync();
		}

		public static string GetCodeChallenge(string codeVerifier) => Misc.Base64UrlEncode(SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier)));

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
