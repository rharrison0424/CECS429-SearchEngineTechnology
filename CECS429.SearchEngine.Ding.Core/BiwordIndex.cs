using Cecs429.Search.Index;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CECS429.SearchEngine.Ding.Core
{
    public class BiwordIndex
    {
		// The dictionary of string vocabulary mapped to a list of postings
		private readonly Dictionary<string, List<PositionalPosting>> mDictionary;

		public BiwordIndex()
		{
			mDictionary = new Dictionary<string, List<PositionalPosting>>();
		}
		public IReadOnlyList<string> GetVocabulary()
		{

			// turn the vocabulary hashset into a list, sort it, and return it
			List<String> vocabulary = new List<String>();
			foreach (var key in mDictionary.Keys)
			{
				vocabulary.Add(key);
			}
			vocabulary.Sort();
			return vocabulary;
		}
		public IList<PositionalPosting> GetPostingsWithoutPositions(string term)
        {
			if (mDictionary.ContainsKey(term))
			{
				return mDictionary[term];
			}
			else
			{

				return null;
			}
		}
		public void AddPosting(string term, int documentId)
		{

			//if term does not exist in dictionary yet, add term to the dictionary and add the document ID where it was found
			if (!mDictionary.ContainsKey(term))
			{

				mDictionary.Add(term, new List<PositionalPosting>());
				mDictionary[term].Add(new PositionalPosting(documentId));
			}
			else
            {
				IList<PositionalPosting> mList = mDictionary[term];
				int lastdocumentId = mList[mList.Count - 1].DocumentId;

				// the last document ID in the postings list is not a duplicate, so add this new document ID
				if (lastdocumentId != documentId)
				{

					mDictionary[term].Add(new PositionalPosting(documentId));
				}
			}
		}
    }
}
