using CISOServer.Core;

namespace CISOServer.Net.Packets
{
	public interface IPacket
	{
		public ValueTask HandleAsync(Server server, Client client);
	}
}
