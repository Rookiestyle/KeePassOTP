using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Reflection;

using KeePass;
using KeePass.Plugins;
using KeePass.Util;
using KeePass.Util.Spr;
using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Utility;

using PluginTranslation;
using PluginTools;
using KeePassLib.Serialization;
using KeePass.DataExchange;
using KeePassLib.Collections;

namespace KeePassOTP
{
	public partial class KeePassOTPExt : Plugin
	{
		#region members
		private IPluginHost m_host = null;

		//menu stuff
		private ToolStripMenuItem m_ContextMenu;
		private ToolStripMenuItem m_ContextMenuCopy;
		private ToolStripMenuItem m_ContextMenuSetup;
		private ToolStripMenuItem m_ContextMenuAutotype;
		private ToolStripMenuItem m_ContextMenuQRCode;
		private ToolStripMenuItem m_MainMenu;
		private ToolStripMenuItem m_MainMenuCopy;
		private ToolStripMenuItem m_MainMenuSetup;
		private ToolStripMenuItem m_MainMenuAutotype;
		private ToolStripMenuItem m_MainMenuQRCode;
		private ToolStripMenuItem m_TrayMenu;
		private ToolStripMenuItem m_Options;
		private static Image Icon_Setup = GfxUtil.ScaleImage(Resources.KeePassOTP_setup, 16, 16);

		MethodInfo m_miAutoType = null;

		//column handling
		private KeePassOTPColumnProvider m_columnOTP = null;

		private MethodInfo m_miSetForegroundWindowEx = null;
		private MethodInfo m_miGetForegroundWindowHandle = null;
		#endregion

		public override bool Initialize(IPluginHost host)
		{
			Terminate();
			if (host == null) return false;
			m_host = host;

			PluginTranslate.Init(this, Program.Translation.Properties.Iso6391Code);
			Tools.DefaultCaption = PluginTranslate.PluginName;
			Tools.PluginURL = "https://github.com/rookiestyle/keepassotp/";

			var t = typeof(Program).Assembly.GetType("KeePass.Native.NativeMethods");

			try { m_miSetForegroundWindowEx = t.GetMethod("SetForegroundWindowEx", BindingFlags.Static | BindingFlags.NonPublic); } catch { }
			if (m_miSetForegroundWindowEx == null) PluginDebug.AddError("Could not locate method 'SetForegroundEx'", 0);

			try { m_miGetForegroundWindowHandle = t.GetMethod("GetForegroundWindowHandle", BindingFlags.Static | BindingFlags.NonPublic); } catch { }
			if (m_miGetForegroundWindowHandle == null) PluginDebug.AddError("Could not locate method 'GetForegroundWindowHandle'", 0);

			m_miAutoType = m_host.MainWindow.GetType().GetMethod("ExecuteGlobalAutoType", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(string) }, null);
			if (m_miSetForegroundWindowEx == null) PluginDebug.AddError("Could not locate method 'ExecuteGlobalAutoType'", 0);

			CreateMenu();
			AddTray();

			Config.Init();

			m_columnOTP = new KeePassOTPColumnProvider();
			m_host.ColumnProviderPool.Add(m_columnOTP);
			m_columnOTP.StartTimer();

			SprEngine.FilterCompile += SprEngine_FilterCompile;
			SprEngine.FilterPlaceholderHints.Add(Config.Placeholder);

			TFASites.Init(false);
			OTPDAO.Init();
			PTHotKeyManager.HotKeyPressed += PTHotKeyManager_HotKeyPressed;
			m_host.MainWindow.FileOpened += MainWindow_FileOpened;

			return true;
		}

		private void PTHotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
		{
			//Check whether KeePass is in foreground
			//
			//If KeePass is in foreground: Copy OTP of selected entry
			//If KeePass is NOT in foreground: Invoke global Auto-Type
			if (m_miGetForegroundWindowHandle != null)
			{
				IntPtr pForegroundHandle = (IntPtr)m_miGetForegroundWindowHandle.Invoke(null, null);
				if (pForegroundHandle == m_host.MainWindow.Handle)
				{
					OnOTPCopy(sender, e);
					return;
				}
			}
			if (m_miAutoType != null)
				m_miAutoType.Invoke(m_host.MainWindow, new object[] { Config.Placeholder });
		}

		private void MainWindow_FileOpened(object sender, KeePass.Forms.FileOpenedEventArgs e)
		{
			KPOTP.GetTimingsAsync(e.Database);
		}

