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
 * [FIX] overflow in convertOrderTimeXvTToXWA()
 * [FIX] missing "l" from "Imperial" in tie2XvT Team name
 * [FIX] arrival difficulty in xvt2XWA
 * [UPD] default save name suggested per original and platform
 * [NEW] IFF name conversion
 * [FIX] skipping empty XvT briefings, which fixed xvt2XWA FG/Global Goal strings and Mission Desc/EoM
 * [FIX] xvt2WA global goals
 * [FIX] removed shipFix and shipOFix entirely, since it was breaking the offset, not fixing it (likely due to changes in Platform/YOGEME)
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
using System.Collections.Generic;

namespace Idmr.Converter
{
	/// <summary>
	/// X-wing series mission converter (TIE -> XvT, TIE -> XWA, XvT -> XWA)
	/// </summary>
	public partial class MainForm : Form
	{
		public static bool T2W = false;
		static bool _hidden = false;
		readonly static short[] _brf = {0,0,0,0,1,1,2,2,0,1,1,1,1,1,1,1,1,0,4,4,4,4,4,4,4,4,3,2,3,2,1,0,0,0,0};

		public MainForm(string[] args, bool hidden)
		{
			InitializeComponent();
			_hidden = hidden;
			if (_hidden == true)		//cmdline run
			{
				Hide();		//don't need the form, just the txt
				txtExist.Text = args[0];
				try
				{
					if (File.Exists(args[0]) == false) throw new Exception("Cannot locate original file.");
					FileStream test;
					test = File.OpenRead(txtExist.Text);
					int d = test.ReadByte();	//check platform, really only make sure it's not XWA and legit
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
					test.Close();
					txtSave.Text = args[1];
					switch (args[2])		//check mode
					{
						case "1":
							if (d == 255) tie2XvT();
							else throw new Exception("Invalid conversion type for file specified.");
							break;
						case "2":
							chkXWA.Checked = true;
							if (d == 255) tie2XWA();
							else throw new Exception("Invalid conversion type for file specified.");
							break;
						case "3":
							chkXWA.Checked = true;
							if (d == 12 || d == 14) xvt2XWA();
							else throw new Exception("Invalid conversion type for file specified.");
							break;
						default:
							throw new Exception("Incorrect parameter usage. Correct usage is as follows:\nOriginal path, new path, mode\nModes: 1 - TIE to XvT, 2 - TIE to XWA, 3 - XvT to XWA");
					}
				}
				catch (Exception x)
				{
					MessageBox.Show(x.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				finally { Application.Exit(); }
			}
		}

		#region Check boxes
		void chkXvT2CheckedChanged(object sender, EventArgs e)
		{
			chkXWA.Checked = !chkXvT2.Checked;
			chkXvtBop.Visible = chkXvT2.Checked;
		}
		
		void chkXWACheckedChanged(object sender, EventArgs e)
		{
			chkXvT2.Checked = !chkXWA.Checked;
			chkXvtBop.Visible = !chkXWA.Checked; //[JB] Updated to hide BoP checkbox.
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
			if (chkXvT2.Checked)
			{
				if (chkXvtBop.Checked) fileName += "_BoP";
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
		
		void savConvertFileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			txtSave.Text = savConvert.FileName;
		}
		
		void cmdConvertClick(object sender, EventArgs e)
		{
			T2W = false;
			if (txtExist.Text == "" | txtSave.Text == "") { return; }
			if (lblType.Text == "TIE")
			{
				if (chkXvT2.Checked) { tie2XvT(); }
				if (chkXWA.Checked) { tie2XWA(); }
			}
			if (lblType.Text == "XvT" | lblType.Text == "BoP") { xvt2XWA(); }
		}
		#endregion

		//TODO: Some of the delays may still be off due to different conversion factors

		void tie2XvT()
		{
			FileStream TIE, XvT;
			TIE = File.OpenRead(txtExist.Text);
			XvT = File.Open(txtSave.Text, FileMode.Create, FileAccess.ReadWrite);
			BinaryReader br = new BinaryReader(TIE);
			BinaryWriter bw = new BinaryWriter(XvT);
			BinaryReader brXvT = new BinaryReader(XvT);
			long XvTPos;
			byte format = 12;  //[JB] Added platform selection for XvT/BoP
			if (chkXvtBop.Checked) format = 14;
			XvT.WriteByte(format);			//XvT format

			XvT.Position = 2;
			TIE.Position = 2;
			short i, j;
			bw.Write(br.ReadInt32());			//# of FGs and messages
			TIE.Position = 2;
			short FGs = br.ReadInt16();			//store them
			short Messages = br.ReadInt16();
			if (chkXvtBop.Checked)  //[JB] patch in
			{
				XvT.Position = 0x64;
				XvT.WriteByte(3);  //MPtraining
			}
			TIE.Position = 0x19A;
			XvT.Position = 0x14;
			for (i = 0; i < 4; i++)	// IFF 3-6 names
			{
				XvTPos = XvT.Position;
				bw.Write(br.ReadBytes(12));
				XvT.Position = XvTPos + 20;
			}
			TIE.Position = 0x1CA;
			XvT.Position = 0xA4;
			#region Flight Groups
			for (i = 0; i < FGs; i++)       //Flight Groups
			{
				long TIEPos = TIE.Position;
				XvTPos = XvT.Position;
				bw.Write(br.ReadBytes(12));     //FG Name
				TIE.Position = TIEPos + 24;
				XvT.Position = XvTPos + 40;
				bw.Write(br.ReadBytes(12));     //Cargo
				XvT.Position = XvTPos + 60;
				bw.Write(br.ReadBytes(12));     //Special Cargo
				XvT.Position = XvTPos + 80;
				bw.Write(br.ReadBytes(8));      //SC ship# -> IFF
				TIE.Position -= 1;
				// if player is Imperial, have to switch Team/IFF values, since Imps have to be Team 1 (0) and Rebs are Team 2 (1)
				XvT.Position = XvTPos + 88;
				switch (TIE.ReadByte())
				{
					case 0:
						if (optImp.Checked) XvT.WriteByte(1);
						else XvT.WriteByte(0);
						break;
					case 1:
						if (optImp.Checked) XvT.WriteByte(0);
						else XvT.WriteByte(1);
						break;
					default:
						TIE.Position--;
						XvT.WriteByte(br.ReadByte());
						break;
				}
				bw.Write(br.ReadBytes(9));      //AI -> # of waves
				TIE.Position += 1;
				XvT.Position += 2;
				if (TIE.ReadByte() != 0)
				{
					XvT.WriteByte(1);                                                       //Player 1
					XvT.Position++;
					TIE.Position--;
					XvT.WriteByte(Convert.ToByte(TIE.ReadByte() - 1));                      //Player's craft #
				}
				else XvT.Position += 3;
				bw.Write(br.ReadBytes(3));      //Orientation values
				TIE.Position = TIEPos + 73;
				XvT.Position = XvTPos + 109;
				bw.Write(br.ReadBytes(9));      //Arrival diff, trigger 1&2
				XvT.Position += 2;
				XvT.WriteByte(br.ReadByte());                              //trigger 1 AND/OR 2
				TIE.Position += 1;
				XvT.Position = XvTPos + 134;
				bw.Write(br.ReadBytes(6));      //Arrv delay -> Dep trigger
				TIE.Position += 2;
				XvT.Position += 9;
				XvT.WriteByte(br.ReadByte());                              //Abort trigger
				TIE.Position += 3;
				XvT.Position += 4;
				bw.Write(br.ReadBytes(8));      //Arrv/Dep methods
				bw.Write(br.ReadBytes(18));     //Order 1
				XvT.Position += 64;
				bw.Write(br.ReadBytes(18));     //Order 2
				XvT.Position += 64;
				bw.Write(br.ReadBytes(18));     //Order 3
				XvT.Position = XvTPos + 502;
				j = br.ReadByte();
				if (j == 9)                                                                 // if EXIST
				{
					XvT.Position--;
					XvT.WriteByte(1);                                                       // must NOT
					XvT.WriteByte(2);                                                       // be destroyed
				}
				else XvT.WriteByte(Convert.ToByte(j));                                      //Primary Goal
				byte b = br.ReadByte();
				if (b == 1) b++;    // 50%
				else if (b >= 2) b += 2;    // >=1 and on
				XvT.WriteByte(b);                                                           //Amount
				XvT.WriteByte(Convert.ToByte((j != 10 && j != 9) ? 1 : 0));                 //250 points  [JB] Changed to 250 points only if a condition exists (not "FALSE") but also not "must have survived"
				if (j != 10) XvT.WriteByte(1);                                              //Enable Primary Goal
				else XvT.WriteByte(0);
				XvT.Position += 73;
				XvT.WriteByte(2);                                                           //Secondary Goal
				j = br.ReadByte();
				if (j == 9)                                                                 // if EXIST
				{
					XvT.Position--;
					XvT.WriteByte(3);                                                       // BONUS must NOT
					XvT.WriteByte(2);                                                       // be destroyed
				}
				else XvT.WriteByte(Convert.ToByte(j));
				b = br.ReadByte();
				if (b == 1) b++;    // 50%
				else if (b >= 2) b += 2;    // >=1 and on
				XvT.WriteByte(b);
				XvT.WriteByte(Convert.ToByte((j != 10) ? 1 : 0));                           //250 points  [JB] Changed to 250 points only if a condition exists (not "FALSE")
				if (j != 10) XvT.WriteByte(1);
				else XvT.WriteByte(0);
				XvT.Position += 73;
				XvT.WriteByte(2);                                                           //Secret Goal
				j = br.ReadByte();
				if (j == 9)                                                                 // if EXIST
				{
					XvT.Position--;
					XvT.WriteByte(3);                                                       // BONUS must NOT
					XvT.WriteByte(2);                                                       // be destroyed
				}
				else XvT.WriteByte(Convert.ToByte(j));
				b = br.ReadByte();
				if (b == 1) b++;    // 50%
				else if (b >= 2) b += 2;    // >=1 and on
				XvT.WriteByte(b);
				XvT.Position += 1;
				if (j != 10) XvT.WriteByte(1);
				else XvT.WriteByte(0);
				XvT.Position += 73;
				XvT.WriteByte(2);                                                           //Bonus Goal
				j = br.ReadByte();
				if (j == 9)                                                                 // if EXIST
				{
					XvT.Position--;
					XvT.WriteByte(3);                                                       // BONUS must NOT
					XvT.WriteByte(2);                                                       // be destroyed
				}
				else XvT.WriteByte(Convert.ToByte(j));
				b = br.ReadByte();
				if (b == 1) b++;    // 50%
				else if (b >= 2) b += 2;    // >=1 and on
				XvT.WriteByte(b);
				XvT.WriteByte(br.ReadByte());      //Bonus points, will need fiddling
				if (j != 10) XvT.WriteByte(1);
				else XvT.WriteByte(0);
				XvT.Position += 74;
				XvT.WriteByte(10);                                  //10 is the 'null' trigger (goal 5)
				XvT.Position += 77;
				XvT.WriteByte(10);                                  //goal 6
				XvT.Position += 77;
				XvT.WriteByte(10);                                  //goal 7
				XvT.Position += 77;
				XvT.WriteByte(10);                                  //goal 8
				TIE.ReadByte();
				XvT.Position = XvTPos + 1126;
				bw.Write(br.ReadBytes(30));     //X points
				XvT.Position += 14;
				bw.Write(br.ReadBytes(30));     //Y points
				XvT.Position += 14;
				bw.Write(br.ReadBytes(30));     //Z points
				XvT.Position += 14;
				bw.Write(br.ReadBytes(30));     //Enable points
				XvT.Position += 90;                                                         //goto End
				TIE.Position += 4;
			}
			#endregion
			#region Messages
			for (i = 0; i < Messages; i++)
			{
				XvT.WriteByte(Convert.ToByte(i));
				XvT.Position++;
				bw.Write(br.ReadBytes(64)); //Color & message
				XvT.WriteByte(1);
				XvT.Position += 9;
				bw.Write(br.ReadBytes(8));  //triggers 1 & 2
				XvT.Position++;
				TIE.Position += 17;
				XvT.WriteByte(br.ReadByte());                          //trigger 1 AND/OR 2
				TIE.Position -= 2;
				XvT.Position += 28;
				XvT.WriteByte(br.ReadByte());                          //Delay
				TIE.Position++;
				XvT.Position++;
			}
			#endregion
			XvT.WriteByte(3);XvT.Position++;	//Unknown
			#region Global Goals
			bw.Write(br.ReadBytes(8));			//Prim Goal trigger 1 & 2
			XvT.Position += 2;
			TIE.Position += 17;
			XvT.WriteByte(br.ReadByte());		//Prim trigger 1 AND/OR 2
			XvT.Position += 73;
			TIE.Position += 2;
			bw.Write(br.ReadBytes(8));			//Sec Goal trigger 1 & 2
			XvT.Position += 2;
			TIE.Position += 17;
			XvT.WriteByte(br.ReadByte());		//Sec Goal trigger A/O

			//[JB] Convert TIE bonus global goals into a second set of XvT secondary goals
			TIE.Position += 2; //Skip 2 bytes after the previous and/or byte to reach the start of the bonus goal triggers
			bw.Write(br.ReadBytes(8));			//Bonus Goal trigger 1 & 2
			XvT.Position += 2;
			TIE.Position += 17;
			XvT.WriteByte(br.ReadByte());		//Bonus Goal trigger A/O
			XvT.Position += 17;
			XvT.WriteByte(0);   //And/Or
			XvT.Position += 2;  //Jump to the end of the Global Goal block, which happens to be the start of the second teams's global goal block
			XvTPos = XvT.Position;

			//[JB] Patch to convert all "must be FALSE" conditions to TRUE so that and/or doesn't cause conflicts
			XvT.Position -= 0x2A; //Rewind back to start of XvT Secondary, Trigger 1
			if (XvT.ReadByte() == 0x0A) { XvT.Position--; XvT.WriteByte(0); }  //Trig1
			XvT.Position += 3;
			if (XvT.ReadByte() == 0x0A) { XvT.Position--; XvT.WriteByte(0); }  //Trig2
			XvT.Position += 3;
			XvT.Position += 3;  //Jump the gap between Trig 1/2 and T3/4
			if (XvT.ReadByte() == 0x0A) { XvT.Position--; XvT.WriteByte(0); }  //Trig3
			XvT.Position += 3;
			if (XvT.ReadByte() == 0x0A) { XvT.Position--; XvT.WriteByte(0); }  //Trig4
	
			XvT.Position = XvTPos;
			for (j = 0; j < 9; j++)
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
			if (optImp.Checked) XvT.Write(System.Text.Encoding.ASCII.GetBytes("Imperial"), 0, 8);
			else XvT.Write(System.Text.Encoding.ASCII.GetBytes("Rebel"), 0, 5);
			XvT.Position = XvTPos + 0x1A;
			XvT.WriteByte(1);									//Team 1 Allies
			XvT.Position++;                                     //Team 2 bad guys
			if (TIE.ReadByte() == 49) XvT.WriteByte(1); else XvT.Position++;			//IFF 3 stance (blue)
			TIE.Position = 422;
			if (TIE.ReadByte() == 49) XvT.WriteByte(1); else XvT.Position++;			//IFF 4 stance (purple)
			TIE.Position = 434;
			if (TIE.ReadByte() == 49) XvT.WriteByte(1); else XvT.Position++;			//IFF 5 stance (red)
			TIE.Position = 446;
			if (TIE.ReadByte() == 49) XvT.WriteByte(1); else XvT.Position++;			//IFF 6 stance (purple)
			XvT.Position += 4;
			TIE.Position = 24;
			bw.Write(br.ReadBytes(128)); //Primary mission complete
			TIE.Position += 128;
			bw.Write(br.ReadBytes(128)); //Primary failed
			TIE.Position -= 256;  //[JB] Fixed offset, was off by one.
			bw.Write(br.ReadBytes(128)); //Secondary complete
			TIE.Position = 410;
			XvT.Position += 67;
			XvTPos = XvT.Position;
			XvT.WriteByte(1);XvT.Position++;
			if (optImp.Checked) XvT.Write(System.Text.Encoding.ASCII.GetBytes("Rebel"), 0, 5);
			else XvT.Write(System.Text.Encoding.ASCII.GetBytes("Imperial"), 0, 8);
			XvT.Position = XvTPos + 0x1E7;
			XvT.WriteByte(1);XvT.Position++;
			if (TIE.ReadByte() != 49) TIE.Position--;                                   //check for hostile char
			bw.Write(br.ReadBytes(11));     //IFF 3 name
			XvT.Position += 474;XvT.WriteByte(1);XvT.Position++;
			TIE.Position = 422;
			if(TIE.ReadByte() != 49) TIE.Position--;
			bw.Write(br.ReadBytes(11));     //IFF 4 name
			XvT.Position += 474;XvT.WriteByte(1);XvT.Position++;
			TIE.Position = 434;
			if(TIE.ReadByte() != 49) TIE.Position--;
			bw.Write(br.ReadBytes(11));     //IFF 5 name
			XvT.Position += 474;XvT.WriteByte(1);XvT.Position++;
			TIE.Position = 446;
			if(TIE.ReadByte() != 49) TIE.Position--;
			bw.Write(br.ReadBytes(11));     //IFF 6 name
			XvT.Position += 474;XvT.WriteByte(1);										//markers infront of other IFF name spots
			XvT.Position += 486;XvT.WriteByte(1);
			XvT.Position += 486;XvT.WriteByte(1);
			XvT.Position += 486;XvT.WriteByte(1);
			#endregion
			XvT.Position += 0x1E6;
			TIE.Position = 0x21E + 0x124*FGs + 0x5A*Messages;
			XvTPos = XvT.Position;
			#region Briefing
			bw.Write(br.ReadBytes(0x32A)); //Briefing
			XvT.WriteByte(1);XvT.Position += 9;
			XvT.Position = XvTPos;
			j = (short)(brXvT.ReadInt16() * 0x14 / 0xC);		// adjust overall briefing length
			XvT.Position -= 2;
			bw.Write(j);
			XvT.Position += 8;
			for (i = 0; i < 0x320; i += 4)      // work our way through length of briefing. i automatically increases by 4 per event
			{
				j = brXvT.ReadInt16();
				if (j == 0x270F) break;     // stop check at t=9999, end briefing
				j = (short)(j * 0x14 / 0xC);
				XvT.Position -= 2;
				bw.Write(j);
				j = brXvT.ReadInt16();       // now get the event type
				if (j == 7)     // Zoom map command
				{
					j = (short)(brXvT.ReadInt16() * 58 / 47); // X
					XvT.Position -= 2;
					bw.Write(j);
					j = (short)(brXvT.ReadInt16() * 88 / 47); // Y
					XvT.Position -= 2;
					bw.Write(j);
					i += 4;
				}
				else
				{
					XvT.Position += 2 * _brf[j];     // skip over vars
					i += (short)(2 * _brf[j]);    // increase length counter by skipped vars
				}
			}
			#endregion
			XvT.Position = 0x1BDE + 0x562*FGs + 0x74*Messages;
			#region Briefing tags & strings
			int BriefingTagLength = 0;
			for (i = 0; i < 64; i++)
			{
				j = br.ReadInt16();       //check length..  (will always be <256)
				BriefingTagLength += 2 + j;  //[JB] Calculate briefing size so we can factor it into calculations to find the description location.
				bw.Write(j);             //..write length..
				if (j != 0)                                     //and copy if not 0
					for (int k = 0; k < j; k++) XvT.WriteByte(br.ReadByte());
			}
			for (i = 0; i < 7; i++)
			{	// End Briefing event at time=9999
				XvT.WriteByte(0xC8); XvT.Position += 5; XvT.WriteByte(2); XvT.Position += 3; XvT.WriteByte(0xF); XvT.WriteByte(0x27); XvT.WriteByte(0x22);
				XvT.Position += 0x3A7;
			}
			#endregion
			#region Mission Questions

			string preMissionQuestions = "";
			for (i = 0; i < 10; i++)
			{
				int len = br.ReadInt16();
				if (len == 0) continue;
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
					if(preMissionQuestions.Length > 0) preMissionQuestions += "$";
					preMissionQuestions += q + "$$" + a + "$"; 
				}
			}
			string postMissionFail = "";
			string postMissionSuccess = "";
			for (i = 0; i < 10; i++)
			{
				int len = br.ReadInt16();
				if (len == 0) continue;
				if (len == 3)
				{
					TIE.Position += 3;
					continue;
				}
				int qCondition = TIE.ReadByte();
				int qType = TIE.ReadByte();
				byte[] buffer = new byte[len - 2];  //Length includes condition/type bytes.
				TIE.Read(buffer, 0, len - 2);

				if (qCondition == 0 || qType == 0) continue;

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
						if (postMissionSuccess.Length > 0) postMissionSuccess += "$";
						postMissionSuccess += q + "$$" + a + "$";
					}
					else if (qCondition == 5)
					{
						if (postMissionFail.Length > 0) postMissionFail += "$";
						postMissionFail += q + "$$" + a + "$";
					}
				}
			}
			#endregion
			#region Mission Description
			XvT.Position = 0xA4 + (FGs * 0x562) + (Messages * 0x74) + (0x80 * 10) + (0x1E7 * 10); //Header + FGs + Messages + 10 Globals + 10 Teams
			XvT.Position += (0x334 + BriefingTagLength) + (0x3B4 * 7);  //Briefing 1 (plus tag/string lengths) + 7 empty briefings (static size)
			XvT.Position += (0x600 * FGs) + 0x5A00;  //FGGoalStrings(FGs*8*3*64) + GlobalGoalStrings(10*3*4*3*64)
			XvT.Position += (0xC00 * 10);  //Empty GlobalGoalStrings data.

			int maxLength = (format == 12 ? 0x400 : 0x1000);
			if (preMissionQuestions.Length > maxLength) preMissionQuestions = preMissionQuestions.Remove(maxLength);
			if (postMissionSuccess.Length > maxLength) postMissionSuccess = postMissionSuccess.Remove(maxLength);
			if (postMissionFail.Length > maxLength) postMissionFail = postMissionFail.Remove(maxLength);

			byte[] desc;
			if (format == 12) desc = System.Text.Encoding.ASCII.GetBytes(preMissionQuestions);
			else desc = System.Text.Encoding.ASCII.GetBytes(postMissionSuccess);

			XvTPos = XvT.Position;
			XvT.Write(desc, 0, desc.Length);
			XvT.Position = XvTPos + maxLength - 1;
			XvT.WriteByte(0);

			if (format == 14)
			{
				XvTPos += maxLength;
				desc = System.Text.Encoding.ASCII.GetBytes(postMissionFail);
				XvT.Write(desc, 0, desc.Length);
				XvT.Position = XvTPos + maxLength - 1;
				XvT.WriteByte(0);

				XvTPos += maxLength;
				desc = System.Text.Encoding.ASCII.GetBytes(preMissionQuestions);
				XvT.Write(desc, 0, desc.Length);
				XvT.Position = XvTPos + maxLength - 1;
				XvT.WriteByte(0);
			}
			#endregion

			XvT.Close();
			TIE.Close();
			if (!T2W && !_hidden) MessageBox.Show("Conversion completed", "Finished");
		}
		
