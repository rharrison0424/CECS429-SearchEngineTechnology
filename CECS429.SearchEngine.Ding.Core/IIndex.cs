using System.Collections.Generic;

namespace Cecs429.Search.Index {
	/// <summary>
	/// An IIndex can retrieve postings for a term from a data structure associating terms and the documents that contain
	/// them.
	/// </summary>
	public interface IIndex {
		/// <summary>
		/// Retrieves a list of Postings of documents that contain the given term.
		/// </summary>
		/// <param name="term">a processed string</param>
		IList<PositionalPosting> GetPostingsWithPositions(string term);

		IList<PositionalPosting> GetPostingsWithoutPositions(string term);

		double GetDocumentWeight(int docID);

		int GetDocumentFrequency(string term);

		List<int> GetTermFrequency(string term, List<int> docID);

		/// <summary>
		/// A (sorted) list of all terms in the index vocabulary.
		/// </summary>
		IReadOnlyList<string> GetVocabulary();
	}
}