		#region Entry context menu
		private void OnEntryContextMenuOpening(object sender, EventArgs e)
		{
			m_ContextMenuCopy.ShortcutKeys = m_MainMenuCopy.ShortcutKeys = Config.Hotkey;
			if (m_host.MainWindow.GetSelectedEntriesCount() != 1)
			{
				m_ContextMenu.Enabled = m_ContextMenuAutotype.Enabled = false;
				m_MainMenu.Enabled = m_MainMenuAutotype.Enabled = false;
			}
			else
			{
				KPOTP myOTP = OTPDAO.GetOTP(m_host.MainWindow.GetSelectedEntry(true));
				m_ContextMenu.Enabled = m_MainMenu.Enabled = true;
				m_ContextMenuCopy.Enabled = m_ContextMenuAutotype.Enabled = m_ContextMenuQRCode.Enabled = myOTP.Valid;
				m_MainMenuCopy.Enabled = m_MainMenuAutotype.Enabled = m_MainMenuQRCode.Enabled = myOTP.Valid;
			}
		}

		private void OnEntryContextMenuClosing(object sender, EventArgs e)
		{
			m_ContextMenuCopy.ShortcutKeys = m_MainMenuCopy.ShortcutKeys = Keys.None;
		}

		private void OnOTPSetup(object sender, EventArgs e)
		{
			if (m_host.MainWindow.GetSelectedEntriesCount() != 1) return;
			PwEntry pe = m_host.MainWindow.GetSelectedEntry(true);
			if (!OTPDAO.EnsureOTPSetupPossible(pe)) return;
			var otpSetup = new KeePassOTPSetup();
			Tools.GlobalWindowManager(otpSetup);
			otpSetup.OTP = OTPDAO.GetOTP(pe);
			otpSetup.EntryUrl = pe.Strings.GetSafe(PwDefs.UrlField).ReadString();
			otpSetup.InitEx();
			if (otpSetup.ShowDialog(m_host.MainWindow) == DialogResult.OK)
				OTPDAO.SaveOTP(otpSetup.OTP, pe);
			otpSetup.Dispose();
		}

		private void OnOTPCopy(object sender, EventArgs e)
		{
			if (m_host.MainWindow.GetSelectedEntriesCount() != 1) return;
			CopyOTP(m_host.MainWindow.GetSelectedEntry(true));
		}


		private void OnOTPQRCode(object sender, EventArgs e)
		{
			if (m_host.MainWindow.GetSelectedEntriesCount() != 1) return;
			KPOTP otp = OTPDAO.GetOTP(m_host.MainWindow.GetSelectedEntry(true));
			if (!otp.Valid) return;
			try
			{
				byte[] bOTP = otp.OTPAuthString.ReadUtf8();
				QRCoder.QRCodeData qrd = QRCoder.QRCodeGenerator.GenerateQrCode(bOTP, QRCoder.QRCodeGenerator.ECCLevel.Q);
				MemUtil.ZeroByteArray(bOTP);
				QRCoder.QRCode qrc = new QRCoder.QRCode(qrd);
				Bitmap bmp = qrc.GetGraphic(8);
				QRForm f = new QRForm();
				f.FormBorderStyle = FormBorderStyle.FixedDialog;
				f.StartPosition = FormStartPosition.CenterParent;
				f.Text = PluginTranslate.PluginName;
				f.MinimizeBox = false;
				f.MaximizeBox = false;
				PictureBox pb = new PictureBox();
				pb.Size = new Size(bmp.Width, bmp.Height);
				pb.Location = new Point(0, 0);
				f.ClientSize = pb.Size;
				pb.Image = bmp;
				f.Controls.Add(pb);
				if (!string.IsNullOrEmpty(otp.Issuer) && (otp.Issuer != PluginTranslate.PluginName))
				{
					Label lIssuer = new Label();
					lIssuer.Width = f.ClientSize.Width;
					lIssuer.Text = otp.Issuer;
					lIssuer.Location = new Point(0, f.ClientSize.Height + 10);
					f.Controls.Add(lIssuer);
					f.Height += lIssuer.Height + 10;
				}
				if (!string.IsNullOrEmpty(otp.Label))
				{
					Label lLabel = new Label();
					lLabel.Width = f.ClientSize.Width;
					lLabel.Text = otp.Label;
					lLabel.Location = new Point(0, f.ClientSize.Height + 10);
					f.Controls.Add(lLabel);
					f.Height += lLabel.Height + 10;
				}
				f.Height += 5;
				Timer tClose = new Timer();
				tClose.Interval = 30000;
				tClose.Tick += (o, e1) => 
				{
					tClose.Stop();
					tClose.Dispose();
					if (f != null) f.Close();
				};
				f.Shown += (o, e2) => 
				{
					KeePass.UI.GlobalWindowManager.AddWindow(f, f);
					tClose.Start();
				};
				f.FormClosed += (o, e1) => { if (f != null) KeePass.UI.GlobalWindowManager.RemoveWindow(f); };
				f.ShowDialog(KeePass.UI.GlobalWindowManager.TopWindow);
				pb.Image.Dispose();
				f.Dispose();
				qrc.Dispose();
				qrd.Dispose();
			}
			catch { }
		}

