using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using KeePass.Forms;

namespace PluginTools
{
  public static partial class Tools
  {
    public static object GetPluginInstance(string PluginName)
    {
      string comp = PluginName + "." + PluginName + "Ext";
      BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic;
      try
      {
        var PluginManager = GetField("m_pluginManager", KeePass.Program.MainForm);
        var PluginList = GetField("m_vPlugins", PluginManager);
        if (PluginList == null) PluginList = GetField("m_lPlugins", PluginManager);
        MethodInfo IteratorMethod = PluginList.GetType().GetMethod("System.Collections.Generic.IEnumerable<T>.GetEnumerator", bf);
        IEnumerator<object> PluginIterator = (IEnumerator<object>)(IteratorMethod.Invoke(PluginList, null));
        while (PluginIterator.MoveNext())
        {
          object result = GetField("m_pluginInterface", PluginIterator.Current);
          if (comp == result.GetType().ToString()) return result;
        }
      }

      catch (Exception) { }
      return null;
    }

    public static Dictionary<string, Version> GetLoadedPluginsName()
    {
      Dictionary<string, Version> dPlugins = new Dictionary<string, Version>();
      BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic;
      try
      {
        var PluginManager = GetField("m_pluginManager", KeePass.Program.MainForm);
        var PluginList = GetField("m_vPlugins", PluginManager);
        if (PluginList == null) PluginList = GetField("m_lPlugins", PluginManager);
        MethodInfo IteratorMethod = PluginList.GetType().GetMethod("System.Collections.Generic.IEnumerable<T>.GetEnumerator", bf);
        IEnumerator<object> PluginIterator = (IEnumerator<object>)(IteratorMethod.Invoke(PluginList, null));
        while (PluginIterator.MoveNext())
        {
          object result = GetField("m_pluginInterface", PluginIterator.Current);
          var x = result.GetType().Assembly;
          object[] v = x.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true);
          Version ver = null;
          if ((v != null) && (v.Length > 0))
            ver = new Version(((AssemblyFileVersionAttribute)v[0]).Version);
          else
            ver = result.GetType().Assembly.GetName().Version;
          if (ver.Build < 0) ver = new Version(ver.Major, ver.Minor, 0, 0);
          if (ver.Revision < 0) ver = new Version(ver.Major, ver.Minor, ver.Build, 0);
          dPlugins[result.GetType().FullName] = ver;
        }
      }
      catch (Exception) { }
      return dPlugins;
    }

    public static event EventHandler<OptionsFormsEventArgs> OptionsFormShown;
    public static event EventHandler<OptionsFormsEventArgs> OptionsFormClosed;

    private static bool OptionsEnabled = (KeePass.Program.Config.UI.UIFlags & (ulong)KeePass.App.Configuration.AceUIFlags.DisableOptions) != (ulong)KeePass.App.Configuration.AceUIFlags.DisableOptions;
    private static bool m_ActivatePluginTab = false;
    private static string m_sPluginClassname = string.Empty;
    private static OptionsForm m_of = null;
    private const string c_tabRookiestyle = "m_tabRookiestyle";
    private const string c_tabControlRookiestyle = "m_tabControlRookiestyle";
    private static string m_TabPageName = string.Empty;
    private static bool m_OptionsShown = false;
    private static bool m_PluginContainerShown = false;