		void tie2XWA()
		{
			//instead of writing it all out again, cheat and use the other two
			T2W = true;
			string save, exist;
			bool bop = chkXvtBop.Checked;
			save = txtSave.Text;
			exist = txtExist.Text;
			txtSave.Text = "temp.tie";
			chkXvtBop.Checked = true;
			tie2XvT();
			txtSave.Text = save;
			txtExist.Text = "temp.tie";
			lblType.Text = "BoP";
			chkXvtBop.Checked = bop;
			xvt2XWA();
			lblType.Text = "TIE";
			txtExist.Text = exist;
			File.Delete("temp.tie");
			if (!_hidden) MessageBox.Show("Conversion completed", "Finished");
		}

		void xvt2XWA_ConvertDesignations(byte[] xvt, byte[] xwa)
		{
			//xvt  input   8 chars, [0] = team, [1..3] text of role.  EX: "2MIS", repeat for role2
			//xwa  output  4 bytes, [0] = role1 enabled, [1] = role2 enabled, [2] = role1 enum, [3] = role2 enum
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
				if (sub[0] == 0) return;

				//Get the role first so that if the team is set to all, both teams can be assigned the same role.
				char team = sub[0];
				sub = sub.Substring(1);
				byte role;
				roleMap.TryGetValue(sub, out role);
				xwa[2+i] = role;

				switch(team)
				{
					case '1': xwa[i] = 0x0; break;
					case '2': xwa[i] = 0x1; break;
					case '3': xwa[i] = 0x2; break;
					case '4': xwa[i] = 0x3; break;
					case 'A': xwa[0] = 0xA; xwa[1] = 0xB; xwa[2] = role; xwa[3] = role; break;	//~MG: the original single-designation version of this function had 0xB for 'A' and 0xA for 'H'. I have this value as being a bool, need to look into it
					case 'H': xwa[0] = 0xA; xwa[1] = 0xB; xwa[2] = role; xwa[3] = role; break;  //No idea (what 'H' means)
					default: xwa[i] = 0x0; break;
				}

				t = t.Substring(4); //Trim so the next 4 become the current.
			}
		}		
		void xvt2XWA()
		{
			short[] fgIcons;
			FileStream XvT, XWA;													//usual crap...
			XvT = File.OpenRead(txtExist.Text);
			XWA = File.Open(txtSave.Text, FileMode.Create, FileAccess.ReadWrite);
			BinaryReader br = new BinaryReader(XvT);
			BinaryReader brXWA = new BinaryReader(XWA);
			BinaryWriter bw = new BinaryWriter(XWA);
			XWA.WriteByte(18);
			XWA.Position++;
			XvT.Position = 2;
			short i, j;
			short FGs = br.ReadInt16();
			short Messages = br.ReadInt16();
			bw.Write((short)(FGs + 2)); // [JB] Modified to +2 since generated skirmish files have two backdrops for ambient lighting
			bw.Write(Messages);
			fgIcons = new short[FGs];
		
			XWA.Position = 8; XWA.WriteByte(1); XWA.Position = 11; XWA.WriteByte(1);        //unknowns
			XvT.Position = 0x14;
			XWA.Position = 0x14;
			bw.Write(br.ReadBytes(80));	// IFFs
			XWA.Position = 100;
			bw.Write(System.Text.Encoding.ASCII.GetBytes("The Final Frontier"));	//make a nice Region name :P
			XWA.Position = 0x23AC; XWA.WriteByte(6); 						//starting hangar
			XWA.Position++;
			XvT.Position = 0x66; bw.Write(br.ReadByte());		//time limit (minutes)
			XWA.Position = 0x23B3; XWA.WriteByte(0x62);						//unknown
			XWA.Position = 0x23F0;

			//[JB] Jumping ahead to get the briefing locations before we load in the FGs.
			long brief1BlockStart = 0xA4 + (0x562 * FGs) + (0x74 * Messages) + 0x500 + 0x1306;  //FGs, messages, teams, global goals
			XvT.Position = brief1BlockStart + 0x334;  //Jump to tags
			long brief1StringSize = 0;
			for (i = 0; i < 64; i++)   //32 tags + 32 strings
			{
				int len = br.ReadInt16();
				brief1StringSize += 2;
				XvT.Position += len;
				brief1StringSize += len;
			}
			//long brief1EndSize = XvT.Position;
			XvT.Position = 0xA4;  //Rewind back to start of FG data
								  //[JB] End loading briefing offset;

			#region Flight Groups
			long XvTPos; //[JB] Sometimes need to bookmark XvT too for better offset handling
			long XWAPos;
			bool Player = false;
			int PlayerCraft = 0; //[JB] for combat engagements
			short[] Briefs = new short[2];
			Briefs[0] = 0;
			Briefs[1] = 0;
			for (i = 0; i < FGs; i++)
			{
				XvTPos = XvT.Position;
				XWAPos = XWA.Position;
				bw.Write(br.ReadBytes(20));   //name
				byte[] buffer2 = new byte[8];
				byte[] des2 = new byte[4];
				XvT.Read(buffer2, 0, 8);
				xvt2XWA_ConvertDesignations(buffer2, des2);
				XvT.Position -= 8;
				XWA.WriteByte(des2[0]); XWA.WriteByte(des2[1]);
				XWA.WriteByte(des2[2]); XWA.WriteByte(des2[3]);
				XWA.Position++;       //Skip unknown
				XWA.WriteByte(255); //Global cargos set to none
				XWA.WriteByte(255);
				XWA.Position = XWAPos + 40;
				XvT.Position += 20;
				bw.Write(br.ReadBytes(40));   //Cargo & Special
				XWA.Position = XWAPos + 0x69;
				bw.Write(br.ReadBytes(26));           //SC ship# -> Roll
				XWA.Position = XWAPos + 0x87;
				XvT.Position = XvTPos + 0x6D;
				bw.Write(br.ReadByte());	// Arr difficulty
				XvT.Position = XvTPos + 0x64;
				// [JB] Modified
				if (!chkXvtCombat.Checked)
				{
					if (XvT.ReadByte() != 0 && Player == false) { Player = true; }              //check for multiple player craft, take the first one
					else
					{
						XWA.Position = XWAPos + 0x7D;
						XWA.WriteByte(0);
					}
				}
				if (XvT.ReadByte() != 0) { PlayerCraft++; }

				XWA.Position = XWAPos + 0x88;
				XvT.Position = XvTPos + 0x6E;
				bw.Write(br.ReadInt32());                //Arival trigger 1 (cheating and using Int32 since it's 4 bytes)...
				XWA.Position += 2;
				bw.Write(br.ReadInt32());                //... and 2
				XWA.Position += 4;
				XvT.Position += 2;
				bw.Write(br.ReadByte());                                    //AT 1 AND/OR 2
				XWA.Position++;
				bw.Write(br.ReadInt32());                //AT 3
				XWA.Position += 2;
				bw.Write(br.ReadInt32());                //AT 4
				XWA.Position += 4;
				XvT.Position += 2;
				bw.Write(br.ReadByte());                                    //AT 3 AND/OR 4
				XWA.Position++;
				bw.Write(br.ReadInt64());                //AT 1/2 AND/OR 3/4 -> DT (8 bytes)
				XWA.Position += 2;
				bw.Write(br.ReadInt32());                //DT 2
				XWA.Position += 4;
				XvT.Position += 2;
				bw.Write(br.ReadByte());                                    //DT 1 AND/OR 2
				XWA.Position += 3;
				XvT.Position += 2;
				bw.Write(br.ReadByte());                                    //Abort trigger
				XvT.Position += 4;
				XWA.Position += 3;
				bw.Write(br.ReadInt64());                //Arr/Dep methods
				long XvTSubOrderStart = XvT.Position;  //[JB] ShipOFix modifies the offsets and I assume it's bad to add anything so I'm going to let the original code run then rewind to these offsets later to apply my patches.
				long XWASubOrderStart = XWA.Position;
				bw.Write(br.ReadBytes(18));       //Order 1
				XvT.Position = XvTPos + 0x46E;
				for (j = 0; j < 8; j++)
				{
					bw.Write(br.ReadInt16());   //WP X
					XvT.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP Y
					XvT.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP Z
					XvT.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP enabled
					XvT.Position -= 0x84;
				}

				//[JB] Patch wait times
				XvT.Position = XvTSubOrderStart;
				XWA.Position = XWASubOrderStart;
				xvtToXWA_ConvertOrderTime(XvT, XWA);
				//Copy order speed
				XvT.Position = XvTSubOrderStart + 18;
				XWA.Position = XWASubOrderStart + 18;
				bw.Write(br.ReadByte());
				//After XvT speed comes flight group player designation, load it to patch the role later. 
				byte[] role = new byte[16];
				XvT.Read(role, 0, 16);
				XWA.Position = XWAPos + 0x50; //Patch in the display role for the player FG slot screen.
				XWA.Write(role, 0, 16);
				//[JB] End patch code.

				XWA.Position = XWAPos + 0x15E;
				XvT.Position = XvTPos + 0xF4;
				XvTSubOrderStart = XvT.Position;  //[JB] ShipOFix modifies the offsets and I assume it's bad to add anything so I'm going to let the original code run then rewind to these offsets later to apply my patches.
				XWASubOrderStart = XWA.Position;
				bw.Write(br.ReadBytes(18));       //Order 2
				XvT.Position = XvTPos + 0x46E;
				for (j = 0; j < 8; j++)
				{
					bw.Write(br.ReadInt16());   //WP X
					XvT.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP Y
					XvT.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP Z
					XvT.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP enabled
					XvT.Position -= 0x84;
				}
				//[JB] Patch wait times
				XvT.Position = XvTSubOrderStart;
				XWA.Position = XWASubOrderStart;
				xvtToXWA_ConvertOrderTime(XvT, XWA);
				//Copy order speed
				XvT.Position = XvTSubOrderStart + 18;
				XWA.Position = XWASubOrderStart + 18;
				bw.Write(br.ReadByte());
				//[JB] End patch code.

				XWA.Position = XWAPos + 0x1F2;
				XvT.Position = XvTPos + 0x146;
				XvTSubOrderStart = XvT.Position;  //[JB] ShipOFix modifies the offsets and I assume it's bad to add anything so I'm going to let the original code run then rewind to these offsets later to apply my patches.
				XWASubOrderStart = XWA.Position;
				bw.Write(br.ReadBytes(18));       //Order 3
				XvT.Position = XvTPos + 0x46E;
				for (j = 0; j < 8; j++)
				{
					bw.Write(br.ReadInt16());   //WP X
					XvT.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP Y
					XvT.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP Z
					XvT.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP enabled
					XvT.Position -= 0x84;
				}
				//[JB] Patch wait times
				XvT.Position = XvTSubOrderStart;
				XWA.Position = XWASubOrderStart;
				xvtToXWA_ConvertOrderTime(XvT, XWA);
				//Copy order speed
				XvT.Position = XvTSubOrderStart + 18;
				XWA.Position = XWASubOrderStart + 18;
				bw.Write(br.ReadByte());
				//[JB] End patch code.

				XWA.Position = XWAPos + 0x286;
				XvT.Position = XvTPos + 0x198;
				XvTSubOrderStart = XvT.Position;  //[JB] ShipOFix modifies the offsets and I assume it's bad to add anything so I'm going to let the original code run then rewind to these offsets later to apply my patches.
				XWASubOrderStart = XWA.Position;
				bw.Write(br.ReadBytes(18));       //Order 4
				XvT.Position = XvTPos + 0x46E;
				for (j = 0; j < 8; j++)
				{
					bw.Write(br.ReadInt16());   //WP X
					XvT.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP Y
					XvT.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP Z
					XvT.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP enabled
					XvT.Position -= 0x84;
				}
				//[JB] Patch wait times
				XvT.Position = XvTSubOrderStart;
				XWA.Position = XWASubOrderStart;
				xvtToXWA_ConvertOrderTime(XvT, XWA);
				//Copy order speed
				XvT.Position = XvTSubOrderStart + 18;
				XWA.Position = XWASubOrderStart + 18;
				bw.Write(br.ReadByte());
				//[JB] End patch code.

				XvT.Position = XvTPos + 0x1EA;
				XWA.Position = XWAPos + 0xA3A;
				bw.Write(br.ReadInt32());        //jump to Order 4 T1
				XWA.Position += 2;
				bw.Write(br.ReadInt32());        //jump to Order 4 T2
				XWA.Position += 4;
				XvT.Position += 2;
				bw.Write(br.ReadByte());                            //jump to Order 4 T 1AND/OR 2
				XWA.Position = XWAPos + 0xB0A;
				XvT.Position = XvTPos + 0x1F5;
				for (j = 0; j < 8; j++)
				{
					bw.Write(br.ReadBytes(14));       //Goals
					XvT.Position += 0x40;
					XWA.Position += 0x42;
				}
				XvT.Position = XvTPos + 0x466;
				XWA.Position = XWAPos + 0xD8A;
				for (j = 0; j < 3; j++)
				{
					bw.Write(br.ReadInt16());   //SP X
					XvT.Position += 0x2A;
					bw.Write(br.ReadInt16());   //SP Y
					XvT.Position += 0x2A;
					bw.Write(br.ReadInt16());   //SP Z
					XvT.Position += 0x2A;
					bw.Write(br.ReadInt16());   //SP enabled
					XvT.Position -= 0x84;
				}
				XvT.Position = XvTPos + 0x480;
				bw.Write(br.ReadInt16());   //HYP X
				XvT.Position += 0x2A;
				bw.Write(br.ReadInt16());   //HYP Y
				XvT.Position += 0x2A;
				bw.Write(br.ReadInt16());   //HYP Z
				XvT.Position += 0x2A;
				bw.Write(br.ReadInt16());   //HYP enabled
				XvT.Position = XvTPos + 0x506;
				if (XvT.ReadByte() == 1)        //okay, briefing time. if BP enabled..
				{
					//[JB] 0x23F0 (fileheader) + 0xE60 (GlobalGoal[10]) + 0x1306 (Team[10]) = 0x4556 (17750 dec)
					XWA.Position = 17762 + (FGs + 2) * 3646 + Messages * 162 + Briefs[0] * 20;  //place for next insert command  [JB] Modified to FGs+2
					XWA.WriteByte(26); XWA.Position++; bw.Write(Briefs[0]);        //Add command
					fgIcons[i] = Briefs[0];     // store the Icon# for the FG
					XvT.Position = XvTPos + 0x52; bw.Write(br.ReadByte()); XWA.Position++;        //Ship
					XvT.Position = XvTPos + 0x57; bw.Write(br.ReadByte()); XWA.Position += 3;     //IFF
					XvT.Position = XvTPos + 0x482;
					XWA.WriteByte(28); XWA.Position++; bw.Write(Briefs[0]);        //Move command
					bw.Write(br.ReadInt16());   //BP X
					XvT.Position += 0x2A;
					bw.Write(br.ReadInt16());   //BP Y
					Briefs[0]++;
				}
				if (chkXvtCombat.Checked)
				{
					XvT.Position = XvTPos + 0x508;
					if (XvT.ReadByte() == 1)        //okay, briefing time. if BP enabled..
					{
						//[JB] 0x23F0 (fileheader) + 0xE60 (GlobalGoal[10]) + 0x1306 (Team[10]) = 0x4556 (17750 dec)
						XWA.Position = 17750 + (FGs + 2) * 3646 + Messages * 162 + Briefs[1] * 20;  //place for next insert command  [JB] Modified to FGs+2
						XWA.Position += 0x4414 + brief1StringSize + 384 + 0x000A + 2;  //briefing(minus strings) + XvT string list size + 192 shorts for empty messages in XWA + start of Brief2 event list
						XWA.WriteByte(26); XWA.Position++; bw.Write(Briefs[1]);        //Add command
						fgIcons[i] = Briefs[1];     // store the Icon# for the FG
						XvT.Position = XvTPos + 0x52; bw.Write(br.ReadByte()); XWA.Position++;        //Ship
						XvT.Position = XvTPos + 0x57; bw.Write(br.ReadByte()); XWA.Position += 3;     //IFF
						XvT.Position = XvTPos + 0x484;  //Offset for BP2
						XWA.WriteByte(28); XWA.Position++; bw.Write(Briefs[1]);        //Move command
						bw.Write(br.ReadInt16());   //BP X
						XvT.Position += 0x2A;
						bw.Write(br.ReadInt16());   //BP Y
						Briefs[1]++;
					}
				}
				XWA.Position = XWAPos + 0xDC7;
				XvT.Position = XvTPos + 0x523;
				bw.Write(br.ReadInt32());        //CM -> Global Unit
				XWA.Position++;
				XvT.Position += 9;
				bw.Write(br.ReadBytes(48));   //Optionals
				XvT.Position = XvTPos + 0x562;
				XWA.Position = XWAPos + 0xE3E;
			}
			Random rnd = new Random();
			int craft1 = rnd.Next(1, 59);
			int craft2 = rnd.Next(63, 102);
			short[] coord1 = {0, 0, 0};
			short[] coord2 = {0, 0, 0};
			coord1[rnd.Next(0, 2)] = 1;	//[JB] ensures backdrop isn't on origin
			coord2[rnd.Next(0, 2)] = -1;

			//okay, now write in the default Backdrop
			XWAPos = XWA.Position;
			bw.Write(System.Text.Encoding.ASCII.GetBytes("1.0 1.0 1.0"));	//Name
			XWA.Position = XWAPos + 20;
			XWA.WriteByte(255);XWA.WriteByte(255);XWA.Position += 3;XWA.WriteByte(255);XWA.WriteByte(255);  //EnableDesignation1, EnableDesignation2, ... GlobalCargoIndex GlobalSpecialCargoIndex 
			XWA.Position = XWAPos + 40;
			bw.Write(System.Text.Encoding.ASCII.GetBytes("1.0"));	//Brightness
			XWA.Position = XWAPos + 60;
			bw.Write(System.Text.Encoding.ASCII.GetBytes("1.9"));	//Size (match skirmish output)
			XWA.Position = XWAPos + 105;XWA.WriteByte(2);           //SpecialCargoCraft?
			XWA.Position++;XWA.WriteByte(183);XWA.WriteByte(1);
			XWA.Position += 3;XWA.WriteByte(4); //[JB] Changed to IFF Red since it is used less frequently, and is less likely to interfere with IFF triggers.
			XWA.Position = XWAPos + 113;XWA.WriteByte(9);   //[JB] Team (so it doesn't interfere with triggers)
			XWA.Position = XWAPos + 120;XWA.WriteByte(31);  //[JB] Global group (for same reason)
			XWA.Position = XWAPos + 2827;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);
			XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;
			XWA.Position = XWAPos + 3466;
			bw.Write(coord1[0]);
			bw.Write(coord1[1]);
			bw.Write(coord1[2]);
			XWA.Position++; XWA.WriteByte(1);
			XWA.Position = XWAPos + 3602;
			XWA.WriteByte((byte)craft1); //[JB] Set backdrop value to random(1-59)
			XWA.Position = XWAPos + 3646;

