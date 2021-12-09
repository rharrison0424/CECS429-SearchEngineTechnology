using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cecs429.Search.Documents {
	/// <summary>
	/// Represents a document in an index.
	/// </summary>
	public interface IDocument {
		/// <summary>
		/// The ID used by the index to represent the document.
		/// </summary>
		int DocumentId { get; }

		/// <summary>
		/// Gets a stream over the content of the document.
		/// </summary>
		TextReader GetContent();

		/// <summary>
		/// The title of the document, for displaying to the user.
		/// </summary>
		TextReader GetTitle();
	}
}
