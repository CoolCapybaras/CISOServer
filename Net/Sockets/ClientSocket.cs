using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;

namespace CISOServer.Net.Sockets
{
	public class ClientSocket : IDisposable
	{
		private WebSocket webSocket;
		private Socket socket;
		private NetworkStream networkStream;
		private StreamReader streamReader;
		private StreamWriter streamWriter;
		private bool disposed;

		public IPAddress Ip { get; }

		public ClientSocket(Socket socket, IPAddress ip)
		{
			this.socket = socket;
			this.networkStream = new NetworkStream(socket);
			this.streamReader = new StreamReader(networkStream);
			this.streamWriter = new StreamWriter(networkStream);
			this.streamWriter.AutoFlush = true;
			this.Ip = ip;
		}

		public ClientSocket(WebSocket webSocket, IPAddress ip)
		{
			this.webSocket = webSocket;
			this.Ip = ip;
		}

		public async Task ReceiveAsync(Stream stream)
		{
#if DEBUG_EDITOR
			string message = await streamReader.ReadLineAsync() ?? throw new SocketException();
			var bytes = Encoding.UTF8.GetBytes(message);
			stream.Write(bytes, 0, bytes.Length);
#else
			var buffer = new byte[4096];
			WebSocketReceiveResult result;
			do
			{
				result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
				stream.Write(buffer, 0, result.Count);
				if (stream.Length > 2097152)
					throw new SocketException();
			}
			while (!result.EndOfMessage);
			if (stream.Length == 0)
				throw new SocketException();
#endif
		}

		public void Send(byte[] message)
		{
#if DEBUG_EDITOR
			streamWriter.WriteLine(Encoding.UTF8.GetString(message));
#else
			webSocket.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None);
#endif
		}

		public Task CloseAsync()
		{
			return webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
		}

		public void Dispose()
		{
			if (disposed)
				return;

			disposed = true;

#if DEBUG_EDITOR
			streamReader.Dispose();
			streamWriter.Dispose();
			networkStream.Dispose();
			socket.Dispose();
#else
			webSocket.Dispose();
#endif

			GC.SuppressFinalize(this);
		}
	}
}
