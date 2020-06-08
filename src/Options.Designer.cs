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
			this.tpDatabases = new System.Windows.Forms.TabPage();
			this.tlp = new System.Windows.Forms.TableLayoutPanel();
			this.pButtons = new System.Windows.Forms.Panel();
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
			this.tpGeneral = new System.Windows.Forms.TabPage();
			this.cgbCheckTFA = new RookieUI.CheckedGroupBox();
			this.gbAutotype = new System.Windows.Forms.GroupBox();
			this.pPlaceholder = new System.Windows.Forms.Panel();
			this.tbPlaceholder = new System.Windows.Forms.TextBox();
			this.lPlaceholder = new System.Windows.Forms.Label();
			this.pHotkey = new System.Windows.Forms.Panel();
			this.hkcKPOTP = new KeePass.UI.HotKeyControlEx();
			this.lHotkey = new System.Windows.Forms.Label();
			this.tpHelp = new System.Windows.Forms.TabPage();
			this.tbHelp = new System.Windows.Forms.TextBox();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.tb2FAHelp = new System.Windows.Forms.TextBox();
			this.tabControl1.SuspendLayout();
			this.tpDatabases.SuspendLayout();
			this.tlp.SuspendLayout();
			this.pButtons.SuspendLayout();
			this.pCheckboxes.SuspendLayout();
			this.gMigrate.SuspendLayout();
			this.tpGeneral.SuspendLayout();
			this.cgbCheckTFA.SuspendLayout();
			this.gbAutotype.SuspendLayout();
			this.pPlaceholder.SuspendLayout();
			this.pHotkey.SuspendLayout();
			this.tpHelp.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tpDatabases);
			this.tabControl1.Controls.Add(this.tpGeneral);
			this.tabControl1.Controls.Add(this.tpHelp);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(780, 435);
			this.tabControl1.TabIndex = 2;
			// 
			// tpDatabases
			// 
			this.tpDatabases.Controls.Add(this.tlp);
			this.tpDatabases.Location = new System.Drawing.Point(4, 29);
			this.tpDatabases.Name = "tpDatabases";
			this.tpDatabases.Padding = new System.Windows.Forms.Padding(10);
			this.tpDatabases.Size = new System.Drawing.Size(772, 402);
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
			this.tlp.Location = new System.Drawing.Point(10, 10);
			this.tlp.Name = "tlp";
			this.tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 180F));
			this.tlp.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tlp.Size = new System.Drawing.Size(752, 382);
			this.tlp.TabIndex = 0;
			// 
			// pButtons
			// 
			this.pButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pButtons.AutoSize = true;
			this.pButtons.Controls.Add(this.bDBSettings);
			this.pButtons.Controls.Add(this.bExport);
			this.pButtons.Controls.Add(this.bChangeMasterKey);
			this.pButtons.Controls.Add(this.bCreateOpen);
			this.pButtons.Controls.Add(this.bMigrate2Entry);
			this.pButtons.Controls.Add(this.bMigrate2DB);
			this.pButtons.Location = new System.Drawing.Point(435, 0);
			this.pButtons.Margin = new System.Windows.Forms.Padding(0);
			this.pButtons.Name = "pButtons";
			this.pButtons.Size = new System.Drawing.Size(317, 180);
			this.pButtons.TabIndex = 2;
			// 
			// bDBSettings
			// 
			this.bDBSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.bDBSettings.AutoSize = true;
			this.bDBSettings.Location = new System.Drawing.Point(0, 60);
			this.bDBSettings.Margin = new System.Windows.Forms.Padding(0);
			this.bDBSettings.Name = "bDBSettings";
			this.bDBSettings.Size = new System.Drawing.Size(314, 30);
			this.bDBSettings.TabIndex = 74;
			this.bDBSettings.Text = "bDBSettings";
			this.bDBSettings.UseVisualStyleBackColor = true;
			this.bDBSettings.Click += new System.EventHandler(this.bDBSettings_Click);
			// 
			// bExport
			// 
			this.bExport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.bExport.AutoSize = true;
			this.bExport.Location = new System.Drawing.Point(0, 90);
			this.bExport.Margin = new System.Windows.Forms.Padding(0);
			this.bExport.Name = "bExport";
			this.bExport.Size = new System.Drawing.Size(314, 30);
			this.bExport.TabIndex = 73;
			this.bExport.Text = "bExport";
			this.bExport.UseVisualStyleBackColor = true;
			this.bExport.Click += new System.EventHandler(this.bExport_Click);
			// 
			// bChangeMasterKey
			// 
			this.bChangeMasterKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.bChangeMasterKey.AutoSize = true;
			this.bChangeMasterKey.Location = new System.Drawing.Point(0, 30);
			this.bChangeMasterKey.Margin = new System.Windows.Forms.Padding(0);
			this.bChangeMasterKey.Name = "bChangeMasterKey";
			this.bChangeMasterKey.Size = new System.Drawing.Size(314, 30);
			this.bChangeMasterKey.TabIndex = 72;
			this.bChangeMasterKey.Text = "bChangeMasterKey";
			this.bChangeMasterKey.UseVisualStyleBackColor = true;
			this.bChangeMasterKey.Click += new System.EventHandler(this.bChangeMasterKey_Click);
			// 
			// bCreateOpen
			// 
			this.bCreateOpen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.bCreateOpen.AutoSize = true;
			this.bCreateOpen.Location = new System.Drawing.Point(0, 0);
			this.bCreateOpen.Margin = new System.Windows.Forms.Padding(0);
			this.bCreateOpen.Name = "bCreateOpen";
			this.bCreateOpen.Size = new System.Drawing.Size(314, 30);
			this.bCreateOpen.TabIndex = 71;
			this.bCreateOpen.Text = "bCreateOpen";
			this.bCreateOpen.UseVisualStyleBackColor = true;
			this.bCreateOpen.Click += new System.EventHandler(this.bCreateOpen_Click);
			// 
			// bMigrate2Entry
			// 
			this.bMigrate2Entry.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.bMigrate2Entry.AutoSize = true;
			this.bMigrate2Entry.Location = new System.Drawing.Point(0, 120);
			this.bMigrate2Entry.Margin = new System.Windows.Forms.Padding(0);
			this.bMigrate2Entry.Name = "bMigrate2Entry";
			this.bMigrate2Entry.Size = new System.Drawing.Size(314, 30);
			this.bMigrate2Entry.TabIndex = 70;
			this.bMigrate2Entry.Text = "bMigrate2Entry";
			this.bMigrate2Entry.UseVisualStyleBackColor = true;
			this.bMigrate2Entry.Click += new System.EventHandler(this.bMigrate2Entry_Click);
			// 
			// bMigrate2DB
			// 
			this.bMigrate2DB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.bMigrate2DB.AutoSize = true;
			this.bMigrate2DB.Location = new System.Drawing.Point(0, 150);
			this.bMigrate2DB.Margin = new System.Windows.Forms.Padding(0);
			this.bMigrate2DB.Name = "bMigrate2DB";
			this.bMigrate2DB.Size = new System.Drawing.Size(314, 30);
			this.bMigrate2DB.TabIndex = 34;
			this.bMigrate2DB.Text = "bMigrate2DB";
			this.bMigrate2DB.UseVisualStyleBackColor = true;
			this.bMigrate2DB.Click += new System.EventHandler(this.bMigrate2DB_Click);
			// 
			// lbDB
			// 
			this.lbDB.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lbDB.FormattingEnabled = true;
			this.lbDB.ItemHeight = 20;
			this.lbDB.Location = new System.Drawing.Point(3, 3);
			this.lbDB.Name = "lbDB";
			this.lbDB.Size = new System.Drawing.Size(429, 174);
			this.lbDB.TabIndex = 20;
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
			this.pCheckboxes.Location = new System.Drawing.Point(3, 183);
			this.pCheckboxes.Name = "pCheckboxes";
			this.pCheckboxes.Size = new System.Drawing.Size(746, 196);
			this.pCheckboxes.TabIndex = 2;
			// 
			// gMigrate
			// 
			this.gMigrate.Controls.Add(this.bMigrate);
			this.gMigrate.Controls.Add(this.cbMigrate);
			this.gMigrate.Dock = System.Windows.Forms.DockStyle.Top;
			this.gMigrate.Location = new System.Drawing.Point(0, 48);
			this.gMigrate.Name = "gMigrate";
			this.gMigrate.Size = new System.Drawing.Size(746, 64);
			this.gMigrate.TabIndex = 5;
			this.gMigrate.TabStop = false;
			this.gMigrate.Text = "gMigrate";
			// 
			// bMigrate
			// 
			this.bMigrate.Location = new System.Drawing.Point(415, 25);
			this.bMigrate.Name = "bMigrate";
			this.bMigrate.Size = new System.Drawing.Size(100, 30);
			this.bMigrate.TabIndex = 1;
			this.bMigrate.Text = "bMigrate";
			this.bMigrate.UseVisualStyleBackColor = true;
			this.bMigrate.Click += new System.EventHandler(this.bMigrate_Click);
			// 
			// cbMigrate
			// 
			this.cbMigrate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbMigrate.FormattingEnabled = true;
			this.cbMigrate.Location = new System.Drawing.Point(6, 25);
			this.cbMigrate.Name = "cbMigrate";
			this.cbMigrate.Size = new System.Drawing.Size(403, 28);
			this.cbMigrate.TabIndex = 0;
			this.cbMigrate.SelectedIndexChanged += new System.EventHandler(this.cbMigrate_SelectedIndexChanged);
			// 
			// cbPreloadOTP
			// 
			this.cbPreloadOTP.AutoSize = true;
			this.cbPreloadOTP.Dock = System.Windows.Forms.DockStyle.Top;
			this.cbPreloadOTP.Location = new System.Drawing.Point(0, 24);
			this.cbPreloadOTP.Name = "cbPreloadOTP";
			this.cbPreloadOTP.Size = new System.Drawing.Size(746, 24);
			this.cbPreloadOTP.TabIndex = 4;
			this.cbPreloadOTP.Text = "cbPreloadOTP";
			this.cbPreloadOTP.UseVisualStyleBackColor = true;
			this.cbPreloadOTP.CheckedChanged += new System.EventHandler(this.cbPreloadOTP_CheckedChanged);
			// 
			// cbUseDBForSeeds
			// 
			this.cbUseDBForSeeds.AutoSize = true;
			this.cbUseDBForSeeds.Dock = System.Windows.Forms.DockStyle.Top;
			this.cbUseDBForSeeds.Location = new System.Drawing.Point(0, 0);
			this.cbUseDBForSeeds.Name = "cbUseDBForSeeds";
			this.cbUseDBForSeeds.Size = new System.Drawing.Size(746, 24);
			this.cbUseDBForSeeds.TabIndex = 3;
			this.cbUseDBForSeeds.Text = "cbUseDBForSeeds";
			this.cbUseDBForSeeds.UseVisualStyleBackColor = true;
			this.cbUseDBForSeeds.CheckedChanged += new System.EventHandler(this.cbUseDBForSeeds_CheckedChanged);
			// 
			// tpGeneral
			// 
			this.tpGeneral.Controls.Add(this.cgbCheckTFA);
			this.tpGeneral.Controls.Add(this.gbAutotype);
			this.tpGeneral.Location = new System.Drawing.Point(4, 29);
			this.tpGeneral.Name = "tpGeneral";
			this.tpGeneral.Padding = new System.Windows.Forms.Padding(10);
			this.tpGeneral.Size = new System.Drawing.Size(772, 402);
			this.tpGeneral.TabIndex = 0;
			this.tpGeneral.Text = "General";
			this.tpGeneral.UseVisualStyleBackColor = true;
			// 
			// cgbCheckTFA
			// 
			this.cgbCheckTFA.AutoSize = true;
			this.cgbCheckTFA.CheckboxOffset = new System.Drawing.Point(6, 0);
			this.cgbCheckTFA.Checked = true;
			this.cgbCheckTFA.Controls.Add(this.tb2FAHelp);
			this.cgbCheckTFA.DisableControlsIfUnchecked = false;
			this.cgbCheckTFA.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cgbCheckTFA.Location = new System.Drawing.Point(10, 110);
			this.cgbCheckTFA.Name = "cgbCheckTFA";
			this.cgbCheckTFA.Size = new System.Drawing.Size(752, 282);
			this.cgbCheckTFA.TabIndex = 10;
			this.cgbCheckTFA.Text = "cgbCheckTFA";
			// 
			// gbAutotype
			// 
			this.gbAutotype.Controls.Add(this.pPlaceholder);
			this.gbAutotype.Controls.Add(this.pHotkey);
			this.gbAutotype.Dock = System.Windows.Forms.DockStyle.Top;
			this.gbAutotype.Location = new System.Drawing.Point(10, 10);
			this.gbAutotype.Name = "gbAutotype";
			this.gbAutotype.Size = new System.Drawing.Size(752, 100);
			this.gbAutotype.TabIndex = 12;
			this.gbAutotype.TabStop = false;
			this.gbAutotype.Text = "gbAutotype";
			// 
			// pPlaceholder
			// 
			this.pPlaceholder.Controls.Add(this.tbPlaceholder);
			this.pPlaceholder.Controls.Add(this.lPlaceholder);
			this.pPlaceholder.Dock = System.Windows.Forms.DockStyle.Top;
			this.pPlaceholder.Location = new System.Drawing.Point(3, 57);
			this.pPlaceholder.Name = "pPlaceholder";
			this.pPlaceholder.Size = new System.Drawing.Size(746, 35);
			this.pPlaceholder.TabIndex = 13;
			// 
			// tbPlaceholder
			// 
			this.tbPlaceholder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.tbPlaceholder.Location = new System.Drawing.Point(294, 0);
			this.tbPlaceholder.Name = "tbPlaceholder";
			this.tbPlaceholder.Size = new System.Drawing.Size(260, 26);
			this.tbPlaceholder.TabIndex = 1;
			// 
			// lPlaceholder
			// 
			this.lPlaceholder.AutoSize = true;
			this.lPlaceholder.Location = new System.Drawing.Point(0, 3);
			this.lPlaceholder.Name = "lPlaceholder";
			this.lPlaceholder.Size = new System.Drawing.Size(96, 20);
			this.lPlaceholder.TabIndex = 0;
			this.lPlaceholder.Text = "Placeholder:";
			// 
			// pHotkey
			// 
			this.pHotkey.Controls.Add(this.hkcKPOTP);
			this.pHotkey.Controls.Add(this.lHotkey);
			this.pHotkey.Dock = System.Windows.Forms.DockStyle.Top;
			this.pHotkey.Location = new System.Drawing.Point(3, 22);
			this.pHotkey.Name = "pHotkey";
			this.pHotkey.Size = new System.Drawing.Size(746, 35);
			this.pHotkey.TabIndex = 12;
			// 
			// hkcKPOTP
			// 
			this.hkcKPOTP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.hkcKPOTP.Location = new System.Drawing.Point(294, 0);
			this.hkcKPOTP.Name = "hkcKPOTP";
			this.hkcKPOTP.Size = new System.Drawing.Size(260, 26);
			this.hkcKPOTP.TabIndex = 1;
			// 
			// lHotkey
			// 
			this.lHotkey.AutoSize = true;
			this.lHotkey.Location = new System.Drawing.Point(0, 3);
			this.lHotkey.Name = "lHotkey";
			this.lHotkey.Size = new System.Drawing.Size(63, 20);
			this.lHotkey.TabIndex = 0;
			this.lHotkey.Text = "Hotkey:";
			// 
			// tpHelp
			// 
			this.tpHelp.Controls.Add(this.tbHelp);
			this.tpHelp.Location = new System.Drawing.Point(4, 29);
			this.tpHelp.Name = "tpHelp";
			this.tpHelp.Padding = new System.Windows.Forms.Padding(10);
			this.tpHelp.Size = new System.Drawing.Size(772, 402);
			this.tpHelp.TabIndex = 1;
			this.tpHelp.Text = "Help";
			this.tpHelp.UseVisualStyleBackColor = true;
			// 
			// tbHelp
			// 
			this.tbHelp.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tbHelp.Location = new System.Drawing.Point(10, 10);
			this.tbHelp.Multiline = true;
			this.tbHelp.Name = "tbHelp";
			this.tbHelp.ReadOnly = true;
			this.tbHelp.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tbHelp.Size = new System.Drawing.Size(752, 382);
			this.tbHelp.TabIndex = 0;
			// 
			// tb2FAHelp
			// 
			this.tb2FAHelp.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tb2FAHelp.Location = new System.Drawing.Point(3, 22);
			this.tb2FAHelp.Multiline = true;
			this.tb2FAHelp.Name = "tb2FAHelp";
			this.tb2FAHelp.ReadOnly = true;
			this.tb2FAHelp.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tb2FAHelp.Size = new System.Drawing.Size(746, 257);
			this.tb2FAHelp.TabIndex = 14;
			// 
			// Options
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.Controls.Add(this.tabControl1);
			this.Name = "Options";
			this.Size = new System.Drawing.Size(780, 435);
			this.Resize += new System.EventHandler(this.Options_Resize);
			this.tabControl1.ResumeLayout(false);
			this.tpDatabases.ResumeLayout(false);
			this.tlp.ResumeLayout(false);
			this.tlp.PerformLayout();
			this.pButtons.ResumeLayout(false);
			this.pButtons.PerformLayout();
			this.pCheckboxes.ResumeLayout(false);
			this.pCheckboxes.PerformLayout();
			this.gMigrate.ResumeLayout(false);
			this.tpGeneral.ResumeLayout(false);
			this.tpGeneral.PerformLayout();
			this.cgbCheckTFA.ResumeLayout(false);
			this.cgbCheckTFA.PerformLayout();
			this.gbAutotype.ResumeLayout(false);
			this.pPlaceholder.ResumeLayout(false);
			this.pPlaceholder.PerformLayout();
			this.pHotkey.ResumeLayout(false);
			this.pHotkey.PerformLayout();
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
		internal RookieUI.CheckedGroupBox cgbCheckTFA;
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
		private System.Windows.Forms.TextBox tb2FAHelp;
	}
}