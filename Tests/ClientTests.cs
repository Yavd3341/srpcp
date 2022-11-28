using FG.SRPCProtocol;
using FG.SRPCProtocol.Commands;
using System.Net;
using System.Net.Sockets;

namespace Tests {
	[TestClass]
	public class ClientTests {
		class TestCommand : ICommand {
			private IClient client;
			private IPEndPoint endPoint;

			public TestCommand(IClient client, IPEndPoint endPoint) {
				this.client = client;
				this.endPoint = endPoint;
			}

			public void Execute(IClient client, IPEndPoint endPoint) {
				Assert.AreSame(this.client, client);
				Assert.AreSame(this.endPoint, endPoint);
			}
		}

		SrpcpClient client = new SrpcpClient();
		IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 2030);

		[TestMethod]
		public void TestExecute() {
			client.Execute(new TestCommand(client, endPoint), endPoint);
		}

		[TestMethod]
		public void TestSend() {
			UdpClient udpClient = new UdpClient();
			udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 2030));
			client.Start(0);

			client.Execute(client.CommandFactory.CreateCallCommand(new NDS(" E")), endPoint);

			IPEndPoint? remoteEndPoint = null;
			byte[] data = udpClient.Receive(ref remoteEndPoint);
			udpClient.Close();
			udpClient.Dispose();
			client.Shutdown();

			CollectionAssert.AreEqual(new byte[] { 0x02, 0x80, 0x01, 0x20, 0x45 }, data);
		}

		[TestMethod]
		public void TestReceiveAndProcess() {
			UdpClient udpClient = new UdpClient();
			client.Start(2030);

			ManualResetEvent resetEvent = new ManualResetEvent(false);

			// Discovery
			OnDiscoveryEventArgs discoveryEventArgs = null;
			client.OnDiscovery += (source, args) => {
				discoveryEventArgs = args;
				resetEvent.Set();
			};

			udpClient.Send(new byte[] { 0x00 }, 1, endPoint);
			Assert.IsTrue(resetEvent.WaitOne(500));
			Assert.IsFalse(discoveryEventArgs.IsResponse);
			resetEvent.Reset();

			udpClient.Send(new byte[] { 0x01 }, 1, endPoint);
			Assert.IsTrue(resetEvent.WaitOne(500));
			Assert.IsTrue(discoveryEventArgs.IsResponse);
			resetEvent.Reset();

			// Call
			DataEventArgs dataEventArgs = null;
			client.OnCall += (source, args) => {
				dataEventArgs = args;
				resetEvent.Set();
			};

			udpClient.Send(new byte[] { 0x02, 0xFF, 0xFF }, 3, endPoint);
			Assert.IsTrue(resetEvent.WaitOne(500));
			Assert.AreEqual(ushort.MaxValue, dataEventArgs.Buffer.GetUShort());
			resetEvent.Reset();

			OnResponseEventArgs responseEventArgs = null;
			client.OnResponse += (source, args) => {
				responseEventArgs = args;
				resetEvent.Set();
			};

			udpClient.Send(new byte[] { 0x03, 0x05 }, 2, endPoint);
			Assert.IsTrue(resetEvent.WaitOne(500));
			Assert.IsFalse(responseEventArgs.IsError);
			Assert.AreEqual(0x05, responseEventArgs.Buffer.GetByte());
			resetEvent.Reset();

			udpClient.Send(new byte[] { 0x04, 0x06 }, 2, endPoint);
			Assert.IsTrue(resetEvent.WaitOne(500));
			Assert.IsTrue(responseEventArgs.IsError);
			Assert.AreEqual(0x06, responseEventArgs.Buffer.GetByte());
			resetEvent.Reset();

			// Other packet
			client.OnData += (source, args) => {
				dataEventArgs = args;
				resetEvent.Set();
			};

			udpClient.Send(new byte[] { 0x05, 0xFF, 0xFF }, 3, endPoint);
			Assert.IsTrue(resetEvent.WaitOne(500));
			CollectionAssert.AreEqual(new byte[] { 0x05, 0xFF, 0xFF }, dataEventArgs.Buffer.GetArray());
			resetEvent.Reset();

			udpClient.Close();
			udpClient.Dispose();
			client.Shutdown();
		}
	}
}