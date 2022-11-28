using FG.SRPCProtocol;

namespace Tests {
	[TestClass]
	public class ByteBufferTests {
		[TestMethod]
		public void TestAllocate() {
			ByteBuffer buffer = ByteBuffer.Allocate(11);

			Assert.AreEqual(11, buffer.Length);

			Assert.AreEqual(0x00, buffer.GetByte());
			Assert.AreEqual(0x00, buffer.GetShort());
			Assert.AreEqual(0.0, buffer.GetDouble());

			Assert.AreEqual(11, buffer.Position);
		}

		[TestMethod]
		public void TestWrap() {
			byte[] bytes = new byte[5] { 0xFF, 0x01, 0x02, 0x00, 0x00 };
			ByteBuffer buffer = ByteBuffer.Wrap(bytes);

			Assert.AreEqual(5, buffer.Length);

			Assert.AreEqual(0xFF, buffer.GetByte());
			Assert.AreEqual(0x0102, buffer.GetShort());
			Assert.AreEqual(0x00, buffer.GetByte());

			Assert.AreEqual(4, buffer.Position);
		}

		[TestMethod]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void TestOutOfRange() {
			ByteBuffer.Allocate(0).GetByte();
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void TestTooShort() {
			ByteBuffer.Allocate(0).GetShort();
		}

		[TestMethod]
		public void TestPutAndGet() {
			double d = 0.45;
			long l = long.MaxValue;

			ByteBuffer buffer = ByteBuffer.Allocate(16);
			buffer.Put(d).Put(l).Seek(0);

			Assert.AreEqual(d, buffer.GetDouble());
			Assert.AreEqual(l, buffer.GetLong());
		}
	}
}