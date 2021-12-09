using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cecs429.Search.Index;
using CECS429.SearchEngine.Ding.Core;

namespace Cecs429.Search.Query {
	/// <summary>
	/// An AndQuery composes other Query objects and merges their postings in an intersection-like operation.
	/// </summary>
	public class AndQuery : IQuery {

		public bool IsNegative { get; }
		private List<IQuery> mChildren = new List<IQuery>();

		public IReadOnlyList<IQuery> Children => mChildren;

		public AndQuery(IEnumerable<IQuery> children) {
			mChildren.AddRange(children);
			IsNegative = false;
		}

		public IList<PositionalPosting> GetPostings(IIndex index, BiwordIndex bIndex, bool? positions) {
			// TODO: program this method. Retrieve the postings for the individual query components in
			// mComponents, and OR merge them together.
			if (mChildren.Any(c => c.IsNegative))
            {
				IQuery notQuery = mChildren.Find(c => c.IsNegative);
				IList<PositionalPosting> notQueryPosting = notQuery.GetPostings(index, null, false);
				mChildren.Remove(notQuery);
				IQuery first = mChildren[0];
				IList<PositionalPosting> firstPosting = first.GetPostings(index, null, false);
				mChildren.Remove(first);
				IList<PositionalPosting> result = AndNotMerge(firstPosting, notQueryPosting);
				if (mChildren.Count > 0)
                {
					IList<PositionalPosting> nextPostingList;
					for (int i = 0; i < mChildren.Count; i++)
					{
						nextPostingList = mChildren[i].GetPostings(index, null, false);
						result = AndMerge(result, nextPostingList);
						if (result == null)
						{
							return null;
						}
					}
				}
				return result;
			}
			else
            {
				//Get the first query component
				IList<PositionalPosting> result = mChildren[0].GetPostings(index, null, false);
				IList<PositionalPosting> nextPostingList;
				for (int i = 1; i < mChildren.Count; i++)
				{
					nextPostingList = mChildren[i].GetPostings(index, null, false);
					result = AndMerge(result, nextPostingList);
					if (result == null)
					{
						return null;
					}
				}
				return result;
			}
		}

		public IList<PositionalPosting> AndMerge(IList<PositionalPosting> p1, IList<PositionalPosting> p2)
		{
			if (p1 == null || p2 == null)
			{
				return null;
			}
			IList<PositionalPosting> result = new List<PositionalPosting>();
			int p1ID, p2ID;
			for (int i = 0, j = 0; i < p1.Count && j < p2.Count;) { 
				
				p1ID = p1[i].DocumentId;
				p2ID = p2[j].DocumentId;

				if (p1ID == p2ID)
			    {
					result.Add(p1[i]);
					i++;
					j++;
				}
				else if (p1ID < p2ID)
				{
					i++;
				}
				else if (p2ID < p1ID)
				{
					j++;
				}
				
			}
			return result;
		}

		public IList<PositionalPosting> AndNotMerge(IList<PositionalPosting> p1, IList<PositionalPosting> p2)
        {
			if (p1 == null || p2 == null)
			{
				return null;
			}
			IList<PositionalPosting> result = new List<PositionalPosting>();
			int p1ID, p2ID;
			for (int i = 0, j = 0; i < p1.Count && j < p2.Count;)
			{
				p1ID = p1[i].DocumentId;
				p2ID = p2[j].DocumentId;

				if (p1ID == p2ID)
				{
					i++;
					j++;
				}
				else if (p1ID < p2ID)
				{
					result.Add(p1[i]);
					i++;
				}
				else if (p2ID < p1ID)
				{
					j++;
				}
				if (j == p2.Count && i != p1.Count)
				{
					for (int a = i; a < p1.Count; a++)
					{
						result.Add(p1[a]);
					}
				}

			}
			return result;
		}
		public override string ToString() {
			return string.Join(" ", mChildren.Select(c => c.ToString()));
		}
	}
}
