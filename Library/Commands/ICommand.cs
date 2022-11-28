using System.Net;

namespace FG.SRPCProtocol.Commands {
	public interface ICommand {
		void Execute(IClient client, IPEndPoint endPoint);
	}
}
