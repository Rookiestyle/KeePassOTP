//
//	QR Code Library
//
//	QR Code decoder.
//
//	Author: Uzi Granot
//	Version: 1.0
//	Date: June 30, 2018
//	Copyright (C) 2013-2018 Uzi Granot. All Rights Reserved
//
//	QR Code Library C# class library and the attached test/demo
//  applications are free software.
//	Software developed by this author is licensed under CPOL 1.02.
//	Some portions of the QRCodeVideoDecoder are licensed under GNU Lesser
//	General Public License v3.0.
//
//	The solution is made of 4 projects:
//	1. QRCodeEncoderDecoderLibrary: QR code encoding and decoding.
//	2. QRCodeEncoderDemo: Create QR Code images.
//	3. QRCodeDecoderDemo: Decode QR code image files.
//	4. QRCodeVideoDecoder: Decode QR code using web camera.
//		This demo program is using some of the source modules of
//		Camera_Net project published at CodeProject.com:
//		https://www.codeproject.com/Articles/671407/Camera_Net-Library
//		and at GitHub: https://github.com/free5lot/Camera_Net.
//		This project is based on DirectShowLib.
//		http://sourceforge.net/projects/directshownet/
//		This project includes a modified subset of the source modules.
//
//	The main points of CPOL 1.02 subject to the terms of the License are:
//
//	Source Code and Executable Files can be used in commercial applications;
//	Source Code and Executable Files can be redistributed; and
//	Source Code can be modified to create derivative works.
//	No claim of suitability, guarantee, or any warranty whatsoever is
//	provided. The software is provided "as-is".
//	The Article accompanying the Work may not be distributed or republished
//	without the Author's consent
//
//	For version history please refer to QRCode.cs
/////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Runtime.InteropServices;

namespace QRDecoder
{
	public class QRDecoder
	{
		public const string VersionNumber = "Rev 2.0.0 - 2019-05-15";

		/// <summary>
		/// Gets QR Code matrix version
		/// </summary>
		public int QRCodeVersion { get; internal set; }

		/// <summary>
		/// Gets QR Code matrix dimension in bits
		/// </summary>
		public int QRCodeDimension { get; internal set; }

		/// <summary>
		/// Gets QR Code error correction code (L, M, Q, H)
		/// </summary>
		public ErrorCorrection ErrorCorrection { get; internal set; }

		/// <summary>
		/// Error correction percent (L, M, Q, H)
		/// </summary>
		public int[] ErrCorrPercent = new int[] { 7, 15, 25, 30 };

		/// <summary>
		/// Get mask code (0 to 7)
		/// </summary>
		public int MaskCode { get; internal set; }

		internal int ImageWidth;
		internal int ImageHeight;
		internal bool[,] BlackWhiteImage;
		internal List<Finder> FinderList;
		internal List<Finder> AlignList;
		internal List<byte[]> DataArrayList;
		internal int MaxCodewords;
		internal int MaxDataCodewords;
		internal int MaxDataBits;
		internal int ErrCorrCodewords;
		internal int BlocksGroup1;
		internal int DataCodewordsGroup1;
		internal int BlocksGroup2;
		internal int DataCodewordsGroup2;

		internal byte[] CodewordsArray;
		internal int CodewordsPtr;
		internal uint BitBuffer;
		internal int BitBufferLen;
		internal byte[,] BaseMatrix;
		internal byte[,] MaskMatrix;

		internal bool Trans4Mode;

		// transformation cooefficients from QR modules to image pixels
		internal double Trans3a;
		internal double Trans3b;
		internal double Trans3c;
		internal double Trans3d;
		internal double Trans3e;
		internal double Trans3f;

		// transformation matrix based on three finders plus one more point
		internal double Trans4a;
		internal double Trans4b;
		internal double Trans4c;
		internal double Trans4d;
		internal double Trans4e;
		internal double Trans4f;
		internal double Trans4g;
		internal double Trans4h;

		internal const double SIGNATURE_MAX_DEVIATION = 0.25;
		internal const double HOR_VERT_SCAN_MAX_DISTANCE = 2.0;
		internal const double MODULE_SIZE_DEVIATION = 0.5; // 0.75;
		internal const double CORNER_SIDE_LENGTH_DEV = 0.8;
		internal const double CORNER_RIGHT_ANGLE_DEV = 0.25; // about Sin(4 deg)
		internal const double ALIGNMENT_SEARCH_AREA = 0.3;

		/*
		 public static string ByteArrayToStr(byte[] DataArray)
		{
			Decoder Decoder = Encoding.UTF8.GetDecoder();
			int CharCount = Decoder.GetCharCount(DataArray, 0, DataArray.Length);
			char[] CharArray = new char[CharCount];
			Decoder.GetChars(DataArray, 0, DataArray.Length, CharArray, 0);
			return new string(CharArray);
		}
		*/

		////////////////////////////////////////////////////////////////////
		// Decode QRCode boolean matrix
		////////////////////////////////////////////////////////////////////
		public byte[][] ImageDecoder(Bitmap InputImage)
		{
			try
			{
				// empty data string output
				DataArrayList = new List<byte[]>();

				// save image dimension
				ImageWidth = InputImage.Width;
				ImageHeight = InputImage.Height;

				// convert input image to black and white boolean image
				if (!ConvertImageToBlackAndWhite(InputImage)) return null;

				// horizontal search for finders
				if (!HorizontalFindersSearch()) return null;

				// vertical search for finders
				VerticalFindersSearch();

				// remove unused finders
				if (!RemoveUnusedFinders()) return null;
			}
			catch { return null; }

			// look for all possible 3 finder patterns
			int Index1End = FinderList.Count - 2;
			int Index2End = FinderList.Count - 1;
			int Index3End = FinderList.Count;
			for (int Index1 = 0; Index1 < Index1End; Index1++)
			{
				for (int Index2 = Index1 + 1; Index2 < Index2End; Index2++)
				{
					for (int Index3 = Index2 + 1; Index3 < Index3End; Index3++)
					{
						try
						{
							// find 3 finders arranged in L shape
							Corner Corner = Corner.CreateCorner(FinderList[Index1], FinderList[Index2], FinderList[Index3]);

							// not a valid corner
							if (Corner == null) continue;

							// get corner info (version, error code and mask)
							// continue if failed
							if (!GetQRCodeCornerInfo(Corner)) continue;

							// decode corner using three finders
							// continue if successful
							if (DecodeQRCodeCorner(Corner)) continue;

							// qr code version 1 has no alignment mark
							// in other words decode failed 
							if (QRCodeVersion == 1) continue;

							// find bottom right alignment mark
							// continue if failed
							if (!FindAlignmentMark(Corner)) continue;

							// decode using 4 points
							foreach (Finder Align in AlignList)
							{
								// calculate transformation based on 3 finders and bottom right alignment mark
								SetTransMatrix(Corner, Align.Row, Align.Col);

								// decode corner using three finders and one alignment mark
								if (DecodeQRCodeCorner(Corner)) break;
							}
						}
						catch { continue; }
					}
				}
			}

			// not found exit
			if (DataArrayList.Count == 0) return null;

			// successful exit
			return DataArrayList.ToArray();
		}

