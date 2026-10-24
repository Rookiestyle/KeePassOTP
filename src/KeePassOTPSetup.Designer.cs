using System;
using System.ComponentModel;
using RookieUI;

namespace KeePassOTP
{
  partial class KeePassOTPSetup
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Button buttonOK;
    private System.Windows.Forms.Button buttonCancel;

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
      this.components = new System.ComponentModel.Container();
      this.buttonOK = new System.Windows.Forms.Button();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.tcSetup = new System.Windows.Forms.TabControl();
      this.tpSetup = new System.Windows.Forms.TabPage();
      this.pbOTPPreview = new System.Windows.Forms.PictureBox();
      this.cbShowInTray = new System.Windows.Forms.CheckBox();
      this.btnReadScreen = new System.Windows.Forms.Button();
      this.btnDragDropQR = new System.Windows.Forms.Button();
      this.pbTOTPLifetime = new System.Windows.Forms.ProgressBar();
      this.otpPreviewNext = new System.Windows.Forms.Label();
      this.otpPreview = new System.Windows.Forms.Label();
      this.tbOTPSeed = new System.Windows.Forms.TextBox();
      this.lSeed = new System.Windows.Forms.Label();
      this.tpAdvanced = new System.Windows.Forms.TabPage();
      this.gTime = new CollapsibleGroupBox();
      this.totpTimeCorrectionValue = new System.Windows.Forms.Label();
      this.lTime = new System.Windows.Forms.Label();
      this.lTimeType = new System.Windows.Forms.Label();
      this.totpTimeCorrectionType = new System.Windows.Forms.ComboBox();
      this.tbTOTPTimeCorrectionURL = new System.Windows.Forms.TextBox();
      this.lURL = new System.Windows.Forms.Label();
      this.gIssuerLabel = new CollapsibleGroupBox();
      this.bLabelFromEntry = new System.Windows.Forms.Button();
      this.bIssuerFromEntry = new System.Windows.Forms.Button();
      this.tbLabel = new System.Windows.Forms.TextBox();
      this.tbIssuer = new System.Windows.Forms.TextBox();
      this.lLabel = new System.Windows.Forms.Label();
      this.lIssuer = new System.Windows.Forms.Label();
      this.gOTP = new CollapsibleGroupBox();
      this.lYandexPin = new System.Windows.Forms.Label();
      this.tbYandexPin = new System.Windows.Forms.TextBox();
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
      this.gRecovery = new CollapsibleGroupBox();
      this.tbRecovery = new System.Windows.Forms.TextBox();
      this.directoryEntry1 = new System.DirectoryServices.DirectoryEntry();
      this.tcSetup.SuspendLayout();
      this.tpSetup.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pbOTPPreview)).BeginInit();
      this.tpAdvanced.SuspendLayout();
      this.gTime.SuspendLayout();
      this.gIssuerLabel.SuspendLayout();
      this.gOTP.SuspendLayout();
      this.gRecovery.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonOK
      // 
      this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.buttonOK.Location = new System.Drawing.Point(303, 581);
      this.buttonOK.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(89, 24);
      this.buttonOK.TabIndex = 1;
      this.buttonOK.Text = "OK";
      this.buttonOK.UseVisualStyleBackColor = true;
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(407, 581);
      this.buttonCancel.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(89, 24);
      this.buttonCancel.TabIndex = 2;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // tcSetup
      // 
      this.tcSetup.Controls.Add(this.tpSetup);
      this.tcSetup.Controls.Add(this.tpAdvanced);
      this.tcSetup.Dock = System.Windows.Forms.DockStyle.Top;
      this.tcSetup.Location = new System.Drawing.Point(4, 4);
      this.tcSetup.Margin = new System.Windows.Forms.Padding(2);
      this.tcSetup.Name = "tcSetup";
      this.tcSetup.SelectedIndex = 0;
      this.tcSetup.Size = new System.Drawing.Size(492, 521);
      this.tcSetup.TabIndex = 1;
      // 
      // tpSetup
      // 
      this.tpSetup.Controls.Add(this.pbOTPPreview);
      this.tpSetup.Controls.Add(this.cbShowInTray);
      this.tpSetup.Controls.Add(this.btnReadScreen);
      this.tpSetup.Controls.Add(this.btnDragDropQR);
      this.tpSetup.Controls.Add(this.pbTOTPLifetime);
      this.tpSetup.Controls.Add(this.otpPreviewNext);
      this.tpSetup.Controls.Add(this.otpPreview);
      this.tpSetup.Controls.Add(this.tbOTPSeed);
      this.tpSetup.Controls.Add(this.lSeed);
      this.tpSetup.Location = new System.Drawing.Point(4, 25);
      this.tpSetup.Margin = new System.Windows.Forms.Padding(2);
      this.tpSetup.Name = "tpSetup";
      this.tpSetup.Padding = new System.Windows.Forms.Padding(2);
      this.tpSetup.Size = new System.Drawing.Size(484, 492);
      this.tpSetup.TabIndex = 0;
      this.tpSetup.Text = "tabPage1";
      this.tpSetup.UseVisualStyleBackColor = true;
      // 
      // pbOTPPreview
      // 
      this.pbOTPPreview.ErrorImage = null;
      this.pbOTPPreview.InitialImage = null;
      this.pbOTPPreview.Location = new System.Drawing.Point(18, 19);
      this.pbOTPPreview.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.pbOTPPreview.Name = "pbOTPPreview";
      this.pbOTPPreview.Size = new System.Drawing.Size(62, 64);
      this.pbOTPPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
      this.pbOTPPreview.TabIndex = 924;
      this.pbOTPPreview.TabStop = false;
      // 
      // cbShowInTray
      // 
      this.cbShowInTray.AutoSize = true;
      this.cbShowInTray.Location = new System.Drawing.Point(18, 101);
      this.cbShowInTray.Name = "cbShowInTray";
      this.cbShowInTray.Size = new System.Drawing.Size(115, 20);
      this.cbShowInTray.TabIndex = 110;
      this.cbShowInTray.Text = "cbShowInTray";
      this.cbShowInTray.UseVisualStyleBackColor = true;
      // 
      // btnReadScreen
      // 
      this.btnReadScreen.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnReadScreen.Location = new System.Drawing.Point(69, 151);
      this.btnReadScreen.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.btnReadScreen.Name = "btnReadScreen";
      this.btnReadScreen.Size = new System.Drawing.Size(120, 120);
      this.btnReadScreen.TabIndex = 120;
      this.btnReadScreen.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
      this.btnReadScreen.SizeChanged += new System.EventHandler(this.btnReadScreen_SizeChanged);
      this.btnReadScreen.Click += new System.EventHandler(this.bSearchScreen_Click);
      // 
      // btnDragDropQR
      // 
      this.btnDragDropQR.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnDragDropQR.Location = new System.Drawing.Point(265, 151);
      this.btnDragDropQR.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.btnDragDropQR.Name = "btnDragDropQR";
      this.btnDragDropQR.Size = new System.Drawing.Size(120, 120);
      this.btnDragDropQR.TabIndex = 130;
      this.btnDragDropQR.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
      this.btnDragDropQR.UseMnemonic = false;
      this.btnDragDropQR.Click += new System.EventHandler(this.btnScanQr_Click);
      // 
      // pbTOTPLifetime
      // 
      this.pbTOTPLifetime.Location = new System.Drawing.Point(95, 54);
      this.pbTOTPLifetime.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.pbTOTPLifetime.Name = "pbTOTPLifetime";
      this.pbTOTPLifetime.Size = new System.Drawing.Size(290, 2);
      this.pbTOTPLifetime.Step = 1;
      this.pbTOTPLifetime.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
      this.pbTOTPLifetime.TabIndex = 916;
      this.pbTOTPLifetime.Value = 100;
      // 
      // otpPreviewNext
      // 
      this.otpPreviewNext.AutoSize = true;
      this.otpPreviewNext.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.otpPreviewNext.Location = new System.Drawing.Point(93, 66);
      this.otpPreviewNext.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.otpPreviewNext.Name = "otpPreviewNext";
      this.otpPreviewNext.Size = new System.Drawing.Size(34, 17);
      this.otpPreviewNext.TabIndex = 915;
      this.otpPreviewNext.Text = "N/A";
      // 
      // otpPreview
      // 
      this.otpPreview.AutoSize = true;
      this.otpPreview.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.otpPreview.ForeColor = System.Drawing.SystemColors.ControlText;
      this.otpPreview.Location = new System.Drawing.Point(90, 25);
      this.otpPreview.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.otpPreview.Name = "otpPreview";
      this.otpPreview.Size = new System.Drawing.Size(63, 31);
      this.otpPreview.TabIndex = 914;
      this.otpPreview.Text = "N/A";
      // 
      // tbOTPSeed
      // 
      this.tbOTPSeed.Location = new System.Drawing.Point(18, 315);
      this.tbOTPSeed.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.tbOTPSeed.Name = "tbOTPSeed";
      this.tbOTPSeed.Size = new System.Drawing.Size(450, 22);
      this.tbOTPSeed.TabIndex = 140;
      // 
      // lSeed
      // 
      this.lSeed.AutoSize = true;
      this.lSeed.Location = new System.Drawing.Point(15, 296);
      this.lSeed.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.lSeed.Name = "lSeed";
      this.lSeed.Size = new System.Drawing.Size(43, 16);
      this.lSeed.TabIndex = 913;
      this.lSeed.Text = "Seed:";
      this.lSeed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // tpAdvanced
      // 
      this.tpAdvanced.Controls.Add(this.gTime);
      this.tpAdvanced.Controls.Add(this.gIssuerLabel);
      this.tpAdvanced.Controls.Add(this.gOTP);
      this.tpAdvanced.Controls.Add(this.gRecovery);
      this.tpAdvanced.Location = new System.Drawing.Point(4, 25);
      this.tpAdvanced.Margin = new System.Windows.Forms.Padding(2);
      this.tpAdvanced.Name = "tpAdvanced";
      this.tpAdvanced.Padding = new System.Windows.Forms.Padding(2);
      this.tpAdvanced.Size = new System.Drawing.Size(484, 492);
      this.tpAdvanced.TabIndex = 2;
      this.tpAdvanced.Text = "tpAdvanced";
      this.tpAdvanced.UseVisualStyleBackColor = true;
      // 
      // gTime
      // 
      this.gTime.Collapsed = false;
      this.gTime.Controls.Add(this.totpTimeCorrectionValue);
      this.gTime.Controls.Add(this.lTime);
      this.gTime.Controls.Add(this.lTimeType);
      this.gTime.Controls.Add(this.totpTimeCorrectionType);
      this.gTime.Controls.Add(this.tbTOTPTimeCorrectionURL);
      this.gTime.Controls.Add(this.lURL);
      this.gTime.Dock = System.Windows.Forms.DockStyle.Top;
      this.gTime.Location = new System.Drawing.Point(2, 333);
      this.gTime.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.gTime.Name = "gTime";
      this.gTime.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.gTime.Size = new System.Drawing.Size(480, 100);
      this.gTime.TabIndex = 907;
      this.gTime.TabStop = false;
      this.gTime.Text = "Time correction - TOTP && Steam only";
      // 
      // totpTimeCorrectionValue
      // 
      this.totpTimeCorrectionValue.Location = new System.Drawing.Point(87, 78);
      this.totpTimeCorrectionValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.totpTimeCorrectionValue.Name = "totpTimeCorrectionValue";
      this.totpTimeCorrectionValue.Size = new System.Drawing.Size(80, 16);
      this.totpTimeCorrectionValue.TabIndex = 11;
      this.totpTimeCorrectionValue.Text = "N/A";
      // 
      // lTime
      // 
      this.lTime.Location = new System.Drawing.Point(6, 78);
      this.lTime.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.lTime.Name = "lTime";
      this.lTime.Size = new System.Drawing.Size(80, 16);
      this.lTime.TabIndex = 10;
      this.lTime.Text = "Time correction:";
      // 
      // lTimeType
      // 
      this.lTimeType.AutoSize = true;
      this.lTimeType.Location = new System.Drawing.Point(6, 27);
      this.lTimeType.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.lTimeType.Name = "lTimeType";
      this.lTimeType.Size = new System.Drawing.Size(42, 16);
      this.lTimeType.TabIndex = 8;
      this.lTimeType.Text = "Type:";
      // 
      // totpTimeCorrectionType
      // 
      this.totpTimeCorrectionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.totpTimeCorrectionType.FormattingEnabled = true;
      this.totpTimeCorrectionType.Location = new System.Drawing.Point(89, 23);
      this.totpTimeCorrectionType.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.totpTimeCorrectionType.Name = "totpTimeCorrectionType";
      this.totpTimeCorrectionType.Size = new System.Drawing.Size(356, 24);
      this.totpTimeCorrectionType.TabIndex = 240;
      // 
      // tbTOTPTimeCorrectionURL
      // 
      this.tbTOTPTimeCorrectionURL.Location = new System.Drawing.Point(89, 51);
      this.tbTOTPTimeCorrectionURL.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.tbTOTPTimeCorrectionURL.Name = "tbTOTPTimeCorrectionURL";
      this.tbTOTPTimeCorrectionURL.Size = new System.Drawing.Size(356, 22);
      this.tbTOTPTimeCorrectionURL.TabIndex = 241;
      // 
      // lURL
      // 
      this.lURL.Location = new System.Drawing.Point(6, 53);
      this.lURL.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.lURL.Name = "lURL";
      this.lURL.Size = new System.Drawing.Size(80, 19);
      this.lURL.TabIndex = 6;
      this.lURL.Text = "URL:";
      // 
      // gIssuerLabel
      // 
      this.gIssuerLabel.Collapsed = false;
      this.gIssuerLabel.Controls.Add(this.bLabelFromEntry);
      this.gIssuerLabel.Controls.Add(this.bIssuerFromEntry);
      this.gIssuerLabel.Controls.Add(this.tbLabel);
      this.gIssuerLabel.Controls.Add(this.tbIssuer);
      this.gIssuerLabel.Controls.Add(this.lLabel);
      this.gIssuerLabel.Controls.Add(this.lIssuer);
      this.gIssuerLabel.Dock = System.Windows.Forms.DockStyle.Top;
      this.gIssuerLabel.Location = new System.Drawing.Point(2, 243);
      this.gIssuerLabel.Margin = new System.Windows.Forms.Padding(2);
      this.gIssuerLabel.Name = "gIssuerLabel";
      this.gIssuerLabel.Padding = new System.Windows.Forms.Padding(2);
      this.gIssuerLabel.Size = new System.Drawing.Size(480, 90);
      this.gIssuerLabel.TabIndex = 908;
      this.gIssuerLabel.TabStop = false;
      this.gIssuerLabel.Text = "groupBox1";
      // 
      // bLabelFromEntry
      // 
      this.bLabelFromEntry.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bLabelFromEntry.AutoEllipsis = true;
      this.bLabelFromEntry.Location = new System.Drawing.Point(352, 49);
      this.bLabelFromEntry.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.bLabelFromEntry.Name = "bLabelFromEntry";
      this.bLabelFromEntry.Size = new System.Drawing.Size(115, 24);
      this.bLabelFromEntry.TabIndex = 233;
      this.bLabelFromEntry.Text = "OK";
      this.bLabelFromEntry.UseVisualStyleBackColor = true;
      // 
      // bIssuerFromEntry
      // 
      this.bIssuerFromEntry.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.bIssuerFromEntry.AutoEllipsis = true;
      this.bIssuerFromEntry.Location = new System.Drawing.Point(352, 21);
      this.bIssuerFromEntry.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.bIssuerFromEntry.Name = "bIssuerFromEntry";
      this.bIssuerFromEntry.Size = new System.Drawing.Size(115, 24);
      this.bIssuerFromEntry.TabIndex = 231;
      this.bIssuerFromEntry.Text = "OK";
      this.bIssuerFromEntry.UseVisualStyleBackColor = true;
      // 
      // tbLabel
      // 
      this.tbLabel.Location = new System.Drawing.Point(89, 52);
      this.tbLabel.Margin = new System.Windows.Forms.Padding(2);
      this.tbLabel.Name = "tbLabel";
      this.tbLabel.Size = new System.Drawing.Size(231, 22);
      this.tbLabel.TabIndex = 232;
      // 
      // tbIssuer
      // 
      this.tbIssuer.Location = new System.Drawing.Point(90, 23);
      this.tbIssuer.Margin = new System.Windows.Forms.Padding(2);
      this.tbIssuer.Name = "tbIssuer";
      this.tbIssuer.Size = new System.Drawing.Size(230, 22);
      this.tbIssuer.TabIndex = 230;
      // 
      // lLabel
      // 
      this.lLabel.AutoSize = true;
      this.lLabel.Location = new System.Drawing.Point(7, 55);
      this.lLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.lLabel.Name = "lLabel";
      this.lLabel.Size = new System.Drawing.Size(44, 16);
      this.lLabel.TabIndex = 1;
      this.lLabel.Text = "label2";
      // 
      // lIssuer
      // 
      this.lIssuer.AutoSize = true;
      this.lIssuer.Location = new System.Drawing.Point(8, 27);
      this.lIssuer.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.lIssuer.Name = "lIssuer";
      this.lIssuer.Size = new System.Drawing.Size(44, 16);
      this.lIssuer.TabIndex = 0;
      this.lIssuer.Text = "label1";
      // 
      // gOTP
      // 
      this.gOTP.Collapsed = false;
      this.gOTP.Controls.Add(this.lYandexPin);
      this.gOTP.Controls.Add(this.tbYandexPin);
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
      this.gOTP.Location = new System.Drawing.Point(2, 102);
      this.gOTP.Margin = new System.Windows.Forms.Padding(2, 3, 2, 30);
      this.gOTP.Name = "gOTP";
      this.gOTP.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.gOTP.Size = new System.Drawing.Size(480, 141);
      this.gOTP.TabIndex = 906;
      this.gOTP.TabStop = false;
      this.gOTP.Text = "OTP settings";
      // 
      // lYandexPin
      // 
      this.lYandexPin.Location = new System.Drawing.Point(222, 19);
      this.lYandexPin.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.lYandexPin.Name = "lYandexPin";
      this.lYandexPin.Size = new System.Drawing.Size(106, 19);
      this.lYandexPin.TabIndex = 5222;
      this.lYandexPin.Text = "Pin:";
      // 
      // tbYandexPin
      // 
      this.tbYandexPin.Location = new System.Drawing.Point(338, 16);
      this.tbYandexPin.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.tbYandexPin.Name = "tbYandexPin";
      this.tbYandexPin.Size = new System.Drawing.Size(90, 22);
      this.tbYandexPin.TabIndex = 221;
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
      this.cbOTPFormat.Location = new System.Drawing.Point(89, 46);
      this.cbOTPFormat.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.cbOTPFormat.Name = "cbOTPFormat";
      this.cbOTPFormat.Size = new System.Drawing.Size(108, 24);
      this.cbOTPFormat.TabIndex = 222;
      // 
      // lFormat
      // 
      this.lFormat.Location = new System.Drawing.Point(7, 50);
      this.lFormat.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.lFormat.Name = "lFormat";
      this.lFormat.Size = new System.Drawing.Size(80, 19);
      this.lFormat.TabIndex = 9;
      this.lFormat.Text = "Format:";
      // 
      // tbTOTPTimestep
      // 
      this.tbTOTPTimestep.Location = new System.Drawing.Point(338, 16);
      this.tbTOTPTimestep.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.tbTOTPTimestep.Name = "tbTOTPTimestep";
      this.tbTOTPTimestep.Size = new System.Drawing.Size(90, 22);
      this.tbTOTPTimestep.TabIndex = 221;
      // 
      // lTimestep
      // 
      this.lTimestep.Location = new System.Drawing.Point(222, 19);
      this.lTimestep.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.lTimestep.Name = "lTimestep";
      this.lTimestep.Size = new System.Drawing.Size(106, 19);
      this.lTimestep.TabIndex = 7;
      this.lTimestep.Text = "Timestep:";
      // 
      // tbHOTPCounter
      // 
      this.tbHOTPCounter.Location = new System.Drawing.Point(338, 16);
      this.tbHOTPCounter.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.tbHOTPCounter.Name = "tbHOTPCounter";
      this.tbHOTPCounter.Size = new System.Drawing.Size(90, 22);
      this.tbHOTPCounter.TabIndex = 221;
      // 
      // lCounter
      // 
      this.lCounter.Location = new System.Drawing.Point(222, 21);
      this.lCounter.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.lCounter.Name = "lCounter";
      this.lCounter.Size = new System.Drawing.Size(106, 19);
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
      this.cbOTPHashFunc.Location = new System.Drawing.Point(89, 107);
      this.cbOTPHashFunc.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.cbOTPHashFunc.Name = "cbOTPHashFunc";
      this.cbOTPHashFunc.Size = new System.Drawing.Size(108, 24);
      this.cbOTPHashFunc.TabIndex = 224;
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
      this.cbOTPLength.Location = new System.Drawing.Point(89, 77);
      this.cbOTPLength.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.cbOTPLength.Name = "cbOTPLength";
      this.cbOTPLength.Size = new System.Drawing.Size(108, 24);
      this.cbOTPLength.TabIndex = 223;
      // 
      // cbOTPType
      // 
      this.cbOTPType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbOTPType.FormattingEnabled = true;
      this.cbOTPType.Items.AddRange(new object[] {
            "HOTP",
            "TOTP",
            "Steam",
            "Yandex"});
      this.cbOTPType.Location = new System.Drawing.Point(89, 16);
      this.cbOTPType.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.cbOTPType.Name = "cbOTPType";
      this.cbOTPType.Size = new System.Drawing.Size(108, 24);
      this.cbOTPType.TabIndex = 220;
      // 
      // lHash
      // 
      this.lHash.Location = new System.Drawing.Point(6, 109);
      this.lHash.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.lHash.Name = "lHash";
      this.lHash.Size = new System.Drawing.Size(80, 19);
      this.lHash.TabIndex = 2;
      this.lHash.Text = "OTP hash:";
      // 
      // lLength
      // 
      this.lLength.Location = new System.Drawing.Point(6, 78);
      this.lLength.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.lLength.Name = "lLength";
      this.lLength.Size = new System.Drawing.Size(80, 19);
      this.lLength.TabIndex = 1;
      this.lLength.Text = "OTP length:";
      // 
      // lType
      // 
      this.lType.Location = new System.Drawing.Point(6, 19);
      this.lType.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.lType.Name = "lType";
      this.lType.Size = new System.Drawing.Size(80, 19);
      this.lType.TabIndex = 0;
      this.lType.Text = "OTP type:";
      // 
      // gRecovery
      // 
      this.gRecovery.Collapsed = false;
      this.gRecovery.Controls.Add(this.tbRecovery);
      this.gRecovery.Dock = System.Windows.Forms.DockStyle.Top;
      this.gRecovery.Location = new System.Drawing.Point(2, 2);
      this.gRecovery.Name = "gRecovery";
      this.gRecovery.Size = new System.Drawing.Size(480, 100);
      this.gRecovery.TabIndex = 909;
      this.gRecovery.TabStop = false;
      this.gRecovery.Text = "Recovery";
      // 
      // tbRecovery
      // 
      this.tbRecovery.AcceptsReturn = true;
      this.tbRecovery.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tbRecovery.Location = new System.Drawing.Point(3, 18);
      this.tbRecovery.Margin = new System.Windows.Forms.Padding(2);
      this.tbRecovery.Multiline = true;
      this.tbRecovery.Name = "tbRecovery";
      this.tbRecovery.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.tbRecovery.Size = new System.Drawing.Size(474, 79);
      this.tbRecovery.TabIndex = 210;
      this.tbRecovery.Leave += new System.EventHandler(this.tbRecovery_Leave);
      // 
      // KeePassOTPSetup
      // 
      this.AcceptButton = this.buttonOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(500, 612);
      this.Controls.Add(this.tcSetup);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOK);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "KeePassOTPSetup";
      this.Padding = new System.Windows.Forms.Padding(4);
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "KeePassOTPSetup";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
      this.tcSetup.ResumeLayout(false);
      this.tpSetup.ResumeLayout(false);
      this.tpSetup.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pbOTPPreview)).EndInit();
      this.tpAdvanced.ResumeLayout(false);
      this.gTime.ResumeLayout(false);
      this.gTime.PerformLayout();
      this.gIssuerLabel.ResumeLayout(false);
      this.gIssuerLabel.PerformLayout();
      this.gOTP.ResumeLayout(false);
      this.gOTP.PerformLayout();
      this.gRecovery.ResumeLayout(false);
      this.gRecovery.PerformLayout();
      this.ResumeLayout(false);

    }


    private System.Windows.Forms.ToolTip toolTip1;
    private System.Windows.Forms.TabControl tcSetup;
    private System.Windows.Forms.TabPage tpSetup;
    private System.Windows.Forms.PictureBox pbOTPPreview;
    internal System.Windows.Forms.CheckBox cbShowInTray;
    private System.Windows.Forms.Button btnReadScreen;
    private System.Windows.Forms.Button btnDragDropQR;
    private System.Windows.Forms.ProgressBar pbTOTPLifetime;
    private System.Windows.Forms.Label otpPreviewNext;
    private System.Windows.Forms.Label otpPreview;
    internal System.Windows.Forms.TextBox tbOTPSeed;
    private System.Windows.Forms.Label lSeed;
    private System.Windows.Forms.TabPage tpAdvanced;
    private System.Windows.Forms.Button bLabelFromEntry;
    private System.Windows.Forms.Button bIssuerFromEntry;
    private System.Windows.Forms.TextBox tbLabel;
    private System.Windows.Forms.TextBox tbIssuer;
    private System.Windows.Forms.Label lLabel;
    private System.Windows.Forms.Label lIssuer;
    private System.Windows.Forms.Label totpTimeCorrectionValue;
    private System.Windows.Forms.Label lTime;
    private System.Windows.Forms.Label lTimeType;
    private System.Windows.Forms.ComboBox totpTimeCorrectionType;
    internal System.Windows.Forms.TextBox tbTOTPTimeCorrectionURL;
    private System.Windows.Forms.Label lURL;
    private System.Windows.Forms.Label lYandexPin;
    internal System.Windows.Forms.TextBox tbYandexPin;
    internal System.Windows.Forms.ComboBox cbOTPFormat;
    private System.Windows.Forms.Label lFormat;
    internal System.Windows.Forms.TextBox tbTOTPTimestep;
    private System.Windows.Forms.Label lTimestep;
    internal System.Windows.Forms.TextBox tbHOTPCounter;
    private System.Windows.Forms.Label lCounter;
    internal System.Windows.Forms.ComboBox cbOTPHashFunc;
    internal System.Windows.Forms.ComboBox cbOTPLength;
    internal System.Windows.Forms.ComboBox cbOTPType;
    private System.Windows.Forms.Label lHash;
    private System.Windows.Forms.Label lLength;
    private System.Windows.Forms.Label lType;
    private System.DirectoryServices.DirectoryEntry directoryEntry1;
    internal System.Windows.Forms.TextBox tbRecovery;
    private CollapsibleGroupBox gIssuerLabel;
    private CollapsibleGroupBox gTime;
    private CollapsibleGroupBox gOTP;
    private CollapsibleGroupBox gRecovery;
  }
}
