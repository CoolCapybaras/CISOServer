using CISOServer.Core;
using CISOServer.Database;
using CISOServer.Utilities;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using System.Text.RegularExpressions;

namespace CISOServer.Net.Packets.Serverbound
{
	public partial class UpdateProfilePacket : IPacket
	{
		[GeneratedRegex("^[A-Za-zА-Яа-я0-9_ ]{3,24}$")]
		private static partial Regex MyNameRegex();

		public static Regex NameRegex { get; } = MyNameRegex();

		public int id = 7;

		public string name;
		public string image;

		public UpdateProfilePacket(string name, string image)
		{
			this.name = name;
			this.image = image;
		}

		public async ValueTask HandleAsync(Server server, Client client)
		{
			if (!client.IsAuthed || !NameRegex.IsMatch(name))
				return;

			Logger.LogInfo($"{client.Name} changed name to {name}");

			client.Name = name;

			if (client.Id < 0)
				return;

			using var db = new ApplicationDbContext();
			var user = await db.users.FirstAsync(x => x.id == client.Id);
			user.username = name;
			await db.SaveChangesAsync();

			if (this.image == null)
				return;

			Image image;
			try
			{
				image = Image.Load(Misc.Base64UrlDecode(this.image));
			}
			catch (Exception ex) when (ex is FormatException || ex is ImageFormatException)
			{
				return;
			}

			await Misc.SaveProfileImage(image, client.Id);

			image.Dispose();
		}
	}
}
