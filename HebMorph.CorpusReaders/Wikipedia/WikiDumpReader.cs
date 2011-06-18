using System;
using System.Text;
using System.IO;
using System.Web;
using HebMorph.CorpusReaders.Common;

// This code was borrowed from the BzReader project: http://code.google.com/p/bzreader/

namespace HebMorph.CorpusReaders.Wikipedia
{
    public class WikiDumpReader : ICorpusReader
    {
        /// <summary>
        /// The maximum number of decoded blocks to keep in memory. A single block is roughly 1 M characters = 2 Mb
        /// </summary>
        private const int MAX_CACHED_BLOCKS_NO = 30;
        /// <summary>
        /// The maximum number of blocks we would be prepared to process in bz2 file.
        /// 200000 blocks = 160 Gb uncompressed, hope this is enough
        /// </summary>
        private const int MAX_BLOCKS_NO = 200000;
        /// <summary>
        /// The total number of blocks in the file
        /// </summary>
        private long totalBlocks = MAX_BLOCKS_NO;
        /// <summary>
        /// UTF8 decoder which would throw on unknown character
        /// </summary>
        private readonly Decoder utf8 = new UTF8Encoding(true, false).GetDecoder();
        /// <summary>
        /// The path to the dump file
        /// </summary>
        private readonly string filePath;
        /// <summary>
        /// The buffer for the beginnings of the blocks
        /// </summary>
        private readonly long[] beginnings = new long[MAX_BLOCKS_NO];
        /// <summary>
        /// The buffer for the ends of the blocks
        /// </summary>
        private readonly long[] ends = new long[MAX_BLOCKS_NO];
        /// <summary>
        /// The buffer for the current block
        /// </summary>
        private byte[] blockBuf;
        /// <summary>
        /// The character buffer for the current block
        /// </summary>
        private char[] charBuf;
        /// <summary>
        /// Percent done for bzip2 block location progress
        /// </summary>
        private int bz2_blocks_pct_done;
        /// <summary>
        /// Total bz2 file size, for progress calculation
        /// </summary>
        private long bz2_filesize;
        
        /// <summary>
        /// Class-level variables to do the allocation just once
        /// </summary>
        private byte[] decompressionBuf;

        /// <summary>
        /// Store the offsets of the previous block in case of the Wiki topic carryover
        /// </summary>
        private long previousBlockBeginning = -1;
        private long previousBlockEnd = -1;

    	public WikiDumpReader(string dumpFilePath)
        {
    		AbortReading = false;
        	filePath = dumpFilePath;
        }

    	/// <summary>
        /// Locates the bzip2 blocks in the file
        /// </summary>
        private void LocateBlocks()
        {
			ReportProgress(0, "Locating bz2 blocks...", true);
            var fi = new FileInfo(filePath);
            bz2_filesize = fi.Length;

            var status = bzip2.BZ2_bzLocateBlocks(filePath, beginnings, ends, ref totalBlocks, ref bz2_blocks_pct_done);

            if (status != bzip2.StatusCode.BZ_OK)
                throw new Exception(String.Format("Failed locating the blocks in {0}: {1}", filePath, status));

            if (totalBlocks < 1)
                throw new Exception(String.Format("No bz2 blocks were found in {0}", filePath));
        }

