using System;
using System.IO;
using System.Windows.Forms;

namespace Converter
{
	/// <summary>
	/// X-wing series mission converter (TIE -> XvT, TIE -> XWA, XvT -> XWA)
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button cmdSave;
		private System.Windows.Forms.Label lblExist;
		private System.Windows.Forms.Label lblTest;
		private System.Windows.Forms.Button cmdExist;
		private System.Windows.Forms.Label lblType;
		private System.Windows.Forms.Button cmdConvert;
		private System.Windows.Forms.TextBox txtSave;
		private System.Windows.Forms.CheckBox chkXWA;
		private System.Windows.Forms.CheckBox chkXvT2;
		private System.Windows.Forms.SaveFileDialog savConvert;
		private System.Windows.Forms.OpenFileDialog opnExist;
		private System.Windows.Forms.TextBox txtExist;
		private System.Windows.Forms.Button cmdExit;
		private System.Windows.Forms.Label lblSave;
		public static bool T2W = false;
		public static bool hidden = false;
		public static string[] Arg;
		private System.Windows.Forms.RadioButton optImp;
		private System.Windows.Forms.RadioButton optReb;
		private System.Windows.Forms.Label lblPlayer;
		private System.ComponentModel.Container components = null;
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


		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code	
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.lblSave = new System.Windows.Forms.Label();
			this.cmdExit = new System.Windows.Forms.Button();
			this.txtExist = new System.Windows.Forms.TextBox();
			this.opnExist = new System.Windows.Forms.OpenFileDialog();
			this.savConvert = new System.Windows.Forms.SaveFileDialog();
			this.chkXvT2 = new System.Windows.Forms.CheckBox();
			this.chkXWA = new System.Windows.Forms.CheckBox();
			this.txtSave = new System.Windows.Forms.TextBox();
			this.cmdConvert = new System.Windows.Forms.Button();
			this.lblType = new System.Windows.Forms.Label();
			this.cmdExist = new System.Windows.Forms.Button();
			this.lblTest = new System.Windows.Forms.Label();
			this.lblExist = new System.Windows.Forms.Label();
			this.cmdSave = new System.Windows.Forms.Button();
			this.optImp = new System.Windows.Forms.RadioButton();
			this.optReb = new System.Windows.Forms.RadioButton();
			this.lblPlayer = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblSave
			// 
			this.lblSave.Location = new System.Drawing.Point(48, 97);
			this.lblSave.Name = "lblSave";
			this.lblSave.Size = new System.Drawing.Size(72, 14);
			this.lblSave.TabIndex = 11;
			this.lblSave.Text = "Save as...";
			// 
			// cmdExit
			// 
			this.cmdExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdExit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdExit.Location = new System.Drawing.Point(232, 156);
			this.cmdExit.Name = "cmdExit";
			this.cmdExit.Size = new System.Drawing.Size(80, 22);
			this.cmdExit.TabIndex = 9;
			this.cmdExit.Text = "Exit";
			this.cmdExit.Click += new System.EventHandler(this.CmdExitClick);
			// 
			// txtExist
			// 
			this.txtExist.Location = new System.Drawing.Point(48, 30);
			this.txtExist.Name = "txtExist";
			this.txtExist.Size = new System.Drawing.Size(272, 20);
			this.txtExist.TabIndex = 0;
			// 
			// opnExist
			// 
			this.opnExist.DefaultExt = "tie";
			this.opnExist.Filter = "Mission Files|*.tie";
			this.opnExist.Title = "Existing Mission";
			this.opnExist.FileOk += new System.ComponentModel.CancelEventHandler(this.OpnExistFileOk);
			// 
			// savConvert
			// 
			this.savConvert.DefaultExt = "tie";
			this.savConvert.Filter = "Mission Files|*.tie";
			this.savConvert.Title = "Save as...";
			this.savConvert.FileOk += new System.ComponentModel.CancelEventHandler(this.SavConvertFileOk);
			// 
			// chkXvT2
			// 
			this.chkXvT2.Checked = true;
			this.chkXvT2.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkXvT2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkXvT2.Location = new System.Drawing.Point(328, 104);
			this.chkXvT2.Name = "chkXvT2";
			this.chkXvT2.Size = new System.Drawing.Size(56, 15);
			this.chkXvT2.TabIndex = 14;
			this.chkXvT2.Text = "XvT";
			this.chkXvT2.CheckedChanged += new System.EventHandler(this.ChkXvT2CheckedChanged);
			// 
			// chkXWA
			// 
			this.chkXWA.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkXWA.Location = new System.Drawing.Point(328, 126);
			this.chkXWA.Name = "chkXWA";
			this.chkXWA.Size = new System.Drawing.Size(56, 15);
			this.chkXWA.TabIndex = 15;
			this.chkXWA.Text = "XWA";
			this.chkXWA.CheckedChanged += new System.EventHandler(this.ChkXWACheckedChanged);
			// 
			// txtSave
			// 
			this.txtSave.Location = new System.Drawing.Point(48, 111);
			this.txtSave.Name = "txtSave";
			this.txtSave.Size = new System.Drawing.Size(272, 20);
			this.txtSave.TabIndex = 1;
			// 
			// cmdConvert
			// 
			this.cmdConvert.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdConvert.Location = new System.Drawing.Point(72, 156);
			this.cmdConvert.Name = "cmdConvert";
			this.cmdConvert.Size = new System.Drawing.Size(80, 22);
			this.cmdConvert.TabIndex = 8;
			this.cmdConvert.Text = "Convert";
			this.cmdConvert.Click += new System.EventHandler(this.CmdConvertClick);
			// 
			// lblType
			// 
			this.lblType.Location = new System.Drawing.Point(328, 30);
			this.lblType.Name = "lblType";
			this.lblType.Size = new System.Drawing.Size(56, 15);
			this.lblType.TabIndex = 17;
			this.lblType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// cmdExist
			// 
			this.cmdExist.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdExist.Location = new System.Drawing.Point(16, 30);
			this.cmdExist.Name = "cmdExist";
			this.cmdExist.Size = new System.Drawing.Size(24, 22);
			this.cmdExist.TabIndex = 6;
			this.cmdExist.Text = "...";
			this.cmdExist.Click += new System.EventHandler(this.CmdExistClick);
			// 
			// lblTest
			// 
			this.lblTest.Location = new System.Drawing.Point(56, 141);
			this.lblTest.Name = "lblTest";
			this.lblTest.Size = new System.Drawing.Size(96, 15);
			this.lblTest.TabIndex = 16;
			// 
			// lblExist
			// 
			this.lblExist.Location = new System.Drawing.Point(48, 15);
			this.lblExist.Name = "lblExist";
			this.lblExist.Size = new System.Drawing.Size(88, 15);
			this.lblExist.TabIndex = 10;
			this.lblExist.Text = "Existing File";
			// 
			// cmdSave
			// 
			this.cmdSave.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cmdSave.Location = new System.Drawing.Point(16, 111);
			this.cmdSave.Name = "cmdSave";
			this.cmdSave.Size = new System.Drawing.Size(24, 23);
			this.cmdSave.TabIndex = 7;
			this.cmdSave.Text = "...";
			this.cmdSave.Click += new System.EventHandler(this.CmdSaveClick);
			// 
			// optImp
			// 
			this.optImp.Checked = true;
			this.optImp.Location = new System.Drawing.Point(112, 56);
			this.optImp.Name = "optImp";
			this.optImp.Size = new System.Drawing.Size(64, 16);
			this.optImp.TabIndex = 18;
			this.optImp.TabStop = true;
			this.optImp.Text = "Imperial";
			this.optImp.Visible = false;
			// 
			// optReb
			// 
			this.optReb.Location = new System.Drawing.Point(184, 56);
			this.optReb.Name = "optReb";
			this.optReb.Size = new System.Drawing.Size(56, 16);
			this.optReb.TabIndex = 18;
			this.optReb.Text = "Rebel";
			this.optReb.Visible = false;
			// 
			// lblPlayer
			// 
			this.lblPlayer.Location = new System.Drawing.Point(48, 56);
			this.lblPlayer.Name = "lblPlayer";
			this.lblPlayer.Size = new System.Drawing.Size(56, 16);
			this.lblPlayer.TabIndex = 19;
			this.lblPlayer.Text = "Player is:";
			this.lblPlayer.Visible = false;
			// 
			// MainForm
			// 
			this.AcceptButton = this.cmdConvert;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cmdExit;
			this.ClientSize = new System.Drawing.Size(392, 204);
			this.Controls.Add(this.lblPlayer);
			this.Controls.Add(this.optImp);
			this.Controls.Add(this.lblType);
			this.Controls.Add(this.lblTest);
			this.Controls.Add(this.chkXWA);
			this.Controls.Add(this.chkXvT2);
			this.Controls.Add(this.lblSave);
			this.Controls.Add(this.lblExist);
			this.Controls.Add(this.cmdExit);
			this.Controls.Add(this.cmdConvert);
			this.Controls.Add(this.cmdSave);
			this.Controls.Add(this.cmdExist);
			this.Controls.Add(this.txtSave);
			this.Controls.Add(this.txtExist);
			this.Controls.Add(this.optReb);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.Text = "IDMR Mission Converter";
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

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
		void ChkXvT2CheckedChanged(object sender, System.EventArgs e)
		{
			if (chkXvT2.Checked == true) { chkXWA.Checked = false;	}
			if (chkXvT2.Checked == false) { chkXWA.Checked = true; }
		}
		
