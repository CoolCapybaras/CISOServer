using CISOServer.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CISOServer.Utilities.JsonConverters
{
	public class LobbyJsonConverter : JsonConverter<GameLobby>
	{
		public override GameLobby Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}

		public override void Write(Utf8JsonWriter writer, GameLobby value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteNumber("Id", value.Id);
			writer.WriteString("Name", value.Name);
			writer.WriteNumber("MaxClients", value.MaxClients);
			writer.WriteNumber("ClientsCount", value.Players.Count);
			writer.WriteEndObject();
		}
	}
}
