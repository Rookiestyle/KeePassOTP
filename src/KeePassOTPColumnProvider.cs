using KeePass.UI;
using KeePassLib;
using PluginTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeePassOTP
{
	internal class KeePassOTPColumnProvider : ColumnProvider
	{
		public const string KPOTPColumnName = "KPOTP";
		private static Dictionary<string, bool> m_dTFAPossible = new Dictionary<string, bool>();
		private System.Timers.Timer m_columnRefreshTimer = new System.Timers.Timer();

		private readonly string[] ColumnName = new[] { KPOTPColumnName };

		public override string[] ColumnNames { get { return ColumnName; } }

		public override string GetCellData(string strColumnName, PwEntry pe)
		{
			if (strColumnName == null) return string.Empty;
			if (pe == null) return string.Empty;
			Random r = new Random();

			string otp = string.Empty;
			System.Diagnostics.StackFrame sf = new System.Diagnostics.StackTrace().GetFrames().FirstOrDefault(x => x.GetMethod().Name == "OnPwListItemDrag");
			if (sf != null)
				otp = OTPDAO.GetOTP(pe).GetOTP();
			else
				otp = OTPDAO.GetReadableOTP(pe);
			if (!string.IsNullOrEmpty(otp)) return otp;

			if (!Config.CheckTFA) return string.Empty;
			string url = pe.Strings.ReadSafe(PwDefs.UrlField);
			if (string.IsNullOrEmpty(url)) return string.Empty;

			TFASites.TFAPossible TFAPossible = TFASites.IsTFAPossible(url);
			if (TFAPossible == TFASites.TFAPossible.Yes)
				return PluginTranslation.PluginTranslate.SetupTFA;
			else if (TFAPossible == TFASites.TFAPossible.Unknown)
				return "Checking 2FA";
			else
				return string.Empty;
		}

		public override bool SupportsCellAction(string strColumnName)
		{
			return strColumnName != null;
		}

		public override void PerformCellAction(string strColumnName, PwEntry pe)
		{
			//Copy OTP to clipboard
			if (strColumnName == null) return;
			if (KeePassOTPExt.CopyOTP(pe) || OTPDAO.OTPDefined(pe) != OTPDAO.OTPDefinition.None) return;

			//Show 2FA setup instructions if available
			if (!Config.CheckTFA) return;
			string url = pe.Strings.ReadSafe(PwDefs.UrlField);
			string target = TFASites.GetTFAUrl(url);
			PluginDebug.AddInfo("Show 2FA instructions", 0, "URL: " + target);
			try
			{
				System.Diagnostics.Process.Start(target);
			}
			catch { }
		}

		public void StartTimer()
		{
			StopTimer();
			m_columnRefreshTimer = new System.Timers.Timer();
			m_columnRefreshTimer.Interval = 1000;
			m_columnRefreshTimer.SynchronizingObject = KeePass.Program.MainForm;
			m_columnRefreshTimer.Elapsed += OnTimerTick;
			m_columnRefreshTimer.Enabled = true;
		}

		public void StopTimer()
		{
			if (m_columnRefreshTimer == null) return;
			m_columnRefreshTimer.Enabled = false;
			m_columnRefreshTimer.Elapsed -= OnTimerTick;
			m_columnRefreshTimer.Dispose();
			m_columnRefreshTimer = null;
		}

		private bool m_bUpdateInProgress = false;
		private void OnTimerTick(object sender, EventArgs e)
		{
			if (m_bUpdateInProgress)
				return;
			m_bUpdateInProgress = true;
			//Trigger refresh of entry list if at least one relevant entry is shown
			//Relevant = Entry has OTP settings defined
			try
			{
				if (KeePass.Program.MainForm.UIIsInteractionBlocked()) return;
				if (!KeePass.Program.MainForm.Visible) return;
				if (!KeePass.Program.MainForm.ActiveDatabase.IsOpen) return;
				if (KeePass.Program.Config.MainWindow.EntryListColumns.Find(x => x.CustomName == KeePassOTPColumnProvider.KPOTPColumnName) == null) return;
				PwGroup pg = KeePass.Program.MainForm.GetSelectedGroup();
				if (pg == null) return;
				bool bRefresh = pg.GetEntries(KeePass.Program.Config.MainWindow.ShowEntriesOfSubGroups).FirstOrDefault(x => OTPDAO.OTPDefined(x) == OTPDAO.OTPDefinition.Complete) != null;
				if (!bRefresh) return;
				bool LVPossible = LV_DirectUpdate();
				if (!LVPossible)
					KeePass.Program.MainForm.RefreshEntriesList();
			}
			finally { m_bUpdateInProgress = false; }
		}

		private ColumnProviderUpdate m_cpr = null;

		private bool LV_DirectUpdate()
		{
			if (m_cpr == null) m_cpr = new ColumnProviderUpdate(this);
			if (!m_cpr.Initialized) return false;
			return m_cpr.UpdateEntryListColumn(KPOTPColumnName) != ColumnProviderUpdate.UpdateResult.Failed;
		}
	}

	public class ColumnProviderUpdate
	{
		public enum UpdateResult
		{
			Failed,
			NothingToDo,
			Success
		}

		private static ListView m_lvEntries = null;
		private ColumnProvider m_cp = null;
		private Dictionary<string, KeePass.App.Configuration.AceColumn> m_dColumns = new Dictionary<string, KeePass.App.Configuration.AceColumn>();

		public bool Initialized { get { return m_cp != null && m_lvEntries != null; } }

		public ColumnProviderUpdate(ColumnProvider cp)
		{
			if (m_lvEntries == null) m_lvEntries = Tools.GetControl("m_lvEntries", KeePass.Program.MainForm) as ListView;
			if (cp == null) return;
			m_cp = cp;
		}

		/// <summary>
		/// Update all shown columns provided by the column provider plugin
		/// </summary>
		/// <returns></returns>
		public UpdateResult UpdateEntryListColumns()
		{
			if (!Initialized) return UpdateResult.Failed;
			if (GetColumns(null) < 1) return UpdateResult.NothingToDo;
			try
			{
				StartUpdate();
				Update();
			}
			catch { return UpdateResult.Failed; }
			finally { EndUpdate(); }
			return UpdateResult.Success;
		}

		/// <summary>
		/// Update a selected list of columns provided by the column provider plugin.
		/// Only currently shown columns will be updated
		/// </summary>
		/// <param name="Columns">List of columns that shall be updated</param>
		/// <returns></returns>
		public UpdateResult UpdateEntryListColumns(List<string> Columns)
		{
			if (!Initialized) return UpdateResult.Failed;
			if (GetColumns(Columns) < 1) return UpdateResult.NothingToDo;
			try
			{
				StartUpdate();
				Update();
			}
			catch { return UpdateResult.Failed; }
			finally	{ EndUpdate(); }
			return UpdateResult.Success;
		}

		/// <summary>
		/// Update a single column provided by the column provider plugin.
		/// The update will happen only if the column is shown
		/// </summary>
		/// <param name="column">Column that shall be updated</param>
		/// <returns></returns>
		public UpdateResult UpdateEntryListColumn(string column)
		{
			return UpdateEntryListColumns(new List<string>() { column });
		}

		/// <summary>
		/// Internal method to updated columns
		/// </summary>
		/// <param name="lColumns">List containing column indices that correspond to KeePass.Program.Config.MainWindow.EntryListColumns</param>
		private void Update()
		{
			if (m_dColumns.Count == 0) return;
			for (int i = 0; i < m_lvEntries.Items.Count; i++)
			{
				PwListItem pli = m_lvEntries.Items[i].Tag as PwListItem;
				if ((pli == null) || (pli.Entry == null)) continue;
				foreach (var c in m_dColumns)
				{
					int col = KeePass.Program.Config.MainWindow.EntryListColumns.IndexOf(c.Value);
					if (col < 0) continue;
					m_lvEntries.Items[i].SubItems[col].Text = KeePassLib.Utility.StrUtil.MultiToSingleLine(m_cp.GetCellData(c.Key, pli.Entry));
				}
			}
		}

		private void StartUpdate()
		{
			if (m_lvEntries != null) m_lvEntries.BeginUpdate();
		}

		private void EndUpdate()
		{
			if (m_lvEntries != null) m_lvEntries.EndUpdate();
		}

		private int GetColumns(List<string> lColumns)
		{
			m_dColumns.Clear();
			foreach (string column in m_cp.ColumnNames)
			{
				if ((lColumns != null) && !lColumns.Contains(column)) continue;
				var c = KeePass.Program.Config.MainWindow.EntryListColumns.Find(x =>
					(x.Type == KeePass.App.Configuration.AceColumnType.PluginExt)
					&&
					(x.CustomName == column));
				if ((c == null) || c.HideWithAsterisks) continue;
				m_dColumns[c.CustomName] = c;
			}
			return m_dColumns.Count;
		}
	}
}