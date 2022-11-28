using System.Net;

namespace FG.SRPCProtocol.Commands {
	class ResponseCommand : ICommand {
		public readonly NDS Payload;
		public readonly bool IsError;

		public ResponseCommand(bool isError, NDS payload) {
			IsError = isError;
			Payload = payload;
		}

		public void Execute(IClient client, IPEndPoint endPoint) {
			byte[] payload = Payload.Encode();
			client.Send(ByteBuffer.Allocate(1 + payload.Length).Put((byte)(IsError ? 0x04 : 0x03)).Put(payload).GetArray(), endPoint);
		}
	}
}