		void ChkXWACheckedChanged(object sender, System.EventArgs e)
		{
			if (chkXWA.Checked == true) { chkXvT2.Checked = false; }
			if (chkXWA.Checked == false) { chkXvT2.Checked = true;	}
		}
		#endregion

		#region buttons
		void CmdExitClick(object sender, System.EventArgs e)
		{
			Application.Exit();
		}
		
		void CmdExistClick(object sender, System.EventArgs e)
		{
			opnExist.ShowDialog();
		}
		
		void CmdSaveClick(object sender, System.EventArgs e)
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
					break;
				case 12:
					lblType.Text = "XvT";
					chkXvT2.Checked = false;
					chkXWA.Checked = true;
					chkXvT2.Enabled = false;
					chkXWA.Enabled = false;
					break;
				case 14:
					lblType.Text = "BoP";
					chkXvT2.Checked = false;
					chkXWA.Checked = true;
					chkXvT2.Enabled = false;
					chkXWA.Enabled = false;
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
		
		void CmdConvertClick(object sender, System.EventArgs e)
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
			XvT.WriteByte(12);			//XvT format
			XvT.Position = 2;
			TIE.Position = 2;
			int i, j;
			for(i = 0;i < 4;i++) XvT.WriteByte(Convert.ToByte(TIE.ReadByte()));			//# of FGs and messages
			TIE.Position = 2;
			int FGs = ReadInt(TIE);			//store them
			int Messages = ReadInt(TIE);
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
				XvT.WriteByte(1);															//250 points
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
				XvT.WriteByte(1);															//250 points
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
			XvT.Position += 31;
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
			TIE.Position -= 257;
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
			for(i = 0;i < 64;i++)
			{
				j = ReadInt(TIE);		//check length..  (read int really just puts TIE in the right spot, will always be <256
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
			XvT.Position += (0xD9A6 + 0x600*FGs);
			XvT.WriteByte(0);
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
			XWA.Position = 2;XWA.WriteByte(Convert.ToByte(FGs + 1));	// +1 because I'm inserting a backdrop
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
			#region Flight Groups
			long XWAPos;
			bool Player = false;
			int Brief = 0;
			for(i = 0;i < FGs;i++)
			{
				long XvTPos = XvT.Position;
				XWAPos = XWA.Position;
				for(j = 0;j < 20;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//name
				XWA.WriteByte(255);														//no Designations, can't convert
				XWA.WriteByte(255);
				XWA.Position += 4;
				XWA.WriteByte(255);
				XWA.Position = XWAPos + 40;
				XvT.Position += 20;
				for(j = 0;j < 40;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//Cargo & Special
				XWA.Position = XWAPos + 105;
				for(j = 0;j < 30;j++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));			//SC ship# -> Arrival Difficulty
				XvT.Position = XvTPos + 100;
				if (XvT.ReadByte() != 0 && Player == false) { Player = true; }				//check for multiple player craft, take the first one
				else
				{
					XWA.Position = XWAPos + 125;
					XWA.WriteByte(0);
				}
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
				XWA.Position = XWAPos + 350;
				XvT.Position = XvTPos + 244;
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
				XWA.Position = XWAPos + 498;
				XvT.Position = XvTPos + 326;
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
				XWA.Position = XWAPos + 646;
				XvT.Position = XvTPos + 408;
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
					XWA.Position = 17762 + (FGs+1) * 3646 + Messages * 162 + Brief * 20;	//place for next insert command
					XWA.WriteByte(26);XWA.Position++;XWA.WriteByte(Convert.ToByte(Brief));XWA.Position++;		//Add command
					fgIcons[i] = Brief;		// store the Icon# for the FG
					XvT.Position = XvTPos + 82;XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.Position++;		//Ship
					XvT.Position = XvTPos + 87;XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.Position += 3;		//IFF
					XvT.Position = XvTPos + 1154;
					XWA.WriteByte(28);XWA.Position++;XWA.WriteByte(Convert.ToByte(Brief));XWA.Position++;		//Move command
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//BP X
					XvT.Position += 42;
					XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//BP Y
					Brief++;
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
			//okay, now write in the default Backdrop
			XWAPos = XWA.Position;
			XWA.WriteByte(49);XWA.WriteByte(46);XWA.WriteByte(48);XWA.WriteByte(32);XWA.WriteByte(49);XWA.WriteByte(46);XWA.WriteByte(48);XWA.WriteByte(32);XWA.WriteByte(49);XWA.WriteByte(46);XWA.WriteByte(48);
			XWA.Position = XWAPos + 20;
			XWA.WriteByte(255);XWA.WriteByte(255);XWA.Position += 4;XWA.WriteByte(255);
			XWA.Position = XWAPos + 40;
			XWA.WriteByte(49);XWA.WriteByte(46);XWA.WriteByte(48);
			XWA.Position = XWAPos + 60;
			XWA.WriteByte(49);XWA.WriteByte(46);XWA.WriteByte(48);
			XWA.Position = XWAPos + 105;XWA.WriteByte(2);
			XWA.Position++;XWA.WriteByte(183);XWA.WriteByte(1);
			XWA.Position += 3;XWA.WriteByte(5);
			XWA.Position = XWAPos + 2827;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);
			XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;XWA.WriteByte(10);XWA.Position += 79;
			XWA.Position = XWAPos + 3466;XWA.WriteByte(160);XWA.Position++;XWA.WriteByte(160);XWA.Position++;XWA.WriteByte(160);XWA.Position++;XWA.WriteByte(1);
			XWA.Position = XWAPos + 3646;
			#endregion
			#region Messages
			for(i = 0;i < Messages;i++)
			{
				XWAPos = XWA.Position;
				long XvTPos = XvT.Position;
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
				XWA.Position = XWAPos + 140;
				XvT.Position = XvTPos + 114;
				XWA.WriteByte(Convert.ToByte(XvT.ReadByte() * 4.2));						//Delay
				XWA.Position += 3; XWA.WriteByte(10);
				XWA.Position += 5; XWA.WriteByte(10);										//make sure the Cancel triggers are set to FALSE
				XWA.Position = XWAPos + 162;
				XvT.Position = XvTPos + 116;
			}
			#endregion
			XWA.WriteByte(3);
			#region Global Goals
			XvT.Position++;
			for(i = 0;i < 5;i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//Prim T1
			ShipFix(XvT, XWA);
			for(i = 0;i < 4;i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//PT2
			ShipFix(XvT, XWA);
			XWA.Position += 2;
			XvT.Position += 2;
			XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));							//PT 1 AND/OR 2
			XWA.Position++;
			for(i = 0;i < 4;i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//PT 3
			ShipFix(XvT, XWA);
			for(i = 0;i < 4;i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//PT 4
			ShipFix(XvT, XWA);
			XvT.Position += 2;
			XWA.Position += 2;
			XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));							//PT 3 AND/OR 4
			XvT.Position += 17;
			XWA.Position += 18;
			for(i = 0;i < 3;i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//PT (1/2) AND/OR (3/4) -> Points
			XWA.Position += 70;
			for(i = 0;i < 4;i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//Prev T1
			ShipFix(XvT, XWA);
			for(i = 0;i < 4;i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//PT2
			ShipFix(XvT, XWA);
			XWA.Position += 2;
			XvT.Position += 2;
			XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));							//PT 1 AND/OR 2
			XWA.Position++;
			for(i = 0;i < 4;i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//PT 3
			ShipFix(XvT, XWA);
			for(i = 0;i < 4;i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//PT 4
			ShipFix(XvT, XWA);
			XvT.Position += 2;
			XWA.Position += 2;
			XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));							//PT 3 AND/OR 4
			XvT.Position += 17;
			XWA.Position += 18;
			for(i = 0;i < 3;i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//PT (1/2) AND/OR (3/4) -> Points
			XWA.Position += 70;
			for(i = 0;i < 4;i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//Sec T1
			ShipFix(XvT, XWA);
			for(i = 0;i < 4;i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//ST2
			ShipFix(XvT, XWA);
			XWA.Position += 2;
			XvT.Position += 2;
			XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));							//ST 1 AND/OR 2
			XWA.Position++;
			for(i = 0;i < 4;i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//ST 3
			ShipFix(XvT, XWA);
			for(i = 0;i < 4;i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//ST 4
			ShipFix(XvT, XWA);
			XvT.Position += 2;
			XWA.Position += 2;
			XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));							//ST 3 AND/OR 4
			XvT.Position += 17;
			XWA.Position += 18;
			for(i = 0;i < 3;i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));		//ST (1/2) AND/OR (3/4) -> Points
			XWA.Position += 70;
			for(j = 0;j < 9;j++)
			{
				XWA.WriteByte(3);
				XWA.Position += 367;
			}
			#endregion
			XvT.Position += 1152;
			#region IFF/Teams
			for(i = 0;i < 4870;i++) XWA.WriteByte(Convert.ToByte(XvT.ReadByte()));	//well, that was simple..
			#endregion
			#region Briefing
			XWAPos = XWA.Position;
			for(i = 0;i < 6;i++) { XWA.WriteByte(Convert.ToByte(XvT.ReadByte())); }	//briefing intro
			WriteInt(XWA, ReadInt(XvT) + 10*Brief);	// adjust length for add/moves
			XvT.Position += 2;		
			XWA.Position += 20 * Brief + 2;
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
			XWA.Position = 0x8960 + (FGs+1)*0xE3E + Messages*0xA2;
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
			#endregion
			XWA.Position += 50708; 				//pass over Briefing 2, bunch of unknown space, message comments
			XvT.Position += 6636;
			#region FG Goal strings
			for(i = 0;i < FGs;i++)
			{
				for(j = 0;j < 24;j++)
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
			XWA.Position += 24;						//compensate for adding the Backdrop
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
	}
}
