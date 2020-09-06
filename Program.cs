/*
 * Convert.exe, Up-converts mission formats between TIE, XvT and XWA
 * Copyright (C) 2005-2020 Michael Gaisser (mjgaisser@gmail.com)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL (License.txt) was not distributed
 * with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
 *
 * VERSION: 1.5+
 */

/* CHANGELOG
 * [NEW] Program split out
 * [UPD] Error message, Arg handling
 */

using System;
using System.Windows.Forms;

namespace Idmr.Converter
{
	static class Program
	{
		[STAThread]
#pragma warning disable IDE1006 // Naming Styles
		static void Main(string[] Args)
#pragma warning restore IDE1006 // Naming Styles
		{
			if (Args.Length > 0 && (Args.Length < 2 || Args.Length > 3)) // if args are detected but not the correct amount, treat as mis-use
			{
				MessageBox.Show(MainForm.ExceptionMessage, "Error");
				return;
			}
			bool hidden = (Args.Length >= 2);
			Application.Run(new MainForm(Args, hidden));
		}
	}
}
