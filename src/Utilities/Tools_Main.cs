using KeePass.UI;
using System;
using System.Windows.Forms;

namespace PluginTools
{
	public static partial class Tools
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
			m_sPluginClassname = typeof(Tools).Assembly.GetName().Name + "Ext";
		}

		public static void OpenUrl(string sURL)
		{
			OpenUrl(sURL, null);
		}

		public static void OpenUrl(string sURL, KeePassLib.PwEntry pe)
		{
			//Use KeePass built-in logic instead of System.Diagnostics.Process.Start
			//For details see: https://sourceforge.net/p/keepass/discussion/329221/thread/f399b6d74b/#4801 
			KeePass.Util.WinUtil.OpenUrl(sURL, pe, true);
		}

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
}