namespace HebMorph.CorpusReaders
{
	public delegate void ReportProgressDelegate(byte progressPercentage, string status, bool isRunning);
    public delegate void HitDocumentDelegate(CorpusDocument doc);

    public interface ICorpusReader
    {
    	event ReportProgressDelegate OnProgress;
    	event HitDocumentDelegate OnDocument;
        bool AbortReading { get;set;}
        void Read();
    }
}
