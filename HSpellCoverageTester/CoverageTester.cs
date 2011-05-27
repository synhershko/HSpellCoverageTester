using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

using HSpellCoverageTester.CorpusReaders;
using HSpellCoverageTester.Common;
using HebMorph.DataStructures;

namespace HSpellCoverageTester
{
    public delegate void ReportProgressDelegate(int progressPercentage, string status, bool isRunning);

    public class CoverageTester
    {
        public event ProgressChangedEventHandler ProgressChanged;
        public bool ComputeCoverage = true;

        private readonly ICorpusReader corpusReader;
        private readonly string HSpellPath = string.Empty;
        private HebMorph.StreamLemmatizer lemmatizer;
        private DictRadix<CoverageData> radix;

        protected class CoverageData
        {
            public bool KnownToHSpell;
            public int Count;
            public object FirstKnownLocation;
        }

    	public bool WasAbortSet { get; set; }

    	public CoverageTester(ICorpusReader cr, string hspellPath)
        {
        	WasAbortSet = false;
        	corpusReader = cr;
            HSpellPath = hspellPath;
        }

        public void Run()
        {
        }

        public void Run(string reportPath)
        {
            radix = null;
            radix = new DictRadix<CoverageData>();

            ReportProgress(0, "Initializing hspell...", true);
            lemmatizer = new HebMorph.StreamLemmatizer(HSpellPath, true, false) {TolerateWhenLemmatizingStream = false};

        	corpusReader.HitDocumentFunc = GotDocument;
            corpusReader.ProgressFunc = ReportProgress;
            corpusReader.AbortReading = false;
            corpusReader.Read();

            if (!WasAbortSet && !string.IsNullOrEmpty(reportPath))
            {
                SaveReport(reportPath);
            }

            ReportProgress(100, "Finalizing...", false);
        }

        protected void ReportProgress(int progressPercentage, string status, bool isRunning)
        {
            var pi = new ProgressInfo {Status = status, IsStillRunning = isRunning};
        	ReportProgress(progressPercentage, pi);
        }

        protected void ReportProgress(int progressPercentage, ProgressInfo pi)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged(this, new ProgressChangedEventArgs(progressPercentage, pi));
            }
        }

        private void GotDocument(object doc, object docId)
        {
            if (doc == null)
                return;

            var docContents = doc.ToString();
            var word = string.Empty;
            var tokens = new List<HebMorph.Token>();

            lemmatizer.SetStream(new System.IO.StringReader(docContents));
            
            // The HebMorph lemmatizer will always return a token, unless an unrecognized Hebrew
            // word was hit, then an empty tokens array will be returned.
            while (lemmatizer.LemmatizeNextToken(out word, tokens) != 0)
            {
                // Invalid token
                if (string.IsNullOrEmpty(word) || word.Length <= 1)
                    continue;

                // Unrecognized Hebrew word
                if (tokens.Count == 0)
                {
                    var o = radix.Lookup(word);
                    if (o != null)
                    {
                        o.Count++;
                    }
                    else
                    {
                        o = new CoverageData {Count = 1, FirstKnownLocation = docId, KnownToHSpell = false};
                    	radix.AddNode(word, o);
                    }
                    continue;
                }

                // Otherwise, the token is either in the dictionary already, or is not a Hebrew word. If we
                // are performing complete coverage computation, add it to the radix as well
                
                // If we are performing a coverage calculation
                if (ComputeCoverage)
                {
                    // A non-Hebrew word
                    if (tokens.Count == 1 && !(tokens[0] is HebMorph.HebrewToken))
                        continue;

                    // Hebrew words with one lemma or more - store the word in the radix with a flag
                    // signaling it was indeed found
                    var o = radix.Lookup(word);
                    if (o != null)
                    {
                        o.Count++;
                    }
                    else
                    {
                        o = new CoverageData {Count = 1, FirstKnownLocation = docId, KnownToHSpell = true};
                    	radix.AddNode(word, o);
                    }
                }
            }
        }

        public void Abort()
        {
            WasAbortSet = true;
            corpusReader.AbortReading = true;
        }

        public void SaveReport(string reportPath)
        {
            ReportProgress(99, "Saving report...", true);

            int unrecWordsCount = 0, totalWordsCount = 0, likelyErrors = 0;
            var utf8 = new UTF8Encoding(true, false).GetEncoder();

            // Naive serialization, to support planned extensions more easily
            using (var fs = new System.IO.FileStream(reportPath, System.IO.FileMode.Create))
            {
                var BOM = Encoding.UTF8.GetPreamble();
                fs.Write(BOM, 0, BOM.Length);

                var buf = StringToByteArray(@"<?xml version=""1.0""?><unrecognized>" + Environment.NewLine, utf8);
                fs.Write(buf, 0, buf.Length);

            	int wordsCount = radix.Count;

                // Get and sort all the unknown words
                CoverageData cd;
                var en = radix.GetEnumerator() as DictRadix<CoverageData>.RadixEnumerator;

				if (en == null) return;

                while (en.MoveNext() && !WasAbortSet)
                {
                    totalWordsCount++;

                    // A known word - only relevant if ComputeCoverage == true
                    cd = en.Current;
                    if (cd == null || cd.KnownToHSpell)
                        continue;

                    var w = en.CurrentKey;
                    unrecWordsCount++;

                    var bLikelyError = false;

                    if (!Regex.IsMatch(w, @"^[אבגדהוזחטיכלמנסעפצקרשת'""]+?[ףץךןם]??[']??$") // final letters should be used only at the end of word
                        || w.Length > 15 // too long a word
                        )
                    {
                        likelyErrors++;
                        bLikelyError = true;
                    }

                    buf = StringToByteArray(string.Format(@"<word text=""{0}"" location=""{1}"" occurs=""{2}"" {3}/>{4}"
                        , w, cd.FirstKnownLocation, cd.Count
                        , bLikelyError ? @"likelyerror=""true"" " : string.Empty, Environment.NewLine)
                        , utf8);
                    fs.Write(buf, 0, buf.Length);

                    ReportProgress(99, string.Format("Saving report... ({0} / {1})", totalWordsCount, wordsCount), true);
                }

                if (!WasAbortSet)
                {
                    buf = StringToByteArray(@"</unrecognized>" + Environment.NewLine, utf8);
                    fs.Write(buf, 0, buf.Length);

                    string stats = string.Format(@"<stats unknownWords=""{0}"" likelyErrors=""{1}"" ", unrecWordsCount, likelyErrors);
                    if (ComputeCoverage)
                    {
                        unrecWordsCount -= likelyErrors;
                        stats += string.Format(@"totalWords=""{0}"" coverageRate=""{1}%"" ", totalWordsCount,
                           (100 - (unrecWordsCount * 100 / totalWordsCount)));
                    }
                    buf = StringToByteArray(stats + " />" + Environment.NewLine, utf8);
                    fs.Write(buf, 0, buf.Length);
                }

                fs.Close();
            }
        }

        protected static byte[] StringToByteArray(string str, Encoder enc)
        {
            var ca = str.ToCharArray();
            var ret = new byte[enc.GetByteCount(ca, 0, ca.Length, false)];
            int charsUsed, bytesUsed;
            bool completed;
            enc.Convert(ca, 0, ca.Length, ret, 0, ret.Length, true, out charsUsed, out bytesUsed, out completed);
            return ret;
        }
    }
}
