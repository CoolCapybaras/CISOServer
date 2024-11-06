using CISOServer.Core;
using CISOServer.Net.Packets;
using System.Text.Json.Serialization;

namespace CISOServer.Gamelogic
{
	public class Player
	{
		[JsonIgnore]
		public Client Client { get; set; }
		public int Id { get; }
		public string Name => Client.Name;
		public string Avatar => Client.Avatar;
		public int Health { get; set; }
		public HashSet<Character> Characters { get; } = [];
		[JsonIgnore]
		public HashSet<Card> Cards { get; } = [];
		public int CardCount => Cards.Count;

		public Player(Client client, int id)
		{
			this.Client = client;
			this.Id = id;
		}

		public void SendPacket(byte[] packet) => Client.SendPacket(packet);
		public void SendPacket(IPacket packet) => Client.SendPacket(packet);
		public void SendMessage(string text, int type) => Client.SendMessage(text, type);
	}
}
