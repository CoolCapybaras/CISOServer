using CISOServer.Utilities.JsonConverters;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace CISOServer.Utilities
{
	public static class Misc
	{
		public static string AppHostname { get; private set; }

		public static JsonSerializerOptions JsonSerializerOptions { get; } = new()
		{
			Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
			IncludeFields = true
		};

		public static JsonSerializerOptions JsonLobbySerializerOptions { get; } = new()
		{
			IncludeFields = true
		};

		public static void Init(string hostname)
		{
			AppHostname = hostname;
			JsonLobbySerializerOptions.Converters.Add(new LobbyJsonConverter());
		}

		private static readonly JpegEncoder jpegEncoder = new()
		{
			Quality = 62
		};

		public static int RandomId()
		{
			return Random.Shared.Next();
		}

		public static int GetRandomGuestId()
		{
			return Random.Shared.Next() - int.MaxValue;
		}

		public static string Base64UrlEncode(byte[] input)
		{
			return Convert.ToBase64String(input).Replace('+', '-').Replace('/', '_').TrimEnd('=');
		}

		public static byte[] Base64UrlDecode(string input)
		{
			string base64 = input.Replace('-', '+').Replace('_', '/');
			switch (input.Length % 4)
			{
				case 2: base64 += "=="; break;
				case 3: base64 += "="; break;
			}
			return Convert.FromBase64String(base64);
		}

		public static async Task SaveProfileImage(Stream stream, int userId)
		{
			using var image = await Image.LoadAsync(stream);
			await SaveProfileImage(image, userId);
		}

		public static Task SaveProfileImage(Image image, int userId)
		{
			image.Mutate(x => x.Resize(200, 200));
			return image.SaveAsync($"profileImages/{userId}.jpg", jpegEncoder);
		}
	}
}
