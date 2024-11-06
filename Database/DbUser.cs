namespace CISOServer.Database
{
	public class DbUser
	{
		public int id { get; set; }
		public string username { get; set; }
		public long authId { get; set; }
		public string ip { get; set; }
		public DateTimeOffset lastlogin { get; set; }
		public string regip { get; set; }
		public DateTimeOffset regdate { get; set; }
		public bool authType { get; set; }
	}
}
