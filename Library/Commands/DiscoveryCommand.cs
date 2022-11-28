using System.Net;

namespace FG.SRPCProtocol.Commands {
	class DiscoveryCommand : ICommand {
		private readonly bool IsResponse;

		public DiscoveryCommand(bool isResponse) => IsResponse = isResponse;

		public void Execute(IClient client, IPEndPoint endPoint) =>
			client.Send(new byte[] { (byte)(IsResponse ? 0x01 : 0x00) }, endPoint);
	}
}
