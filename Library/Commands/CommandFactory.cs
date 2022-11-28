namespace FG.SRPCProtocol.Commands {
	public class CommandFactory {
		public ICommand CreateDiscoveryCommand() => new DiscoveryCommand(false);
		public ICommand CreateDiscoveryResponseCommand() => new DiscoveryCommand(true);
		public ICommand CreateCallCommand(NDS payload) => new CallCommand(payload);
		public ICommand CreateResponseSuccessCommand(NDS payload) => new ResponseCommand(false, payload);
		public ICommand CreateResponseErrorCommand(NDS payload) => new ResponseCommand(true, payload);
		public ICommand CreateCompositeCommand(params ICommand[] commands) => new CompositeCommand(commands);
	}
}
