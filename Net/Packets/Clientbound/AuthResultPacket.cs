using CISOServer.Core;

namespace CISOServer.Net.Packets.Clientbound
{
	public enum AuthResultType
	{
		Ok,
		Url,
	}

	public class AuthResultPacket : IPacket
	{
		public int id = 8;

		public AuthResultType type;
		public int clientId;
		public string name;
		public string avatar;
		public string? token;
		public string url;

		public AuthResultPacket(string url)
		{
			this.type = AuthResultType.Url;
			this.url = url;
		}

		public AuthResultPacket(int clientId, string name, string avatar, string? token)
		{
			this.type = AuthResultType.Ok;
			this.clientId = clientId;
			this.name = name;
			this.avatar = avatar;
			this.token = token;
		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			throw new NotImplementedException();
		}
	}
}
