using Cecs429.Search.Documents;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace Cecs429.SearchEngine.Ding.Core
{
	public class JsonFileDocument : IFileDocument
	{
		public int DocumentId { get; }
		/// <summary>
		/// The absolute path to the document's file.
		/// </summary>
		public string FilePath { get; }


		/// <summary>
		/// Constructs a JsonFileDocument with the given document ID representing the file at the given 
		/// absolute file path.
		/// </summary>
		public JsonFileDocument(int id, string absoluteFilePath)
		{
			DocumentId = id;
			FilePath = absoluteFilePath;

		}
		public TextReader GetTitle()
        {
			JObject obj = JObject.Parse(File.ReadAllText(FilePath));
			string title = (string)obj.SelectToken("Title");
			return new StringReader(title);
		}
		public TextReader GetContent()
		{
			JObject obj = JObject.Parse(File.ReadAllText(FilePath));
			string body = (string)obj.SelectToken("Body");
			return new StringReader(body);
		}

		/// <summary>
		/// A factory method for constructing basic text documents that consist solely of content.
		/// </summary>
		public static JsonFileDocument CreateJsonFileDocument(string absoluteFilePath, int documentId)
		{
			return new JsonFileDocument(documentId, absoluteFilePath);
		}
	}

}
