using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using KeePass.Resources;
using PluginTranslation;
using KeePassLib;
using KeePassLib.Utility;
using PluginTools;
using KeePass.DataExchange;
using KeePass.UI;
using KeePass.App;
using KeePass.Forms;

namespace KeePassOTP
{
	public partial class Options : UserControl
	{
		public class DBSettings
		{
			public bool UseOTPDB;
			public bool Preload;
		}

		public Dictionary<PwDatabase, DBSettings> OTPDBSettings {  get { return m_dDB; } }
		private Dictionary<PwDatabase, DBSettings> m_dDB = new Dictionary<PwDatabase, DBSettings>();

		private const int ACTION_CREATE = 1;
		private const int ACTION_OPEN = 2;
		private const int ACTION_MASTERKEY = 3;
		private const int ACTION_EXPORT = 4;
		private const int ACTION_SETTINGS = 5;

		private Dictionary<string, MigrationBase> m_dMigration = new Dictionary<string, MigrationBase>();
		private bool m_bReadingConfig = false;

		public Options()
		{
			InitializeComponent();

			tpGeneral.Text = KPRes.General;
			gbAutotype.Text = KPRes.ConfigureAutoType;
			cgbCheckTFA.Text = PluginTranslate.Options_CheckTFA;
			lHotkey.Text = PluginTranslate.Hotkey;
			string[] aLines = PluginTranslate.Options_Check2FA_Help.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
			for (int i = 0; i < aLines.Length; i++)
			{
				aLines[i] = aLines[i].Replace("{Options_CheckTFA}", PluginTranslate.Options_CheckTFA);
				aLines[i] = aLines[i].Replace("{Options_UseOTPDB}", PluginTranslate.Options_UseOTPDB);
				aLines[i] = aLines[i].Replace("{Options_PreloadOTPDB}", PluginTranslate.Options_PreloadOTPDB);
				aLines[i] = aLines[i].Replace("{Options_Migrate2DB}", PluginTranslate.Options_Migrate2DB);
				aLines[i] = aLines[i].Replace("{Options_Migrate2Entries}", PluginTranslate.Options_Migrate2Entries);
			}
			tb2FAHelp.Lines = aLines;


			tpDatabases.Text = PluginTranslate.Options_OTPSettings;
			cbUseDBForSeeds.Text = PluginTranslate.Options_UseOTPDB;
			cbPreloadOTP.Text = PluginTranslate.Options_PreloadOTPDB;

			bCreateOpen.Text = KPRes.CreateNewDatabase2;
			bChangeMasterKey.Text = KPRes.ChangeMasterKey;
			bDBSettings.Text = KPRes.DatabaseSettings;
			bExport.Text = KPRes.Export;
			bMigrate2Entry.Text = PluginTranslate.Options_Migrate2Entries;
			bMigrate2DB.Text = PluginTranslate.Options_Migrate2DB;
			toolTip1.SetToolTip(bMigrate2DB, PluginTranslate.Options_Migrate2DBHint);
			bMigrate2Entry.Text = PluginTranslate.Options_Migrate2Entries;
			toolTip1.SetToolTip(bMigrate2Entry, PluginTranslate.Options_Migrate2EntriesHint);

			m_dMigration["KeeOTP"] = new MigrationKeeOTP();
			m_dMigration["KeeTrayTOTP"] = new MigrationTraytotp();

			int idx = 1;
			foreach (string s in m_dMigration.Keys)
			{
				cbMigrate.Items.Add(s + " -> KeePassOTP");
				cbMigrate.Items.Add("KeePassOTP -> " + s);
				if (idx++ < m_dMigration.Count)
					cbMigrate.Items.Add(string.Empty);
			}
			cbMigrate_SelectedIndexChanged(null, null);
			bMigrate.Text = KPRes.Ok;
			gMigrate.Text = PluginTranslate.MigrateOtherPlugins;

			tpHelp.Text = KPRes.MoreInfo;
			aLines = PluginTranslate.Options_Help.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
			for (int i = 0; i < aLines.Length; i++)
			{
				aLines[i] = aLines[i].Replace("{Options_CheckTFA}", PluginTranslate.Options_CheckTFA);
				aLines[i] = aLines[i].Replace("{Options_UseOTPDB}", PluginTranslate.Options_UseOTPDB);
				aLines[i] = aLines[i].Replace("{Options_PreloadOTPDB}", PluginTranslate.Options_PreloadOTPDB);
				aLines[i] = aLines[i].Replace("{Options_Migrate2DB}", PluginTranslate.Options_Migrate2DB);
				aLines[i] = aLines[i].Replace("{Options_Migrate2Entries}", PluginTranslate.Options_Migrate2Entries);
			}
			tbHelp.Lines = aLines;
		}