		private void OnOTPAutotype(object sender, EventArgs e)
		{
			PwEntry pe = m_host.MainWindow.GetSelectedEntry(false);
			if (pe == null) return;
			AutotypeOTP(pe, false);
		}
		#endregion

		#region Options & menu definition
		private void CreateMenu()
		{
			m_host.MainWindow.EntryContextMenu.Opening += OnEntryContextMenuOpening;
			m_host.MainWindow.EntryContextMenu.Closing += OnEntryContextMenuClosing;

			m_ContextMenu = new ToolStripMenuItem(PluginTranslate.PluginName);
			m_ContextMenu.Image = Icon_Setup;
			m_ContextMenuCopy = new ToolStripMenuItem(PluginTranslate.OTPCopy);
			m_ContextMenuCopy.Click += OnOTPCopy;
			m_ContextMenuCopy.Image = Icon_Setup;
			m_ContextMenuQRCode = new ToolStripMenuItem(PluginTranslate.OTPQRCode);
			m_ContextMenuQRCode.Click += OnOTPQRCode;
			m_ContextMenuQRCode.Image = Icon_Setup;
			m_ContextMenuSetup = new ToolStripMenuItem(PluginTranslate.OTPSetup);
			m_ContextMenuSetup.Click += OnOTPSetup;
			m_ContextMenuSetup.Image = Icon_Setup;
			m_ContextMenu.DropDownItems.Add(m_ContextMenuCopy);
			m_ContextMenu.DropDownItems.Add(m_ContextMenuQRCode);
			m_ContextMenu.DropDownItems.Add(m_ContextMenuSetup);
			m_host.MainWindow.EntryContextMenu.Items.Insert(m_host.MainWindow.EntryContextMenu.Items.Count, m_ContextMenu);
			m_ContextMenuAutotype = new ToolStripMenuItem();

			m_MainMenu = new ToolStripMenuItem(PluginTranslate.PluginName);
			m_MainMenu.Image = Icon_Setup;
			m_MainMenuCopy = new ToolStripMenuItem(PluginTranslate.OTPCopy);
			m_MainMenuCopy.Click += OnOTPCopy;
			m_MainMenuCopy.Image = Icon_Setup;
			m_MainMenuQRCode = new ToolStripMenuItem(PluginTranslate.OTPQRCode);
			m_MainMenuQRCode.Click += OnOTPQRCode;
			m_MainMenuQRCode.Image = Icon_Setup;
			m_MainMenuSetup = new ToolStripMenuItem(PluginTranslate.OTPSetup);
			m_MainMenuSetup.Click += OnOTPSetup;
			m_MainMenuSetup.Image = Icon_Setup;
			m_MainMenu.DropDownItems.Add(m_MainMenuCopy);
			m_MainMenu.DropDownItems.Add(m_MainMenuQRCode);
			m_MainMenu.DropDownItems.Add(m_MainMenuSetup);
			m_MainMenuAutotype = new ToolStripMenuItem();

			try
			{
				ToolStripMenuItem last = m_host.MainWindow.MainMenu.Items["m_menuEntry"] as ToolStripMenuItem;
				last.DropDownOpening += OnEntryContextMenuOpening;
				last.DropDownClosed += OnEntryContextMenuClosing;
				last.DropDownItems.Add(m_MainMenu);
			}
			catch { }

			ToolStripItem[] autotypes = m_host.MainWindow.EntryContextMenu.Items.Find("m_ctxEntryAutoTypeAdv", true);
			if (autotypes.Length != 0)
			{
				ToolStripMenuItem autotype = (ToolStripMenuItem)autotypes[0];
				m_ContextMenuAutotype.Text = Config.Placeholder;
				m_MainMenuAutotype.Name = PluginTranslate.PluginName + "AutoTypeContextMenu";
				m_ContextMenuAutotype.Image = autotype.DropDownItems[0].Image;
				m_ContextMenuAutotype.Click += OnOTPAutotype;
				autotype.DropDownItems.Add(m_ContextMenuAutotype);
			}

			autotypes = m_host.MainWindow.MainMenu.Items.Find("m_menuEntryAutoTypeAdv", true);
			if (autotypes.Length != 0)
			{
				ToolStripMenuItem autotype = (ToolStripMenuItem)autotypes[0];
				m_MainMenuAutotype.Text = Config.Placeholder;
				m_MainMenuAutotype.Name = PluginTranslate.PluginName+"AutoTypeMainMenu";
				m_MainMenuAutotype.Image = autotype.DropDownItems[0].Image;
				m_MainMenuAutotype.Click += OnOTPAutotype;
				autotype.DropDownItems.Add(m_MainMenuAutotype);
			}

			m_Options = new ToolStripMenuItem(PluginTranslate.PluginName + "...");
			m_Options.Image = SmallIcon;
			m_Options.Click += (o, e) => Tools.ShowOptions();
			m_host.MainWindow.ToolsMenu.DropDownItems.Add(m_Options);

			Tools.OptionsFormShown += OptionsFormShown;
			Tools.OptionsFormClosed += OptionsFormClosed;
		}