		////////////////////////////////////////////////////////////////////
		// Convert image to black and white boolean matrix
		////////////////////////////////////////////////////////////////////
		private bool ConvertImageToBlackAndWhite(Bitmap InputImage)
		{
			// lock image bits
			BitmapData BitmapData = InputImage.LockBits(new Rectangle(0, 0, ImageWidth, ImageHeight), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

			// address of first line
			IntPtr BitArrayPtr = BitmapData.Scan0;

			// length in bytes of one scan line
			int ScanLineWidth = BitmapData.Stride;
			if (ScanLineWidth < 0) return false;

			// image total bytes
			int TotalBytes = ScanLineWidth * ImageHeight;
			byte[] BitmapArray = new byte[TotalBytes];

			// Copy the RGB values into the array.
			Marshal.Copy(BitArrayPtr, BitmapArray, 0, TotalBytes);

			// unlock image
			InputImage.UnlockBits(BitmapData);

			// allocate gray image 
			byte[,] GrayImage = new byte[ImageHeight, ImageWidth];
			int[] GrayLevel = new int[256];

			// convert to gray
			int Delta = ScanLineWidth - 3 * ImageWidth;
			int BitmapPtr = 0;
			for (int Row = 0; Row < ImageHeight; Row++)
			{
				for (int Col = 0; Col < ImageWidth; Col++)
				{
					int Module = (30 * BitmapArray[BitmapPtr] + 59 * BitmapArray[BitmapPtr + 1] + 11 * BitmapArray[BitmapPtr + 2]) / 100;
					GrayLevel[Module]++;
					GrayImage[Row, Col] = (byte)Module;
					BitmapPtr += 3;
				}
				BitmapPtr += Delta;
			}

			// gray level cutoff between black and white
			int LevelStart;
			int LevelEnd;
			for (LevelStart = 0; LevelStart < 256 && GrayLevel[LevelStart] == 0; LevelStart++) ;
			for (LevelEnd = 255; LevelEnd >= LevelStart && GrayLevel[LevelEnd] == 0; LevelEnd--) ;
			LevelEnd++;
			if (LevelEnd - LevelStart < 2) return false;

			int CutoffLevel = (LevelStart + LevelEnd) / 2;

			// create boolean image white = false, black = true
			BlackWhiteImage = new bool[ImageHeight, ImageWidth];
			for (int Row = 0; Row < ImageHeight; Row++)
			{
				for (int Col = 0; Col < ImageWidth; Col++)
				{
					BlackWhiteImage[Row, Col] = GrayImage[Row, Col] < CutoffLevel;
				}
			}
			return true;
		}

		////////////////////////////////////////////////////////////////////
		// search row by row for finders blocks
		////////////////////////////////////////////////////////////////////
		internal bool HorizontalFindersSearch()
		{
			// create empty finders list
			FinderList = new List<Finder>();

			// look for finder patterns
			int[] ColPos = new int[ImageWidth + 1];
			int PosPtr = 0;

			// scan one row at a time
			for (int Row = 0; Row < ImageHeight; Row++)
			{
				// look for first black pixel
				int Col;
				for (Col = 0; Col < ImageWidth && !BlackWhiteImage[Row, Col]; Col++) ;
				if (Col == ImageWidth) continue;

				// first black
				PosPtr = 0;
				ColPos[PosPtr++] = Col;

				// loop for pairs
				for (; ; )
				{
					// look for next white
					// if black is all the way to the edge, set next white after the edge
					for (; Col < ImageWidth && BlackWhiteImage[Row, Col]; Col++) ;
					ColPos[PosPtr++] = Col;
					if (Col == ImageWidth) break;

					// look for next black
					for (; Col < ImageWidth && !BlackWhiteImage[Row, Col]; Col++) ;
					if (Col == ImageWidth) break;
					ColPos[PosPtr++] = Col;
				}

				// we must have at least 6 positions
				if (PosPtr < 6) continue;

				// build length array
				int PosLen = PosPtr - 1;
				int[] Len = new int[PosLen];
				for (int Ptr = 0; Ptr < PosLen; Ptr++) Len[Ptr] = ColPos[Ptr + 1] - ColPos[Ptr];

				// test signature
				int SigLen = PosPtr - 5;
				for (int SigPtr = 0; SigPtr < SigLen; SigPtr += 2)
				{
					double ModuleSize;
					if (TestFinderSig(ColPos, Len, SigPtr, out ModuleSize))
						FinderList.Add(new Finder(Row, ColPos[SigPtr + 2], ColPos[SigPtr + 3], ModuleSize));
				}
			}

			// no finders found
			if (FinderList.Count < 3) return false;

			return true;
		}

		////////////////////////////////////////////////////////////////////
		// search row by row for alignment blocks
		////////////////////////////////////////////////////////////////////

		internal bool HorizontalAlignmentSearch(int AreaLeft, int AreaTop, int AreaWidth, int AreaHeight)
		{
			// create empty finders list
			AlignList = new List<Finder>();

			// look for finder patterns
			int[] ColPos = new int[AreaWidth + 1];
			int PosPtr = 0;

			// area right and bottom
			int AreaRight = AreaLeft + AreaWidth;
			int AreaBottom = AreaTop + AreaHeight;

			// scan one row at a time
			for (int Row = AreaTop; Row < AreaBottom; Row++)
			{
				// look for first black pixel
				int Col;
				for (Col = AreaLeft; Col < AreaRight && !BlackWhiteImage[Row, Col]; Col++) ;
				if (Col == AreaRight) continue;

				// first black
				PosPtr = 0;
				ColPos[PosPtr++] = Col;

				// loop for pairs
				for (; ; )
				{
					// look for next white
					// if black is all the way to the edge, set next white after the edge
					for (; Col < AreaRight && BlackWhiteImage[Row, Col]; Col++) ;
					ColPos[PosPtr++] = Col;
					if (Col == AreaRight) break;

					// look for next black
					for (; Col < AreaRight && !BlackWhiteImage[Row, Col]; Col++) ;
					if (Col == AreaRight) break;
					ColPos[PosPtr++] = Col;
				}

				// we must have at least 6 positions
				if (PosPtr < 6) continue;

				// build length array
				int PosLen = PosPtr - 1;
				int[] Len = new int[PosLen];
				for (int Ptr = 0; Ptr < PosLen; Ptr++) Len[Ptr] = ColPos[Ptr + 1] - ColPos[Ptr];

				// test signature
				int SigLen = PosPtr - 5;
				for (int SigPtr = 0; SigPtr < SigLen; SigPtr += 2)
				{
					double ModuleSize;
					if (TestAlignSig(ColPos, Len, SigPtr, out ModuleSize))
						AlignList.Add(new Finder(Row, ColPos[SigPtr + 2], ColPos[SigPtr + 3], ModuleSize));
				}
			}

			// list is now empty or has less than three finders
			return AlignList.Count != 0;
		}

		////////////////////////////////////////////////////////////////////
		// search column by column for finders blocks
		////////////////////////////////////////////////////////////////////
		internal void VerticalFindersSearch()
		{
			// active columns
			bool[] ActiveColumn = new bool[ImageWidth];
			foreach (Finder HF in FinderList)
			{
				for (int Col = HF.Col1; Col < HF.Col2; Col++) ActiveColumn[Col] = true;
			}

			// look for finder patterns
			int[] RowPos = new int[ImageHeight + 1];
			int PosPtr = 0;

			// scan one column at a time
			for (int Col = 0; Col < ImageWidth; Col++)
			{
				// not active column
				if (!ActiveColumn[Col]) continue;

				// look for first black pixel
				int Row;
				for (Row = 0; Row < ImageHeight && !BlackWhiteImage[Row, Col]; Row++) ;
				if (Row == ImageWidth) continue;

				// first black
				PosPtr = 0;
				RowPos[PosPtr++] = Row;

				// loop for pairs
				for (; ; )
				{
					// look for next white
					// if black is all the way to the edge, set next white after the edge
					for (; Row < ImageHeight && BlackWhiteImage[Row, Col]; Row++) ;
					RowPos[PosPtr++] = Row;
					if (Row == ImageHeight) break;

					// look for next black
					for (; Row < ImageHeight && !BlackWhiteImage[Row, Col]; Row++) ;
					if (Row == ImageHeight) break;
					RowPos[PosPtr++] = Row;
				}

				// we must have at least 6 positions
				if (PosPtr < 6) continue;

				// build length array
				int PosLen = PosPtr - 1;
				int[] Len = new int[PosLen];
				for (int Ptr = 0; Ptr < PosLen; Ptr++) Len[Ptr] = RowPos[Ptr + 1] - RowPos[Ptr];

				// test signature
				int SigLen = PosPtr - 5;
				for (int SigPtr = 0; SigPtr < SigLen; SigPtr += 2)
				{
					double ModuleSize;
					if (!TestFinderSig(RowPos, Len, SigPtr, out ModuleSize)) continue;
					foreach (Finder HF in FinderList)
					{
						HF.Match(Col, RowPos[SigPtr + 2], RowPos[SigPtr + 3], ModuleSize);
					}
				}
			}
		}

		////////////////////////////////////////////////////////////////////
		// search column by column for finders blocks
		////////////////////////////////////////////////////////////////////
		internal void VerticalAlignmentSearch(int AreaLeft, int AreaTop, int AreaWidth, int AreaHeight)
		{
			// active columns
			bool[] ActiveColumn = new bool[AreaWidth];
			foreach (Finder HF in AlignList)
			{
				for (int Col = HF.Col1; Col < HF.Col2; Col++) ActiveColumn[Col - AreaLeft] = true;
			}

			// look for finder patterns
			int[] RowPos = new int[AreaHeight + 1];
			int PosPtr = 0;

			// area right and bottom
			int AreaRight = AreaLeft + AreaWidth;
			int AreaBottom = AreaTop + AreaHeight;

			// scan one column at a time
			for (int Col = AreaLeft; Col < AreaRight; Col++)
			{
				// not active column
				if (!ActiveColumn[Col - AreaLeft]) continue;

				// look for first black pixel
				int Row;
				for (Row = AreaTop; Row < AreaBottom && !BlackWhiteImage[Row, Col]; Row++) ;
				if (Row == AreaBottom) continue;

				// first black
				PosPtr = 0;
				RowPos[PosPtr++] = Row;

				// loop for pairs
				for (; ; )
				{
					// look for next white
					// if black is all the way to the edge, set next white after the edge
					for (; Row < AreaBottom && BlackWhiteImage[Row, Col]; Row++) ;
					RowPos[PosPtr++] = Row;
					if (Row == AreaBottom) break;

					// look for next black
					for (; Row < AreaBottom && !BlackWhiteImage[Row, Col]; Row++) ;
					if (Row == AreaBottom) break;
					RowPos[PosPtr++] = Row;
				}

				// we must have at least 6 positions
				if (PosPtr < 6) continue;

				// build length array
				int PosLen = PosPtr - 1;
				int[] Len = new int[PosLen];
				for (int Ptr = 0; Ptr < PosLen; Ptr++) Len[Ptr] = RowPos[Ptr + 1] - RowPos[Ptr];

				// test signature
				int SigLen = PosPtr - 5;
				for (int SigPtr = 0; SigPtr < SigLen; SigPtr += 2)
				{
					double ModuleSize;
					if (!TestAlignSig(RowPos, Len, SigPtr, out ModuleSize)) continue;
					foreach (Finder HF in AlignList)
					{
						HF.Match(Col, RowPos[SigPtr + 2], RowPos[SigPtr + 3], ModuleSize);
					}
				}
			}
		}

		////////////////////////////////////////////////////////////////////
		// search column by column for finders blocks
		////////////////////////////////////////////////////////////////////
		internal bool RemoveUnusedFinders()
		{
			// remove all entries without a match
			for (int Index = 0; Index < FinderList.Count; Index++)
			{
				if (FinderList[Index].Distance == double.MaxValue)
				{
					FinderList.RemoveAt(Index);
					Index--;
				}
			}

			// list is now empty or has less than three finders
			if (FinderList.Count < 3) return false;

			// keep best entry for each overlapping area
			for (int Index = 0; Index < FinderList.Count; Index++)
			{
				Finder Finder = FinderList[Index];
				for (int Index1 = Index + 1; Index1 < FinderList.Count; Index1++)
				{
					Finder Finder1 = FinderList[Index1];
					if (!Finder.Overlap(Finder1)) continue;
					if (Finder1.Distance < Finder.Distance)
					{
						Finder = Finder1;
						FinderList[Index] = Finder;
					}
					FinderList.RemoveAt(Index1);
					Index1--;
				}
			}

			// list is now empty or has less than three finders
			if (FinderList.Count < 3) return false;

			return true;
		}

		////////////////////////////////////////////////////////////////////
		// search column by column for finders blocks
		////////////////////////////////////////////////////////////////////
		internal bool RemoveUnusedAlignMarks()
		{
			// remove all entries without a match
			for (int Index = 0; Index < AlignList.Count; Index++)
			{
				if (AlignList[Index].Distance == double.MaxValue)
				{
					AlignList.RemoveAt(Index);
					Index--;
				}
			}

			// keep best entry for each overlapping area
			for (int Index = 0; Index < AlignList.Count; Index++)
			{
				Finder Finder = AlignList[Index];
				for (int Index1 = Index + 1; Index1 < AlignList.Count; Index1++)
				{
					Finder Finder1 = AlignList[Index1];
					if (!Finder.Overlap(Finder1)) continue;
					if (Finder1.Distance < Finder.Distance)
					{
						Finder = Finder1;
						AlignList[Index] = Finder;
					}
					AlignList.RemoveAt(Index1);
					Index1--;
				}
			}

			// list is now empty or has less than three finders
			return AlignList.Count != 0;
		}

		////////////////////////////////////////////////////////////////////
		// test finder signature 1 1 3 1 1
		////////////////////////////////////////////////////////////////////
		internal bool TestFinderSig(int[] Pos, int[] Len, int Index, out double Module)
		{
			Module = (Pos[Index + 5] - Pos[Index]) / 7.0;
			double MaxDev = SIGNATURE_MAX_DEVIATION * Module;
			if (Math.Abs(Len[Index] - Module) > MaxDev) return false;
			if (Math.Abs(Len[Index + 1] - Module) > MaxDev) return false;
			if (Math.Abs(Len[Index + 2] - 3 * Module) > MaxDev) return false;
			if (Math.Abs(Len[Index + 3] - Module) > MaxDev) return false;
			if (Math.Abs(Len[Index + 4] - Module) > MaxDev) return false;
			return true;
		}

		////////////////////////////////////////////////////////////////////
		// test alignment signature n 1 1 1 n
		////////////////////////////////////////////////////////////////////

		internal bool TestAlignSig(int[] Pos, int[] Len, int Index, out double Module)
		{
			Module = (Pos[Index + 4] - Pos[Index + 1]) / 3.0;
			double MaxDev = SIGNATURE_MAX_DEVIATION * Module;
			if (Len[Index] < Module - MaxDev) return false;
			if (Math.Abs(Len[Index + 1] - Module) > MaxDev) return false;
			if (Math.Abs(Len[Index + 2] - Module) > MaxDev) return false;
			if (Math.Abs(Len[Index + 3] - Module) > MaxDev) return false;
			if (Len[Index + 4] < Module - MaxDev) return false;
			return true;
		}

		////////////////////////////////////////////////////////////////////
		// Build corner list
		////////////////////////////////////////////////////////////////////
		internal List<Corner> BuildCornerList()
		{
			// empty list
			List<Corner> Corners = new List<Corner>();

			// look for all possible 3 finder patterns
			int Index1End = FinderList.Count - 2;
			int Index2End = FinderList.Count - 1;
			int Index3End = FinderList.Count;
			for (int Index1 = 0; Index1 < Index1End; Index1++)
			{
				for (int Index2 = Index1 + 1; Index2 < Index2End; Index2++)
				{
					for (int Index3 = Index2 + 1; Index3 < Index3End; Index3++)
					{
						// find 3 finders arranged in L shape
						Corner Corner = Corner.CreateCorner(FinderList[Index1], FinderList[Index2], FinderList[Index3]);

						// add corner to list
						if (Corner != null) Corners.Add(Corner);
					}
				}
			}
			return Corners.Count == 0 ? null : Corners;
		}

		////////////////////////////////////////////////////////////////////
		// Get QR Code corner info
		////////////////////////////////////////////////////////////////////
		internal bool GetQRCodeCornerInfo(Corner Corner)
		{
			try
			{
				// initial version number
				QRCodeVersion = Corner.InitialVersionNumber();

				// qr code dimension
				QRCodeDimension = 17 + 4 * QRCodeVersion;

				// set transformation matrix
				SetTransMatrix(Corner);

				// if version number is 7 or more, get version code
				if (QRCodeVersion >= 7)
				{
					int Version = GetVersionOne();
					if (Version == 0)
					{
						Version = GetVersionTwo();
						if (Version == 0) return false;
					}

					// QR Code version number is different than initial version
					if (Version != QRCodeVersion)
					{
						// initial version number and dimension
						QRCodeVersion = Version;

						// qr code dimension
						QRCodeDimension = 17 + 4 * QRCodeVersion;

						// set transformation matrix
						SetTransMatrix(Corner);
					}
				}

				// get format info arrays
				int FormatInfo = GetFormatInfoOne();
				if (FormatInfo < 0)
				{
					FormatInfo = GetFormatInfoTwo();
					if (FormatInfo < 0) return false;
				}

				// set error correction code and mask code
				ErrorCorrection = FormatInfoToErrCode(FormatInfo >> 3);
				MaskCode = FormatInfo & 7;

				// successful exit
				return true;
			}
			catch { return false; }
		}

		////////////////////////////////////////////////////////////////////
		// Search for QR Code version
		////////////////////////////////////////////////////////////////////
		internal bool DecodeQRCodeCorner(Corner Corner)
		{
			try
			{
				// create base matrix
				BuildBaseMatrix();

				// create data matrix and test fixed modules
				ConvertImageToMatrix();

				// based on version and format information
				// set number of data and error correction codewords length  
				SetDataCodewordsLength();

				// apply mask as per get format information step
				ApplyMask(MaskCode);

				// unload data from binary matrix to byte format
				UnloadDataFromMatrix();

				// restore blocks (undo interleave)
				RestoreBlocks();

				// calculate error correction
				// in case of error try to correct it
				CalculateErrorCorrection();

				// decode data
				byte[] DataArray = DecodeData();
				DataArrayList.Add(DataArray);


				// successful exit
				return true;
			}
			catch { return false; }
		}

		internal void SetTransMatrix(Corner Corner)
		{
			// save
			int BottomRightPos = QRCodeDimension - 4;

			// transformation matrix based on three finders
			double[,] Matrix1 = new double[3, 4];
			double[,] Matrix2 = new double[3, 4];

			// build matrix 1 for horizontal X direction
			Matrix1[0, 0] = 3;
			Matrix1[0, 1] = 3;
			Matrix1[0, 2] = 1;
			Matrix1[0, 3] = Corner.TopLeftFinder.Col;

			Matrix1[1, 0] = BottomRightPos;
			Matrix1[1, 1] = 3;
			Matrix1[1, 2] = 1;
			Matrix1[1, 3] = Corner.TopRightFinder.Col;

			Matrix1[2, 0] = 3;
			Matrix1[2, 1] = BottomRightPos;
			Matrix1[2, 2] = 1;
			Matrix1[2, 3] = Corner.BottomLeftFinder.Col;

			// build matrix 2 for Vertical Y direction
			Matrix2[0, 0] = 3;
			Matrix2[0, 1] = 3;
			Matrix2[0, 2] = 1;
			Matrix2[0, 3] = Corner.TopLeftFinder.Row;

			Matrix2[1, 0] = BottomRightPos;
			Matrix2[1, 1] = 3;
			Matrix2[1, 2] = 1;
			Matrix2[1, 3] = Corner.TopRightFinder.Row;

			Matrix2[2, 0] = 3;
			Matrix2[2, 1] = BottomRightPos;
			Matrix2[2, 2] = 1;
			Matrix2[2, 3] = Corner.BottomLeftFinder.Row;

			// solve matrix1
			SolveMatrixOne(Matrix1);
			Trans3a = Matrix1[0, 3];
			Trans3c = Matrix1[1, 3];
			Trans3e = Matrix1[2, 3];

			// solve matrix2
			SolveMatrixOne(Matrix2);
			Trans3b = Matrix2[0, 3];
			Trans3d = Matrix2[1, 3];
			Trans3f = Matrix2[2, 3];

			// reset trans 4 mode
			Trans4Mode = false;
		}

		internal void SolveMatrixOne(double[,] Matrix)
		{
			for (int Row = 0; Row < 3; Row++)
			{
				// If the element is zero, make it non zero by adding another row
				if (Matrix[Row, Row] == 0)
				{
					int Row1;
					for (Row1 = Row + 1; Row1 < 3 && Matrix[Row1, Row] == 0; Row1++) ;
					if (Row1 == 3) throw new ApplicationException("Solve linear equations failed");

					for (int Col = Row; Col < 4; Col++) Matrix[Row, Col] += Matrix[Row1, Col];
				}

				// make the diagonal element 1.0
				for (int Col = 3; Col > Row; Col--) Matrix[Row, Col] /= Matrix[Row, Row];

				// subtract current row from next rows to eliminate one value
				for (int Row1 = Row + 1; Row1 < 3; Row1++)
				{
					for (int Col = 3; Col > Row; Col--) Matrix[Row1, Col] -= Matrix[Row, Col] * Matrix[Row1, Row];
				}
			}

			// go up from last row and eliminate all solved values
			Matrix[1, 3] -= Matrix[1, 2] * Matrix[2, 3];
			Matrix[0, 3] -= Matrix[0, 2] * Matrix[2, 3];
			Matrix[0, 3] -= Matrix[0, 1] * Matrix[1, 3];
		}

		////////////////////////////////////////////////////////////////////
		// Get image pixel color
		////////////////////////////////////////////////////////////////////

		internal bool GetModule(int Row, int Col)
		{
			// get module based on three finders
			if (!Trans4Mode)
			{
				int Trans3Col = (int)Math.Round(Trans3a * Col + Trans3c * Row + Trans3e, 0, MidpointRounding.AwayFromZero);
				int Trans3Row = (int)Math.Round(Trans3b * Col + Trans3d * Row + Trans3f, 0, MidpointRounding.AwayFromZero);
				return BlackWhiteImage[Trans3Row, Trans3Col];
			}

			// get module based on three finders plus one alignment mark
			double W = Trans4g * Col + Trans4h * Row + 1.0;
			int Trans4Col = (int)Math.Round((Trans4a * Col + Trans4b * Row + Trans4c) / W, 0, MidpointRounding.AwayFromZero);
			int Trans4Row = (int)Math.Round((Trans4d * Col + Trans4e * Row + Trans4f) / W, 0, MidpointRounding.AwayFromZero);
			return BlackWhiteImage[Trans4Row, Trans4Col];
		}

		////////////////////////////////////////////////////////////////////
		// search row by row for finders blocks
		////////////////////////////////////////////////////////////////////

		internal bool FindAlignmentMark(Corner Corner)
		{
			// alignment mark estimated position
			int AlignRow = QRCodeDimension - 7;
			int AlignCol = QRCodeDimension - 7;
			int ImageCol = (int)Math.Round(Trans3a * AlignCol + Trans3c * AlignRow + Trans3e, 0, MidpointRounding.AwayFromZero);
			int ImageRow = (int)Math.Round(Trans3b * AlignCol + Trans3d * AlignRow + Trans3f, 0, MidpointRounding.AwayFromZero);


			// search area
			int Side = (int)Math.Round(ALIGNMENT_SEARCH_AREA * (Corner.TopLineLength + Corner.LeftLineLength), 0, MidpointRounding.AwayFromZero);

			int AreaLeft = ImageCol - Side / 2;
			int AreaTop = ImageRow - Side / 2;
			int AreaWidth = Side;
			int AreaHeight = Side;


			// horizontal search for finders
			if (!HorizontalAlignmentSearch(AreaLeft, AreaTop, AreaWidth, AreaHeight)) return false;

			// vertical search for finders
			VerticalAlignmentSearch(AreaLeft, AreaTop, AreaWidth, AreaHeight);

			// remove unused alignment entries
			if (!RemoveUnusedAlignMarks()) return false;

			// successful exit
			return true;
		}

		internal void SetTransMatrix(Corner Corner, double ImageAlignRow, double ImageAlignCol)
		{
			// top right and bottom left QR code position
			int FarFinder = QRCodeDimension - 4;
			int FarAlign = QRCodeDimension - 7;

			double[,] Matrix = new double[8, 9];

			Matrix[0, 0] = 3.0;
			Matrix[0, 1] = 3.0;
			Matrix[0, 2] = 1.0;
			Matrix[0, 6] = -3.0 * Corner.TopLeftFinder.Col;
			Matrix[0, 7] = -3.0 * Corner.TopLeftFinder.Col;
			Matrix[0, 8] = Corner.TopLeftFinder.Col;

			Matrix[1, 0] = FarFinder;
			Matrix[1, 1] = 3.0;
			Matrix[1, 2] = 1.0;
			Matrix[1, 6] = -FarFinder * Corner.TopRightFinder.Col;
			Matrix[1, 7] = -3.0 * Corner.TopRightFinder.Col;
			Matrix[1, 8] = Corner.TopRightFinder.Col;

			Matrix[2, 0] = 3.0;
			Matrix[2, 1] = FarFinder;
			Matrix[2, 2] = 1.0;
			Matrix[2, 6] = -3.0 * Corner.BottomLeftFinder.Col;
			Matrix[2, 7] = -FarFinder * Corner.BottomLeftFinder.Col;
			Matrix[2, 8] = Corner.BottomLeftFinder.Col;

			Matrix[3, 0] = FarAlign;
			Matrix[3, 1] = FarAlign;
			Matrix[3, 2] = 1.0;
			Matrix[3, 6] = -FarAlign * ImageAlignCol;
			Matrix[3, 7] = -FarAlign * ImageAlignCol;
			Matrix[3, 8] = ImageAlignCol;

			Matrix[4, 3] = 3.0;
			Matrix[4, 4] = 3.0;
			Matrix[4, 5] = 1.0;
			Matrix[4, 6] = -3.0 * Corner.TopLeftFinder.Row;
			Matrix[4, 7] = -3.0 * Corner.TopLeftFinder.Row;
			Matrix[4, 8] = Corner.TopLeftFinder.Row;

			Matrix[5, 3] = FarFinder;
			Matrix[5, 4] = 3.0;
			Matrix[5, 5] = 1.0;
			Matrix[5, 6] = -FarFinder * Corner.TopRightFinder.Row;
			Matrix[5, 7] = -3.0 * Corner.TopRightFinder.Row;
			Matrix[5, 8] = Corner.TopRightFinder.Row;

			Matrix[6, 3] = 3.0;
			Matrix[6, 4] = FarFinder;
			Matrix[6, 5] = 1.0;
			Matrix[6, 6] = -3.0 * Corner.BottomLeftFinder.Row;
			Matrix[6, 7] = -FarFinder * Corner.BottomLeftFinder.Row;
			Matrix[6, 8] = Corner.BottomLeftFinder.Row;

			Matrix[7, 3] = FarAlign;
			Matrix[7, 4] = FarAlign;
			Matrix[7, 5] = 1.0;
			Matrix[7, 6] = -FarAlign * ImageAlignRow;
			Matrix[7, 7] = -FarAlign * ImageAlignRow;
			Matrix[7, 8] = ImageAlignRow;

			for (int Row = 0; Row < 8; Row++)
			{
				// If the element is zero, make it non zero by adding another row
				if (Matrix[Row, Row] == 0)
				{
					int Row1;
					for (Row1 = Row + 1; Row1 < 8 && Matrix[Row1, Row] == 0; Row1++) ;
					if (Row1 == 8) throw new ApplicationException("Solve linear equations failed");

					for (int Col = Row; Col < 9; Col++) Matrix[Row, Col] += Matrix[Row1, Col];
				}

				// make the diagonal element 1.0
				for (int Col = 8; Col > Row; Col--) Matrix[Row, Col] /= Matrix[Row, Row];

				// subtract current row from next rows to eliminate one value
				for (int Row1 = Row + 1; Row1 < 8; Row1++)
				{
					for (int Col = 8; Col > Row; Col--) Matrix[Row1, Col] -= Matrix[Row, Col] * Matrix[Row1, Row];
				}
			}

			// go up from last row and eliminate all solved values
			for (int Col = 7; Col > 0; Col--) for (int Row = Col - 1; Row >= 0; Row--)
				{
					Matrix[Row, 8] -= Matrix[Row, Col] * Matrix[Col, 8];
				}

			Trans4a = Matrix[0, 8];
			Trans4b = Matrix[1, 8];
			Trans4c = Matrix[2, 8];
			Trans4d = Matrix[3, 8];
			Trans4e = Matrix[4, 8];
			Trans4f = Matrix[5, 8];
			Trans4g = Matrix[6, 8];
			Trans4h = Matrix[7, 8];

			// set trans 4 mode
			Trans4Mode = true;
		}

		////////////////////////////////////////////////////////////////////
		// Get version code bits top right
		////////////////////////////////////////////////////////////////////

		internal int GetVersionOne()
		{
			int VersionCode = 0;
			for (int Index = 0; Index < 18; Index++)
			{
				if (GetModule(Index / 3, QRCodeDimension - 11 + (Index % 3))) VersionCode |= 1 << Index;
			}
			return TestVersionCode(VersionCode);
		}

		////////////////////////////////////////////////////////////////////
		// Get version code bits bottom left
		////////////////////////////////////////////////////////////////////

		internal int GetVersionTwo()
		{
			int VersionCode = 0;
			for (int Index = 0; Index < 18; Index++)
			{
				if (GetModule(QRCodeDimension - 11 + (Index % 3), Index / 3)) VersionCode |= 1 << Index;
			}
			return TestVersionCode(VersionCode);
		}

		////////////////////////////////////////////////////////////////////
		// Test version code bits
		////////////////////////////////////////////////////////////////////

		internal int TestVersionCode(int VersionCode)
		{
			// format info
			int Code = VersionCode >> 12;

			// test for exact match
			if (Code >= 7 && Code <= 40 && StaticTables.VersionCodeArray[Code - 7] == VersionCode)
			{
				return Code;
			}

			// look for a match
			int BestInfo = 0;
			int Error = int.MaxValue;
			for (int Index = 0; Index < 34; Index++)
			{
				// test for exact match
				int ErrorBits = StaticTables.VersionCodeArray[Index] ^ VersionCode;
				if (ErrorBits == 0) return VersionCode >> 12;

				// count errors
				int ErrorCount = CountBits(ErrorBits);

				// save best result
				if (ErrorCount < Error)
				{
					Error = ErrorCount;
					BestInfo = Index;
				}
			}

			return Error <= 3 ? BestInfo + 7 : 0;
		}

		////////////////////////////////////////////////////////////////////
		// Get format info around top left corner
		////////////////////////////////////////////////////////////////////

		public int GetFormatInfoOne()
		{
			int Info = 0;
			for (int Index = 0; Index < 15; Index++)
			{
				if (GetModule(StaticTables.FormatInfoOne[Index, 0], StaticTables.FormatInfoOne[Index, 1])) Info |= 1 << Index;
			}
			return TestFormatInfo(Info);
		}

		////////////////////////////////////////////////////////////////////
		// Get format info around top right and bottom left corners
		////////////////////////////////////////////////////////////////////

		internal int GetFormatInfoTwo()
		{
			int Info = 0;
			for (int Index = 0; Index < 15; Index++)
			{
				int Row = StaticTables.FormatInfoTwo[Index, 0];
				if (Row < 0) Row += QRCodeDimension;
				int Col = StaticTables.FormatInfoTwo[Index, 1];
				if (Col < 0) Col += QRCodeDimension;
				if (GetModule(Row, Col)) Info |= 1 << Index;
			}
			return TestFormatInfo(Info);
		}

		////////////////////////////////////////////////////////////////////
		// Test format info bits
		////////////////////////////////////////////////////////////////////

		internal int TestFormatInfo(int FormatInfo)
		{
			// format info
			int Info = (FormatInfo ^ 0x5412) >> 10;

			// test for exact match
			if (StaticTables.FormatInfoArray[Info] == FormatInfo) return Info;

			// look for a match
			int BestInfo = 0;
			int Error = int.MaxValue;
			for (int Index = 0; Index < 32; Index++)
			{
				int ErrorCount = CountBits(StaticTables.FormatInfoArray[Index] ^ FormatInfo);
				if (ErrorCount < Error)
				{
					Error = ErrorCount;
					BestInfo = Index;
				}
			}

			return Error <= 3 ? BestInfo : -1;
		}

		////////////////////////////////////////////////////////////////////
		// Count Bits
		////////////////////////////////////////////////////////////////////

		internal int CountBits(int Value)
		{
			int Count = 0;
			for (int Mask = 0x4000; Mask != 0; Mask >>= 1) if ((Value & Mask) != 0) Count++;
			return Count;
		}

		////////////////////////////////////////////////////////////////////
		// Convert image to qr code matrix and test fixed modules
		////////////////////////////////////////////////////////////////////

		internal void ConvertImageToMatrix()
		{
			// loop for all modules
			int FixedCount = 0;
			int ErrorCount = 0;
			for (int Row = 0; Row < QRCodeDimension; Row++) for (int Col = 0; Col < QRCodeDimension; Col++)
				{
					// the module (Row, Col) is not a fixed module 
					if ((BaseMatrix[Row, Col] & StaticTables.Fixed) == 0)
					{
						if (GetModule(Row, Col)) BaseMatrix[Row, Col] |= StaticTables.Black;
					}

					// fixed module
					else
					{
						// total fixed modules
						FixedCount++;

						// test for error
						if ((GetModule(Row, Col) ? StaticTables.Black : StaticTables.White) != (BaseMatrix[Row, Col] & 1)) ErrorCount++;
					}
				}

			if (ErrorCount > FixedCount * ErrCorrPercent[(int)ErrorCorrection] / 100) throw new ApplicationException("Fixed modules error");
			return;
		}

		////////////////////////////////////////////////////////////////////
		// Unload matrix data from base matrix
		////////////////////////////////////////////////////////////////////
		internal void UnloadDataFromMatrix()
		{
			// input array pointer initialization
			int Ptr = 0;
			int PtrEnd = 8 * MaxCodewords;
			CodewordsArray = new byte[MaxCodewords];

			// bottom right corner of output matrix
			int Row = QRCodeDimension - 1;
			int Col = QRCodeDimension - 1;

			// step state
			int State = 0;
			for (; ; )
			{
				// current module is data
				if ((MaskMatrix[Row, Col] & StaticTables.NonData) == 0)
				{
					// unload current module with
					if ((MaskMatrix[Row, Col] & 1) != 0) CodewordsArray[Ptr >> 3] |= (byte)(1 << (7 - (Ptr & 7)));
					if (++Ptr == PtrEnd) break;
				}

				// current module is non data and vertical timing line condition is on
				else if (Col == 6) Col--;

				// update matrix position to next module
				switch (State)
				{
					// going up: step one to the left
					case 0:
						Col--;
						State = 1;
						continue;

					// going up: step one row up and one column to the right
					case 1:
						Col++;
						Row--;
						// we are not at the top, go to state 0
						if (Row >= 0)
						{
							State = 0;
							continue;
						}
						// we are at the top, step two columns to the left and start going down
						Col -= 2;
						Row = 0;
						State = 2;
						continue;

					// going down: step one to the left
					case 2:
						Col--;
						State = 3;
						continue;

					// going down: step one row down and one column to the right
					case 3:
						Col++;
						Row++;
						// we are not at the bottom, go to state 2
						if (Row < QRCodeDimension)
						{
							State = 2;
							continue;
						}
						// we are at the bottom, step two columns to the left and start going up
						Col -= 2;
						Row = QRCodeDimension - 1;
						State = 0;
						continue;
				}
			}
		}

		////////////////////////////////////////////////////////////////////
		// Restore interleave data and error correction blocks
		////////////////////////////////////////////////////////////////////

		internal void RestoreBlocks()
		{
			// allocate temp codewords array
			byte[] TempArray = new byte[MaxCodewords];

			// total blocks
			int TotalBlocks = BlocksGroup1 + BlocksGroup2;

			// create array of data blocks starting point
			int[] Start = new int[TotalBlocks];
			for (int Index = 1; Index < TotalBlocks; Index++) Start[Index] = Start[Index - 1] + (Index <= BlocksGroup1 ? DataCodewordsGroup1 : DataCodewordsGroup2);

			// step one. iterleave base on group one length
			int PtrEnd = DataCodewordsGroup1 * TotalBlocks;

			// restore group one and two
			int Ptr;
			int Block = 0;
			for (Ptr = 0; Ptr < PtrEnd; Ptr++)
			{
				TempArray[Start[Block]] = CodewordsArray[Ptr];
				Start[Block]++;
				Block++;
				if (Block == TotalBlocks) Block = 0;
			}

			// restore group two
			if (DataCodewordsGroup2 > DataCodewordsGroup1)
			{
				// step one. iterleave base on group one length
				PtrEnd = MaxDataCodewords;

				Block = BlocksGroup1;
				for (; Ptr < PtrEnd; Ptr++)
				{
					TempArray[Start[Block]] = CodewordsArray[Ptr];
					Start[Block]++;
					Block++;
					if (Block == TotalBlocks) Block = BlocksGroup1;
				}
			}

			// create array of error correction blocks starting point
			Start[0] = MaxDataCodewords;
			for (int Index = 1; Index < TotalBlocks; Index++) Start[Index] = Start[Index - 1] + ErrCorrCodewords;

			// restore all groups
			PtrEnd = MaxCodewords;
			Block = 0;
			for (; Ptr < PtrEnd; Ptr++)
			{
				TempArray[Start[Block]] = CodewordsArray[Ptr];
				Start[Block]++;
				Block++;
				if (Block == TotalBlocks) Block = 0;
			}

			// save result
			CodewordsArray = TempArray;
		}

		////////////////////////////////////////////////////////////////////
		// Calculate Error Correction
		////////////////////////////////////////////////////////////////////

		protected void CalculateErrorCorrection()
		{
			// total error count
			int TotalErrorCount = 0;

			// set generator polynomial array
			byte[] Generator = StaticTables.GenArray[ErrCorrCodewords - 7];

			// error correcion calculation buffer
			int BufSize = Math.Max(DataCodewordsGroup1, DataCodewordsGroup2) + ErrCorrCodewords;
			byte[] ErrCorrBuff = new byte[BufSize];

			// initial number of data codewords
			int DataCodewords = DataCodewordsGroup1;
			int BuffLen = DataCodewords + ErrCorrCodewords;

			// codewords pointer
			int DataCodewordsPtr = 0;

			// codewords buffer error correction pointer
			int CodewordsArrayErrCorrPtr = MaxDataCodewords;

			// loop one block at a time
			int TotalBlocks = BlocksGroup1 + BlocksGroup2;
			for (int BlockNumber = 0; BlockNumber < TotalBlocks; BlockNumber++)
			{
				// switch to group2 data codewords
				if (BlockNumber == BlocksGroup1)
				{
					DataCodewords = DataCodewordsGroup2;
					BuffLen = DataCodewords + ErrCorrCodewords;
				}

				// copy next block of codewords to the buffer and clear the remaining part
				Array.Copy(CodewordsArray, DataCodewordsPtr, ErrCorrBuff, 0, DataCodewords);
				Array.Copy(CodewordsArray, CodewordsArrayErrCorrPtr, ErrCorrBuff, DataCodewords, ErrCorrCodewords);

				// make a duplicate
				byte[] CorrectionBuffer = (byte[])ErrCorrBuff.Clone();

				// error correction polynomial division
				ReedSolomon.PolynominalDivision(ErrCorrBuff, BuffLen, Generator, ErrCorrCodewords);

				// test for error
				int Index;
				for (Index = 0; Index < ErrCorrCodewords && ErrCorrBuff[DataCodewords + Index] == 0; Index++) ;
				if (Index < ErrCorrCodewords)
				{
					// correct the error
					int ErrorCount = ReedSolomon.CorrectData(CorrectionBuffer, BuffLen, ErrCorrCodewords);
					if (ErrorCount <= 0)
					{
						throw new ApplicationException("Data is damaged. Error correction failed");
					}

					TotalErrorCount += ErrorCount;

					// fix the data
					Array.Copy(CorrectionBuffer, 0, CodewordsArray, DataCodewordsPtr, DataCodewords);
				}

				// update codewords array to next buffer
				DataCodewordsPtr += DataCodewords;

				// update pointer				
				CodewordsArrayErrCorrPtr += ErrCorrCodewords;
			}
		}

		////////////////////////////////////////////////////////////////////
		// Convert bit array to byte array
		////////////////////////////////////////////////////////////////////

		internal byte[] DecodeData()
		{
			// bit buffer initial condition
			BitBuffer = (UInt32)((CodewordsArray[0] << 24) | (CodewordsArray[1] << 16) | (CodewordsArray[2] << 8) | CodewordsArray[3]);
			BitBufferLen = 32;
			CodewordsPtr = 4;

			// allocate data byte list
			List<byte> DataSeg = new List<byte>();

			// data might be made of blocks
			for (; ; )
			{
				// first 4 bits is mode indicator
				EncodingMode EncodingMode = (EncodingMode)ReadBitsFromCodewordsArray(4);

				// end of data
				if (EncodingMode <= 0) break;

				// read data length
				int DataLength = ReadBitsFromCodewordsArray(DataLengthBits(EncodingMode));
				if (DataLength < 0)
				{
					throw new ApplicationException("Premature end of data (DataLengh)");
				}

				// save start of segment
				int SegStart = DataSeg.Count;

				// switch based on encode mode
				// numeric code indicator is 0001, alpha numeric 0010, byte 0100
				switch (EncodingMode)
				{
					// numeric mode
					case EncodingMode.Numeric:
						// encode digits in groups of 2
						int NumericEnd = (DataLength / 3) * 3;
						for (int Index = 0; Index < NumericEnd; Index += 3)
						{
							int Temp = ReadBitsFromCodewordsArray(10);
							if (Temp < 0)
							{
								throw new ApplicationException("Premature end of data (Numeric 1)");
							}
							DataSeg.Add(StaticTables.DecodingTable[Temp / 100]);
							DataSeg.Add(StaticTables.DecodingTable[(Temp % 100) / 10]);
							DataSeg.Add(StaticTables.DecodingTable[Temp % 10]);
						}

						// we have one character remaining
						if (DataLength - NumericEnd == 1)
						{
							int Temp = ReadBitsFromCodewordsArray(4);
							if (Temp < 0)
							{
								throw new ApplicationException("Premature end of data (Numeric 2)");
							}
							DataSeg.Add(StaticTables.DecodingTable[Temp]);
						}

						// we have two character remaining
						else if (DataLength - NumericEnd == 2)
						{
							int Temp = ReadBitsFromCodewordsArray(7);
							if (Temp < 0)
							{
								throw new ApplicationException("Premature end of data (Numeric 3)");
							}
							DataSeg.Add(StaticTables.DecodingTable[Temp / 10]);
							DataSeg.Add(StaticTables.DecodingTable[Temp % 10]);
						}
						break;

					// alphanumeric mode
					case EncodingMode.AlphaNumeric:
						// encode digits in groups of 2
						int AlphaNumEnd = (DataLength / 2) * 2;
						for (int Index = 0; Index < AlphaNumEnd; Index += 2)
						{
							int Temp = ReadBitsFromCodewordsArray(11);
							if (Temp < 0)
							{
								throw new ApplicationException("Premature end of data (Alpha Numeric 1)");
							}
							DataSeg.Add(StaticTables.DecodingTable[Temp / 45]);
							DataSeg.Add(StaticTables.DecodingTable[Temp % 45]);
						}

						// we have one character remaining
						if (DataLength - AlphaNumEnd == 1)
						{
							int Temp = ReadBitsFromCodewordsArray(6);
							if (Temp < 0)
							{
								throw new ApplicationException("Premature end of data (Alpha Numeric 2)");
							}
							DataSeg.Add(StaticTables.DecodingTable[Temp]);
						}
						break;

					// byte mode					
					case EncodingMode.Byte:
						// append the data after mode and character count
						for (int Index = 0; Index < DataLength; Index++)
						{
							int Temp = ReadBitsFromCodewordsArray(8);
							if (Temp < 0)
							{
								throw new ApplicationException("Premature end of data (byte mode)");
							}
							DataSeg.Add((byte)Temp);
						}
						break;

					default:
						throw new ApplicationException(string.Format("Encoding mode not supported {0}", EncodingMode.ToString()));
				}

				if (DataLength != DataSeg.Count - SegStart) throw new ApplicationException("Data encoding length in error");
			}

			// save data
			return DataSeg.ToArray();
		}

		////////////////////////////////////////////////////////////////////
		// Read data from codeword array
		////////////////////////////////////////////////////////////////////

		internal int ReadBitsFromCodewordsArray(int Bits)
		{
			if (Bits > BitBufferLen) return -1;
			int Data = (int)(BitBuffer >> (32 - Bits));
			BitBuffer <<= Bits;
			BitBufferLen -= Bits;
			while (BitBufferLen <= 24 && CodewordsPtr < MaxDataCodewords)
			{
				BitBuffer |= (UInt32)(CodewordsArray[CodewordsPtr++] << (24 - BitBufferLen));
				BitBufferLen += 8;
			}
			return Data;
		}
		////////////////////////////////////////////////////////////////////
		// Set encoded data bits length
		////////////////////////////////////////////////////////////////////

		internal int DataLengthBits(EncodingMode EncodingMode)
		{
			// Data length bits
			switch (EncodingMode)
			{
				// numeric mode
				case EncodingMode.Numeric:
					return QRCodeVersion < 10 ? 10 : (QRCodeVersion < 27 ? 12 : 14);

				// alpha numeric mode
				case EncodingMode.AlphaNumeric:
					return QRCodeVersion < 10 ? 9 : (QRCodeVersion < 27 ? 11 : 13);

				// byte mode
				case EncodingMode.Byte:
					return QRCodeVersion < 10 ? 8 : 16;
			}
			throw new ApplicationException("Encoding mode error");
		}

		////////////////////////////////////////////////////////////////////
		// Set data and error correction codewords length
		////////////////////////////////////////////////////////////////////

		internal void SetDataCodewordsLength()
		{
			// index shortcut
			int BlockInfoIndex = (QRCodeVersion - 1) * 4 + (int)ErrorCorrection;

			// Number of blocks in group 1
			BlocksGroup1 = StaticTables.ECBlockInfo[BlockInfoIndex, StaticTables.BLOCKS_GROUP1];

			// Number of data codewords in blocks of group 1
			DataCodewordsGroup1 = StaticTables.ECBlockInfo[BlockInfoIndex, StaticTables.DATA_CODEWORDS_GROUP1];

			// Number of blocks in group 2
			BlocksGroup2 = StaticTables.ECBlockInfo[BlockInfoIndex, StaticTables.BLOCKS_GROUP2];

			// Number of data codewords in blocks of group 2
			DataCodewordsGroup2 = StaticTables.ECBlockInfo[BlockInfoIndex, StaticTables.DATA_CODEWORDS_GROUP2];

			// Total number of data codewords for this version and EC level
			MaxDataCodewords = BlocksGroup1 * DataCodewordsGroup1 + BlocksGroup2 * DataCodewordsGroup2;
			MaxDataBits = 8 * MaxDataCodewords;

			// total data plus error correction bits
			MaxCodewords = StaticTables.MaxCodewordsArray[QRCodeVersion];

			// Error correction codewords per block
			ErrCorrCodewords = (MaxCodewords - MaxDataCodewords) / (BlocksGroup1 + BlocksGroup2);

			// exit
			return;
		}

		////////////////////////////////////////////////////////////////////
		// Format info to error correction code
		////////////////////////////////////////////////////////////////////

		internal ErrorCorrection FormatInfoToErrCode(int Info)
		{
			return (ErrorCorrection)(Info ^ 1);
		}

		////////////////////////////////////////////////////////////////////
		// Build Base Matrix
		////////////////////////////////////////////////////////////////////

		internal void BuildBaseMatrix()
		{
			// allocate base matrix
			BaseMatrix = new byte[QRCodeDimension + 5, QRCodeDimension + 5];

			// top left finder patterns
			for (int Row = 0; Row < 9; Row++) for (int Col = 0; Col < 9; Col++) BaseMatrix[Row, Col] = StaticTables.FinderPatternTopLeft[Row, Col];

			// top right finder patterns
			int Pos = QRCodeDimension - 8;
			for (int Row = 0; Row < 9; Row++) for (int Col = 0; Col < 8; Col++) BaseMatrix[Row, Pos + Col] = StaticTables.FinderPatternTopRight[Row, Col];

			// bottom left finder patterns
			for (int Row = 0; Row < 8; Row++) for (int Col = 0; Col < 9; Col++) BaseMatrix[Pos + Row, Col] = StaticTables.FinderPatternBottomLeft[Row, Col];

			// Timing pattern
			for (int Z = 8; Z < QRCodeDimension - 8; Z++) BaseMatrix[Z, 6] = BaseMatrix[6, Z] = (Z & 1) == 0 ? StaticTables.FixedBlack : StaticTables.FixedWhite;

			// alignment pattern
			if (QRCodeVersion > 1)
			{
				byte[] AlignPos = StaticTables.AlignmentPositionArray[QRCodeVersion];
				int AlignmentDimension = AlignPos.Length;
				for (int Row = 0; Row < AlignmentDimension; Row++) for (int Col = 0; Col < AlignmentDimension; Col++)
					{
						if (Col == 0 && Row == 0 || Col == AlignmentDimension - 1 && Row == 0 || Col == 0 && Row == AlignmentDimension - 1) continue;

						int PosRow = AlignPos[Row];
						int PosCol = AlignPos[Col];
						for (int ARow = -2; ARow < 3; ARow++) for (int ACol = -2; ACol < 3; ACol++)
							{
								BaseMatrix[PosRow + ARow, PosCol + ACol] = StaticTables.AlignmentPattern[ARow + 2, ACol + 2];
							}
					}
			}

			// reserve version information
			if (QRCodeVersion >= 7)
			{
				// position of 3 by 6 rectangles
				Pos = QRCodeDimension - 11;

				// top right
				for (int Row = 0; Row < 6; Row++) for (int Col = 0; Col < 3; Col++) BaseMatrix[Row, Pos + Col] = StaticTables.FormatWhite;

				// bottom right
				for (int Col = 0; Col < 6; Col++) for (int Row = 0; Row < 3; Row++) BaseMatrix[Pos + Row, Col] = StaticTables.FormatWhite;
			}
		}

		////////////////////////////////////////////////////////////////////
		// Apply Mask
		////////////////////////////////////////////////////////////////////

		internal void ApplyMask(int Mask)
		{
			MaskMatrix = (byte[,])BaseMatrix.Clone();
			switch (Mask)
			{
				case 0:
					ApplyMask0();
					break;

				case 1:
					ApplyMask1();
					break;

				case 2:
					ApplyMask2();
					break;

				case 3:
					ApplyMask3();
					break;

				case 4:
					ApplyMask4();
					break;

				case 5:
					ApplyMask5();
					break;

				case 6:
					ApplyMask6();
					break;

				case 7:
					ApplyMask7();
					break;
			}
			return;
		}

		////////////////////////////////////////////////////////////////////
		// Apply Mask 0
		// (row + column) % 2 == 0
		////////////////////////////////////////////////////////////////////

		internal void ApplyMask0()
		{
			for (int Row = 0; Row < QRCodeDimension; Row += 2) for (int Col = 0; Col < QRCodeDimension; Col += 2)
				{
					if ((MaskMatrix[Row, Col] & StaticTables.NonData) == 0) MaskMatrix[Row, Col] ^= 1;
					if ((MaskMatrix[Row + 1, Col + 1] & StaticTables.NonData) == 0) MaskMatrix[Row + 1, Col + 1] ^= 1;
				}
			return;
		}

		////////////////////////////////////////////////////////////////////
		// Apply Mask 1
		// row % 2 == 0
		////////////////////////////////////////////////////////////////////

		internal void ApplyMask1()
		{
			for (int Row = 0; Row < QRCodeDimension; Row += 2) for (int Col = 0; Col < QRCodeDimension; Col++)
					if ((MaskMatrix[Row, Col] & StaticTables.NonData) == 0) MaskMatrix[Row, Col] ^= 1;
			return;
		}

		////////////////////////////////////////////////////////////////////
		// Apply Mask 2
		// column % 3 == 0
		////////////////////////////////////////////////////////////////////

		internal void ApplyMask2()
		{
			for (int Row = 0; Row < QRCodeDimension; Row++) for (int Col = 0; Col < QRCodeDimension; Col += 3)
					if ((MaskMatrix[Row, Col] & StaticTables.NonData) == 0) MaskMatrix[Row, Col] ^= 1;
			return;
		}

		////////////////////////////////////////////////////////////////////
		// Apply Mask 3
		// (row + column) % 3 == 0
		////////////////////////////////////////////////////////////////////

		internal void ApplyMask3()
		{
			for (int Row = 0; Row < QRCodeDimension; Row += 3) for (int Col = 0; Col < QRCodeDimension; Col += 3)
				{
					if ((MaskMatrix[Row, Col] & StaticTables.NonData) == 0) MaskMatrix[Row, Col] ^= 1;
					if ((MaskMatrix[Row + 1, Col + 2] & StaticTables.NonData) == 0) MaskMatrix[Row + 1, Col + 2] ^= 1;
					if ((MaskMatrix[Row + 2, Col + 1] & StaticTables.NonData) == 0) MaskMatrix[Row + 2, Col + 1] ^= 1;
				}
		}

		////////////////////////////////////////////////////////////////////
		// Apply Mask 4
		// ((row / 2) + (column / 3)) % 2 == 0
		////////////////////////////////////////////////////////////////////

		internal void ApplyMask4()
		{
			for (int Row = 0; Row < QRCodeDimension; Row += 4) for (int Col = 0; Col < QRCodeDimension; Col += 6)
				{
					if ((MaskMatrix[Row, Col] & StaticTables.NonData) == 0) MaskMatrix[Row, Col] ^= 1;
					if ((MaskMatrix[Row, Col + 1] & StaticTables.NonData) == 0) MaskMatrix[Row, Col + 1] ^= 1;
					if ((MaskMatrix[Row, Col + 2] & StaticTables.NonData) == 0) MaskMatrix[Row, Col + 2] ^= 1;

					if ((MaskMatrix[Row + 1, Col] & StaticTables.NonData) == 0) MaskMatrix[Row + 1, Col] ^= 1;
					if ((MaskMatrix[Row + 1, Col + 1] & StaticTables.NonData) == 0) MaskMatrix[Row + 1, Col + 1] ^= 1;
					if ((MaskMatrix[Row + 1, Col + 2] & StaticTables.NonData) == 0) MaskMatrix[Row + 1, Col + 2] ^= 1;

					if ((MaskMatrix[Row + 2, Col + 3] & StaticTables.NonData) == 0) MaskMatrix[Row + 2, Col + 3] ^= 1;
					if ((MaskMatrix[Row + 2, Col + 4] & StaticTables.NonData) == 0) MaskMatrix[Row + 2, Col + 4] ^= 1;
					if ((MaskMatrix[Row + 2, Col + 5] & StaticTables.NonData) == 0) MaskMatrix[Row + 2, Col + 5] ^= 1;

					if ((MaskMatrix[Row + 3, Col + 3] & StaticTables.NonData) == 0) MaskMatrix[Row + 3, Col + 3] ^= 1;
					if ((MaskMatrix[Row + 3, Col + 4] & StaticTables.NonData) == 0) MaskMatrix[Row + 3, Col + 4] ^= 1;
					if ((MaskMatrix[Row + 3, Col + 5] & StaticTables.NonData) == 0) MaskMatrix[Row + 3, Col + 5] ^= 1;
				}
			return;
		}

		////////////////////////////////////////////////////////////////////
		// Apply Mask 5
		// ((row * column) % 2) + ((row * column) % 3) == 0
		////////////////////////////////////////////////////////////////////

		internal void ApplyMask5()
		{
			for (int Row = 0; Row < QRCodeDimension; Row += 6) for (int Col = 0; Col < QRCodeDimension; Col += 6)
				{
					for (int Delta = 0; Delta < 6; Delta++) if ((MaskMatrix[Row, Col + Delta] & StaticTables.NonData) == 0) MaskMatrix[Row, Col + Delta] ^= 1;
					for (int Delta = 1; Delta < 6; Delta++) if ((MaskMatrix[Row + Delta, Col] & StaticTables.NonData) == 0) MaskMatrix[Row + Delta, Col] ^= 1;
					if ((MaskMatrix[Row + 2, Col + 3] & StaticTables.NonData) == 0) MaskMatrix[Row + 2, Col + 3] ^= 1;
					if ((MaskMatrix[Row + 3, Col + 2] & StaticTables.NonData) == 0) MaskMatrix[Row + 3, Col + 2] ^= 1;
					if ((MaskMatrix[Row + 3, Col + 4] & StaticTables.NonData) == 0) MaskMatrix[Row + 3, Col + 4] ^= 1;
					if ((MaskMatrix[Row + 4, Col + 3] & StaticTables.NonData) == 0) MaskMatrix[Row + 4, Col + 3] ^= 1;
				}
			return;
		}

		////////////////////////////////////////////////////////////////////
		// Apply Mask 6
		// (((row * column) % 2) + ((row * column) mod 3)) mod 2 == 0
		////////////////////////////////////////////////////////////////////

		internal void ApplyMask6()
		{
			for (int Row = 0; Row < QRCodeDimension; Row += 6) for (int Col = 0; Col < QRCodeDimension; Col += 6)
				{
					for (int Delta = 0; Delta < 6; Delta++) if ((MaskMatrix[Row, Col + Delta] & StaticTables.NonData) == 0) MaskMatrix[Row, Col + Delta] ^= 1;
					for (int Delta = 1; Delta < 6; Delta++) if ((MaskMatrix[Row + Delta, Col] & StaticTables.NonData) == 0) MaskMatrix[Row + Delta, Col] ^= 1;
					if ((MaskMatrix[Row + 1, Col + 1] & StaticTables.NonData) == 0) MaskMatrix[Row + 1, Col + 1] ^= 1;
					if ((MaskMatrix[Row + 1, Col + 2] & StaticTables.NonData) == 0) MaskMatrix[Row + 1, Col + 2] ^= 1;
					if ((MaskMatrix[Row + 2, Col + 1] & StaticTables.NonData) == 0) MaskMatrix[Row + 2, Col + 1] ^= 1;
					if ((MaskMatrix[Row + 2, Col + 3] & StaticTables.NonData) == 0) MaskMatrix[Row + 2, Col + 3] ^= 1;
					if ((MaskMatrix[Row + 2, Col + 4] & StaticTables.NonData) == 0) MaskMatrix[Row + 2, Col + 4] ^= 1;
					if ((MaskMatrix[Row + 3, Col + 2] & StaticTables.NonData) == 0) MaskMatrix[Row + 3, Col + 2] ^= 1;
					if ((MaskMatrix[Row + 3, Col + 4] & StaticTables.NonData) == 0) MaskMatrix[Row + 3, Col + 4] ^= 1;
					if ((MaskMatrix[Row + 4, Col + 2] & StaticTables.NonData) == 0) MaskMatrix[Row + 4, Col + 2] ^= 1;
					if ((MaskMatrix[Row + 4, Col + 3] & StaticTables.NonData) == 0) MaskMatrix[Row + 4, Col + 3] ^= 1;
					if ((MaskMatrix[Row + 4, Col + 5] & StaticTables.NonData) == 0) MaskMatrix[Row + 4, Col + 5] ^= 1;
					if ((MaskMatrix[Row + 5, Col + 4] & StaticTables.NonData) == 0) MaskMatrix[Row + 5, Col + 4] ^= 1;
					if ((MaskMatrix[Row + 5, Col + 5] & StaticTables.NonData) == 0) MaskMatrix[Row + 5, Col + 5] ^= 1;
				}
			return;
		}

		////////////////////////////////////////////////////////////////////
		// Apply Mask 7
		// (((row + column) % 2) + ((row * column) mod 3)) mod 2 == 0
		////////////////////////////////////////////////////////////////////

		internal void ApplyMask7()
		{
			for (int Row = 0; Row < QRCodeDimension; Row += 6) for (int Col = 0; Col < QRCodeDimension; Col += 6)
				{
					if ((MaskMatrix[Row, Col] & StaticTables.NonData) == 0) MaskMatrix[Row, Col] ^= 1;
					if ((MaskMatrix[Row, Col + 2] & StaticTables.NonData) == 0) MaskMatrix[Row, Col + 2] ^= 1;
					if ((MaskMatrix[Row, Col + 4] & StaticTables.NonData) == 0) MaskMatrix[Row, Col + 4] ^= 1;

					if ((MaskMatrix[Row + 1, Col + 3] & StaticTables.NonData) == 0) MaskMatrix[Row + 1, Col + 3] ^= 1;
					if ((MaskMatrix[Row + 1, Col + 4] & StaticTables.NonData) == 0) MaskMatrix[Row + 1, Col + 4] ^= 1;
					if ((MaskMatrix[Row + 1, Col + 5] & StaticTables.NonData) == 0) MaskMatrix[Row + 1, Col + 5] ^= 1;

					if ((MaskMatrix[Row + 2, Col] & StaticTables.NonData) == 0) MaskMatrix[Row + 2, Col] ^= 1;
					if ((MaskMatrix[Row + 2, Col + 4] & StaticTables.NonData) == 0) MaskMatrix[Row + 2, Col + 4] ^= 1;
					if ((MaskMatrix[Row + 2, Col + 5] & StaticTables.NonData) == 0) MaskMatrix[Row + 2, Col + 5] ^= 1;

					if ((MaskMatrix[Row + 3, Col + 1] & StaticTables.NonData) == 0) MaskMatrix[Row + 3, Col + 1] ^= 1;
					if ((MaskMatrix[Row + 3, Col + 3] & StaticTables.NonData) == 0) MaskMatrix[Row + 3, Col + 3] ^= 1;
					if ((MaskMatrix[Row + 3, Col + 5] & StaticTables.NonData) == 0) MaskMatrix[Row + 3, Col + 5] ^= 1;

					if ((MaskMatrix[Row + 4, Col] & StaticTables.NonData) == 0) MaskMatrix[Row + 4, Col] ^= 1;
					if ((MaskMatrix[Row + 4, Col + 1] & StaticTables.NonData) == 0) MaskMatrix[Row + 4, Col + 1] ^= 1;
					if ((MaskMatrix[Row + 4, Col + 2] & StaticTables.NonData) == 0) MaskMatrix[Row + 4, Col + 2] ^= 1;

					if ((MaskMatrix[Row + 5, Col + 1] & StaticTables.NonData) == 0) MaskMatrix[Row + 5, Col + 1] ^= 1;
					if ((MaskMatrix[Row + 5, Col + 2] & StaticTables.NonData) == 0) MaskMatrix[Row + 5, Col + 2] ^= 1;
					if ((MaskMatrix[Row + 5, Col + 3] & StaticTables.NonData) == 0) MaskMatrix[Row + 5, Col + 3] ^= 1;
				}
			return;
		}
	}

