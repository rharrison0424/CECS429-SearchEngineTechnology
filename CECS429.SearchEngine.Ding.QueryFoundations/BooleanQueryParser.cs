using CECS429.SearchEngine.Ding.QueryFoundations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cecs429.Search.Query {
	/// <summary>
	/// Parses Boolean queries to produce an object representation that can be executed to find documents
	/// satisfying the query. Does not handle phrase queries, NOT queries, NEAR queries, 
	/// or wildcard queries... yet.
	/// </summary>
	/// <example>
	/// Index myIndex = ...;
	/// BooleanQueryParser p = new BooleanQueryParser();
	/// IQuery c = p.ParseQuery("white whale + captain ahab");
	/// var postings = c.GetPostings(myIndex);
	/// </example>
	public class BooleanQueryParser {
		/// <summary>
		/// Identifies a portion of a string with a starting index and a length.
		/// </summary>
		private struct StringBounds {
			/// <summary>
			/// The starting index of the string portion.
			/// </summary>
			public int Start { get; }
			/// <summary>
			/// The length of the string portion.
			/// </summary>
			public int Length { get; }


			public StringBounds(int start, int length) {
				Start = start;
				Length = length;
			}
		}

		/// <summary>
		/// Encapsulates a parsed literal from a query, including the literal's bounds within the query
		/// and an IQuery for that portion of the query.
		/// </summary>
		private class Literal {
			/// <summary>
			/// The bounds of the literal within the original query.
			/// </summary>
			public StringBounds Bounds { get; }

			/// <summary>
			/// A parsed IQuery that corresponds to the literal.
			/// </summary>
			public IQuery LiteralComponent { get; }

			public Literal(StringBounds bounds, IQuery literal) {
				Bounds = bounds;
				LiteralComponent = literal;
			}
		}

		/// <summary>
		/// Parses a Boolean search query to produce an IQuery for that query.
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public IQuery ParseQuery(String query) {
			int start = 0;

			// General routine: scan the query to identify a literal, and put that literal into a list.
			//	Repeat until a + or the end of the query is encountered; build an AND query with each
			//	of the literals found. Repeat the scan-and-build-AND-query phase for each segment of the
			// query separated by + signs. In the end, build a single OR query that composes all of the built
			// AND subqueries.

			var allSubqueries = new List<IQuery>();
			do {
				// Identify the next subquery: a portion of the query up to the next + sign.
				StringBounds nextSubquery = FindNextSubquery(query, start);
				// Extract the identified subquery into its own string.
				String subquery = query.Substring(nextSubquery.Start, nextSubquery.Length);
				int subStart = 0;

				// Store all the individual components of this subquery.
				var subqueryLiterals = new List<IQuery>();

				do {
					// Extract the next literal from the subquery.
					Literal lit = FindNextLiteral(subquery, subStart);

					// Add the literal component to the conjunctive list.
					subqueryLiterals.Add(lit.LiteralComponent);

					// Set the next index to start searching for a literal.
					subStart = lit.Bounds.Start + lit.Bounds.Length;

				} while (subStart < subquery.Length);

				// After processing all literals, we are left with a list of query components that we are
				// ANDing together, and must fold that list into the final OR list of components.

				// If there was only one literal in the subquery, we don't need to AND it with anything --
				// its component can go straight into the list.
				if (subqueryLiterals.Count == 1) {
					allSubqueries.Add(subqueryLiterals[0]);
				}
				else {
					// With more than one literal, we must wrap them in an AndQuery component.
					allSubqueries.Add(new AndQuery(subqueryLiterals));
				}
				start = nextSubquery.Start + nextSubquery.Length;
			} while (start < query.Length);

			// After processing all subqueries, we either have a single component or multiple components
			// that must be combined with an OrQuery.
			if (allSubqueries.Count == 1) {
				return allSubqueries[0];
			}
			else if (allSubqueries.Count > 1) {
				return new OrQuery(allSubqueries);
			}
			else {
				return null;
			}
		}

		/// <summary>
		/// Locates the start index and length of the next subquery in the given query string
		/// starting at the given index.
		/// </summary>
		private StringBounds FindNextSubquery(String query, int startIndex) {
			int lengthOut, firstParenthesis, nextParenthesis, nextPlus;
			// Find the start of the next subquery by skipping spaces and + signs.
			char test = query[startIndex];
			while (test == ' ' || test == '+') {
				test = query[++startIndex];
			}

			var subquery = query.Substring(startIndex);
			if (subquery.Contains("("))
			{
				firstParenthesis = query.IndexOf('(', startIndex);
				nextParenthesis = query.IndexOf(')', firstParenthesis + 1);
				nextPlus = query.IndexOf('+', nextParenthesis + 1);
			}
			else { 

				// Find the end of the next subquery.
				nextPlus = query.IndexOf('+', startIndex + 1);
		    }

			if (nextPlus < 0)
			{
				// If there is no other + sign, then this is the final subquery in the
				// query string.
				lengthOut = query.Length - startIndex;
			}
			else
			{
				// If there is another + sign, then the length of this subquery goes up
				// to the next + sign.

				// Move nextPlus backwards until finding a non-space non-plus character.
				test = query[nextPlus];
				while (test == ' ' || test == '+')
				{
					test = query[--nextPlus];
				}

				lengthOut = 1 + nextPlus - startIndex;
				}
			
			// startIndex and lengthOut give the bounds of the subquery.
			return new StringBounds(startIndex, lengthOut);
		}

		/// <summary>
		/// Locates and returns the next literal from the given subquery string.
		/// </summary>
		private Literal FindNextLiteral(String subquery, int startIndex) {

			int subLength = subquery.Length;
			int lengthOut;

			while (subquery[startIndex] == ' ') {
				++startIndex;
			}
			if (subquery[startIndex].Equals('\"'))
            {
				int nextApostrophe = subquery.IndexOf('\"', startIndex + 1);
				if (nextApostrophe < 0)
                {
					lengthOut = subLength - startIndex;
                }
				else
                {
					lengthOut = nextApostrophe - startIndex;
                }
				subquery = subquery.Substring(startIndex, lengthOut + 1);
				string [] phraseQueries = subquery.Split(' ');
				var phraseLiterals = new List<IQuery>();
				foreach (var query in phraseQueries)
                {
					string newQuery = query.Replace("\"", "");
					phraseLiterals.Add(FindNextLiteral(newQuery, 0).LiteralComponent);
				}
				return new Literal(
					new StringBounds(startIndex, lengthOut + 1),
					new PhraseLiteral(phraseLiterals));    
            }
			else if (subquery[startIndex].Equals('('))
            {
				int nextParenthesis = subquery.IndexOf(')', startIndex + 1);
				if (nextParenthesis < 0)
                {
					lengthOut = subLength - startIndex;
                }
				else
                {
					lengthOut = nextParenthesis - startIndex;
                }
				subquery = subquery.Substring(startIndex + 1, lengthOut - 1);
				var DNFquery = ParseQuery(subquery);
                return new Literal(
					new StringBounds(startIndex, lengthOut + 1),
					DNFquery);

            }
			else if (subquery[startIndex].Equals('-'))
            {
				int newStartIndex = startIndex + 1;
				if (subquery[newStartIndex].Equals('\"'))
				{
					int nextApostrophe = subquery.IndexOf('\"', newStartIndex + 1);
					if (nextApostrophe < 0)
					{
						lengthOut = subLength - startIndex;
					}
					else
					{
						lengthOut = nextApostrophe - startIndex;
					}
					subquery = subquery.Substring(newStartIndex, lengthOut);
					newStartIndex = 0;
				}
				else
                {
					int nextSpace = subquery.IndexOf(' ', startIndex + 1);
					if (nextSpace < 0)
					{
						// No more literals in this subquery.
						lengthOut = subLength - startIndex;
					}
					else
					{
						lengthOut = nextSpace - startIndex;
					}
				}
				IQuery notQuery = FindNextLiteral(subquery, newStartIndex).LiteralComponent;
				return new Literal(
					new StringBounds(startIndex, lengthOut + 1),
					new NotQuery(notQuery));
			}
			else
            {
				// Locate the next space to find the end of this literal.
				int nextSpace = subquery.IndexOf(' ', startIndex);
				if (nextSpace < 0)
				{
					// No more literals in this subquery.
					lengthOut = subLength - startIndex;
				}
				else
				{
					lengthOut = nextSpace - startIndex;
				}
				subquery = subquery.Substring(startIndex, lengthOut);
				// This is a term literal containing a single term.
				return new Literal(
				 // startIndex and lengthOut identify the bounds of the literal
				 new StringBounds(startIndex, lengthOut),
				 // we assume this is a single term literal... for now
				 new TermLiteral(subquery));
			}

			/*
			TODO:
			Instead of assuming that we only have single-term literals, modify this method so it will create a PhraseLiteral
			object if the first non-space character you find is a double-quote ("). In this case, the literal is not ended
			by the next space character, but by the next double-quote character. Construct a PhraseLiteral object wrapping
			each of the individual terms in the phrase in this case.
			 */
		}
	}
}
