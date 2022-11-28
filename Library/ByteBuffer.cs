using System;

namespace FG.SRPCProtocol {
	public class ByteBuffer : ICloneable {
		private byte[] array;

		public int Position { get; set; }
		public int Length => array.Length;
		public int Remaining => Length - Position;

		private ByteBuffer(byte[] array) => this.array = array;

		public static ByteBuffer Allocate(int size) => new ByteBuffer(new byte[size]);

		public static ByteBuffer Wrap(byte[] array) => new ByteBuffer(array);

		public ByteBuffer Resize(int size) {
			Array.Resize(ref array, size);
			return this;
		}

		public ByteBuffer Trim() => Resize(Position);

		public ByteBuffer Seek(int position) {
			Position = position;
			return this;
		}

		public ByteBuffer Put(byte value) {
			array[Position++] = value;
			return this;
		}
		public ByteBuffer Put(byte[] value) {
			value.CopyTo(array, Position);
			Position += value.Length;
			return this;
		}

		public ByteBuffer Put(double value) {
			byte[] data = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(data);
			return Put(data);
		}

		public ByteBuffer Put(long value) {
			byte[] data = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(data);
			return Put(data);
		}

		public ByteBuffer Put(ushort value) {
			byte[] data = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(data);
			return Put(data);
		}

		public ByteBuffer Put(short value) {
			byte[] data = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(data);
			return Put(data);
		}

		public byte GetByte() => array[Position++];

		public void GetBytes(ref byte[] array) {
			Array.Copy(this.array, Position, array, 0, array.Length);
			Position += array.Length;
		}

		public byte[] GetBytes(int count) {
			byte[] array = new byte[count];
			GetBytes(ref array);
			return array;
		}

		public double GetDouble() {
			byte[] data = new byte[8];
			GetBytes(ref data);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(data);
			return BitConverter.ToDouble(data, 0);
		}

		public long GetLong() {
			byte[] data = new byte[8];
			GetBytes(ref data);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(data);
			return BitConverter.ToInt64(data, 0);
		}

		public ushort GetUShort() {
			byte[] data = new byte[2];
			GetBytes(ref data);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(data);
			return BitConverter.ToUInt16(data, 0);
		}

		public short GetShort() {
			byte[] data = new byte[2];
			GetBytes(ref data);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(data);
			return BitConverter.ToInt16(data, 0);
		}

		public byte[] GetArray() => array;

		public object Clone() {
			ByteBuffer byteBuffer = Wrap((byte[]) array.Clone());
			byteBuffer.Position = Position;
			return byteBuffer;
		}
	}
}
