using System;
using System.Collections.Generic;
using System.Text;

namespace FG.SRPCProtocol {

	// NDS - Network Data Structure format
	public class NDS {
		public enum ValueType {
			Binary, Long, Double, Boolean, String, Collection
		}

		public ValueType Type { get; private set; }
		private object value;

		public NDS(byte[] value) {
			this.value = value;
			Type = ValueType.Binary;
		}

		public NDS(long value) {
			this.value = value;
			Type = ValueType.Long;
		}

		public NDS(double value) {
			this.value = value;
			Type = ValueType.Double;
		}

		public NDS(bool value) {
			this.value = value;
			Type = ValueType.Boolean;
		}

		public NDS(string value) {
			this.value = value;
			Type = ValueType.String;
		}

		public NDS(List<NDS> value) {
			this.value = value;
			Type = ValueType.Collection;
		}

		public void Set(byte[] value) {
			this.value = value;
			Type = ValueType.Binary;
		}

		public void Set(long value) {
			this.value = value;
			Type = ValueType.Long;
		}

		public void Set(double value) {
			this.value = value;
			Type = ValueType.Double;
		}

		public void Set(bool value) {
			this.value = value;
			Type = ValueType.Boolean;
		}

		public void Set(string value) {
			this.value = value;
			Type = ValueType.String;
		}

		public void Set(List<NDS> value) {
			this.value = value;
			Type = ValueType.Collection;
		}

		public byte[] GetBinary() {
			if (Type != ValueType.Binary)
				throw new InvalidCastException("Can't cast " + Type.ToString().ToLower() + " to binary");
			return (byte[]) value;
		}

		public long GetLong() {
			if (Type != ValueType.Long)
				throw new InvalidCastException("Can't cast " + Type.ToString().ToLower() + " to long");
			return (long) value;
		}

		public double GetDouble() {
			if (Type != ValueType.Double)
				throw new InvalidCastException("Can't cast " + Type.ToString().ToLower() + " to double");
			return (double) value;
		}

		public bool GetBoolean() {
			if (Type != ValueType.Boolean)
				throw new InvalidCastException("Can't cast " + Type.ToString().ToLower() + " to boolean");
			return (bool) value;
		}

		public string GetString() {
			if (Type != ValueType.String)
				throw new InvalidCastException("Can't cast " + Type.ToString().ToLower() + " to string");
			return (string) value;
		}

		public object GetObject() => value;

		public List<NDS> GetCollection() {
			if (Type != ValueType.Collection)
				throw new InvalidCastException("Can't cast " + Type.ToString().ToLower() + " to collection");
			return (List<NDS>) value;
		}

		public override string ToString() {
			if (Type == ValueType.Binary)
				return '(' + string.Join(", ", GetBinary()) + ')';
			else if (Type == ValueType.Collection)
				return '[' + string.Join(", ", GetCollection()) + ']';
			else if (Type == ValueType.String)
				return '"' + GetString() + '"';
			else
				return value.ToString();
		}

		public byte[] Encode() {
			switch (Type) {
				case ValueType.Boolean:
					if ((bool) value)
						return new byte[] { 0b01111111 };
					else
						return new byte[] { 0b01000000 };

				case ValueType.Binary: {
					byte[] data = (byte[]) value;
					if (data.Length > 32)
						throw new PayloadOverflowException(string.Format("Too many bytes to encode (max. count: 32, provided: %d)", data.Length));
					else if (data.Length == 0)
						throw new PayloadOverflowException("Empty byte arrays are not supported");
					return ByteBuffer.Allocate(data.Length + 1).Put((byte) ((0b00011111 & data.Length) - 1)).Put(data).GetArray();
				}

				case ValueType.Collection: {
					List<NDS> data = (List<NDS>) value;

					if (data.Count > 1018)
						throw new PayloadOverflowException(string.Format("Collection is too big to encode (max. size: 1017 (depends), provided: %d)", data.Count));
					else if (data.Count == 0)
						throw new PayloadOverflowException("Empty collections are not supported");

					ByteBuffer encoded = ByteBuffer.Allocate(1019);
					try {
						encoded.Put((short) (0b11000000_00000000 | (0b00111111_11111111 & (data.Count - 1))));
						foreach (NDS nv in data)
							encoded.Put(nv.Encode());
					}
					catch (OverflowException e) {
						throw new PayloadOverflowException("Encoded collection is too big (max. length: 1019 (depends)).", e);
					}

					return encoded.Trim().GetArray();
				}

				case ValueType.Double:
					return ByteBuffer.Allocate(9).Put(0b00110000).Put((double) value).GetArray();

				case ValueType.Long:
					return ByteBuffer.Allocate(9).Put(0b00100000).Put((long) value).GetArray();

				case ValueType.String: {
					byte[] data = Encoding.UTF8.GetBytes((string) value);
					if (data.Length > 1018)
						throw new PayloadOverflowException(string.Format("string is too big to encode (max. length: 1017, provided: %d)", data.Length));
					else if (data.Length == 0)
						throw new PayloadOverflowException("Empty string are not supported");
					return ByteBuffer.Allocate(data.Length + 2).Put((short) (0b10000000_00000000 | (0b00111111_11111111 & (data.Length - 1)))).Put(data).GetArray();
				}
			}
			return null;
		}

		public static NDS Decode(byte[] data) => Decode(ByteBuffer.Wrap(data));

		public static NDS Decode(ByteBuffer buffer) {
			byte id = buffer.GetByte();
			switch ((id >> 6) & 0b11) {
				case 0b00: {
					if ((id >> 5) == 0) {
						byte[] blob = new byte[(id & 0b00011111) + 1];
						buffer.GetBytes(ref blob);
						return new NDS(blob);
					}
					else if ((id >> 4) == 2) {
						return new NDS(buffer.GetLong());
					}
					else if ((id >> 4) == 3) {
						return new NDS(buffer.GetDouble());
					}
				}
				break;

				case 0b01: {
					return new NDS((id & 0b00111111) != 0);
				}

				case 0b10: {
					buffer.Position--;
					short len = (short) ((buffer.GetShort() & 0b00111111_11111111) + 1);
					byte[] encodedString = new byte[len];
					buffer.GetBytes(ref encodedString);
					return new NDS(Encoding.UTF8.GetString(encodedString));
				}

				case 0b11: {
					buffer.Position--;
					short len = (short) ((buffer.GetShort() & 0b00111111_11111111) + 1);
					List<NDS> list = new List<NDS>(len);
					for (int i = 0; i < len; i++)
						list.Add(Decode(buffer));
					return new NDS(list);
				}
			}
			throw new ArgumentException("Can't decode provided data (data ID: " + (id >> 6) + ")");
		}

		public override bool Equals(object o) => o is NDS variable && variable.Type == Type && variable.GetObject().Equals(value);
	}

	public class PayloadOverflowException : Exception {
		public PayloadOverflowException() : base() { }
		public PayloadOverflowException(string message) : base(message) { }
		public PayloadOverflowException(string message, Exception cause) : base(message, cause) { }
	}
}
