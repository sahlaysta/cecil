//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

using System;

namespace Mono.Cecil.PE {

	class ByteBuffer {

		private byte [] buf;
		internal virtual byte [] buffer {
			get {
				return buf;
			}
			set {
				buf = value;
			}
		}
		internal int length;
		internal int position;

		public ByteBuffer ()
		{
			this.buf = Empty<byte>.Array;
		}

		public ByteBuffer (int length)
		{
			this.buf = new byte [length];
		}

		public ByteBuffer (byte [] buffer)
		{
			this.buf = buffer ?? Empty<byte>.Array;
			this.length = this.buf.Length;
		}




		//sahlaysta: methods made virtual

		public virtual void Advance (int length)
		{
			position += length;
		}

		public virtual byte ReadByte ()
		{
			return buf [position++];
		}

		public sbyte ReadSByte ()
		{
			return (sbyte) ReadByte ();
		}

		public virtual byte [] ReadBytes (int length)
		{
			var bytes = new byte [length];
			Buffer.BlockCopy (buf, position, bytes, 0, length);
			position += length;
			return bytes;
		}

		public virtual ushort ReadUInt16 ()
		{
			ushort value = (ushort) (buf [position]
				| (buf [position + 1] << 8));
			position += 2;
			return value;
		}

		public short ReadInt16 ()
		{
			return (short) ReadUInt16 ();
		}

		public virtual uint ReadUInt32 ()
		{
			uint value = (uint) (buf [position]
				| (buf [position + 1] << 8)
				| (buf [position + 2] << 16)
				| (buf [position + 3] << 24));
			position += 4;
			return value;
		}

		public int ReadInt32 ()
		{
			return (int) ReadUInt32 ();
		}

		public ulong ReadUInt64 ()
		{
			uint low = ReadUInt32 ();
			uint high = ReadUInt32 ();

			return (((ulong) high) << 32) | low;
		}

		public long ReadInt64 ()
		{
			return (long) ReadUInt64 ();
		}

		public uint ReadCompressedUInt32 ()
		{
			byte first = ReadByte ();
			if ((first & 0x80) == 0)
				return first;

			if ((first & 0x40) == 0)
				return ((uint) (first & ~0x80) << 8)
					| ReadByte ();

			return ((uint) (first & ~0xc0) << 24)
				| (uint) ReadByte () << 16
				| (uint) ReadByte () << 8
				| ReadByte ();
		}

		public virtual int ReadCompressedInt32 ()
		{
			var b = buf [position];
			var u = (int) ReadCompressedUInt32 ();
			var v = u >> 1;
			if ((u & 1) == 0)
				return v;

			switch (b & 0xc0)
			{
				case 0:
				case 0x40:
					return v - 0x40;
				case 0x80:
					return v - 0x2000;
				default:
					return v - 0x10000000;
			}
		}

		public virtual float ReadSingle ()
		{
			if (!BitConverter.IsLittleEndian) {
				var bytes = ReadBytes (4);
				Array.Reverse (bytes);
				return BitConverter.ToSingle (bytes, 0);
			}

			var bytes2 = ReadBytes (4);
			float value = BitConverter.ToSingle (bytes2, 0);
			return value;
		}

		public double ReadDouble ()
		{
			if (!BitConverter.IsLittleEndian) {
				var bytes = ReadBytes (8);
				Array.Reverse (bytes);
				return BitConverter.ToDouble (bytes, 0);
			}

			var bytes2 = ReadBytes (8);
			double value = BitConverter.ToDouble (bytes2, 0);
			return value;
		}

		public virtual void WriteByte (byte value)
		{
			if (position == buf.Length)
				Grow (1);

			buf [position++] = value;

			if (position > length)
				length = position;
		}

		public void WriteSByte (sbyte value)
		{
			WriteByte ((byte) value);
		}