    public static void AddPluginToOptionsForm(KeePass.Plugins.Plugin p, UserControl uc)
    {
      m_OptionsShown = m_PluginContainerShown = false;
      TabPage tPlugin = new TabPage(DefaultCaption);
      tPlugin.CreateControl();
      tPlugin.Name = m_TabPageName = c_tabRookiestyle + p.GetType().Name;
      uc.Dock = DockStyle.Fill;
      uc.Padding = new Padding(15, 10, 15, 10);
      tPlugin.Controls.Add(uc);
      PluginDebug.AddInfo("Adding/Searching " + c_tabControlRookiestyle);
      TabControl tcPlugins = AddPluginTabContainer();
      int i = 0;
      bool insert = false;
      for (int j = 0; j < tcPlugins.TabPages.Count; j++)
      {
        if (string.Compare(tPlugin.Text, tcPlugins.TabPages[j].Text, StringComparison.CurrentCultureIgnoreCase) < 0)
        {
          i = j;
          insert = true;
          break;
        }
      }
      if (!insert)
      {
        i = tcPlugins.TabPages.Count;
        PluginDebug.AddInfo(p.GetType().Name + " tab index : " + i.ToString() + " - insert!", 0);
      }
      else PluginDebug.AddInfo(p.GetType().Name + " tab index : " + i.ToString(), 0);
      tcPlugins.TabPages.Insert(i, tPlugin);
      AddPluginToOverview(tPlugin.Name.Replace(c_tabRookiestyle, string.Empty), tcPlugins);
      if (p.SmallIcon != null)
      {
        tcPlugins.ImageList.Images.Add(tPlugin.Name, p.SmallIcon);
        tPlugin.ImageKey = tPlugin.Name;
      }
      TabControl tcMain = Tools.GetControl("m_tabMain", m_of) as TabControl;
      if (!string.IsNullOrEmpty(PluginURL)) AddPluginLink(uc);
    }

    public static void AddPluginToOverview(string sPluginName)
    {
      AddPluginToOverview(sPluginName, null);
    }

    private static void AddPluginToOverview(string sPluginName, TabControl tcPlugins)
    {
      if (tcPlugins == null) tcPlugins = AddPluginTabContainer();
      TabPage tpOverview = null;
      ListView lv = null;
      string sTabName = c_tabRookiestyle + "_PluginOverview";
      string sListViewName = c_tabRookiestyle + "_PluginOverviewListView";
      if (tcPlugins.TabPages.ContainsKey(sTabName))
      {
        tpOverview = tcPlugins.TabPages[sTabName];
        lv = (ListView)tpOverview.Controls.Find(sListViewName, true)[0];
        PluginDebug.AddInfo("Found " + sTabName, 0, "Listview: " + (lv == null ? "null" : lv.Items.Count.ToString() + " /" + lv.Name.ToString()));
      }
      else
      {
        tpOverview = new TabPage("Overview");
        tpOverview.CreateControl();
        tpOverview.Name = sTabName;
        UserControl uc = new UserControl();
        uc.Dock = DockStyle.Fill;
        uc.Padding = new Padding(15, 10, 15, 10);
        tpOverview.Controls.Add(uc);
        lv = new ListView();
        lv.Name = sListViewName;
        lv.Dock = DockStyle.Fill;
        lv.View = View.Details;
        lv.Columns.Add("Plugin");
        lv.Columns.Add("Version");
        lv.CheckBoxes = true;
        tpOverview.Layout += TpOverview_Layout;
        Label lInfo = new Label();
        lInfo.AutoSize = true;
        lInfo.Text = "Use the checkbox to activate/deactivate debug mode";
        lInfo.Dock = DockStyle.Bottom;
        uc.Controls.Add(lv);
        uc.Controls.Add(lInfo);
      }
      lv.ItemCheck += Lv_ItemCheck;
      lv.Sorting = SortOrder.Ascending;
      lv.FullRowSelect = true;
      ListViewItem lvi = new ListViewItem();
      lvi.Name = sPluginName;
      lvi.Checked = PluginDebug.DebugMode;
      lvi.Text = DefaultCaption;
      Version v = new Version(0, 0);
      GetLoadedPluginsName().TryGetValue(sPluginName.Replace("Ext", string.Empty) + "." + sPluginName, out v);
      if (v == null) PluginDebug.AddError("Could not get loaded plugins' data", 0);
      string ver = (v == null) ? "???" : v.ToString();
      if (ver.EndsWith(".0")) ver = ver.Substring(0, ver.Length - 2);
      else ver += " (Dev)";
      lvi.SubItems.Add(ver);
      lv.Items.Add(lvi);
      tcPlugins.TabPages.Remove(tpOverview);
      tcPlugins.TabPages.Add(tpOverview);
      PluginDebug.AddInfo("Added " + sTabName, 0, "Listview: " + (lv == null ? "null" : lv.Items.Count.ToString() + " /" + lv.Name.ToString()));
    }

