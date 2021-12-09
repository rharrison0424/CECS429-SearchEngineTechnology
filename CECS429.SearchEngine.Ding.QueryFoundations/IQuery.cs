using System;
using System.Collections.Generic;
using Cecs429.Search.Index;
using Cecs429.Search.Text;
using CECS429.SearchEngine.Ding.Core;

namespace Cecs429.Search.Query {
	/// <summary>
	/// A Query is a piece or whole of a Boolean query, whether that piece is a literal string or represents a merging of
 	/// other queries. All nodes in a query parse tree are Query objects.
	/// </summary>
	public interface IQuery {

		bool IsNegative { get; }
		IList<PositionalPosting> GetPostings(IIndex index, BiwordIndex bIndex, bool? positions);
	}
}
