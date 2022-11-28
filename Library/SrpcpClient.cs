using FG.SRPCProtocol.Commands;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace FG.SRPCProtocol {
	public class SrpcpClient : IClient {
		public int Port { get; protected set; }
		public CommandFactory CommandFactory { get; } = new CommandFactory();

		public UdpClient UdpClient { get; protected set; }

		public event EventHandler<OnDiscoveryEventArgs> OnDiscovery;
		public event EventHandler<DataEventArgs> OnCall;
		public event EventHandler<OnResponseEventArgs> OnResponse;
		public event EventHandler<DataEventArgs> OnData;

		public void Execute(ICommand command, IPEndPoint endPoint) => command.Execute(this, endPoint);

		public void Start(int port) {
			if (UdpClient != null)
				return;

			Port = port;

			Console.WriteLine("Preparing UDP client...");
			UdpClient = new UdpClient(AddressFamily.InterNetwork) { EnableBroadcast = true, MulticastLoopback = false };
			UdpClient.Client.Bind(new IPEndPoint(IPAddress.Any, Port));

			Console.WriteLine("Begin recieve...");
			UdpClient.BeginReceive(new AsyncCallback(Recieve), null);
		}

		public void Shutdown() {
			Console.WriteLine("Shutting down...");
			if (UdpClient != null) {
				UdpClient udpClient = UdpClient;
				UdpClient = null;
				udpClient.Close();
				udpClient.Dispose();
			}
		}

		public void Send(byte[] data, IPEndPoint endPoint) {
			if (UdpClient == null || data == null || endPoint == null)
				return;

			Console.WriteLine("Sending packet to {0} ...", endPoint);
			UdpClient.SendAsync(data, data.Length, endPoint);
		}

		protected void Recieve(IAsyncResult asyncResult) {
			if (UdpClient == null)
				return;

			try {
				IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
				byte[] buffer = UdpClient.EndReceive(asyncResult, ref endPoint);

				Console.WriteLine("Packet received from {0}", endPoint);

				Process(buffer, endPoint);
			}
			catch (IOException exception) {
				Console.Error.WriteLine("Recieve exception: {0}", exception);
			}
			finally {
				UdpClient.BeginReceive(new AsyncCallback(Recieve), null);
			}
		}

		protected void Process(byte[] data, IPEndPoint recievedFrom) {
			ByteBuffer buffer = ByteBuffer.Wrap(data);

			byte packetId = buffer.GetByte();
			if (packetId <= 0x01) {
				bool isResponse = packetId == 0x01;
				Console.WriteLine("Recieved {0} packet from {1}", isResponse ? "DISCOVERY_RESPONSE" : "DISCOVERY", recievedFrom);

				OnDiscoveryEventArgs eventArgs = new OnDiscoveryEventArgs(isResponse, recievedFrom);
				OnDiscovery?.Invoke(this, eventArgs);

				if (isResponse) {
					if (eventArgs.Cancelled) {
						Console.WriteLine("Operation cancelled (packet processing)");
						return;
					}
					Execute(CommandFactory.CreateDiscoveryResponseCommand(), recievedFrom);
				}
			}
			else if (packetId == 0x02) {
				Console.WriteLine("Recieved CALL packet from {0}", recievedFrom);
				DataEventArgs eventArgs = new DataEventArgs(buffer, recievedFrom);
				OnCall?.Invoke(this, eventArgs);
			}
			else if (packetId <= 0x04) {
				bool isError = packetId == 0x04;
				Console.WriteLine("Recieved {0} packet from {1}", isError ? "RESPONSE_ERROR" : "RESPONSE_SUCCESS", recievedFrom);
				OnResponseEventArgs eventArgs = new OnResponseEventArgs(isError, buffer, recievedFrom);
				OnResponse?.Invoke(this, eventArgs);
			}
			else {
				DataEventArgs eventArgs = new DataEventArgs(buffer.Seek(0), recievedFrom);
				OnData?.Invoke(this, eventArgs);
			}
		}
	}
}
