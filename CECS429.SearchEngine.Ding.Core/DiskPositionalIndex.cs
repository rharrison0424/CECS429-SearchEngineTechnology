using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cecs429.Search.Index;
using Microsoft.Data.Sqlite;

namespace CECS429.SearchEngine.Ding.Core
{
    public class DiskPositionalIndex : IIndex
    {
        private string mFilePath;

        public DiskPositionalIndex(string path)
        {
            mFilePath = path;
        }
		public IReadOnlyList<string> GetVocabulary()
		{
			List<string> vocabulary = new List<string>();
			string databaseFile = System.IO.Path.Combine(mFilePath, "vocabOffsets.db");
			databaseFile = "DataSource=" + databaseFile;
			var connection = new SqliteConnection(databaseFile);
			connection.Open();
			var transaction = connection.BeginTransaction();
			var command = connection.CreateCommand();

			command.CommandText = "SELECT vocab FROM vocabOffsets";
			SqliteDataReader reader = command.ExecuteReader();

			while (reader.Read())
            {
				vocabulary.Add(reader.GetString(0));
            }
			transaction.Commit();
			connection.Close();
			return vocabulary.AsReadOnly();
		}

		public IList<PositionalPosting> GetPostingsWithPositions(string term)
		{
			long offset;
			string databaseFile = System.IO.Path.Combine(mFilePath, "vocabOffsets.db");
			databaseFile = "DataSource=" + databaseFile;
			var connection = new SqliteConnection(databaseFile);
			connection.Open();
			var transaction = connection.BeginTransaction();
			var command = connection.CreateCommand();

			command.CommandText = "SELECT offset FROM vocabOffsets WHERE vocab = '" + term + "'";
			if (command.ExecuteScalar() == null)
			{
				transaction.Commit();
				connection.Close();
				return new List<PositionalPosting>();
			}
			offset = (Int64)command.ExecuteScalar();

			string postingsFile = System.IO.Path.Combine(mFilePath, "postings.bin");
			BinaryReader binReader = new BinaryReader(new FileStream(postingsFile, FileMode.Open));
            binReader.BaseStream.Seek(offset, SeekOrigin.Begin);

			int documents, terms, docID = 0, position = 0;
			List<PositionalPosting> postings = new List<PositionalPosting>();
			documents = binReader.ReadInt32();
			for (int i = 0; i < documents; i++)
            {
				docID += binReader.ReadInt32();
				postings.Add(new PositionalPosting(docID));
				terms = binReader.ReadInt32();

				for (int j = 0; j < terms; j++)
                {
					position += binReader.ReadInt32();
					postings[i].addPosition(position);
                }
				position = 0;
            }
			transaction.Commit();
			connection.Close();
			binReader.Dispose();
			return postings;
		}
		public IList<PositionalPosting> GetPostingsWithoutPositions(string term)
		{
			long offset;
			string databaseFile = System.IO.Path.Combine(mFilePath, "vocabOffsets.db");
			databaseFile = "DataSource=" + databaseFile;
			var connection = new SqliteConnection(databaseFile);
			connection.Open();
			var transaction = connection.BeginTransaction();
			var command = connection.CreateCommand();

			command.CommandText = "SELECT offset FROM vocabOffsets WHERE vocab = '" + term + "'";
			if (command.ExecuteScalar() == null)
			{
				transaction.Commit();
				connection.Close();
				return new List<PositionalPosting>();
			}
			offset = (Int64)command.ExecuteScalar();

			string postingsFile = System.IO.Path.Combine(mFilePath, "postings.bin");
			BinaryReader binReader = new BinaryReader(new FileStream(postingsFile, FileMode.Open));
			binReader.BaseStream.Seek(offset, SeekOrigin.Begin);

			int documents, docID = 0, terms;
			List<PositionalPosting> postings = new List<PositionalPosting>();
			documents = binReader.ReadInt32();
			for (int i = 0; i < documents; i++)
			{
				docID += binReader.ReadInt32();
				postings.Add(new PositionalPosting(docID));
				terms = binReader.ReadInt32();
				binReader.BaseStream.Seek(terms * 4, SeekOrigin.Current);
			}
			transaction.Commit();
			connection.Close();
			binReader.Dispose();
			return postings;
		}
		public double GetDocumentWeight(int docID)
        {
			string postingsFile = System.IO.Path.Combine(mFilePath, "docWeight.bin");
			BinaryReader binReader = new BinaryReader(new FileStream(postingsFile, FileMode.Open));
			binReader.BaseStream.Seek(docID * 8, SeekOrigin.Begin);
			double weight = binReader.ReadDouble();
			binReader.Dispose();
			return weight;
		}
		public int GetDocumentFrequency(string term)
        {
			long offset;
			string databaseFile = System.IO.Path.Combine(mFilePath, "vocabOffsets.db");
			databaseFile = "DataSource=" + databaseFile;
			var connection = new SqliteConnection(databaseFile);
			connection.Open();
			var transaction = connection.BeginTransaction();
			var command = connection.CreateCommand();

			command.CommandText = "SELECT offset FROM vocabOffsets WHERE vocab = '" + term + "'";
			if (command.ExecuteScalar() == null)
            {
				transaction.Commit();
				connection.Close();
				return 0;
            }
			offset = (Int64)command.ExecuteScalar();

			string postingsFile = System.IO.Path.Combine(mFilePath, "postings.bin");
			BinaryReader binReader = new BinaryReader(new FileStream(postingsFile, FileMode.Open));
			binReader.BaseStream.Seek(offset, SeekOrigin.Begin);
			int docFrequency = binReader.ReadInt32();
			transaction.Commit();
			connection.Close();
			binReader.Dispose();
			return docFrequency;
		}
		public List<int> GetTermFrequency(string term, List<int> docID)
        {
			long offset;
			List<int> frequency = new List<int>();
			string databaseFile = System.IO.Path.Combine(mFilePath, "vocabOffsets.db");
			databaseFile = "DataSource=" + databaseFile;
			var connection = new SqliteConnection(databaseFile);
			connection.Open();
			var transaction = connection.BeginTransaction();
			var command = connection.CreateCommand();

			command.CommandText = "SELECT offset FROM vocabOffsets WHERE vocab = '" + term + "'";
			if (command.ExecuteScalar() == null)
			{
				offset = 0;
			}
			else
            {
				offset = (Int64)command.ExecuteScalar();
			}
			transaction.Commit();

			string postingsFile = System.IO.Path.Combine(mFilePath, "postings.bin");
			BinaryReader binReader = new BinaryReader(new FileStream(postingsFile, FileMode.Open));
			binReader.BaseStream.Seek(offset, SeekOrigin.Begin);
			int termFrequency = 0, currentDocID = 0;
			binReader.ReadInt32();
			bool found = false;
			foreach (int doc in docID)
            {
				while (!found)
				{
					currentDocID += binReader.ReadInt32();
					termFrequency = binReader.ReadInt32();

					if (currentDocID == doc)
					{
						found = true;
						frequency.Add(termFrequency);
					}
					offset = termFrequency * 4;
					binReader.BaseStream.Seek(offset, SeekOrigin.Current);

				}
				found = false;
			}
			binReader.Dispose();
			connection.Close();
			return frequency;
		}
		public IList<PositionalPosting> GetBiword(string term)
		{
			long offset;
			string databaseFile = System.IO.Path.Combine(mFilePath, "biwordOffsets.db");
			databaseFile = "DataSource=" + databaseFile;
			var connection = new SqliteConnection(databaseFile);
			connection.Open();
			var transaction = connection.BeginTransaction();
			var command = connection.CreateCommand();

			command.CommandText = "SELECT offset FROM biwordOffsets WHERE vocab = '" + term + "'";
			if (command.ExecuteScalar() == null)
			{
				transaction.Commit();
				connection.Close();
				return new List<PositionalPosting>();
			}
			offset = (Int64)command.ExecuteScalar();

			string postingsFile = System.IO.Path.Combine(mFilePath, "biword.bin");
			BinaryReader binReader = new BinaryReader(new FileStream(postingsFile, FileMode.Open));
			binReader.BaseStream.Seek(offset, SeekOrigin.Begin);

			int documents, docID = 0;
			List<PositionalPosting> postings = new List<PositionalPosting>();
			documents = binReader.ReadInt32();
			for (int i = 0; i < documents; i++)
			{
				docID += binReader.ReadInt32();
				postings.Add(new PositionalPosting(docID));
			}
			transaction.Commit();
			connection.Close();
			binReader.Dispose();
			return postings;
		}
	}
}
