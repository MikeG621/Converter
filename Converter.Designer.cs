/*
 * Convert.exe, Converts mission formats between TIE, XvT and XWA
 * Copyright (C) 2005-2018 Michael Gaisser (mjgaisser@gmail.com)
 * Licensed under the MPL v2.0 or later
 * 
 * VERSION: 1.5
 */
 
namespace Idmr.Converter
{
	partial class MainForm
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
		private System.Windows.Forms.RadioButton optImp;
		private System.Windows.Forms.RadioButton optReb;
		private System.Windows.Forms.Label lblPlayer;
		/// <summary>Required designer variable.</summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.CheckBox chkXvtCombat;
		private System.Windows.Forms.CheckBox chkXvtBop;

		/// <summary>Clean up any resources being used.</summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
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
			this.chkXvtCombat = new System.Windows.Forms.CheckBox();
			this.chkXvtBop = new System.Windows.Forms.CheckBox();
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
			this.cmdExit.Location = new System.Drawing.Point(235, 160);
			this.cmdExit.Name = "cmdExit";
			this.cmdExit.Size = new System.Drawing.Size(80, 22);
			this.cmdExit.TabIndex = 9;
			this.cmdExit.Text = "Exit";
			this.cmdExit.Click += new System.EventHandler(this.cmdExitClick);
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
			this.opnExist.FileOk += new System.ComponentModel.CancelEventHandler(this.opnExistFileOk);
			// 
			// savConvert
			// 
			this.savConvert.DefaultExt = "tie";
			this.savConvert.Filter = "Mission Files|*.tie";
			this.savConvert.Title = "Save as...";
			this.savConvert.FileOk += new System.ComponentModel.CancelEventHandler(this.savConvertFileOk);
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
			this.chkXvT2.CheckedChanged += new System.EventHandler(this.chkXvT2CheckedChanged);
			// 
			// chkXWA
			// 
			this.chkXWA.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkXWA.Location = new System.Drawing.Point(328, 126);
			this.chkXWA.Name = "chkXWA";
			this.chkXWA.Size = new System.Drawing.Size(56, 15);
			this.chkXWA.TabIndex = 15;
			this.chkXWA.Text = "XWA";
			this.chkXWA.CheckedChanged += new System.EventHandler(this.chkXWACheckedChanged);
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
			this.cmdConvert.Location = new System.Drawing.Point(72, 160);
			this.cmdConvert.Name = "cmdConvert";
			this.cmdConvert.Size = new System.Drawing.Size(80, 22);
			this.cmdConvert.TabIndex = 8;
			this.cmdConvert.Text = "Convert";
			this.cmdConvert.Click += new System.EventHandler(this.cmdConvertClick);
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
			this.cmdExist.Click += new System.EventHandler(this.cmdExistClick);
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
			this.cmdSave.Click += new System.EventHandler(this.cmdSaveClick);
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
			// chkXvtCombat
			// 
			this.chkXvtCombat.AutoSize = true;
			this.chkXvtCombat.Location = new System.Drawing.Point(48, 141);
			this.chkXvtCombat.Name = "chkXvtCombat";
			this.chkXvtCombat.Size = new System.Drawing.Size(267, 17);
			this.chkXvtCombat.TabIndex = 20;
			this.chkXvtCombat.Text = "Convert to skirmish, multiplayer slots + second team";
			this.chkXvtCombat.UseVisualStyleBackColor = true;
			this.chkXvtCombat.Visible = false;
			// 
			// chkXvtBop
			// 
			this.chkXvtBop.AutoSize = true;
			this.chkXvtBop.Checked = true;
			this.chkXvtBop.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkXvtBop.Location = new System.Drawing.Point(96, 141);
			this.chkXvtBop.Name = "chkXvtBop";
			this.chkXvtBop.Size = new System.Drawing.Size(188, 17);
			this.chkXvtBop.TabIndex = 21;
			this.chkXvtBop.Text = "Save as BoP (longer descriptions).";
			this.chkXvtBop.UseVisualStyleBackColor = true;
			this.chkXvtBop.Visible = false;
			// 
			// MainForm
			// 
			this.AcceptButton = this.cmdConvert;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cmdExit;
			this.ClientSize = new System.Drawing.Size(392, 204);
			this.Controls.Add(this.chkXvtBop);
			this.Controls.Add(this.chkXvtCombat);
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
	}
}
