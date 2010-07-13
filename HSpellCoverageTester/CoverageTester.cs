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
    public delegate void ReportProgressDelegate(int progressPercentage, string Status, bool isRunning);

    public class CoverageTester
    {
        public event ProgressChangedEventHandler ProgressChanged;
        public bool ComputeCoverage = true;

        private ICorpusReader corpusReader;
        private string HSpellPath = string.Empty;
        private HebMorph.StreamLemmatizer lemmatizer;
        private HebMorph.DataStructures.DictRadix<object> radix;

        public CoverageTester(ICorpusReader cr, string _hspellPath)
        {
            this.corpusReader = cr;
            this.HSpellPath = _hspellPath;
        }

        public void Run()
        {
        }

        public void Run(string reportPath)
        {
            radix = null;
            radix = new HebMorph.DataStructures.DictRadix<object>();

            ReportProgress(0, "Initializing hspell...", true);
            lemmatizer = new HebMorph.StreamLemmatizer(HSpellPath, true, false);
            lemmatizer.TolerateWhenLemmatizingStream = false;

            corpusReader.HitDocumentFunc = GotDocument;
            corpusReader.ProgressFunc = ReportProgress;
            corpusReader.AbortReading = false;
            corpusReader.Read();

            if (!corpusReader.AbortReading && !string.IsNullOrEmpty(reportPath))
            {
                ReportProgress(99, "Saving report...", true);
                SaveReport(reportPath);
            }

            ReportProgress(100, "Finalizing...", false);
        }

        protected void ReportProgress(int progressPercentage, string Status, bool isRunning)
        {
            ProgressInfo pi = new ProgressInfo();
            pi.Status = Status;
            pi.IsStillRunning = isRunning;
            ReportProgress(progressPercentage, pi);
        }

        protected void ReportProgress(int progressPercentage, ProgressInfo pi)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged(this, new ProgressChangedEventArgs(progressPercentage, pi));
            }
        }

        private void GotDocument(object doc, object docID)
        {
            if (doc == null)
                return;

            string docContents = doc.ToString();
            string word = string.Empty;
            List<HebMorph.Token> tokens = new List<HebMorph.Token>();

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
                    radix.AddNode(word, docID);
                    continue;
                }

                // Otherwise, the token is either in the dictionary already, or is not a Hebrew word. If we
                // are performing complete coverage computation, add it to the radix as well
                
                // A non-Hebrew word, or we are not performing a coverage calculation
                if (!ComputeCoverage || (tokens.Count == 1 && !(tokens[0] is HebMorph.HebrewToken)))
                    continue;

                // Hebrew words with one lemma or more - store the word in the radix with a flag
                // signaling it was indeed found
                radix.AddNode(word, (sbyte)-1);
            }
        }

        public void Abort()
        {
            corpusReader.AbortReading = true;
        }

        public void SaveReport(string reportPath)
        {
            int unrecWordsCount = 0, totalWordsCount = 0, likelyErrors = 0;
            Encoder utf8 = new UTF8Encoding(true, false).GetEncoder();

            // Get and sort all the unknown words
            RealSortedList<string> unrecognizedWords = new RealSortedList<string>(SortOrder.Asc);
            DictRadix<object>.RadixEnumerator en = radix.GetEnumerator() as DictRadix<object>.RadixEnumerator;
            while (en.MoveNext())
            {
                totalWordsCount++;

                // A known word
                if (ComputeCoverage && en.Current is sbyte && ((sbyte)en.Current) == -1)
                    continue;

                unrecognizedWords.AddUnique(en.CurrentKey);
                unrecWordsCount++;
            }

            // Naive serialization, to support planned extensions more easily
            using (System.IO.FileStream fs = new System.IO.FileStream(reportPath, System.IO.FileMode.Create))
            {
                byte[] BOM = Encoding.UTF8.GetPreamble();
                fs.Write(BOM, 0, BOM.Length);

                byte[] buf = StringToByteArray(@"<?xml version=""1.0""?><unrecognized>" + Environment.NewLine, utf8);
                fs.Write(buf, 0, buf.Length);

                bool bLikelyError;
                foreach (string w in unrecognizedWords)
                {
                    bLikelyError = false;

                    if (Regex.IsMatch(w, @"[\W]*?[ףץןם]+?[\W]+") // final letters should be used only at the end of word
                        || w.Length > 15 // too long a word
                        )
                    {
                        likelyErrors++;
                        bLikelyError = true;
                    }

                    object oData = radix.Lookup(w);
                    buf = StringToByteArray(string.Format(@"<word text=""{0}"" location=""{1}"" {2}/>{3}",
                        w, oData, bLikelyError ? @"likelyerror=""true"" " : string.Empty, Environment.NewLine)
                        , utf8);
                    fs.Write(buf, 0, buf.Length);
                }

                buf = StringToByteArray(@"</unrecognized>" + Environment.NewLine, utf8);
                fs.Write(buf, 0, buf.Length);

                string stats = string.Format(@"<stats unknownWords=""{0}"" likelyErrors=""{1}"" ", unrecWordsCount, likelyErrors);
                if (ComputeCoverage)
                    stats += string.Format(@"totalWords=""{0}"" coverageRate=""{1}%"" ", totalWordsCount,
                       (byte)(100 - (byte)(((unrecWordsCount - likelyErrors) / totalWordsCount) * 100)));
                buf = StringToByteArray(stats + " />" + Environment.NewLine, utf8);
                fs.Write(buf, 0, buf.Length);

                fs.Close();
            }
        }

        protected static byte[] StringToByteArray(string str, Encoder enc)
        {
            char[] ca = str.ToCharArray();
            byte[] ret = new byte[enc.GetByteCount(ca, 0, ca.Length, false)];
            int charsUsed, bytesUsed;
            bool completed;
            enc.Convert(ca, 0, ca.Length, ret, 0, ret.Length, true, out charsUsed, out bytesUsed, out completed);
            return ret;
        }
    }
}