    private static void TpOverview_Layout(object sender, LayoutEventArgs e)
    {
      string sListViewName = c_tabRookiestyle + "_PluginOverviewListView";
      ListView lv = (sender as TabPage).Controls.Find(sListViewName, true)[0] as ListView;
      lv.BeginUpdate();
      lv.Columns[1].DisplayIndex = 0;
      lv.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
      int w = lv.Columns[1].Width;
      lv.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
      lv.Columns[1].Width = Math.Max(w, lv.Columns[1].Width);
      lv.Columns[0].Width = lv.ClientSize.Width - lv.Columns[1].Width;
      if (lv.Columns[0].Width < 150)
      {
        lv.Columns[1].Width = 100;
        lv.Columns[0].Width = lv.ClientSize.Width - lv.Columns[1].Width;
      }
      lv.Columns[1].DisplayIndex = 1;
      lv.EndUpdate();
    }

    private static void Lv_ItemCheck(object sender, ItemCheckEventArgs e)
    {
      ListViewItem lvi = (sender as ListView).Items[e.Index];
      if (lvi == null) return;
      if (lvi.Text != DefaultCaption) return;
      PluginDebug.DebugMode = e.NewValue == CheckState.Checked;
    }

    private static void OnPluginTabsSelected(object sender, TabControlEventArgs e)
    {
      m_OptionsShown |= (e.TabPage.Name == m_TabPageName);
      m_PluginContainerShown |= (m_OptionsShown || (e.TabPage.Name == c_tabRookiestyle));
    }

    public static UserControl GetPluginFromOptions(KeePass.Plugins.Plugin p, out bool PluginOptionsShown)
    {
      PluginOptionsShown = m_OptionsShown && m_PluginContainerShown;
      TabPage tPlugin = Tools.GetControl(c_tabRookiestyle + p.GetType().Name, m_of) as TabPage;
      if (tPlugin == null) return null;
      return tPlugin.Controls[0] as UserControl;
    }

    public static void ShowOptions()
    {
      m_ActivatePluginTab = true;
      if (OptionsEnabled)
        KeePass.Program.MainForm.ToolsMenu.DropDownItems["m_menuToolsOptions"].PerformClick();
      else
      {
        m_of = new OptionsForm();
        m_of.InitEx(KeePass.Program.MainForm.ClientIcons);
        m_of.ShowDialog();
      }
    }

    private static void AddPluginLink(UserControl uc)
    {
      LinkLabel llUrl = new LinkLabel();
      llUrl.Links.Add(0, PluginURL.Length, PluginURL);
      llUrl.Text = PluginURL;
      uc.Controls.Add(llUrl);
      llUrl.Dock = DockStyle.Bottom;
      llUrl.AutoSize = true;
      llUrl.LinkClicked += new LinkLabelLinkClickedEventHandler(PluginURLClicked);
    }

