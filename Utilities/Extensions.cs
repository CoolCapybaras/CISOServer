namespace CISOServer.Utilities
{
	public static class Extensions
	{
		public static void Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				int k = Random.Shared.Next(n--);
				T temp = list[n];
				list[n] = list[k];
				list[k] = temp;
			}
		}
	}
}
