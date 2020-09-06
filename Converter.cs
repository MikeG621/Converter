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
 * [UPD] Program.cs split out
 * [UPD] Processing code split out
 * [UPD] default save name suggested per original and platform
 * [UPD] Error message, Arg handling
 * v1.5, 190513
 * [UPD] Source file split to create .Designer.cs
 * [UPD] general cleaning
 * [NEW] BoP functionality [JB]
 * [UPD] TIE2XvT goal points only if goal exists [JB]
 * [UPD] text individual char/byte writes replaced with byte arrays
 * [FIX] offset in TIE2XvT EndOfMissionMessages [JB]
 * [UPD] Read/WriteInt() replaced with BinaryReader/Writer calls, several variables subsequently changed from int to short
 * [NEW] TIE questions converted to appropriate pre/post-mission text [JB]
 * [NEW] TIE Global Bonus Goals converted to 2nd set of XvT Secondary goals [JB]
 * [NEW] Combat Engagement capability [JB]
 * [NEW] helper function to convert XvT Designations to XWA format [JB]
 * [NEW] additional backdrop added to XWA missions [JB]
 * [UPD] remove multiple player check for XWA Combat missions [JB]
 * [NEW] XWA Order Speed [JB]
 * [FIX] XWA order wait times adjusted due to different multipliers [JB]
 * [UPD] added backdrops now randomized
 * [FIX] corrected XWA message delay [JB]
 * [UPD] XWA GlobalGoals now do all teams [JB]
 * [FIX] offset error in XWA GG Prim T1
 * [NEW] 2nd briefing converted to XWA [JB]
 * [UPD] Briefings 3-8 properly skipped when converting MP missions [JB]
 */

using System;
using System.IO;
using System.Windows.Forms;

namespace Idmr.Converter
{
	/// <summary>
	/// X-wing series mission converter (TIE -> XvT, TIE -> XWA, XvT -> XWA)
	/// </summary>
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		#region Check boxes
		void chkToXvTCheckedChanged(object sender, EventArgs e)
		{
			chkToXWA.Checked = !chkToXvT.Checked;
			chkToBop.Visible = chkToXvT.Checked;
		}
		
		void chkToXWACheckedChanged(object sender, EventArgs e)
		{
			chkToXvT.Checked = !chkToXWA.Checked;
			chkToBop.Visible = !chkToXWA.Checked; //[JB] Updated to hide BoP checkbox.
		}
		#endregion

		#region buttons
		void cmdExitClick(object sender, EventArgs e)
		{
			Application.Exit();
		}
		
		void cmdExistClick(object sender, EventArgs e)
		{
			opnExist.ShowDialog();
		}
		
		void cmdSaveClick(object sender, EventArgs e)
		{
			string fileName = Path.GetFileNameWithoutExtension(txtExist.Text);
			if (chkToXvT.Checked)
			{
				if (chkToBop.Checked) fileName += "_BoP";
				else fileName += "_XvT";
			}
			else fileName += "_XWA";
			fileName += ".tie";
			savConvert.FileName = fileName;
			savConvert.ShowDialog();
		}
		
		void opnExistFileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			txtExist.Text = opnExist.FileName;
			FileStream test = File.OpenRead(txtExist.Text);
			chkXvtCombat.Visible = false; //[JB] Added
			chkToBop.Visible = false;
			int d = test.ReadByte();
			test.Close();
			switch (d)
			{
				case 255:
					lblType.Text = "TIE";
					chkToXvT.Enabled = true;
					chkToXWA.Enabled = true;
					chkToBop.Visible = chkToXvT.Checked;
					break;
				case 12:
					lblType.Text = "XvT";
					chkToXvT.Checked = false;
					chkToXWA.Checked = true;
					chkToXvT.Enabled = false;
					chkToXWA.Enabled = false;
					chkXvtCombat.Visible = true;  //[JB] Added
					break;
				case 14:
					lblType.Text = "BoP";
					chkToXvT.Checked = false;
					chkToXWA.Checked = true;
					chkToXvT.Enabled = false;
					chkToXWA.Enabled = false;
					chkXvtCombat.Visible = true;  //[JB] Added
					break;
				case 18:
					MessageBox.Show("Cannot convert existing XWA missions!", "Error");
					txtExist.Text = "";
					lblType.Text = "XWA";
					break;
				default:
					MessageBox.Show("Invalid file", "Error");
					txtExist.Text = "";
					lblType.Text = "";
					break;
			}
		}
		
		void savConvertFileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			txtSave.Text = savConvert.FileName;
		}
		
		void cmdConvertClick(object sender, EventArgs e)
		{
			if (txtExist.Text == "" || txtSave.Text == "" || lblType.Text == "XWA") { return; }

			if (lblType.Text == "TIE")
			{
				if (chkToXvT.Checked) { Processor.TieToXvt(txtExist.Text, txtSave.Text, chkToBop.Checked); }
				if (chkToXWA.Checked) { Processor.TieToXwa(txtExist.Text, txtSave.Text); }
			}
			if (lblType.Text == "XvT" || lblType.Text == "BoP") { Processor.XvtToXwa(txtExist.Text, txtSave.Text, chkXvtCombat.Checked); }
			MessageBox.Show("Conversion completed", "Finished");
		}
		#endregion
	}
}