		private void OptionsFormShown(object sender, Tools.OptionsFormsEventArgs e)
		{
			Options options = new Options();
			options.cgbCheckTFA.Checked = Config.CheckTFA;
			options.hkcKPOTP.HotKey = Config.Hotkey;
			options.tbPlaceholder.Text = Config.Placeholder;
			Dictionary<PwDatabase, Options.DBSettings> dDB = new Dictionary<PwDatabase, Options.DBSettings>();
			foreach (PwDatabase db in m_host.MainWindow.DocumentManager.GetOpenDatabases())
				dDB[db] = new Options.DBSettings { UseOTPDB = db.UseDBForOTPSeeds(), Preload = db.PreloadOTPDB() };
			options.InitEx(dDB, m_host.Database);
			Tools.AddPluginToOptionsForm(this, options);
		}

		private Timer m_tMigratePlaceholder = null;
		private void OptionsFormClosed(object sender, Tools.OptionsFormsEventArgs e)
		{
			if (e.form.DialogResult != DialogResult.OK) return;
			bool shown;
			Options options = (Options)Tools.GetPluginFromOptions(this, out shown);
			if (!shown) return;

			Dictionary<PwDatabase, Options.DBSettings> dDB = options.OTPDBSettings;
			foreach (KeyValuePair<PwDatabase, Options.DBSettings> kvp in dDB)
			{
				kvp.Key.UseDBForOTPSeeds(kvp.Value.UseOTPDB);
				kvp.Key.PreloadOTPDB(kvp.Value.Preload);
			}

			Config.CheckTFA = options.cgbCheckTFA.Checked;
			Config.Hotkey = options.hkcKPOTP.HotKey;
			string sOldPlaceholder = Config.Placeholder;
			Config.Placeholder = options.tbPlaceholder.Text;
			if ((sOldPlaceholder != Config.Placeholder)
				&& (Tools.AskYesNo(string.Format(PluginTranslate.MigratePlaceholder, sOldPlaceholder, Config.Placeholder)) == DialogResult.Yes))
				MigratePlacholderInOpenDatabases(sOldPlaceholder, Config.Placeholder);
			if (m_ContextMenuAutotype != null) m_ContextMenuAutotype.Text = Config.Placeholder;
			if (m_MainMenuAutotype != null) m_MainMenuAutotype.Text = Config.Placeholder;
			TFASites.Init(false);
		}

