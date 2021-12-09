using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cecs429.Search.Text {
	/// <summary>
	/// An EnglishTokenStream creates tokens by splitting on whitespace.
	/// </summary>
	public class EnglishTokenStream : ITokenStream {
		private TextReader mReader;

		/// <summary>
		/// Constructs a token stream that reads from the given input.
		/// </summary>
		public EnglishTokenStream(TextReader inputStream) {
			mReader = inputStream;
		}

		public IEnumerable<string> GetTokens() {
			string line;
			while ((line = mReader.ReadLine()) != null) {
				var split = line.Split(' ');
				foreach (string s in split) {
					yield return s;
				}
			}
		}

		public void Dispose() {
			mReader?.Dispose();
		}
	}
}
