// Copyright (C) 2013, 2016  Simon Mika <simon@mika.se>
//
// This file is part of SysPL.
//
// SysPL is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
//
// SysPL is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with SysPL.  If not, see <http://www.gnu.org/licenses/>.
//

using Tasks = System.Threading.Tasks;
using Generic = System.Collections.Generic;
using Kean.Extension;
using Kean.Collection.Extension;

namespace Kean.IO
{
	public class ByteDevice :
			SeekableDevice,
			ISeekableByteDevice
	{
		readonly object peekedLock = new object();
		Collection.IQueue<byte> peeked;
		protected override int PeekedCount { get { return this.peeked.NotNull() ? 1 : 0; } }
		public override Tasks.Task<bool> Empty { get { return this.Peek().ContinueWith(value => value.NotNull()); } }
		internal ByteDevice(System.IO.Stream backend, Uri.Locator resource) :
			base(backend, resource)
		{ }
		async Tasks.Task<Collection.IQueue<byte>> RawRead()
		{
			var buffer = new byte[64 * 1024];
			return await this.backend.ReadAsync(buffer, 0, 1) == 1 ? new Collection.Queue<byte>(buffer) : null;
		}
		protected override void OnSeek()
		{
			lock (this.peekedLock)
				this.peeked = null;
		}
		public async Tasks.Task<byte?> Peek()
		{
			lock (this.peekedLock)
				return this.peeked?.Peek() ?? (this.peeked = await this.RawRead())?.Peek();
		}
		public async Tasks.Task<byte?> Read()
		{
			return this.peeked?.Dequeue() ?? (this.peeked = await this.RawRead())?.Dequeue();
		}
		public async Tasks.Task<bool> Write(Generic.IEnumerable<byte> buffer)
		{
			bool result = buffer.NotNull();
			if (result)
			{
				int seek = 0;
				lock (this.peekedLock)
				{
					if (this.peeked.NotNull())
					{
						seek = -this.peeked.Count;
						this.peeked = null;
					}
				}
				if (seek != 0)
					await this.Seek(seek);
				try
				{
					byte[] array = buffer.AsArray();
					await this.backend.WriteAsync(array, 0, array.Length);
					if (this.AutoFlush)
						await this.Flush();
				}
				catch (System.Exception)
				{
					result = false;
				}
			}
			return result;
		}
	}
}