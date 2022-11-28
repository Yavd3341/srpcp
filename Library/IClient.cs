using FG.SRPCProtocol.Commands;
using System;
using System.Net;

namespace FG.SRPCProtocol {
	public interface IClient {
		int Port { get; }

		void Execute(ICommand command, IPEndPoint endPoint);

		void Start(int port);
		void Shutdown();

		void Send(byte[] data, IPEndPoint endPoint);
	}
}
