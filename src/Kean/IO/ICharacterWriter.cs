// Copyright (C) 2011  Simon Mika <simon@mika.se>
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

namespace Kean.IO
{
	public interface ICharacterWriter :
		IOutDevice
	{
		char[] NewLine { get; set; }
		Tasks.Task<bool> Write(params char[] buffer);
		Tasks.Task<bool> Write(string value);
		Tasks.Task<bool> Write<T>(T value) where T : IConvertible;
		Tasks.Task<bool> Write(string format, params object[] arguments);
		Tasks.Task<bool> Write(Generic.IEnumerable<char> buffer);
		Tasks.Task<bool> WriteLine();
		Tasks.Task<bool> WriteLine(params char[] buffer);
		Tasks.Task<bool> WriteLine(string value);
		Tasks.Task<bool> WriteLine<T>(T value) where T : IConvertible;
		Tasks.Task<bool> WriteLine(string format, params object[] arguments);
		Tasks.Task<bool> WriteLine(Generic.IEnumerable<char> buffer);
	}
}