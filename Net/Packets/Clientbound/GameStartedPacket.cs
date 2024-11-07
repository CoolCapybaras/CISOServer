using CISOServer.Core;

namespace CISOServer.Net.Packets.Clientbound
{
	public class GameStartedPacket
	{
		public int id = 17;

		public ValueTask HandleAsync(Server server, Client client)
		{
			throw new NotImplementedException();
		}
	}
}
