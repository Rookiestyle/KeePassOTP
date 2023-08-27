using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using KeePass.App;
using KeePass.DataExchange;
using KeePass.Forms;
using KeePass.Resources;
using KeePass.UI;
using KeePassLib;
using KeePassLib.Utility;
using PluginTools;
using PluginTranslation;

namespace KeePassOTP
{
  public partial class Options : UserControl
  {
    public class DBSettings
    {
      public bool UseOTPDB;
      public bool Preload;
    }
    private class DBAction
    {
      internal int Action = -1;
      internal DBAction(int action)
      {
        Action = action;
      }

      public override string ToString()
      {
        if (Action == ACTION_CREATE) return KPRes.CreateNewDatabase2;
        if (Action == ACTION_OPEN) return KPRes.OpenDatabase;
        if (Action == ACTION_CLOSE) return KPRes.Close;
        if (Action == ACTION_DELETE) return KPRes.Delete;
        return string.Empty;
      }
    }
    public Dictionary<PwDatabase, DBSettings> OTPDBSettings { get { return m_dDB; } }
    private Dictionary<PwDatabase, DBSettings> m_dDB = new Dictionary<PwDatabase, DBSettings>();

    private const int ACTION_NONE = 0;
    private const int ACTION_CREATE = 1;
    private const int ACTION_OPEN = 2;
    private const int ACTION_CLOSE = 3;
    private const int ACTION_DELETE = 4;

    private Dictionary<string, MigrationBase> m_dMigration = new Dictionary<string, MigrationBase>();
    private bool m_bReadingConfig = false;

    public Options()
    {
      InitializeComponent();

      tpGeneral.Text = KPRes.General;
      gbAutotype.Text = KPRes.ConfigureAutoType;
      cbLocalHotkey.Text = PluginTranslate.LocalHotkey;
      toolTip1.SetToolTip(cbLocalHotkey, PluginTranslate.LocalHotkeyTooltip);
      cbCheckTFA.Text = PluginTranslate.Options_CheckTFA;
      lHotkey.Text = PluginTranslate.Hotkey;
      string sUrl = KPRes.Error;
      try
      {
        Uri u = new Uri(TFASites.TFA_JSON_FILE);
        sUrl = u.Scheme + "://" + u.Host;
      }
      catch { }
      string[] aLines = string.Format(PluginTranslate.Options_Check2FA_Help, sUrl).Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
      for (int i = 0; i < aLines.Length; i++)
      {
        aLines[i] = aLines[i].Replace("{Options_CheckTFA}", PluginTranslate.Options_CheckTFA);
        aLines[i] = aLines[i].Replace("{Options_UseOTPDB}", PluginTranslate.Options_UseOTPDB);
        aLines[i] = aLines[i].Replace("{Options_PreloadOTPDB}", PluginTranslate.Options_PreloadOTPDB);
        aLines[i] = aLines[i].Replace("{Options_Migrate2DB}", PluginTranslate.Options_Migrate2DB);
        aLines[i] = aLines[i].Replace("{Options_Migrate2Entries}", PluginTranslate.Options_Migrate2Entries);
      }
      tb2FAHelp.Lines = aLines;
      lPlaceholder.Text = PluginTranslate.Placeholder;

      tpDatabases.Text = PluginTranslate.Options_OTPSettings;
      cbUseDBForSeeds.Text = PluginTranslate.Options_UseOTPDB;
      cbPreloadOTP.Text = PluginTranslate.Options_PreloadOTPDB;

      bCreateOpen.Text = KPRes.Ok;
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
      m_dMigration["KeePass"] = new MigrationKeePass();

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

      gOtherOptions.Text = KPRes.Options;
      lOTPDisplay.Text = PluginTranslate.OTPDisplayMode_label;
      cbOTPDisplay.Items.Add("OTP / " + PluginTranslate.SetupTFA);
      cbOTPDisplay.Items.Add(PluginTranslate.TFADefined + " / " + PluginTranslate.SetupTFA);
      lOTPRenewal.Text = PluginTranslate.OTPRenewal;
      cbOTPRenewal.Items.Add(PluginTranslate.OTPRenewal_Inactive);
      cbOTPRenewal.Items.Add(PluginTranslate.OTPRenewal_PreventShortDuration);
      cbOTPRenewal.Items.Add(PluginTranslate.OTPRenewal_RespectClipboardTimeout);
    }

