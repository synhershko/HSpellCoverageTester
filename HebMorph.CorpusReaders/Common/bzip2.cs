using System;
using System.Runtime.InteropServices;

// This code was borrowed from the BzReader project: http://code.google.com/p/bzreader/

namespace HebMorph.CorpusReaders.Common
{
	public class bzip2
	{
		public enum StateCode : int
		{
			/// BZ_RUN -> 0
			BZ_RUN = 0,

			/// BZ_FLUSH -> 1
			BZ_FLUSH = 1,

			/// BZ_FINISH -> 2
			BZ_FINISH = 2
		}

		public enum StatusCode : int
		{
			/// BZ_OK -> 0
			BZ_OK = 0,

			/// BZ_RUN_OK -> 1
			BZ_RUN_OK = 1,

			/// BZ_FLUSH_OK -> 2
			BZ_FLUSH_OK = 2,

			/// BZ_FINISH_OK -> 3
			BZ_FINISH_OK = 3,

			/// BZ_STREAM_END -> 4
			BZ_STREAM_END = 4,

			/// BZ_SEQUENCE_ERROR -> (-1)
			BZ_SEQUENCE_ERROR = -1,

			/// BZ_PARAM_ERROR -> (-2)
			BZ_PARAM_ERROR = -2,

			/// BZ_MEM_ERROR -> (-3)
			BZ_MEM_ERROR = -3,

			/// BZ_DATA_ERROR -> (-4)
			BZ_DATA_ERROR = -4,

			/// BZ_DATA_ERROR_MAGIC -> (-5)
			BZ_DATA_ERROR_MAGIC = -5,

			/// BZ_IO_ERROR -> (-6)
			BZ_IO_ERROR = -6,

			/// BZ_UNEXPECTED_EOF -> (-7)
			BZ_UNEXPECTED_EOF = -7,

			/// BZ_OUTBUFF_FULL -> (-8)
			BZ_OUTBUFF_FULL = -8,

			/// BZ_CONFIG_ERROR -> (-9)
			BZ_CONFIG_ERROR = -9
		};

		/// Return Type: int
		///dest: char*
		///destLen: unsigned int*
		///source: char*
		///sourceLen: unsigned int
		///small: int
		///verbosity: int
		[DllImport("bzip2.dll")]
		private static extern StatusCode BZ2_bzBuffToBuffDecompress(IntPtr dest, ref uint destLen, IntPtr source, uint sourceLen, int small, int verbosity);

		/// Return Type: char*
		[DllImport("bzip2.dll")]
		private static extern IntPtr BZ2_bzlibVersion();

		[DllImport("bzip2.dll")]
		private static extern StatusCode BZ2_bzLocateBlocks(string path, IntPtr beginnings, IntPtr ends, ref long bufSize, ref int bz2_blocks_pct_done);

		[DllImport("bzip2.dll")]
		private static extern StatusCode BZ2_bzLoadBlock(string path, long beginning, long end, IntPtr buf, ref long bufSize);

		public static string BZ2_bzVersion()
		{
			IntPtr ptr = BZ2_bzlibVersion();

			return Marshal.PtrToStringAnsi(ptr);
		}

		public static StatusCode BZ2_bzDecompress(byte[] src, int len, byte[] buf, ref int bufSize)
		{
			uint _len = (uint)len;
			uint _bufSize = (uint)bufSize;

			StatusCode ret;

			GCHandle gch1 = GCHandle.Alloc(src, GCHandleType.Pinned);
			GCHandle gch2 = GCHandle.Alloc(buf, GCHandleType.Pinned);

			try
			{
				ret = BZ2_bzBuffToBuffDecompress(Marshal.UnsafeAddrOfPinnedArrayElement(buf, 0),
				                                 ref _bufSize,
				                                 Marshal.UnsafeAddrOfPinnedArrayElement(src, 0),
				                                 _len,
				                                 0,
				                                 0);

				bufSize = (int)_bufSize;
			}
			finally
			{
				gch1.Free();
				gch2.Free();
			}

			return ret;
		}

		public static StatusCode BZ2_bzLocateBlocks(string path, long[] beginnings, long[] ends, ref long bufSize, ref int bz2_blocks_pct_done)
		{
			StatusCode ret;

			GCHandle gch1 = GCHandle.Alloc(beginnings, GCHandleType.Pinned);
			GCHandle gch2 = GCHandle.Alloc(ends, GCHandleType.Pinned);

			try
			{
				ret = BZ2_bzLocateBlocks(path,
				                         Marshal.UnsafeAddrOfPinnedArrayElement(beginnings, 0),
				                         Marshal.UnsafeAddrOfPinnedArrayElement(ends, 0),
				                         ref bufSize, ref bz2_blocks_pct_done);
			}
			finally
			{
				gch1.Free();
				gch2.Free();
			}

			return ret;
		}

		public static StatusCode BZ2_bzLoadBlock(string path, long beginning, long end, byte[] buf, ref long bufSize)
		{
			StatusCode ret;

			GCHandle gch = GCHandle.Alloc(buf, GCHandleType.Pinned);

			try
			{
				ret = BZ2_bzLoadBlock(path,
				                      beginning,
				                      end,
				                      Marshal.UnsafeAddrOfPinnedArrayElement(buf, 0),
				                      ref bufSize);
			}
			finally
			{
				gch.Free();
			}

			return ret;
		}
	}
}
