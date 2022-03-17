using KeePass.Resources;
using KeePassLib;
using KeePassLib.Security;
using PluginTranslation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeePassOTP
{
	public partial class GoogleAuthenticatorExportSelection : Form
	{
		private Dictionary<PwEntry, GoogleAuthenticatorImport.OtpParameters> m_dEntries = null;
		public Dictionary<PwEntry, GoogleAuthenticatorImport.OtpParameters> SelectedEntries
		{
			get
			{
				Dictionary<PwEntry, GoogleAuthenticatorImport.OtpParameters> dEntries = new Dictionary<PwEntry, GoogleAuthenticatorImport.OtpParameters>();
				foreach (ListViewItem i in lvEntries.CheckedItems)
                {
					if (i.Tag == null || !(i.Tag is KeyValuePair<PwEntry, GoogleAuthenticatorImport.OtpParameters>)) continue;
					KeyValuePair<PwEntry, GoogleAuthenticatorImport.OtpParameters> kvp = (KeyValuePair<PwEntry, GoogleAuthenticatorImport.OtpParameters>)i.Tag;
					dEntries[kvp.Key] = kvp.Value;
                }
				return dEntries;
			}
		}

		public GoogleAuthenticatorExportSelection()
		{
			InitializeComponent();

			Text = PluginTranslate.PluginName + " -> Google Authenticator";
			bOK.Text = KPRes.Ok;
			bCancel.Text = KPRes.Cancel;
			lDesc.Text = PluginTranslate.SelectEntriesForExport;
			chGroup.Text = KPRes.Group;
			chEntryTitle.Text = KPRes.Entry;
			chEntryUsername.Text = KPRes.UserName;
			bSelectAll.Text = KPRes.SelectAll;
			bDeselectAll.Text = PluginTranslate.DeselectAll;
		}

		public void InitEx(Dictionary<PwEntry, GoogleAuthenticatorImport.OtpParameters> dEntries)
		{
			m_dEntries = dEntries;
			lvEntries.Items.Clear();
			var fStrikeOut = KeePass.UI.FontUtil.CreateFont(lvEntries.Font, FontStyle.Strikeout);
			PwGroup pgRecycleBin = null;
			if (KeePass.Program.MainForm.ActiveDatabase.RecycleBinEnabled)
			{
				pgRecycleBin = KeePass.Program.MainForm.ActiveDatabase.RootGroup.FindGroup(KeePass.Program.MainForm.ActiveDatabase.RecycleBinUuid, true);
			}

			ListViewGroup lvgRegular = new ListViewGroup(string.Empty);
			ListViewGroup lvgDeleted = new ListViewGroup(KPRes.RecycleBin);
			ListViewGroup lvgExpired = new ListViewGroup(KPRes.ExpiredEntries);

			if (!lvEntries.Groups.Contains(lvgRegular)) lvEntries.Groups.Add(lvgRegular);
			if (!lvEntries.Groups.Contains(lvgExpired)) lvEntries.Groups.Add(lvgExpired);
			if (!lvEntries.Groups.Contains(lvgDeleted)) lvEntries.Groups.Add(lvgDeleted);

			foreach (var otp in m_dEntries)
			{
				bool bExpired = false;
				bool bDeleted = false;

				ListViewItem lvi = new ListViewItem();
				lvi.Tag = otp;
				lvi.Text = otp.Key.ParentGroup.Name;
				lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, otp.Key.Strings.ReadSafe(PwDefs.TitleField)));
				lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, otp.Key.Strings.ReadSafe(PwDefs.UserNameField))); 
				if (otp.Key.IsContainedIn(pgRecycleBin))
				{
					bDeleted = true;
					lvi.SubItems[0].Tag = "deleted";
					lvi.Group = lvgDeleted;
				}
				else if (otp.Key.Expires && otp.Key.ExpiryTime <= DateTime.UtcNow)
				{
					bExpired = true;
					lvi.SubItems[0].Tag = "expired";
					lvi.Group = lvgExpired;
				}
				else
				{
					lvi.Group = lvgRegular;
				}
				lvEntries.Items.Add(lvi);
				if (otp.Value == null) lvi.Font = fStrikeOut;

				else lvi.Checked = !bDeleted && !bExpired;
			}
		}

		private void lvEntries_ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			try
			{
				if (!e.Item.Checked) return;
				if (e.Item.Tag == null || !(e.Item.Tag is KeyValuePair<PwEntry, GoogleAuthenticatorImport.OtpParameters>)) return;
				KeyValuePair<PwEntry, GoogleAuthenticatorImport.OtpParameters> kvp = (KeyValuePair<PwEntry, GoogleAuthenticatorImport.OtpParameters>)e.Item.Tag;
				if (kvp.Value != null) return;

				lvEntries.ItemChecked -= lvEntries_ItemChecked;
				e.Item.Checked = false;
				lvEntries.ItemChecked += lvEntries_ItemChecked;
			}
			finally { bOK.Enabled = lvEntries.CheckedItems.Count > 0; }
		}

		private void GoogleAuthenticatorImportSelection_Shown(object sender, EventArgs e)
		{
			lvEntries.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

			if (bSelectAll.Width > bDeselectAll.Width) bDeselectAll.Width = bSelectAll.Width;
			if (bDeselectAll.Width > bSelectAll.Width) bSelectAll.Width = bDeselectAll.Width;
		}

		private void OnSelectDeselectAll(object sender, EventArgs e)
		{
			bool bChecked = sender == bSelectAll;
			foreach (ListViewItem lvi in lvEntries.Items)
			{
				if (lvi.Checked == bChecked) continue;
				lvi.Checked = bChecked;
			}
		}
	}
}