        /// <summary>
        /// Loads the bzip2 block from the file
        /// </summary>
        /// <param name="beginning">The beginning of the block, in bits.</param>
        /// <param name="end">The end of the block, in bits.</param>
        /// <param name="buf">The buffer to load into.</param>
        /// <returns>The length of the loaded data, in bytes</returns>
        private long LoadBlock(long beginning, long end, ref byte[] buf)
        {
            var bufSize = buf.LongLength;
            var status = bzip2.BZ2_bzLoadBlock(filePath, beginning, end, buf, ref bufSize);

            if (status != bzip2.StatusCode.BZ_OK)
                throw new Exception(String.Format("Failed loading {0} block starting at {1}: {2}", filePath, beginning, status));

            // Just some initial value, we will reallocate the buffer as needed
            if (decompressionBuf == null)
                decompressionBuf = new byte[buf.Length * 4];

            var intBufSize = (int)bufSize;
            var intDecompSize = decompressionBuf.Length;

            status = bzip2.BZ2_bzDecompress(buf, intBufSize, decompressionBuf, ref intDecompSize);

            // Limit a single uncompressed block size to 32 Mb
            while (status == bzip2.StatusCode.BZ_OUTBUFF_FULL &&
                decompressionBuf.Length < 32000000)
            {
                decompressionBuf = new byte[decompressionBuf.Length * 2];

                intDecompSize = decompressionBuf.Length;

                status = bzip2.BZ2_bzDecompress(buf, intBufSize, decompressionBuf, ref intDecompSize);
            }

            if (decompressionBuf.Length > 32000000)
                throw new OutOfMemoryException(String.Format("Failed uncompressing block starting at {0}: too much memory required", beginning));

            if (status != bzip2.StatusCode.BZ_OK)
                throw new Exception(String.Format("Failed uncompressing block starting at {0}: {1}", beginning, status));

            // Exchange the raw buffer and the uncompressed one

            byte[] exch = buf;

            buf = decompressionBuf;

            decompressionBuf = exch;

            return intDecompSize;
        }

        public void Read()
        {
            // Locate the bzip2 blocks in the file
            LocateBlocks();

            // Two times more than the first block but not less than 100 bytes
            var bufSize = ((ends[0] - beginnings[0]) / 8) * 2 + 100;

            // Buffers for the current and next block
            blockBuf = new byte[bufSize];
            charBuf = new char[bufSize];

            // Whether there was a Wiki topic carryover from current block to the next one
            var charCarryOver = new char[0];

            // The length of the currently loaded data

			ReportProgress(0, "Reading Wiki pages...", true);
            var sb = new StringBuilder();

            for (long currentBlock = 0; currentBlock < totalBlocks && !AbortReading; currentBlock++)
            {
				ReportProgress((byte)(currentBlock * 100 / (double)totalBlocks), null, true);

                var loadedLength = LoadBlock(beginnings[currentBlock], ends[currentBlock], ref blockBuf);

                if (charBuf.Length < blockBuf.Length)
                {
                    charBuf = new char[blockBuf.Length];
                }

                int bytesUsed = 0;
                int charsUsed = 0;
                bool completed = false;

                // Convert the text to UTF8
                utf8.Convert(blockBuf, 0, (int)loadedLength, charBuf, 0, charBuf.Length, currentBlock == totalBlocks - 1, out bytesUsed, out charsUsed, out completed);

                if (!completed)
                    throw new Exception("UTF8 decoder could not complete the conversion");

                // Construct a current string
                sb.Length = 0;

                if (charCarryOver.Length > 0)
                {
                    sb.Append(charCarryOver);
                }

                sb.Append(charBuf, 0, charsUsed);

                var carryOverLength = charCarryOver.Length;
                var charsMatched = ProcessBlock(sb.ToString(), beginnings[currentBlock], ends[currentBlock],
                     carryOverLength, currentBlock == totalBlocks - 1);

                // There's a Wiki topic carryover, let's store the characters which need to be carried over 
                if (charsMatched > 0)
                {
                    charCarryOver = new char[charsMatched];

                    sb.CopyTo(charsUsed + carryOverLength - charsMatched, charCarryOver, 0, charsMatched);
                }
                else
                {
                    charCarryOver = new char[0];
                }
            }
        }

