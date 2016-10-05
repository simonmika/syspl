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
using Kean.Extension;
using Kean.Collection.Extension;

namespace Kean.IO
{
	public class BlockDevice :
			SeekableDevice,
			ISeekableBlockDevice
	{
		readonly object peekedLock = new object();
		Collection.IBlock<byte> peeked;
		protected override int PeekedCount { get { return this.peeked?.Count ?? 0; } }
		public override Tasks.Task<bool> Empty { get { return this.Peek().ContinueWith(value => value.NotNull()); } }
		internal BlockDevice(System.IO.Stream backend, Uri.Locator resource) :
			base(backend, resource)
		{ }
		protected override void OnSeek() {
			lock(this.peekedLock)
				this.peeked = null;
		}
		async Tasks.Task<Collection.IBlock<byte>> RawRead()
		{
			var buffer = new byte[64 * 1024];
			int count = await this.backend.ReadAsync(buffer, 0, buffer.Length);
			return count > 0 ? buffer.Slice(0, count) : null;
		}
		public async Tasks.Task<Collection.IBlock<byte>> Peek()
		{
			lock (this.peekedLock)
				return this.peeked ?? (this.peeked = await this.RawRead());
		}
		public async Tasks.Task<Collection.IBlock<byte>> Read()
		{
			Collection.IBlock<byte> result;
			lock (this.peekedLock)
			{
				result = this.peeked;
				this.peeked = null;
			}
			return result ?? await this.RawRead();
		}
		public async Tasks.Task<bool> Write(Collection.IBlock<byte> buffer)
		{
			bool result = true;
			try
			{
				int seek = 0;
				lock (this.peekedLock) {
					if (this.peeked.NotNull()) {
						seek = -this.peeked.Count;
						this.peeked = null;
					}
				}
				if (seek != 0)
					await this.Seek(seek);
				byte[] array = buffer.AsArray();
				await this.backend.WriteAsync(array, 0, array.Length);
				if (this.AutoFlush)
					await this.Flush();
			}
			catch (System.Exception)
			{
				result = false;
			}
			return result;
		}
	}
}