		private void MigratePlacholderInOpenDatabases(string sOldPlaceholder, string placeholder)
		{
			//This method is called when the options form is removed by GlobalWindowManager
			//GlobalWindowManager.WindowCount is adjusted AFTERWARDS
			//OpenDatabase will exit if GlobalWindowManager.WindowCount != 0
			//We will wait 1 second and then try to migrate the placeholders...
			m_tMigratePlaceholder = new Timer();
			m_tMigratePlaceholder.Interval = 1000;
			m_tMigratePlaceholder.Tick += (o, e1) =>
			{
				if (KeePass.UI.GlobalWindowManager.WindowCount != 0) return;
				m_tMigratePlaceholder.Stop();
				m_tMigratePlaceholder = null;
				MigrationBase m = new MigrationBase();
				bool bChanged = false;
				foreach (KeePass.UI.PwDocument doc in m_host.MainWindow.DocumentManager.Documents)
				{
					if (doc.Database == null) continue;
					if (!doc.Database.IsOpen) m_host.MainWindow.OpenDatabase(doc.LockedIoc, null, doc.LockedIoc.IsLocalFile());
					if (!doc.Database.IsOpen) continue;
					m.SetDB(doc.Database);
					if (m.MigratePlaceholder(sOldPlaceholder, Config.Placeholder, false))
					{
						doc.Database.Modified = true;
						bChanged = true;
					}
					m.SetDB(null);
				}
				if (bChanged)
					m_host.MainWindow.UpdateUI(false, null, false, null, false, null, (m_host.Database != null) && m_host.Database.IsOpen && m_host.Database.Modified);
			};
			m_tMigratePlaceholder.Start();
		}

		private void RemoveMenu()
		{
			m_host.MainWindow.ToolsMenu.DropDownItems.Remove(m_Options);
			m_host.MainWindow.EntryContextMenu.Opening -= OnEntryContextMenuOpening;
			m_host.MainWindow.EntryContextMenu.Closing -= OnEntryContextMenuClosing;
			m_host.MainWindow.EntryContextMenu.Items.Remove(m_ContextMenu);

			try
			{
				(m_MainMenu.OwnerItem as ToolStripMenuItem).DropDownOpening -= OnEntryContextMenuOpening;
				(m_MainMenu.OwnerItem as ToolStripMenuItem).DropDownClosed -= OnEntryContextMenuClosing;
				m_MainMenu.Owner.Items.Remove(m_MainMenu);
			}
			catch { }

			if (m_ContextMenuCopy.Owner != null) m_ContextMenuAutotype.Owner.Items.Remove(m_ContextMenuAutotype);
			if (m_MainMenuCopy.Owner != null) m_MainMenuCopy.Owner.Items.Remove(m_MainMenuCopy);
			if (m_ContextMenuQRCode.Owner != null) m_ContextMenuQRCode.Owner.Items.Remove(m_ContextMenuQRCode);
			if (m_MainMenuQRCode.Owner != null) m_MainMenuQRCode.Owner.Items.Remove(m_MainMenuQRCode);
			if (m_ContextMenuAutotype.Owner != null) m_ContextMenuAutotype.Owner.Items.Remove(m_ContextMenuAutotype);
			if (m_MainMenuAutotype.Owner != null) m_MainMenuAutotype.Owner.Items.Remove(m_MainMenuAutotype);

			Tools.OptionsFormShown -= OptionsFormShown;
			Tools.OptionsFormClosed -= OptionsFormClosed;
		}
		#endregion

		#region Tray icon
		private void AddTray()
		{
			m_TrayMenu = new ToolStripMenuItem();
			m_host.MainWindow.TrayContextMenu.Items.Insert(0, m_TrayMenu);
			m_host.MainWindow.TrayContextMenu.Opening += OnTrayOpening;
		}

		private void RemoveTray()
		{
			m_host.MainWindow.TrayContextMenu.Opening -= OnTrayOpening;
			if ((m_TrayMenu == null) || m_TrayMenu.IsDisposed) return;
			m_host.MainWindow.TrayContextMenu.Items.Remove(m_TrayMenu);
			m_TrayMenu.Dispose();
		}

