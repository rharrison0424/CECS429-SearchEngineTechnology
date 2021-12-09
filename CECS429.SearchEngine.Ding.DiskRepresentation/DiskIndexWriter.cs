using Cecs429.Search.Index;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace CECS429.SearchEngine.Ding.DiskRepresentation
{
    public class DiskIndexWriter
    {
        public List<long> writeIndex(IIndex index, string path)
        {
            string postingsFile = System.IO.Path.Combine(path, "postings.bin");
            BinaryWriter binWriter = new BinaryWriter(new FileStream(postingsFile, FileMode.Create));
            IReadOnlyList<string> vocab = index.GetVocabulary();
            IList<PositionalPosting> postings;
            List<int> positions;
            int currentDoc, previousDoc = 0, currentPosition, previousPosition = 0, docGap, termGap;
            List<long> termStartPositions = new List<long>();
            long newTerm;
            Stream stream;

            foreach (string term in vocab)
            {
                stream = binWriter.BaseStream;
                newTerm = stream.Position;
                termStartPositions.Add(newTerm);
                postings = index.GetPostingsWithPositions(term);
                binWriter.Write(postings.Count);

                foreach (var posting in postings)
                {
                    currentDoc = posting.DocumentId;
                    docGap = currentDoc - previousDoc;
                    binWriter.Write(docGap);
                    previousDoc = currentDoc;
                    positions = posting.Positions;
                    binWriter.Write(positions.Count);

                    foreach (int pos in positions)
                    {
                        currentPosition = pos;
                        termGap = currentPosition - previousPosition;
                        binWriter.Write(termGap);
                        previousPosition = currentPosition;
                    }
                    previousPosition = 0;
                }
                previousDoc = 0;
            }
            binWriter.Dispose();
            return termStartPositions;
        }
        public List<long> writeBiwordIndex(IIndex index, string path)
        {
            string postingsFile = System.IO.Path.Combine(path, "biword.bin");
            BinaryWriter binWriter = new BinaryWriter(new FileStream(postingsFile, FileMode.Create));
            IReadOnlyList<string> vocab = index.GetVocabulary();
            IList<PositionalPosting> postings;
            List<int> positions;
            int currentDoc, previousDoc = 0, docGap;
            List<long> termStartPositions = new List<long>();
            long newTerm;
            Stream stream;

            foreach (string term in vocab)
            {
                stream = binWriter.BaseStream;
                newTerm = stream.Position;
                termStartPositions.Add(newTerm);
                postings = index.GetPostingsWithPositions(term);
                binWriter.Write(postings.Count);

                foreach (var posting in postings)
                {
                    currentDoc = posting.DocumentId;
                    docGap = currentDoc - previousDoc;
                    binWriter.Write(docGap);
                    previousDoc = currentDoc;                  
                }
                previousDoc = 0;
            }
            binWriter.Dispose();
            return termStartPositions;
        }
        public void writeDocumentLength(string path, List<double> docLengths)
        {
            string postingsFile = System.IO.Path.Combine(path, "docWeight.bin");
            BinaryWriter binWriter = new BinaryWriter(new FileStream(postingsFile, FileMode.Open));
            foreach (var length in docLengths)
            {
                binWriter.Write(length);
            }
            binWriter.Dispose();
        }
        public string createIndexFolder(string path)
        {
            string subDirectory = System.IO.Path.Combine(path, "Index");
            if (Directory.Exists(subDirectory))
            {

                Directory.Delete(subDirectory, true);
            }
            System.IO.Directory.CreateDirectory(subDirectory);
            return subDirectory;
        }
        public void createDocWeightFile(string path)
        {
            string postingsFile = System.IO.Path.Combine(path, "docWeight.bin");
            FileStream fs = new FileStream(postingsFile, FileMode.Create);
            fs.Dispose();
        }
        public void createDatabaseFile(IIndex index, List<long> offsets, string path)
        {
            IReadOnlyList<string> vocab = index.GetVocabulary();
            long offset;
            SqliteParameter vocabParameter, offsetParameter;
            string databaseFile = System.IO.Path.Combine(path, "vocabOffsets.db"), word;
            databaseFile = "DataSource=" + databaseFile;
            var connection = new SqliteConnection(databaseFile);
            connection.Open();
            var transaction = connection.BeginTransaction();
            var command = connection.CreateCommand();

            command.CommandText = "DROP TABLE IF EXISTS vocabOffsets";
            command.ExecuteNonQuery();

            command.CommandText = "CREATE TABLE vocabOffsets(vocab TEXT PRIMARY KEY, offset INT)";
            command.ExecuteNonQuery();

            command.CommandText = "INSERT INTO vocabOffsets(vocab, offset) VALUES(@vocab, @offset)";
            if (vocab.Count == offsets.Count)
            {
                for (int i = 0; i < vocab.Count; i++)
                {
                    word = vocab[i];
                    offset = offsets[i];
                    vocabParameter = new SqliteParameter("@vocab", SqlDbType.Text) { Value = word };
                    offsetParameter = new SqliteParameter("@offset", SqlDbType.Int) { Value = offset };
                    command.Parameters.Add(vocabParameter);
                    command.Parameters.Add(offsetParameter);
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }
                transaction.Commit();
            }
            connection.Close();
        }
        public void createBiwordDatabaseFile(IIndex index, List<long> offsets, string path)
        {
            IReadOnlyList<string> vocab = index.GetVocabulary();
            long offset;
            SqliteParameter vocabParameter, offsetParameter;
            string databaseFile = System.IO.Path.Combine(path, "biwordOffsets.db"), word;
            databaseFile = "DataSource=" + databaseFile;
            var connection = new SqliteConnection(databaseFile);
            connection.Open();
            var transaction = connection.BeginTransaction();
            var command = connection.CreateCommand();

            command.CommandText = "DROP TABLE IF EXISTS biwordOffsets";
            command.ExecuteNonQuery();

            command.CommandText = "CREATE TABLE biwordOffsets(vocab TEXT PRIMARY KEY, offset INT)";
            command.ExecuteNonQuery();

            command.CommandText = "INSERT INTO biwordOffsets(vocab, offset) VALUES(@vocab, @offset)";
            if (vocab.Count == offsets.Count)
            {
                for (int i = 0; i < vocab.Count; i++)
                {
                    word = vocab[i];
                    offset = offsets[i];
                    vocabParameter = new SqliteParameter("@vocab", SqlDbType.Text) { Value = word };
                    offsetParameter = new SqliteParameter("@offset", SqlDbType.Int) { Value = offset };
                    command.Parameters.Add(vocabParameter);
                    command.Parameters.Add(offsetParameter);
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }
                transaction.Commit();
            }
            connection.Close();
        }
    }
}
