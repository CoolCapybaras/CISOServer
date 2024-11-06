using CISOServer.Core;

namespace CISOServer.Net.Packets.Clientbound
{
	public class MessagePacket : IPacket
	{
		public int id = 0;

		public int type;
		public string text;

		public MessagePacket(int type, string text)
		{
			this.type = type;
			this.text = text;
		}

		public ValueTask HandleAsync(Server server, Client client)
		{
			throw new NotImplementedException();
		}
	}
}