		public virtual void WriteUInt16 (ushort value)
		{
			if (position + 2 > buf.Length)
				Grow (2);

			buf [position++] = (byte) value;
			buf [position++] = (byte) (value >> 8);

			if (position > length)
				length = position;
		}

		public void WriteInt16 (short value)
		{
			WriteUInt16 ((ushort) value);
		}

		public virtual void WriteUInt32 (uint value)
		{
			if (position + 4 > buf.Length)
				Grow (4);

			buf [position++] = (byte) value;
			buf [position++] = (byte) (value >> 8);
			buf [position++] = (byte) (value >> 16);
			buf [position++] = (byte) (value >> 24);

			if (position > length)
				length = position;
		}

		public void WriteInt32 (int value)
		{
			WriteUInt32 ((uint) value);
		}

		public virtual void WriteUInt64 (ulong value)
		{
			if (position + 8 > buf.Length)
				Grow (8);

			buf [position++] = (byte) value;
			buf [position++] = (byte) (value >> 8);
			buf [position++] = (byte) (value >> 16);
			buf [position++] = (byte) (value >> 24);
			buf [position++] = (byte) (value >> 32);
			buf [position++] = (byte) (value >> 40);
			buf [position++] = (byte) (value >> 48);
			buf [position++] = (byte) (value >> 56);

			if (position > length)
				length = position;
		}

		public void WriteInt64 (long value)
		{
			WriteUInt64 ((ulong) value);
		}

		public void WriteCompressedUInt32 (uint value)
		{
			if (value < 0x80)
				WriteByte ((byte) value);
			else if (value < 0x4000) {
				WriteByte ((byte) (0x80 | (value >> 8)));
				WriteByte ((byte) (value & 0xff));
			} else {
				WriteByte ((byte) ((value >> 24) | 0xc0));
				WriteByte ((byte) ((value >> 16) & 0xff));
				WriteByte ((byte) ((value >> 8) & 0xff));
				WriteByte ((byte) (value & 0xff));
			}
		}

		public void WriteCompressedInt32 (int value)
		{
			if (value >= 0) {
				WriteCompressedUInt32 ((uint) (value << 1));
				return;
			}

			if (value > -0x40)
				value = 0x40 + value;
			else if (value >= -0x2000)
				value = 0x2000 + value;
			else if (value >= -0x20000000)
				value = 0x20000000 + value;

			WriteCompressedUInt32 ((uint) ((value << 1) | 1));
		}

		public virtual void WriteBytes (byte [] bytes)
		{
			var length = bytes.Length;
			if (position + length > buf.Length)
				Grow (length);

			Buffer.BlockCopy (bytes, 0, buf, position, length);
			position += length;

			if (position > this.length)
				this.length = position;
		}

		public virtual void WriteBytes (int length)
		{
			if (position + length > buf.Length)
				Grow (length);

			position += length;

			if (position > this.length)
				this.length = position;
		}

		public virtual void WriteBytes (ByteBuffer buffer)
		{
			if (position + buffer.length > this.buf.Length)
				Grow (buffer.length);

			Buffer.BlockCopy (buffer.buf, 0, this.buf, position, buffer.length);
			position += buffer.length;

			if (position > this.length)
				this.length = position;
		}

		public void WriteSingle (float value)
		{
			var bytes = BitConverter.GetBytes (value);

			if (!BitConverter.IsLittleEndian)
				Array.Reverse (bytes);

			WriteBytes (bytes);
		}

		public void WriteDouble (double value)
		{
			var bytes = BitConverter.GetBytes (value);

			if (!BitConverter.IsLittleEndian)
				Array.Reverse (bytes);

			WriteBytes (bytes);
		}

		public virtual void Grow (int desired)
		{
			var current = this.buf;
			var current_length = current.Length;

			var buffer = new byte [System.Math.Max (current_length + desired, current_length * 2)];
			Buffer.BlockCopy (current, 0, buffer, 0, current_length);
			this.buf = buffer;
		}
	}
}
