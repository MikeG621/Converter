/*
 * Convert.exe, Up-converts mission formats between TIE, XvT and XWA
 * Copyright (C) 2005-2020 Michael Gaisser (mjgaisser@gmail.com)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL (License.txt) was not distributed
 * with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
 *
 * VERSION: 1.6
 */

/* CHANGELOG
 * v1.6, 200906
 * [NEW] Program.cs split out
 * [UPD] Error message, Arg handling
 */

using System;
using System.IO;
using System.Windows.Forms;

namespace Idmr.Converter
{
	static class Program
	{
		static readonly string _exceptionMessage = "Incorrect parameter usage. Correct usage is as follows:\n" +
									"<original path> <new path> (mode)\n" +
									"Modes: 1 - TIE to XvT, 2 - TIE/XvT/BoP to XWA, 4 - TIE to BoP\n" +
									"Mode is optional and ignored for XvT/BoP to XWA.";

		[STAThread]
#pragma warning disable IDE1006 // Naming Styles
		static void Main(string[] Args)
#pragma warning restore IDE1006 // Naming Styles
		{
			if (Args.Length > 0 && (Args.Length < 2 || Args.Length > 3)) // if args are detected but not the correct amount, treat as mis-use
			{
				MessageBox.Show(_exceptionMessage, "Error");
				return;
			}
			if (Args.Length > 0)
			{
				try
				{
					if (!File.Exists(Args[0])) throw new Exception("Cannot locate original file.");
					FileStream test = File.OpenRead(Args[0]);
					int d = test.ReadByte();    //check platform, really only make sure it's not XWA and legit
					test.Close();
					switch (d)
					{
						case 255:
						case 12:
						case 14:
							break;
						case 18:
							throw new Exception("Cannot convert existing XWA missions.");
						default:
							throw new Exception("Invalid source file");
					}
					if (Args.Length == 3)
					{
						string invalid = "Invalid conversion type for file specified.";
						switch (Args[2])        //check mode
						{
							case "1":
								if (d == 255) Processor.TieToXvt(Args[0], Args[1], false);
								else throw new Exception(invalid);
								break;
							case "2":
								if (d == 255) Processor.TieToXwa(Args[0], Args[1]);
								else if (d == 12 || d == 14) Processor.XvtToXwa(Args[0], Args[1], true);
								break;
							case "3":   // deprecated, original functionality unchanged
								if (d == 12 || d == 14) Processor.XvtToXwa(Args[0], Args[1], true);
								else throw new Exception(invalid);
								break;
							case "4":
								if (d == 255) Processor.TieToXvt(Args[0], Args[1], true);
								else throw new Exception(invalid);
								break;
							default:
								throw new Exception(_exceptionMessage);
						}
					}
					else if (d == 12 || d == 14) Processor.XvtToXwa(Args[0], Args[1], true);
					else throw new Exception(_exceptionMessage);
				}
				catch (Exception x)
				{
					MessageBox.Show(x.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			else Application.Run(new MainForm());
		}
	}
}