	/// <summary>
	/// QR Code error correction code enumeration
	/// </summary>
	public enum ErrorCorrection
	{
		/// <summary>
		/// Low (01)
		/// </summary>
		L,

		/// <summary>
		/// Medium (00)
		/// </summary>
		M,

		/// <summary>
		/// Medium-high (11)
		/// </summary>
		Q,

		/// <summary>
		/// High (10)
		/// </summary>
		H,
	}

	/// <summary>
	/// QR Code encoding modes
	/// </summary>
	public enum EncodingMode
	{
		/// <summary>
		/// Terminator
		/// </summary>
		Terminator,

		/// <summary>
		/// Numeric
		/// </summary>
		Numeric,

		/// <summary>
		/// Alpha numeric
		/// </summary>
		AlphaNumeric,

		/// <summary>
		/// Append
		/// </summary>
		Append,

		/// <summary>
		/// byte encoding
		/// </summary>
		Byte,

		/// <summary>
		/// FNC1 first
		/// </summary>
		FNC1First,

		/// <summary>
		/// Unknown encoding constant
		/// </summary>
		Unknown6,

		/// <summary>
		/// Unknown encoding constant
		/// </summary>
		Unknown7,

		/// <summary>
		/// Kanji encoding (not implemented by this software)
		/// </summary>
		Kanji,

