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
			if (!DebugMode && (KeePass.Program.CommandLineArgs["debugplugin"] != null))
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