		private OTPDAO.OTPHandler_DB m_handler = null;
		private void DBAction_Init(PwDatabase db)
		{
			if (db == null)
			{
				tpDatabases.Enabled = false;
				bCreateOpen.Text = KPRes.CreateNewDatabase2;
				return;
			}
			RefreshHandler(db);
			bCreateOpen.Enabled = !m_handler.OTPDB_Exists || !m_handler.OTPDB_Opened;
			if (m_handler.OTPDB_Exists) bCreateOpen.Text = KPRes.OpenDatabase;
			else bCreateOpen.Text = KPRes.CreateNewDatabase2;
			bCreateOpen.Enabled = !m_handler.OTPDB_Opened;
			bChangeMasterKey.Enabled = m_handler.OTPDB_Opened;
			bExport.Enabled = m_handler.OTPDB_Opened;
			bDBSettings.Enabled = m_handler.OTPDB_Opened;
			bMigrate2DB.Enabled = m_handler.OTPDB_Opened;
			bMigrate2Entry.Enabled = m_handler.OTPDB_Opened;
		}

		private void RefreshHandler(PwDatabase db)
		{
			Config.UseDBForOTPSeeds(db, cbUseDBForSeeds.Checked);
			Config.PreloadOTPDB(db, cbPreloadOTP.Checked);
			m_handler = OTPDAO.GetOTPHandler(db) as OTPDAO.OTPHandler_DB;
			if (m_handler == null)
			{
				m_handler = new OTPDAO.OTPHandler_DB();
				m_handler.SetDB(db, false);
			}
		}

		public void InitEx(Dictionary<PwDatabase, DBSettings> dDB, PwDatabase current)
		{
			bool bUnix = KeePassLib.Native.NativeLib.IsUnix();
			hkcKPOTP.Enabled = !bUnix;
			lHotkey.Enabled = !bUnix;
			lbDB.Items.Clear();
			m_dDB.Clear();
			if ((dDB == null) || dDB.Count == 0)
			{
				tpDatabases.Enabled = false;
				return;
			}
			int idx = -1;
			foreach (KeyValuePair<PwDatabase, DBSettings> kvp in dDB)
			{
				m_dDB[kvp.Key] = kvp.Value;
				if (kvp.Key == current) idx = lbDB.Items.Count;
				lbDB.Items.Add(GetFriendlyName(kvp.Key));
			}
			if (idx > -1) lbDB.SelectedIndex = idx;
			else lbDB_SelectedIndexChanged(null, null);
		}

		private void cbUseDBForSeeds_CheckedChanged(object sender, EventArgs e)
		{
			PwDatabase db = m_dDB.ElementAt(lbDB.SelectedIndex).Key;
			m_dDB[db].UseOTPDB = cbUseDBForSeeds.Checked;
			cbPreloadOTP.Enabled = cbUseDBForSeeds.Checked;
			DBAction_Init(db);
		}

		private string GetFriendlyName(PwDatabase db)
		{
			if (!string.IsNullOrEmpty(db.Name)) return db.Name;
			return UrlUtil.GetFileName(db.IOConnectionInfo.Path);
		}

		private void lbDB_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (lbDB.SelectedIndex < 0)
			{
				DBAction_Init(null);
				return;
			}
			PwDatabase db = m_dDB.ElementAt(lbDB.SelectedIndex).Key;

