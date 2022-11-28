using FG.SRPCProtocol;

namespace Tests {
	[TestClass]
	public class NDSTests {
		[TestMethod]
		public void TestEncode() {
			// Bool
			CollectionAssert.AreEqual(new byte[1] { 0b01111111 }, new NDS(true).Encode());
			CollectionAssert.AreEqual(new byte[1] { 0b01000000 }, new NDS(false).Encode());

			// Binary
			CollectionAssert.AreEqual(new byte[3] { 0b00000001, 0xFF, 0xFE }, new NDS(new byte[2] { 0xFF, 0xFE }).Encode());

			// Long
			CollectionAssert.AreEqual(new byte[9]
				{ 0b00100000, 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, new NDS(long.MaxValue).Encode());

			// Double
			CollectionAssert.AreEqual(new byte[9]
				{ 0b00110000, 0x3F, 0xDC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCD }, new NDS(0.45).Encode());

			// String
			CollectionAssert.AreEqual(new byte[4] { 0b10000000, 0x01, 0x20, 0x41 }, new NDS(" A").Encode());
		}

		[TestMethod]
		public void TestDecode() {
			// Bool
			Assert.IsTrue(NDS.Decode(new byte[1] { 0b01111111 }).GetBoolean());
			Assert.IsFalse(NDS.Decode(new byte[1] { 0b01000000 }).GetBoolean());

			// Binary
			CollectionAssert.AreEqual(new byte[2] { 0xFF, 0xFE }, NDS.Decode(new byte[3] { 0b00000001, 0xFF, 0xFE }).GetBinary());

			// Long
			Assert.AreEqual(long.MaxValue, NDS.Decode(new byte[9]
				{ 0b00100000, 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }).GetLong());

			// Double
			Assert.AreEqual(0.45, NDS.Decode(new byte[9]
				{ 0b00110000, 0x3F, 0xDC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCD }).GetDouble());

			// String
			Assert.AreEqual(" A", NDS.Decode(new byte[4] { 0b10000000, 0x01, 0x20, 0x41 }).GetString());
		}

		[TestMethod]
		public void TestEncodeComplex() {
			NDS complexNds = new NDS(new List<NDS>() {
				new NDS(true),
				new NDS(" A"),
				new NDS(new byte[2] { 0xFF, 0xFE })
			});

			byte[] expectedData = new byte[] {
				0xC0, 0x02, 0x7F, 0x80, 0x01, 0x20, 0x41, 0x01, 0xFF, 0xFE
			};

			CollectionAssert.AreEqual(expectedData, complexNds.Encode());
		}

		[TestMethod]
		public void TestDecodeComplex() {
			byte[] encodedNds = new byte[] {
				0xC0, 0x02, 0x7F, 0x80, 0x01, 0x20, 0x41, 0x01, 0xFF, 0xFE
			};

			NDS decodedNds = NDS.Decode(encodedNds);

			Assert.AreEqual(NDS.ValueType.Collection, decodedNds.Type);
			Assert.AreEqual(3, decodedNds.GetCollection().Count);
			Assert.IsTrue(decodedNds.GetCollection()[0].GetBoolean());
			Assert.AreEqual(" A", decodedNds.GetCollection()[1].GetString());
			CollectionAssert.AreEqual(new byte[2] { 0xFF, 0xFE },
				decodedNds.GetCollection()[2].GetBinary());
		}
	}
}