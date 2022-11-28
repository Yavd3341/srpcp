using System.Net;

namespace FG.SRPCProtocol.Commands {
	class CallCommand : ICommand {
		public readonly NDS Payload;

		public CallCommand(NDS payload) => Payload = payload;

		public void Execute(IClient client, IPEndPoint endPoint) {
			byte[] payload = Payload.Encode();
			client.Send(ByteBuffer.Allocate(1 + payload.Length).Put(0x02).Put(payload).GetArray(), endPoint);
		}
	}
}
