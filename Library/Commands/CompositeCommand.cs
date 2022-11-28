using System.Net;

namespace FG.SRPCProtocol.Commands {
	class CompositeCommand : ICommand {
		public readonly ICommand[] Commands;

		public CompositeCommand(params ICommand[] commands) => Commands = commands;

		public void Execute(IClient client, IPEndPoint endPoint) {
			foreach (ICommand command in Commands)
				command.Execute(client, endPoint);
		}
	}
}
