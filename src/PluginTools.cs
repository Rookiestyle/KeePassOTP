using KeePass.Forms;
using KeePass.UI;
using KeePassLib.Utility;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace PluginTools
{
	public static class Tools
	{
		public static Version KeePassVersion { get; private set; }
		public static string DefaultCaption = string.Empty;
		public static string PluginURL = string.Empty;
		public static string KeePassLanguageIso6391 { get; private set; }

		private static bool MonoWorkaroundRequired = KeePassLib.Native.NativeLib.IsUnix();

		static Tools()
		{
			KeePass.UI.GlobalWindowManager.WindowAdded += OnWindowAdded;
			KeePass.UI.GlobalWindowManager.WindowRemoved += OnWindowRemoved;
			KeePassVersion = typeof(KeePass.Program).Assembly.GetName().Version;
			KeePassLanguageIso6391 = KeePass.Program.Translation.Properties.Iso6391Code;
			if (string.IsNullOrEmpty(KeePassLanguageIso6391)) KeePassLanguageIso6391 = "en";
			m_sPuginClassname = typeof(Tools).Assembly.GetName().Name + "Ext";
		}

		#region Form and field handling
		public static object GetField(string field, object obj)
		{
			BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic;
			return GetField(field, obj, bf);
		}

		public static object GetField(string field, object obj, BindingFlags bf)
		{
			if (obj == null) return null;
			FieldInfo fi = obj.GetType().GetField(field, bf);
			if (fi == null) return null;
			return fi.GetValue(obj);
		}

		public static Control GetControl(string control)
		{
			return GetControl(control, KeePass.Program.MainForm);
		}

		public static Control GetControl(string control, Control form)
		{
			if (form == null) return null;
			if (string.IsNullOrEmpty(control)) return null;
			Control[] cntrls = form.Controls.Find(control, true);
			if (cntrls.Length == 0) return null;
			return cntrls[0];
		}

		public static ToolStripMenuItem FindToolStripMenuItem(ToolStripItemCollection tsic, string key, bool searchAllChildren)
		{
			if (tsic == null) return null;
			ToolStripItem[] tsi = FindToolStripMenuItems(tsic, key, searchAllChildren);
			if (tsi.Length > 0) return tsi[0] as ToolStripMenuItem;
			return null;
		}

		public static ToolStripItem[] FindToolStripMenuItems(ToolStripItemCollection tsic, string key, bool searchAllChildren)
		{
			if (tsic == null) return new ToolStripItem[] { };
			ToolStripItem[] tsi = tsic.Find(key, searchAllChildren);
			if (!MonoWorkaroundRequired || !searchAllChildren) return tsi;

			//Mono does not support 'searchAllChildren' for ToolStripItemCollection
			//Iterate over all items and search for given item
			List<ToolStripItem> lItems = new List<ToolStripItem>(tsi);
			foreach (var item in tsic)
			{
				ToolStripMenuItem tsmi = item as ToolStripMenuItem;
				if (tsmi == null) continue;
				lItems.AddRange(FindToolStripMenuItems(tsmi.DropDownItems, key, searchAllChildren));
			}
			return lItems.ToArray();
		}
		#endregion

		#region Plugin options and instance
		public static object GetPluginInstance(string PluginName)
		{
			string comp = PluginName + "." + PluginName + "Ext";
			BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic;
			try
			{
				var PluginManager = GetField("m_pluginManager", KeePass.Program.MainForm);
				var PluginList = GetField("m_vPlugins", PluginManager);
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
		private static string m_sPuginClassname = string.Empty;
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
			llUrl.LinkClicked += new LinkLabelLinkClickedEventHandler(PluginURLClicked);
		}

		private static void PluginURLClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			string target = e.Link.LinkData as string;
			System.Diagnostics.Process.Start(target);
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
				tcPlugins.SelectedIndex = tcPlugins.TabPages.IndexOfKey(c_tabRookiestyle + m_sPuginClassname);
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
		#endregion

		#region MessageBox shortcuts
		public static DialogResult ShowError(string msg)
		{
			return ShowError(msg, DefaultCaption);
		}

		public static DialogResult ShowInfo(string msg)
		{
			return ShowInfo(msg, DefaultCaption);
		}

		public static DialogResult AskYesNo(string msg)
		{
			return AskYesNo(msg, DefaultCaption);
		}

		public static DialogResult ShowError(string msg, string caption)
		{
			PluginDebug.AddError("Show error", 6, caption, msg);
			return MessageBox.Show(msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public static DialogResult ShowInfo(string msg, string caption)
		{
			PluginDebug.AddInfo("Show info", 6, caption, msg);
			return MessageBox.Show(msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		public static DialogResult AskYesNo(string msg, string caption)
		{
			DialogResult result = MessageBox.Show(msg, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			PluginDebug.AddInfo("Ask question", 6, caption, msg, "Result: " + result.ToString());
			return result;
		}
		#endregion

		#region GlobalWindowManager
		public static void GlobalWindowManager(Form form)
		{
			if ((form == null) || (form.IsDisposed)) return;
			form.Load += FormLoaded;
			form.FormClosed += FormClosed;
		}

		private static void FormLoaded(object sender, EventArgs e)
		{
			KeePass.UI.GlobalWindowManager.AddWindow(sender as Form, sender as IGwmWindow);
		}

		private static void FormClosed(object sender, FormClosedEventArgs e)
		{
			KeePass.UI.GlobalWindowManager.RemoveWindow(sender as Form);
		}
		#endregion
	}

	public static class PluginDebug
	{
		[Flags]
		public enum LogLevelFlags
		{
			None = 0,
			Info = 1,
			Warning = 2,
			Error = 4,
			Success = 8,
			All = Info | Warning | Error | Success
		}

		public static string DebugFile { get; private set; }
		public static LogLevelFlags LogLevel = LogLevelFlags.All;

		private static bool AutoSave = false;
		private static bool AutoOpen = false;
		private static bool AskOpen = true;
		private static List<DebugEntry> m_DebugEntries = new List<DebugEntry>();
		private static string PluginName = string.Empty;
		private static string PluginVersion;
		private static bool m_DebugMode = false;
		public static bool DebugMode
		{
			get { return m_DebugMode; }
			set { m_DebugMode = value; }
		}
		private static Dictionary<string, Version> m_plugins = new Dictionary<string, Version>();
		public static Version DotNetVersion { get; private set; }
		private static int m_DotNetRelease = 0;

		private static DateTime m_Start = DateTime.UtcNow;

		//Init
		static PluginDebug()
		{
			PluginName = Assembly.GetExecutingAssembly().GetName().Name;
			PluginVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

			ulong uInst = KeePass.Util.WinUtil.GetMaxNetFrameworkVersion();
			DotNetVersion = new Version(StrUtil.VersionToString(uInst));
			try
			{
				RegistryKey rkRel = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full", false);
				m_DotNetRelease = (int)rkRel.GetValue("Release");
				if (rkRel != null) rkRel.Close();
			}
			catch { }

			DebugFile = System.IO.Path.GetTempPath() + "Debug_" + PluginName + "_" + m_Start.ToString("yyyyMMddTHHmmssZ") + ".xml";

			string level = KeePass.Program.CommandLineArgs["debuglevel"];
			if (string.IsNullOrEmpty(level))
				level = LogLevelFlags.All.ToString();
			try
			{
				LogLevel = (LogLevelFlags)Enum.Parse(LogLevel.GetType(), level);
			}
			catch { }
			AutoSave = KeePass.Program.CommandLineArgs["debugautosave"] != null;
			AutoOpen = KeePass.Program.CommandLineArgs["debugautoopen"] != null;
			AskOpen = KeePass.Program.CommandLineArgs["debugsaveonly"] == null;

			DebugMode = KeePass.Program.CommandLineArgs[KeePass.App.AppDefs.CommandLineOptions.Debug] != null;
			if (!DebugMode)
			{
				try
				{
					string[] plugins = KeePass.Program.CommandLineArgs["debugplugin"].ToLowerInvariant().Split(new char[] { ',' });
					DebugMode |= Array.Find(plugins, x => x.Trim() == PluginName.ToLowerInvariant()) != null;
					DebugMode |= Array.Find(plugins, x => x.Trim() == "all") != null;
				}
				catch { }
			}
			KeePass.Program.MainForm.FormLoadPost += LoadPluginNames;
			if (AutoSave)
				AddInfo("AutoSave mode active", 0);
		}

		#region Handle debug messages
		public static void AddInfo(string msg)
		{
			AddMessage(LogLevelFlags.Info, msg, 5, null);
		}

		public static void AddInfo(string msg, params string[] parameters)
		{
			AddMessage(LogLevelFlags.Info, msg, 5, parameters);
		}

		public static void AddInfo(string msg, int CallstackFrames)
		{
			AddMessage(LogLevelFlags.Info, msg, CallstackFrames, null);
		}

		public static void AddInfo(string msg, int CallstackFrames, params string[] parameters)
		{
			AddMessage(LogLevelFlags.Info, msg, CallstackFrames, parameters);
		}

		public static void AddWarning(string msg)
		{
			AddMessage(LogLevelFlags.Warning, msg, 5, null);
		}

		public static void AddWarning(string msg, params string[] parameters)
		{
			AddMessage(LogLevelFlags.Warning, msg, 5, parameters);
		}

		public static void AddWarning(string msg, int CallstackFrames)
		{
			AddMessage(LogLevelFlags.Warning, msg, CallstackFrames, null);
		}

		public static void AddWarning(string msg, int CallstackFrames, params string[] parameters)
		{
			AddMessage(LogLevelFlags.Warning, msg, CallstackFrames, parameters);
		}

		public static void AddError(string msg)
		{
			AddMessage(LogLevelFlags.Error, msg, 5, null);
		}

		public static void AddError(string msg, params string[] parameters)
		{
			AddMessage(LogLevelFlags.Error, msg, 5, parameters);
		}

		public static void AddError(string msg, int CallstackFrames)
		{
			AddMessage(LogLevelFlags.Error, msg, CallstackFrames, null);
		}

		public static void AddError(string msg, int CallstackFrames, params string[] parameters)
		{
			AddMessage(LogLevelFlags.Error, msg, CallstackFrames, parameters);
		}

		public static void AddSuccess(string msg)
		{
			AddMessage(LogLevelFlags.Success, msg, 5, null);
		}

		public static void AddSuccess(string msg, params string[] parameters)
		{
			AddMessage(LogLevelFlags.Success, msg, 5, parameters);
		}

		public static void AddSuccess(string msg, int CallstackFrames)
		{
			AddMessage(LogLevelFlags.Success, msg, CallstackFrames, null);
		}

		public static void AddSuccess(string msg, int CallstackFrames, params string[] parameters)
		{
			AddMessage(LogLevelFlags.Success, msg, CallstackFrames, parameters);
		}

		private static void AddMessage(LogLevelFlags severity, string msg, int CallstackFrames, string[] parameters)
		{
			if (m_Saving || !DebugMode || ((severity & LogLevel) != severity)) return;
			if (m_DebugEntries.Count > 0)
			{
				DebugEntry prev = m_DebugEntries[m_DebugEntries.Count - 1];
				if ((prev.severity == severity) && (prev.msg == msg) && ParamsEqual(prev.parameters, parameters))
				{
					m_DebugEntries[m_DebugEntries.Count - 1].counter++;
					return;
				}
			}
			DebugEntry m = new DebugEntry();
			m.severity = severity;
			m.msg = msg;
			m.utc = DateTime.UtcNow;
			m.counter = 1;
			m.parameters = parameters;
			if (CallstackFrames != 0)
			{
				System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
				for (int i = 0; i < st.FrameCount; i++)
				{
					if (m.sf.Count == CallstackFrames) break;
					System.Diagnostics.StackFrame sf = st.GetFrame(i);
					if (sf.GetMethod().DeclaringType.FullName != "PluginTools.PluginDebug")
						m.sf.Add(sf);
				}
			}
			m_DebugEntries.Add(m);
			if (AutoSave) SaveDebugMessages();
		}

		private static bool ParamsEqual(string[] a, string[] b)
		{
			if ((a == null) && (b == null)) return true;
			if ((a == null) && (b != null)) return false;
			if ((a != null) && (b == null)) return false;
			if (a.Length != b.Length) return false;
			for (int i = 0; i < a.Length; i++)
				if (a[i] != b[i]) return false;
			return true;
		}

		public static bool HasMessage(LogLevelFlags severity, string msg)
		{
			return m_DebugEntries.Find(x => (x.severity == severity) && (x.msg == msg)) != null;
		}
		#endregion

		public static void SaveOrShow()
		{
			if (m_DebugEntries.Count == 0) return;
			SaveDebugMessages();
			if (AutoOpen || (AskOpen && Tools.AskYesNo("DebugFile: " + DebugFile + "\n\nOpen debug file?") == DialogResult.Yes))
			{
				try
				{
					System.Diagnostics.Process.Start(DebugFile);
				}
				catch
				{
					if (KeePassLib.Native.NativeLib.IsUnix()) //The above is broken on mono
					{
						System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
						psi.Arguments = DebugFile;
						psi.FileName = "xdg-open";
						System.Diagnostics.Process.Start(psi);
					}
				}
			}
		}

		private static System.Xml.XmlWriter m_xw = null;
		private static System.IO.StringWriter m_sw = null;
		private static void StartXml()
		{
			m_sw = new System.IO.StringWriter();
			System.Xml.XmlWriterSettings ws = new System.Xml.XmlWriterSettings();
			ws.OmitXmlDeclaration = true;
			ws.Indent = true;
			ws.IndentChars = "\t";
			m_xw = System.Xml.XmlWriter.Create(m_sw, ws);
		}

		private static string Xml
		{
			get
			{
				if (m_xw == null) return string.Empty;
				m_sw.Flush();
				m_xw.Flush();
				string s = m_sw.ToString();
				m_xw = null;
				m_sw = null;
				return s;
			}
		}

		public static string DebugMessages
		{
			get
			{
				StartXml();
				LoadPluginNames(null, null);
				string sEncoding = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n";
				m_xw.WriteStartElement("DebugInfo");
				#region General info
				m_xw.WriteStartElement("General");
				m_xw.WriteStartElement("Plugin");
				m_xw.WriteElementString("PluginName", PluginName);
				m_xw.WriteElementString("PluginVersion", PluginVersion);
				m_xw.WriteEndElement();
				m_xw.WriteStartElement("DebugTime");
				m_xw.WriteElementString("DebugStart", m_Start.ToString("yyyyMMddTHHmmssZ"));
				m_xw.WriteElementString("DebugEnd", DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ"));
				m_xw.WriteEndElement();
				m_xw.WriteElementString("LogLevel", LogLevel.ToString());

				#region Add OS info
				string os = string.Empty;
				if (KeePass.Util.WinUtil.IsWindows9x)
					os = "Windows 9x";
				else if (KeePass.Util.WinUtil.IsWindows2000)
					os = "Windows 2000";
				else if (KeePass.Util.WinUtil.IsWindowsXP)
					os = "Windows XP";
				else
				{
					if (KeePass.Util.WinUtil.IsAtLeastWindows10)
						os = ">= Windows 10";
					else if (KeePass.Util.WinUtil.IsAtLeastWindows8)
						os = ">= Windows 8";
					else if (KeePass.Util.WinUtil.IsAtLeastWindows7)
						os = ">= Windows 7";
					else if (KeePass.Util.WinUtil.IsAtLeastWindowsVista)
						os = ">= Windows Vista";
					else if (KeePass.Util.WinUtil.IsAtLeastWindows2000)
						os = ">= Windows 2000";
					else os = "Unknown";
				}
				if (KeePass.Util.WinUtil.IsAppX)
					os += " (AppX)";
				os += " - " + Environment.OSVersion.ToString();
				m_xw.WriteElementString("OS", KeePass.Util.WinUtil.GetOSStr() + " " + os);
				#endregion
				m_xw.WriteElementString("DotNet", DotNetVersion.ToString() + (m_DotNetRelease > 0 ? " (" + m_DotNetRelease.ToString() + ")" : string.Empty));
				m_xw.WriteElementString("KeePass", Tools.KeePassVersion.ToString());

				m_xw.WriteStartElement("LoadedPlugins");
				foreach (KeyValuePair<string, Version> kvp in m_plugins)
				{
					m_xw.WriteStartElement("Plugin");
					m_xw.WriteElementString("PluginName", kvp.Key);
					m_xw.WriteElementString("PluginVersion", kvp.Value.ToString());
					m_xw.WriteEndElement();
				}
				m_xw.WriteEndElement();
				m_xw.WriteEndElement();
				#endregion

				if (m_DebugEntries.Count == 0)
					m_xw.WriteElementString("DebugMessages", null);
				else
				{
					m_xw.WriteStartElement("DebugMessages");
					foreach (var m in m_DebugEntries)
						m.GetXml(m_xw);
					m_xw.WriteEndElement();
				}

				m_xw.WriteEndElement();
				return sEncoding + Xml;
			}
		}

		private static bool m_Saving = false;
		public static void SaveDebugMessages()
		{
			if (m_Saving) return;
			m_Saving = true;
			try
			{
				System.IO.File.WriteAllText(DebugFile, DebugMessages);
			}
			catch (Exception ex)
			{
				Tools.ShowError("Can't save debug file: " + DebugFile + "\n\n" + ex.Message);
			}
			m_Saving = false;
		}

		private static bool m_bAllPluginsLoaded = false;
		private static void LoadPluginNames(object sender, EventArgs e)
		{
			if (m_bAllPluginsLoaded) return;
			m_plugins = Tools.GetLoadedPluginsName();
			if (sender == null) return;
			m_bAllPluginsLoaded = true;
			KeePass.Program.MainForm.FormLoadPost -= LoadPluginNames;
		}

		private class DebugEntry
		{
			public LogLevelFlags severity;
			public string msg;
			public DateTime utc;
			public int counter;
			public List<System.Diagnostics.StackFrame> sf = new List<System.Diagnostics.StackFrame>();
			public string[] parameters = null;

			public void GetXml(System.Xml.XmlWriter xw)
			{
				xw.WriteStartElement("DebugEntry");
				xw.WriteElementString("Message", msg);
				xw.WriteElementString("Counter", counter.ToString());
				xw.WriteElementString("Severity", severity.ToString());
				xw.WriteElementString("DateTimeUtc", utc.ToString("yyyyMMddTHHmmssZ"));
				if ((parameters == null) || parameters.Length == 0)
					xw.WriteElementString("Parameters", null);
				else
				{
					xw.WriteStartElement("Parameters");
					foreach (string p in parameters)
						xw.WriteElementString("Param", p);
					xw.WriteEndElement();
				}
				if (sf.Count == 0)
					xw.WriteElementString("StackFrames", null);
				else
				{
					xw.WriteStartElement("StackFrames");
					foreach (var f in sf)
					{
						xw.WriteStartElement("StackFrame");
						xw.WriteElementString("Method", f.GetMethod().Name + " (" + f.GetMethod().DeclaringType.FullName + ")");
						xw.WriteElementString("FileName", System.IO.Path.GetFileName(f.GetFileName()));
						xw.WriteElementString("Line", f.GetFileLineNumber().ToString());
						xw.WriteEndElement();
					}
					xw.WriteEndElement();
				}
				xw.WriteEndElement();
			}
		}
	}
}