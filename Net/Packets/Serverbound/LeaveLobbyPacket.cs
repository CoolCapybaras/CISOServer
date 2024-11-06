using CISOServer.Core;

namespace CISOServer.Net.Packets.Serverbound
{
	public class LeaveLobbyPacket : IPacket
	{
		public int id = 6;

		public ValueTask HandleAsync(Server server, Client client)
		{
			client.Lobby?.OnClientLeave(client);
			return ValueTask.CompletedTask;
		}
	}
}