			m_bReadingConfig = true;
			cbPreloadOTP.Checked = m_dDB[db].Preload;
			m_bReadingConfig = false;
			cbUseDBForSeeds.Checked = m_dDB[db].UseOTPDB;
			
			DBAction_Init(db);
	}

		private void bCreateOpen_Click(object sender, EventArgs e)
		{
			cbUseDBForSeeds.Checked = true;
			PwDatabase db = m_dDB.ElementAt(lbDB.SelectedIndex).Key;
			RefreshHandler(db);
			if (!m_handler.OTPDB_Exists)
			{
				m_handler.OTPDB_Create();
				if (m_handler.OTPDB_Exists)
				{
					bDBSettings_Click(sender, e);
				}
			}
			else if (!m_handler.OTPDB_Opened)
				m_handler.SetDB(db, true);
			if (m_handler.OTPDB_Opened)
			{
				cbUseDBForSeeds.Checked = true;
				Config.UseDBForOTPSeeds(db, true);
				OTPDAO.GetOTPHandler(db);
				OTPDAO.InitEntries(db);
			}
			lbDB_SelectedIndexChanged(sender, e);
		}

		private void bChangeMasterKey_Click(object sender, EventArgs e)
		{
			PwDatabase db = m_dDB.ElementAt(lbDB.SelectedIndex).Key;
			RefreshHandler(db);
			if (m_handler.OTPDB_Opened) m_handler.OTPDB_ChangePassword(true);
		}

		private void bMigrate2Entry_Click(object sender, EventArgs e)
		{
			PwDatabase db = m_dDB.ElementAt(lbDB.SelectedIndex).Key;
			RefreshHandler(db);
			if (m_handler.OTPDB_Opened)	ShowResult(m_handler.MigrateOTP2Entry());
		}

		private void bMigrate2DB_Click(object sender, EventArgs e)
		{
			PwDatabase db = m_dDB.ElementAt(lbDB.SelectedIndex).Key;
			RefreshHandler(db);
			if (m_handler.OTPDB_Opened)	ShowResult(m_handler.MigrateOTP2DB());
		}

		private void ShowResult(int moved)
		{
			if (moved < 0) Tools.ShowError(PluginTranslate.MoveError);
			else Tools.ShowInfo(string.Format(PluginTranslate.EntriesMoved, moved));
		}

		private void bExport_Click(object sender, EventArgs e)
		{
			PwDatabase db = m_dDB.ElementAt(lbDB.SelectedIndex).Key;
			RefreshHandler(db);
			//if (!m_handler.SetDB(db, false)) return;
			if (!AppPolicy.Current.ExportNoKey && !m_handler.ReAskKey()) return;

			db = m_handler.OTPDB;
			PwGroup pg = db.RootGroup;
			PwExportInfo pwInfo = new PwExportInfo(pg, db, false);

			MessageService.ExternalIncrementMessageCount();
			ShowWarningsLogger swLogger = KeePass.Program.MainForm.CreateShowWarningsLogger();
			swLogger.StartLogging(KPRes.ExportingStatusMsg, true);

			ExportUtil.Export(pwInfo, swLogger);
			swLogger.SetText(string.Empty, KeePassLib.Interfaces.LogStatusType.Info);
			swLogger.EndLogging();
			MessageService.ExternalDecrementMessageCount();
		}

		private void cbPreloadOTP_CheckedChanged(object sender, EventArgs e)
		{
			if (m_bReadingConfig) return;
			PwDatabase db = m_dDB.ElementAt(lbDB.SelectedIndex).Key;
			m_dDB[db].Preload = cbPreloadOTP.Checked;
			DBAction_Init(db);
		}

