using KeePass.Resources;
using KeePassLib.Security;
using PluginTranslation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeePassOTP
{
	public partial class GoogleAuthenticatorImportSelection : Form
	{
		private List<GoogleAuthenticatorImport.OtpParameters> m_Entries = null;
		public GoogleAuthenticatorImport.OtpParameters SelectedEntry
		{
			get
			{
				if (lvEntries.CheckedIndices.Count != 1) return null;
				return m_Entries[lvEntries.CheckedIndices[0]];
			}
		}

		public GoogleAuthenticatorImportSelection()
		{
			InitializeComponent();

			Text = PluginTranslate.PluginName;
			bOK.Text = KPRes.Ok;
			bCancel.Text = KPRes.Cancel;
			lDesc.Text = PluginTranslate.SelectSingleEntry;
			chIssuer.Text = PluginTranslate.Issuer;
			chLabel.Text = KPRes.UserName;
		}

		public void InitEx(List<GoogleAuthenticatorImport.OtpParameters> lEntries)
		{
			m_Entries = lEntries;
			lvEntries.Items.Clear();
			foreach (var otp in m_Entries)
			{
				ListViewItem lvi = new ListViewItem();
				lvi.Text = otp.Issuer;
				string label = string.IsNullOrEmpty(otp.Issuer) ? otp.Name : otp.Name.Remove(0, otp.Issuer.Length + 1);
				lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, label));
				lvEntries.Items.Add(lvi);
			}
		}

		private void lvEntries_ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			lvEntries.ItemChecked -= lvEntries_ItemChecked;
			foreach (ListViewItem lvi in lvEntries.Items)
				lvi.Checked = lvi.Index == e.Item.Index && e.Item.Checked;
			lvEntries.ItemChecked += lvEntries_ItemChecked;
			bOK.Enabled = lvEntries.CheckedItems.Count == 1;
		}

		private void GoogleAuthenticatorImportSelection_Shown(object sender, EventArgs e)
		{
			chIssuer.Width = chLabel.Width = lvEntries.ClientSize.Width / 2;
			lvEntries.Items[0].Checked = true;
		}
	}
}
