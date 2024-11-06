using CISOServer.Core;

namespace CISOServer.Net.Packets.Clientbound
{
	public enum ClientState
	{
		Ok,
		ConnectionError
	}

	public class ClientStatePacket : IPacket
	{
		public int id = 14;

		public int clientId;
		public ClientState state;

		public ClientStatePacket(int clientId, ClientState state)
		{
			this.clientId = clientId;
			this.state = state;
		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			throw new NotImplementedException();
		}
	}
}