		private void OnTrayOpening(object sender, EventArgs e)
		{
			PluginDebug.AddInfo("Tray setup: Start", 0);
			m_TrayMenu.DropDownItems.Clear();
			List<PwDatabase> lDB = m_host.MainWindow.DocumentManager.GetOpenDatabases();
			SearchParameters sp = new SearchParameters();
			sp.ExcludeExpired = !Program.Config.Integration.AutoTypeExpiredCanMatch; //exclude expired entries only if they can not match
			sp.SearchInStringNames = true;
			Dictionary<string, PwObjectList<PwEntry>> dEntries = new Dictionary<string, PwObjectList<PwEntry>>();
			foreach (PwDatabase db in lDB)
			{
				string dbName = UrlUtil.GetFileName(db.IOConnectionInfo.Path);
				if (!string.IsNullOrEmpty(db.Name))
					dbName = db.Name + " (" + dbName + ")";
				PwObjectList<PwEntry> entries = new PwObjectList<PwEntry>();
				if (Config.UseDBForOTPSeeds(db))
					sp.SearchString = OTPDAO.OTPHandler_DB.DBNAME;
				else
					sp.SearchString = Config.OTPFIELD; // Config.SEED + " " + Config.SETTINGS;
				db.RootGroup.SearchEntries(sp, entries);

				PluginDebug.AddInfo("Tray setup: Check database", 0, "DB: " + dbName, "Entries: " + entries.UCount.ToString());
				if ((entries == null) || (entries.UCount == 0)) continue;
				//Ignore deleted entries
				PwGroup pgRecycle = db.RecycleBinEnabled ? db.RootGroup.FindGroup(db.RecycleBinUuid, true) : null;
				if (pgRecycle != null)
				{
					for (int i = (int)entries.UCount - 1; i >= 0; i--)
					{
						PwEntry pe = entries.GetAt((uint)i);
						if (pe.IsContainedIn(pgRecycle))
							entries.Remove(pe);
					}
				}
				entries.Sort(SortEntries);
				dEntries.Add(dbName, entries);
			}
			foreach (var kvp in dEntries)
			{
				ToolStripMenuItem parent = null;
				//If entries of only one DB are found don't include the DB as additional menu level
				if (dEntries.Count == 1)
					parent = m_TrayMenu;
				else
				{
					parent = new ToolStripMenuItem(kvp.Key);
					m_TrayMenu.DropDownItems.Add(parent);
				}
				foreach (PwEntry pe in kvp.Value)
				{
					ToolStripMenuItem entry = new ToolStripMenuItem();
					string[] text = GetTrayText(pe);
					PluginDebug.AddInfo("Tray setup: Add entry", 0, "Uuid: " + text[2]); // Do NOT add username and entry title to debugfile
					if (text[0] == string.Empty)
						entry.Text = StrUtil.EncodeMenuText(string.Format(PluginTranslate.User, text[1]));
					else if (text[1] == string.Empty)
						entry.Text = StrUtil.EncodeMenuText(text[0]);
					else
						entry.Text = StrUtil.EncodeMenuText(text[0] + " (" + text[1] + ")");
					entry.Name = "KPOTP_" + pe.Uuid.ToString();
					entry.Click += OnOTPTray;
					entry.Tag = pe;
					parent.DropDownItems.Add(entry);
				}
			}
			m_TrayMenu.Enabled = dEntries.Count > 0;
			m_TrayMenu.Text = m_TrayMenu.Enabled ? PluginTranslate.OTPCopyTrayEntries : PluginTranslate.OTPCopyTrayNoEntries;
		}

