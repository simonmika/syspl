// Copyright (C) 2013-2014  Simon Mika <simon@mika.se>
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

using Kean.Extension;
using Kean.Collection.Extension;
using Generic = System.Collections.Generic;

namespace Kean.IO
{
	class Encoder :
		Generic.IEnumerator<byte>
	{
		System.Collections.Generic.IEnumerator<char> backend;
		System.Text.Encoding encoding;
		Collection.IQueue<byte> queue;

		#region Constructors
		public Encoder(System.Collections.Generic.IEnumerator<char> backend, System.Text.Encoding encoding)
		{
			this.backend = backend;
			this.encoding = encoding;
			this.queue = new Collection.Queue<byte>();
		}
		~Encoder() { this.Dispose(); }
		#endregion
		bool MoveNext(Collection.IList<char> buffer)
		{
			while (this.queue.Empty && this.backend.MoveNext())
			{
				buffer.Add(this.backend.Current);
				this.queue.Enqueue(this.encoding.GetBytes(buffer.ToArray()));
			}
			return !this.queue.Empty;
		}
		#region IEnumerator<byte> and IEnumerator Members
		public byte Current { get { return this.queue.Peek(); } }
		object System.Collections.IEnumerator.Current {  get { return this.Current; } }
		public bool MoveNext()
		{
			if (!this.queue.Empty)
				this.queue.Dequeue();
			return !this.queue.Empty || this.MoveNext(new Collection.List<char>());
		}
		public void Reset()
		{
			this.queue.Clear();
			this.backend.Reset();
		}
		#endregion
		#region IDisposable Members
		public void Dispose()
		{
			if (this.backend.NotNull())
			{
				this.backend.Dispose();
				this.backend = null;
			}
		}
		#endregion
	}
}
