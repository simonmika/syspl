// Copyright (C) 2013, 2016  Simon Mika <simon@mika.se>
//
// This file is part of Kean.
//
// Kean is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Kean is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with Kean.  If not, see <http://www.gnu.org/licenses/>.
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
		internal ByteDevice(System.IO.Stream backend, Uri.Locator resource, bool dontClose = false) :
			base(backend, resource, dontClose)
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
		#region Static Open, Wrap & Create
		#region Open
		public static IByteDevice Open(System.IO.Stream stream, Uri.Locator resource = null)
		{
			return stream.NotNull() ? new ByteDevice(stream, resource) : null;
		}
		public static IByteDevice Open(Uri.Locator resource)
		{
			return ByteDevice.Open(resource, System.IO.FileMode.Open);
		}
		public static IByteDevice Open(Uri.Locator input, Uri.Locator output)
		{
			return ByteDeviceCombiner.Open(ByteDevice.Open(input), ByteDevice.Create(output));
		}
		static IByteDevice Open(Uri.Locator resource, System.IO.FileMode mode)
		{
			IByteDevice result = null;
			if (resource.NotNull())
				switch (resource.Scheme)
				{
					case "assembly":
						result = resource.Authority == "" ? ByteDevice.Open(System.Reflection.Assembly.GetEntryAssembly(), resource.Path) : ByteDevice.Open(System.Reflection.Assembly.Load(new System.Reflection.AssemblyName(resource.Authority)), resource.Path);
						break;
					case "file":
						try
						{
							System.IO.FileStream stream = System.IO.File.Open(System.IO.Path.GetFullPath(resource.PlatformPath), mode, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite);
							if (stream.NotNull())
								result = new ByteDevice(stream, resource);
						}
						catch (System.IO.DirectoryNotFoundException)
						{
							result = null;
						}
						catch (System.IO.FileNotFoundException)
						{
							result = null;
						}
						break;
					case "http":
					case "https":
						if (mode == System.IO.FileMode.Open)
						{
							// TODO: support http and https.
						}
						break;
				}
			return result;
		}
		public static IByteDevice Open(System.Reflection.Assembly assembly, Uri.Path resource)
		{
			return new ByteDevice(assembly.GetManifestResourceStream(assembly.GetName().Name + ((string)resource).Replace('/', '.')), new Uri.Locator("assembly", assembly.GetName().Name, resource));
		}
		#endregion
		#region Create
		public static IByteDevice Create(Uri.Locator resource)
		{
			IByteDevice result = ByteDevice.Open(resource, System.IO.FileMode.Create);
			if (result.IsNull() && resource.NotNull())
			{
				System.IO.Directory.CreateDirectory(resource.Path.FolderPath.PlatformPath);
				result = ByteDevice.Open(resource, System.IO.FileMode.Create);
			}
			return result;
		}
		#endregion
		#region Wrap
		public static IByteDevice Wrap(System.IO.Stream stream, Uri.Locator resource = null)
		{
			return stream.NotNull() ? new ByteDevice(stream, resource, true) : null;
		}
		#endregion
		#endregion
		}
}
