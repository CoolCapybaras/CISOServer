using CISOServer.Core;
using CISOServer.Net.Packets;
using CISOServer.Net.Packets.Clientbound;
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
		public ClientState State { get; set; }
		public int Health { get; set; } = 3;
		public HashSet<Character> Characters { get; } = [];
		[JsonIgnore]
		public List<Card> Cards { get; } = [];
		public int CardCount => Cards.Count;

		public Player(Client client, int id)
		{
			this.Client = client;
			this.Id = id;
		}

		public void SendPacket(byte[] packet)
		{
			if (State == ClientState.InGame)
				Client.SendPacket(packet);
		}
		public void SendPacket(IPacket packet)
		{
			if (State == ClientState.InGame)
				Client.SendPacket(packet);
		}
		public void SendMessage(string text, int type)
		{
			if (State == ClientState.InGame)
				Client.SendMessage(text, type);
		}
	}
}
