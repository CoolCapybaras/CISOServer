using CISOServer.Utilities;
using System.Security.Cryptography;
using System.Text;

namespace CISOServer.Managers
{
	public static class HMACToken
	{
		private static HMACSHA256 hMACSHA256;

		public static void Init(string secretKey)
		{
			hMACSHA256 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
		}

		public static string Create(int userId, int expires)
		{
			string payload = $"{userId}.{expires}";
			string base64Payload = Misc.Base64UrlEncode(Encoding.UTF8.GetBytes(payload));
			var hash = hMACSHA256.ComputeHash(Encoding.UTF8.GetBytes(base64Payload));
			string signature = Misc.Base64UrlEncode(hash);
			return $"{base64Payload}.{signature}";
		}

		public static (int userId, int expires)? Validate(string token)
		{
			var args = token.Split('.');
			if (args.Length != 2)
				return null;

			var hash = hMACSHA256.ComputeHash(Encoding.UTF8.GetBytes(args[0]));
			string signature = Misc.Base64UrlEncode(hash);
			if (args[1] != signature)
				return null;

			string payload = Encoding.UTF8.GetString(Misc.Base64UrlDecode(args[0]));
			var payloadArgs = payload.Split('.');
			if (payloadArgs.Length != 2 || !int.TryParse(payloadArgs[0], out int id) || !int.TryParse(payloadArgs[1], out int expires))
				return null;

			return (id, expires);
		}
	}
}
