using HSpellCoverageTester.Common;

namespace HSpellCoverageTester.CorpusReaders
{
	public delegate void ReportProgressDelegate(int progressPercentage, string status, bool isRunning);
    public delegate void HitDocumentDelegate(CorpusDocument doc);

    public interface ICorpusReader
    {
        ReportProgressDelegate ProgressFunc { get; set; }
        HitDocumentDelegate HitDocumentFunc { get;set;}
        bool AbortReading { get;set;}
        void Read();
    }
}
