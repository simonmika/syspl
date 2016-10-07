// Copyright (C) 2012  Simon Mika <simon@mika.se>
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

using System;
using Tasks = System.Threading.Tasks;
using Generic = System.Collections.Generic;
using Kean.Extension;
using Kean.Collection.Extension;

namespace Kean.IO.Abstract
{
	public abstract class CharacterWriter :
		ICharacterWriter
	{
		public char[] NewLine { get; set; }
		public abstract Uri.Locator Resource { get; }
		public abstract bool Opened { get; }
		public abstract bool Writable { get; }
		protected CharacterWriter()
		{
			this.NewLine = new char[] { '\n' };
		}
		~CharacterWriter()
		{
			this.Close().Wait();
		}
		public abstract Tasks.Task<bool> Close();
		#region IDisposable Members
		void IDisposable.Dispose()
		{
			this.Close();
		}
		#endregion
		#region ICharacterWriter Members
		public Tasks.Task<bool> Write(params char[] buffer)
		{
			return this.Write((Generic.IEnumerable<char>)buffer);
		}
		public virtual Tasks.Task<bool> Write(string value)
		{
			return this.Write((Generic.IEnumerable<char>)value);
		}
		public Tasks.Task<bool> Write<T>(T value) where T : IConvertible
		{
			return this.Write(value.ToString((IFormatProvider)System.Globalization.CultureInfo.InvariantCulture.GetFormat(typeof(T))));
		}
		public Tasks.Task<bool> Write(string format, params object[] arguments)
		{
			return this.Write(string.Format(format, arguments));
		}
		public abstract Tasks.Task<bool> Write(Generic.IEnumerable<char> buffer);
		public virtual Tasks.Task<bool> WriteLine()
		{
			return this.Write('\n'); // The newline characters are converted by bool Write(Generic.IEnumerable<char> buffer)
		}
		public virtual Tasks.Task<bool> WriteLine(params char[] buffer)
		{
			return this.Write((System.Collections.Generic.IEnumerable<char>)buffer.Merge(this.NewLine));
		}
		public virtual Tasks.Task<bool> WriteLine(string value)
		{
			return this.Write(((Generic.IEnumerable<char>)value).Append('\n')); // The newline characters are converted by bool Write(Generic.IEnumerable<char> buffer)
		}
		public Tasks.Task<bool> WriteLine<T>(T value) where T : IConvertible
		{
			return this.WriteLine(value.ToString((IFormatProvider)System.Globalization.CultureInfo.InvariantCulture.GetFormat(typeof(T))));
		}
		public Tasks.Task<bool> WriteLine(string format, params object[] arguments)
		{
			return this.WriteLine(string.Format(format, arguments));
		}
		public Tasks.Task<bool> WriteLine(Generic.IEnumerable<char> buffer)
		{
			return this.Write(buffer.Append('\n')); // The newline characters are converted by bool Write(Generic.IEnumerable<char> buffer)
		}
		#endregion
		#region IOutDevice Members
		public virtual bool AutoFlush { get; set; }
		public virtual Tasks.Task<bool> Flush()
		{
			return Tasks.Task.Run(() => true);
		}
		#endregion
	}
}
