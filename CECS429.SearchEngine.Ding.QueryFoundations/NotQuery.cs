using Cecs429.Search.Index;
using Cecs429.Search.Query;
using CECS429.SearchEngine.Ding.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace CECS429.SearchEngine.Ding.QueryFoundations
{
    public class NotQuery : IQuery
    {
        public bool IsNegative { get; }
        public IQuery Child { get; }

        public NotQuery(IQuery child)
        {
            Child = child;
            IsNegative = true;
        }
        public IList<PositionalPosting> GetPostings(IIndex index, BiwordIndex bIndex, bool? positions)
        {
            return Child.GetPostings(index, null, false);
        }
    }
}
