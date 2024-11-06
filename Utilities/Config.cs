using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CISOServer.Utilities
{
	public static class Config
	{
		private static readonly JsonObject defaultConfig = new()
		{
			{ "appHostname", "" },
			{ "serverWebSocketHostname", "http://localhost:8887/" },
			{ "serverTcpPort", 8887 },
			{ "secretKey", Misc.Base64UrlEncode(RandomNumberGenerator.GetBytes(16)) },
			{ "dbConnectionString", "Host=localhost;Port=5432;Database=database;Username=user;Password=password" },

			{ "vkClientId", 0 },
			{ "vkCodeVerifier", Misc.Base64UrlEncode(RandomNumberGenerator.GetBytes(16)) },

			{ "tgUsername", "" },
			{ "tgToken", "" }
		};

		private static readonly JsonSerializerOptions jsonSerializerOptions = new()
		{
			WriteIndented = true
		};

		private static JsonObject config = new();

		public static bool Load()
		{
			if (File.Exists("config.json"))
				config = JsonNode.Parse(File.ReadAllText("config.json")).AsObject();

			foreach (var property in defaultConfig)
			{
				if (config.ContainsKey(property.Key))
					continue;

				config.Add(property.Key, property.Value.DeepClone());
			}

			File.WriteAllText("config.json", config.ToJsonString(jsonSerializerOptions));

			foreach (var property in config)
			{
				if (property.Value.GetValueKind() == JsonValueKind.String
					&& property.Value.GetValue<string>() == ""
					|| property.Value.GetValueKind() == JsonValueKind.Number
					&& property.Value.GetValue<int>() == 0)
					return false;
			}

			return true;
		}

		public static T Get<T>(string name) => config[name].GetValue<T>();
	}
}