		private void bDBSettings_Click(object sender, EventArgs e)
		{
			PwDatabase db = m_dDB.ElementAt(lbDB.SelectedIndex).Key;
			RefreshHandler(db);
			if (!m_handler.OTPDB_Opened) return;
			PwDatabase kpotpdb = m_handler.OTPDB;
			if (kpotpdb == null) return;

			DatabaseSettingsForm dsf = new DatabaseSettingsForm();
			dsf.InitEx(false, kpotpdb);
			dsf.Shown += DBSettings_Shown;
			if (UIUtil.ShowDialogAndDestroy(dsf) == DialogResult.OK)
			{
				m_handler.FlagChanged(true);
				m_handler.FlagChanged(false);
				PluginDebug.AddInfo("Changed OTP Db settings", 0);
				//KeePass.Program.MainForm.RefreshEntriesList(); // History items might have been deleted
			}
		}

		private void DBSettings_Shown(object sender, EventArgs e)
		{
			DatabaseSettingsForm dsf = sender as DatabaseSettingsForm;
			if (dsf == null) return;

			DeactivateControl("m_tabGeneral", dsf);
			DeactivateControl("m_tabRecycleBin", dsf);
			DeactivateControl("m_grpTemplates", dsf);
			DeactivateControl("m_grpMasterKey", dsf);

			TabPage tpSec = Tools.GetControl("m_tabSecurity", dsf) as TabPage;
			if (tpSec == null) return;
			TabControl tc = tpSec.Parent as TabControl;
			tc.SelectedIndex = tc.TabPages.IndexOf(tpSec);
		}

		private void DeactivateControl(string v, Form f)
		{
			Control c = Tools.GetControl(v, f);
			if (c == null) return;
			c.Enabled = false;
		}

		private void Options_Resize(object sender, EventArgs e)
		{
			tlp.ColumnStyles[1].SizeType = SizeType.AutoSize;
			int w = 0;
			foreach (Control c in pButtons.Controls)
				w = Math.Max(w, c.Width);
			w += 10;
			w = Math.Min(w, tlp.ClientSize.Width / 2);
			tlp.ColumnStyles[1].SizeType = SizeType.Absolute;
			tlp.ColumnStyles[1].Width = w;
			foreach (Control c in pButtons.Controls)
				c.Width = pButtons.ClientSize.Width;
		}

		private void bMigrate_Click(object sender, EventArgs e)
		{
			PwDatabase db = m_dDB.ElementAt(lbDB.SelectedIndex).Key;
			bool bToKeePassOTP = cbMigrate.SelectedItem.ToString().EndsWith("KeePassOTP");
			string sPlugin = PluginTranslate.PluginName;

			MigrationBase mBase = null;
			string sel = cbMigrate.SelectedItem.ToString();
			foreach (KeyValuePair<string, MigrationBase> kvp in m_dMigration)
			{
				if (sel.Contains(kvp.Key))
				{
					mBase = kvp.Value;
					if (bToKeePassOTP) sPlugin = kvp.Key;
					break;
				}
			}
			if (mBase == null) return;

			bool bRemove = Tools.AskYesNo(string.Format(PluginTranslate.MigrateOtherPlugins_Delete, sPlugin)) == DialogResult.Yes;

			int EntriesOverall;
			int EntriesMigrated;

			mBase.SetDB(db);
			if (bToKeePassOTP)
				mBase.MigrateToKeePassOTP(bRemove, out EntriesOverall, out EntriesMigrated);
			else
				mBase.MigrateFromKeePassOTP(bRemove, out EntriesOverall, out EntriesMigrated);

			if ((EntriesMigrated != -1) && (EntriesOverall != -1))
			{
				OTPDAO.OTPHandler_DB h = OTPDAO.GetOTPHandler(db) as OTPDAO.OTPHandler_DB;
				if (h != null) h.OTPDB_SaveAfterMigration();
				Tools.ShowInfo(string.Format(PluginTranslate.MigrateOtherPlugins_Result, EntriesMigrated, EntriesOverall));
			}
		}

		private void cbMigrate_SelectedIndexChanged(object sender, EventArgs e)
		{
			bMigrate.Enabled = (cbMigrate.SelectedIndex > -1) && ((cbMigrate.SelectedIndex + 1) % 3 != 0);
		}
	}
}