		/// <summary>
		/// FNC1 second
		/// </summary>
		FNC1Second,

		/// <summary>
		/// Unknown encoding constant
		/// </summary>
		Unknown10,

		/// <summary>
		/// Unknown encoding constant
		/// </summary>
		Unknown11,

		/// <summary>
		/// Unknown encoding constant
		/// </summary>
		Unknown12,

		/// <summary>
		/// Unknown encoding constant
		/// </summary>
		Unknown13,

		/// <summary>
		/// Unknown encoding constant
		/// </summary>
		Unknown14,

		/// <summary>
		/// Unknown encoding constant
		/// </summary>
		Unknown15,
	}

	public enum ImageFormatIndex
	{
		Png,
		Jpeg,
		Bmp,
		Gif,
	}

	internal class Finder
	{
		// horizontal scan
		internal int Row;
		internal int Col1;
		internal int Col2;
		internal double HModule;

		// vertical scan
		internal int Col;
		internal int Row1;
		internal int Row2;
		internal double VModule;

		internal double Distance;
		internal double ModuleSize;

		/// <summary>
		/// Constructor during horizontal scan
		/// </summary>
		internal Finder(int Row, int Col1, int Col2, double HModule)
		{
			this.Row = Row;
			this.Col1 = Col1;
			this.Col2 = Col2;
			this.HModule = HModule;
			Distance = double.MaxValue;
			return;
		}