        /// <summary>
        /// Indexes the provided string
        /// </summary>
        /// <param name="currentText">The string to index</param>
        /// <param name="beginning">The beginning offset of the block</param>
        /// <param name="end">The end offset of the block</param>
        /// <param name="charCarryOver">Whether there was a Wiki topic carryover from previous block</param>
        /// <param name="lastBlock">True if this is the last block</param>
        /// <returns>The number of characters in the end of the string that match the header entry</returns>
        private int ProcessBlock(string currentText, long beginning, long end, int charCarryOver, bool lastBlock)
        {
            var firstRun = true;
            var topicStart = currentText.IndexOf("<title>", StringComparison.InvariantCultureIgnoreCase);
			var title = String.Empty;

            int titleEnd, idStart, idEnd, topicEnd = -1;
            long id;
        	bool shouldBreak = false;

            while (topicStart >= 0 && !AbortReading)
            {
                titleEnd = -1;
                idStart = -1;
                idEnd = -1;
                topicEnd = -1;

                titleEnd = currentText.IndexOf("</title>", topicStart, StringComparison.InvariantCultureIgnoreCase);

                if (titleEnd < 0)
                    break;

                title = currentText.Substring(topicStart + "<title>".Length, titleEnd - topicStart - "<title>".Length);
            	title = System.Web.HttpUtility.HtmlDecode(title); // The title is stored HTML encoded

                idStart = currentText.IndexOf("<id>", titleEnd, StringComparison.InvariantCultureIgnoreCase);
                if (idStart < 0)
                    break;

                idEnd = currentText.IndexOf("</id>", idStart, StringComparison.InvariantCultureIgnoreCase);
                if (idEnd < 0)
                    break;

                id = Convert.ToInt64(currentText.Substring(idStart + "<id>".Length, idEnd - idStart - "<id>".Length));
                
                topicEnd = currentText.IndexOf("</text>", idEnd, StringComparison.InvariantCultureIgnoreCase);
                if (topicEnd < 0)
                    break;

                // Start creating the object for the tokenizing ThreadPool thread
                var begins = new long[1];
                var ends = new long[1];

                // Was there a carryover?
                if (firstRun)
                {
                    // Did the <title> happen in the carryover area?
                    if (charCarryOver > 0 &&
                        topicStart < charCarryOver)
                    {
                        if (previousBlockBeginning > -1 &&
                            previousBlockEnd > -1)
                        {
                            begins = new long[2];
                            ends = new long[2];

                            begins[1] = previousBlockBeginning;
                            ends[1] = previousBlockEnd;
                        }
                        else
                        {
                            throw new Exception("A Wiki topic title carryover occurred, but no previous block has been stored");
                        }
                    }
                }

                begins[0] = beginning;
                ends[0] = end;

            	var prevTopicStart = topicStart;
				// Store the last successful title start position
				var nextTopicStart = currentText.IndexOf("<title>", topicStart + 1, StringComparison.InvariantCultureIgnoreCase);
				if (nextTopicStart >= 0)
				{
					topicStart = nextTopicStart;
				}
				else
				{
					shouldBreak = true;
				}

				firstRun = false;

				// skip meta-pages - pages with irrelevant or no content
				if (title.StartsWith("תבנית:") || title.StartsWith("עזרה:") || title.StartsWith("ויקיפדיה:")
					|| title.StartsWith("קטגוריה:") || title.StartsWith("קובץ:") || title.StartsWith("פורטל:"))
				{
					if (shouldBreak) break;
					continue;
				}

            	var contents = currentText.Substring(prevTopicStart, topicEnd - prevTopicStart + 7/* == "</text>".Length */);
                contents = GetContentSection(contents, id, title);

				// For some weird reason, the Niqqud character Dagesh is not being used directly in he-wiki but
				// through the use of special markup
				var strippedContent = contents.Replace("{{דגש}}", "\u05BC");
                
                // Process document
            	var doc = new CorpusDocument {Id = id.ToString(), Title = title};
            	doc.SetContent(strippedContent, CorpusDocument.ContentFormat.WikiMarkup);
				if (OnDocument != null) OnDocument(doc);

				if (shouldBreak) break;
            }

            // Now calculate how many characters we need to save for next block
            var charsToSave = 0;
            if (topicStart == -1)
            {
                if (!lastBlock)
                {
                    throw new Exception("No topics were found in the block");
                }
            }
            else
            {
                if (!lastBlock)
                {
                    if (topicEnd == -1)
                    {
                        charsToSave = currentText.Length - topicStart;
                    }
                    else
                    {
                        if (topicStart < topicEnd)
                        {
                            charsToSave = currentText.Length - topicEnd - "</text>".Length;
                        }
                        else
                        {
                            charsToSave = currentText.Length - topicStart;
                        }
                    }
                }
            }

            previousBlockBeginning = beginning;
            previousBlockEnd = end;

            return charsToSave;
        }

