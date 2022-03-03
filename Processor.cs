/*
 * Convert.exe, Up-converts mission formats between TIE, XvT and XWA
 * Copyright (C) 2005-2022 Michael Gaisser (mjgaisser@gmail.com)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL (License.txt) was not distributed
 * with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
 *
 * VERSION: 1.7
 */

/* CHANGELOG
 * [FIX] XvT to XWA Order waypoints
 * [FIX] XvT to XWA target Ship Type indexes
 * v1.6, 200906
 * [NEW] Split out source
 * [FIX] overflow in convertOrderTimeXvTToXWA()
 * [FIX] missing "l" from "Imperial" in TieToXvt Team name
 * [FIX] arrival difficulty in XvtToXwa
 * [NEW] IFF name conversion (was original just doing IFF -> Teams)
 * [FIX] skipping empty XvT briefings, which fixed XvtToXwa FG/Global Goal strings and Mission Desc/EoM
 * [FIX] XvtToXwa global goals
 * [FIX] removed shipFix and shipOFix entirely, since it was breaking the offset, not fixing it (likely due to changes in Platform/YOGEME)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Idmr.Converter
{
	public static class Processor
	{
		readonly static short[] _brf = { 0, 0, 0, 0, 1, 1, 2, 2, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 4, 4, 4, 4, 4, 4, 4, 4, 3, 2, 3, 2, 1, 0, 0, 0, 0 };

		//NOTE: Some of the delays may still be off due to different conversion factors

		public static void TieToXvt(string originalPath, string newPath, bool toBoP)
		{
			FileStream tie, xvt;
			tie = File.OpenRead(originalPath);
			xvt = File.Open(newPath, FileMode.Create, FileAccess.ReadWrite);
			BinaryReader br = new BinaryReader(tie);
			BinaryWriter bw = new BinaryWriter(xvt);
			BinaryReader brXvT = new BinaryReader(xvt);	// this is needed to fiddle with the Briefing after the bulk copy
			long xvtPos;
			if (toBoP) xvt.WriteByte(14);
			else xvt.WriteByte(12);

			xvt.Position = 2;
			tie.Position = 2;
			short i, j;
			bw.Write(br.ReadInt32());           //# of FGs and messages
			tie.Position = 2;
			short fgs = br.ReadInt16();         //store them
			short messages = br.ReadInt16();
			if (toBoP)  //[JB] patch in
			{
				xvt.Position = 0x64;
				xvt.WriteByte(3);  //MPtraining
			}
			tie.Position = 0x19A;
			xvt.Position = 0x14;
			for (i = 0; i < 4; i++) // IFF 3-6 names
			{
				xvtPos = xvt.Position;
				bw.Write(br.ReadBytes(12));
				xvt.Position = xvtPos + 20;
			}
			xvt.Position = 0xA4;
			// Before we can start processing FGs, we need to first determine if the player craft is Imperial or Rebel (probably Imp, but still)
			bool playerIsImperial = true;
			for (i = 0; i < fgs; i++)
			{
				tie.Position = 0x1CA + i * 0x124 + 0x42;	// PlayerCraft
				if (tie.ReadByte() != 0)
				{
					tie.Position = 0x1CA + i * 0x124 + 0x37;    // IFF
					playerIsImperial = tie.ReadByte() == 1;
					break;
				}
			}
			tie.Position = 0x1CA;
			#region Flight Groups
			for (i = 0; i < fgs; i++)       //Flight Groups
			{
				long tiePos = tie.Position;
				xvtPos = xvt.Position;
				bw.Write(br.ReadBytes(12));     //FG Name
				tie.Position = tiePos + 24;
				xvt.Position = xvtPos + 40;
				bw.Write(br.ReadBytes(12));     //Cargo
				xvt.Position = xvtPos + 60;
				bw.Write(br.ReadBytes(12));     //Special Cargo
				xvt.Position = xvtPos + 80;
				bw.Write(br.ReadBytes(8));      //SC ship# -> IFF
				tie.Position--;
				// if player is Imperial, have to switch Team/IFF values, since Imps have to be Team 1 (0) and Rebs are Team 2 (1)
				xvt.Position = xvtPos + 88;
				switch (tie.ReadByte())
				{
					case 0:
						if (playerIsImperial) xvt.WriteByte(1);
						else xvt.WriteByte(0);
						break;
					case 1:
						if (playerIsImperial) xvt.WriteByte(0);
						else xvt.WriteByte(1);
						break;
					default:
						tie.Position--;
						xvt.WriteByte(br.ReadByte());
						break;
				}
				bw.Write(br.ReadBytes(9));      //AI -> # of waves
				tie.Position++;
				xvt.Position += 2;
				if (tie.ReadByte() != 0)
				{
					xvt.WriteByte(1);                                                       //Player 1
					xvt.Position++;
					tie.Position--;
					xvt.WriteByte(Convert.ToByte(tie.ReadByte() - 1));                      //Player's craft #
				}
				else xvt.Position += 3;
				bw.Write(br.ReadBytes(3));      //Orientation values
				tie.Position = tiePos + 73;
				xvt.Position = xvtPos + 109;
				bw.Write(br.ReadBytes(9));      //Arrival diff, trigger 1&2
				xvt.Position += 2;
				xvt.WriteByte(br.ReadByte());                              //trigger 1 AND/OR 2
				tie.Position += 1;
				xvt.Position = xvtPos + 134;
				bw.Write(br.ReadBytes(6));      //Arrv delay -> Dep trigger
				tie.Position += 2;
				xvt.Position += 9;
				xvt.WriteByte(br.ReadByte());                              //Abort trigger
				tie.Position += 3;
				xvt.Position += 4;
				bw.Write(br.ReadBytes(8));      //Arrv/Dep methods
				bw.Write(br.ReadBytes(18));     //Order 1
				xvt.Position += 64;
				bw.Write(br.ReadBytes(18));     //Order 2
				xvt.Position += 64;
				bw.Write(br.ReadBytes(18));     //Order 3
				xvt.Position = xvtPos + 502;
				j = br.ReadByte();
				if (j == 9)                                                                 // if EXIST
				{
					xvt.Position--;
					xvt.WriteByte(1);                                                       // must NOT
					xvt.WriteByte(2);                                                       // be destroyed
				}
				else xvt.WriteByte(Convert.ToByte(j));                                      //Primary Goal
				byte b = br.ReadByte();
				if (b == 1) b++;    // 50%
				else if (b >= 2) b += 2;    // >=1 and on
				xvt.WriteByte(b);                                                           //Amount
				xvt.WriteByte(Convert.ToByte((j != 10 && j != 9) ? 1 : 0));                 //250 points  [JB] Changed to 250 points only if a condition exists (not "FALSE") but also not "must have survived"
				if (j != 10) xvt.WriteByte(1);                                              //Enable Primary Goal
				else xvt.WriteByte(0);
				xvt.Position += 73;
				xvt.WriteByte(2);                                                           //Secondary Goal
				j = br.ReadByte();
				if (j == 9)                                                                 // if EXIST
				{
					xvt.Position--;
					xvt.WriteByte(3);                                                       // BONUS must NOT
					xvt.WriteByte(2);                                                       // be destroyed
				}
				else xvt.WriteByte(Convert.ToByte(j));
				b = br.ReadByte();
				if (b == 1) b++;    // 50%
				else if (b >= 2) b += 2;    // >=1 and on
				xvt.WriteByte(b);
				xvt.WriteByte(Convert.ToByte((j != 10) ? 1 : 0));                           //250 points  [JB] Changed to 250 points only if a condition exists (not "FALSE")
				if (j != 10) xvt.WriteByte(1);
				else xvt.WriteByte(0);
				xvt.Position += 73;
				xvt.WriteByte(2);                                                           //Secret Goal
				j = br.ReadByte();
				if (j == 9)                                                                 // if EXIST
				{
					xvt.Position--;
					xvt.WriteByte(3);                                                       // BONUS must NOT
					xvt.WriteByte(2);                                                       // be destroyed
				}
				else xvt.WriteByte(Convert.ToByte(j));
				b = br.ReadByte();
				if (b == 1) b++;    // 50%
				else if (b >= 2) b += 2;    // >=1 and on
				xvt.WriteByte(b);
				xvt.Position += 1;
				if (j != 10) xvt.WriteByte(1);
				else xvt.WriteByte(0);
				xvt.Position += 73;
				xvt.WriteByte(2);                                                           //Bonus Goal
				j = br.ReadByte();
				if (j == 9)                                                                 // if EXIST
				{
					xvt.Position--;
					xvt.WriteByte(3);                                                       // BONUS must NOT
					xvt.WriteByte(2);                                                       // be destroyed
				}
				else xvt.WriteByte(Convert.ToByte(j));
				b = br.ReadByte();
				if (b == 1) b++;    // 50%
				else if (b >= 2) b += 2;    // >=1 and on
				xvt.WriteByte(b);
				xvt.WriteByte(br.ReadByte());      //Bonus points, will need fiddling
				if (j != 10) xvt.WriteByte(1);
				else xvt.WriteByte(0);
				xvt.Position += 74;
				xvt.WriteByte(10);                                  //10 is the 'null' trigger (goal 5)
				xvt.Position += 77;
				xvt.WriteByte(10);                                  //goal 6
				xvt.Position += 77;
				xvt.WriteByte(10);                                  //goal 7
				xvt.Position += 77;
				xvt.WriteByte(10);                                  //goal 8
				tie.ReadByte();
				xvt.Position = xvtPos + 1126;
				bw.Write(br.ReadBytes(30));     //X points
				xvt.Position += 14;
				bw.Write(br.ReadBytes(30));     //Y points
				xvt.Position += 14;
				bw.Write(br.ReadBytes(30));     //Z points
				xvt.Position += 14;
				bw.Write(br.ReadBytes(30));     //Enable points
				xvt.Position += 90;                                                         //goto End
				tie.Position += 4;
			}
			#endregion
			#region Messages
			for (i = 0; i < messages; i++)
			{
				xvt.WriteByte(Convert.ToByte(i));
				xvt.Position++;
				bw.Write(br.ReadBytes(64)); //Color & message
				xvt.WriteByte(1);
				xvt.Position += 9;
				bw.Write(br.ReadBytes(8));  //triggers 1 & 2
				xvt.Position++;
				tie.Position += 17;
				xvt.WriteByte(br.ReadByte());                          //trigger 1 AND/OR 2
				tie.Position -= 2;
				xvt.Position += 28;
				xvt.WriteByte(br.ReadByte());                          //Delay
				tie.Position++;
				xvt.Position++;
			}
			#endregion
			xvt.WriteByte(3); xvt.Position++;   //Unknown
			#region Global Goals
			bw.Write(br.ReadBytes(8));          //Prim Goal trigger 1 & 2
			xvt.Position += 2;
			tie.Position += 17;
			xvt.WriteByte(br.ReadByte());       //Prim trigger 1 AND/OR 2
			xvt.Position += 73;
			tie.Position += 2;
			bw.Write(br.ReadBytes(8));          //Sec Goal trigger 1 & 2
			xvt.Position += 2;
			tie.Position += 17;
			xvt.WriteByte(br.ReadByte());       //Sec Goal trigger A/O

			//[JB] Convert TIE bonus global goals into a second set of XvT secondary goals
			tie.Position += 2; //Skip 2 bytes after the previous and/or byte to reach the start of the bonus goal triggers
			bw.Write(br.ReadBytes(8));          //Bonus Goal trigger 1 & 2
			xvt.Position += 2;
			tie.Position += 17;
			xvt.WriteByte(br.ReadByte());       //Bonus Goal trigger A/O
			xvt.Position += 17;
			xvt.WriteByte(0);   //And/Or
			xvt.Position += 2;  //Jump to the end of the Global Goal block, which happens to be the start of the second teams's global goal block
			xvtPos = xvt.Position;

			//[JB] Patch to convert all "must be FALSE" conditions to TRUE so that and/or doesn't cause conflicts
			xvt.Position -= 0x2A; //Rewind back to start of XvT Secondary, Trigger 1
			if (xvt.ReadByte() == 0x0A) { xvt.Position--; xvt.WriteByte(0); }  //Trig1
			xvt.Position += 3;
			if (xvt.ReadByte() == 0x0A) { xvt.Position--; xvt.WriteByte(0); }  //Trig2
			xvt.Position += 3;
			xvt.Position += 3;  //Jump the gap between Trig 1/2 and T3/4
			if (xvt.ReadByte() == 0x0A) { xvt.Position--; xvt.WriteByte(0); }  //Trig3
			xvt.Position += 3;
			if (xvt.ReadByte() == 0x0A) { xvt.Position--; xvt.WriteByte(0); }  //Trig4

			xvt.Position = xvtPos;
			for (j = 0; j < 9; j++)
			{
				xvt.WriteByte(3);
				xvt.Position += 127;
			}
			#endregion
			tie.Position = 0x19A;
			#region IFF/Teams
			xvtPos = xvt.Position;
			xvt.WriteByte(1);
			xvt.Position++;
			if (playerIsImperial) bw.Write(Encoding.ASCII.GetBytes("Imperial"));
			else bw.Write(Encoding.ASCII.GetBytes("Rebel"));
			xvt.Position = xvtPos + 0x1A;
			xvt.WriteByte(1);                                   //Team 1 Allies
			xvt.Position++;                                     //Team 2 bad guys
			if (tie.ReadByte() == 49) xvt.WriteByte(1); else xvt.Position++;            //IFF 3 stance (blue)
			tie.Position = 422;
			if (tie.ReadByte() == 49) xvt.WriteByte(1); else xvt.Position++;            //IFF 4 stance (purple)
			tie.Position = 434;
			if (tie.ReadByte() == 49) xvt.WriteByte(1); else xvt.Position++;            //IFF 5 stance (red)
			tie.Position = 446;
			if (tie.ReadByte() == 49) xvt.WriteByte(1); else xvt.Position++;            //IFF 6 stance (purple)
			xvt.Position += 4;
			tie.Position = 24;
			bw.Write(br.ReadBytes(128)); //Primary mission complete
			tie.Position += 128;
			bw.Write(br.ReadBytes(128)); //Primary failed
			tie.Position -= 256;  //[JB] Fixed offset, was off by one.
			bw.Write(br.ReadBytes(128)); //Secondary complete
			tie.Position = 410;
			xvt.Position += 67;
			xvtPos = xvt.Position;
			xvt.WriteByte(1); xvt.Position++;
			if (playerIsImperial) bw.Write(Encoding.ASCII.GetBytes("Rebel"));
			else bw.Write(Encoding.ASCII.GetBytes("Imperial"));
			xvt.Position = xvtPos + 0x1E7;
			xvt.WriteByte(1); xvt.Position++;
			if (tie.ReadByte() != 49) tie.Position--;                                   //check for hostile char
			bw.Write(br.ReadBytes(11));     //IFF 3 name
			xvt.Position += 474; xvt.WriteByte(1); xvt.Position++;
			tie.Position = 422;
			if (tie.ReadByte() != 49) tie.Position--;
			bw.Write(br.ReadBytes(11));     //IFF 4 name
			xvt.Position += 474; xvt.WriteByte(1); xvt.Position++;
			tie.Position = 434;
			if (tie.ReadByte() != 49) tie.Position--;
			bw.Write(br.ReadBytes(11));     //IFF 5 name
			xvt.Position += 474; xvt.WriteByte(1); xvt.Position++;
			tie.Position = 446;
			if (tie.ReadByte() != 49) tie.Position--;
			bw.Write(br.ReadBytes(11));     //IFF 6 name
			xvt.Position += 474; xvt.WriteByte(1);                                      //markers infront of other IFF name spots
			xvt.Position += 486; xvt.WriteByte(1);
			xvt.Position += 486; xvt.WriteByte(1);
			xvt.Position += 486; xvt.WriteByte(1);
			#endregion
			xvt.Position += 0x1E6;
			tie.Position = 0x21E + 0x124 * fgs + 0x5A * messages;
			xvtPos = xvt.Position;
			#region Briefing
			bw.Write(br.ReadBytes(0x32A)); //Briefing
			xvt.WriteByte(1); xvt.Position += 9;
			xvt.Position = xvtPos;
			j = (short)(brXvT.ReadInt16() * 0x14 / 0xC);        // adjust overall briefing length
			xvt.Position -= 2;
			bw.Write(j);
			xvt.Position += 8;
			for (i = 0; i < 0x320; i += 4)      // work our way through length of briefing. i automatically increases by 4 per event
			{
				j = brXvT.ReadInt16();
				if (j == 0x270F) break;     // stop check at t=9999, end briefing
				j = (short)(j * 0x14 / 0xC);
				xvt.Position -= 2;
				bw.Write(j);
				j = brXvT.ReadInt16();       // now get the event type
				if (j == 7)     // Zoom map command
				{
					j = (short)(brXvT.ReadInt16() * 58 / 47); // X
					xvt.Position -= 2;
					bw.Write(j);
					j = (short)(brXvT.ReadInt16() * 88 / 47); // Y
					xvt.Position -= 2;
					bw.Write(j);
					i += 4;
				}
				else
				{
					xvt.Position += 2 * _brf[j];     // skip over vars
					i += (short)(2 * _brf[j]);    // increase length counter by skipped vars
				}
			}
			#endregion
			xvt.Position = 0x1BDE + 0x562 * fgs + 0x74 * messages;
			#region Briefing tags & strings
			int briefingTagLength = 0;
			for (i = 0; i < 64; i++)
			{
				j = br.ReadInt16();       //check length..  (will always be <256)
				briefingTagLength += 2 + j;  //[JB] Calculate briefing size so we can factor it into calculations to find the description location.
				bw.Write(j);             //..write length..
				if (j != 0)                                     //and copy if not 0
					for (int k = 0; k < j; k++) xvt.WriteByte(br.ReadByte());
			}
			for (i = 0; i < 7; i++)
			{   // End Briefing event at time=9999
				xvt.WriteByte(0xC8); xvt.Position += 5; xvt.WriteByte(2); xvt.Position += 3; xvt.WriteByte(0xF); xvt.WriteByte(0x27); xvt.WriteByte(0x22);
				xvt.Position += 0x3A7;
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
					tie.Position++;
					continue;
				}
				byte[] buffer = new byte[len];
				tie.Read(buffer, 0, len);
				string s = System.Text.Encoding.ASCII.GetString(buffer);
				int sep = s.IndexOf((char)0xA);
				if (sep >= 0)
				{
					string q = s.Substring(0, sep);
					string a = s.Substring(sep + 1);
					a = a.Replace((char)0xA, ' ');
					a = a.Replace((char)0x2, '[');
					a = a.Replace((char)0x1, ']');
					if (preMissionQuestions.Length > 0) preMissionQuestions += "$";
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
					tie.Position += 3;
					continue;
				}
				int qCondition = tie.ReadByte();
				int qType = tie.ReadByte();
				byte[] buffer = new byte[len - 2];  //Length includes condition/type bytes.
				tie.Read(buffer, 0, len - 2);

				if (qCondition == 0 || qType == 0) continue;

				string s = System.Text.Encoding.ASCII.GetString(buffer);
				if (buffer[len - 3] == 0xFF)  //If this is the last byte of the string data, remove it.
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
			xvt.Position = 0xA4 + (fgs * 0x562) + (messages * 0x74) + (0x80 * 10) + (0x1E7 * 10); //Header + FGs + Messages + 10 Globals + 10 Teams
			xvt.Position += (0x334 + briefingTagLength) + (0x3B4 * 7);  //Briefing 1 (plus tag/string lengths) + 7 empty briefings (static size)
			xvt.Position += (0x600 * fgs) + 0x5A00;  //FGGoalStrings(FGs*8*3*64) + GlobalGoalStrings(10*3*4*3*64)
			xvt.Position += (0xC00 * 10);  //Empty GlobalGoalStrings data.

			int maxLength = (!toBoP ? 0x400 : 0x1000);
			if (preMissionQuestions.Length > maxLength) preMissionQuestions = preMissionQuestions.Remove(maxLength);
			if (postMissionSuccess.Length > maxLength) postMissionSuccess = postMissionSuccess.Remove(maxLength);
			if (postMissionFail.Length > maxLength) postMissionFail = postMissionFail.Remove(maxLength);

			byte[] desc;
			if (!toBoP) desc = System.Text.Encoding.ASCII.GetBytes(preMissionQuestions);
			else desc = System.Text.Encoding.ASCII.GetBytes(postMissionSuccess);

			xvtPos = xvt.Position;
			xvt.Write(desc, 0, desc.Length);
			xvt.Position = xvtPos + maxLength - 1;
			xvt.WriteByte(0);

			if (toBoP)
			{
				xvtPos += maxLength;
				desc = Encoding.ASCII.GetBytes(postMissionFail);
				xvt.Write(desc, 0, desc.Length);
				xvt.Position = xvtPos + maxLength - 1;
				xvt.WriteByte(0);

				xvtPos += maxLength;
				desc = Encoding.ASCII.GetBytes(preMissionQuestions);
				xvt.Write(desc, 0, desc.Length);
				xvt.Position = xvtPos + maxLength - 1;
				xvt.WriteByte(0);
			}
			#endregion

			xvt.Close();
			tie.Close();
		}

		public static void TieToXwa(string originalPath, string newPath)
		{
			//instead of writing it all out again, cheat and use the other two
			string temp = "temp.tie";
			TieToXvt(originalPath, temp, true);
			XvtToXwa(temp, newPath, false);
			File.Delete(temp);
		}

		public static void XvtToXwa(string originalPath, string newPath, bool toSkirmish)
		{
			short[] fgIcons;
			FileStream xvt, xwa;                                                    //usual crap...
			xvt = File.OpenRead(originalPath);
			xwa = File.Open(newPath, FileMode.Create, FileAccess.ReadWrite);
			BinaryReader br = new BinaryReader(xvt);
			BinaryReader brXWA = new BinaryReader(xwa);
			BinaryWriter bw = new BinaryWriter(xwa);
			xwa.WriteByte(18);
			xwa.Position++;
			bool isBoP = (xvt.ReadByte() == 14);
			xvt.Position = 2;
			short i, j;
			short fgs = br.ReadInt16();
			short messages = br.ReadInt16();
			bw.Write((short)(fgs + 2)); // [JB] Modified to +2 since generated skirmish files have two backdrops for ambient lighting
			bw.Write(messages);
			fgIcons = new short[fgs];

			xwa.Position = 8; xwa.WriteByte(1); xwa.Position = 11; xwa.WriteByte(1);        //unknowns
			xvt.Position = 0x14;
			xwa.Position = 0x14;
			for (i = 0; i < 4; i++) // IFFs
			{
				if (xvt.ReadByte() != 49)
				{
					xvt.Position--;
					bw.Write(br.ReadBytes(20));
				}
				else
				{
					bw.Write(br.ReadBytes(19));
					xwa.Position++;
				}
			}
			xwa.Position = 100;
			bw.Write(System.Text.Encoding.ASCII.GetBytes("The Final Frontier"));    //make a nice Region name :P
			xwa.Position = 0x23AC; xwa.WriteByte(6);                        //starting hangar
			xwa.Position++;
			xvt.Position = 0x66; bw.Write(br.ReadByte());       //time limit (minutes)
			xwa.Position = 0x23B3; xwa.WriteByte(0x62);                     //unknown
			xwa.Position = 0x23F0;

			//[JB] Jumping ahead to get the briefing locations before we load in the FGs.
			long brief1BlockStart = 0xA4 + (0x562 * fgs) + (0x74 * messages) + 0x500 + 0x1306;  //FGs, messages, teams, global goals
			xvt.Position = brief1BlockStart + 0x334;  //Jump to tags
			long brief1StringSize = 0;
			for (i = 0; i < 64; i++)   //32 tags + 32 strings
			{
				int len = br.ReadInt16();
				brief1StringSize += 2;
				xvt.Position += len;
				brief1StringSize += len;
			}
			xvt.Position = 0xA4;  //Rewind back to start of FG data

			#region Flight Groups
			long xvtPos; //[JB] Sometimes need to bookmark XvT too for better offset handling
			long xwaPos;
			bool isPlayer = false;
			int playerCraft = 0; //[JB] for combat engagements
			short[] briefShipCount = new short[2];
			briefShipCount[0] = 0;
			briefShipCount[1] = 0;
			for (i = 0; i < fgs; i++)
			{
				xvtPos = xvt.Position;
				xwaPos = xwa.Position;
				bw.Write(br.ReadBytes(20));   //name
				byte[] buffer2 = new byte[8];
				byte[] des2 = new byte[4];
				xvt.Read(buffer2, 0, 8);
				xvt2XWA_ConvertDesignations(buffer2, des2);
				xvt.Position -= 8;
				xwa.WriteByte(des2[0]); xwa.WriteByte(des2[1]);
				xwa.WriteByte(des2[2]); xwa.WriteByte(des2[3]);
				xwa.Position++;       //Skip unknown
				xwa.WriteByte(255); //Global cargos set to none
				xwa.WriteByte(255);
				xwa.Position = xwaPos + 40;
				xvt.Position += 20;
				bw.Write(br.ReadBytes(40));   //Cargo & Special
				xwa.Position = xwaPos + 0x69;
				bw.Write(br.ReadBytes(26));           //SC ship# -> Roll
				xwa.Position = xwaPos + 0x87;
				xvt.Position = xvtPos + 0x6D;
				bw.Write(br.ReadByte());    // Arr difficulty
				xvt.Position = xvtPos + 0x64;
				// [JB] Modified
				if (!toSkirmish)
				{
					if (xvt.ReadByte() != 0 && !isPlayer) { isPlayer = true; }              //check for multiple player craft, take the first one
					else
					{
						xwa.Position = xwaPos + 0x7D;
						xwa.WriteByte(0);
					}
				}
				if (xvt.ReadByte() != 0) { playerCraft++; }

				xwa.Position = xwaPos + 0x88;
				xvt.Position = xvtPos + 0x6E;
				bw.Write(br.ReadInt32());                //Arival trigger 1 (cheating and using Int32 since it's 4 bytes)...
				xwa.Position += 2;
				bw.Write(br.ReadInt32());                //... and 2
				xwa.Position += 4;
				xvt.Position += 2;
				bw.Write(br.ReadByte());                                    //AT 1 AND/OR 2
				xwa.Position++;
				bw.Write(br.ReadInt32());                //AT 3
				xwa.Position += 2;
				bw.Write(br.ReadInt32());                //AT 4
				xwa.Position += 4;
				xvt.Position += 2;
				bw.Write(br.ReadByte());                                    //AT 3 AND/OR 4
				xwa.Position++;
				bw.Write(br.ReadInt64());                //AT 1/2 AND/OR 3/4 -> DT (8 bytes)
				xwa.Position += 2;
				bw.Write(br.ReadInt32());                //DT 2
				xwa.Position += 4;
				xvt.Position += 2;
				bw.Write(br.ReadByte());                                    //DT 1 AND/OR 2
				xwa.Position += 3;
				xvt.Position += 2;
				bw.Write(br.ReadByte());                                    //Abort trigger
				xvt.Position += 4;
				xwa.Position += 3;
				bw.Write(br.ReadInt64());                //Arr/Dep methods
				long xvtSubOrderStart = xvt.Position;  //[JB] I'm going to let the original code run then rewind to these offsets later to apply my patches.
				long xwaSubOrderStart = xwa.Position;
				bw.Write(br.ReadBytes(18));       //Order 1
				//xwa.Position += 2;
				shipOFix(xvt, xwa);
				xvt.Position = xvtPos + 0x46E;
				for (j = 0; j < 8; j++)
				{
					bw.Write(br.ReadInt16());   //WP X
					xvt.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP Y
					xvt.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP Z
					xvt.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP enabled
					xvt.Position -= 0x84;
				}

				//[JB] Patch wait times
				xvt.Position = xvtSubOrderStart;
				xwa.Position = xwaSubOrderStart;
				xvtToXWA_ConvertOrderTime(xvt, xwa);
				//Copy order speed
				xvt.Position = xvtSubOrderStart + 18;
				xwa.Position = xwaSubOrderStart + 18;
				bw.Write(br.ReadByte());
				//After XvT speed comes flight group player designation, load it to patch the role later. 
				byte[] role = new byte[16];
				xvt.Read(role, 0, 16);
				xwa.Position = xwaPos + 0x50; //Patch in the display role for the player FG slot screen.
				xwa.Write(role, 0, 16);
				//[JB] End patch code.

				xwa.Position = xwaPos + 0x15E;
				xvt.Position = xvtPos + 0xF4;
				xvtSubOrderStart = xvt.Position;  //[JB] ShipOFix modifies the offsets and I assume it's bad to add anything so I'm going to let the original code run then rewind to these offsets later to apply my patches.
				xwaSubOrderStart = xwa.Position;
				bw.Write(br.ReadBytes(18));       //Order 2
				//xwa.Position += 2;
				shipOFix(xvt, xwa);
				xvt.Position = xvtPos + 0x46E;
				for (j = 0; j < 8; j++)
				{
					bw.Write(br.ReadInt16());   //WP X
					xvt.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP Y
					xvt.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP Z
					xvt.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP enabled
					xvt.Position -= 0x84;
				}
				//[JB] Patch wait times
				xvt.Position = xvtSubOrderStart;
				xwa.Position = xwaSubOrderStart;
				xvtToXWA_ConvertOrderTime(xvt, xwa);
				//Copy order speed
				xvt.Position = xvtSubOrderStart + 18;
				xwa.Position = xwaSubOrderStart + 18;
				bw.Write(br.ReadByte());
				//[JB] End patch code.

				xwa.Position = xwaPos + 0x1F2;
				xvt.Position = xvtPos + 0x146;
				xvtSubOrderStart = xvt.Position;  //[JB] ShipOFix modifies the offsets and I assume it's bad to add anything so I'm going to let the original code run then rewind to these offsets later to apply my patches.
				xwaSubOrderStart = xwa.Position;
				//bw.Write(br.ReadBytes(18));       //Order 3
				//xwa.Position += 2;
				shipOFix(xvt, xwa);
				xvt.Position = xvtPos + 0x46E;
				for (j = 0; j < 8; j++)
				{
					bw.Write(br.ReadInt16());   //WP X
					xvt.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP Y
					xvt.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP Z
					xvt.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP enabled
					xvt.Position -= 0x84;
				}
				//[JB] Patch wait times
				xvt.Position = xvtSubOrderStart;
				xwa.Position = xwaSubOrderStart;
				xvtToXWA_ConvertOrderTime(xvt, xwa);
				//Copy order speed
				xvt.Position = xvtSubOrderStart + 18;
				xwa.Position = xwaSubOrderStart + 18;
				bw.Write(br.ReadByte());
				//[JB] End patch code.

				xwa.Position = xwaPos + 0x286;
				xvt.Position = xvtPos + 0x198;
				xvtSubOrderStart = xvt.Position;  //[JB] ShipOFix modifies the offsets and I assume it's bad to add anything so I'm going to let the original code run then rewind to these offsets later to apply my patches.
				xwaSubOrderStart = xwa.Position;
				bw.Write(br.ReadBytes(18));       //Order 4
				//xwa.Position += 2;
				shipOFix(xvt, xwa);
				xvt.Position = xvtPos + 0x46E;
				for (j = 0; j < 8; j++)
				{
					bw.Write(br.ReadInt16());   //WP X
					xvt.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP Y
					xvt.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP Z
					xvt.Position += 0x2A;
					bw.Write(br.ReadInt16());   //WP enabled
					xvt.Position -= 0x84;
				}
				//[JB] Patch wait times
				xvt.Position = xvtSubOrderStart;
				xwa.Position = xwaSubOrderStart;
				xvtToXWA_ConvertOrderTime(xvt, xwa);
				//Copy order speed
				xvt.Position = xvtSubOrderStart + 18;
				xwa.Position = xwaSubOrderStart + 18;
				bw.Write(br.ReadByte());
				//[JB] End patch code.

				xvt.Position = xvtPos + 0x1EA;
				xwa.Position = xwaPos + 0xA3A;
				bw.Write(br.ReadInt32());        //jump to Order 4 T1
				xwa.Position += 2;
				bw.Write(br.ReadInt32());        //jump to Order 4 T2
				xwa.Position += 4;
				xvt.Position += 2;
				bw.Write(br.ReadByte());                            //jump to Order 4 T 1AND/OR 2
				xwa.Position = xwaPos + 0xB0A;
				xvt.Position = xvtPos + 0x1F5;
				for (j = 0; j < 8; j++)
				{
					bw.Write(br.ReadBytes(14));       //Goals
					xvt.Position += 0x40;
					xwa.Position += 0x42;
				}
				xvt.Position = xvtPos + 0x466;
				xwa.Position = xwaPos + 0xD8A;
				for (j = 0; j < 3; j++)
				{
					bw.Write(br.ReadInt16());   //SP X
					xvt.Position += 0x2A;
					bw.Write(br.ReadInt16());   //SP Y
					xvt.Position += 0x2A;
					bw.Write(br.ReadInt16());   //SP Z
					xvt.Position += 0x2A;
					bw.Write(br.ReadInt16());   //SP enabled
					xvt.Position -= 0x84;
				}
				xvt.Position = xvtPos + 0x480;
				bw.Write(br.ReadInt16());   //HYP X
				xvt.Position += 0x2A;
				bw.Write(br.ReadInt16());   //HYP Y
				xvt.Position += 0x2A;
				bw.Write(br.ReadInt16());   //HYP Z
				xvt.Position += 0x2A;
				bw.Write(br.ReadInt16());   //HYP enabled
				xvt.Position = xvtPos + 0x506;
				if (xvt.ReadByte() == 1)        //okay, briefing time. if BP enabled..
				{
					//[JB] 0x23F0 (fileheader) + 0xE60 (GlobalGoal[10]) + 0x1306 (Team[10]) = 0x4556 (17750 dec)
					xwa.Position = 17762 + (fgs + 2) * 3646 + messages * 162 + briefShipCount[0] * 20;  //place for next insert command  [JB] Modified to FGs+2
					xwa.WriteByte(26); xwa.Position++; bw.Write(briefShipCount[0]);        //Add command
					fgIcons[i] = briefShipCount[0];     // store the Icon# for the FG
					xvt.Position = xvtPos + 0x52; bw.Write(br.ReadByte()); xwa.Position++;        //Ship
					xvt.Position = xvtPos + 0x57; bw.Write(br.ReadByte()); xwa.Position += 3;     //IFF
					xvt.Position = xvtPos + 0x482;
					xwa.WriteByte(28); xwa.Position++; bw.Write(briefShipCount[0]);        //Move command
					bw.Write(br.ReadInt16());   //BP X
					xvt.Position += 0x2A;
					bw.Write(br.ReadInt16());   //BP Y
					briefShipCount[0]++;
				}
				if (toSkirmish)
				{
					xvt.Position = xvtPos + 0x508;
					if (xvt.ReadByte() == 1)        //okay, briefing time. if BP enabled..
					{
						//[JB] 0x23F0 (fileheader) + 0xE60 (GlobalGoal[10]) + 0x1306 (Team[10]) = 0x4556 (17750 dec)
						xwa.Position = 17750 + (fgs + 2) * 3646 + messages * 162 + briefShipCount[1] * 20;  //place for next insert command  [JB] Modified to FGs+2
						xwa.Position += 0x4414 + brief1StringSize + 384 + 0x000A + 2;  //briefing(minus strings) + XvT string list size + 192 shorts for empty messages in XWA + start of Brief2 event list
						xwa.WriteByte(26); xwa.Position++; bw.Write(briefShipCount[1]);        //Add command
						fgIcons[i] = briefShipCount[1];     // store the Icon# for the FG
						xvt.Position = xvtPos + 0x52; bw.Write(br.ReadByte()); xwa.Position++;        //Ship
						xvt.Position = xvtPos + 0x57; bw.Write(br.ReadByte()); xwa.Position += 3;     //IFF
						xvt.Position = xvtPos + 0x484;  //Offset for BP2
						xwa.WriteByte(28); xwa.Position++; bw.Write(briefShipCount[1]);        //Move command
						bw.Write(br.ReadInt16());   //BP X
						xvt.Position += 0x2A;
						bw.Write(br.ReadInt16());   //BP Y
						briefShipCount[1]++;
					}
				}
				xwa.Position = xwaPos + 0xDC7;
				xvt.Position = xvtPos + 0x523;
				bw.Write(br.ReadInt32());        //CM -> Global Unit
				xwa.Position++;
				xvt.Position += 9;
				bw.Write(br.ReadBytes(48));   //Optionals
				xvt.Position = xvtPos + 0x562;
				xwa.Position = xwaPos + 0xE3E;
			}
			Random rnd = new Random();
			int craft1 = rnd.Next(1, 59);
			int craft2 = rnd.Next(63, 102);
			short[] coord1 = { 0, 0, 0 };
			short[] coord2 = { 0, 0, 0 };
			coord1[rnd.Next(0, 2)] = 1; //[JB] ensures backdrop isn't on origin
			coord2[rnd.Next(0, 2)] = -1;

			//okay, now write in the default Backdrop
			xwaPos = xwa.Position;
			bw.Write(System.Text.Encoding.ASCII.GetBytes("1.0 1.0 1.0"));   //Name
			xwa.Position = xwaPos + 20;
			xwa.WriteByte(255); xwa.WriteByte(255); xwa.Position += 3; xwa.WriteByte(255); xwa.WriteByte(255);  //EnableDesignation1, EnableDesignation2, ... GlobalCargoIndex GlobalSpecialCargoIndex 
			xwa.Position = xwaPos + 40;
			bw.Write(System.Text.Encoding.ASCII.GetBytes("1.0"));   //Brightness
			xwa.Position = xwaPos + 60;
			bw.Write(System.Text.Encoding.ASCII.GetBytes("1.9"));   //Size (match skirmish output)
			xwa.Position = xwaPos + 105; xwa.WriteByte(2);           //SpecialCargoCraft?
			xwa.Position++; xwa.WriteByte(183); xwa.WriteByte(1);
			xwa.Position += 3; xwa.WriteByte(4); //[JB] Changed to IFF Red since it is used less frequently, and is less likely to interfere with IFF triggers.
			xwa.Position = xwaPos + 113; xwa.WriteByte(9);   //[JB] Team (so it doesn't interfere with triggers)
			xwa.Position = xwaPos + 120; xwa.WriteByte(31);  //[JB] Global group (for same reason)
			xwa.Position = xwaPos + 2827; xwa.WriteByte(10); xwa.Position += 79; xwa.WriteByte(10); xwa.Position += 79; xwa.WriteByte(10); xwa.Position += 79; xwa.WriteByte(10);
			xwa.Position += 79; xwa.WriteByte(10); xwa.Position += 79; xwa.WriteByte(10); xwa.Position += 79; xwa.WriteByte(10); xwa.Position += 79; xwa.WriteByte(10); xwa.Position += 79;
			xwa.Position = xwaPos + 3466;
			bw.Write(coord1[0]);
			bw.Write(coord1[1]);
			bw.Write(coord1[2]);
			xwa.Position++; xwa.WriteByte(1);
			xwa.Position = xwaPos + 3602;
			xwa.WriteByte((byte)craft1); //[JB] Set backdrop value to random(1-59)
			xwa.Position = xwaPos + 3646;

			//[JB] Adding a second backdrop, since the game generates two backdrops for skirmish files.  Offers better ambient lighting for the player.
			xwaPos = xwa.Position;
			bw.Write(System.Text.Encoding.ASCII.GetBytes("1.0 1.0 1.0"));   //Name
			xwa.Position = xwaPos + 20;
			xwa.WriteByte(255); xwa.WriteByte(255); xwa.Position += 3; xwa.WriteByte(255); xwa.WriteByte(255);  //[JB] Modified to update both global cargo values.
			xwa.Position = xwaPos + 40;
			bw.Write(System.Text.Encoding.ASCII.GetBytes("1.0"));   //Brightness
			xwa.Position = xwaPos + 60;
			bw.Write(System.Text.Encoding.ASCII.GetBytes("1.0"));   //Size
			xwa.Position = xwaPos + 105; xwa.WriteByte(2);
			xwa.Position++; xwa.WriteByte(183); xwa.WriteByte(1);
			xwa.Position += 3; xwa.WriteByte(4);
			xwa.Position = xwaPos + 113; xwa.WriteByte(9);   //[JB] Team (so it doesn't interfere with triggers)
			xwa.Position = xwaPos + 120; xwa.WriteByte(31);  //[JB] Global group (for same reason)
			xwa.Position = xwaPos + 2827; xwa.WriteByte(10); xwa.Position += 79; xwa.WriteByte(10); xwa.Position += 79; xwa.WriteByte(10); xwa.Position += 79; xwa.WriteByte(10);
			xwa.Position += 79; xwa.WriteByte(10); xwa.Position += 79; xwa.WriteByte(10); xwa.Position += 79; xwa.WriteByte(10); xwa.Position += 79; xwa.WriteByte(10); xwa.Position += 79;
			xwa.Position = xwaPos + 3466;
			bw.Write(coord2[0]);
			bw.Write(coord2[1]);
			bw.Write(coord2[2]);
			xwa.Position++; xwa.WriteByte(1);
			xwa.Position = xwaPos + 3602; xwa.WriteByte((byte)craft2); //[JB] Set backdrop value to random(63-102)
			xwa.Position = xwaPos + 3646;

			#endregion
			//[JB] Now that flight groups are done, check for player count and patch in skirmish mode
			if (toSkirmish && playerCraft > 1)
			{
				long backupPos = xwa.Position;
				xwa.Position = 0x23AC;
				xwa.WriteByte(4);
				xwa.Position = backupPos;
			}
			#region Messages
			for (i = 0; i < messages; i++)
			{
				xvtPos = xvt.Position;
				xwaPos = xwa.Position;
				bw.Write(br.ReadInt16());       //Message# - 1
				switch (xvt.ReadByte())                                         //takes care of colors if needed
				{
					case 49:
						bw.Write(br.ReadBytes(63)); //green
						xwa.Position = xwaPos + 142; xwa.WriteByte(0);
						break;
					case 50:
						bw.Write(br.ReadBytes(63)); //blue
						xwa.Position = xwaPos + 142; xwa.WriteByte(2);
						break;
					case 51:
						bw.Write(br.ReadBytes(63)); ;   //yellow
						xwa.Position = xwaPos + 142; xwa.WriteByte(3);
						break;
					default:
						xvt.Position--;
						bw.Write(br.ReadBytes(64)); //red
						xwa.Position = xwaPos + 142; xwa.WriteByte(1);
						break;
				}
				xwa.Position = xwaPos + 82;
				bw.Write(br.ReadBytes(14));     //Sent to.. -> T1
				xwa.Position += 2;
				bw.Write(br.ReadInt32());           //T2
				xvt.Position += 2;
				xwa.Position += 4;
				bw.Write(br.ReadByte());                                //T1 AND/OR T2
				xwa.Position++;
				bw.Write(br.ReadInt32());           //T3
				xwa.Position += 2;
				bw.Write(br.ReadInt32());           //T4
				xvt.Position += 2;
				xwa.Position += 4;
				bw.Write(br.ReadByte());                                //T3 AND/OR T4
				xwa.Position = xwaPos + 141;
				xvt.Position += 17;
				bw.Write(br.ReadByte());                                //T (1/2) AND/OR (3/4)
				xwa.Position = xwaPos + 132;         //[JB] OriginatingFG
				xwa.WriteByte(Convert.ToByte(fgs + 1));  //[JB] Set to last FG (+2 inserted backdrops so the last new FG index is FG+1). Assigning messages to backdrops ensures the object is always present so messages always fire.
				xwa.Position = xwaPos + 140;
				xvt.Position = xvtPos + 114;
				int msgDelaySec = xvt.ReadByte() * 5;  //[JB] Corrected delay time.
				xwa.WriteByte(Convert.ToByte(msgDelaySec % 60));                        //Delay
				xwa.WriteByte(Convert.ToByte(msgDelaySec / 60));
				xwa.Position += 2; xwa.WriteByte(10);  //[JB] Modified offset for second delay byte
				xwa.Position += 5; xwa.WriteByte(10);                                       //make sure the Cancel triggers are set to FALSE
				xwa.Position = xwaPos + 162;
				xvt.Position = xvtPos + 116;
			}
			#endregion
			#region Global Goals
			xvtPos = xvt.Position;
			xwaPos = xwa.Position;
			for (int ti = 0; ti < 10; ti++) //[JB] Converting all 10 teams just in case some triggers depend on them.
			{
				xvt.Position = xvtPos + (0x80 * ti);
				xwa.Position = xwaPos + (0x170 * ti);
				xwa.WriteByte(3);
				xwa.Position++;
				xvt.Position += 2;
				bw.Write(br.ReadInt32());       //Prim T1
				xwa.Position += 2;
				bw.Write(br.ReadInt32());       //PT2
				xwa.Position += 4;
				xvt.Position += 2;
				bw.Write(br.ReadByte());                            //PT 1 AND/OR 2
				xwa.Position++;
				bw.Write(br.ReadInt32());       //PT 3
				xwa.Position += 2;
				bw.Write(br.ReadInt32());       //PT 4
				xwa.Position += 4;
				xvt.Position += 2;
				bw.Write(br.ReadByte());                            //PT 3 AND/OR 4
				xvt.Position += 17;
				xwa.Position += 18;
				bw.Write(br.ReadBytes(3));      //PT (1/2) AND/OR (3/4) -> Points
				xwa.Position += 70;
				bw.Write(br.ReadInt32());       //Prev T1
				xwa.Position += 2;
				bw.Write(br.ReadInt32());       //PT2
				xwa.Position += 4;
				xvt.Position += 2;
				bw.Write(br.ReadByte());                            //PT 1 AND/OR 2
				xwa.Position++;
				bw.Write(br.ReadInt32());       //PT 3
				xwa.Position += 2;
				bw.Write(br.ReadInt32());       //PT 4
				xwa.Position += 4;
				xvt.Position += 2;
				bw.Write(br.ReadByte());                            //PT 3 AND/OR 4
				xvt.Position += 17;
				xwa.Position += 18;
				bw.Write(br.ReadBytes(3));      //PT (1/2) AND/OR (3/4) -> Points
				xwa.Position += 70;
				bw.Write(br.ReadInt32());       //Sec T1
				xwa.Position += 2;
				bw.Write(br.ReadInt32());       //ST2
				xwa.Position += 4;
				xvt.Position += 2;
				bw.Write(br.ReadByte());                            //ST 1 AND/OR 2
				xwa.Position++;
				bw.Write(br.ReadInt32());       //ST 3
				xwa.Position += 2;
				bw.Write(br.ReadInt32());       //ST 4
				xwa.Position += 4;
				xvt.Position += 2;
				bw.Write(br.ReadByte());                            //ST 3 AND/OR 4
				xvt.Position += 17;
				xwa.Position += 18;
				bw.Write(br.ReadBytes(3));      //ST (1/2) AND/OR (3/4) -> Points
				xwa.Position += 70;
			}
			xvt.Position = xvtPos + (0x80 * 10);  //10 teams
			xwa.Position = xwaPos + (0x170 * 10);
			#endregion
			#region IFF/Teams
			bw.Write(br.ReadBytes(4870));   //well, that was simple..
			#endregion
			#region Briefing
			xwaPos = xwa.Position;
			long xwaBriefing1 = xwaPos;
			bw.Write(br.ReadBytes(6));  //briefing intro
			bw.Write((short)(br.ReadInt16() + 10 * briefShipCount[0])); // adjust length for add/moves
			xvt.Position += 2;
			xwa.Position += 20 * briefShipCount[0] + 2;
			bw.Write(br.ReadBytes(0x32A));  //briefing content
			xwa.Position = xwaPos;
			j = (short)(brXWA.ReadInt16() * 0x19 / 0x14);       // adjust overall briefing length
			xwa.Position -= 2;
			bw.Write(j);
			xwa.Position += 8;
			for (i = 0; i < 0x320; i += 4)      // work our way through length of briefing. i automatically increases by 4 per event
			{
				j = brXWA.ReadInt16();
				if (j == 0x270F) break;     // stop check at t=9999, end briefing
				j = (short)(j * 0x19 / 0x14);
				xwa.Position -= 2;
				bw.Write(j);
				j = brXWA.ReadInt16();      // now get the event type
				if (j > 8 && j < 17)        // FG tags 1-8
				{
					j = brXWA.ReadInt16();  // FG number
					xwa.Position -= 2;
					bw.Write(fgIcons[j]);   // Overwrite with the Icon#
					i += 2;
				}
				else if (j == 7)        // Zoom map command
				{
					j = (short)(brXWA.ReadInt16() * 124 / 58);  // X
					xwa.Position -= 2;
					bw.Write(j);
					j = (short)(brXWA.ReadInt16() * 124 / 88);  // Y
					xwa.Position -= 2;
					bw.Write(j);
					i += 4;
				}
				else
				{
					xwa.Position += 2 * _brf[j];       // skip over vars
					i += (short)(2 * _brf[j]);   // increase length counter by skipped vars
				}
			}
			xwa.Position = 0x8960 + (fgs + 2) * 0xE3E + messages * 0xA2;  //[JB] FGs+2
			xwa.WriteByte(1);                   //show the non-existant briefing
			xwa.Position += 9;
			#endregion Briefing
			#region Briefing tags & strings
			for (i = 0; i < 32; i++)    //tags
			{
				j = br.ReadInt16();     //check length..
				bw.Write(j);                //..write length..
				if (j != 0)                                     //and copy if not 0
					bw.Write(br.ReadBytes(j));
			}
			xwa.Position += 192;
			for (i = 0; i < 32; i++)    //strings
			{
				j = br.ReadInt16();     //check length..
				bw.Write(j);                //..write length..
				if (j != 0)                                     //and copy if not 0
					bw.Write(br.ReadBytes(j));
			}
			xwa.Position += 192;
			#endregion Briefing T&S
			#region Briefing2
			//[JB] Begin briefing 2.  Basically just copy/paste the same code.
			if (toSkirmish)
			{
				long xwaBriefing2 = xwa.Position;
				xwaPos = xwa.Position;
				bw.Write(br.ReadBytes(6));  //briefing intro
				bw.Write((short)(br.ReadInt16() + 10 * briefShipCount[1])); // adjust length for add/moves
				xvt.Position += 2;
				xwa.Position += 20 * briefShipCount[1] + 2;
				bw.Write(br.ReadBytes(0x32A));  //briefing content
				xwa.Position = xwaPos;
				j = (short)(brXWA.ReadInt16() * 0x19 / 0x14);       // adjust overall briefing length
				xwa.Position -= 2;
				bw.Write(j);
				xwa.Position += 8;
				for (i = 0; i < 0x320; i += 4)      // work our way through length of briefing. i automatically increases by 4 per event
				{
					j = brXWA.ReadInt16();
					if (j == 0x270F) break;     // stop check at t=9999, end briefing
					j = (short)(j * 0x19 / 0x14);
					xwa.Position -= 2;
					bw.Write(j);
					j = brXWA.ReadInt16();      // now get the event type
					if (j > 8 && j < 17)        // FG tags 1-8
					{
						j = brXWA.ReadInt16();  // FG number
						xwa.Position -= 2;
						bw.Write(fgIcons[j]);   // Overwrite with the Icon#
						i += 2;
					}
					else if (j == 7)        // Zoom map command
					{
						j = (short)(brXWA.ReadInt16() * 124 / 58);  // X
						xwa.Position -= 2;
						bw.Write(j);
						j = (short)(brXWA.ReadInt16() * 124 / 88);  // Y
						xwa.Position -= 2;
						bw.Write(j);
						i += 4;
					}
					else
					{
						xwa.Position += 2 * _brf[j];        // skip over vars
						i += (short)(2 * _brf[j]);  // increase length counter by skipped vars
					}
				}
				xwa.Position = 0x8960 + (xwaBriefing2 - xwaBriefing1) + (fgs + 2) * 0xE3E + messages * 0xA2;   //[JB] FGs+2
				xwa.WriteByte(0);                   //show the non-existant briefing
				xwa.WriteByte(1);                   //show the non-existant briefing
				xwa.Position += 8;
				for (i = 0; i < 32; i++)    //tags
				{
					j = br.ReadInt16();     //check length..
					bw.Write(j);                //..write length..
					if (j != 0)                                     //and copy if not 0
						bw.Write(br.ReadBytes(j));
				}
				xwa.Position += 192;
				for (i = 0; i < 32; i++)    //strings
				{
					j = br.ReadInt16();     //check length..
					bw.Write(j);                //..write length..
					if (j != 0)                                     //and copy if not 0
						bw.Write(br.ReadBytes(j));
				}
				xwa.Position += 192;
			}
			else
			{
				xvt.Position += 0x334;    //Jump to tags
				for (i = 0; i < 64; i++)
				{   //32 tags + 32 strings
					j = br.ReadInt16();
					xvt.Position += j;
				}
				xwa.Position += 0x4614;  //Empty briefing plus empty tags/strings
			}
			#endregion Briefing2

			xwa.Position += 0x187C; //Skip EditorNotes
			xwa.Position += 0x3200; //Skip BriefingStringNotes
			xwa.Position += 0x1900; //Skip MessageNotes
			xwa.Position += 0xBB8;  //Skip EomNotes
			xwa.Position += 0xFA0;  //Skip Unknown
			xwa.Position += 0x12C;  //Skip DescriptionNotes

			//[JB] Briefings have variable length. Need to step over the remaining 6 XvT briefings by properly calculating how big they are.
			for (i = 2; i < 8; i++)
			{
				xvt.Position += 0x334;    //Jump to tags
				short l;
				for (j = 0; j < 64; j++)
				{  //32 tags + 32 strings
					l = br.ReadInt16();
					xvt.Position += l;
				}
			}
			#region FG Goal strings
			for (i = 0; i < fgs; i++)
			{
				for (j = 0; j < 24; j++)  //8 goals * 3 strings
				{
					if (xvt.ReadByte() == 0)
					{
						xwa.Position++;
						xvt.Position += 63;
					}
					else
					{
						xvt.Position--;
						bw.Write(br.ReadBytes(64));
					}
				}
			}
			xwa.Position += 48;                     //compensate for adding the Backdrop  [JB] Was 24 for one backdrop, needs to be 48 since I added an extra one.
			#endregion
			#region Global Goal strings
			for (i = 0; i < 10; i++)
			{
				for (j = 0; j < 36; j++)
				{
					if (xvt.ReadByte() == 0)
					{
						xwa.Position++;
						xvt.Position += 63;
					}
					else
					{
						xvt.Position--;
						bw.Write(br.ReadBytes(64));
					}
				}
				xvt.Position += 3072;
			}
			#endregion
			xwa.Position += 3552;               //skip custom order strings
			#region Debrief and Descrip
			if (!isBoP)
			{
				xwa.Position += 4096;
				xwa.WriteByte(35);
				xwa.Position += 4095;
				xwa.WriteByte(35);
				for (i = 0; i < 1024; i++)
				{
					int d = xvt.ReadByte();
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
							xwa.WriteByte(35);
							break;
						default:
							xvt.Position--;
							bw.Write(br.ReadByte());
							break;
					}
				}
				xwa.Position += 3071 + j;
			}
			else
			{
				bw.Write(br.ReadBytes(4096));   // Debrief
				xwa.WriteByte(35);
				bw.Write(br.ReadBytes(4095));   // Hints
				xvt.Position++;
				xwa.WriteByte(35);
				bw.Write(br.ReadBytes(4095));   // Brief/Description
			}
			#endregion
			xwa.WriteByte(0);               //EOF
			xvt.Close();
			xwa.Close();
		}

		static void xvt2XWA_ConvertDesignations(byte[] xvt, byte[] xwa)
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

			string t = Encoding.ASCII.GetString(xvt).ToUpper();
			for (int i = 0; i < 2; i++)
			{
				string sub = t.Substring(0, 4);
				if (sub[0] == 0) return;

				//Get the role first so that if the team is set to all, both teams can be assigned the same role.
				char team = sub[0];
				sub = sub.Substring(1);
				byte role;
				roleMap.TryGetValue(sub, out role);
				xwa[2 + i] = role;

				switch (team)
				{
					case '1': xwa[i] = 0x0; break;
					case '2': xwa[i] = 0x1; break;
					case '3': xwa[i] = 0x2; break;
					case '4': xwa[i] = 0x3; break;
					case 'A': xwa[0] = 0xA; xwa[1] = 0xB; xwa[2] = role; xwa[3] = role; break;  //~MG: the original single-designation version of this function had 0xB for 'A' and 0xA for 'H'. I have this value as being a bool, need to look into it
					case 'H': xwa[0] = 0xA; xwa[1] = 0xB; xwa[2] = role; xwa[3] = role; break;  //No idea (what 'H' means)
					default: xwa[i] = 0x0; break;
				}

				t = t.Substring(4); //Trim so the next 4 become the current.
			}
		}
		static byte convertOrderTimeXvTToXWA(byte xvtTime)
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
		static void xvtToXWA_ConvertOrderTime(FileStream xvt, FileStream xwa)
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

		static void shipOFix(FileStream original, FileStream xwa)  //seperate function for Orders
		{
			original.Position -= 12;
			if (original.ReadByte() == 2)               //Target 3
			{
				original.Position++;
				xwa.Position -= 10;
				byte shipType = (byte)original.ReadByte();
				if (shipType != 255) shipType += 1;
				xwa.WriteByte(shipType);
				original.Position -= 2;
			}
			else { xwa.Position -= 9; }
			if (original.ReadByte() == 2)               //Target 4
			{
				original.Position++;
				byte shipType = (byte)original.ReadByte();
				if (shipType != 255) shipType += 1;
				xwa.WriteByte(shipType);
				original.Position += 2;
				xwa.Position += 3;
			}
			else
			{
				xwa.Position += 4;
				original.Position += 4;
			}
			if (original.ReadByte() == 2)               //Target 1
			{
				byte shipType = (byte)original.ReadByte();
				if (shipType != 255) shipType += 1;
				xwa.WriteByte(shipType);
				xwa.Position++;
			}
			else
			{
				original.Position++;
				xwa.Position += 2;
			}
			if (original.ReadByte() == 2)               //Target 2
			{
				byte shipType = (byte)original.ReadByte();
				if (shipType != 255) shipType += 1;
				xwa.WriteByte(shipType);
				original.Position += 2;
				xwa.Position += 4;
			}
			else
			{
				original.Position += 3;
				xwa.Position += 5;
			}
		}
	}
}