		/// <summary>
		/// Match during vertical scan
		/// </summary>
		internal void Match(int Col, int Row1, int Row2, double VModule)
		{
			// test if horizontal and vertical are not related
			if (Col < Col1 || Col >= Col2 || Row < Row1 || Row >= Row2) return;

			// Module sizes must be about the same
			if (Math.Min(HModule, VModule) < Math.Max(HModule, VModule) * QRDecoder.MODULE_SIZE_DEVIATION) return;

			// calculate distance
			double DeltaX = Col - 0.5 * (Col1 + Col2);
			double DeltaY = Row - 0.5 * (Row1 + Row2);
			double Delta = Math.Sqrt(DeltaX * DeltaX + DeltaY * DeltaY);

			// distance between two points must be less than 2 pixels
			if (Delta > QRDecoder.HOR_VERT_SCAN_MAX_DISTANCE) return;

			// new result is better than last result
			if (Delta < Distance)
			{
				this.Col = Col;
				this.Row1 = Row1;
				this.Row2 = Row2;
				this.VModule = VModule;
				ModuleSize = 0.5 * (HModule + VModule);
				Distance = Delta;
			}
			return;
		}

		/// <summary>
		/// Horizontal and vertical scans overlap
		/// </summary>
		internal bool Overlap
				(
				Finder Other
				)
		{
			return Other.Col1 < Col2 && Other.Col2 >= Col1 && Other.Row1 < Row2 && Other.Row2 >= Row1;
		}

		/// <summary>
		/// Finder to string
		/// </summary>
		public override string ToString()
		{
			if (Distance == double.MaxValue)
			{
				return string.Format("Finder: Row: {0}, Col1: {1}, Col2: {2}, HModule: {3:0.00}", Row, Col1, Col2, HModule);
			}

			return string.Format("Finder: Row: {0}, Col: {1}, Module: {2:0.00}, Distance: {3:0.00}", Row, Col, ModuleSize, Distance);
		}
	}

	internal class Corner
	{
		internal Finder TopLeftFinder;
		internal Finder TopRightFinder;
		internal Finder BottomLeftFinder;

		internal double TopLineDeltaX;
		internal double TopLineDeltaY;
		internal double TopLineLength;
		internal double LeftLineDeltaX;
		internal double LeftLineDeltaY;
		internal double LeftLineLength;

		/////////////////////////////////////////////////////////////////////
		// QR corner constructor
		/////////////////////////////////////////////////////////////////////

		private Corner
				(
				Finder TopLeftFinder,
				Finder TopRightFinder,
				Finder BottomLeftFinder
				)
		{
			// save three finders
			this.TopLeftFinder = TopLeftFinder;
			this.TopRightFinder = TopRightFinder;
			this.BottomLeftFinder = BottomLeftFinder;

			// top line slope
			TopLineDeltaX = TopRightFinder.Col - TopLeftFinder.Col;
			TopLineDeltaY = TopRightFinder.Row - TopLeftFinder.Row;

			// top line length
			TopLineLength = Math.Sqrt(TopLineDeltaX * TopLineDeltaX + TopLineDeltaY * TopLineDeltaY);

			// left line slope
			LeftLineDeltaX = BottomLeftFinder.Col - TopLeftFinder.Col;
			LeftLineDeltaY = BottomLeftFinder.Row - TopLeftFinder.Row;

			// left line length
			LeftLineLength = Math.Sqrt(LeftLineDeltaX * LeftLineDeltaX + LeftLineDeltaY * LeftLineDeltaY);
			return;
		}

		/////////////////////////////////////////////////////////////////////
		// Test QR corner for validity
		/////////////////////////////////////////////////////////////////////

		internal static Corner CreateCorner
				(
				Finder TopLeftFinder,
				Finder TopRightFinder,
				Finder BottomLeftFinder
				)
		{
			// try all three possible permutation of three finders
			for (int Index = 0; Index < 3; Index++)
			{
				// TestCorner runs three times to test all posibilities
				// rotate top left, top right and bottom left
				if (Index != 0)
				{
					Finder Temp = TopLeftFinder;
					TopLeftFinder = TopRightFinder;
					TopRightFinder = BottomLeftFinder;
					BottomLeftFinder = Temp;
				}

				// top line slope
				double TopLineDeltaX = TopRightFinder.Col - TopLeftFinder.Col;
				double TopLineDeltaY = TopRightFinder.Row - TopLeftFinder.Row;

				// left line slope
				double LeftLineDeltaX = BottomLeftFinder.Col - TopLeftFinder.Col;
				double LeftLineDeltaY = BottomLeftFinder.Row - TopLeftFinder.Row;

				// top line length
				double TopLineLength = Math.Sqrt(TopLineDeltaX * TopLineDeltaX + TopLineDeltaY * TopLineDeltaY);

				// left line length
				double LeftLineLength = Math.Sqrt(LeftLineDeltaX * LeftLineDeltaX + LeftLineDeltaY * LeftLineDeltaY);

				// the short side must be at least 80% of the long side
				if (Math.Min(TopLineLength, LeftLineLength) < QRDecoder.CORNER_SIDE_LENGTH_DEV * Math.Max(TopLineLength, LeftLineLength)) continue;

				// top line vector
				double TopLineSin = TopLineDeltaY / TopLineLength;
				double TopLineCos = TopLineDeltaX / TopLineLength;

				// rotate lines such that top line is parallel to x axis
				// left line after rotation
				double NewLeftX = TopLineCos * LeftLineDeltaX + TopLineSin * LeftLineDeltaY;
				double NewLeftY = -TopLineSin * LeftLineDeltaX + TopLineCos * LeftLineDeltaY;

				// new left line X should be zero (or between +/- 4 deg)
				if (Math.Abs(NewLeftX / LeftLineLength) > QRDecoder.CORNER_RIGHT_ANGLE_DEV) continue;

				// swap top line with left line
				if (NewLeftY < 0)
				{
					// swap top left with bottom right
					Finder TempFinder = TopRightFinder;
					TopRightFinder = BottomLeftFinder;
					BottomLeftFinder = TempFinder;
				}

				return new Corner(TopLeftFinder, TopRightFinder, BottomLeftFinder);
			}
			return null;
		}

		/////////////////////////////////////////////////////////////////////
		// Test QR corner for validity
		/////////////////////////////////////////////////////////////////////

		internal int InitialVersionNumber()
		{
			// version number based on top line
			double TopModules = 7;

			// top line is mostly horizontal
			if (Math.Abs(TopLineDeltaX) >= Math.Abs(TopLineDeltaY))
			{
				TopModules += TopLineLength * TopLineLength /
					(Math.Abs(TopLineDeltaX) * 0.5 * (TopLeftFinder.HModule + TopRightFinder.HModule));
			}

			// top line is mostly vertical
			else
			{
				TopModules += TopLineLength * TopLineLength /
					(Math.Abs(TopLineDeltaY) * 0.5 * (TopLeftFinder.VModule + TopRightFinder.VModule));
			}

			// version number based on left line
			double LeftModules = 7;

			// Left line is mostly vertical
			if (Math.Abs(LeftLineDeltaY) >= Math.Abs(LeftLineDeltaX))
			{
				LeftModules += LeftLineLength * LeftLineLength /
					(Math.Abs(LeftLineDeltaY) * 0.5 * (TopLeftFinder.VModule + BottomLeftFinder.VModule));
			}

			// left line is mostly horizontal
			else
			{
				LeftModules += LeftLineLength * LeftLineLength /
					(Math.Abs(LeftLineDeltaX) * 0.5 * (TopLeftFinder.HModule + BottomLeftFinder.HModule));
			}

			// version (there is rounding in the calculation)
			int Version = ((int)Math.Round(0.5 * (TopModules + LeftModules)) - 15) / 4;

			// not a valid corner
			if (Version < 1 || Version > 40) throw new ApplicationException("Corner is not valid (version number must be 1 to 40)");

			// exit with version number
			return Version;
		}
	}

