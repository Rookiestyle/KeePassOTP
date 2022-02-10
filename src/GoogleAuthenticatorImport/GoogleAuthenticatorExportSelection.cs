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
			chEntry.Text = KPRes.Entry;
		}

		public void InitEx(Dictionary<PwEntry, GoogleAuthenticatorImport.OtpParameters> dEntries)
		{
			m_dEntries = dEntries;
			lvEntries.Items.Clear();
			var fStrikeOut = KeePass.UI.FontUtil.CreateFont(lvEntries.Font, FontStyle.Strikeout);
			foreach (var otp in m_dEntries)
			{
				ListViewItem lvi = new ListViewItem();
				lvi.Tag = otp;
				lvi.Text = otp.Key.ParentGroup.Name;
				lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, otp.Key.Strings.ReadSafe(PwDefs.TitleField)));
				lvEntries.Items.Add(lvi);
				if (otp.Value == null) lvi.Font = fStrikeOut;
				else lvi.Checked = true;
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
			chGroup.Width = chEntry.Width = lvEntries.ClientSize.Width / 2;
			lvEntries.Height = ClientSize.Height - pDesc.Height - pButtons.Height - Padding.Top - Padding.Bottom;
			lvEntries.Width = ClientSize.Width - Padding.Left - Padding.Right;
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
