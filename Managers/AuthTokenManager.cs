using CISOServer.Core;
using CISOServer.Utilities;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace CISOServer.Managers
{
	public class AuthToken
	{
		public Client Client { get; set; }
		public string Token { get; set; }
		public DateTimeOffset ExpirationTime { get; set; }

		public AuthToken(Client client)
		{
			Client = client;
			Token = Misc.Base64UrlEncode(RandomNumberGenerator.GetBytes(8));
			ExpirationTime = DateTimeOffset.UtcNow.AddMinutes(1);
		}
	}

	public class AuthTokenManager
	{
		private ConcurrentDictionary<string, AuthToken> tokens = [];
		private ConcurrentDictionary<Client, AuthToken> clientToToken = [];

		public string CreateToken(Client client)
		{
			RemoveToken(client);

			var token = new AuthToken(client);
			tokens.TryAdd(token.Token, token);
			clientToToken.TryAdd(client, token);
			return token.Token;
		}

		public bool TryGetToken(string token, [MaybeNullWhen(false)] out AuthToken value)
		{
			bool result = tokens.TryRemove(token, out value);
			if (result)
				clientToToken.TryRemove(value.Client, out _);
			return result;
		}

		public void RemoveToken(Client client)
		{
			if (clientToToken.TryRemove(client, out var _token))
				tokens.TryRemove(_token.Token, out _);
		}
	}
}
