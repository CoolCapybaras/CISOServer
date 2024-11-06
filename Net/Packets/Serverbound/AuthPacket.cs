using CISOServer.Core;
using CISOServer.Database;
using CISOServer.Managers;
using CISOServer.Net.Packets.Clientbound;
using CISOServer.Utilities;
using Microsoft.EntityFrameworkCore;

namespace CISOServer.Net.Packets.Serverbound
{
	public enum AuthType
	{
		Anonymous,
		Token,
		VK,
		Telegram
	}

	public class AuthPacket : IPacket
	{
		public int id = 2;

		public AuthType type;
		public string data;
		public int lobbyId;

		public AuthPacket()
		{

		}

		public AuthPacket(AuthType type, string data)
		{
			this.type = type;
			this.data = data;
		}

		public AuthPacket(AuthType type, string data, int lobbyId)
		{
			this.type = type;
			this.data = data;
			this.lobbyId = lobbyId;
		}

		public async ValueTask HandleAsync(Server server, Client client)
		{
			if (client.IsAuthed)
				return;

			if (type == AuthType.Anonymous)
			{
				if (!UpdateProfilePacket.NameRegex.IsMatch(data))
				{
					client.SendMessage("Имя должно быть от 3 до 24 символов и содержать только буквы или цифры", 1);
					return;
				}

				client.Auth(Misc.GetRandomGuestId(), data);

				if (lobbyId != 0)
					client.JoinLobby(lobbyId);

				Logger.LogInfo($"{client.Ip} authed as {client.Name} anonymously");
			}
			else if (type == AuthType.Token)
			{
				if (data == null)
					return;

				var token = HMACToken.Validate(data);
				if (!token.HasValue)
					return;

				var timestamp = DateTimeOffset.UtcNow;
				if (timestamp >= DateTimeOffset.FromUnixTimeSeconds(token.Value.expires))
					return;

				using var db = new ApplicationDbContext();
				var user = await db.users.FirstOrDefaultAsync(x => x.id == token.Value.userId);
				if (user == null)
					return;

				user.ip = client.Ip.ToString();
				user.lastlogin = timestamp;
				await db.SaveChangesAsync();

				client.Auth(user.id, user.username);

				if (lobbyId != 0)
					client.JoinLobby(lobbyId);

				Logger.LogInfo($"{user.ip} authed as {client.Name} using token");
			}
			else if (type == AuthType.VK)
			{
				string token = server.AuthTokenManager.CreateToken(client, lobbyId);
				client.SendPacket(new AuthResultPacket(server.VkAuthService.GetAuthUrl(token)));
			}
			else if (type == AuthType.Telegram)
			{
				string token = server.AuthTokenManager.CreateToken(client, lobbyId);
				client.SendPacket(new AuthResultPacket(server.TelegramBotService.GetAuthUrl(token)));
			}
		}
	}
}