    private OTPDAO.OTPHandler_DB m_handler = null;
    private void DBAction_Init(PwDatabase db)
    {
      if (db == null)
      {
        tpDatabases.Enabled = false;
        //bCreateOpen.Text = KPRes.CreateNewDatabase2;
        return;
      }
      RefreshHandler(db);
      int i = cbDBAction.SelectedIndex;
      cbDBAction.Items.Clear();
      cbDBAction.Items.Add(new DBAction(ACTION_NONE));
      if (!m_handler.OTPDB_Exists) cbDBAction.Items.Add(new DBAction(ACTION_CREATE));
      else
      {
        if (!m_handler.OTPDB_Opened) cbDBAction.Items.Add(new DBAction(ACTION_OPEN));
        else cbDBAction.Items.Add(new DBAction(ACTION_CLOSE));
        cbDBAction.Items.Add(new DBAction(ACTION_DELETE));
      }
      if (i < cbDBAction.Items.Count) cbDBAction.SelectedIndex = i;
      //bCreateOpen.Enabled = !m_handler.OTPDB_Exists || !m_handler.OTPDB_Opened;
      //if (m_handler.OTPDB_Exists) bCreateOpen.Text = KPRes.OpenDatabase;
      //else bCreateOpen.Text = KPRes.CreateNewDatabase2;
      //bCreateOpen.Enabled = !m_handler.OTPDB_Opened;
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
      SetOTPDisplay();
      SetOTPRenewal();

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

    private void SetOTPDisplay()
    {
      if (Config.OTPDisplay) cbOTPDisplay.SelectedIndex = 0;
      else cbOTPDisplay.SelectedIndex = 1;
    }

    private void SetOTPRenewal()
    {
      if (Config.OTPRenewal == Config.OTPRenewal_Enum.Inactive) cbOTPRenewal.SelectedIndex = 0;
      else if (Config.OTPRenewal == Config.OTPRenewal_Enum.RespectClipboardTimeout) cbOTPRenewal.SelectedIndex = 2;
      else cbOTPRenewal.SelectedIndex = 1;
    }

    internal bool GetOTPDisplay()
    {
      return cbOTPDisplay.SelectedIndex == 0;
    }

    internal Config.OTPRenewal_Enum GetOTPRenewal()
    {
      if (cbOTPRenewal.SelectedIndex == 0) return Config.OTPRenewal_Enum.Inactive;
      else if (cbOTPRenewal.SelectedIndex == 2) return Config.OTPRenewal_Enum.RespectClipboardTimeout;
      else return Config.OTPRenewal_Enum.PreventShortDuration;
    }

    private void cbUseDBForSeeds_CheckedChanged(object sender, EventArgs e)
    {
      PwDatabase db = m_dDB.ElementAt(lbDB.SelectedIndex).Key;
      if (!cbUseDBForSeeds.Checked)
      {
        OTPDAO.OTPHandler_DB h = OTPDAO.GetOTPHandler(db);
        if ((h != null) && h.OTPDB_Exists)
        {
          DialogResult dr = DialogResult.None;
          //Delete DB if it is opened but does not contain any entries
          if (!h.OTPDB_Exists || (h.OTPDB_Opened && !h.HasEntries()))
          {
            dr = DialogResult.Yes;
          }
          else
          {
            dr = MessageBox.Show(string.Format(PluginTranslate.ConfirmOTPDBDelete, KPRes.Yes, KPRes.No),
              PluginTranslate.PluginName,
              MessageBoxButtons.YesNoCancel,
              MessageBoxIcon.Question,
              MessageBoxDefaultButton.Button2);
          }
          if (dr == DialogResult.Cancel)
          {
            cbUseDBForSeeds.CheckedChanged -= cbUseDBForSeeds_CheckedChanged;
            cbUseDBForSeeds.Checked = true;
            cbUseDBForSeeds.CheckedChanged += cbUseDBForSeeds_CheckedChanged;
            return;
          }
          if (dr == DialogResult.Yes)
          {
            h.OTPDB_Remove();
            OTPDAO.RemoveHandler(db.IOConnectionInfo.Path, true);
            OTPDAO.InitEntries(db);
          }
          else if (dr == DialogResult.No)
          {
            h.OTPDB_Close();
            OTPDAO.RemoveHandler(db.IOConnectionInfo.Path, true);
            OTPDAO.InitEntries(db);
          }
        }
      }
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
      DBAction dba = cbDBAction.SelectedItem as DBAction;
      if (dba == null) return;
      if ((dba.Action == ACTION_CREATE) || !m_handler.OTPDB_Exists)
      {
        m_handler.OTPDB_Create();
        if (m_handler.OTPDB_Exists)
        {
          bDBSettings_Click(sender, e);
        }
      }
      else if (dba.Action == ACTION_OPEN)
        m_handler.SetDB(db, true);
      else if (dba.Action == ACTION_CLOSE)
      {
        m_handler.OTPDB_Close();
        OTPDAO.RemoveHandler(db.IOConnectionInfo.Path, true);
        OTPDAO.GetOTPHandler(db);
        OTPDAO.InitEntries(db);
        KeePassOTPColumnProvider.ForceUpdate = true;
      }
      else if (dba.Action == ACTION_DELETE)
      {
        m_handler.OTPDB_Remove();
        OTPDAO.RemoveHandler(db.IOConnectionInfo.Path, true);
        OTPDAO.InitEntries(db);
        KeePassOTPColumnProvider.ForceUpdate = true;
      }

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
      if (m_handler.OTPDB_Opened) ShowResult(m_handler.MigrateOTP2Entry());
    }

    private void bMigrate2DB_Click(object sender, EventArgs e)
    {
      PwDatabase db = m_dDB.ElementAt(lbDB.SelectedIndex).Key;
      RefreshHandler(db);
      if (m_handler.OTPDB_Opened) ShowResult(m_handler.MigrateOTP2DB());
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
      //If configured, KeePass 2.46 will ask for the masterkey during the export
      //No need to ask here
      if (Tools.KeePassVersion < new Version(2, 46))
      {
        if (!AppPolicy.Current.ExportNoKey && !m_handler.ReAskKey()) return;
      }

      db = m_handler.OTPDB;
      PwGroup pg = db.RootGroup;
      PwExportInfo pwInfo = new PwExportInfo(pg, db, false);

      MessageService.ExternalIncrementMessageCount();
      ShowWarningsLogger swLogger = KeePass.Program.MainForm.CreateShowWarningsLogger();
      swLogger.StartLogging(KPRes.ExportingStatusMsg, true);
      try
      {
        ExportUtil.Export(pwInfo, swLogger);
        swLogger.SetText(string.Empty, KeePassLib.Interfaces.LogStatusType.Info);
      }
      finally
      {
        swLogger.EndLogging();
        MessageService.ExternalDecrementMessageCount();
      }
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
      {
        if ((c != bCreateOpen) && (c != cbDBAction))
          c.Width = pButtons.ClientSize.Width;
      }
      cbDBAction.Width = bDBSettings.Width - bCreateOpen.Width - 5;
      cbDBAction.DropDownWidth = pButtons.ClientSize.Width;
      cbAutoSubmit.Left = hkcKPOTP.Left;
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

    private void tbPlaceholder_TextChanged(object sender, EventArgs e)
    {
      cbAutoSubmit.Enabled = tbPlaceholder.Text.StartsWith("{") && tbPlaceholder.Text.EndsWith("}");
      cbAutoSubmit.Text = string.Format(PluginTranslate.PlaceholderAutoSubmit, tbPlaceholder.Text);
    }
  }
}
