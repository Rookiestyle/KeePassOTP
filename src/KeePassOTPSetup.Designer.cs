using System;
using System.ComponentModel;

namespace KeePassOTP
{
	partial class KeePassOTPSetup
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.GroupBox gSeed;
		internal System.Windows.Forms.TextBox tbOTPSeed;
		private System.Windows.Forms.Label lSeed;
		internal System.Windows.Forms.ComboBox cbOTPHashFunc;
		internal System.Windows.Forms.ComboBox cbOTPLength;
		internal System.Windows.Forms.ComboBox cbOTPType;
		private System.Windows.Forms.Label lHash;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.GroupBox gOTP;
		private System.Windows.Forms.Label lLength;
		private System.Windows.Forms.Label lType;
		internal System.Windows.Forms.TextBox tbTOTPTimestep;
		private System.Windows.Forms.Label lTimestep;
		internal System.Windows.Forms.TextBox tbHOTPCounter;
		private System.Windows.Forms.Label lCounter;
		private System.Windows.Forms.GroupBox gTime;
		private System.Windows.Forms.Label lTimeType;
		private System.Windows.Forms.ComboBox totpTimeCorrectionType;
		internal System.Windows.Forms.TextBox tbTOTPTimeCorrectionURL;
		private System.Windows.Forms.Label lURL;
		private System.Windows.Forms.Label totpTimeCorrectionValue;
		private System.Windows.Forms.Label lTime;

		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				m_timer.Stop();
				m_timer.Dispose();
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		private void InitializeComponent()
		{
			this.gSeed = new System.Windows.Forms.GroupBox();
			this.pbTOTPLifetime = new System.Windows.Forms.ProgressBar();
			this.otpPreviewNext = new System.Windows.Forms.Label();
			this.otpPreview = new System.Windows.Forms.Label();
			this.tbOTPSeed = new System.Windows.Forms.TextBox();
			this.lSeed = new System.Windows.Forms.Label();
			this.gOTP = new System.Windows.Forms.GroupBox();
			this.cbOTPFormat = new System.Windows.Forms.ComboBox();
			this.lFormat = new System.Windows.Forms.Label();
			this.tbTOTPTimestep = new System.Windows.Forms.TextBox();
			this.lTimestep = new System.Windows.Forms.Label();
			this.tbHOTPCounter = new System.Windows.Forms.TextBox();
			this.lCounter = new System.Windows.Forms.Label();
			this.cbOTPHashFunc = new System.Windows.Forms.ComboBox();
			this.cbOTPLength = new System.Windows.Forms.ComboBox();
			this.cbOTPType = new System.Windows.Forms.ComboBox();
			this.lHash = new System.Windows.Forms.Label();
			this.lLength = new System.Windows.Forms.Label();
			this.lType = new System.Windows.Forms.Label();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.gTime = new System.Windows.Forms.GroupBox();
			this.totpTimeCorrectionValue = new System.Windows.Forms.Label();
			this.lTime = new System.Windows.Forms.Label();
			this.lTimeType = new System.Windows.Forms.Label();
			this.totpTimeCorrectionType = new System.Windows.Forms.ComboBox();
			this.tbTOTPTimeCorrectionURL = new System.Windows.Forms.TextBox();
			this.lURL = new System.Windows.Forms.Label();
			this.cbAdvanced = new System.Windows.Forms.CheckBox();
			this.pbQR = new System.Windows.Forms.PictureBox();
			this.gSeed.SuspendLayout();
			this.gOTP.SuspendLayout();
			this.gTime.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbQR)).BeginInit();
			this.SuspendLayout();
			// 
			// gSeed
			// 
			this.gSeed.Controls.Add(this.pbQR);
			this.gSeed.Controls.Add(this.pbTOTPLifetime);
			this.gSeed.Controls.Add(this.otpPreviewNext);
			this.gSeed.Controls.Add(this.otpPreview);
			this.gSeed.Controls.Add(this.tbOTPSeed);
			this.gSeed.Controls.Add(this.lSeed);
			this.gSeed.Dock = System.Windows.Forms.DockStyle.Top;
			this.gSeed.Location = new System.Drawing.Point(5, 5);
			this.gSeed.Name = "gSeed";
			this.gSeed.Size = new System.Drawing.Size(528, 146);
			this.gSeed.TabIndex = 2;
			this.gSeed.TabStop = false;
			this.gSeed.Text = "Seed data";
			// 
			// pbTOTPLifetime
			// 
			this.pbTOTPLifetime.Location = new System.Drawing.Point(98, 86);
			this.pbTOTPLifetime.Name = "pbTOTPLifetime";
			this.pbTOTPLifetime.Size = new System.Drawing.Size(400, 2);
			this.pbTOTPLifetime.Step = 1;
			this.pbTOTPLifetime.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.pbTOTPLifetime.TabIndex = 13;
			this.pbTOTPLifetime.Value = 100;
			// 
			// otpPreviewNext
			// 
			this.otpPreviewNext.AutoSize = true;
			this.otpPreviewNext.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.otpPreviewNext.Location = new System.Drawing.Point(94, 99);
			this.otpPreviewNext.Name = "otpPreviewNext";
			this.otpPreviewNext.Size = new System.Drawing.Size(38, 20);
			this.otpPreviewNext.TabIndex = 12;
			this.otpPreviewNext.Text = "N/A";
			// 
			// otpPreview
			// 
			this.otpPreview.AutoSize = true;
			this.otpPreview.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.otpPreview.ForeColor = System.Drawing.SystemColors.ControlText;
			this.otpPreview.Location = new System.Drawing.Point(95, 58);
			this.otpPreview.Name = "otpPreview";
			this.otpPreview.Size = new System.Drawing.Size(52, 26);
			this.otpPreview.TabIndex = 11;
			this.otpPreview.Text = "N/A";
			// 
			// tbOTPSeed
			// 
			this.tbOTPSeed.Location = new System.Drawing.Point(100, 26);
			this.tbOTPSeed.Name = "tbOTPSeed";
			this.tbOTPSeed.Size = new System.Drawing.Size(400, 26);
			this.tbOTPSeed.TabIndex = 1;
			this.tbOTPSeed.Leave += new System.EventHandler(this.OnValueChanged);
			// 
			// lSeed
			// 
			this.lSeed.Location = new System.Drawing.Point(6, 30);
			this.lSeed.Name = "lSeed";
			this.lSeed.Size = new System.Drawing.Size(90, 20);
			this.lSeed.TabIndex = 4;
			this.lSeed.Text = "Seed:";
			// 
			// gOTP
			// 
			this.gOTP.Controls.Add(this.cbOTPFormat);
			this.gOTP.Controls.Add(this.lFormat);
			this.gOTP.Controls.Add(this.tbTOTPTimestep);
			this.gOTP.Controls.Add(this.lTimestep);
			this.gOTP.Controls.Add(this.tbHOTPCounter);
			this.gOTP.Controls.Add(this.lCounter);
			this.gOTP.Controls.Add(this.cbOTPHashFunc);
			this.gOTP.Controls.Add(this.cbOTPLength);
			this.gOTP.Controls.Add(this.cbOTPType);
			this.gOTP.Controls.Add(this.lHash);
			this.gOTP.Controls.Add(this.lLength);
			this.gOTP.Controls.Add(this.lType);
			this.gOTP.Dock = System.Windows.Forms.DockStyle.Top;
			this.gOTP.Location = new System.Drawing.Point(5, 175);
			this.gOTP.Name = "gOTP";
			this.gOTP.Size = new System.Drawing.Size(528, 176);
			this.gOTP.TabIndex = 3;
			this.gOTP.TabStop = false;
			this.gOTP.Text = "OTP settings";
			// 
			// cbOTPFormat
			// 
			this.cbOTPFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbOTPFormat.FormattingEnabled = true;
			this.cbOTPFormat.Items.AddRange(new object[] {
            "BASE32",
            "BASE64",
            "HEX",
            "UTF8"});
			this.cbOTPFormat.Location = new System.Drawing.Point(100, 20);
			this.cbOTPFormat.Name = "cbOTPFormat";
			this.cbOTPFormat.Size = new System.Drawing.Size(121, 28);
			this.cbOTPFormat.TabIndex = 8;
			// 
			// lFormat
			// 
			this.lFormat.Location = new System.Drawing.Point(8, 26);
			this.lFormat.Name = "lFormat";
			this.lFormat.Size = new System.Drawing.Size(90, 23);
			this.lFormat.TabIndex = 9;
			this.lFormat.Text = "Format:";
			// 
			// tbTOTPTimestep
			// 
			this.tbTOTPTimestep.Location = new System.Drawing.Point(380, 58);
			this.tbTOTPTimestep.Name = "tbTOTPTimestep";
			this.tbTOTPTimestep.Size = new System.Drawing.Size(100, 26);
			this.tbTOTPTimestep.TabIndex = 4;
			this.tbTOTPTimestep.Leave += new System.EventHandler(this.OnValueChanged);
			// 
			// lTimestep
			// 
			this.lTimestep.Location = new System.Drawing.Point(250, 62);
			this.lTimestep.Name = "lTimestep";
			this.lTimestep.Size = new System.Drawing.Size(120, 23);
			this.lTimestep.TabIndex = 7;
			this.lTimestep.Text = "Timestep:";
			// 
			// tbHOTPCounter
			// 
			this.tbHOTPCounter.Location = new System.Drawing.Point(380, 58);
			this.tbHOTPCounter.Name = "tbHOTPCounter";
			this.tbHOTPCounter.Size = new System.Drawing.Size(100, 26);
			this.tbHOTPCounter.TabIndex = 5;
			this.tbHOTPCounter.TextChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// lCounter
			// 
			this.lCounter.Location = new System.Drawing.Point(250, 62);
			this.lCounter.Name = "lCounter";
			this.lCounter.Size = new System.Drawing.Size(120, 23);
			this.lCounter.TabIndex = 7;
			this.lCounter.Text = "counter:";
			// 
			// cbOTPHashFunc
			// 
			this.cbOTPHashFunc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbOTPHashFunc.FormattingEnabled = true;
			this.cbOTPHashFunc.Items.AddRange(new object[] {
            "SHA1",
            "SHA256",
            "SHA512"});
			this.cbOTPHashFunc.Location = new System.Drawing.Point(100, 134);
			this.cbOTPHashFunc.Name = "cbOTPHashFunc";
			this.cbOTPHashFunc.Size = new System.Drawing.Size(121, 28);
			this.cbOTPHashFunc.TabIndex = 7;
			this.cbOTPHashFunc.SelectedIndexChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// cbOTPLength
			// 
			this.cbOTPLength.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbOTPLength.FormattingEnabled = true;
			this.cbOTPLength.Items.AddRange(new object[] {
            "6",
            "7",
            "8",
            "9",
            "10"});
			this.cbOTPLength.Location = new System.Drawing.Point(100, 96);
			this.cbOTPLength.Name = "cbOTPLength";
			this.cbOTPLength.Size = new System.Drawing.Size(121, 28);
			this.cbOTPLength.TabIndex = 6;
			this.cbOTPLength.SelectedIndexChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// cbOTPType
			// 
			this.cbOTPType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbOTPType.FormattingEnabled = true;
			this.cbOTPType.Items.AddRange(new object[] {
            "HOTP",
            "TOTP"});
			this.cbOTPType.Location = new System.Drawing.Point(100, 58);
			this.cbOTPType.Name = "cbOTPType";
			this.cbOTPType.Size = new System.Drawing.Size(121, 28);
			this.cbOTPType.TabIndex = 3;
			this.cbOTPType.SelectedIndexChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// lHash
			// 
			this.lHash.Location = new System.Drawing.Point(6, 136);
			this.lHash.Name = "lHash";
			this.lHash.Size = new System.Drawing.Size(90, 23);
			this.lHash.TabIndex = 2;
			this.lHash.Text = "OTP hash:";
			// 
			// lLength
			// 
			this.lLength.Location = new System.Drawing.Point(6, 98);
			this.lLength.Name = "lLength";
			this.lLength.Size = new System.Drawing.Size(90, 23);
			this.lLength.TabIndex = 1;
			this.lLength.Text = "OTP length:";
			// 
			// lType
			// 
			this.lType.Location = new System.Drawing.Point(6, 62);
			this.lType.Name = "lType";
			this.lType.Size = new System.Drawing.Size(90, 23);
			this.lType.TabIndex = 0;
			this.lType.Text = "OTP type:";
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(316, 486);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(100, 30);
			this.buttonOK.TabIndex = 10;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(433, 486);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(100, 30);
			this.buttonCancel.TabIndex = 11;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// gTime
			// 
			this.gTime.Controls.Add(this.totpTimeCorrectionValue);
			this.gTime.Controls.Add(this.lTime);
			this.gTime.Controls.Add(this.lTimeType);
			this.gTime.Controls.Add(this.totpTimeCorrectionType);
			this.gTime.Controls.Add(this.tbTOTPTimeCorrectionURL);
			this.gTime.Controls.Add(this.lURL);
			this.gTime.Dock = System.Windows.Forms.DockStyle.Top;
			this.gTime.Location = new System.Drawing.Point(5, 351);
			this.gTime.Name = "gTime";
			this.gTime.Size = new System.Drawing.Size(528, 120);
			this.gTime.TabIndex = 10;
			this.gTime.TabStop = false;
			this.gTime.Text = "Time correction - TOTP only";
			// 
			// totpTimeCorrectionValue
			// 
			this.totpTimeCorrectionValue.Location = new System.Drawing.Point(98, 93);
			this.totpTimeCorrectionValue.Name = "totpTimeCorrectionValue";
			this.totpTimeCorrectionValue.Size = new System.Drawing.Size(90, 20);
			this.totpTimeCorrectionValue.TabIndex = 11;
			this.totpTimeCorrectionValue.Text = "N/A";
			// 
			// lTime
			// 
			this.lTime.Location = new System.Drawing.Point(6, 93);
			this.lTime.Name = "lTime";
			this.lTime.Size = new System.Drawing.Size(90, 20);
			this.lTime.TabIndex = 10;
			this.lTime.Text = "Time correction:";
			// 
			// lTimeType
			// 
			this.lTimeType.AutoSize = true;
			this.lTimeType.Location = new System.Drawing.Point(6, 34);
			this.lTimeType.Name = "lTimeType";
			this.lTimeType.Size = new System.Drawing.Size(47, 20);
			this.lTimeType.TabIndex = 8;
			this.lTimeType.Text = "Type:";
			// 
			// totpTimeCorrectionType
			// 
			this.totpTimeCorrectionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.totpTimeCorrectionType.FormattingEnabled = true;
			this.totpTimeCorrectionType.Location = new System.Drawing.Point(100, 29);
			this.totpTimeCorrectionType.Name = "totpTimeCorrectionType";
			this.totpTimeCorrectionType.Size = new System.Drawing.Size(400, 28);
			this.totpTimeCorrectionType.TabIndex = 8;
			this.totpTimeCorrectionType.SelectedIndexChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// tbTOTPTimeCorrectionURL
			// 
			this.tbTOTPTimeCorrectionURL.Location = new System.Drawing.Point(100, 63);
			this.tbTOTPTimeCorrectionURL.Name = "tbTOTPTimeCorrectionURL";
			this.tbTOTPTimeCorrectionURL.Size = new System.Drawing.Size(400, 26);
			this.tbTOTPTimeCorrectionURL.TabIndex = 9;
			// 
			// lURL
			// 
			this.lURL.Location = new System.Drawing.Point(6, 66);
			this.lURL.Name = "lURL";
			this.lURL.Size = new System.Drawing.Size(90, 23);
			this.lURL.TabIndex = 6;
			this.lURL.Text = "URL:";
			// 
			// cbAdvanced
			// 
			this.cbAdvanced.AutoSize = true;
			this.cbAdvanced.Checked = true;
			this.cbAdvanced.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbAdvanced.Dock = System.Windows.Forms.DockStyle.Top;
			this.cbAdvanced.Location = new System.Drawing.Point(5, 151);
			this.cbAdvanced.Name = "cbAdvanced";
			this.cbAdvanced.Size = new System.Drawing.Size(528, 24);
			this.cbAdvanced.TabIndex = 11;
			this.cbAdvanced.Text = "cbAdvanced";
			this.cbAdvanced.UseVisualStyleBackColor = true;
			this.cbAdvanced.CheckedChanged += new System.EventHandler(this.cbAdvanced_CheckedChanged);
			// 
			// pbQR
			// 
			this.pbQR.Image = global::KeePassOTP.Properties.Resources.qr_code;
			this.pbQR.Location = new System.Drawing.Point(8, 53);
			this.pbQR.Name = "pbQR";
			this.pbQR.Size = new System.Drawing.Size(80, 80);
			this.pbQR.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pbQR.TabIndex = 14;
			this.pbQR.TabStop = false;
			this.pbQR.DragDrop += new System.Windows.Forms.DragEventHandler(this.pbQR_DragDrop);
			this.pbQR.DragEnter += new System.Windows.Forms.DragEventHandler(this.pbQR_DragEnter);
			// 
			// KeePassOTPSetup
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(538, 525);
			this.Controls.Add(this.gTime);
			this.Controls.Add(this.gOTP);
			this.Controls.Add(this.cbAdvanced);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.gSeed);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "KeePassOTPSetup";
			this.Padding = new System.Windows.Forms.Padding(5);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "KeePassOTPSetup";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
			this.Shown += new System.EventHandler(this.KeePassOTPSetup_Shown);
			this.gSeed.ResumeLayout(false);
			this.gSeed.PerformLayout();
			this.gOTP.ResumeLayout(false);
			this.gOTP.PerformLayout();
			this.gTime.ResumeLayout(false);
			this.gTime.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbQR)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private System.Windows.Forms.ProgressBar pbTOTPLifetime;
		private System.Windows.Forms.Label otpPreviewNext;
		private System.Windows.Forms.Label otpPreview;
		internal System.Windows.Forms.ComboBox cbOTPFormat;
		private System.Windows.Forms.Label lFormat;
		private System.Windows.Forms.CheckBox cbAdvanced;
		private System.Windows.Forms.PictureBox pbQR;
	}
}
