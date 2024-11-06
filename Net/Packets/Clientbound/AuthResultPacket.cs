using CISOServer.Core;

namespace CISOServer.Net.Packets.Clientbound
{
	[Flags]
	public enum AuthResultFlags
	{
		Ok = 1,
		HasUrl = 2,
		HasToken = 4,
	}

	public class AuthResultPacket : IPacket
	{
		public int id = 5;

		public AuthResultFlags flags;
		public int clientId;
		public string name;
		public string avatar;
		public string url;
		public string token;

		public AuthResultPacket(string url)
		{
			this.flags = AuthResultFlags.HasUrl;
			this.url = url;
		}

		public AuthResultPacket(int clientId, string name, string avatar, string? token)
		{
			this.flags = AuthResultFlags.Ok;
			this.clientId = clientId;
			this.name = name;
			this.avatar = avatar;

			if (token != null)
			{
				this.token = token;
				this.flags |= AuthResultFlags.HasToken;
			}
		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			throw new NotImplementedException();
		}
	}
}
