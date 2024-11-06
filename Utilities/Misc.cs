using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CISOServer.Utilities
{
	public static class Misc
	{
		public static string AppHostname { get; private set; }

		public static void InitAppHostname(string hostname)
		{
			AppHostname = hostname;
		}

		public static JsonSerializerOptions JsonSerializerOptions { get; } = new()
		{
			IncludeFields = true
		};

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

		public static async Task SaveProfileImageAsync(Stream stream, int userId)
		{
			using var image = await Image.LoadAsync(stream);
			await SaveProfileImageAsync(image, userId);
		}

		public static Task SaveProfileImageAsync(Image image, int userId)
		{
			image.Mutate(x => x.Resize(200, 200));
			return image.SaveAsync($"profileImages/{userId}.jpg", jpegEncoder);
		}
	}
}
