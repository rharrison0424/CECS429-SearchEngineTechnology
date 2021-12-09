using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cecs429.Search.Text {
	/// <summary>
	/// A TokenStream creates a sequence of string tokens from the contents of a stream, breaking the bytes of the stream
	/// in some way.
	/// </summary>
	public interface ITokenStream : IDisposable {
		/// <summary>
		/// Returns a sequence of tokens built from the ITokenStream's input source.
		/// </summary>
		IEnumerable<string> GetTokens();
	}
}
