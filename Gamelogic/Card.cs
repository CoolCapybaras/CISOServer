
namespace CISOServer.Gamelogic
{
	public enum CardType
	{
		Attack,
		Defense,
		Counterattack,
		Reflection
	}

	public class Card
	{
		public CardType Type;

		public Card(CardType type)
		{
			Type = type;
		}

		public override bool Equals(object? obj)
		{
			return obj is Card card &&
				   Type == card.Type;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Type);
		}

		public static bool operator ==(Card? left, Card? right)
		{
			return EqualityComparer<Card>.Default.Equals(left, right);
		}

		public static bool operator !=(Card? left, Card? right)
		{
			return !(left == right);
		}
	}
}
