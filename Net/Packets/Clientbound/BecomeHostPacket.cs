using CISOServer.Core;

namespace CISOServer.Net.Packets.Clientbound
{
	public class BecomeHostPacket : IPacket
	{
		public int id = 13;

		public ValueTask HandleAsync(Server server, Client client)
		{
			throw new NotImplementedException();
		}
	}
}
