namespace KeePassOTP
{
	partial class Options
	{
		/// <summary> 
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Verwendete Ressourcen bereinigen.
		/// </summary>
		/// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Vom Komponenten-Designer generierter Code

		/// <summary> 
		/// Erforderliche Methode für die Designerunterstützung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpGeneral = new System.Windows.Forms.TabPage();
            this.gOtherOptions = new System.Windows.Forms.GroupBox();
            this.tb2FAHelp = new System.Windows.Forms.TextBox();
            this.cbCheckTFA = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cbOTPDisplay = new System.Windows.Forms.ComboBox();
            this.lOTPDisplay = new System.Windows.Forms.Label();
            this.lOTPRenewal = new System.Windows.Forms.Label();
            this.cbOTPRenewal = new System.Windows.Forms.ComboBox();
            this.gbAutotype = new System.Windows.Forms.GroupBox();
            this.pPlaceholder = new System.Windows.Forms.Panel();
            this.cbAutoSubmit = new System.Windows.Forms.CheckBox();
            this.tbPlaceholder = new System.Windows.Forms.TextBox();
            this.lPlaceholder = new System.Windows.Forms.Label();
            this.pHotkey = new System.Windows.Forms.Panel();
            this.cbLocalHotkey = new System.Windows.Forms.CheckBox();
            this.hkcKPOTP = new KeePass.UI.HotKeyControlEx();
            this.lHotkey = new System.Windows.Forms.Label();
            this.tpDatabases = new System.Windows.Forms.TabPage();
            this.tlp = new System.Windows.Forms.TableLayoutPanel();
            this.pButtons = new System.Windows.Forms.Panel();
            this.cbDBAction = new System.Windows.Forms.ComboBox();
            this.bDBSettings = new System.Windows.Forms.Button();
            this.bExport = new System.Windows.Forms.Button();
            this.bChangeMasterKey = new System.Windows.Forms.Button();
            this.bCreateOpen = new System.Windows.Forms.Button();
            this.bMigrate2Entry = new System.Windows.Forms.Button();
            this.bMigrate2DB = new System.Windows.Forms.Button();
            this.lbDB = new System.Windows.Forms.ListBox();
            this.pCheckboxes = new System.Windows.Forms.Panel();
            this.gMigrate = new System.Windows.Forms.GroupBox();
            this.bMigrate = new System.Windows.Forms.Button();
            this.cbMigrate = new System.Windows.Forms.ComboBox();
            this.cbPreloadOTP = new System.Windows.Forms.CheckBox();
            this.cbUseDBForSeeds = new System.Windows.Forms.CheckBox();
            this.tpHelp = new System.Windows.Forms.TabPage();
            this.tbHelp = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tabControl1.SuspendLayout();
            this.tpGeneral.SuspendLayout();
            this.gOtherOptions.SuspendLayout();
            this.panel1.SuspendLayout();
            this.gbAutotype.SuspendLayout();
            this.pPlaceholder.SuspendLayout();
            this.pHotkey.SuspendLayout();
            this.tpDatabases.SuspendLayout();
            this.tlp.SuspendLayout();
            this.pButtons.SuspendLayout();
            this.pCheckboxes.SuspendLayout();
            this.gMigrate.SuspendLayout();
            this.tpHelp.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpGeneral);
            this.tabControl1.Controls.Add(this.tpDatabases);
            this.tabControl1.Controls.Add(this.tpHelp);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(5);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1387, 674);
            this.tabControl1.TabIndex = 2;
            // 
            // tpGeneral
            // 
            this.tpGeneral.Controls.Add(this.gOtherOptions);
            this.tpGeneral.Controls.Add(this.gbAutotype);
            this.tpGeneral.Location = new System.Drawing.Point(10, 48);
            this.tpGeneral.Margin = new System.Windows.Forms.Padding(5);
            this.tpGeneral.Name = "tpGeneral";
            this.tpGeneral.Padding = new System.Windows.Forms.Padding(18, 15, 18, 15);
            this.tpGeneral.Size = new System.Drawing.Size(1367, 616);
            this.tpGeneral.TabIndex = 0;
            this.tpGeneral.Text = "General";
            this.tpGeneral.UseVisualStyleBackColor = true;
            // 
            // gOtherOptions
            // 
            this.gOtherOptions.AutoSize = true;
            this.gOtherOptions.Controls.Add(this.tb2FAHelp);
            this.gOtherOptions.Controls.Add(this.cbCheckTFA);
            this.gOtherOptions.Controls.Add(this.panel1);
            this.gOtherOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gOtherOptions.Location = new System.Drawing.Point(18, 216);
            this.gOtherOptions.Name = "gOtherOptions";
            this.gOtherOptions.Size = new System.Drawing.Size(1331, 385);
            this.gOtherOptions.TabIndex = 13;
            this.gOtherOptions.TabStop = false;
            this.gOtherOptions.Text = "OTP Renewal";
            // 
            // tb2FAHelp
            // 
            this.tb2FAHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tb2FAHelp.Location = new System.Drawing.Point(3, 184);
            this.tb2FAHelp.Margin = new System.Windows.Forms.Padding(5);
            this.tb2FAHelp.Multiline = true;
            this.tb2FAHelp.Name = "tb2FAHelp";
            this.tb2FAHelp.ReadOnly = true;
            this.tb2FAHelp.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tb2FAHelp.Size = new System.Drawing.Size(1325, 198);
            this.tb2FAHelp.TabIndex = 180;
            // 
            // cbCheckTFA
            // 
            this.cbCheckTFA.AutoSize = true;
            this.cbCheckTFA.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbCheckTFA.Location = new System.Drawing.Point(3, 148);
            this.cbCheckTFA.Name = "cbCheckTFA";
            this.cbCheckTFA.Size = new System.Drawing.Size(1325, 36);
            this.cbCheckTFA.TabIndex = 170;
            this.cbCheckTFA.Text = "Check TFA";
            this.cbCheckTFA.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cbOTPDisplay);
            this.panel1.Controls.Add(this.lOTPDisplay);
            this.panel1.Controls.Add(this.lOTPRenewal);
            this.panel1.Controls.Add(this.cbOTPRenewal);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 34);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1325, 114);
            this.panel1.TabIndex = 2;
            // 
            // cbOTPDisplay
            // 
            this.cbOTPDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbOTPDisplay.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbOTPDisplay.FormattingEnabled = true;
            this.cbOTPDisplay.Location = new System.Drawing.Point(520, 8);
            this.cbOTPDisplay.Name = "cbOTPDisplay";
            this.cbOTPDisplay.Size = new System.Drawing.Size(789, 39);
            this.cbOTPDisplay.TabIndex = 150;
            // 
            // lOTPDisplay
            // 
            this.lOTPDisplay.AutoSize = true;
            this.lOTPDisplay.Location = new System.Drawing.Point(5, 17);
            this.lOTPDisplay.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lOTPDisplay.Name = "lOTPDisplay";
            this.lOTPDisplay.Size = new System.Drawing.Size(182, 32);
            this.lOTPDisplay.TabIndex = 3;
            this.lOTPDisplay.Text = "OTP Display:";
            // 
            // lOTPRenewal
            // 
            this.lOTPRenewal.AutoSize = true;
            this.lOTPRenewal.Location = new System.Drawing.Point(5, 57);
            this.lOTPRenewal.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lOTPRenewal.Name = "lOTPRenewal";
            this.lOTPRenewal.Size = new System.Drawing.Size(199, 32);
            this.lOTPRenewal.TabIndex = 3;
            this.lOTPRenewal.Text = "OTP Renewal:";
            // 
            // cbOTPRenewal
            // 
            this.cbOTPRenewal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbOTPRenewal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbOTPRenewal.FormattingEnabled = true;
            this.cbOTPRenewal.Location = new System.Drawing.Point(520, 54);
            this.cbOTPRenewal.Name = "cbOTPRenewal";
            this.cbOTPRenewal.Size = new System.Drawing.Size(789, 39);
            this.cbOTPRenewal.TabIndex = 160;
            // 
            // gbAutotype
            // 
            this.gbAutotype.Controls.Add(this.pPlaceholder);
            this.gbAutotype.Controls.Add(this.pHotkey);
            this.gbAutotype.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbAutotype.Location = new System.Drawing.Point(18, 15);
            this.gbAutotype.Margin = new System.Windows.Forms.Padding(5);
            this.gbAutotype.Name = "gbAutotype";
            this.gbAutotype.Padding = new System.Windows.Forms.Padding(5);
            this.gbAutotype.Size = new System.Drawing.Size(1331, 201);
            this.gbAutotype.TabIndex = 12;
            this.gbAutotype.TabStop = false;
            this.gbAutotype.Text = "gbAutotype";
            // 
            // pPlaceholder
            // 
            this.pPlaceholder.Controls.Add(this.cbAutoSubmit);
            this.pPlaceholder.Controls.Add(this.tbPlaceholder);
            this.pPlaceholder.Controls.Add(this.lPlaceholder);
            this.pPlaceholder.Dock = System.Windows.Forms.DockStyle.Top;
            this.pPlaceholder.Location = new System.Drawing.Point(5, 90);
            this.pPlaceholder.Margin = new System.Windows.Forms.Padding(5);
            this.pPlaceholder.Name = "pPlaceholder";
            this.pPlaceholder.Size = new System.Drawing.Size(1321, 101);
            this.pPlaceholder.TabIndex = 13;
            // 
            // cbAutoSubmit
            // 
            this.cbAutoSubmit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAutoSubmit.AutoSize = true;
            this.cbAutoSubmit.Location = new System.Drawing.Point(524, 48);
            this.cbAutoSubmit.Margin = new System.Windows.Forms.Padding(5);
            this.cbAutoSubmit.Name = "cbAutoSubmit";
            this.cbAutoSubmit.Size = new System.Drawing.Size(287, 36);
            this.cbAutoSubmit.TabIndex = 140;
            this.cbAutoSubmit.Text = "Placholder + Enter";
            this.cbAutoSubmit.UseVisualStyleBackColor = true;
            // 
            // tbPlaceholder
            // 
            this.tbPlaceholder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbPlaceholder.Location = new System.Drawing.Point(518, 0);
            this.tbPlaceholder.Margin = new System.Windows.Forms.Padding(5);
            this.tbPlaceholder.Name = "tbPlaceholder";
            this.tbPlaceholder.Size = new System.Drawing.Size(459, 38);
            this.tbPlaceholder.TabIndex = 130;
            this.tbPlaceholder.TextChanged += new System.EventHandler(this.tbPlaceholder_TextChanged);
            // 
            // lPlaceholder
            // 
            this.lPlaceholder.AutoSize = true;
            this.lPlaceholder.Location = new System.Drawing.Point(0, 5);
            this.lPlaceholder.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lPlaceholder.Name = "lPlaceholder";
            this.lPlaceholder.Size = new System.Drawing.Size(175, 32);
            this.lPlaceholder.TabIndex = 0;
            this.lPlaceholder.Text = "Placeholder:";
            // 
            // pHotkey
            // 
            this.pHotkey.Controls.Add(this.cbLocalHotkey);
            this.pHotkey.Controls.Add(this.hkcKPOTP);
            this.pHotkey.Controls.Add(this.lHotkey);
            this.pHotkey.Dock = System.Windows.Forms.DockStyle.Top;
            this.pHotkey.Location = new System.Drawing.Point(5, 36);
            this.pHotkey.Margin = new System.Windows.Forms.Padding(5);
            this.pHotkey.Name = "pHotkey";
            this.pHotkey.Size = new System.Drawing.Size(1321, 54);
            this.pHotkey.TabIndex = 12;
            // 
            // cbLocalHotkey
            // 
            this.cbLocalHotkey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbLocalHotkey.Location = new System.Drawing.Point(987, 5);
            this.cbLocalHotkey.Margin = new System.Windows.Forms.Padding(5);
            this.cbLocalHotkey.Name = "cbLocalHotkey";
            this.cbLocalHotkey.Size = new System.Drawing.Size(320, 34);
            this.cbLocalHotkey.TabIndex = 120;
            this.cbLocalHotkey.Text = "Copy instead of auto-type";
            this.cbLocalHotkey.UseVisualStyleBackColor = true;
            // 
            // hkcKPOTP
            // 
            this.hkcKPOTP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.hkcKPOTP.Location = new System.Drawing.Point(518, 0);
            this.hkcKPOTP.Margin = new System.Windows.Forms.Padding(5);
            this.hkcKPOTP.Name = "hkcKPOTP";
            this.hkcKPOTP.Size = new System.Drawing.Size(459, 38);
            this.hkcKPOTP.TabIndex = 100;
            // 
            // lHotkey
            // 
            this.lHotkey.AutoSize = true;
            this.lHotkey.Location = new System.Drawing.Point(0, 5);
            this.lHotkey.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lHotkey.Name = "lHotkey";
            this.lHotkey.Size = new System.Drawing.Size(111, 32);
            this.lHotkey.TabIndex = 0;
            this.lHotkey.Text = "Hotkey:";
            // 
            // tpDatabases
            // 
            this.tpDatabases.Controls.Add(this.tlp);
            this.tpDatabases.Location = new System.Drawing.Point(10, 48);
            this.tpDatabases.Margin = new System.Windows.Forms.Padding(5);
            this.tpDatabases.Name = "tpDatabases";
            this.tpDatabases.Padding = new System.Windows.Forms.Padding(18, 15, 18, 15);
            this.tpDatabases.Size = new System.Drawing.Size(1367, 616);
            this.tpDatabases.TabIndex = 0;
            this.tpDatabases.Text = "Settings";
            this.tpDatabases.UseVisualStyleBackColor = true;
            // 
            // tlp
            // 
            this.tlp.ColumnCount = 2;
            this.tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlp.Controls.Add(this.pButtons, 1, 0);
            this.tlp.Controls.Add(this.lbDB, 0, 0);
            this.tlp.Controls.Add(this.pCheckboxes, 0, 1);
            this.tlp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlp.Location = new System.Drawing.Point(18, 15);
            this.tlp.Margin = new System.Windows.Forms.Padding(5);
            this.tlp.Name = "tlp";
            this.tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 279F));
            this.tlp.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlp.Size = new System.Drawing.Size(1331, 586);
            this.tlp.TabIndex = 0;
            // 
            // pButtons
            // 
            this.pButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pButtons.AutoSize = true;
            this.pButtons.Controls.Add(this.cbDBAction);
            this.pButtons.Controls.Add(this.bDBSettings);
            this.pButtons.Controls.Add(this.bExport);
            this.pButtons.Controls.Add(this.bChangeMasterKey);
            this.pButtons.Controls.Add(this.bCreateOpen);
            this.pButtons.Controls.Add(this.bMigrate2Entry);
            this.pButtons.Controls.Add(this.bMigrate2DB);
            this.pButtons.Location = new System.Drawing.Point(623, 0);
            this.pButtons.Margin = new System.Windows.Forms.Padding(0);
            this.pButtons.Name = "pButtons";
            this.pButtons.Size = new System.Drawing.Size(708, 279);
            this.pButtons.TabIndex = 2;
            // 
            // cbDBAction
            // 
            this.cbDBAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDBAction.FormattingEnabled = true;
            this.cbDBAction.Location = new System.Drawing.Point(0, 0);
            this.cbDBAction.Margin = new System.Windows.Forms.Padding(5);
            this.cbDBAction.Name = "cbDBAction";
            this.cbDBAction.Size = new System.Drawing.Size(601, 39);
            this.cbDBAction.TabIndex = 110;
            // 
            // bDBSettings
            // 
            this.bDBSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bDBSettings.AutoSize = true;
            this.bDBSettings.Location = new System.Drawing.Point(0, 93);
            this.bDBSettings.Margin = new System.Windows.Forms.Padding(0);
            this.bDBSettings.Name = "bDBSettings";
            this.bDBSettings.Size = new System.Drawing.Size(702, 45);
            this.bDBSettings.TabIndex = 140;
            this.bDBSettings.Text = "bDBSettings";
            this.bDBSettings.UseVisualStyleBackColor = true;
            this.bDBSettings.Click += new System.EventHandler(this.bDBSettings_Click);
            // 
            // bExport
            // 
            this.bExport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bExport.AutoSize = true;
            this.bExport.Location = new System.Drawing.Point(0, 139);
            this.bExport.Margin = new System.Windows.Forms.Padding(0);
            this.bExport.Name = "bExport";
            this.bExport.Size = new System.Drawing.Size(702, 45);
            this.bExport.TabIndex = 150;
            this.bExport.Text = "bExport";
            this.bExport.UseVisualStyleBackColor = true;
            this.bExport.Click += new System.EventHandler(this.bExport_Click);
            // 
            // bChangeMasterKey
            // 
            this.bChangeMasterKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bChangeMasterKey.AutoSize = true;
            this.bChangeMasterKey.Location = new System.Drawing.Point(0, 46);
            this.bChangeMasterKey.Margin = new System.Windows.Forms.Padding(0);
            this.bChangeMasterKey.Name = "bChangeMasterKey";
            this.bChangeMasterKey.Size = new System.Drawing.Size(702, 45);
            this.bChangeMasterKey.TabIndex = 130;
            this.bChangeMasterKey.Text = "bChangeMasterKey";
            this.bChangeMasterKey.UseVisualStyleBackColor = true;
            this.bChangeMasterKey.Click += new System.EventHandler(this.bChangeMasterKey_Click);
            // 
            // bCreateOpen
            // 
            this.bCreateOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bCreateOpen.Location = new System.Drawing.Point(613, 0);
            this.bCreateOpen.Margin = new System.Windows.Forms.Padding(0);
            this.bCreateOpen.Name = "bCreateOpen";
            this.bCreateOpen.Size = new System.Drawing.Size(89, 46);
            this.bCreateOpen.TabIndex = 120;
            this.bCreateOpen.Text = "OK";
            this.bCreateOpen.UseVisualStyleBackColor = true;
            this.bCreateOpen.Click += new System.EventHandler(this.bCreateOpen_Click);
            // 
            // bMigrate2Entry
            // 
            this.bMigrate2Entry.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bMigrate2Entry.AutoSize = true;
            this.bMigrate2Entry.Location = new System.Drawing.Point(0, 186);
            this.bMigrate2Entry.Margin = new System.Windows.Forms.Padding(0);
            this.bMigrate2Entry.Name = "bMigrate2Entry";
            this.bMigrate2Entry.Size = new System.Drawing.Size(702, 45);
            this.bMigrate2Entry.TabIndex = 160;
            this.bMigrate2Entry.Text = "bMigrate2Entry";
            this.bMigrate2Entry.UseVisualStyleBackColor = true;
            this.bMigrate2Entry.Click += new System.EventHandler(this.bMigrate2Entry_Click);
            // 
            // bMigrate2DB
            // 
            this.bMigrate2DB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bMigrate2DB.AutoSize = true;
            this.bMigrate2DB.Location = new System.Drawing.Point(0, 232);
            this.bMigrate2DB.Margin = new System.Windows.Forms.Padding(0);
            this.bMigrate2DB.Name = "bMigrate2DB";
            this.bMigrate2DB.Size = new System.Drawing.Size(702, 45);
            this.bMigrate2DB.TabIndex = 170;
            this.bMigrate2DB.Text = "bMigrate2DB";
            this.bMigrate2DB.UseVisualStyleBackColor = true;
            this.bMigrate2DB.Click += new System.EventHandler(this.bMigrate2DB_Click);
            // 
            // lbDB
            // 
            this.lbDB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbDB.FormattingEnabled = true;
            this.lbDB.ItemHeight = 31;
            this.lbDB.Location = new System.Drawing.Point(5, 5);
            this.lbDB.Margin = new System.Windows.Forms.Padding(5);
            this.lbDB.Name = "lbDB";
            this.lbDB.Size = new System.Drawing.Size(613, 269);
            this.lbDB.TabIndex = 100;
            this.lbDB.SelectedIndexChanged += new System.EventHandler(this.lbDB_SelectedIndexChanged);
            // 
            // pCheckboxes
            // 
            this.pCheckboxes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pCheckboxes.AutoSize = true;
            this.tlp.SetColumnSpan(this.pCheckboxes, 2);
            this.pCheckboxes.Controls.Add(this.gMigrate);
            this.pCheckboxes.Controls.Add(this.cbPreloadOTP);
            this.pCheckboxes.Controls.Add(this.cbUseDBForSeeds);
            this.pCheckboxes.Location = new System.Drawing.Point(5, 284);
            this.pCheckboxes.Margin = new System.Windows.Forms.Padding(5);
            this.pCheckboxes.Name = "pCheckboxes";
            this.pCheckboxes.Size = new System.Drawing.Size(1321, 297);
            this.pCheckboxes.TabIndex = 2;
            // 
            // gMigrate
            // 
            this.gMigrate.Controls.Add(this.bMigrate);
            this.gMigrate.Controls.Add(this.cbMigrate);
            this.gMigrate.Dock = System.Windows.Forms.DockStyle.Top;
            this.gMigrate.Location = new System.Drawing.Point(0, 72);
            this.gMigrate.Margin = new System.Windows.Forms.Padding(5);
            this.gMigrate.Name = "gMigrate";
            this.gMigrate.Padding = new System.Windows.Forms.Padding(5);
            this.gMigrate.Size = new System.Drawing.Size(1321, 99);
            this.gMigrate.TabIndex = 200;
            this.gMigrate.TabStop = false;
            this.gMigrate.Text = "gMigrate";
            // 
            // bMigrate
            // 
            this.bMigrate.Location = new System.Drawing.Point(738, 39);
            this.bMigrate.Margin = new System.Windows.Forms.Padding(5);
            this.bMigrate.Name = "bMigrate";
            this.bMigrate.Size = new System.Drawing.Size(178, 46);
            this.bMigrate.TabIndex = 210;
            this.bMigrate.Text = "bMigrate";
            this.bMigrate.UseVisualStyleBackColor = true;
            this.bMigrate.Click += new System.EventHandler(this.bMigrate_Click);
            // 
            // cbMigrate
            // 
            this.cbMigrate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMigrate.FormattingEnabled = true;
            this.cbMigrate.Location = new System.Drawing.Point(11, 39);
            this.cbMigrate.Margin = new System.Windows.Forms.Padding(5);
            this.cbMigrate.Name = "cbMigrate";
            this.cbMigrate.Size = new System.Drawing.Size(713, 39);
            this.cbMigrate.TabIndex = 200;
            this.cbMigrate.SelectedIndexChanged += new System.EventHandler(this.cbMigrate_SelectedIndexChanged);
            // 
            // cbPreloadOTP
            // 
            this.cbPreloadOTP.AutoSize = true;
            this.cbPreloadOTP.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbPreloadOTP.Location = new System.Drawing.Point(0, 36);
            this.cbPreloadOTP.Margin = new System.Windows.Forms.Padding(5);
            this.cbPreloadOTP.Name = "cbPreloadOTP";
            this.cbPreloadOTP.Size = new System.Drawing.Size(1321, 36);
            this.cbPreloadOTP.TabIndex = 190;
            this.cbPreloadOTP.Text = "cbPreloadOTP";
            this.cbPreloadOTP.UseVisualStyleBackColor = true;
            this.cbPreloadOTP.CheckedChanged += new System.EventHandler(this.cbPreloadOTP_CheckedChanged);
            // 
            // cbUseDBForSeeds
            // 
            this.cbUseDBForSeeds.AutoSize = true;
            this.cbUseDBForSeeds.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbUseDBForSeeds.Location = new System.Drawing.Point(0, 0);
            this.cbUseDBForSeeds.Margin = new System.Windows.Forms.Padding(5);
            this.cbUseDBForSeeds.Name = "cbUseDBForSeeds";
            this.cbUseDBForSeeds.Size = new System.Drawing.Size(1321, 36);
            this.cbUseDBForSeeds.TabIndex = 180;
            this.cbUseDBForSeeds.Text = "cbUseDBForSeeds";
            this.cbUseDBForSeeds.UseVisualStyleBackColor = true;
            this.cbUseDBForSeeds.CheckedChanged += new System.EventHandler(this.cbUseDBForSeeds_CheckedChanged);
            // 
            // tpHelp
            // 
            this.tpHelp.Controls.Add(this.tbHelp);
            this.tpHelp.Location = new System.Drawing.Point(10, 48);
            this.tpHelp.Margin = new System.Windows.Forms.Padding(5);
            this.tpHelp.Name = "tpHelp";
            this.tpHelp.Padding = new System.Windows.Forms.Padding(18, 15, 18, 15);
            this.tpHelp.Size = new System.Drawing.Size(1367, 616);
            this.tpHelp.TabIndex = 1;
            this.tpHelp.Text = "Help";
            this.tpHelp.UseVisualStyleBackColor = true;
            // 
            // tbHelp
            // 
            this.tbHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbHelp.Location = new System.Drawing.Point(18, 15);
            this.tbHelp.Margin = new System.Windows.Forms.Padding(5);
            this.tbHelp.Multiline = true;
            this.tbHelp.Name = "tbHelp";
            this.tbHelp.ReadOnly = true;
            this.tbHelp.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbHelp.Size = new System.Drawing.Size(1331, 586);
            this.tbHelp.TabIndex = 0;
            // 
            // Options
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "Options";
            this.Size = new System.Drawing.Size(1387, 674);
            this.Resize += new System.EventHandler(this.Options_Resize);
            this.tabControl1.ResumeLayout(false);
            this.tpGeneral.ResumeLayout(false);
            this.tpGeneral.PerformLayout();
            this.gOtherOptions.ResumeLayout(false);
            this.gOtherOptions.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.gbAutotype.ResumeLayout(false);
            this.pPlaceholder.ResumeLayout(false);
            this.pPlaceholder.PerformLayout();
            this.pHotkey.ResumeLayout(false);
            this.pHotkey.PerformLayout();
            this.tpDatabases.ResumeLayout(false);
            this.tlp.ResumeLayout(false);
            this.tlp.PerformLayout();
            this.pButtons.ResumeLayout(false);
            this.pButtons.PerformLayout();
            this.pCheckboxes.ResumeLayout(false);
            this.pCheckboxes.PerformLayout();
            this.gMigrate.ResumeLayout(false);
            this.tpHelp.ResumeLayout(false);
            this.tpHelp.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tpGeneral;
		private System.Windows.Forms.TabPage tpDatabases;
		private System.Windows.Forms.TableLayoutPanel tlp;
		private System.Windows.Forms.ListBox lbDB;
		private System.Windows.Forms.Panel pButtons;
		private System.Windows.Forms.Panel pCheckboxes;
		private System.Windows.Forms.Button bMigrate2Entry;
		private System.Windows.Forms.Button bMigrate2DB;
		private System.Windows.Forms.TabPage tpHelp;
		private System.Windows.Forms.TextBox tbHelp;
		private System.Windows.Forms.ToolTip toolTip1;
		internal System.Windows.Forms.CheckBox cbPreloadOTP;
		internal System.Windows.Forms.CheckBox cbUseDBForSeeds;
		private System.Windows.Forms.Button bDBSettings;
		private System.Windows.Forms.Button bExport;
		private System.Windows.Forms.Button bChangeMasterKey;
		private System.Windows.Forms.Button bCreateOpen;
		private System.Windows.Forms.GroupBox gMigrate;
		private System.Windows.Forms.Button bMigrate;
		private System.Windows.Forms.ComboBox cbMigrate;
		private System.Windows.Forms.GroupBox gbAutotype;
		private System.Windows.Forms.Panel pPlaceholder;
		internal System.Windows.Forms.TextBox tbPlaceholder;
		private System.Windows.Forms.Label lPlaceholder;
		private System.Windows.Forms.Panel pHotkey;
		internal KeePass.UI.HotKeyControlEx hkcKPOTP;
		private System.Windows.Forms.Label lHotkey;
		private System.Windows.Forms.ComboBox cbDBAction;
		internal System.Windows.Forms.CheckBox cbAutoSubmit;
        private System.Windows.Forms.GroupBox gOtherOptions;
        private System.Windows.Forms.TextBox tb2FAHelp;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lOTPDisplay;
        private System.Windows.Forms.Label lOTPRenewal;
        private System.Windows.Forms.ComboBox cbOTPRenewal;
        internal System.Windows.Forms.CheckBox cbCheckTFA;
        private System.Windows.Forms.ComboBox cbOTPDisplay;
        internal System.Windows.Forms.CheckBox cbLocalHotkey;
    }
}