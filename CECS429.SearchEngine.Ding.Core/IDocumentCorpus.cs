using System;
using System.Collections.Generic;
using System.Text;

namespace Cecs429.Search.Documents {
	/// <summary>
	/// Represents a collection of documents used to build an index.
	/// </summary>
	public interface IDocumentCorpus {
		/// <summary>
		/// Gets all documents in the corpus.
		/// </summary>
		IEnumerable<IDocument> GetDocuments();

		/// <summary>
		/// The number of documents in the corpus.
		/// </summary>
		int CorpusSize { get; }

		/// <summary>
		/// Returns the document with the given document ID.
		/// </summary>
		/// <param name="id">A document id, such as from an IIndex.</param>
		IDocument GetDocument(int id);
	}
}
