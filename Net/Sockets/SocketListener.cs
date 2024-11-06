using System.Net;
using System.Net.Sockets;

namespace CISOServer.Net.Sockets
{
	public class SocketListener
	{
		private TcpListener tcpListener;
		private HttpListener httpListener;

		public Action<HttpListenerContext> AuthRequest;

		public SocketListener(int port)
		{
			tcpListener = new TcpListener(IPAddress.Any, port);
		}

		public SocketListener(string hostname)
		{
			httpListener = new HttpListener();
			httpListener.Prefixes.Add(hostname);
			httpListener.Prefixes.Add("http://*:8886/auth/");
		}

		public void Start()
		{
#if DEBUG_EDITOR
			tcpListener.Start();
#else
			httpListener.Start();
#endif
		}

		public async Task<ClientSocket> AcceptSocketAsync(CancellationToken cancellationToken)
		{
#if DEBUG_EDITOR
			var socket = await tcpListener.AcceptSocketAsync(cancellationToken);
			return new ClientSocket(socket, ((IPEndPoint)socket.RemoteEndPoint!).Address);
#else
			while (true)
			{
				var context = await httpListener.GetContextAsync().WaitAsync(cancellationToken);

				if (context.Request.Url.Port == 8886)
				{
					AuthRequest.Invoke(context);
					continue;
				}

				if (!context.Request.IsWebSocketRequest)
				{
					context.Response.Close();
					continue;
				}

				var webSocketContext = await context.AcceptWebSocketAsync(null);
				var webSocket = webSocketContext.WebSocket;
				return new ClientSocket(webSocket, context.Request.RemoteEndPoint.Address);
			}
#endif
		}
	}
}