			//[JB] Adding a second backdrop, since the game generates two backdrops for skirmish files.  Offers better ambient lighting for the player.
			XWAPos = XWA.Position;
			bw.Write(System.Text.Encoding.ASCII.GetBytes("1.0 1.0 1.0"));   //Name
			XWA.Position = XWAPos + 20;
			XWA.WriteByte(255);XWA.WriteByte(255);XWA.Position += 3;XWA.WriteByte(255);XWA.WriteByte(255);  //[JB] Modified to update both global cargo values.
			XWA.Position = XWAPos + 40;
			bw.Write(System.Text.Encoding.ASCII.GetBytes("1.0"));   //Brightness
			XWA.Position = XWAPos + 60;
			bw.Write(System.Text.Encoding.ASCII.GetBytes("1.0"));   //Size
			XWA.Position = XWAPos + 105;XWA.WriteByte(2);
			XWA.Position++;XWA.WriteByte(183);XWA.WriteByte(1);
			XWA.Position += 3;XWA.WriteByte(4);
			XWA.Position = XWAPos + 113;XWA.WriteByte(9);   //[JB] Team (so it doesn't interfere with triggers)
			XWA.Position = XWAPos + 120;XWA.WriteByte(31);  //[JB] Global group (for same reason)
			XWA.Position = XWAPos + 2827;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);
			XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;
			XWA.Position = XWAPos + 3466;
			bw.Write(coord2[0]);
			bw.Write(coord2[1]);
			bw.Write(coord2[2]);
			XWA.Position++; XWA.WriteByte(1);
			XWA.Position = XWAPos + 3602;XWA.WriteByte((byte)craft2); //[JB] Set backdrop value to random(63-102)
			XWA.Position = XWAPos + 3646;

			#endregion
			//[JB] Now that flight groups are done, check for player count and patch in skirmish mode
			if (chkXvtCombat.Checked && PlayerCraft > 1)
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
				bw.Write(br.ReadInt16());		//Message# - 1
				switch(XvT.ReadByte())											//takes care of colors if needed
				{
					case 49:
						bw.Write(br.ReadBytes(63));	//green
						XWA.Position = XWAPos + 142;XWA.WriteByte(0);
						break;
					case 50:
						bw.Write(br.ReadBytes(63));	//blue
						XWA.Position = XWAPos + 142;XWA.WriteByte(2);
						break;
					case 51:
						bw.Write(br.ReadBytes(63)); ;	//yellow
						XWA.Position = XWAPos + 142;XWA.WriteByte(3);
						break;
					default:
						XvT.Position--;
						bw.Write(br.ReadBytes(64)); //red
						XWA.Position = XWAPos + 142;XWA.WriteByte(1);
						break;
				}
				XWA.Position = XWAPos + 82;
				bw.Write(br.ReadBytes(14));     //Sent to.. -> T1
				XWA.Position += 2;
				bw.Write(br.ReadInt32());			//T2
				XvT.Position += 2;
				XWA.Position += 4;
				bw.Write(br.ReadByte());								//T1 AND/OR T2
				XWA.Position++;
				bw.Write(br.ReadInt32());           //T3
				XWA.Position += 2;
				bw.Write(br.ReadInt32());           //T4
				XvT.Position += 2;
				XWA.Position += 4;
				bw.Write(br.ReadByte());								//T3 AND/OR T4
				XWA.Position = XWAPos + 141;
				XvT.Position += 17;
				bw.Write(br.ReadByte());								//T (1/2) AND/OR (3/4)
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
				XWA.Position++;
				XvT.Position += 2;
				bw.Write(br.ReadInt32());       //Prim T1
				XWA.Position += 2;
				bw.Write(br.ReadInt32());       //PT2
				XWA.Position += 4;
				XvT.Position += 2;
				bw.Write(br.ReadByte());							//PT 1 AND/OR 2
				XWA.Position++;
				bw.Write(br.ReadInt32());       //PT 3
				XWA.Position += 2;
				bw.Write(br.ReadInt32());       //PT 4
				XWA.Position += 4;
				XvT.Position += 2;
				bw.Write(br.ReadByte());							//PT 3 AND/OR 4
				XvT.Position += 17;
				XWA.Position += 18;
				bw.Write(br.ReadBytes(3));		//PT (1/2) AND/OR (3/4) -> Points
				XWA.Position += 70;
				bw.Write(br.ReadInt32());       //Prev T1
				XWA.Position += 2;
				bw.Write(br.ReadInt32());       //PT2
				XWA.Position += 4;
				XvT.Position += 2;
				bw.Write(br.ReadByte());							//PT 1 AND/OR 2
				XWA.Position++;
				bw.Write(br.ReadInt32());       //PT 3
				XWA.Position += 2;
				bw.Write(br.ReadInt32());       //PT 4
				XWA.Position += 4;
				XvT.Position += 2;
				bw.Write(br.ReadByte());							//PT 3 AND/OR 4
				XvT.Position += 17;
				XWA.Position += 18;
				bw.Write(br.ReadBytes(3));      //PT (1/2) AND/OR (3/4) -> Points
				XWA.Position += 70;
				bw.Write(br.ReadInt32());       //Sec T1
				XWA.Position += 2;
				bw.Write(br.ReadInt32());       //ST2
				XWA.Position += 4;
				XvT.Position += 2;
				bw.Write(br.ReadByte());							//ST 1 AND/OR 2
				XWA.Position++;
				bw.Write(br.ReadInt32());       //ST 3
				XWA.Position += 2;
				bw.Write(br.ReadInt32());       //ST 4
				XWA.Position += 4;
				XvT.Position += 2;
				bw.Write(br.ReadByte());							//ST 3 AND/OR 4
				XvT.Position += 17;
				XWA.Position += 18;
				bw.Write(br.ReadBytes(3));      //ST (1/2) AND/OR (3/4) -> Points
				XWA.Position += 70;
			}
			XvT.Position = XvTPos + (0x80 * 10);  //10 teams
			XWA.Position = XWAPos + (0x170 * 10);
			#endregion
			#region IFF/Teams
			bw.Write(br.ReadBytes(4870));	//well, that was simple..
			#endregion
			#region Briefing
			XWAPos = XWA.Position;
			long XWABriefing1 = XWAPos;
			bw.Write(br.ReadBytes(6));	//briefing intro
			bw.Write((short)(br.ReadInt16() + 10 * Briefs[0])); // adjust length for add/moves
			XvT.Position += 2;		
			XWA.Position += 20 * Briefs[0] + 2;
			bw.Write(br.ReadBytes(0x32A));	//briefing content
			XWA.Position = XWAPos;
			j = (short)(brXWA.ReadInt16() * 0x19 / 0x14);		// adjust overall briefing length
			XWA.Position -= 2;
			bw.Write(j);
			XWA.Position += 8;
			for (i = 0; i < 0x320; i += 4)      // work our way through length of briefing. i automatically increases by 4 per event
			{
				j = brXWA.ReadInt16();
				if (j == 0x270F) break;     // stop check at t=9999, end briefing
				j = (short)(j * 0x19 / 0x14);
				XWA.Position -= 2;
				bw.Write(j);
				j = brXWA.ReadInt16();      // now get the event type
				if (j > 8 && j < 17)        // FG tags 1-8
				{
					j = brXWA.ReadInt16();  // FG number
					XWA.Position -= 2;
					bw.Write(fgIcons[j]);   // Overwrite with the Icon#
					i += 2;
				}
				else if (j == 7)        // Zoom map command
				{
					j = (short)(brXWA.ReadInt16() * 124 / 58);  // X
					XWA.Position -= 2;
					bw.Write(j);
					j = (short)(brXWA.ReadInt16() * 124 / 88);  // Y
					XWA.Position -= 2;
					bw.Write(j);
					i += 4;
				}
				else
				{
					XWA.Position += 2 * _brf[j];       // skip over vars
					i += (short)(2 * _brf[j]);   // increase length counter by skipped vars
				}
			}
			XWA.Position = 0x8960 + (FGs+2)*0xE3E + Messages*0xA2;  //[JB] FGs+2
			XWA.WriteByte(1);					//show the non-existant briefing
			XWA.Position += 9;
			#endregion Briefing
			#region Briefing tags & strings
			for (i = 0; i < 32; i++)    //tags
			{
				j = br.ReadInt16();     //check length..
				bw.Write(j);                //..write length..
				if (j != 0)                                     //and copy if not 0
					bw.Write(br.ReadBytes(j));
			}
			XWA.Position += 192;
			for (i = 0; i < 32; i++)    //strings
			{
				j = br.ReadInt16();     //check length..
				bw.Write(j);                //..write length..
				if (j != 0)                                     //and copy if not 0
					bw.Write(br.ReadBytes(j));
			}
			XWA.Position += 192;
			#endregion Briefing T&S
			#region Briefing2
			//[JB] Begin briefing 2.  Basically just copy/paste the same code.
			if (chkXvtCombat.Checked)
			{
				long XWABriefing2 = XWA.Position;
				XWAPos = XWA.Position;
				bw.Write(br.ReadBytes(6));	//briefing intro
				bw.Write((short)(br.ReadInt16() + 10 * Briefs[1]));	// adjust length for add/moves
				XvT.Position += 2;
				XWA.Position += 20 * Briefs[1] + 2;
				bw.Write(br.ReadBytes(0x32A));	//briefing content
				XWA.Position = XWAPos;
				j = (short)(brXWA.ReadInt16() * 0x19 / 0x14);		// adjust overall briefing length
				XWA.Position -= 2;
				bw.Write(j);
				XWA.Position += 8;
				for (i = 0; i < 0x320; i += 4)		// work our way through length of briefing. i automatically increases by 4 per event
				{
					j = brXWA.ReadInt16();
					if (j == 0x270F) break;		// stop check at t=9999, end briefing
					j = (short)(j * 0x19 / 0x14);
					XWA.Position -= 2;
					bw.Write(j);
					j = brXWA.ReadInt16();		// now get the event type
					if (j > 8 && j < 17)		// FG tags 1-8
					{
						j = brXWA.ReadInt16();	// FG number
						XWA.Position -= 2;
						bw.Write(fgIcons[j]);	// Overwrite with the Icon#
						i += 2;
					}
					else if (j == 7)		// Zoom map command
					{
						j = (short)(brXWA.ReadInt16() * 124 / 58);	// X
						XWA.Position -= 2;
						bw.Write(j);
						j = (short)(brXWA.ReadInt16() * 124 / 88);	// Y
						XWA.Position -= 2;
						bw.Write(j);
						i += 4;
					}
					else
					{
						XWA.Position += 2 * _brf[j];		// skip over vars
						i += (short)(2 * _brf[j]);	// increase length counter by skipped vars
					}
				}
				XWA.Position = 0x8960 + (XWABriefing2 - XWABriefing1) + (FGs + 2) * 0xE3E + Messages * 0xA2;   //[JB] FGs+2
				XWA.WriteByte(0);					//show the non-existant briefing
				XWA.WriteByte(1);					//show the non-existant briefing
				XWA.Position += 8;
				for (i = 0; i < 32; i++)	//tags
				{
					j = br.ReadInt16();		//check length..
					bw.Write(j);				//..write length..
					if (j != 0)                                     //and copy if not 0
						bw.Write(br.ReadBytes(j));
				}
				XWA.Position += 192;
				for (i = 0; i < 32; i++)	//strings
				{
					j = br.ReadInt16();     //check length..
					bw.Write(j);				//..write length..
					if (j != 0)                                     //and copy if not 0
						bw.Write(br.ReadBytes(j));
				}
				XWA.Position += 192;
			}
			else
			{
				XvT.Position += 0x334;    //Jump to tags
				for (i = 0; i < 64; i++)
				{   //32 tags + 32 strings
					j = br.ReadInt16();
					XvT.Position += j;
				}
				XWA.Position += 0x4614;  //Empty briefing plus empty tags/strings
			}
			#endregion Briefing2

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
				short l;
				for (j = 0; j < 64; j++)
				{  //32 tags + 32 strings
					l = br.ReadInt16();
					XvT.Position += l;
				}
			}
			#region FG Goal strings
			for (i = 0; i < FGs; i++)
			{
				for (j = 0; j < 24; j++)  //8 goals * 3 strings
				{
					if (XvT.ReadByte() == 0)
					{
						XWA.Position++;
						XvT.Position += 63;
					}
					else
					{
						XvT.Position--;
						bw.Write(br.ReadBytes(64));
					}
				}
			}
			XWA.Position += 48;                     //compensate for adding the Backdrop  [JB] Was 24 for one backdrop, needs to be 48 since I added an extra one.
			#endregion
			#region Global Goal strings
			for (i = 0; i < 10; i++)
			{
				for (j = 0; j < 36; j++)
				{
					if (XvT.ReadByte() == 0)
					{
						XWA.Position++;
						XvT.Position += 63;
					}
					else
					{
						XvT.Position--;
						bw.Write(br.ReadBytes(64));
					}
				}
				XvT.Position += 3072;
			}
			#endregion
			XWA.Position += 3552;               //skip custom order strings
			#region Debrief and Descrip
			if (lblType.Text == "XvT")
			{
				XWA.Position += 4096;
				XWA.WriteByte(35);
				XWA.Position += 4095;
				XWA.WriteByte(35);
				for (i = 0; i < 1024; i++)
				{
					int d = XvT.ReadByte();
					j = (short)(1024 - i);
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
							bw.Write(br.ReadByte());
							break;
					}
				}
				XWA.Position += 3071 + j;
			}
			else
			{
				bw.Write(br.ReadBytes(4096));   // Debrief
				XWA.WriteByte(35);
				bw.Write(br.ReadBytes(4095));   // Hints
				XvT.Position++;
				XWA.WriteByte(35);
				bw.Write(br.ReadBytes(4095));   // Brief/Description
			}
			#endregion
			XWA.WriteByte(0);				//EOF
			XvT.Close();
			XWA.Close();
			if (!T2W && !_hidden) MessageBox.Show("Conversion completed", "Finished");
		}

		byte convertOrderTimeXvTToXWA(byte xvtTime)
		{
			//XWA time value, if 20 (decimal) or under is exact seconds.
			//21 = 25 sec, 22 = 30 sec, etc.
			int seconds = xvtTime * 5;
			int xwaTime = ((seconds - 20) / 5) + 20;
			if (seconds <= 20)
				return Convert.ToByte(seconds);
			if (xwaTime > 255) xwaTime = 255;
			return Convert.ToByte(xwaTime);
		}
		void xvtToXWA_ConvertOrderTime(FileStream xvt, FileStream xwa)
		{
			long curXvT = xvt.Position;
			long curXWA = xwa.Position;
			byte orderEnum = (byte)xvt.ReadByte();
			xvt.Position++;
			byte var1 = (byte)xvt.ReadByte();
			xwa.Position += 2;
			switch (orderEnum)
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
					xwa.WriteByte(convertOrderTimeXvTToXWA(var1));
					break;
				default:
					break;
			}

			xvt.Position = curXvT;
			xwa.Position = curXWA;
		}
	}
}