    private static void PluginURLClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      string target = e.Link.LinkData as string;
      OpenUrl(target);
    }

    private static void OnOptionsFormShown(object sender, EventArgs e)
    {
      m_of.Shown -= OnOptionsFormShown;
      TabControl tcMain = Tools.GetControl("m_tabMain", m_of) as TabControl;
      if (!tcMain.TabPages.ContainsKey(c_tabRookiestyle)) return;
      TabPage tPlugins = tcMain.TabPages[c_tabRookiestyle];
      TabControl tcPlugins = Tools.GetControl(c_tabControlRookiestyle, tPlugins) as TabControl;
      if (m_ActivatePluginTab)
      {
        tcMain.SelectedIndex = tcMain.TabPages.IndexOfKey(c_tabRookiestyle);
        KeePass.Program.Config.Defaults.OptionsTabIndex = (uint)tcMain.SelectedIndex;
        tcPlugins.SelectedIndex = tcPlugins.TabPages.IndexOfKey(c_tabRookiestyle + m_sPluginClassname);
      }
      m_ActivatePluginTab = false;
      tcMain.Selected += OnPluginTabsSelected;
      tcPlugins.Selected += OnPluginTabsSelected;
      tcMain.ImageList.Images.Add(c_tabRookiestyle + "Icon", (Image)KeePass.Program.Resources.GetObject("B16x16_BlockDevice"));
      tPlugins.ImageKey = c_tabRookiestyle + "Icon";
      m_PluginContainerShown |= tcMain.SelectedTab == tPlugins;
      m_OptionsShown |= (tcPlugins.SelectedTab.Name == m_TabPageName);
      CheckKeeTheme(tPlugins);
    }

    private static void CheckKeeTheme(Control c)
    {
      Control check = GetControl("Rookiestyle_KeeTheme_Check", m_of);
      if (check != null) return;
      PluginDebug.AddInfo("Checking for KeeTheme");
      check = new Control();
      check.Name = "Rookiestyle_KeeTheme_Check";
      check.Visible = false;
      m_of.Controls.Add(check);
      KeePass.Plugins.Plugin p = (KeePass.Plugins.Plugin)GetPluginInstance("KeeTheme");
      if (p == null) return;
      var t = GetField("_theme", p);
      if (t == null) return;
      bool bKeeThemeEnabled = (bool)t.GetType().GetProperty("Enabled").GetValue(t, null);
      if (!bKeeThemeEnabled) return;
      var v = GetField("_controlVisitor", p);
      if (v == null) return;
      MethodInfo miVisit = v.GetType().GetMethod("Visit", new Type[] { typeof(Control) });
      if (miVisit == null) return;
      miVisit.Invoke(v, new object[] { c });
    }

    private static void OnWindowAdded(object sender, KeePass.UI.GwmWindowEventArgs e)
    {
      if (OptionsFormShown == null) return;
      if (e.Form is OptionsForm)
      {
        m_of = e.Form as OptionsForm;
        m_of.Shown += OnOptionsFormShown;
        OptionsFormsEventArgs o = new OptionsFormsEventArgs(m_of);
        OptionsFormShown(sender, o);
      }
    }

    private static void OnWindowRemoved(object sender, KeePass.UI.GwmWindowEventArgs e)
    {
      if (OptionsFormClosed == null) return;
      if (e.Form is OptionsForm)
      {
        OptionsFormsEventArgs o = new OptionsFormsEventArgs(m_of);
        OptionsFormClosed(sender, o);
      }
    }

    private static TabControl AddPluginTabContainer()
    {
      if (m_of == null)
      {
        PluginDebug.AddError("Could not identify KeePass options form", 0);
        return null;
      }
      TabControl tcMain = Tools.GetControl("m_tabMain", m_of) as TabControl;
      if (tcMain == null)
      {
        PluginDebug.AddError("Could not locate m_tabMain", 0);
        return null;
      }
      TabPage tPlugins = null;
      TabControl tcPlugins = null;
      if (tcMain.TabPages.ContainsKey(c_tabRookiestyle))
      {
        tPlugins = tcMain.TabPages[c_tabRookiestyle];
        tcPlugins = (TabControl)tPlugins.Controls[c_tabControlRookiestyle];
        if (tcPlugins == null)
        {
          PluginDebug.AddError("Could not locate " + c_tabControlRookiestyle, 0);
          return null;
        }
        tcPlugins.Multiline = false; //Older version of PluginTools might still be used by other plugins
        PluginDebug.AddInfo("Found " + c_tabControlRookiestyle, 0);
        return tcPlugins;
      }
      tPlugins = new TabPage(KeePass.Resources.KPRes.Plugin + " " + m_of.Text);
      tPlugins.Name = c_tabRookiestyle;
      tPlugins.CreateControl();
      if (!OptionsEnabled)
      {
        while (tcMain.TabCount > 0)
          tcMain.TabPages.RemoveAt(0);
        PluginDebug.AddInfo("Removed tab pages from KeePass options form", 0);
      }
      tcMain.TabPages.Add(tPlugins);
      tcPlugins = new TabControl();
      tcPlugins.Name = c_tabControlRookiestyle;
      tcPlugins.Dock = DockStyle.Fill;
      tcPlugins.Multiline = false;
      tcPlugins.CreateControl();
      if (tcPlugins.ImageList == null)
        tcPlugins.ImageList = new ImageList();
      tPlugins.Controls.Add(tcPlugins);
      PluginDebug.AddInfo("Added " + c_tabControlRookiestyle, 0);
      return tcPlugins;
    }

    public class OptionsFormsEventArgs : EventArgs
    {
      public Form form;

      public OptionsFormsEventArgs(Form form)
      {
        this.form = form;
      }
    }
  }
}
