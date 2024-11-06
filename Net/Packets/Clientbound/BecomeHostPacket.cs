using CISOServer.Core;

namespace CISOServer.Net.Packets.Clientbound
{
	public class BecomeHostPacket : IPacket
	{
		public int id = 9;

		public ValueTask HandleAsync(Server server, Client client)
		{
			throw new NotImplementedException();
		}
	}
}
