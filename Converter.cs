/*
 * Convert.exe, Converts mission formats between TIE, XvT and XWA
 * Copyright (C) 2005- Michael Gaisser (mjgaisser@gmail.com)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL (License.txt) was not distributed
 * with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
 *
 * VERSION: 1.5
 */

/* CHANGELOG
* v1.5, 
* [UPD] Source file split to create .Designer.cs
* [UPD] general cleaning
* [NEW] BoP functionality [JB]
*/

using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Idmr.Converter
{
	/// <summary>
	/// X-wing series mission converter (TIE -> XvT, TIE -> XWA, XvT -> XWA)
	/// </summary>
	public partial class MainForm : System.Windows.Forms.Form
	{
		public static bool T2W = false;
		public static bool hidden = false;
		public static string[] Arg;
		private static int[] BRF = {0,0,0,0,1,1,2,2,0,1,1,1,1,1,1,1,1,0,4,4,4,4,4,4,4,4,3,2,3,2,1,0,0,0,0};

		public MainForm()
		{
			InitializeComponent();
			if (hidden == true)		//cmdline run
			{
				this.Hide();		//don't need the form, just the txt
				txtExist.Text = Arg[0];
				try
				{
					if (File.Exists(Arg[0]) == false) throw new Exception("Cannot locate original file.");
					FileStream Test;
					Test = File.OpenRead(txtExist.Text);
					int d = Test.ReadByte();	//check platform, really only make sure it's not XWA and legit
					switch (d)
					{
						case 255:
							break;
						case 12:
							break;
						case 14:
							break;
						case 18:
							throw new Exception("Cannot convert existing XWA missions.");
						default:
							throw new Exception("Invalid file");
					}
					Test.Close();
					txtSave.Text = Arg[1];
					switch (Arg[2])		//check mode
					{
						case "1":
							if (d == 255) TIE2XvT();
							else throw new Exception("Invalid conversion type for file specified.");
							break;
						case "2":
							if (d == 255) TIE2XWA();
							else throw new Exception("Invalid conversion type for file specified.");
							break;
						case "3":
							if (d == 12 || d == 14) XvT2XWA();
							else throw new Exception("Invalid conversion type for file specified.");
							break;
						default:
							throw new Exception("Incorrect parameter usage. Correct usage is as follows:\nOriginal path, new path, mode\nModes: 1 - TIE to XvT, 2 - TIE to XWA, 3 - XvT to XWA");
					}
				}
				catch (Exception x)
				{
					MessageBox.Show(x.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Application.Exit();
				}
			}
		}

		[STAThread]
		static void Main(string[] Args) 
		{
			if (Args.Length > 0 && Args.Length != 3) // if args are detected but not the correct amount, treat as mis-use
			{
				MessageBox.Show("Incorrect parameter usage. Correct usage is as follows:\nOriginal path, new path, mode\nModes: 1 - TIE to XvT, 2 - TIE to XWA, 3 - XvT to XWA", "Error");
				return;
			}
			if (Args.Length == 3)
			{
				Arg = new string[3];
				int i;
				for (i=0;i<3;i++)
				{
					Arg[i] = Args[i]; 
				}
				hidden = true;		//run silent
			}
			Application.Run(new MainForm());
		}

		#region Check boxes
		void ChkXvT2CheckedChanged(object sender, EventArgs e)
		{
			chkXWA.Checked = !chkXvT2.Checked;
		}
		
		void ChkXWACheckedChanged(object sender, EventArgs e)
		{
			chkXvT2.Checked = !chkXWA.Checked;
			chkXvtBop.Visible = !chkXWA.Checked; //[JB] Updated to hide BoP checkbox.
		}
		#endregion

		#region buttons
		void CmdExitClick(object sender, EventArgs e)
		{
			Application.Exit();
		}
		
		void CmdExistClick(object sender, EventArgs e)
		{
			opnExist.ShowDialog();
		}
		
		void CmdSaveClick(object sender, EventArgs e)
		{
			savConvert.ShowDialog();
		}
		
		void OpnExistFileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			txtExist.Text = opnExist.FileName;
			FileStream Test;
			Test = File.OpenRead(txtExist.Text);
			lblPlayer.Visible = false;
			optImp.Visible = false;
			optReb.Visible = false;
			chkXvtCombat.Visible = false; //[JB] Added
			chkXvtBop.Visible = false;
			int d = Test.ReadByte();
			switch (d)
			{
				case 255:
					lblType.Text = "TIE";
					chkXvT2.Enabled = true;
					chkXWA.Enabled = true;
					lblPlayer.Visible = true;
					optImp.Checked = true;
					optImp.Visible = true;
					optReb.Visible = true;
					chkXvtBop.Visible = chkXvT2.Checked;
					break;
				case 12:
					lblType.Text = "XvT";
					chkXvT2.Checked = false;
					chkXWA.Checked = true;
					chkXvT2.Enabled = false;
					chkXWA.Enabled = false;
					chkXvtCombat.Visible = true;  //[JB] Added
					break;
				case 14:
					lblType.Text = "BoP";
					chkXvT2.Checked = false;
					chkXWA.Checked = true;
					chkXvT2.Enabled = false;
					chkXWA.Enabled = false;
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
			Test.Close();
		}
		
		void SavConvertFileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			txtSave.Text = savConvert.FileName;
		}
		
		void CmdConvertClick(object sender, EventArgs e)
		{
			if (txtExist.Text == "" | txtSave.Text == "") { return; }
			if (lblType.Text == "TIE")
			{
				if (chkXvT2.Checked == true) { TIE2XvT(); }
				if (chkXWA.Checked == true) { TIE2XWA(); }
			}
			if (lblType.Text == "XvT" | lblType.Text == "BoP") { XvT2XWA(); }
		}
		#endregion

		void TIE2XvT()
		{
			FileStream TIE, XvT;
			TIE = File.OpenRead(txtExist.Text);
			XvT = File.Open(txtSave.Text, FileMode.Create, FileAccess.ReadWrite);
			long XvTPos;
			int format = 12;  //[JB] Added platform selection for XvT/BoP
			if(chkXvtBop.Checked == true)
				format = 14;
			XvT.WriteByte((byte)format);			//XvT format

			XvT.Position = 2;
			TIE.Position = 2;
			int i, j;
			for(i = 0;i < 4;i++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));			//# of FGs and messages
			TIE.Position = 2;
			int FGs = ReadInt(TIE);			//store them
			int Messages = ReadInt(TIE);
			if (chkXvtBop.Checked == true)  //[JB] patch in
			{
				XvT.Position = 0x64;
				XvT.WriteByte(3);  //MPtraining
			}
			TIE.Position = 0x1CA;
			XvT.Position = 0xA4;
			#region Flight Groups
			for(i = 0;i < FGs;i++)		//Flight Groups
			{
				long TIEPos = TIE.Position;
				XvTPos = XvT.Position;
				for(j = 0;j < 12;j++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));		//FG Name
				TIE.Position = TIEPos + 24;
				XvT.Position = XvTPos + 40;
				for(j = 0;j < 12;j++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));		//Cargo
				XvT.Position = XvTPos + 60;
				for(j = 0;j < 12;j++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));		//Special Cargo
				XvT.Position = XvTPos + 80;
				for(j = 0;j < 8;j++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));			//SC ship# -> IFF
				TIE.Position -= 1;
				// if player is Imperial, have to switch Team/IFF values, since Imps have to be Team 1 (0) and Rebs are Team 2 (1)
				XvT.Position = XvTPos + 88;
				switch(TIE.ReadByte())
				{
					case 0:
						if (optImp.Checked) XvT.WriteByte(1);
						else XvT.WriteByte(0);
						break;
					case 1:
						if (optImp.Checked)	XvT.WriteByte(0);
						else XvT.WriteByte(1);
						break;
					default:
						TIE.Position--;
						XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));
						break;
				}
				for(j = 0;j < 9;j++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));			//AI -> # of waves
				TIE.Position += 1;
				XvT.Position += 2;
				if (TIE.ReadByte() != 0)
				{
					XvT.WriteByte(1);														//Player 1
					XvT.Position++;
					TIE.Position--;
					XvT.WriteByte(Convert.ToByte(TIE.ReadByte()-1));						//Player's craft #
				}
				else XvT.Position += 3;
				for(j = 0;j < 3;j++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));			//Orientation values
				TIE.Position = TIEPos + 73;
				XvT.Position = XvTPos + 109;
				for(j = 0;j < 9;j++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));			//Arrival trigger 1&2
				XvT.Position += 2;
				XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));								//trigger 1 AND/OR 2
				TIE.Position += 1;
				XvT.Position = XvTPos + 134;
				for(j = 0;j < 6;j++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));			//Arrv delay -> Dep trigger
				TIE.Position += 2;
				XvT.Position += 9;
				XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));								//Abort trigger
				TIE.Position += 3;
				XvT.Position += 4;
				for(j = 0;j < 8;j++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));			//Arrv/Dep methods
				for(j = 0;j < 18;j++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));		//Order 1
				XvT.Position += 64;
				for(j = 0;j < 18;j++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));		//Order 2
				XvT.Position += 64;
				for(j = 0;j < 18;j++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));		//Order 3
				XvT.Position = XvTPos + 502;
				j = TIE.ReadByte();
				if (j == 9)																	// if EXIST
				{
					XvT.Position--;
					XvT.WriteByte(1);														// must NOT
					XvT.WriteByte(2);														// be destroyed
				}
				else XvT.WriteByte(Convert.ToByte(j));										//Primary Goal
				byte b = Convert.ToByte(TIE.ReadByte());
				if (b == 1) b++;	// 50%
				else if (b >= 2) b += 2;	// >=1 and on
				XvT.WriteByte(b);															//Amount
				XvT.WriteByte(Convert.ToByte((j != 10 && j != 9) ? 1 : 0));								//250 points  [JB] Changed to 250 points only if a condition exists (not "FALSE") but also not "must have survived"
				if (j != 10) XvT.WriteByte(1);												//Enable Primary Goal
				else XvT.WriteByte(0);
				XvT.Position += 73;
				XvT.WriteByte(2);															//Secondary Goal
				j = TIE.ReadByte();
				if (j == 9)																	// if EXIST
				{
					XvT.Position--;
					XvT.WriteByte(3);														// BONUS must NOT
					XvT.WriteByte(2);														// be destroyed
				}
				else XvT.WriteByte(Convert.ToByte(j));	
				b = Convert.ToByte(TIE.ReadByte());
				if (b == 1) b++;	// 50%
				else if (b >= 2) b += 2;	// >=1 and on
				XvT.WriteByte(b);
				XvT.WriteByte(Convert.ToByte((j != 10) ? 1 : 0));											//250 points  [JB] Changed to 250 points only if a condition exists (not "FALSE")
				if (j != 10) XvT.WriteByte(1);
				else XvT.WriteByte(0);
				XvT.Position += 73;
				XvT.WriteByte(2);							  								//Secret Goal
				j = TIE.ReadByte();
				if (j == 9)																	// if EXIST
				{
					XvT.Position--;
					XvT.WriteByte(3);														// BONUS must NOT
					XvT.WriteByte(2);														// be destroyed
				}
				else XvT.WriteByte(Convert.ToByte(j));	
				b = Convert.ToByte(TIE.ReadByte());
				if (b == 1) b++;	// 50%
				else if (b >= 2) b += 2;	// >=1 and on
				XvT.WriteByte(b);
				XvT.Position += 1;
				if (j != 10) XvT.WriteByte(1);
				else XvT.WriteByte(0);
				XvT.Position += 73;
				XvT.WriteByte(2);							  								//Bonus Goal
				j = TIE.ReadByte();
				if (j == 9)																	// if EXIST
				{
					XvT.Position--;
					XvT.WriteByte(3);														// BONUS must NOT
					XvT.WriteByte(2);														// be destroyed
				}
				else XvT.WriteByte(Convert.ToByte(j));	
				b = Convert.ToByte(TIE.ReadByte());
				if (b == 1) b++;	// 50%
				else if (b >= 2) b += 2;	// >=1 and on
				XvT.WriteByte(b);
				XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));		//Bonus points, will need fiddling
				if (j != 10) XvT.WriteByte(1);
				else XvT.WriteByte(0);
				XvT.Position += 74;
				XvT.WriteByte(10);									//10 is the 'null' trigger (goal 5)
				XvT.Position += 77;
				XvT.WriteByte(10);									//goal 6
				XvT.Position += 77;
				XvT.WriteByte(10);									//goal 7
				XvT.Position += 77;
				XvT.WriteByte(10);									//goal 8
				TIE.ReadByte();
				XvT.Position = XvTPos + 1126;
				for(j = 0;j < 30;j++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));		//X points
				XvT.Position += 14;
				for(j = 0;j < 30;j++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));		//Y points
				XvT.Position += 14;
				for(j = 0;j < 30;j++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));		//Z points
				XvT.Position += 14;
				for(j = 0;j < 30;j++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));		//Enable points
				XvT.Position += 90;															//goto End
				TIE.Position += 4;
			}
			#endregion
			#region Messages
			for(i = 0;i < Messages;i++)
			{
				XvT.WriteByte(Convert.ToByte(i));
				XvT.Position++;
				for(j = 0;j < 64;j++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));	//Color & message
				XvT.WriteByte(1);
				XvT.Position += 9;
				for(j = 0;j < 8;j++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));		//triggers 1 & 2
				XvT.Position++;
				TIE.Position += 17;
				XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));							//trigger 1 AND/OR 2
				TIE.Position -= 2;
				XvT.Position += 28;
				XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));							//Delay
				TIE.Position++;
				XvT.Position++;
			}
			#endregion
			XvT.WriteByte(3);XvT.Position++;											//Unknown
			#region Global Goals
			for(j = 0;j < 8;j++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));			//Prim Goal trigger 1 & 2
			XvT.Position += 2;
			TIE.Position += 17;
			XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));								//Prim trigger 1 AND/OR 2
			XvT.Position += 73;
			TIE.Position += 2;
			for(j = 0;j < 8;j++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));			//Sec Goal trigger 1 & 2
			XvT.Position += 2;
			TIE.Position += 17;
			XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));								//Sec Goal trigger A/O


			//[JB] Convert TIE bonus global goals into a second set of XvT secondary goals
			TIE.Position += 2; //Skip 2 bytes after the previous and/or byte to reach the start of the bonus goal triggers
			for(j = 0;j < 8;j++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));			//Bonus Goal trigger 1 & 2
			XvT.Position += 2;
			TIE.Position += 17;
			XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));								//Bonus Goal trigger A/O
			XvT.Position += 17;
			XvT.WriteByte(0);   //And/Or
			XvT.Position += 2;  //Jump to the end of the Global Goal block, which happens to be the start of the second teams's global goal block
			XvTPos = XvT.Position;

			//[JB] Patch to convert all "must be FALSE" conditions to TRUE so that and/or doesn't cause conflicts
			XvT.Position -= 0x2A; //Rewind back to start of XvT Secondary, Trigger 1
			if(XvT.ReadByte() == 0x0A) {XvT.Position--;XvT.WriteByte(0);}  //Trig1
			XvT.Position += 3;
			if(XvT.ReadByte() == 0x0A) {XvT.Position--;XvT.WriteByte(0);}  //Trig2
			XvT.Position += 3;
			XvT.Position += 3;  //Jump the gap between Trig 1/2 and T3/4
			if(XvT.ReadByte() == 0x0A) {XvT.Position--;XvT.WriteByte(0);}  //Trig3
			XvT.Position += 3;
			if(XvT.ReadByte() == 0x0A) {XvT.Position--;XvT.WriteByte(0);}  //Trig4
	
			XvT.Position = XvTPos;

			//XvT.Position += 31; //Don't need to jump to the next Global Goal block since I did that earlier after accounting for the offset changes in the new code.
			for(j = 0;j < 9;j++)
			{
				XvT.WriteByte(3);
				XvT.Position += 127;
			}
			#endregion
			TIE.Position = 0x19A;
			#region IFF/Teams
			XvTPos = XvT.Position;
			XvT.WriteByte(1);
			XvT.Position++;
			if (optImp.Checked) { XvT.WriteByte(73);XvT.WriteByte(109);XvT.WriteByte(112);XvT.WriteByte(101);XvT.WriteByte(114);XvT.WriteByte(105);XvT.WriteByte(97);XvT.WriteByte(108); }
			else { XvT.WriteByte(82);XvT.WriteByte(101);XvT.WriteByte(98);XvT.WriteByte(101);XvT.WriteByte(108); }
			XvT.Position = XvTPos + 0x1A;
			XvT.WriteByte(1);									//Team 1 Allies
			XvT.Position++;										//Team 2 bad guys
			if(TIE.ReadByte() == 49) XvT.WriteByte(1);else XvT.Position++;			//IFF 3 stance (blue)
			TIE.Position = 422;
			if(TIE.ReadByte() == 49) XvT.WriteByte(1);else XvT.Position++;			//IFF 4 stance (purple)
			TIE.Position = 434;
			if(TIE.ReadByte() == 49) XvT.WriteByte(1);else XvT.Position++;			//IFF 5 stance (red)
			TIE.Position = 446;
			if(TIE.ReadByte() == 49) XvT.WriteByte(1);else XvT.Position++;			//IFF 6 stance (purple)
			XvT.Position += 4;
			TIE.Position = 24;
			for(i = 0;i < 128;i++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));		//Primary mission complete
			TIE.Position += 128;
			for(i = 0;i < 128;i++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));		//Primary failed
			TIE.Position -= 256;  //[JB] Fixed offset, was off by one.
			for(i = 0;i < 128;i++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));		//Secondary complete
			TIE.Position = 410;
			XvT.Position += 67;
			XvTPos = XvT.Position;
			XvT.WriteByte(1);XvT.Position++;
			if (optImp.Checked) { XvT.WriteByte(82);XvT.WriteByte(101);XvT.WriteByte(98);XvT.WriteByte(101);XvT.WriteByte(108); }
			else { XvT.WriteByte(73);XvT.WriteByte(109);XvT.WriteByte(112);XvT.WriteByte(101);XvT.WriteByte(114);XvT.WriteByte(105);XvT.WriteByte(97);XvT.WriteByte(108); }
			XvT.Position = XvTPos + 0x1E7;
			XvT.WriteByte(1);XvT.Position++;
			if(TIE.ReadByte() != 49) TIE.Position--;									//check for hostile char
			for(i = 0;i < 11;i++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));		//IFF 3 name
			XvT.Position += 474;XvT.WriteByte(1);XvT.Position++;
			TIE.Position = 422;
			if(TIE.ReadByte() != 49) TIE.Position--;
			for(i = 0;i < 11;i++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));		//IFF 4 name
			XvT.Position += 474;XvT.WriteByte(1);XvT.Position++;
			TIE.Position = 434;
			if(TIE.ReadByte() != 49) TIE.Position--;
			for(i = 0;i < 11;i++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));		//IFF 5 name
			XvT.Position += 474;XvT.WriteByte(1);XvT.Position++;
			TIE.Position = 446;
			if(TIE.ReadByte() != 49) TIE.Position--;
			for(i = 0;i < 11;i++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));		//IFF 6 name
			XvT.Position += 474;XvT.WriteByte(1);										//markers infront of other IFF name spots
			XvT.Position += 486;XvT.WriteByte(1);
			XvT.Position += 486;XvT.WriteByte(1);
			XvT.Position += 486;XvT.WriteByte(1);
			#endregion
			XvT.Position += 0x1E6;
			TIE.Position = 0x21E + 0x124*FGs + 0x5A*Messages;
			XvTPos = XvT.Position;
			#region Briefing
			for(i = 0;i < 0x32A;i++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));		//Briefing
			XvT.WriteByte(1);XvT.Position += 9;
			XvT.Position = XvTPos;
			j = ReadInt(XvT) * 0x14 / 0xC;		// adjust overall briefing length
			XvT.Position -= 2;
			WriteInt(XvT, j);
			XvT.Position += 8;
			for (i=0;i<0x320;i+=4)		// work our way through length of briefing. i automatically increases by 4 per event
			{
				j = ReadInt(XvT);
				if (j == 0x270F) break;		// stop check at t=9999, end briefing
				j = j * 0x14 / 0xC;
				XvT.Position -= 2;
				WriteInt(XvT,j);
				j = ReadInt(XvT);		// now get the event type
				if (j == 7)		// Zoom map command
				{
					j = ReadInt(XvT) * 58 / 47;	// X
					XvT.Position -= 2;
					WriteInt(XvT, j);
					j = ReadInt(XvT) * 88 / 47;	// Y
					XvT.Position -= 2;
					WriteInt(XvT, j);
					i += 4;
				}
				else
				{
					XvT.Position += 2*BRF[j];		// skip over vars
					i += 2*BRF[j];	// increase length counter by skipped vars
				}
			}
			#endregion
			XvT.Position = 0x1BDE + 0x562*FGs + 0x74*Messages;
			#region Briefing tags & strings
			int BriefingTagLength = 0;
			for(i = 0;i < 64;i++)
			{
				j = ReadInt(TIE);		//check length..  (read int really just puts TIE in the right spot, will always be <256
				BriefingTagLength += 2 + j;  //[JB] Calculate briefing size so we can factor it into calculations to find the description location.
				XvT.WriteByte(Convert.ToByte(j)); XvT.WriteByte(0);				//..write length..
				if(j != 0)										//and copy if not 0
				{
					int k;
					for(k = 0;k < j;k++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));
				}
			}
			for (i = 0;i <7;i++)
			{
				XvT.WriteByte(0xC8);XvT.Position += 5;XvT.WriteByte(2);XvT.Position += 3;XvT.WriteByte(0xF);XvT.WriteByte(0x27);XvT.WriteByte(0x22);
				XvT.Position += 0x3A7;
			}
			#endregion
			#region Mission Questions

			string preMissionQuestions = "";
			for (i = 0; i < 10; i++)
			{
				int len = ReadInt(TIE);
				if(len == 0)
					continue;
				if (len == 1)
				{
					TIE.Position++;
					continue;
				}
				byte[] buffer = new byte[len];
				TIE.Read(buffer, 0, len);
				string s = System.Text.Encoding.ASCII.GetString(buffer);
				int sep = s.IndexOf((char)0xA);
				if (sep >= 0)
				{
					string q = s.Substring(0, sep);
					string a = s.Substring(sep + 1);
					a = a.Replace((char)0xA, ' ');
					a = a.Replace((char)0x2, '[');
					a = a.Replace((char)0x1, ']');
					if(preMissionQuestions.Length > 0)
						preMissionQuestions += "$";
					preMissionQuestions += q + "$$" + a + "$"; 
				}
			}
			string postMissionFail = "";
			string postMissionSuccess = "";
			for (i = 0; i < 10; i++)
			{
				int len = ReadInt(TIE);
				if(len == 0)
					continue;
				if (len == 3)
				{
					TIE.Position += 3;
					continue;
				}
				int qCondition = TIE.ReadByte();
				int qType = TIE.ReadByte();
				byte[] buffer = new byte[len - 2];  //Length includes condition/type bytes.
				TIE.Read(buffer, 0, len - 2);

				if(qCondition == 0 || qType == 0)
					continue;

				string s = System.Text.Encoding.ASCII.GetString(buffer);
				if(buffer[len - 3] == 0xFF)  //If this is the last byte of the string data, remove it.
					s = s.Remove(s.Length - 1);
				int sep = s.IndexOf((char)0xA);
				if (sep >= 0)
				{
					string q = s.Substring(0, sep);
					string a = s.Substring(sep + 1);
					a = a.Replace((char)0xA, ' ');
					a = a.Replace((char)0x2, '[');
					a = a.Replace((char)0x1, ']');

					if (qCondition == 4)
					{
						if (postMissionSuccess.Length > 0)
							postMissionSuccess += "$";
						postMissionSuccess += q + "$$" + a + "$";
					}
					else if (qCondition == 5)
					{
						if (postMissionFail.Length > 0)
							postMissionFail += "$";
						postMissionFail += q + "$$" + a + "$";
					}
				}
			}
			#endregion
			//XvT.Position += (0xD9A6 + 0x600*FGs);
			//XvT.WriteByte(0);
			#region Mission Description
			XvT.Position = 0xA4 + (FGs * 0x562) + (Messages * 0x74) + (0x80 * 10) + (0x1E7 * 10); //Header + FGs + Messages + 10 Globals + 10 Teams
			XvT.Position += (0x334 + BriefingTagLength) + (0x3B4 * 7);  //Briefing 1 (plus tag/string lengths) + 7 empty briefings (static size)
			XvT.Position += (0x600 * FGs) + 0x5A00;  //FGGoalStrings(FGs*8*3*64) + GlobalGoalStrings(10*3*4*3*64)
			XvT.Position += (0xC00 * 10);  //Empty GlobalGoalStrings data.  Undocumented?  Value taken from YOGEME save/load functions.

			int maxLength = (format == 12) ? 0x400 : 0x1000;
			if(preMissionQuestions.Length > maxLength)
				preMissionQuestions = preMissionQuestions.Remove(maxLength);
			if(postMissionSuccess.Length > maxLength)
				postMissionSuccess = postMissionSuccess.Remove(maxLength);
			if(postMissionFail.Length > maxLength)
				postMissionFail = postMissionFail.Remove(maxLength);


			byte[] desc;
			if(format == 12)
				desc = System.Text.Encoding.ASCII.GetBytes(preMissionQuestions);
			else
				desc = System.Text.Encoding.ASCII.GetBytes(postMissionSuccess);

			XvTPos = XvT.Position;
			XvT.Write(desc, 0, desc.Length);
			XvT.Position = XvTPos + maxLength - 1;
			XvT.WriteByte(0);

			if (format == 14)
			{
				XvTPos += maxLength;
				XvT.Position = XvTPos;
				desc = System.Text.Encoding.ASCII.GetBytes(postMissionFail);
				XvT.Write(desc, 0, desc.Length);
				XvT.Position = XvTPos + maxLength - 1;
				XvT.WriteByte(0);

				XvTPos += maxLength;
				XvT.Position = XvTPos;
				desc = System.Text.Encoding.ASCII.GetBytes(preMissionQuestions);
				XvT.Write(desc, 0, desc.Length);
				XvT.Position = XvTPos + maxLength - 1;
				XvT.WriteByte(0);
			}
			#endregion

			XvT.Close();
			TIE.Close();
			if (T2W == false && hidden == false) MessageBox.Show("Conversion completed", "Finished");
		}
		
		void TIE2XWA()
		{
			//instead of writing it all out again, cheat and use the other two
			T2W = true;
			string save, exist;
			save = txtSave.Text;
			exist = txtExist.Text;
			txtSave.Text = "temp.tie";
			TIE2XvT();
			txtSave.Text = save;
			txtExist.Text = "temp.tie";
			lblType.Text = "XvT";
			XvT2XWA();
			lblType.Text = "TIE";
			txtExist.Text = exist;
			File.Delete("temp.tie");
			if (hidden == false) MessageBox.Show("Conversion completed", "Finished");
		}

		void XvT2XWA_ConvertDesignation(byte[] xvt, byte[] xwa)
		{
			//xvt  input   4 chars, [0] = team, [1..3] text of role.  EX: "2MIS"
			//xwa  output  2 bytes, [0] = team index, [1] = role enum
			xwa[0] = 0xFF;

			string t = System.Text.Encoding.ASCII.GetString(xvt).ToUpper();
			if(t[0] == 0)
				return;
			byte value = 0;
			switch(t[0])
			{
				case '1': value = 0; break;
				case '2': value = 1; break;
				case '3': value = 2; break;
				case '4': value = 3; break;
				case 'A': value = 0xB; break;
				case 'H': value = 0xA; break;  //No idea.
				default: value = 0xFF; break;
			}
			xwa[0] = value;

			
			Dictionary<string, byte> roleMap = new Dictionary<string, byte> {
				{"NON", 0xFF},
				{"BAS", 1},
				{"COM", 0},
				{"CON", 4},
				{"MAN", 11},
				{"MIS", 3},
				{"PRI", 7},
				{"REL", 6},
				{"RES", 10},
				{"SEC", 8},
				{"STA", 2},
				{"STR", 5},
				{"TER", 9}
			};
			t = t.Substring(1);
			xwa[1] = 0;
			if(roleMap.TryGetValue(t, out value) == true)
				xwa[1] = value;

	/*
			value = 0xFF;
			t = t.Substring(1);
			if(t == "NON") value = 0xFF;
			else if(t == "BAS") value = 1;


*/
		}
		void XvT2XWA_ConvertDesignation2(byte[] xvt, byte[] xwa)
		{
			//xvt  input   4 chars, [0] = team, [1..3] text of role.  EX: "2MIS"
			//xwa  output  2 bytes, [0] = team index, [1] = role enum
			xwa[0] = 0xFF;
			xwa[1] = 0xFF;
			xwa[2] = 0x00;
			xwa[3] = 0x00;

			Dictionary<string, byte> roleMap = new Dictionary<string, byte> {
				{"NON", 0xFF},
				{"BAS", 1},
				{"COM", 0},
				{"CON", 4},
				{"MAN", 11},
				{"MIS", 3},
				{"PRI", 7},
				{"REL", 6},
				{"RES", 10},
				{"SEC", 8},
				{"STA", 2},
				{"STR", 5},
				{"TER", 9}
			};

			string t = System.Text.Encoding.ASCII.GetString(xvt).ToUpper();
			for (int i = 0; i < 2; i++)
			{
				string sub = t.Substring(0, 4);
				if(sub[0] == 0)
					return;

				//Get the role first so that if the team is set to all, both teams can be assigned the same role.
				char team = sub[0];
				sub = sub.Substring(1);
				byte role = 0;
				roleMap.TryGetValue(sub, out role);
				xwa[2+i] = role;

				switch(team)
				{
					case '1': xwa[i] = 0x0; break;
					case '2': xwa[i] = 0x1; break;
					case '3': xwa[i] = 0x2; break;
					case '4': xwa[i] = 0x3; break;
					case 'A': xwa[0] = 0xA; xwa[1] = 0xB; xwa[2] = role; xwa[3] = role; break;
					case 'H': xwa[0] = 0xA; xwa[1] = 0xB; xwa[2] = role; xwa[3] = role; break;  //No idea.
					default: xwa[i] = 0x0; break;
				}



				t = t.Substring(4); //Trim so the next 4 become the current.
			}
		}		
		void XvT2XWA()
		{
			int[] fgIcons;
			FileStream XvT, XWA;													//usual crap...
			XvT = File.OpenRead(txtExist.Text);
			XWA = File.Open(txtSave.Text, FileMode.Create, FileAccess.ReadWrite);
			XWA.WriteByte(18);
			XWA.Position++;
			XvT.Position = 2;
			int i, j;
			for(i = 0;i < 4;i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));
			XvT.Position = 2;
			int FGs = ReadInt(XvT);
			fgIcons = new int[FGs];
			XWA.Position = 2;XWA.WriteByte(Convert.ToByte(FGs + 2));	// +1 because I'm inserting a backdrop   [JB] Modified to +2 since generated skirmish files have two backdrops for ambient lighting
			int Messages = ReadInt(XvT);
			XWA.Position = 8;XWA.WriteByte(1);XWA.Position = 11;XWA.WriteByte(1);		//unknown
			XWA.Position = 100;XWA.WriteByte(84);XWA.WriteByte(104);XWA.WriteByte(101);XWA.WriteByte(32);XWA.WriteByte(70);XWA.WriteByte(105);XWA.WriteByte(110);XWA.WriteByte(97);XWA.WriteByte(108);
			XWA.WriteByte(32);XWA.WriteByte(70);XWA.WriteByte(114);XWA.WriteByte(111);XWA.WriteByte(110);XWA.WriteByte(116);XWA.WriteByte(105);XWA.WriteByte(101);XWA.WriteByte(114);	//make a nice Region name :P
			XWA.Position = 0x23AC; XWA.WriteByte(6); 						//starting hangar
			XWA.Position++;
			XvT.Position = 0x66;XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//time limit (minutes)
			XWA.Position = 0x23B3; XWA.WriteByte(0x62);						//unknown
			XWA.Position = 0x23F0;
			XvT.Position = 0xA4;
			//[JB] Jumping ahead to get the briefing locations before we load in the FGs.
			long brief1BlockStart = 0xA4 + (0x562 * FGs) + (0x74 * Messages) + 0x500 + 0x1306;  //FGs, messages, teams, global goals
			XvT.Position = brief1BlockStart + 0x334;  //Jump to tags
			long brief1StringSize = 0;
			long brief1EndSize = 0;
			for(i = 0; i < 64; i++)   //32 tags + 32 strings
			{
				int len = ReadInt(XvT);
				brief1StringSize += 2;
				XvT.Position += len;
				brief1StringSize += len;
			}
			brief1EndSize = XvT.Position;
			XvT.Position = 0xA4;  //Rewind back to start of FG data
			//[JB] End loading briefing offset;

			#region Flight Groups
			long XvTPos; //[JB] Sometimes need to bookmark XvT too for better offset handling
			long XWAPos;
			bool Player = false;
			int PlayerCraft = 0; //[JB] for combat engagements
			//int Brief = 0;
			int[] Briefs = new int[2];
			Briefs[0] = 0;
			Briefs[1] = 0;
			for(i = 0;i < FGs;i++)
			{
				XvTPos = XvT.Position;
				XWAPos = XWA.Position;
				for(j = 0;j < 20;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//name
				byte[] d1 = new byte[2];
				byte[] d2 = new byte[2];
				byte[] buffer = new byte[4];
				byte[] buffer2 = new byte[8];
				byte[] des2 = new byte[4];
				/*
				XvT.Read(buffer, 0, 4);
				XvT2XWA_ConvertDesignation(buffer, d1);
				XvT.Read(buffer, 0, 4);
				XvT2XWA_ConvertDesignation(buffer, d2);
				 * */
				XvT.Read(buffer2, 0, 8);
				XvT2XWA_ConvertDesignation2(buffer2, des2);
				XvT.Position -= 8;
				//XWA.WriteByte(d1[0]); XWA.WriteByte(d2[0]);
				//XWA.WriteByte(d1[1]); XWA.WriteByte(d2[1]);
				XWA.WriteByte(des2[0]); XWA.WriteByte(des2[1]);
				XWA.WriteByte(des2[2]); XWA.WriteByte(des2[3]);
				XWA.Position ++;       //Skip unknown
				//XWA.WriteByte(255);														//no Designations, can't convert
				//XWA.WriteByte(255);
				//XWA.Position += 3;  //[JB] Changed from 4 to 3
				XWA.WriteByte(255); //Global cargos set to none
				XWA.WriteByte(255);
				XWA.Position = XWAPos + 40;
				XvT.Position += 20;
				for(j = 0;j < 40;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//Cargo & Special
				XWA.Position = XWAPos + 105;
				for(j = 0;j < 30;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));			//SC ship# -> Arrival Difficulty
				XvT.Position = XvTPos + 100;
				// [JB] Modified
				if (this.chkXvtCombat.Checked == false)
				{
					if (XvT.ReadByte() != 0 && Player == false) { Player = true; }				//check for multiple player craft, take the first one
					else
					{
						XWA.Position = XWAPos + 125;
						XWA.WriteByte(0);
					}
				}
				if (XvT.ReadByte() != 0) { PlayerCraft++;}

				XWA.Position = XWAPos + 136;
				XvT.Position = XvTPos + 110;
				for(j = 0;j < 4;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));				//Arival trigger 1...
				ShipFix(XvT, XWA);
				for(j = 0;j < 4;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));				//... and 2
				ShipFix(XvT, XWA);
				XWA.Position += 2;
				XvT.Position += 2;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));									//AT 1 AND/OR 2
				XWA.Position++;
				for(j = 0;j < 4;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));				//AT 3
				ShipFix(XvT, XWA);
				for(j = 0;j < 4;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));				//AT 4
				ShipFix(XvT, XWA);
				XWA.Position += 2;
				XvT.Position += 2;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));									//AT 3 AND/OR 4
				XWA.Position++;
				for(j = 0;j < 8;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));				//AT 1/2 AND/OR 3/4 -> DT
				ShipFix(XvT, XWA);
				for(j = 0;j < 4;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));				//DT 2
				ShipFix(XvT, XWA);
				XWA.Position += 2;
				XvT.Position += 2;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));									//DT 1 AND/OR 2
				XWA.Position += 3;
				XvT.Position += 2;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));									//Abort trigger
				XvT.Position += 4;
				XWA.Position += 3;
				for(j = 0;j < 8;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));				//Arr/Dep methods
				long XvTOrderStart = XvT.Position;
				long XWAOrderStart = XWA.Position;
				long XvTSubOrderStart = XvT.Position;  //[JB] ShipOFix modifies the offsets and I assume it's bad to add anything so I'm going to let the original code run then rewind to these offsets later to apply my patches.
				long XWASubOrderStart = XWA.Position;
				for(j = 0;j < 18;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//Order 1
				ShipOFix(XvT, XWA);
				XvT.Position = XvTPos + 1134;
				for(j = 0;j < 8;j++)
				{
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//WP X
					XvT.Position += 42;
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//WP Y
					XvT.Position += 42;
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//WP Z
					XvT.Position += 42;
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//WP enabled
					XvT.Position -= 132;
				}

				//[JB] Patch wait times
				XvT.Position = XvTSubOrderStart;
				XWA.Position = XWASubOrderStart;
				XvTToXWA_ConvertOrderTime(XvT, XWA);
				//Copy order speed
				XvT.Position = XvTSubOrderStart + 18;
				XWA.Position = XWASubOrderStart + 18;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));
				//After XvT speed comes flight group player designation, load it to patch the role later. 
				byte[] role = new byte[16];
				XvT.Read(role, 0, 16);
				XWA.Position = XWAPos + 0x50; //Patch in the display role for the player FG slot screen.
				XWA.Write(role, 0, 16);
				//[JB] End patch code.

				XWA.Position = XWAPos + 350;
				XvT.Position = XvTPos + 244;
				XvTSubOrderStart = XvT.Position;  //[JB] ShipOFix modifies the offsets and I assume it's bad to add anything so I'm going to let the original code run then rewind to these offsets later to apply my patches.
				XWASubOrderStart = XWA.Position;
				for(j = 0;j < 18;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//Order 2
				ShipOFix(XvT, XWA);
				XvT.Position = XvTPos + 1134;
				for(j = 0;j < 8;j++)
				{
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//WP X
					XvT.Position += 42;
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//WP Y
					XvT.Position += 42;
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//WP Z
					XvT.Position += 42;
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//WP enabled
					XvT.Position -= 132;
				}
				//[JB] Patch wait times
				XvT.Position = XvTSubOrderStart;
				XWA.Position = XWASubOrderStart;
				XvTToXWA_ConvertOrderTime(XvT, XWA);
				//Copy order speed
				XvT.Position = XvTSubOrderStart + 18;
				XWA.Position = XWASubOrderStart + 18;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));
				//[JB] End patch code.

				XWA.Position = XWAPos + 498;
				XvT.Position = XvTPos + 326;
				XvTSubOrderStart = XvT.Position;  //[JB] ShipOFix modifies the offsets and I assume it's bad to add anything so I'm going to let the original code run then rewind to these offsets later to apply my patches.
				XWASubOrderStart = XWA.Position;
				for(j = 0;j < 18;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//Order 3
				ShipOFix(XvT, XWA);
				XvT.Position = XvTPos + 1134;
				for(j = 0;j < 8;j++)
				{
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//WP X
					XvT.Position += 42;
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//WP Y
					XvT.Position += 42;
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//WP Z
					XvT.Position += 42;
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//WP enabled
					XvT.Position -= 132;
				}
				//[JB] Patch wait times
				XvT.Position = XvTSubOrderStart;
				XWA.Position = XWASubOrderStart;
				XvTToXWA_ConvertOrderTime(XvT, XWA);
				//Copy order speed
				XvT.Position = XvTSubOrderStart + 18;
				XWA.Position = XWASubOrderStart + 18;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));
				//[JB] End patch code.
	
				XWA.Position = XWAPos + 646;
				XvT.Position = XvTPos + 408;
				XvTSubOrderStart = XvT.Position;  //[JB] ShipOFix modifies the offsets and I assume it's bad to add anything so I'm going to let the original code run then rewind to these offsets later to apply my patches.
				XWASubOrderStart = XWA.Position;
				for(j = 0;j < 18;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//Order 4
				ShipOFix(XvT, XWA);
				XvT.Position = XvTPos + 1134;
				for(j = 0;j < 8;j++)
				{
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//WP X
					XvT.Position += 42;
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//WP Y
					XvT.Position += 42;
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//WP Z
					XvT.Position += 42;
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//WP enabled
					XvT.Position -= 132;
				}
				//[JB] Patch wait times
				XvT.Position = XvTSubOrderStart;
				XWA.Position = XWASubOrderStart;
				XvTToXWA_ConvertOrderTime(XvT, XWA);
				//Copy order speed
				XvT.Position = XvTSubOrderStart + 18;
				XWA.Position = XWASubOrderStart + 18;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));
				//[JB] End patch code.

				XvT.Position = XvTPos + 490;
				XWA.Position = XWAPos + 2618;
				for(j = 0;j < 4;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//jump to Order 4 T1
				ShipFix(XvT, XWA);
				for(j = 0;j < 4;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//jump to Order 4 T2
				ShipFix(XvT, XWA);
				XWA.Position += 2;
				XvT.Position += 2;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));							//jump to Order 4 T 1AND/OR 2
				XWA.Position = XWAPos + 2826;
				XvT.Position = XvTPos + 501;
				for(j = 0;j < 14;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//Goal 1
				XvT.Position += 64;
				XWA.Position += 66;
				for(j = 0;j < 14;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//Goal 2
				XvT.Position += 64;
				XWA.Position += 66;
				for(j = 0;j < 14;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//Goal 3
				XvT.Position += 64;
				XWA.Position += 66;
				for(j = 0;j < 14;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//Goal 4
				XvT.Position += 64;
				XWA.Position += 66;
				for(j = 0;j < 14;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//Goal 5
				XvT.Position += 64;
				XWA.Position += 66;
				for(j = 0;j < 14;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//Goal 6
				XvT.Position += 64;
				XWA.Position += 66;
				for(j = 0;j < 14;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//Goal 7
				XvT.Position += 64;
				XWA.Position += 66;
				for(j = 0;j < 14;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//Goal 8
				XvT.Position = XvTPos + 1126;
				XWA.Position = XWAPos + 3466;
				for(j = 0;j < 3;j++)
				{
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//SP X
					XvT.Position += 42;
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//SP Y
					XvT.Position += 42;
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//SP Z
					XvT.Position += 42;
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//SP enabled
					XvT.Position -= 132;
				}
				XvT.Position = XvTPos + 1152;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//HYP X
				XvT.Position += 42;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//HYP Y
				XvT.Position += 42;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//HYP Z
				XvT.Position += 42;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//HYP enabled
				XvT.Position = XvTPos + 1286;
				if (XvT.ReadByte() == 1)		//okay, briefing time. if BP enabled..
				{
					//[JB] 0x23F0 (fileheader) + 0xE60 (GlobalGoal[10]) + 0x1306 (Team[10]) = 0x4556 (17750 dec)
					XWA.Position = 17762 + (FGs+2) * 3646 + Messages * 162 + Briefs[0] * 20;	//place for next insert command  [JB] Modified to FGs+2
					XWA.WriteByte(26);XWA.Position++;XWA.WriteByte(Convert.ToByte(Briefs[0]));XWA.Position++;		//Add command
					fgIcons[i] = Briefs[0];		// store the Icon# for the FG
					XvT.Position = XvTPos + 82;XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.Position++;		//Ship
					XvT.Position = XvTPos + 87;XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.Position += 3;		//IFF
					XvT.Position = XvTPos + 1154;
					XWA.WriteByte(28);XWA.Position++;XWA.WriteByte(Convert.ToByte(Briefs[0]));XWA.Position++;		//Move command
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//BP X
					XvT.Position += 42;
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//BP Y
					Briefs[0]++;
				}
				if (this.chkXvtCombat.Checked == true)
				{
					XvT.Position = XvTPos + 1286 + 2;
					if (XvT.ReadByte() == 1)		//okay, briefing time. if BP enabled..
					{
						//[JB] 0x23F0 (fileheader) + 0xE60 (GlobalGoal[10]) + 0x1306 (Team[10]) = 0x4556 (17750 dec)
						XWA.Position = 17750 + (FGs + 2) * 3646 + Messages * 162 + Briefs[1] * 20;	//place for next insert command  [JB] Modified to FGs+2
						XWA.Position += 0x4414 + brief1StringSize + 384 + 0x000A + 2;  //briefing(minus strings) + XvT string list size + 192 shorts for empty messages in XWA + start of Brief2 event list
						XWA.WriteByte(26); XWA.Position++; XWA.WriteByte(Convert.ToByte(Briefs[1])); XWA.Position++;		//Add command
						fgIcons[i] = Briefs[1];		// store the Icon# for the FG
						XvT.Position = XvTPos + 82; XWA.WriteByte(Convert.ToByte(XvT.ReadByte())); XWA.Position++;		//Ship
						XvT.Position = XvTPos + 87; XWA.WriteByte(Convert.ToByte(XvT.ReadByte())); XWA.Position += 3;		//IFF
						XvT.Position = XvTPos + 1154 + 2;  //Offset for BP2
						XWA.WriteByte(28); XWA.Position++; XWA.WriteByte(Convert.ToByte(Briefs[1])); XWA.Position++;		//Move command
						XWA.WriteByte(Convert.ToByte(XvT.ReadByte())); XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//BP X
						XvT.Position += 42;
						XWA.WriteByte(Convert.ToByte(XvT.ReadByte())); XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//BP Y
						Briefs[1]++;
					}
				}
				XWA.Position = XWAPos + 3527;
				XvT.Position = XvTPos + 1315;
				for(j = 0;j < 4;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//CM -> Global Unit
				XWA.Position++;
				XvT.Position += 9;
				for(j = 0;j < 48;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//Optionals
				XvT.Position = XvTPos + 1378;
				XWA.Position = XWAPos + 3646;
			}
			Random rnd = new Random();
			int craft1 = rnd.Next(1, 59);
			int craft2 = rnd.Next(63, 102);
			short[] coord1 = {0, 0, 0};
			short[] coord2 = {0, 0, 0};
			coord1[rnd.Next(0, 2)] = 1;
			coord2[rnd.Next(0, 2)] = -1;

			//okay, now write in the default Backdrop
			XWAPos = XWA.Position;
			XWA.WriteByte(49);XWA.WriteByte(46);XWA.WriteByte(48);XWA.WriteByte(32);XWA.WriteByte(49);XWA.WriteByte(46);XWA.WriteByte(48);XWA.WriteByte(32);XWA.WriteByte(49);XWA.WriteByte(46);XWA.WriteByte(48);  //Name = 1.0 1.0 1.0
			XWA.Position = XWAPos + 20;
			XWA.WriteByte(255);XWA.WriteByte(255);XWA.Position += 3;XWA.WriteByte(255);XWA.WriteByte(255);  //EnableDesignation1, EnableDesignation2, ... GlobalCargoIndex GlobalSpecialCargoIndex 
			XWA.Position = XWAPos + 40;
			XWA.WriteByte(49);XWA.WriteByte(46);XWA.WriteByte(48);  //Brightness = 1.0
			XWA.Position = XWAPos + 60;
			XWA.WriteByte(49);XWA.WriteByte(46);XWA.WriteByte(57);  //Size = 1.9   (match skirmish output)
			XWA.Position = XWAPos + 105;XWA.WriteByte(2);           //SpecialCargoCraft?
			XWA.Position++;XWA.WriteByte(183);XWA.WriteByte(1);
			XWA.Position += 3;XWA.WriteByte(4); //[JB] Changed to IFF Red since it is used less frequently, and is less likely to interfere with IFF triggers.
			XWA.Position = XWAPos + 113;XWA.WriteByte(9);   //[JB] Team (so it doesn't interfere with triggers)
			XWA.Position = XWAPos + 120;XWA.WriteByte(31);  //[JB] Global group (for same reason)
			XWA.Position = XWAPos + 2827;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);
			XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;
			//XWA.Position = XWAPos + 3466;XWA.WriteByte(1);XWA.Position++;XWA.WriteByte(0);XWA.Position++;XWA.WriteByte(0);XWA.Position++;XWA.WriteByte(1);  //[JB] Changed to xyz (0.01, 0, 0)
			XWA.Position = XWAPos + 3466;WriteInt(XWA, coord1[0]);WriteInt(XWA, coord1[1]);WriteInt(XWA, coord1[2]);XWA.Position++;XWA.WriteByte(1);
			XWA.Position = XWAPos + 3602;XWA.WriteByte((byte)craft1); //[JB] Set backdrop value to 20
			XWA.Position = XWAPos + 3646;

			//[JB] Adding a second backdrop, since the game generates two backdrops for skirmish files.  Offers better ambient lighting for the player.
			XWAPos = XWA.Position;
			XWA.WriteByte(49);XWA.WriteByte(46);XWA.WriteByte(48);XWA.WriteByte(32);XWA.WriteByte(49);XWA.WriteByte(46);XWA.WriteByte(48);XWA.WriteByte(32);XWA.WriteByte(49);XWA.WriteByte(46);XWA.WriteByte(48);
			XWA.Position = XWAPos + 20;
			XWA.WriteByte(255);XWA.WriteByte(255);XWA.Position += 3;XWA.WriteByte(255);XWA.WriteByte(255);  //[JB] Modified to update both global cargo values.
			XWA.Position = XWAPos + 40;
			XWA.WriteByte(49);XWA.WriteByte(46);XWA.WriteByte(48);
			XWA.Position = XWAPos + 60;
			XWA.WriteByte(49);XWA.WriteByte(46);XWA.WriteByte(48);
			XWA.Position = XWAPos + 105;XWA.WriteByte(2);
			XWA.Position++;XWA.WriteByte(183);XWA.WriteByte(1);
			XWA.Position += 3;XWA.WriteByte(4);
			XWA.Position = XWAPos + 113;XWA.WriteByte(9);   //[JB] Team (so it doesn't interfere with triggers)
			XWA.Position = XWAPos + 120;XWA.WriteByte(31);  //[JB] Global group (for same reason)
			XWA.Position = XWAPos + 2827;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);
			XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;
			//XWA.Position = XWAPos + 3466;XWA.WriteByte(0);XWA.Position++;XWA.WriteByte(0);XWA.Position++;XWA.WriteByte(1);XWA.Position++;XWA.WriteByte(1);  //At xyz (0, 0, 0.01), raw=0,0,1
			XWA.Position = XWAPos + 3466;WriteInt(XWA, coord2[0]);WriteInt(XWA, coord2[1]);WriteInt(XWA, coord2[2]);XWA.Position++;XWA.WriteByte(1);
			XWA.Position = XWAPos + 3602;XWA.WriteByte((byte)craft2); //[JB] Set backdrop value to 90
			XWA.Position = XWAPos + 3646;

			#endregion
			//[JB] Now that flight groups are done, check for player count and patch in skirmish mode
			if (this.chkXvtCombat.Checked == true && PlayerCraft > 1)
			{
				long backupPos = XWA.Position;
				XWA.Position = 0x23AC;
				XWA.WriteByte(4);
				XWA.Position = backupPos;
			}
			#region Messages
			for(i = 0;i < Messages;i++)
			{
				XvTPos = XvT.Position;
				XWAPos = XWA.Position;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//Message# - 1
				switch(XvT.ReadByte())											//takes care of colors if needed
				{
					case 49:
						for(j = 0;j < 63;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//green
						XWA.Position = XWAPos + 142;XWA.WriteByte(0);
						break;
					case 50:
						for(j = 0;j < 63;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//blue
						XWA.Position = XWAPos + 142;XWA.WriteByte(2);
						break;
					case 51:
						for(j = 0;j < 63;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//yellow
						XWA.Position = XWAPos + 142;XWA.WriteByte(3);
						break;
					default:
						XvT.Position--;
						for(j = 0;j < 64;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//red
						XWA.Position = XWAPos + 142;XWA.WriteByte(1);
						break;
				}
				XWA.Position = XWAPos + 82;
				for(j = 0;j < 14;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//Sent to.. -> T1
				ShipFix(XvT, XWA);
				for(j = 0;j < 4;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));			//T2
				ShipFix(XvT, XWA);
				XvT.Position += 2;
				XWA.Position += 2;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));								//T1 AND/OR T2
				XWA.Position++;
				for(j = 0;j < 4;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));			//T3
				ShipFix(XvT, XWA);
				for(j = 0;j < 4;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));			//T4
				ShipFix(XvT, XWA);
				XvT.Position += 2;
				XWA.Position += 2;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));								//T3 AND/OR T4
				XWA.Position = XWAPos + 141;
				XvT.Position += 17;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));								//T (1/2) AND/OR (3/4)
				XWA.Position = XWAPos + 132;         //[JB] OriginatingFG
				XWA.WriteByte(Convert.ToByte(FGs+1));  //[JB] Set to last FG (+2 inserted backdrops so the last new FG index is FG+1). Assigning messages to backdrops ensures the object is always present so messages always fire.
				XWA.Position = XWAPos + 140;
				XvT.Position = XvTPos + 114;
				int msgDelaySec = XvT.ReadByte() * 5;  //[JB] Corrected delay time.
				XWA.WriteByte(Convert.ToByte(msgDelaySec % 60));						//Delay
				XWA.WriteByte(Convert.ToByte(msgDelaySec / 60));
				XWA.Position += 2; XWA.WriteByte(10);  //[JB] Modified offset for second delay byte
				XWA.Position += 5; XWA.WriteByte(10);										//make sure the Cancel triggers are set to FALSE
				XWA.Position = XWAPos + 162;
				XvT.Position = XvTPos + 116;
			}
			#endregion
			#region Global Goals
			XvTPos = XvT.Position;
			XWAPos = XWA.Position;
			for (int ti = 0; ti < 10; ti++) //[JB] Converting all 10 teams just in case some triggers depend on them.
			{
				XvT.Position = XvTPos + (0x80 * ti);
				XWA.Position = XWAPos + (0x170 * ti);
				XWA.WriteByte(3);
				XvT.Position++;
				for (i = 0; i < 5; i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//Prim T1
				ShipFix(XvT, XWA);
				for (i = 0; i < 4; i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//PT2
				ShipFix(XvT, XWA);
				XWA.Position += 2;
				XvT.Position += 2;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));							//PT 1 AND/OR 2
				XWA.Position++;
				for (i = 0; i < 4; i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//PT 3
				ShipFix(XvT, XWA);
				for (i = 0; i < 4; i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//PT 4
				ShipFix(XvT, XWA);
				XvT.Position += 2;
				XWA.Position += 2;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));							//PT 3 AND/OR 4
				XvT.Position += 17;
				XWA.Position += 18;
				for (i = 0; i < 3; i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//PT (1/2) AND/OR (3/4) -> Points
				XWA.Position += 70;
				for (i = 0; i < 4; i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//Prev T1
				ShipFix(XvT, XWA);
				for (i = 0; i < 4; i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//PT2
				ShipFix(XvT, XWA);
				XWA.Position += 2;
				XvT.Position += 2;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));							//PT 1 AND/OR 2
				XWA.Position++;
				for (i = 0; i < 4; i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//PT 3
				ShipFix(XvT, XWA);
				for (i = 0; i < 4; i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//PT 4
				ShipFix(XvT, XWA);
				XvT.Position += 2;
				XWA.Position += 2;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));							//PT 3 AND/OR 4
				XvT.Position += 17;
				XWA.Position += 18;
				for (i = 0; i < 3; i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//PT (1/2) AND/OR (3/4) -> Points
				XWA.Position += 70;
				for (i = 0; i < 4; i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//Sec T1
				ShipFix(XvT, XWA);
				for (i = 0; i < 4; i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//ST2
				ShipFix(XvT, XWA);
				XWA.Position += 2;
				XvT.Position += 2;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));							//ST 1 AND/OR 2
				XWA.Position++;
				for (i = 0; i < 4; i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//ST 3
				ShipFix(XvT, XWA);
				for (i = 0; i < 4; i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//ST 4
				ShipFix(XvT, XWA);
				XvT.Position += 2;
				XWA.Position += 2;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));							//ST 3 AND/OR 4
				XvT.Position += 17;
				XWA.Position += 18;
				for (i = 0; i < 3; i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//ST (1/2) AND/OR (3/4) -> Points
				XWA.Position += 70;
			}
			XvT.Position = XvTPos + (0x80 * 10);  //10 teams
			XWA.Position = XWAPos + (0x170 * 10);
			#endregion
			//XvT.Position += 1152;  //[JB] Removed
			#region IFF/Teams
			for(i = 0;i < 4870;i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//well, that was simple..
			#endregion
			#region Briefing
			XWAPos = XWA.Position;
			long XWABriefing1 = XWAPos;
			for(i = 0;i < 6;i++) { XWA.WriteByte(Convert.ToByte(XvT.ReadByte())); }	//briefing intro
			WriteInt(XWA, ReadInt(XvT) + 10*Briefs[0]);	// adjust length for add/moves
			XvT.Position += 2;		
			XWA.Position += 20 * Briefs[0] + 2;
			for(i = 0;i < 0x32A;i++) { XWA.WriteByte(Convert.ToByte(XvT.ReadByte())); }	//briefing content
			XWA.Position = XWAPos;
			j = ReadInt(XWA) * 0x19 / 0x14;		// adjust overall briefing length
			XWA.Position -= 2;
			WriteInt(XWA, j);
			XWA.Position += 8;
			for (i=0;i<0x320;i+=4)		// work our way through length of briefing. i automatically increases by 4 per event
			{
				j = ReadInt(XWA);
				if (j == 0x270F) break;		// stop check at t=9999, end briefing
				j = j * 0x19 / 0x14;
				XWA.Position -= 2;
				WriteInt(XWA,j);
				j = ReadInt(XWA);		// now get the event type
				if (j>8 && j<17)		// FG tags 1-8
				{
					j = ReadInt(XWA);	// FG number
					XWA.Position -= 2;
					WriteInt(XWA, fgIcons[j]);	// Overwrite with the Icon#
					i += 2;
				}
				else if (j == 7)		// Zoom map command
				{
					j = ReadInt(XWA) * 124 / 58;	// X
					XWA.Position -= 2;
					WriteInt(XWA, j);
					j = ReadInt(XWA) * 124 / 88;	// Y
					XWA.Position -= 2;
					WriteInt(XWA, j);
					i += 4;
				}
				else 
				{
					XWA.Position += 2*BRF[j];		// skip over vars
					i += 2*BRF[j];	// increase length counter by skipped vars
				}
			}
			XWA.Position = 0x8960 + (FGs+2)*0xE3E + Messages*0xA2;  //[JB] FGs+2
			XWA.WriteByte(1);					//show the non-existant briefing
			XWA.Position += 9;
			#endregion
			#region Briefing tags & strings
			for(i = 0;i < 32;i++)	//tags
			{
				j = XvT.ReadByte() + 256 * XvT.ReadByte();		//check length..
				XWA.WriteByte(Convert.ToByte(j)); XWA.WriteByte(0);				//..write length..
				if(j != 0)										//and copy if not 0
				{
					int k;
					for(k = 0;k < j;k++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));
				}
			}
			XWA.Position += 192;
			for(i = 0;i < 32;i++)	//strings
			{
				j = XvT.ReadByte() + 256 * XvT.ReadByte();		//check length..
				XWA.WriteByte(Convert.ToByte(j)); XWA.WriteByte(0);				//..write length..
				if(j != 0)										//and copy if not 0
				{
					int k;
					for(k = 0;k < j;k++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));
				}
			}
			XWA.Position += 192;

			//[JB] Begin briefing 2.  Basically just copy/paste the same code.
			if (this.chkXvtCombat.Checked == true)
			{
				long XWABriefing2 = XWA.Position;
				XWAPos = XWA.Position;
				for (i = 0; i < 6; i++) { XWA.WriteByte(Convert.ToByte(XvT.ReadByte())); }	//briefing intro
				WriteInt(XWA, ReadInt(XvT) + 10 * Briefs[1]);	// adjust length for add/moves
				XvT.Position += 2;
				XWA.Position += 20 * Briefs[1] + 2;
				for (i = 0; i < 0x32A; i++) { XWA.WriteByte(Convert.ToByte(XvT.ReadByte())); }	//briefing content
				XWA.Position = XWAPos;
				j = ReadInt(XWA) * 0x19 / 0x14;		// adjust overall briefing length
				XWA.Position -= 2;
				WriteInt(XWA, j);
				XWA.Position += 8;
				for (i = 0; i < 0x320; i += 4)		// work our way through length of briefing. i automatically increases by 4 per event
				{
					j = ReadInt(XWA);
					if (j == 0x270F) break;		// stop check at t=9999, end briefing
					j = j * 0x19 / 0x14;
					XWA.Position -= 2;
					WriteInt(XWA, j);
					j = ReadInt(XWA);		// now get the event type
					if (j > 8 && j < 17)		// FG tags 1-8
					{
						j = ReadInt(XWA);	// FG number
						XWA.Position -= 2;
						WriteInt(XWA, fgIcons[j]);	// Overwrite with the Icon#
						i += 2;
					}
					else if (j == 7)		// Zoom map command
					{
						j = ReadInt(XWA) * 124 / 58;	// X
						XWA.Position -= 2;
						WriteInt(XWA, j);
						j = ReadInt(XWA) * 124 / 88;	// Y
						XWA.Position -= 2;
						WriteInt(XWA, j);
						i += 4;
					}
					else
					{
						XWA.Position += 2 * BRF[j];		// skip over vars
						i += 2 * BRF[j];	// increase length counter by skipped vars
					}
				}
				XWA.Position = 0x8960 + (XWABriefing2 - XWABriefing1) + (FGs + 2) * 0xE3E + Messages * 0xA2;   //[JB] FGs+2
				XWA.WriteByte(0);					//show the non-existant briefing
				XWA.WriteByte(1);					//show the non-existant briefing
				XWA.Position += 8;
			#endregion
				#region Briefing tags & strings
				for (i = 0; i < 32; i++)	//tags
				{
					j = XvT.ReadByte() + 256 * XvT.ReadByte();		//check length..
					XWA.WriteByte(Convert.ToByte(j)); XWA.WriteByte(0);				//..write length..
					if (j != 0)										//and copy if not 0
					{
						int k;
						for (k = 0; k < j; k++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));
					}
				}
				XWA.Position += 192;
				for (i = 0; i < 32; i++)	//strings
				{
					j = XvT.ReadByte() + 256 * XvT.ReadByte();		//check length..
					XWA.WriteByte(Convert.ToByte(j)); XWA.WriteByte(0);				//..write length..
					if (j != 0)										//and copy if not 0
					{
						int k;
						for (k = 0; k < j; k++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));
					}
				}
				XWA.Position += 192;
			}
			else
			{
				XvT.Position += 0x334;    //Jump to tags
				for(j = 0; j < 64; j++)   //32 tags + 32 strings
				{
					int len = ReadInt(XvT);
					XvT.Position += len;
				}
				XWA.Position += 0x4614;  //Empty briefing plus empty tags/strings
			}
			//End briefing 2.
			#endregion
			//XWA.Position += 50708 - 0x4614; 				//([JB]: no longer pass over empty briefing 2 which would be 0x4614 bytes), bunch of unknown space, message comments
			XWA.Position += 0x187C; //Skip EditorNotes
			XWA.Position += 0x3200; //Skip BriefingStringNotes
			XWA.Position += 0x1900; //Skip MessageNotes
			XWA.Position += 0xBB8;  //Skip EomNotes
			XWA.Position += 0xFA0;  //Skip Unknown
			XWA.Position += 0x12C;  //Skip DescriptionNotes

			//[JB] Briefings have variable length. Need to step over the remaining 6 XvT briefings by properly calculating how big they are.
			for (i = 2; i < 8; i++)
			{
				XvT.Position += 0x334;    //Jump to tags
				for(j = 0; j < 64; j++)   //32 tags + 32 strings
				{
					int len = ReadInt(XvT);
					XvT.Position += len;
				}
			}
			//XvT.Position += 6636
			#region FG Goal strings
			for(i = 0;i < FGs;i++)
			{
				for(j = 0;j < 24;j++)  //8 goals * 3 strings
				{
					if (XvT.ReadByte() == 0) 
					{
						XWA.Position++;
						XvT.Position += 63;
					}
					else
					{
						int k;
						XvT.Position--;
						for(k = 0;k < 64;k++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));
					}
				}
			}
			XWA.Position += 48;						//compensate for adding the Backdrop  [JB] Was 24 for one backdrop, needs to be 48 since I added an extra one.
			#endregion
			#region Global Goal strings
			for(i = 0;i < 10;i++)
			{
				for(j = 0;j < 36;j++)
				{
					if (XvT.ReadByte() == 0) 
					{
						XWA.Position++;
						XvT.Position += 63;
					}
					else
					{
						int k;
						XvT.Position--;
						for(k = 0;k < 64;k++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));
					}
				}
				XvT.Position += 3072;
			}
			#endregion
			XWA.Position += 3552;				//skip custom order strings
			#region Debrief and Descrip
			if (lblType.Text == "XvT")
			{
				XWA.Position += 4096;
				XWA.WriteByte(35);
				XWA.Position += 4095;
				XWA.WriteByte(35);
				for(i = 0;i < 1024;i++)
				{
					int d = XvT.ReadByte();
					j = 1024 - i;
					switch (d)
					{
						case -1:
							i = 1024;
							break;
						case 0:
							i = 1024;
							break;
						case 10:
							XWA.WriteByte(35);
							break;
						default:
							XvT.Position--;
							XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));
							break;
					}
				}
				XWA.Position += 3071 + j;
			}
			else
			{
				for(i = 0;i < 4096;i++) { XWA.WriteByte(Convert.ToByte(XvT.ReadByte())); }	// Debrief
				XWA.WriteByte(35);
				for(i = 0;i < 4095;i++) { XWA.WriteByte(Convert.ToByte(XvT.ReadByte())); }	// Hints
				XvT.Position++;
				XWA.WriteByte(35);
				for(i = 0;i < 4095;i++) { XWA.WriteByte(Convert.ToByte(XvT.ReadByte())); }	// Brief/Description
			}
			#endregion
			XWA.WriteByte(0);				//EOF
			XvT.Close();
			XWA.Close();
			if (T2W == false && hidden == false) MessageBox.Show("Conversion completed", "Finished");
		}
		
		void ShipFix(FileStream In, FileStream XWA)		//checks for Ship Type trigger, adjusts value
		{
			In.Position -= 3;
			if (In.ReadByte() == 2)
			{
				XWA.Position -= 2;
				XWA.WriteByte(Convert.ToByte(In.ReadByte() + 1));
				XWA.Position++;
				In.Position++;
			}
			else { In.Position += 2; }
			XWA.Position += 2;
		}
		void ShipOFix(FileStream In, FileStream XWA)	//seperate function for Orders
		{
			In.Position -= 12;
			if (In.ReadByte() == 2)				//Target 3
			{
				In.Position++;
				XWA.Position -= 10;
				XWA.WriteByte(Convert.ToByte(In.ReadByte() + 1));
				In.Position -= 2;
			}
			else { XWA.Position -= 9; }
			if (In.ReadByte() == 2)				//Target 4
			{
				In.Position++;
				XWA.WriteByte(Convert.ToByte(In.ReadByte() + 1));
				In.Position += 2;
				XWA.Position += 3;
			}
			else 
			{ 
				XWA.Position += 4; 
				In.Position += 4;
			}
			if (In.ReadByte() == 2)				//Target 1
			{
				XWA.WriteByte(Convert.ToByte(In.ReadByte() + 1));
				XWA.Position++;
			}
			else 
			{ 
				In.Position++;
				XWA.Position += 2;
			}
			if (In.ReadByte() == 2)				//Target 2
			{
				XWA.WriteByte(Convert.ToByte(In.ReadByte() + 1));
				In.Position += 2;
				XWA.Position += 4;
			}
			else 
			{ 
				In.Position += 3;
				XWA.Position += 5;
			}
		}

		byte ConvertOrderTimeXvTToXWA(byte xvtTime)
		{
			//XWA time value, if 20 (decimal) or under is exact seconds.
			//21 = 25 sec, 22 = 30 sec, etc.
			int seconds = xvtTime * 5;
			if(seconds <= 20)
				return Convert.ToByte(seconds);
			return Convert.ToByte(((seconds - 20) / 5) + 20);
		}
		void XvTToXWA_ConvertOrderTime(FileStream XvT, FileStream XWA)
		{
			long curXvT = XvT.Position;
			long curXWA = XWA.Position;
			byte orderEnum = (byte)XvT.ReadByte();
			XvT.Position++;
			byte var1 = (byte)XvT.ReadByte();
			XWA.Position += 2;
			switch(orderEnum)
			{
			case 0x0C:   //Board and Give Cargo
			case 0x0D:   //Board and Take Cargo
			case 0x0E:   //Board and Exchange Cargo
			case 0x0F:   //Board and Capture Cargo
			case 0x10:   //Board and Destroy Cargo
			case 0x11:   //Pick up
			case 0x12:   //Drop off   (Deploy time?)
			case 0x13:   //Wait
			case 0x14:   //SS Wait
			case 0x1C:   //SS Hold Steady
			case 0x1E:   //SS Wait
			case 0x1F:   //SS Board
			case 0x20:   //Board to Repair
			case 0x24:   //Self-destruct
				XWA.WriteByte(ConvertOrderTimeXvTToXWA(var1));
				break;
			default:
				break;
			}

			XvT.Position = curXvT;
			XWA.Position = curXWA;
		}

		public static int ReadInt(Stream file)
		{
			int i;
			i = (file.ReadByte() + file.ReadByte() * 256);
			return i;
		}
		public static void WriteInt(Stream file, int num)
		{
			file.WriteByte((byte)(num%256));
			file.WriteByte((byte)(num/256));
		}
		public static void WriteInt(Stream file, short num)
		{
			file.WriteByte((byte)(num & 0xFF));
			file.WriteByte((byte)((num & 0xFF00) >> 8));
		}
	}
}
