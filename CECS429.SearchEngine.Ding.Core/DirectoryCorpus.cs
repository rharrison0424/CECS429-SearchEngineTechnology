using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cecs429.SearchEngine.Ding.Core;

namespace Cecs429.Search.Documents {
	/// <summary>
	/// A method for constructing an IFileDocument given a path to that document.
	/// </summary>
	public delegate IFileDocument FileDocumentFactory(string absoluteFilePath, int documentId);


	/// <summary>
	/// A DirectoryCorpus represents a corpus found in a single directory on a local file system.
	/// Instead of assuming a particular file type and format, a consumer must register file extensions
	/// with the corpus so that the corpus will know to load files of particular extensions.
	/// </summary>
	/// <see cref="RegisterFileDocumentFactory(string, FileDocumentFactory)"/>
	public class DirectoryCorpus : IDocumentCorpus {
		// Maintains a map of registered file types that the corpus knows how to load.
		private Dictionary<string, FileDocumentFactory> mFactories
			= new Dictionary<string, FileDocumentFactory>();

		// The map from document ID to document.
		private Dictionary<int, IFileDocument> mDocuments;

		// A filtering function for identifying documents that should get loaded.
		private Func<string, bool> mFileFilter;

		public string DirectoryPath { get; }

		/// <summary>
		/// The number of documents in the corpus.
		/// </summary>
		public int CorpusSize {
			get {
				if (mDocuments == null)
					mDocuments = ReadDocuments();
				return mDocuments.Count;
			}
		}

		/// <summary>
		/// Constructs a corpus over an absolute directory path.
		/// </summary>
		/// <remarks>Before calling GetDocuments(), you must register a factory method with
		/// the RegisterFileDocumentFactory method. Otherwise, the corpus will not know what to do with
		/// the files it finds. The LoadTextDirectory facade method can simplify this initialization.</remarks>
		/// <see cref="LoadTextDirectory(string, string)"/>
		public DirectoryCorpus(string directoryPath)
			: this(directoryPath, s => true) {
		}

		/// <summary>
		/// Constructs a corpus over an absolute directory path, only loading files whose file names satisfy
		/// the given predicate filter.
		/// </summary>
		public DirectoryCorpus(string directoryPath, Func<string, bool> fileFilter) {
			DirectoryPath = directoryPath;
			mFileFilter = fileFilter;
		}

		/// <summary>
		/// Finds all file names that match the corpus filter predicate and have a known file extension.
		/// </summary>
		private IEnumerable<string> FindFiles() {
			return Directory.EnumerateFiles(DirectoryPath)
							.Where(mFileFilter)
							.Where(f => mFactories.ContainsKey(Path.GetExtension(f)));
		}

		/// <summary>
		/// Reads all documents in the corpus into a dictionary from ID to document object.
		/// </summary>
		private Dictionary<int, IFileDocument> ReadDocuments() {
			// Build the mapping from 0-based document ID to each file in the directory path.

			// First, identify all compatible files: those that match the file extension filter, and for which we have
			// a registered factory method.
			var files = FindFiles();

			// Pair each file with an increasing document ID (0-based), then invoke the factory object corresponding
			// to the file's extension to construct a new IFileDocument with the paired document ID. 
			return files
				.Zip(Enumerable.Range(0, files.Count()),
					(fileName, docId) => {
						var extension = Path.GetExtension(fileName);
						var factory = mFactories[extension];
						return factory(fileName, docId);
					})
				// Then create a dictionary that maps from a document's ID to the document itself.
				.ToDictionary(doc => doc.DocumentId);
		}

		/// <summary>
		/// Gets all documents in the corpus.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IFileDocument> GetDocuments() {
			if (mDocuments == null) {
				mDocuments = ReadDocuments();
			}
			return mDocuments.Values;
		}

		public IDocument GetDocument(int id) {
			return mDocuments[id];
		}

		IEnumerable<IDocument> IDocumentCorpus.GetDocuments() {
			return GetDocuments();
		}

		/// <summary>
		/// Registers a factory method for loading documents of the given file extension. By default, a corpus
		/// does not know how to load any files -- this method must be called prior to GetDocuments().
		/// </summary>
		public void RegisterFileDocumentFactory(string fileExtension, FileDocumentFactory factory) {
			mFactories[fileExtension] = factory;
		}

		/// <summary>
		/// Constructs a corpus over a directory of simple text documents.
		/// </summary>
		/// <param name="fileExtension">The extension of the text documents to load, e.g., ".txt".</param>
		public static DirectoryCorpus LoadTextDirectory(string absolutePath, string fileExtension) {
			DirectoryCorpus corpus = new DirectoryCorpus(absolutePath);
			corpus.RegisterFileDocumentFactory(fileExtension, TextFileDocument.CreateTextFileDocument);
			return corpus;
		}

		public static DirectoryCorpus LoadJsonDirectory(string absolutePath, string fileExtension)
        {
			DirectoryCorpus corpus = new DirectoryCorpus(absolutePath);
			corpus.RegisterFileDocumentFactory(fileExtension, JsonFileDocument.CreateJsonFileDocument);
			return corpus;
		}
		public static DirectoryCorpus LoadDifferentDirectory(string absolutePath, string fileExtension1, string fileExtension2)
		{
			DirectoryCorpus corpus = new DirectoryCorpus(absolutePath);
			corpus.RegisterFileDocumentFactory(fileExtension1, TextFileDocument.CreateTextFileDocument);
			corpus.RegisterFileDocumentFactory(fileExtension2, JsonFileDocument.CreateJsonFileDocument);
			return corpus;
		}
	}
}