	internal class ReedSolomon
	{
		private static int INCORRECTABLE_ERROR = -1;

		internal static int CorrectData(byte[] ReceivedData, int DataLength, int ErrCorrCodewords)
		{
			// calculate syndrome vector
			int[] Syndrome = CalculateSyndrome(ReceivedData, DataLength, ErrCorrCodewords);

			// received data has no error
			// note: this should not happen because we call this method only if error was detected
			if (Syndrome == null) return 0;

			// Modified Berlekamp-Massey
			// calculate sigma and omega
			int[] Sigma = new int[ErrCorrCodewords / 2 + 2];
			int[] Omega = new int[ErrCorrCodewords / 2 + 1];
			int ErrorCount = CalculateSigmaMBM(Sigma, Omega, Syndrome, ErrCorrCodewords);

			// data cannot be corrected
			if (ErrorCount <= 0) return INCORRECTABLE_ERROR;

			// look for error position using Chien search
			int[] ErrorPosition = new int[ErrorCount];
			if (!ChienSearch(ErrorPosition, DataLength, ErrorCount, Sigma)) return INCORRECTABLE_ERROR;

			// correct data array based on position array
			ApplyCorrection(ReceivedData, DataLength, ErrorCount, ErrorPosition, Sigma, Omega);

			// return error count before it was corrected
			return ErrorCount;
		}

		// Syndrome vector calculation
		// S0 = R0 + R1 +        R2 + ....        + Rn
		// S1 = R0 + R1 * A**1 + R2 * A**2 + .... + Rn * A**n
		// S2 = R0 + R1 * A**2 + R2 * A**4 + .... + Rn * A**2n
		// ....
		// Sm = R0 + R1 * A**m + R2 * A**2m + .... + Rn * A**mn
		private static int[] CalculateSyndrome(byte[] ReceivedData, int DataLength, int ErrCorrCodewords)
		{
			// allocate syndrome vector
			int[] Syndrome = new int[ErrCorrCodewords];

			// reset error indicator
			bool Error = false;

			// syndrome[zero] special case
			// Total = Data[0] + Data[1] + ... Data[n]
			int Total = ReceivedData[0];
			for (int SumIndex = 1; SumIndex < DataLength; SumIndex++) Total = ReceivedData[SumIndex] ^ Total;
			Syndrome[0] = Total;
			if (Total != 0) Error = true;

			// all other synsromes
			for (int Index = 1; Index < ErrCorrCodewords; Index++)
			{
				// Total = Data[0] + Data[1] * Alpha + Data[2] * Alpha ** 2 + ... Data[n] * Alpha ** n
				Total = ReceivedData[0];
				for (int IndexT = 1; IndexT < DataLength; IndexT++) Total = ReceivedData[IndexT] ^ MultiplyIntByExp(Total, Index);
				Syndrome[Index] = Total;
				if (Total != 0) Error = true;
			}

			// if there is an error return syndrome vector otherwise return null
			return Error ? Syndrome : null;
		}

		// Modified Berlekamp-Massey
		private static int CalculateSigmaMBM(int[] Sigma, int[] Omega, int[] Syndrome, int ErrCorrCodewords)
		{
			int[] PolyC = new int[ErrCorrCodewords];
			int[] PolyB = new int[ErrCorrCodewords];
			PolyC[1] = 1;
			PolyB[0] = 1;
			int ErrorControl = 1;
			int ErrorCount = 0;   // L
			int m = -1;

			for (int ErrCorrIndex = 0; ErrCorrIndex < ErrCorrCodewords; ErrCorrIndex++)
			{
				// Calculate the discrepancy
				int Dis = Syndrome[ErrCorrIndex];
				for (int i = 1; i <= ErrorCount; i++) Dis ^= Multiply(PolyB[i], Syndrome[ErrCorrIndex - i]);

				if (Dis != 0)
				{
					int DisExp = StaticTables.IntToExp[Dis];
					int[] WorkPolyB = new int[ErrCorrCodewords];
					for (int Index = 0; Index <= ErrCorrIndex; Index++) WorkPolyB[Index] = PolyB[Index] ^ MultiplyIntByExp(PolyC[Index], DisExp);
					int js = ErrCorrIndex - m;
					if (js > ErrorCount)
					{
						m = ErrCorrIndex - ErrorCount;
						ErrorCount = js;
						if (ErrorCount > ErrCorrCodewords / 2) return INCORRECTABLE_ERROR;
						for (int Index = 0; Index <= ErrorControl; Index++) PolyC[Index] = DivideIntByExp(PolyB[Index], DisExp);
						ErrorControl = ErrorCount;
					}
					PolyB = WorkPolyB;
				}

				// shift polynomial right one
				Array.Copy(PolyC, 0, PolyC, 1, Math.Min(PolyC.Length - 1, ErrorControl));
				PolyC[0] = 0;
				ErrorControl++;
			}

			PolynomialMultiply(Omega, PolyB, Syndrome);
			Array.Copy(PolyB, 0, Sigma, 0, Math.Min(PolyB.Length, Sigma.Length));
			return ErrorCount;
		}

		// Chien search is a fast algorithm for determining roots of polynomials defined over a finite field.
		// The most typical use of the Chien search is in finding the roots of error-locator polynomials
		// encountered in decoding Reed-Solomon codes and BCH codes.
		private static bool ChienSearch(int[] ErrorPosition, int DataLength, int ErrorCount, int[] Sigma)
		{
			// last error
			int LastPosition = Sigma[1];

			// one error
			if (ErrorCount == 1)
			{
				// position is out of range
				if (StaticTables.IntToExp[LastPosition] >= DataLength) return false;

				// save the only error position in position array
				ErrorPosition[0] = LastPosition;
				return true;
			}

			// we start at last error position
			int PosIndex = ErrorCount - 1;
			for (int DataIndex = 0; DataIndex < DataLength; DataIndex++)
			{
				int DataIndexInverse = 255 - DataIndex;
				int Total = 1;
				for (int Index = 1; Index <= ErrorCount; Index++) Total ^= MultiplyIntByExp(Sigma[Index], (DataIndexInverse * Index) % 255);
				if (Total != 0) continue;

				int Position = StaticTables.ExpToInt[DataIndex];
				LastPosition ^= Position;
				ErrorPosition[PosIndex--] = Position;
				if (PosIndex == 0)
				{
					// position is out of range
					if (StaticTables.IntToExp[LastPosition] >= DataLength) return false;
					ErrorPosition[0] = LastPosition;
					return true;
				}
			}

			// search failed
			return false;
		}

		private static void ApplyCorrection(byte[] ReceivedData, int DataLength, int ErrorCount, int[] ErrorPosition, int[] Sigma, int[] Omega)
		{
			for (int ErrIndex = 0; ErrIndex < ErrorCount; ErrIndex++)
			{
				int ps = ErrorPosition[ErrIndex];
				int zlog = 255 - StaticTables.IntToExp[ps];
				int OmegaTotal = Omega[0];
				for (int Index = 1; Index < ErrorCount; Index++) OmegaTotal ^= MultiplyIntByExp(Omega[Index], (zlog * Index) % 255);
				int SigmaTotal = Sigma[1];
				for (int j = 2; j < ErrorCount; j += 2) SigmaTotal ^= MultiplyIntByExp(Sigma[j + 1], (zlog * j) % 255);
				ReceivedData[DataLength - 1 - StaticTables.IntToExp[ps]] ^= (byte)MultiplyDivide(ps, OmegaTotal, SigmaTotal);
			}
		}

		internal static void PolynominalDivision(byte[] Polynomial, int PolyLength, byte[] Generator, int ErrCorrCodewords)
		{
			int DataCodewords = PolyLength - ErrCorrCodewords;

			// error correction polynomial division
			for (int Index = 0; Index < DataCodewords; Index++)
			{
				// current first codeword is zero
				if (Polynomial[Index] == 0) continue;

				// current first codeword is not zero
				int Multiplier = StaticTables.IntToExp[Polynomial[Index]];

				// loop for error correction coofficients
				for (int GeneratorIndex = 0; GeneratorIndex < ErrCorrCodewords; GeneratorIndex++)
				{
					Polynomial[Index + 1 + GeneratorIndex] = (byte)(Polynomial[Index + 1 + GeneratorIndex] ^ StaticTables.ExpToInt[Generator[GeneratorIndex] + Multiplier]);
				}
			}
		}

		private static int Multiply(int Int1, int Int2)
		{
			return (Int1 == 0 || Int2 == 0) ? 0 : StaticTables.ExpToInt[StaticTables.IntToExp[Int1] + StaticTables.IntToExp[Int2]];
		}

		private static int MultiplyIntByExp(int Int, int Exp)
		{
			return Int == 0 ? 0 : StaticTables.ExpToInt[StaticTables.IntToExp[Int] + Exp];
		}

		private static int MultiplyDivide(int Int1, int Int2, int Int3)
		{
			return (Int1 == 0 || Int2 == 0) ? 0 : StaticTables.ExpToInt[(StaticTables.IntToExp[Int1] + StaticTables.IntToExp[Int2] - StaticTables.IntToExp[Int3] + 255) % 255];
		}

		private static int DivideIntByExp(int Int, int Exp)
		{
			return Int == 0 ? 0 : StaticTables.ExpToInt[StaticTables.IntToExp[Int] - Exp + 255];
		}

		private static void PolynomialMultiply(int[] Result, int[] Poly1, int[] Poly2)
		{
			Array.Clear(Result, 0, Result.Length);
			for (int Index1 = 0; Index1 < Poly1.Length; Index1++)
			{
				if (Poly1[Index1] == 0) continue;
				int loga = StaticTables.IntToExp[Poly1[Index1]];
				int Index2End = Math.Min(Poly2.Length, Result.Length - Index1);
				// = Sum(Poly1[Index1] * Poly2[Index2]) for all Index2
				for (int Index2 = 0; Index2 < Index2End; Index2++)
					if (Poly2[Index2] != 0) Result[Index1 + Index2] ^= StaticTables.ExpToInt[loga + StaticTables.IntToExp[Poly2[Index2]]];
			}
			return;
		}
	}

	internal class StaticTables
	{
		// alignment symbols position as function of dimension
		internal static readonly byte[][] AlignmentPositionArray =
		{
			null,
			null,
			new byte[] {  6,  18},
			new byte[] {  6,  22},
			new byte[] {  6,  26},
			new byte[] {  6,  30},
			new byte[] {  6,  34},
			new byte[] {  6,  22,  38},
			new byte[] {  6,  24,  42},
			new byte[] {  6,  26,  46},
			new byte[] {  6,  28,  50},
			new byte[] {  6,  30,  54},
			new byte[] {  6,  32,  58},
			new byte[] {  6,  34,  62},
			new byte[] {  6,  26,  46,  66},
			new byte[] {  6,  26,  48,  70},
			new byte[] {  6,  26,  50,  74},
			new byte[] {  6,  30,  54,  78},
			new byte[] {  6,  30,  56,  82},
			new byte[] {  6,  30,  58,  86},
			new byte[] {  6,  34,  62,  90},
			new byte[] {  6,  28,  50,  72,  94},
			new byte[] {  6,  26,  50,  74,  98},
			new byte[] {  6,  30,  54,  78, 102},
			new byte[] {  6,  28,  54,  80, 106},
			new byte[] {  6,  32,  58,  84, 110},
			new byte[] {  6,  30,  58,  86, 114},
			new byte[] {  6,  34,  62,  90, 118},
			new byte[] {  6,  26,  50,  74,  98, 122},
			new byte[] {  6,  30,  54,  78, 102, 126},
			new byte[] {  6,  26,  52,  78, 104, 130},
			new byte[] {  6,  30,  56,  82, 108, 134},
			new byte[] {  6,  34,  60,  86, 112, 138},
			new byte[] {  6,  30,  58,  86, 114, 142},
			new byte[] {  6,  34,  62,  90, 118, 146},
			new byte[] {  6,  30,  54,  78, 102, 126, 150},
			new byte[] {  6,  24,  50,  76, 102, 128, 154},
			new byte[] {  6,  28,  54,  80, 106, 132, 158},
			new byte[] {  6,  32,  58,  84, 110, 136, 162},
			new byte[] {  6,  26,  54,  82, 110, 138, 166},
			new byte[] {  6,  30,  58,  86, 114, 142, 170},
		};

		// maximum code words as function of dimension
		internal static readonly int[] MaxCodewordsArray =
		{
			0,
			26,   44,   70,  100,  134,  172,  196,  242,  292,  346,
			404,  466,  532,  581,  655,  733,  815,  901,  991, 1085,
			1156, 1258, 1364, 1474, 1588, 1706, 1828, 1921, 2051, 2185,
			2323, 2465, 2611, 2761, 2876, 3034, 3196, 3362, 3532, 3706
		};

		// Encodable character set:
		// 1) numeric data (digits 0 - 9);
		// 2) alphanumeric data (digits 0 - 9; upper case letters A -Z; nine other characters: space, $ % * + - . / : );
		// 3) 8-bit byte data (JIS 8-bit character set (Latin and Kana) in accordance with JIS X 0201);
		// 4) Kanji characters (Shift JIS character set in accordance with JIS X 0208 Annex 1 Shift Coded
		//    Representation. Note that Kanji characters in QR Code can have values 8140HEX -9FFCHEX and E040HEX -
		//    EBBFHEX , which can be compacted into 13 bits.)
		internal static readonly byte[] EncodingTable =
		{
			45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45,
			45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45,
			36, 45, 45, 45, 37, 38, 45, 45, 45, 45, 39, 40, 45, 41, 42, 43,
			0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 44, 45, 45, 45, 45, 45,
			 45, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
			 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 45, 45, 45, 45, 45,
			 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45,
			 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45,
			 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45,
			 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45,
			 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45,
			 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45,
			 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45,
			 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45,
			 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45,
			 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45,
		};

		internal static readonly byte[] DecodingTable =
		{
			(byte) '0',	// 0
			(byte) '1',	// 1
			(byte) '2',	// 2
			(byte) '3',	// 3
			(byte) '4',	// 4
			(byte) '5',	// 5
			(byte) '6',	// 6
			(byte) '7',	// 7
			(byte) '8',	// 8
			(byte) '9',	// 9
			(byte) 'A',	// 10
			(byte) 'B',	// 11
			(byte) 'C',	// 12
			(byte) 'D',	// 13
			(byte) 'E',	// 14
			(byte) 'F',	// 15
			(byte) 'G',	// 16
			(byte) 'H',	// 17
			(byte) 'I',	// 18
			(byte) 'J',	// 19
			(byte) 'K',	// 20
			(byte) 'L',	// 21
			(byte) 'M',	// 22
			(byte) 'N',	// 23
			(byte) 'O',	// 24
			(byte) 'P',	// 25
			(byte) 'Q',	// 26
			(byte) 'R',	// 27
			(byte) 'S',	// 28
			(byte) 'T',	// 29
			(byte) 'U',	// 30
			(byte) 'V',	// 31
			(byte) 'W',	// 32
			(byte) 'X',	// 33
			(byte) 'Y',	// 34
			(byte) 'Z',	// 35
			(byte) ' ',	// 36 (space)
			(byte) '$',	// 37
			(byte) '%',	// 38
			(byte) '*',	// 39
			(byte) '+',	// 40
			(byte) '-',	// 41
			(byte) '.',	// 42
			(byte) '/',	// 43
			(byte) ':',	// 44
		};

		// Error correction block information
		// A-Number of blocks in group 1
		internal const int BLOCKS_GROUP1 = 0;
		// B-Number of data codewords in blocks of group 1
		internal const int DATA_CODEWORDS_GROUP1 = 1;
		// C-Number of blocks in group 2
		internal const int BLOCKS_GROUP2 = 2;
		// D-Number of data codewords in blocks of group 2
		internal const int DATA_CODEWORDS_GROUP2 = 3;

