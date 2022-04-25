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
using System.IO;

namespace Mono.Cecil.PE {

	class BinaryStreamWriter : BinaryWriter {

		public int Position {
			get { return (int) BaseStream.Position; }
			set { BaseStream.Position = value; }
		}

		public BinaryStreamWriter (Stream stream)
			: base (stream)
		{
		}

		public void WriteByte (byte value)
		{
			Write (value);
		}

		public void WriteUInt16 (ushort value)
		{
			Write (value);
		}

		public void WriteInt16 (short value)
		{
			Write (value);
		}

		public void WriteUInt32 (uint value)
		{
			Write (value);
		}

		public void WriteInt32 (int value)
		{
			Write (value);
		}

		public void WriteUInt64 (ulong value)
		{
			Write (value);
		}

		public void WriteBytes (byte [] bytes)
		{
			Write (bytes);
		}

		public void WriteDataDirectory (DataDirectory directory)
		{
			Write (directory.VirtualAddress);
			Write (directory.Size);
		}

		public void WriteBuffer (ByteBuffer buffer)
		{
			Write (buffer.buffer, 0, buffer.length);
		}


		//sahlaysta: write from fs stream
		public void WriteBuffer (System.IO.Stream stream)
		{
			byte [] buffer = new byte [4096];
			int read;
			while ((read = stream.Read (buffer, 0, buffer.Length)) != 0)
				Write (buffer, 0, read);
		}

		protected void Advance (int bytes)
		{
			BaseStream.Seek (bytes, SeekOrigin.Current);
		}

		public void Align (int align)
		{
			align--;
			var position = Position;
			var bytes = ((position + align) & ~align) - position;

			for (int i = 0; i < bytes; i++)
				WriteByte (0);
		}
	}
}
