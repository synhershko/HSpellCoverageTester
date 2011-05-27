namespace HebMorph.CorpusReaders
{
	/// <summary>
	/// Represents a document in a corpus
	/// </summary>
	public class CorpusDocument
	{
		public enum ContentFormat
		{
			Html,
			WikiMarkup,
			Markdown
		}

		public string Id { get; set; }
		public string Title { get; set; }
		public string Content { get; protected set; }
		public ContentFormat Format { get; protected set; }

		public void SetContent(string content, ContentFormat contentFormat)
		{
			Content = content;
			Format = contentFormat;
		}
	}
}