		internal static readonly byte[,] ECBlockInfo =
		{
			// A,   B,   C,   D 
			{  1,  19,   0,   0},	// 1-L
			{  1,  16,   0,   0},	// 1-M
			{  1,  13,   0,   0},	// 1-Q
			{  1,   9,   0,   0},	// 1-H
			{  1,  34,   0,   0},	// 2-L
			{  1,  28,   0,   0},	// 2-M
			{  1,  22,   0,   0},	// 2-Q
			{  1,  16,   0,   0},	// 2-H
			{  1,  55,   0,   0},	// 3-L
			{  1,  44,   0,   0},	// 3-M
			{  2,  17,   0,   0},	// 3-Q
			{  2,  13,   0,   0},	// 3-H
			{  1,  80,   0,   0},	// 4-L
			{  2,  32,   0,   0},	// 4-M
			{  2,  24,   0,   0},	// 4-Q
			{  4,   9,   0,   0},	// 4-H
			{  1, 108,   0,   0},	// 5-L
			{  2,  43,   0,   0},	// 5-M
			{  2,  15,   2,  16},	// 5-Q
			{  2,  11,   2,  12},	// 5-H
			{  2,  68,   0,   0},	// 6-L
			{  4,  27,   0,   0},	// 6-M
			{  4,  19,   0,   0},	// 6-Q
			{  4,  15,   0,   0},	// 6-H
			{  2,  78,   0,   0},	// 7-L
			{  4,  31,   0,   0},	// 7-M
			{  2,  14,   4,  15},	// 7-Q
			{  4,  13,   1,  14},	// 7-H
			{  2,  97,   0,   0},	// 8-L
			{  2,  38,   2,  39},	// 8-M
			{  4,  18,   2,  19},	// 8-Q
			{  4,  14,   2,  15},	// 8-H
			{  2, 116,   0,   0},	// 9-L
			{  3,  36,   2,  37},	// 9-M
			{  4,  16,   4,  17},	// 9-Q
			{  4,  12,   4,  13},	// 9-H
			{  2,  68,   2,  69},	// 10-L
			{  4,  43,   1,  44},	// 10-M
			{  6,  19,   2,  20},	// 10-Q
			{  6,  15,   2,  16},	// 10-H
			{  4,  81,   0,   0},	// 11-L
			{  1,  50,   4,  51},	// 11-M
			{  4,  22,   4,  23},	// 11-Q
			{  3,  12,   8,  13},	// 11-H
			{  2,  92,   2,  93},	// 12-L
			{  6,  36,   2,  37},	// 12-M
			{  4,  20,   6,  21},	// 12-Q
			{  7,  14,   4,  15},	// 12-H
			{  4, 107,   0,   0},	// 13-L
			{  8,  37,   1,  38},	// 13-M
			{  8,  20,   4,  21},	// 13-Q
			{ 12,  11,   4,  12},	// 13-H
			{  3, 115,   1, 116},	// 14-L
			{  4,  40,   5,  41},	// 14-M
			{ 11,  16,   5,  17},	// 14-Q
			{ 11,  12,   5,  13},	// 14-H
			{  5,  87,   1,  88},	// 15-L
			{  5,  41,   5,  42},	// 15-M
			{  5,  24,   7,  25},	// 15-Q
			{ 11,  12,   7,  13},	// 15-H
			{  5,  98,   1,  99},	// 16-L
			{  7,  45,   3,  46},	// 16-M
			{ 15,  19,   2,  20},	// 16-Q
			{  3,  15,  13,  16},	// 16-H
			{  1, 107,   5, 108},	// 17-L
			{ 10,  46,   1,  47},	// 17-M
			{  1,  22,  15,  23},	// 17-Q
			{  2,  14,  17,  15},	// 17-H
			{  5, 120,   1, 121},	// 18-L
			{  9,  43,   4,  44},	// 18-M
			{ 17,  22,   1,  23},	// 18-Q
			{  2,  14,  19,  15},	// 18-H
			{  3, 113,   4, 114},	// 19-L
			{  3,  44,  11,  45},	// 19-M
			{ 17,  21,   4,  22},	// 19-Q
			{  9,  13,  16,  14},	// 19-H
			{  3, 107,   5, 108},	// 20-L
			{  3,  41,  13,  42},	// 20-M
			{ 15,  24,   5,  25},	// 20-Q
			{ 15,  15,  10,  16},	// 20-H
			{  4, 116,   4, 117},	// 21-L
			{ 17,  42,   0,   0},	// 21-M
			{ 17,  22,   6,  23},	// 21-Q
			{ 19,  16,   6,  17},	// 21-H
			{  2, 111,   7, 112},	// 22-L
			{ 17,  46,   0,   0},	// 22-M
			{  7,  24,  16,  25},	// 22-Q
			{ 34,  13,   0,   0},	// 22-H
			{  4, 121,   5, 122},	// 23-L
			{  4,  47,  14,  48},	// 23-M
			{ 11,  24,  14,  25},	// 23-Q
			{ 16,  15,  14,  16},	// 23-H
			{  6, 117,   4, 118},	// 24-L
			{  6,  45,  14,  46},	// 24-M
			{ 11,  24,  16,  25},	// 24-Q
			{ 30,  16,   2,  17},	// 24-H
			{  8, 106,   4, 107},	// 25-L
			{  8,  47,  13,  48},	// 25-M
			{  7,  24,  22,  25},	// 25-Q
			{ 22,  15,  13,  16},	// 25-H
			{ 10, 114,   2, 115},	// 26-L
			{ 19,  46,   4,  47},	// 26-M
			{ 28,  22,   6,  23},	// 26-Q
			{ 33,  16,   4,  17},	// 26-H
			{  8, 122,   4, 123},	// 27-L
			{ 22,  45,   3,  46},	// 27-M
			{  8,  23,  26,  24},	// 27-Q
			{ 12,  15,  28,  16},	// 27-H
			{  3, 117,  10, 118},	// 28-L
			{  3,  45,  23,  46},	// 28-M
			{  4,  24,  31,  25},	// 28-Q
			{ 11,  15,  31,  16},	// 28-H
			{  7, 116,   7, 117},	// 29-L
			{ 21,  45,   7,  46},	// 29-M
			{  1,  23,  37,  24},	// 29-Q
			{ 19,  15,  26,  16},	// 29-H
			{  5, 115,  10, 116},	// 30-L
			{ 19,  47,  10,  48},	// 30-M
			{ 15,  24,  25,  25},	// 30-Q
			{ 23,  15,  25,  16},	// 30-H
			{ 13, 115,   3, 116},	// 31-L
			{  2,  46,  29,  47},	// 31-M
			{ 42,  24,   1,  25},	// 31-Q
			{ 23,  15,  28,  16},	// 31-H
			{ 17, 115,   0,   0},	// 32-L
			{ 10,  46,  23,  47},	// 32-M
			{ 10,  24,  35,  25},	// 32-Q
			{ 19,  15,  35,  16},	// 32-H
			{ 17, 115,   1, 116},	// 33-L
			{ 14,  46,  21,  47},	// 33-M
			{ 29,  24,  19,  25},	// 33-Q
			{ 11,  15,  46,  16},	// 33-H
			{ 13, 115,   6, 116},	// 34-L
			{ 14,  46,  23,  47},	// 34-M
			{ 44,  24,   7,  25},	// 34-Q
			{ 59,  16,   1,  17},	// 34-H
			{ 12, 121,   7, 122},	// 35-L
			{ 12,  47,  26,  48},	// 35-M
			{ 39,  24,  14,  25},	// 35-Q
			{ 22,  15,  41,  16},	// 35-H
			{  6, 121,  14, 122},	// 36-L
			{  6,  47,  34,  48},	// 36-M
			{ 46,  24,  10,  25},	// 36-Q
			{  2,  15,  64,  16},	// 36-H
			{ 17, 122,   4, 123},	// 37-L
			{ 29,  46,  14,  47},	// 37-M
			{ 49,  24,  10,  25},	// 37-Q
			{ 24,  15,  46,  16},	// 37-H
			{  4, 122,  18, 123},	// 38-L
			{ 13,  46,  32,  47},	// 38-M
			{ 48,  24,  14,  25},	// 38-Q
			{ 42,  15,  32,  16},	// 38-H
			{ 20, 117,   4, 118},	// 39-L
			{ 40,  47,   7,  48},	// 39-M
			{ 43,  24,  22,  25},	// 39-Q
			{ 10,  15,  67,  16},	// 39-H
			{ 19, 118,   6, 119},	// 40-L
			{ 18,  47,  31,  48},	// 40-M
			{ 34,  24,  34,  25},	// 40-Q
			{ 20,  15,  61,  16},	// 40-H
			};

		internal static readonly byte[] Generator7 = { 87, 229, 146, 149, 238, 102, 21 };
		internal static readonly byte[] Generator10 = { 251, 67, 46, 61, 118, 70, 64, 94, 32, 45 };
		internal static readonly byte[] Generator13 = { 74, 152, 176, 100, 86, 100, 106, 104, 130, 218, 206, 140, 78 };
		internal static readonly byte[] Generator15 = { 8, 183, 61, 91, 202, 37, 51, 58, 58, 237, 140, 124, 5, 99, 105 };
		internal static readonly byte[] Generator16 = { 120, 104, 107, 109, 102, 161,  76,   3,  91, 191, 147, 169, 182, 194, 225, 120};
		internal static readonly byte[] Generator17 = {  43, 139, 206,  78,  43, 239, 123, 206, 214, 147,  24,  99, 150,  39, 243, 163, 136};
		internal static readonly byte[] Generator18 = { 215, 234, 158,  94, 184,  97, 118, 170,  79, 187, 152, 148, 252, 179,   5,  98, 96, 153};
		internal static readonly byte[] Generator20 = {  17,  60,  79,  50,  61, 163,  26, 187, 202, 180, 221, 225,  83, 239, 156, 164, 212, 212, 188, 190};
		internal static readonly byte[] Generator22 =
			{ 210, 171, 247, 242,  93, 230,  14, 109, 221,  53, 200,  74,   8, 172,  98,  80,
			219, 134, 160, 105, 165, 231};
		internal static readonly byte[] Generator24 =
			{ 229, 121, 135,  48, 211, 117, 251, 126, 159, 180, 169, 152, 192, 226, 228, 218,
			111,   0, 117, 232,  87,  96, 227,  21};
		internal static readonly byte[] Generator26 =
			{ 173, 125, 158,   2, 103, 182, 118,  17, 145, 201, 111,  28, 165,  53, 161,  21,
			245, 142,  13, 102,  48, 227, 153, 145, 218,  70};
		internal static readonly byte[] Generator28 =
			{ 168, 223, 200, 104, 224, 234, 108, 180, 110, 190, 195, 147, 205,  27, 232, 201,
			 21,  43, 245,  87,  42, 195, 212, 119, 242,  37,   9, 123};
		internal static readonly byte[] Generator30 =
			{  41, 173, 145, 152, 216,  31, 179, 182,  50,  48, 110,  86, 239,  96, 222, 125,
			 42, 173, 226, 193, 224, 130, 156,  37, 251, 216, 238,  40, 192, 180};
		internal static readonly byte[] Generator32 =
			{  10,   6, 106, 190, 249, 167,   4,  67, 209, 138, 138,  32, 242, 123,  89,  27,
			120, 185,  80, 156,  38,  60, 171,  60,  28, 222,  80,  52, 254, 185, 220, 241};
		internal static readonly byte[] Generator34 =
			{ 111,  77, 146,  94,  26,  21, 108,  19, 105,  94, 113, 193,  86, 140, 163, 125,
			 58, 158, 229, 239, 218, 103,  56,  70, 114,  61, 183, 129, 167,  13,  98,  62,
			129,  51};
		internal static readonly byte[] Generator36 =
			{ 200, 183,  98,  16, 172,  31, 246, 234,  60, 152, 115,   0, 167, 152, 113, 248,
			238, 107,  18,  63, 218,  37,  87, 210, 105, 177, 120,  74, 121, 196, 117, 251,
			113, 233,  30, 120};
		internal static readonly byte[] Generator40 =
			{  59, 116,  79, 161, 252,  98, 128, 205, 128, 161, 247,  57, 163,  56, 235, 106,
			 53,  26, 187, 174, 226, 104, 170,   7, 175,  35, 181, 114,  88,  41,  47, 163,
			125, 134,  72,  20, 232,  53,  35,  15};
		internal static readonly byte[] Generator42 =
			{ 250, 103, 221, 230,  25,  18, 137, 231,   0,   3,  58, 242, 221, 191, 110,  84,
			230,   8, 188, 106,  96, 147,  15, 131, 139,  34, 101, 223,  39, 101, 213, 199,
			237, 254, 201, 123, 171, 162, 194, 117,  50,  96};
		internal static readonly byte[] Generator44 =
			{ 190,   7,  61, 121,  71, 246,  69,  55, 168, 188,  89, 243, 191,  25,  72, 123,
				9, 145,  14, 247,   1, 238,  44,  78, 143,  62, 224, 126, 118, 114,  68, 163,
			 52, 194, 217, 147, 204, 169,  37, 130, 113, 102,  73, 181};
		internal static readonly byte[] Generator46 =
			{ 112,  94,  88, 112, 253, 224, 202, 115, 187,  99,  89,   5,  54, 113, 129,  44,
			 58,  16, 135, 216, 169, 211,  36,   1,   4,  96,  60, 241,  73, 104, 234,   8,
			249, 245, 119, 174,  52,  25, 157, 224,  43, 202, 223,  19,  82,  15};
		internal static readonly byte[] Generator48 =
			{ 228,  25, 196, 130, 211, 146,  60,  24, 251,  90,  39, 102, 240,  61, 178,  63,
			 46, 123, 115,  18, 221, 111, 135, 160, 182, 205, 107, 206,  95, 150, 120, 184,
			 91,  21, 247, 156, 140, 238, 191,  11,  94, 227,  84,  50, 163,  39,  34, 108};
		internal static readonly byte[] Generator50 =
			{ 232, 125, 157, 161, 164,   9, 118,  46, 209,  99, 203, 193,  35,   3, 209, 111,
			195, 242, 203, 225,  46,  13,  32, 160, 126, 209, 130, 160, 242, 215, 242,  75,
			 77,  42, 189,  32, 113,  65, 124,  69, 228, 114, 235, 175, 124, 170, 215, 232,
			133, 205};
		internal static readonly byte[] Generator52 =
			{ 116,  50,  86, 186,  50, 220, 251,  89, 192,  46,  86, 127, 124,  19, 184, 233,
			151, 215,  22,  14,  59, 145,  37, 242, 203, 134, 254,  89, 190,  94,  59,  65,
			124, 113, 100, 233, 235, 121,  22,  76,  86,  97,  39, 242, 200, 220, 101,  33,
			239, 254, 116,  51};
		internal static readonly byte[] Generator54 =
			{ 183,  26, 201,  84, 210, 221, 113,  21,  46,  65,  45,  50, 238, 184, 249, 225,
			102,  58, 209, 218, 109, 165,  26,  95, 184, 192,  52, 245,  35, 254, 238, 175,
			172,  79, 123,  25, 122,  43, 120, 108, 215,  80, 128, 201, 235,   8, 153,  59,
			101,  31, 198,  76,  31, 156};
		internal static readonly byte[] Generator56 =
			{ 106, 120, 107, 157, 164, 216, 112, 116,   2,  91, 248, 163,  36, 201, 202, 229,
				6, 144, 254, 155, 135, 208, 170, 209,  12, 139, 127, 142, 182, 249, 177, 174,
			190,  28,  10,  85, 239, 184, 101, 124, 152, 206,  96,  23, 163,  61,  27, 196,
			247, 151, 154, 202, 207,  20,  61,  10};
		internal static readonly byte[] Generator58 =
			{  82, 116,  26, 247,  66,  27,  62, 107, 252, 182, 200, 185, 235,  55, 251, 242,
			210, 144, 154, 237, 176, 141, 192, 248, 152, 249, 206,  85, 253, 142,  65, 165,
			125,  23,  24,  30, 122, 240, 214,   6, 129, 218,  29, 145, 127, 134, 206, 245,
			117,  29,  41,  63, 159, 142, 233, 125, 148, 123};
		internal static readonly byte[] Generator60 =
			{ 107, 140,  26,  12,   9, 141, 243, 197, 226, 197, 219,  45, 211, 101, 219, 120,
			 28, 181, 127,   6, 100, 247,   2, 205, 198,  57, 115, 219, 101, 109, 160,  82,
			 37,  38, 238,  49, 160, 209, 121,  86,  11, 124,  30, 181,  84,  25, 194,  87,
			 65, 102, 190, 220,  70,  27, 209,  16,  89,   7,  33, 240};
		internal static readonly byte[] Generator62 =
			{  65, 202, 113,  98,  71, 223, 248, 118, 214,  94,   0, 122,  37,  23,   2, 228,
			 58, 121,   7, 105, 135,  78, 243, 118,  70,  76, 223,  89,  72,  50,  70, 111,
			194,  17, 212, 126, 181,  35, 221, 117, 235,  11, 229, 149, 147, 123, 213,  40,
			115,   6, 200, 100,  26, 246, 182, 218, 127, 215,  36, 186, 110, 106};
		internal static readonly byte[] Generator64 =
			{  45,  51, 175,   9,   7, 158, 159,  49,  68, 119,  92, 123, 177, 204, 187, 254,
			200,  78, 141, 149, 119,  26, 127,  53, 160,  93, 199, 212,  29,  24, 145, 156,
			208, 150, 218, 209,   4, 216,  91,  47, 184, 146,  47, 140, 195, 195, 125, 242,
			238,  63,  99, 108, 140, 230, 242,  31, 204,  11, 178, 243, 217, 156, 213, 231};
		internal static readonly byte[] Generator66 =
			{   5, 118, 222, 180, 136, 136, 162,  51,  46, 117,  13, 215,  81,  17, 139, 247,
			197, 171,  95, 173,  65, 137, 178,  68, 111,  95, 101,  41,  72, 214, 169, 197,
			 95,   7,  44, 154,  77, 111, 236,  40, 121, 143,  63,  87,  80, 253, 240, 126,
			217,  77,  34, 232, 106,  50, 168,  82,  76, 146,  67, 106, 171,  25, 132,  93,
			 45, 105};
		internal static readonly byte[] Generator68 =
			{ 247, 159, 223,  33, 224,  93,  77,  70,  90, 160,  32, 254,  43, 150,  84, 101,
			190, 205, 133,  52,  60, 202, 165, 220, 203, 151,  93,  84,  15,  84, 253, 173,
			160,  89, 227,  52, 199,  97,  95, 231,  52, 177,  41, 125, 137, 241, 166, 225,
			118,   2,  54,  32,  82, 215, 175, 198,  43, 238, 235,  27, 101, 184, 127,   3,
				5,   8, 163, 238};

