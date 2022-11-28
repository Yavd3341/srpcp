using FG.SRPCProtocol;
using FG.SRPCProtocol.Commands;
using System.Net;

namespace Tests {
	[TestClass]
	public class CommandTests {
		class TestClient : IClient {
			public int Port => throw new NotImplementedException();

			public IPEndPoint EndPoint { get; private set; }
			public byte[] Data { get => buffer.Trim().GetArray(); }

			private ByteBuffer buffer = ByteBuffer.Allocate(1024);

			public void Execute(ICommand command, IPEndPoint endPoint) => command.Execute(this, endPoint);
			public void Send(byte[] data, IPEndPoint endPoint) {
				buffer.Put(data);
				EndPoint = endPoint;
			}
			public void Shutdown() => throw new NotImplementedException();
			public void Start(int port) => throw new NotImplementedException();
		}

		IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 2030);
		CommandFactory factory = new CommandFactory();

		[TestMethod]
		public void TestDiscovery() {
			TestClient client = new TestClient();

			client.Execute(factory.CreateDiscoveryCommand(), endPoint);
			Assert.AreSame(endPoint, client.EndPoint);
			CollectionAssert.AreEqual(new byte[] { 0x00 }, client.Data);
		}

		[TestMethod]
		public void TestDiscoveryResponse() {
			TestClient client = new TestClient();

			client.Execute(factory.CreateDiscoveryResponseCommand(), endPoint);
			Assert.AreSame(endPoint, client.EndPoint);
			CollectionAssert.AreEqual(new byte[] { 0x01 }, client.Data);
		}

		[TestMethod]
		public void TestCall() {
			TestClient client = new TestClient();

			client.Execute(factory.CreateCallCommand(new NDS(" A")), endPoint);
			Assert.AreSame(endPoint, client.EndPoint);
			CollectionAssert.AreEqual(new byte[] { 0x02, 0x80, 0x01, 0x20, 0x41 }, client.Data);
		}

		[TestMethod]
		public void TestResponseSuccess() {
			TestClient client = new TestClient();

			client.Execute(factory.CreateResponseSuccessCommand(new NDS(" B")), endPoint);
			Assert.AreSame(endPoint, client.EndPoint);
			CollectionAssert.AreEqual(new byte[] { 0x03, 0x80, 0x01, 0x20, 0x42 }, client.Data);
		}

		[TestMethod]
		public void TestResponseError() {
			TestClient client = new TestClient();

			client.Execute(factory.CreateResponseErrorCommand(new NDS(" C")), endPoint);
			Assert.AreSame(endPoint, client.EndPoint);
			CollectionAssert.AreEqual(new byte[] { 0x04, 0x80, 0x01, 0x20, 0x43 }, client.Data);
		}

		[TestMethod]
		public void TestCompositeCommand() {
			TestClient client = new TestClient();

			ICommand cmd = factory.CreateCompositeCommand(
				factory.CreateDiscoveryCommand(),
				factory.CreateCallCommand(new NDS(" D"))
			);

			client.Execute(cmd, endPoint);
			Assert.AreSame(endPoint, client.EndPoint);
			CollectionAssert.AreEqual(new byte[] { 0x00, 0x02, 0x80, 0x01, 0x20, 0x44 }, client.Data);
		}
	}
}