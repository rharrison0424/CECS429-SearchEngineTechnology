using System;
using System.Collections.Generic;
using System.Text;

namespace Cecs429.Search.Text {
	/// <summary>
	/// A TokenProcessor applies some rules of normalization to a token from a document and returns a term for that 
	/// token.
	/// </summary>
	public interface ITokenProcessor {
		/// <summary>
		/// Normalize a token into a term.
		/// </summary>
		List<string> ProcessToken(string token, bool removeHyphens);

	}
}
