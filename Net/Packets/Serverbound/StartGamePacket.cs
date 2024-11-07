using CISOServer.Core;

namespace CISOServer.Net.Packets.Serverbound
{
	public class StartGamePacket : IPacket
	{
		public int id = 15;

		public ValueTask HandleAsync(Server server, Client client)
		{
			client.Lobby?.OnGameStart(client);
			return ValueTask.CompletedTask;
		}
	}
}
