using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using KeePass.Forms;
using KeePass.Resources;
using KeePassLib;
using PluginTools;
using PluginTranslation;

namespace KeePassOTP
{
  public partial class KPOTP_Details : UserControl
  {
    private PwEntryForm _pef = null;
    private PwEntry _pe = null;
    private TabPage _tpKeePassOTP = null;
    private TabControl _tc = null;
    public KPOTP_Details()
    {
      InitializeComponent();

      Dock = DockStyle.Fill;

      lSetupURL.Text = PluginTranslate.TFA_SetupURL;
      lDocURL.Text = KPRes.MoreInfo;
      lRecoveryURL.Text = PluginTranslate.TFA_RecoveryURL;
      lNotes.Text = KPRes.Notes;
      lTFA_Modes.Text = KPRes.Options;
    }

    internal void InitEx(PwEntryForm f)
    {
      _pef = f;
      _tc = Tools.GetControl("m_tabMain", _pef) as TabControl;
      if (_tc == null) return;

      InitTab();

      DoRefresh();
    }

    internal void InitEx(KeePassOTPSetup f, TFASites.TFAData tfa)
    {
      _tc = Tools.GetControl("tcSetup", f) as TabControl;
      if (_tc == null) return;

      InitTab();

      DoRefresh(tfa);
    }
    private void InitTab()
    {
      _tpKeePassOTP = new TabPage(_pef != null ? PluginTranslate.PluginName : KPRes.Details);
      _tpKeePassOTP.Controls.Add(this);
      if (_pef != null)
      {
        _tc.TabPages[0].Leave += CheckActiveTabStop;
        _tc.TabPages.Insert(5, _tpKeePassOTP);
      }
      else
      {
        _tc.TabPages.Add(_tpKeePassOTP);
        tableLayoutPanel1.RowStyles[3].SizeType = SizeType.Absolute;
        tableLayoutPanel1.RowStyles[3].Height = 150;
        tableLayoutPanel1.RowStyles[4].SizeType = SizeType.Absolute;
        tableLayoutPanel1.RowStyles[4].Height = 150;
      }
    }

    private void DoRefresh()
    {
      _pef.UpdateEntryStrings(true, false);
      var tfa = TFASites.GetTFAData(_pef.EntryStrings.ReadSafe(PwDefs.UrlField));
      DoRefresh(tfa);
    }
    private void DoRefresh(TFASites.TFAData tfa)
    {
      if (tfa == null)
      {
        tfa = new TFASites.TFAData();
      }
      SetupUrl(lSetupURL, llSetupUrl, tfa.url);
      SetupUrl(lDocURL, llDocURL, tfa.documentation);
      SetupUrl(lRecoveryURL, llRecoveryURL, tfa.recovery);
      tbNotes.Text = tfa.notes;
      tbNotes.Enabled = !string.IsNullOrEmpty(tfa.notes);
      lbTFA_Modes.Items.Clear();
      lbTFA_Modes.Items.AddRange(tfa.tfa.ToArray());

      this.Enabled = tfa.tfa_possible;

      if (!tfa.tfa_possible) _tc.TabPages.Remove(_tpKeePassOTP);
      else if (!_tc.TabPages.Contains(_tpKeePassOTP)) _tc.TabPages.Insert(5, _tpKeePassOTP);
    }

    private void CheckActiveTabStop(object sender, EventArgs e)
    {
      DoRefresh();
    }

    private void SetupUrl(Label l, LinkLabel ll, string url)
    {
      ll.Text = string.Empty;
      ll.Links.Clear();
      if (string.IsNullOrEmpty(url))
      {
        l.Enabled = false;
        return;
      }
      l.Enabled = true;
      if (url.Length <= 150) ll.Text = url;
      else ll.Text = url.Substring(0, 50) + "..." + url.Substring(url.Length - 50, 50);
      ll.Links.Add(0, ll.Text.Length, url);
    }

    private void OnLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      var pe = _pef == null ? _pe : _pef.EntryRef;
      KeePass.Util.WinUtil.OpenUrl(e.Link.LinkData as string, pe, true);
    }
  }
}