		private string[] GetTrayText(PwEntry pe)
		{
			string title = pe.Strings.ReadSafe(PwDefs.TitleField);
			string user = pe.Strings.ReadSafe(PwDefs.UserNameField);
			string sHex = pe.Uuid.ToHexString();
			if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(user))
				return new string[] { string.Format(PluginTranslate.Group, pe.ParentGroup.Name), string.Format(PluginTranslate.Empty, sHex) };
			if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(user))
				return new string[] { title, user, sHex };
			if (string.IsNullOrEmpty(user))
				return new string[] { string.Format(PluginTranslate.Title, title), string.Empty, sHex };
			return new string[] { string.Format(PluginTranslate.User, user), string.Empty, sHex };
		}

		private int SortEntries(PwEntry a, PwEntry b)
		{
			if ((a == null) && (b == null)) return 0;
			if (a == null) return -1;
			if (b == null) return 1;
			string[] textA = GetTrayText(a);
			string[] textB = GetTrayText(b);
			int sort1 = string.Compare(textA[0], textB[0]);
			int sort2 = string.Compare(textA[1], textB[1]);
			string empty = PluginTranslate.Empty.Replace("{0}", "");
			if (textA[1].Contains(empty) == textB[1].Contains(empty))
			{
				if (!textA[1].Contains(empty) && (sort1 != 0)) return sort1;
				return sort2;
			}
			else if (textA[1].Contains(empty))
				return 1;
			else
				return -1;
		}

		private void OnOTPTray(object sender, EventArgs e)
		{
			PwEntry pe = (PwEntry)((sender as ToolStripMenuItem).Tag);
			if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
				CopyOTP(pe);
			else
				AutotypeOTP(pe, true);
		}
		#endregion

		internal static bool CopyOTP(PwEntry pe)
		{
			if (!OTPDAO.EnsureOTPUsagePossible(pe)) 
			{
				PluginDebug.AddError("Copy OTP failed", 0, "Uuid: " + pe.Uuid.ToHexString(), "OTP db not unlocked");
				return false;
			}
			KPOTP myOTP = OTPDAO.GetOTP(pe);
			if (!myOTP.Valid)
			{
				PluginDebug.AddError("Copy OTP failed", 0, "Uuid: " + pe.Uuid.ToHexString());
				return false;
			}
			ClipboardUtil.CopyAndMinimize(myOTP.GetOTP(false, true), true, Program.MainForm, pe, Program.MainForm.DocumentManager.SafeFindContainerOf(pe));
			Program.MainForm.StartClipboardCountdown();
			PluginDebug.AddInfo("Copy OTP success", 0, "Uuid: " + pe.Uuid.ToString());
			if (myOTP.Type == KPOTPType.HOTP)
			{
				myOTP.HOTPCounter++;
				OTPDAO.SaveOTP(myOTP, pe);
			}
			return true;
		}

		private void AutotypeOTP(PwEntry pe, bool FromTray)
		{
			try
			{
				if (FromTray)
				{
					if (m_miSetForegroundWindowEx != null)
					{
						FormWindowState ws = Program.MainForm.WindowState;
						m_miSetForegroundWindowEx.Invoke(null, new object[] { Program.GetSafeMainWindowHandle() });
						AutoType.PerformIntoPreviousWindow(m_host.MainWindow, pe, m_host.MainWindow.DocumentManager.SafeFindContainerOf(pe), Config.Placeholder);
						if (ws == FormWindowState.Minimized) Program.MainForm.WindowState = ws;
					}
				}
				else
					AutoType.PerformIntoPreviousWindow(m_host.MainWindow, pe, m_host.MainWindow.DocumentManager.SafeFindContainerOf(pe), Config.Placeholder);
			}
			catch (Exception) { }
		}

		private void SprEngine_FilterCompile(object sender, SprEventArgs e)
		{
			if (e.Text.IndexOf(Config.Placeholder, StringComparison.InvariantCultureIgnoreCase) >= 0)
			{
				OTPDAO.EnsureOTPUsagePossible(e.Context.Entry);
				KPOTP myOTP = OTPDAO.GetOTP(e.Context.Entry);
				if (!myOTP.Valid)
					PluginDebug.AddError("Auto-Type OTP failed", 0, "Uuid: " + e.Context.Entry.Uuid.ToHexString());
				else
					PluginDebug.AddInfo("Auto-Type OTP success", 0, "Uuid: " + e.Context.Entry.Uuid.ToHexString());
				e.Text = StrUtil.ReplaceCaseInsensitive(e.Text, Config.Placeholder, myOTP.GetOTP(false, true));
				if (myOTP.Valid && (myOTP.Type == KPOTPType.HOTP))
				{
					myOTP.HOTPCounter++;
					OTPDAO.SaveOTP(myOTP, e.Context.Entry);
				}
			}
		}

		public override void Terminate()
		{
			if (m_host == null) return;

			PTHotKeyManager.HotKeyPressed -= PTHotKeyManager_HotKeyPressed;

			OTPDAO.Cleanup();

			m_host.MainWindow.FileOpened -= MainWindow_FileOpened;

			SprEngine.FilterCompile -= SprEngine_FilterCompile;
			SprEngine.FilterPlaceholderHints.Remove(Config.Placeholder);

			m_columnOTP.StopTimer();
			m_host.ColumnProviderPool.Remove(m_columnOTP);

			Config.Cleanup();

			RemoveTray();
			RemoveMenu();

			PluginDebug.SaveOrShow();

			m_host = null;
		}

		public override string UpdateUrl
		{
			get { return "https://raw.githubusercontent.com/rookiestyle/keepassotp/master/version.info"; }
		}

		public override Image SmallIcon
		{
			get { return Icon_Setup; }
		}
	}
	public class QRForm: Form, KeePass.UI.IGwmWindow
	{
		public bool CanCloseWithoutDataLoss { get { return true; } }
	}
}