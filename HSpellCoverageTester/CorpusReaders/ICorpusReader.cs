namespace HSpellCoverageTester.CorpusReaders
{
    public delegate void HitDocumentDelegate(object documentContent, object documentLocation);

    public interface ICorpusReader
    {
        ReportProgressDelegate ProgressFunc { get; set; }
        HitDocumentDelegate HitDocumentFunc { get;set;}
        bool AbortReading { get;set;}
        void Read();
    }
}