		internal static readonly byte[][] GenArray =
			{ Generator7, null, null, Generator10, null, null, Generator13, null, Generator15, Generator16,
			Generator17, Generator18, null, Generator20, null, Generator22, null, Generator24, null, Generator26,
			null, Generator28, null, Generator30, null, Generator32, null, Generator34, null, Generator36,
			null, null, null, Generator40, null, Generator42, null, Generator44, null, Generator46,
			null, Generator48, null, Generator50, null, Generator52, null, Generator54, null, Generator56,
			null, Generator58, null, Generator60, null, Generator62, null, Generator64, null, Generator66,
			null, Generator68};

		internal static readonly byte[] ExpToInt = //	ExpToInt =
				{
				 1,   2,   4,   8,  16,  32,  64, 128,  29,  58, 116, 232, 205, 135,  19,  38,
				76, 152,  45,  90, 180, 117, 234, 201, 143,   3,   6,  12,  24,  48,  96, 192,
			 157,  39,  78, 156,  37,  74, 148,  53, 106, 212, 181, 119, 238, 193, 159,  35,
				70, 140,   5,  10,  20,  40,  80, 160,  93, 186, 105, 210, 185, 111, 222, 161,
				95, 190,  97, 194, 153,  47,  94, 188, 101, 202, 137,  15,  30,  60, 120, 240,
			 253, 231, 211, 187, 107, 214, 177, 127, 254, 225, 223, 163,  91, 182, 113, 226,
			 217, 175,  67, 134,  17,  34,  68, 136,  13,  26,  52, 104, 208, 189, 103, 206,
			 129,  31,  62, 124, 248, 237, 199, 147,  59, 118, 236, 197, 151,  51, 102, 204,
			 133,  23,  46,  92, 184, 109, 218, 169,  79, 158,  33,  66, 132,  21,  42,  84,
			 168,  77, 154,  41,  82, 164,  85, 170,  73, 146,  57, 114, 228, 213, 183, 115,
			 230, 209, 191,  99, 198, 145,  63, 126, 252, 229, 215, 179, 123, 246, 241, 255,
			 227, 219, 171,  75, 150,  49,  98, 196, 149,  55, 110, 220, 165,  87, 174,  65,
			 130,  25,  50, 100, 200, 141,   7,  14,  28,  56, 112, 224, 221, 167,  83, 166,
				81, 162,  89, 178, 121, 242, 249, 239, 195, 155,  43,  86, 172,  69, 138,   9,
				18,  36,  72, 144,  61, 122, 244, 245, 247, 243, 251, 235, 203, 139,  11,  22,
				44,  88, 176, 125, 250, 233, 207, 131,  27,  54, 108, 216, 173,  71, 142,   1,

							2,   4,   8,  16,  32,  64, 128,  29,  58, 116, 232, 205, 135,  19,  38,
				76, 152,  45,  90, 180, 117, 234, 201, 143,   3,   6,  12,  24,  48,  96, 192,
			 157,  39,  78, 156,  37,  74, 148,  53, 106, 212, 181, 119, 238, 193, 159,  35,
				70, 140,   5,  10,  20,  40,  80, 160,  93, 186, 105, 210, 185, 111, 222, 161,
				95, 190,  97, 194, 153,  47,  94, 188, 101, 202, 137,  15,  30,  60, 120, 240,
			 253, 231, 211, 187, 107, 214, 177, 127, 254, 225, 223, 163,  91, 182, 113, 226,
			 217, 175,  67, 134,  17,  34,  68, 136,  13,  26,  52, 104, 208, 189, 103, 206,
			 129,  31,  62, 124, 248, 237, 199, 147,  59, 118, 236, 197, 151,  51, 102, 204,
			 133,  23,  46,  92, 184, 109, 218, 169,  79, 158,  33,  66, 132,  21,  42,  84,
			 168,  77, 154,  41,  82, 164,  85, 170,  73, 146,  57, 114, 228, 213, 183, 115,
			 230, 209, 191,  99, 198, 145,  63, 126, 252, 229, 215, 179, 123, 246, 241, 255,
			 227, 219, 171,  75, 150,  49,  98, 196, 149,  55, 110, 220, 165,  87, 174,  65,
			 130,  25,  50, 100, 200, 141,   7,  14,  28,  56, 112, 224, 221, 167,  83, 166,
				81, 162,  89, 178, 121, 242, 249, 239, 195, 155,  43,  86, 172,  69, 138,   9,
				18,  36,  72, 144,  61, 122, 244, 245, 247, 243, 251, 235, 203, 139,  11,  22,
				44,  88, 176, 125, 250, 233, 207, 131,  27,  54, 108, 216, 173,  71, 142,   1
			};

		internal static readonly byte[] IntToExp = //	IntToExp =
				{
				 0,   0,   1,  25,   2,  50,  26, 198,   3, 223,  51, 238,  27, 104, 199,  75,
				 4, 100, 224,  14,  52, 141, 239, 129,  28, 193, 105, 248, 200,   8,  76, 113,
				 5, 138, 101,  47, 225,  36,  15,  33,  53, 147, 142, 218, 240,  18, 130,  69,
				29, 181, 194, 125, 106,  39, 249, 185, 201, 154,   9, 120,  77, 228, 114, 166,
				 6, 191, 139,  98, 102, 221,  48, 253, 226, 152,  37, 179,  16, 145,  34, 136,
				54, 208, 148, 206, 143, 150, 219, 189, 241, 210,  19,  92, 131,  56,  70,  64,
				30,  66, 182, 163, 195,  72, 126, 110, 107,  58,  40,  84, 250, 133, 186,  61,
			 202,  94, 155, 159,  10,  21, 121,  43,  78, 212, 229, 172, 115, 243, 167,  87,
				 7, 112, 192, 247, 140, 128,  99,  13, 103,  74, 222, 237,  49, 197, 254,  24,
			 227, 165, 153, 119,  38, 184, 180, 124,  17,  68, 146, 217,  35,  32, 137,  46,
				55,  63, 209,  91, 149, 188, 207, 205, 144, 135, 151, 178, 220, 252, 190,  97,
			 242,  86, 211, 171,  20,  42,  93, 158, 132,  60,  57,  83,  71, 109,  65, 162,
				31,  45,  67, 216, 183, 123, 164, 118, 196,  23,  73, 236, 127,  12, 111, 246,
			 108, 161,  59,  82,  41, 157,  85, 170, 251,  96, 134, 177, 187, 204,  62,  90,
			 203,  89,  95, 176, 156, 169, 160,  81,  11, 245,  22, 235, 122, 117,  44, 215,
				79, 174, 213, 233, 230, 231, 173, 232, 116, 214, 244, 234, 168,  80,  88, 175
			};

		internal static readonly int[] FormatInfoArray =
				{
			0x5412, 0x5125, 0x5E7C, 0x5B4B, 0x45F9, 0x40CE, 0x4F97, 0x4AA0,		// M = 00
			0x77C4, 0x72F3, 0x7DAA, 0x789D, 0x662F, 0x6318, 0x6C41, 0x6976,		// L = 01
			0x1689, 0x13BE, 0x1CE7, 0x19D0,  0x762,  0x255,  0xD0C,  0x83B,		// H - 10
			0x355F, 0x3068, 0x3F31, 0x3A06, 0x24B4, 0x2183, 0x2EDA, 0x2BED,		// Q = 11
			};

		internal static readonly int[,] FormatInfoOne = new int[,]
				{
			{0, 8}, {1, 8}, {2, 8}, {3, 8}, {4, 8}, {5, 8}, {7, 8}, {8, 8},
			{8, 7}, {8, 5}, {8, 4}, {8, 3}, {8, 2}, {8, 1}, {8, 0}
				};

		internal static readonly int[,] FormatInfoTwo = new int[,]
				{
			{8, -1}, {8, -2}, {8, -3}, {8, -4}, {8, -5}, {8, -6}, {8, -7}, {8, -8},
			{-7, 8}, {-6, 8}, {-5, 8}, {-4, 8}, {-3, 8}, {-2, 8}, {-1, 8}
				};

		internal static readonly int[] VersionCodeArray =
				{
			 0x7c94,  0x85bc,  0x9a99,  0xa4d3,  0xbbf6,  0xc762,  0xd847,  0xe60d,  0xf928, 0x10b78,
			0x1145d, 0x12a17, 0x13532, 0x149a6, 0x15683, 0x168c9, 0x177ec, 0x18ec4, 0x191e1, 0x1afab,
			0x1b08e, 0x1cc1a, 0x1d33f, 0x1ed75, 0x1f250, 0x209d5, 0x216f0, 0x228ba, 0x2379f, 0x24b0b,
			0x2542e, 0x26a64, 0x27541, 0x28c69
			};

		internal const byte White = 0;
		internal const byte Black = 1;
		internal const byte NonData = 2;
		internal const byte Fixed = 4;
		internal const byte DataWhite = White;
		internal const byte DataBlack = Black;
		internal const byte FormatWhite = NonData | White;
		internal const byte FormatBlack = NonData | Black;
		internal const byte FixedWhite = Fixed | NonData | White;
		internal const byte FixedBlack = Fixed | NonData | Black;

		internal static readonly byte[,] FinderPatternTopLeft =
			{
		{FixedBlack,  FixedBlack,  FixedBlack,  FixedBlack,  FixedBlack,  FixedBlack,  FixedBlack,  FixedWhite,  FormatWhite},
		{FixedBlack,  FixedWhite,  FixedWhite,  FixedWhite,  FixedWhite,  FixedWhite,  FixedBlack,  FixedWhite,  FormatWhite},
		{FixedBlack,  FixedWhite,  FixedBlack,  FixedBlack,  FixedBlack,  FixedWhite,  FixedBlack,  FixedWhite,  FormatWhite},
		{FixedBlack,  FixedWhite,  FixedBlack,  FixedBlack,  FixedBlack,  FixedWhite,  FixedBlack,  FixedWhite,  FormatWhite},
		{FixedBlack,  FixedWhite,  FixedBlack,  FixedBlack,  FixedBlack,  FixedWhite,  FixedBlack,  FixedWhite,  FormatWhite},
		{FixedBlack,  FixedWhite,  FixedWhite,  FixedWhite,  FixedWhite,  FixedWhite,  FixedBlack,  FixedWhite,  FormatWhite},
		{FixedBlack,  FixedBlack,  FixedBlack,  FixedBlack,  FixedBlack,  FixedBlack,  FixedBlack,  FixedWhite,  FormatWhite},
		{FixedWhite,  FixedWhite,  FixedWhite,  FixedWhite,  FixedWhite,  FixedWhite,  FixedWhite,  FixedWhite,  FormatWhite},
		{FormatWhite, FormatWhite, FormatWhite, FormatWhite, FormatWhite, FormatWhite, FormatWhite, FormatWhite, FormatWhite},
		};

		internal static readonly byte[,] FinderPatternTopRight =
			{
		{FixedWhite,  FixedBlack,  FixedBlack,  FixedBlack,  FixedBlack,  FixedBlack,  FixedBlack,  FixedBlack},
		{FixedWhite,  FixedBlack,  FixedWhite,  FixedWhite,  FixedWhite,  FixedWhite,  FixedWhite,  FixedBlack},
		{FixedWhite,  FixedBlack,  FixedWhite,  FixedBlack,  FixedBlack,  FixedBlack,  FixedWhite,  FixedBlack},
		{FixedWhite,  FixedBlack,  FixedWhite,  FixedBlack,  FixedBlack,  FixedBlack,  FixedWhite,  FixedBlack},
		{FixedWhite,  FixedBlack,  FixedWhite,  FixedBlack,  FixedBlack,  FixedBlack,  FixedWhite,  FixedBlack},
		{FixedWhite,  FixedBlack,  FixedWhite,  FixedWhite,  FixedWhite,  FixedWhite,  FixedWhite,  FixedBlack},
		{FixedWhite,  FixedBlack,  FixedBlack,  FixedBlack,  FixedBlack,  FixedBlack,  FixedBlack,  FixedBlack},
		{FixedWhite,  FixedWhite,  FixedWhite,  FixedWhite,  FixedWhite,  FixedWhite,  FixedWhite,  FixedWhite},
		{FormatWhite, FormatWhite, FormatWhite, FormatWhite, FormatWhite, FormatWhite, FormatWhite, FormatWhite},
		};

		internal static readonly byte[,] FinderPatternBottomLeft =
			{
		{FixedWhite, FixedWhite, FixedWhite, FixedWhite, FixedWhite, FixedWhite, FixedWhite, FixedWhite, FixedBlack},
		{FixedBlack, FixedBlack, FixedBlack, FixedBlack, FixedBlack, FixedBlack, FixedBlack, FixedWhite, FormatWhite},
		{FixedBlack, FixedWhite, FixedWhite, FixedWhite, FixedWhite, FixedWhite, FixedBlack, FixedWhite, FormatWhite},
		{FixedBlack, FixedWhite, FixedBlack, FixedBlack, FixedBlack, FixedWhite, FixedBlack, FixedWhite, FormatWhite},
		{FixedBlack, FixedWhite, FixedBlack, FixedBlack, FixedBlack, FixedWhite, FixedBlack, FixedWhite, FormatWhite},
		{FixedBlack, FixedWhite, FixedBlack, FixedBlack, FixedBlack, FixedWhite, FixedBlack, FixedWhite, FormatWhite},
		{FixedBlack, FixedWhite, FixedWhite, FixedWhite, FixedWhite, FixedWhite, FixedBlack, FixedWhite, FormatWhite},
		{FixedBlack, FixedBlack, FixedBlack, FixedBlack, FixedBlack, FixedBlack, FixedBlack, FixedWhite, FormatWhite},
		};

		internal static readonly byte[,] AlignmentPattern =
			{
		{FixedBlack, FixedBlack, FixedBlack, FixedBlack, FixedBlack},
		{FixedBlack, FixedWhite, FixedWhite, FixedWhite, FixedBlack},
		{FixedBlack, FixedWhite, FixedBlack, FixedWhite, FixedBlack},
		{FixedBlack, FixedWhite, FixedWhite, FixedWhite, FixedBlack},
		{FixedBlack, FixedBlack, FixedBlack, FixedBlack, FixedBlack},
		};
	}
}