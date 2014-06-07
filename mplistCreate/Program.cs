using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using System.IO;
using Microsoft.VisualBasic;

namespace mplistCreate
{
	class Program
	{
		private const string JSMPLISTFILE = "mplist";
		private const int RECORDSIZE = 320;	// 0x140
		private const int FILENAMEOFFSET = 0x0;
		private const int FILENAMELENGTH = 256;
		private const int TITLEOFFSET = 0x100;
		private const int TITLELENGTH = 24;
		private const int ARTISTNAMEOFFSET = 0x11A;
		private const int ARTISTNAMELENGTH = 24;
		private const int SONGNUMBEROFFSET = 0x13E;
		private const int SONGNUMBERLENGTH = 1;

		static void Main(string[] args)
		{
			if (1 != args.Length)
			{
				printUsage();
				return;
			}

			string filePath = args[0];
			string outPath = Directory.GetCurrentDirectory() + "\\" + JSMPLISTFILE;
			byte[] buffer = new byte[RECORDSIZE];
			string fileName, title, artistName;
			int songNumber = 0;

			using (FileStream fso = File.Open(outPath, FileMode.Create))
			{
				using (TextFieldParser parser = new TextFieldParser(filePath, Encoding.GetEncoding(932)))
				{
					parser.TextFieldType = FieldType.Delimited;
					parser.SetDelimiters(",");

					while (!parser.EndOfData)
					{
						string[] row = parser.ReadFields();
						if (4 != row.Length)
						{
							continue;
						}

						if (!int.TryParse(row[0], out songNumber))
						{
							continue;
						}

						if (100 < songNumber)
						{
							Console.WriteLine("100曲を超えています。");
							break;
						}
						title = Strings.StrConv(row[1], VbStrConv.Wide, 0);
						artistName = Strings.StrConv(row[2], VbStrConv.Wide, 0);
						fileName = row[3];

						constructBuffer(buffer, fileName, title, artistName, songNumber);

						fso.Write(buffer, 0, RECORDSIZE);
					}
				}
			}
		}

		private static void constructBuffer(byte[] buffer, string fileName, string title, string artistName, int songNumber)
		{
			Encoding cp932Encoding = Encoding.GetEncoding(932);
			byte[] fileNameBytes = new byte[FILENAMELENGTH];
			byte[] titleBytes = new byte[TITLELENGTH];
			byte[] artistNameBytes = new byte[ARTISTNAMELENGTH];
			byte[] songNumberBytes = { (byte)songNumber };

			Array.Copy(cp932Encoding.GetBytes(fileName), 0, fileNameBytes, 0, Math.Min(cp932Encoding.GetBytes(fileName).Length, FILENAMELENGTH));
			Array.Copy(cp932Encoding.GetBytes(title), 0, titleBytes, 0, Math.Min(cp932Encoding.GetBytes(title).Length, TITLELENGTH));
			Array.Copy(cp932Encoding.GetBytes(artistName), 0, artistNameBytes, 0, Math.Min(cp932Encoding.GetBytes(artistName).Length, ARTISTNAMELENGTH));

			Array.Clear(buffer, 0, buffer.Length);
			Array.Copy(fileNameBytes, 0, buffer, FILENAMEOFFSET, FILENAMELENGTH);
			Array.Copy(titleBytes, 0, buffer, TITLEOFFSET, TITLELENGTH);
			Array.Copy(artistNameBytes, 0, buffer, ARTISTNAMEOFFSET, ARTISTNAMELENGTH);
			Array.Copy(songNumberBytes, 0, buffer, SONGNUMBEROFFSET, SONGNUMBERLENGTH);

		}

		private static void printUsage()
		{
			Console.WriteLine("Usage: mplistCreate <<Path to CSV>>");
			Console.WriteLine("CSV Format Must be:");
			Console.WriteLine("NO,タイトル,アーティスト,ファイル名");
		}
	}
}
