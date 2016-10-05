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

using System;
using Tasks = System.Threading.Tasks;
using Kean.Extension;

namespace Kean.IO
{
	public abstract class SeekableDevice :
			ISeekableDevice
	{
		protected System.IO.Stream backend;
		public Uri.Locator Resource { get; }
		public bool AutoFlush { get; set; }
		public bool Opened { get { return this.backend.NotNull(); } }
		public bool Readable { get { return this.backend.NotNull() && this.backend.CanRead; } }
		public bool Writable { get { return this.backend.NotNull() && this.backend.CanWrite; } }
		public bool Seekable { get { return this.backend.NotNull() && this.backend.CanSeek; } }
		public abstract Tasks.Task<bool> Empty { get; }
		protected abstract int PeekedCount { get; }
		public long? Position
		{
			get { return this.backend?.Position - this.PeekedCount; }
			set
			{
				if (value.HasValue)
				{
					this.backend.Seek(value.Value < 0 ? -value.Value : value.Value, value.Value < 0 ? System.IO.SeekOrigin.End : System.IO.SeekOrigin.Begin);
					this.OnSeek();
				}
			}
		}
		public long? Size { get { return this.backend?.Length; } }
		public SeekableDevice(System.IO.Stream backend, Uri.Locator resource)
		{
			this.backend = backend;
			this.Resource = resource;
		}
		void IDisposable.Dispose()
		{
			this.Close().Wait();
		}
		public Tasks.Task<bool> Close()
		{
			return Tasks.Task.Run(() =>
			{
				bool r;
				if (r = this.backend != null)
				{
					this.backend.Dispose();
					this.backend = null;
				}
				return r;
			});
		}
		protected abstract void OnSeek();
		public async Tasks.Task<bool> Flush()
		{
			bool result;
			if (result = this.backend.NotNull())
				await this.backend.FlushAsync();
			return result;
		}
		public Tasks.Task<long?> Seek(long delta)
		{
			return Tasks.Task.Run(() =>
			{
				var result = this?.backend.Seek(delta - this.PeekedCount, System.IO.SeekOrigin.Current);
				this.OnSeek();
				return result;
			});
		}
	}
}