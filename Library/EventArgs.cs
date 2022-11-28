using System;
using System.Net;

namespace FG.SRPCProtocol {
	public class ReceiveEventArgs : EventArgs {
		public IPEndPoint RecievedFrom { get; }
		public ReceiveEventArgs(IPEndPoint recievedFrom) => RecievedFrom = recievedFrom;
	}

	public class DataEventArgs : ReceiveEventArgs {
		public ByteBuffer Buffer { get; }
		public DataEventArgs(ByteBuffer buffer, IPEndPoint recievedFrom) : base(recievedFrom) => Buffer = buffer;
	}

	public class OnDiscoveryEventArgs : ReceiveEventArgs {
		public bool Cancelled { get; set; } = false;
		public bool IsResponse { get; }
		public OnDiscoveryEventArgs(bool isResponse, IPEndPoint recievedFrom) : base(recievedFrom) => IsResponse = isResponse;
	}

	public class OnResponseEventArgs : DataEventArgs {
		public bool IsError { get; }
		public OnResponseEventArgs(bool isError, ByteBuffer buffer, IPEndPoint recievedFrom) : base(buffer, recievedFrom) => IsError = isError;
	}
}
