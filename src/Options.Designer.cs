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
            this.llHotKeyUnix = new System.Windows.Forms.LinkLabel();
            this.cbAutoSubmit = new System.Windows.Forms.CheckBox();
            this.tbPlaceholder = new System.Windows.Forms.TextBox();
            this.lPlaceholder = new System.Windows.Forms.Label();
            this.pHotkey = new System.Windows.Forms.Panel();
            this.cbLocalHotkey = new System.Windows.Forms.CheckBox();
            this.hkcKPOTP = new KeePass.UI.HotKeyControlEx();
            this.lHotkey = new System.Windows.Forms.Label();
            this.tpDatabases = new System.Windows.Forms.TabPage();
            this.pButtons = new System.Windows.Forms.Panel();
            this.bMigrate2EntryOrDB = new System.Windows.Forms.Button();
            this.bChangeMasterKey = new System.Windows.Forms.Button();
            this.bExport = new System.Windows.Forms.Button();
            this.pDropdown = new System.Windows.Forms.Panel();
            this.cbDBAction = new System.Windows.Forms.ComboBox();
            this.bCreateOpen = new System.Windows.Forms.Button();
            this.tlp = new System.Windows.Forms.TableLayoutPanel();
            this.lbDB = new System.Windows.Forms.ListBox();
            this.pCheckboxes = new System.Windows.Forms.Panel();
            this.gMigrate = new System.Windows.Forms.GroupBox();
            this.bMigrate = new System.Windows.Forms.Button();
            this.cbMigrate = new System.Windows.Forms.ComboBox();
            this.cbPreloadOTP = new System.Windows.Forms.CheckBox();
            this.cbUseDBForSeeds = new System.Windows.Forms.CheckBox();
            this.tpHelp = new System.Windows.Forms.TabPage();
            this.tbHelp = new System.Windows.Forms.TextBox();
            this.bDBSettings = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tabControl1.SuspendLayout();
            this.tpGeneral.SuspendLayout();
            this.gOtherOptions.SuspendLayout();
            this.panel1.SuspendLayout();
            this.gbAutotype.SuspendLayout();
            this.pPlaceholder.SuspendLayout();
            this.pHotkey.SuspendLayout();
            this.tpDatabases.SuspendLayout();
            this.pButtons.SuspendLayout();
            this.pDropdown.SuspendLayout();
            this.tlp.SuspendLayout();
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
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(694, 348);
            this.tabControl1.TabIndex = 2;
            // 
            // tpGeneral
            // 
            this.tpGeneral.Controls.Add(this.gOtherOptions);
            this.tpGeneral.Controls.Add(this.gbAutotype);
            this.tpGeneral.Location = new System.Drawing.Point(4, 25);
            this.tpGeneral.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tpGeneral.Name = "tpGeneral";
            this.tpGeneral.Padding = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.tpGeneral.Size = new System.Drawing.Size(686, 319);
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
            this.gOtherOptions.Location = new System.Drawing.Point(9, 112);
            this.gOtherOptions.Margin = new System.Windows.Forms.Padding(2);
            this.gOtherOptions.Name = "gOtherOptions";
            this.gOtherOptions.Padding = new System.Windows.Forms.Padding(2);
            this.gOtherOptions.Size = new System.Drawing.Size(668, 199);
            this.gOtherOptions.TabIndex = 13;
            this.gOtherOptions.TabStop = false;
            this.gOtherOptions.Text = "OTP Renewal";
            // 
            // tb2FAHelp
            // 
            this.tb2FAHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tb2FAHelp.Location = new System.Drawing.Point(2, 96);
            this.tb2FAHelp.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tb2FAHelp.Multiline = true;
            this.tb2FAHelp.Name = "tb2FAHelp";
            this.tb2FAHelp.ReadOnly = true;
            this.tb2FAHelp.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tb2FAHelp.Size = new System.Drawing.Size(664, 101);
            this.tb2FAHelp.TabIndex = 180;
            // 
            // cbCheckTFA
            // 
            this.cbCheckTFA.AutoSize = true;
            this.cbCheckTFA.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbCheckTFA.Location = new System.Drawing.Point(2, 76);
            this.cbCheckTFA.Margin = new System.Windows.Forms.Padding(2);
            this.cbCheckTFA.Name = "cbCheckTFA";
            this.cbCheckTFA.Size = new System.Drawing.Size(664, 20);
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
            this.panel1.Location = new System.Drawing.Point(2, 17);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(664, 59);
            this.panel1.TabIndex = 2;
            // 
            // cbOTPDisplay
            // 
            this.cbOTPDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbOTPDisplay.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbOTPDisplay.FormattingEnabled = true;
            this.cbOTPDisplay.Location = new System.Drawing.Point(262, 4);
            this.cbOTPDisplay.Margin = new System.Windows.Forms.Padding(2);
            this.cbOTPDisplay.Name = "cbOTPDisplay";
            this.cbOTPDisplay.Size = new System.Drawing.Size(396, 24);
            this.cbOTPDisplay.TabIndex = 150;
            // 
            // lOTPDisplay
            // 
            this.lOTPDisplay.AutoSize = true;
            this.lOTPDisplay.Location = new System.Drawing.Point(2, 9);
            this.lOTPDisplay.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lOTPDisplay.Name = "lOTPDisplay";
            this.lOTPDisplay.Size = new System.Drawing.Size(87, 16);
            this.lOTPDisplay.TabIndex = 3;
            this.lOTPDisplay.Text = "OTP Display:";
            // 
            // lOTPRenewal
            // 
            this.lOTPRenewal.AutoSize = true;
            this.lOTPRenewal.Location = new System.Drawing.Point(2, 29);
            this.lOTPRenewal.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lOTPRenewal.Name = "lOTPRenewal";
            this.lOTPRenewal.Size = new System.Drawing.Size(94, 16);
            this.lOTPRenewal.TabIndex = 3;
            this.lOTPRenewal.Text = "OTP Renewal:";
            // 
            // cbOTPRenewal
            // 
            this.cbOTPRenewal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbOTPRenewal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbOTPRenewal.FormattingEnabled = true;
            this.cbOTPRenewal.Location = new System.Drawing.Point(262, 28);
            this.cbOTPRenewal.Margin = new System.Windows.Forms.Padding(2);
            this.cbOTPRenewal.Name = "cbOTPRenewal";
            this.cbOTPRenewal.Size = new System.Drawing.Size(396, 24);
            this.cbOTPRenewal.TabIndex = 160;
            // 
            // gbAutotype
            // 
            this.gbAutotype.Controls.Add(this.pPlaceholder);
            this.gbAutotype.Controls.Add(this.pHotkey);
            this.gbAutotype.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbAutotype.Location = new System.Drawing.Point(9, 8);
            this.gbAutotype.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gbAutotype.Name = "gbAutotype";
            this.gbAutotype.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gbAutotype.Size = new System.Drawing.Size(668, 104);
            this.gbAutotype.TabIndex = 12;
            this.gbAutotype.TabStop = false;
            this.gbAutotype.Text = "gbAutotype";
            // 
            // pPlaceholder
            // 
            this.pPlaceholder.Controls.Add(this.llHotKeyUnix);
            this.pPlaceholder.Controls.Add(this.cbAutoSubmit);
            this.pPlaceholder.Controls.Add(this.tbPlaceholder);
            this.pPlaceholder.Controls.Add(this.lPlaceholder);
            this.pPlaceholder.Dock = System.Windows.Forms.DockStyle.Top;
            this.pPlaceholder.Location = new System.Drawing.Point(2, 46);
            this.pPlaceholder.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pPlaceholder.Name = "pPlaceholder";
            this.pPlaceholder.Size = new System.Drawing.Size(664, 52);
            this.pPlaceholder.TabIndex = 13;
            // 
            // llHotKeyUnix
            // 
            this.llHotKeyUnix.AutoSize = true;
            this.llHotKeyUnix.Location = new System.Drawing.Point(0, 25);
            this.llHotKeyUnix.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.llHotKeyUnix.Name = "llHotKeyUnix";
            this.llHotKeyUnix.Size = new System.Drawing.Size(83, 16);
            this.llHotKeyUnix.TabIndex = 141;
            this.llHotKeyUnix.TabStop = true;
            this.llHotKeyUnix.Text = "llHotKeyUnix";
            this.llHotKeyUnix.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llHotKeyUnix_LinkClicked);
            // 
            // cbAutoSubmit
            // 
            this.cbAutoSubmit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAutoSubmit.AutoSize = true;
            this.cbAutoSubmit.Location = new System.Drawing.Point(271, 25);
            this.cbAutoSubmit.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cbAutoSubmit.Name = "cbAutoSubmit";
            this.cbAutoSubmit.Size = new System.Drawing.Size(138, 20);
            this.cbAutoSubmit.TabIndex = 140;
            this.cbAutoSubmit.Text = "Placholder + Enter";
            this.cbAutoSubmit.UseVisualStyleBackColor = true;
            // 
            // tbPlaceholder
            // 
            this.tbPlaceholder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbPlaceholder.Location = new System.Drawing.Point(263, 0);
            this.tbPlaceholder.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tbPlaceholder.Name = "tbPlaceholder";
            this.tbPlaceholder.Size = new System.Drawing.Size(232, 22);
            this.tbPlaceholder.TabIndex = 130;
            this.tbPlaceholder.TextChanged += new System.EventHandler(this.tbPlaceholder_TextChanged);
            // 
            // lPlaceholder
            // 
            this.lPlaceholder.AutoSize = true;
            this.lPlaceholder.Location = new System.Drawing.Point(0, 3);
            this.lPlaceholder.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lPlaceholder.Name = "lPlaceholder";
            this.lPlaceholder.Size = new System.Drawing.Size(83, 16);
            this.lPlaceholder.TabIndex = 0;
            this.lPlaceholder.Text = "Placeholder:";
            // 
            // pHotkey
            // 
            this.pHotkey.Controls.Add(this.cbLocalHotkey);
            this.pHotkey.Controls.Add(this.hkcKPOTP);
            this.pHotkey.Controls.Add(this.lHotkey);
            this.pHotkey.Dock = System.Windows.Forms.DockStyle.Top;
            this.pHotkey.Location = new System.Drawing.Point(2, 18);
            this.pHotkey.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pHotkey.Name = "pHotkey";
            this.pHotkey.Size = new System.Drawing.Size(664, 28);
            this.pHotkey.TabIndex = 12;
            // 
            // cbLocalHotkey
            // 
            this.cbLocalHotkey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbLocalHotkey.Location = new System.Drawing.Point(498, 3);
            this.cbLocalHotkey.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cbLocalHotkey.Name = "cbLocalHotkey";
            this.cbLocalHotkey.Size = new System.Drawing.Size(160, 18);
            this.cbLocalHotkey.TabIndex = 120;
            this.cbLocalHotkey.Text = "Copy instead of auto-type";
            this.cbLocalHotkey.UseVisualStyleBackColor = true;
            // 
            // hkcKPOTP
            // 
            this.hkcKPOTP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.hkcKPOTP.Location = new System.Drawing.Point(263, 0);
            this.hkcKPOTP.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.hkcKPOTP.Name = "hkcKPOTP";
            this.hkcKPOTP.Size = new System.Drawing.Size(232, 22);
            this.hkcKPOTP.TabIndex = 100;
            // 
            // lHotkey
            // 
            this.lHotkey.AutoSize = true;
            this.lHotkey.Location = new System.Drawing.Point(0, 3);
            this.lHotkey.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lHotkey.Name = "lHotkey";
            this.lHotkey.Size = new System.Drawing.Size(53, 16);
            this.lHotkey.TabIndex = 0;
            this.lHotkey.Text = "Hotkey:";
            // 
            // tpDatabases
            // 
            this.tpDatabases.Controls.Add(this.pButtons);
            this.tpDatabases.Controls.Add(this.tlp);
            this.tpDatabases.Location = new System.Drawing.Point(4, 25);
            this.tpDatabases.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tpDatabases.Name = "tpDatabases";
            this.tpDatabases.Padding = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.tpDatabases.Size = new System.Drawing.Size(686, 319);
            this.tpDatabases.TabIndex = 0;
            this.tpDatabases.Text = "Settings";
            this.tpDatabases.UseVisualStyleBackColor = true;
            // 
            // pButtons
            // 
            this.pButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pButtons.AutoSize = true;
            this.pButtons.Controls.Add(this.bMigrate2EntryOrDB);
            this.pButtons.Controls.Add(this.bChangeMasterKey);
            this.pButtons.Controls.Add(this.bExport);
            this.pButtons.Controls.Add(this.pDropdown);
            this.pButtons.Location = new System.Drawing.Point(324, 5);
            this.pButtons.Margin = new System.Windows.Forms.Padding(0);
            this.pButtons.Name = "pButtons";
            this.pButtons.Size = new System.Drawing.Size(351, 144);
            this.pButtons.TabIndex = 2;
            // 
            // bMigrate2EntryOrDB
            // 
            this.bMigrate2EntryOrDB.AutoSize = true;
            this.bMigrate2EntryOrDB.Dock = System.Windows.Forms.DockStyle.Top;
            this.bMigrate2EntryOrDB.Location = new System.Drawing.Point(0, 88);
            this.bMigrate2EntryOrDB.Margin = new System.Windows.Forms.Padding(0);
            this.bMigrate2EntryOrDB.Name = "bMigrate2EntryOrDB";
            this.bMigrate2EntryOrDB.Size = new System.Drawing.Size(351, 32);
            this.bMigrate2EntryOrDB.TabIndex = 160;
            this.bMigrate2EntryOrDB.Text = "bMigrate2EntryOrDB";
            this.bMigrate2EntryOrDB.UseVisualStyleBackColor = true;
            this.bMigrate2EntryOrDB.Click += new System.EventHandler(this.bMigrate2EntryOrDB_Click);
            // 
            // bChangeMasterKey
            // 
            this.bChangeMasterKey.AutoSize = true;
            this.bChangeMasterKey.Dock = System.Windows.Forms.DockStyle.Top;
            this.bChangeMasterKey.Location = new System.Drawing.Point(0, 56);
            this.bChangeMasterKey.Margin = new System.Windows.Forms.Padding(0);
            this.bChangeMasterKey.Name = "bChangeMasterKey";
            this.bChangeMasterKey.Size = new System.Drawing.Size(351, 32);
            this.bChangeMasterKey.TabIndex = 130;
            this.bChangeMasterKey.Text = "bChangeMasterKey";
            this.bChangeMasterKey.UseVisualStyleBackColor = true;
            this.bChangeMasterKey.Click += new System.EventHandler(this.bChangeMasterKey_Click);
            // 
            // bExport
            // 
            this.bExport.AutoSize = true;
            this.bExport.Dock = System.Windows.Forms.DockStyle.Top;
            this.bExport.Location = new System.Drawing.Point(0, 24);
            this.bExport.Margin = new System.Windows.Forms.Padding(0);
            this.bExport.Name = "bExport";
            this.bExport.Size = new System.Drawing.Size(351, 32);
            this.bExport.TabIndex = 150;
            this.bExport.Text = "bExport";
            this.bExport.UseVisualStyleBackColor = true;
            this.bExport.Click += new System.EventHandler(this.bExport_Click);
            // 
            // pDropdown
            // 
            this.pDropdown.Controls.Add(this.cbDBAction);
            this.pDropdown.Controls.Add(this.bCreateOpen);
            this.pDropdown.Dock = System.Windows.Forms.DockStyle.Top;
            this.pDropdown.Location = new System.Drawing.Point(0, 0);
            this.pDropdown.Margin = new System.Windows.Forms.Padding(0);
            this.pDropdown.Name = "pDropdown";
            this.pDropdown.Size = new System.Drawing.Size(351, 24);
            this.pDropdown.TabIndex = 171;
            // 
            // cbDBAction
            // 
            this.cbDBAction.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbDBAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDBAction.FormattingEnabled = true;
            this.cbDBAction.Location = new System.Drawing.Point(0, 0);
            this.cbDBAction.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cbDBAction.Name = "cbDBAction";
            this.cbDBAction.Size = new System.Drawing.Size(307, 24);
            this.cbDBAction.TabIndex = 110;
            // 
            // bCreateOpen
            // 
            this.bCreateOpen.Dock = System.Windows.Forms.DockStyle.Right;
            this.bCreateOpen.Location = new System.Drawing.Point(307, 0);
            this.bCreateOpen.Margin = new System.Windows.Forms.Padding(0);
            this.bCreateOpen.Name = "bCreateOpen";
            this.bCreateOpen.Size = new System.Drawing.Size(44, 24);
            this.bCreateOpen.TabIndex = 120;
            this.bCreateOpen.Text = "OK";
            this.bCreateOpen.UseVisualStyleBackColor = true;
            this.bCreateOpen.Click += new System.EventHandler(this.bCreateOpen_Click);
            // 
            // tlp
            // 
            this.tlp.ColumnCount = 2;
            this.tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlp.Controls.Add(this.lbDB, 0, 0);
            this.tlp.Controls.Add(this.pCheckboxes, 0, 1);
            this.tlp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlp.Location = new System.Drawing.Point(9, 8);
            this.tlp.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tlp.Name = "tlp";
            this.tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 144F));
            this.tlp.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlp.Size = new System.Drawing.Size(668, 303);
            this.tlp.TabIndex = 0;
            // 
            // lbDB
            // 
            this.lbDB.FormattingEnabled = true;
            this.lbDB.ItemHeight = 16;
            this.lbDB.Location = new System.Drawing.Point(2, 3);
            this.lbDB.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.lbDB.Name = "lbDB";
            this.lbDB.Size = new System.Drawing.Size(311, 132);
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
            this.pCheckboxes.Location = new System.Drawing.Point(2, 147);
            this.pCheckboxes.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pCheckboxes.Name = "pCheckboxes";
            this.pCheckboxes.Size = new System.Drawing.Size(664, 153);
            this.pCheckboxes.TabIndex = 2;
            // 
            // gMigrate
            // 
            this.gMigrate.Controls.Add(this.bMigrate);
            this.gMigrate.Controls.Add(this.cbMigrate);
            this.gMigrate.Dock = System.Windows.Forms.DockStyle.Top;
            this.gMigrate.Location = new System.Drawing.Point(0, 40);
            this.gMigrate.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gMigrate.Name = "gMigrate";
            this.gMigrate.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gMigrate.Size = new System.Drawing.Size(664, 51);
            this.gMigrate.TabIndex = 200;
            this.gMigrate.TabStop = false;
            this.gMigrate.Text = "gMigrate";
            // 
            // bMigrate
            // 
            this.bMigrate.Location = new System.Drawing.Point(369, 20);
            this.bMigrate.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.bMigrate.Name = "bMigrate";
            this.bMigrate.Size = new System.Drawing.Size(89, 24);
            this.bMigrate.TabIndex = 210;
            this.bMigrate.Text = "bMigrate";
            this.bMigrate.UseVisualStyleBackColor = true;
            this.bMigrate.Click += new System.EventHandler(this.bMigrate_Click);
            // 
            // cbMigrate
            // 
            this.cbMigrate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMigrate.FormattingEnabled = true;
            this.cbMigrate.Location = new System.Drawing.Point(6, 20);
            this.cbMigrate.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cbMigrate.Name = "cbMigrate";
            this.cbMigrate.Size = new System.Drawing.Size(358, 24);
            this.cbMigrate.TabIndex = 200;
            this.cbMigrate.SelectedIndexChanged += new System.EventHandler(this.cbMigrate_SelectedIndexChanged);
            // 
            // cbPreloadOTP
            // 
            this.cbPreloadOTP.AutoSize = true;
            this.cbPreloadOTP.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbPreloadOTP.Location = new System.Drawing.Point(0, 20);
            this.cbPreloadOTP.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cbPreloadOTP.Name = "cbPreloadOTP";
            this.cbPreloadOTP.Size = new System.Drawing.Size(664, 20);
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
            this.cbUseDBForSeeds.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cbUseDBForSeeds.Name = "cbUseDBForSeeds";
            this.cbUseDBForSeeds.Size = new System.Drawing.Size(664, 20);
            this.cbUseDBForSeeds.TabIndex = 180;
            this.cbUseDBForSeeds.Text = "cbUseDBForSeeds";
            this.cbUseDBForSeeds.UseVisualStyleBackColor = true;
            this.cbUseDBForSeeds.CheckedChanged += new System.EventHandler(this.cbUseDBForSeeds_CheckedChanged);
            // 
            // tpHelp
            // 
            this.tpHelp.Controls.Add(this.tbHelp);
            this.tpHelp.Location = new System.Drawing.Point(4, 25);
            this.tpHelp.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tpHelp.Name = "tpHelp";
            this.tpHelp.Padding = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.tpHelp.Size = new System.Drawing.Size(686, 319);
            this.tpHelp.TabIndex = 1;
            this.tpHelp.Text = "Help";
            this.tpHelp.UseVisualStyleBackColor = true;
            // 
            // tbHelp
            // 
            this.tbHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbHelp.Location = new System.Drawing.Point(9, 8);
            this.tbHelp.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tbHelp.Multiline = true;
            this.tbHelp.Name = "tbHelp";
            this.tbHelp.ReadOnly = true;
            this.tbHelp.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbHelp.Size = new System.Drawing.Size(668, 303);
            this.tbHelp.TabIndex = 0;
            // 
            // bDBSettings
            // 
            this.bDBSettings.AutoSize = true;
            this.bDBSettings.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bDBSettings.Location = new System.Drawing.Point(0, 48);
            this.bDBSettings.Margin = new System.Windows.Forms.Padding(0);
            this.bDBSettings.Name = "bDBSettings";
            this.bDBSettings.Size = new System.Drawing.Size(638, 26);
            this.bDBSettings.TabIndex = 140;
            this.bDBSettings.Text = "bDBSettings";
            this.bDBSettings.UseVisualStyleBackColor = true;
            this.bDBSettings.Click += new System.EventHandler(this.bDBSettings_Click);
            // 
            // Options
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "Options";
            this.Size = new System.Drawing.Size(694, 348);
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
            this.tpDatabases.PerformLayout();
            this.pButtons.ResumeLayout(false);
            this.pButtons.PerformLayout();
            this.pDropdown.ResumeLayout(false);
            this.tlp.ResumeLayout(false);
            this.tlp.PerformLayout();
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
		private System.Windows.Forms.Button bMigrate2EntryOrDB;
		private System.Windows.Forms.TabPage tpHelp;
		private System.Windows.Forms.TextBox tbHelp;
		private System.Windows.Forms.ToolTip toolTip1;
		internal System.Windows.Forms.CheckBox cbPreloadOTP;
		internal System.Windows.Forms.CheckBox cbUseDBForSeeds;
		private System.Windows.Forms.Button bDBSettings;
		private System.Windows.Forms.Button bExport;
		private System.Windows.Forms.Button bChangeMasterKey;
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
    private System.Windows.Forms.LinkLabel llHotKeyUnix;
    private System.Windows.Forms.Panel pDropdown;
    private System.Windows.Forms.Button bCreateOpen;
  }
}