        /// <summary>
        /// Loads a block from the file, decompresses it and decodes to string
        /// </summary>
        /// <param name="begin">The list of block beginnings</param>
        /// <param name="end">The list of block ends</param>
        /// <returns>The decoded string</returns>
        public string LoadAndDecodeBlock(long[] begin, long[] end)
        {
            byte[] currentBuf = null;

            utf8.Reset();
            var sb = new StringBuilder();

            for (var i = 0; i < begin.Length; i++)
            {
                if (blockBuf == null)
                {
                    blockBuf = new byte[((end[i] - begin[i]) / 8) * 2 + 100];
                }

                var loadedLen = LoadBlock(begin[i], end[i], ref blockBuf);

                currentBuf = new byte[loadedLen];

                Array.Copy(blockBuf, 0, currentBuf, 0, (int)loadedLen);

                if (charBuf == null ||
                    charBuf.Length < currentBuf.Length)
                {
                    charBuf = new char[currentBuf.Length];
                }

                int bytesUsed = 0;
                int charsUsed = 0;
                bool completed = false;

                utf8.Convert(currentBuf, 0, currentBuf.Length, charBuf, 0, charBuf.Length, i == begin.Length,
                    out bytesUsed, out charsUsed, out completed);

                sb.Append(charBuf, 0, charsUsed);
            }

            return sb.ToString();
        }

		/// <summary>
		/// Given a raw Wiki content, returns the actual content section
		/// </summary>
		/// <param name="rawContent"></param>
		/// <param name="topicId"></param>
		/// <param name="title"></param>
		/// <returns></returns>
		public static string GetContentSection(string rawContent, long topicId, string title)
		{
			var searchfor = String.Format("<id>{0}</id>", topicId);

			var pos = rawContent.IndexOf(searchfor, StringComparison.InvariantCultureIgnoreCase);
			if (pos < 0)
				throw new Exception(String.Format("Could not locate topic {0} in block", title));

			var textStart = rawContent.IndexOf("<text", pos, StringComparison.InvariantCultureIgnoreCase);
			if (textStart < 0)
				throw new Exception(String.Format("Could not locate text marker for topic {0} in block", title));

			var extractionStart = rawContent.IndexOf('>', textStart);
			if (extractionStart < 0)
				throw new Exception(String.Format("Could not locate text start for topic {0} in block", title));

			var extractionEnd = rawContent.IndexOf("</text>", extractionStart, StringComparison.InvariantCultureIgnoreCase);
			if (extractionEnd < 0)
				throw new Exception(String.Format("Could not locate text end for topic {0} in block", title));

			return HttpUtility.HtmlDecode(rawContent.Substring(extractionStart + 1, extractionEnd - extractionStart - 1));
		}

		protected void ReportProgress(byte progressPercentage, string status, bool isRunning)
		{
			if (OnProgress != null)
				OnProgress(progressPercentage, status, isRunning);
		}

    	#region ICorpusReader Members

    	public event ReportProgressDelegate OnProgress;
    	public event HitDocumentDelegate OnDocument;
    	public bool AbortReading { get; set; }

    	#endregion
    }
}
