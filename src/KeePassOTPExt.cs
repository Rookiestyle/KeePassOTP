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
using KeePass.UI;
using KeePass.Forms;

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

		private MethodInfo m_miAutoType = null;

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

			CleanupColumns();

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

			GlobalWindowManager.WindowAdded += GlobalWindowManager_WindowAdded;

			return true;
		}

		private void CleanupColumns()
		{
			//Column KPOTP_Reduced has been removed (use KeePassOTP options instead)
			//If column is currently displayed do the following:
			// - Switch OTP display mode to reduced mode
			// - Replace KPOTP_Reduced column by KPOTP column
			var cReduced = Program.Config.MainWindow.EntryListColumns.FirstOrDefault(x => x.CustomName == "KPOTP_Reduced" && x.Type == KeePass.App.Configuration.AceColumnType.PluginExt);
			if (cReduced == null) return;

			var cFull = Program.Config.MainWindow.EntryListColumns.FirstOrDefault(x => x.CustomName == KeePassOTPColumnProvider.OTPColumn && x.Type == KeePass.App.Configuration.AceColumnType.PluginExt);
			if (cFull == null) cReduced.CustomName = KeePassOTPColumnProvider.OTPColumn;
			else
			{
				string sIndex = Program.Config.MainWindow.EntryListColumns.IndexOf(cReduced).ToString();
				Program.Config.MainWindow.EntryListColumnDisplayOrder = Program.Config.MainWindow.EntryListColumnDisplayOrder.Replace(sIndex + " ", string.Empty);
				Program.Config.MainWindow.EntryListColumnDisplayOrder = Program.Config.MainWindow.EntryListColumnDisplayOrder.Replace(" " + sIndex, string.Empty);
				Program.Config.MainWindow.EntryListColumns.Remove(cReduced);
			}
			Config.OTPDisplay = false;
		}

		private void GlobalWindowManager_WindowAdded(object sender, GwmWindowEventArgs e)
		{
			if (!m_bOTPHotkeyPressed) return;
			if (!(e.Form is AutoTypeCtxForm)) return;

			PluginDebug.AddInfo("Auto-Type entry selection window added", 0);

			List<AutoTypeCtx> lCtx = (List<AutoTypeCtx>)Tools.GetField("m_lCtxs", e.Form);
			if (lCtx == null) return;

			//Adjust sequence to show correct auto-type sequence
			//Remove lines that don't have KPOTP defined
			int PrevCount = lCtx.Count;
			lCtx.RemoveAll(x => OTPDAO.OTPDefined(x.Entry) == OTPDAO.OTPDefinition.None);
			PluginDebug.AddInfo("Removed sequences without valid OTP settings", 0,
				"Before: " + PrevCount.ToString(),
				"After: " + lCtx.Count.ToString());

			//If now 0 or 1 entries remain, we need to hook the Shown event
			//simply to close it
			//We do not want to display an entry selection form with less then 2 entries
			if (lCtx.Count < 2) e.Form.Shown += OnAutoTypeFormShown;
		}

		private void OnAutoTypeFormShown(object sender, EventArgs e)
		{
			AutoTypeCtxForm f = sender as AutoTypeCtxForm;
			ListView lv = Tools.GetControl("m_lvItems", f) as ListView;
			PluginDebug.AddInfo("Auto-Type entry selection window shown", 0);
			if ((lv != null) && (lv.Items.Count == 0) && !Program.Config.Integration.AutoTypeAlwaysShowSelDialog)
			{
				PluginDebug.AddInfo("Auto-Type Entry Selection window closed", 0, "Reason: No entries to display");
				f.Close();
				return;
			}
			if ((lv != null) && (lv.Items.Count == 1) && !Program.Config.Integration.AutoTypeAlwaysShowSelDialog)
			{
				lv.Items[0].Selected = true;
				try
				{
					MethodInfo miPIS = f.GetType().GetMethod("ProcessItemSelection", BindingFlags.NonPublic | BindingFlags.Instance);
					miPIS.Invoke(f, null);
					PluginDebug.AddInfo("Auto-Type Entry Selection window closed", 0, "Reason: Only one entry to be shown");
				}
				catch (Exception ex)
				{
					PluginDebug.AddError("Auto-Type Entry Selection window NOT closed", 0, "Reason: Could not process entry", "Details: " + ex.Message);
				}
				return;
			}
		}

		private bool m_bOTPHotkeyPressed = false;
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
			{
				m_bOTPHotkeyPressed = true;
				m_miAutoType.Invoke(m_host.MainWindow, new object[] { Config.Placeholder + (Config.KPOTPAutoSubmit ? "{ENTER}" : string.Empty) });
				m_bOTPHotkeyPressed = false;
			}
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
				ZXing.BarcodeWriter zBW = new ZXing.BarcodeWriter();
				zBW.Options.Height = 320;
				zBW.Options.Width = 320;
				zBW.Format = ZXing.BarcodeFormat.QR_CODE;
				Bitmap bmp = zBW.Write(otp.OTPAuthString.ReadString());
				QRForm f = new QRForm();
				f.FormBorderStyle = FormBorderStyle.FixedDialog;
				f.StartPosition = FormStartPosition.CenterParent;
				f.Text = PluginTranslate.PluginName;
				f.MinimizeBox = false;
				f.MaximizeBox = false;
				PictureBox pb = new PictureBox();
				pb.Location = new Point(0, 0);
				pb.Image = new Bitmap(bmp, bmp.Size); //Assigning bmp directly did not work in my Ubuntu VM...
				pb.ClientSize = pb.Image.Size;
				f.ClientSize = pb.Size;
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
				bmp.Dispose();
			}
			catch { };
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
				m_MainMenuAutotype.Name = PluginTranslate.PluginName + "AutoTypeMainMenu";
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
			PluginDebug.AddInfo("Prepare options page", 0);
			Options options = new Options();
			options.cbCheckTFA.Checked = Config.CheckTFA;
			options.hkcKPOTP.HotKey = Config.Hotkey;
			options.cbAutoSubmit.Checked = Config.KPOTPAutoSubmit;
			options.tbPlaceholder.Text = Config.Placeholder;
			Dictionary<PwDatabase, Options.DBSettings> dDB = new Dictionary<PwDatabase, Options.DBSettings>();
			foreach (PwDatabase db in m_host.MainWindow.DocumentManager.GetOpenDatabases())
				dDB[db] = new Options.DBSettings { UseOTPDB = db.UseDBForOTPSeeds(), Preload = db.PreloadOTPDB() };
			PluginDebug.AddInfo("Options page prepared, " + dDB.Count.ToString() + " open databases found", 0);
			options.InitEx(dDB, m_host.Database);
			PluginDebug.AddInfo(dDB.Count.ToString() + " open databases added to options page", 0);
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

			Config.CheckTFA = options.cbCheckTFA.Checked;
			Config.Hotkey = options.hkcKPOTP.HotKey;
			string sOldPlaceholder = Config.Placeholder;
			Config.KPOTPAutoSubmit = options.cbAutoSubmit.Checked;
			Config.Placeholder = options.tbPlaceholder.Text;
			Config.OTPDisplay = options.GetOTPDisplay();
			Config.OTPRenewal = options.GetOTPRenewal();
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
					if (m.MigratePlaceholder(sOldPlaceholder, Config.Placeholder))
					{
						doc.Database.Modified = true;
						bChanged = true;
					}
					m.SetDB(null);
				}
				if (bChanged)
					Tools.RefreshEntriesList((m_host.Database != null) && m_host.Database.IsOpen && m_host.Database.Modified);
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
			m_TrayMenu.Image = SmallIcon;
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
			List<Tray_Database> lDB_Entries = new List<Tray_Database>();
			DateTime dtExpired = DateTime.UtcNow;
			bool bAdjustedEntryColor = false;
			foreach (PwDatabase db in lDB)
			{
				var tdb = new Tray_Database(db);
				if (Config.UseDBForOTPSeeds(db))
				{
					tdb.Entries = db.RootGroup.GetEntries(true).Where(
						x => x.CustomData.Exists(OTPDAO.OTPHandler_DB.DBNAME)
							&& StrUtil.StringToBool(x.CustomData.Get(OTPDAO.OTPHandler_DB.DBNAME))).ToList();
				}
				else
				{
					tdb.Entries = db.RootGroup.GetEntries(true).Where(x => x.Strings.Exists(Config.OTPFIELD)).ToList();
				}
				if (!Program.Config.Integration.AutoTypeExpiredCanMatch) // Remove expired entries
				{
					tdb.Entries = tdb.Entries.Where(x => !x.Expires || x.ExpiryTime >= dtExpired).ToList();
				}
				PluginDebug.AddInfo("Tray setup: Check database", 0, "DB: " + tdb.DBName, "Entries: " + tdb.Entries.Count.ToString());
				if (tdb.Entries.Count == 0) continue;
				//Ignore deleted entries
				PwGroup pgRecycle = db.RecycleBinEnabled ? db.RootGroup.FindGroup(db.RecycleBinUuid, true) : null;
				if (pgRecycle != null)
				{
					for (int i = tdb.Entries.Count - 1; i >= 0; i--)
					{
						PwEntry pe = tdb.Entries[i];
						if (pe.IsContainedIn(pgRecycle))
							tdb.Entries.Remove(pe);
					}
				}
				tdb.Entries.Sort(SortEntries);
				lDB_Entries.Add(tdb);
			}
			foreach (var tdb in lDB_Entries)
			{
				ToolStripMenuItem parent = null;
				//If entries of only one DB are found don't include the DB as additional menu level
				if (lDB_Entries.Count == 1)
					parent = m_TrayMenu;
				else
				{
					parent = new ToolStripMenuItem(tdb.DBName);
					if (!UIUtil.ColorsEqual(tdb.DBColor, Color.Empty))
						parent.Image = UIUtil.CreateColorBitmap24(parent.Height, parent.Height, tdb.DBColor);
					m_TrayMenu.DropDownItems.Add(parent);
				}
				foreach (PwEntry pe in tdb.Entries)
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
					if (PwUuid.Zero != pe.CustomIconUuid)
						entry.Image = tdb.GetCustomIcon(pe.CustomIconUuid, entry.Height, entry.Height);
					if (entry.Image == null)
						entry.Image = m_host.MainWindow.ClientIcons.Images[(int)pe.IconId];

					bAdjustedEntryColor |= AdjustEntryColor(entry);
					parent.DropDownItems.Add(entry);
				}
			}
			m_TrayMenu.Enabled = lDB_Entries.Count > 0;
			m_TrayMenu.Text = m_TrayMenu.Enabled ? PluginTranslate.OTPCopyTrayEntries : PluginTranslate.OTPCopyTrayNoEntries;

			if (bAdjustedEntryColor) AdjustToolStripRender();
		}

		private void AdjustToolStripRender()
		{
			//KeePass uses one out of many own toolstrip renderers
			//If we use color coding, enforce usage of our colors
			//This is experimental and can be switched off in KeePass' config file!
			if (Config.TrayColorCodeMode != Config.Tray_ColorCoding.On) return;

			var rPrev = m_host.MainWindow.TrayContextMenu.Renderer;
			m_host.MainWindow.TrayContextMenu.Renderer = new Tray_Renderer(rPrev as ToolStripProfessionalRenderer);
			m_host.MainWindow.TrayContextMenu.Closed += CleanupRenderer;
		}

		private void CleanupRenderer(object sender, ToolStripDropDownClosedEventArgs e)
		{
			m_host.MainWindow.TrayContextMenu.Closed -= CleanupRenderer;

			//Don't restore
			//Set 'Renderer' to null instead to use use whatever is assigned to ToolStripManager.Renderer
			//This way, a change of the renderer will be considered next time the context menu is displayed
			m_host.MainWindow.TrayContextMenu.Renderer = null;
		}

		private bool AdjustEntryColor(ToolStripMenuItem entry)
		{
			//KeePass uses one out of many own toolstrip renderers
			//If we use color coding, enforce usage of our colors
			//This is experimental and can be switched off in KeePass' config file!
			bool bAdjusted = false;

			if (Config.TrayColorCodeMode == Config.Tray_ColorCoding.Off) return bAdjusted;

			PwEntry pe = entry.Tag as PwEntry;
			if (pe == null) return bAdjusted;

			if (!UIUtil.ColorsEqual(pe.ForegroundColor, Color.Empty) && !UIUtil.ColorsEqual(pe.ForegroundColor, entry.ForeColor))
			{
				entry.ForeColor = pe.ForegroundColor;
				bAdjusted = true;
			}
			if (!UIUtil.ColorsEqual(pe.BackgroundColor, Color.Empty) && !UIUtil.ColorsEqual(pe.BackgroundColor, entry.BackColor))
			{
				entry.BackColor = pe.BackgroundColor;
				bAdjusted = true;
			}
			if (bAdjusted) entry.Name = "KeePassOTP_Tray_" + pe.Uuid.ToHexString();
			return bAdjusted;
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
			CopyToClipboardAndStartClipboardCountdown(pe, myOTP);
			PluginDebug.AddInfo("Copy OTP success", 0, "Uuid: " + pe.Uuid.ToString());
			if (myOTP.Type == KPOTPType.HOTP)
			{
				myOTP.HOTPCounter++;
				OTPDAO.SaveOTP(myOTP, pe);
			}
			return true;
		}

		private static Timer m_tClipboardRenewalTimer = null;
		internal static void CopyToClipboardAndStartClipboardCountdown(PwEntry pe, KPOTP kOTP)
		{
			if (m_tClipboardRenewalTimer != null)
			{
				m_tClipboardRenewalTimer.Stop();
				m_tClipboardRenewalTimer.Dispose();
				m_tClipboardRenewalTimer = null;
			}

			//Copy OTP to clipboard
			string sOTP = kOTP.GetOTP(false, true);
			ClipboardUtil.CopyAndMinimize(sOTP, true, Program.MainForm, pe, Program.MainForm.DocumentManager.SafeFindContainerOf(pe));

			//Start clipboard countdown
			Program.MainForm.StartClipboardCountdown();

			//Check whether renewal is required
			if (Config.OTPRenewal == Config.OTPRenewal_Enum.Inactive) return;

			//No renewal for HOTP to ensure counters remain in sync
			if (kOTP.Type == KPOTPType.HOTP) return;

			int iOTPValiditiy = kOTP.RemainingSeconds;
			int iRemainingSeconds = Program.Config.Security.ClipboardClearAfterSeconds - iOTPValiditiy;
			int iAdditionalRenewals = 0;

			bool bRestartClipboardCountdownOnce = false;
			//Renew OTP once if it will expire soon 
			//This applies for both PreventShortDuration as well as RespectClipboardTimeout
			if (iOTPValiditiy < Config.TOTPSoonExpiring)
			{
				iAdditionalRenewals = 1;
				//If clipboard countdown is active but lower then Config.TOTPSoonExpiring
				//Restart clipboard countdown as well
				bRestartClipboardCountdownOnce = Program.Config.Security.ClipboardClearAfterSeconds > 0
					&& Program.Config.Security.ClipboardClearAfterSeconds <= Config.TOTPSoonExpiring;
			}
			else if (iRemainingSeconds < 0) //OTP lifetime is longer than clipboard countdown timer
				iAdditionalRenewals = 0;
			else if (Config.OTPRenewal == Config.OTPRenewal_Enum.RespectClipboardTimeout)
			{
				iAdditionalRenewals = 1;
				while (iRemainingSeconds > kOTP.TOTPTimestep)
				{
					iAdditionalRenewals++;
					iRemainingSeconds -= kOTP.TOTPTimestep;
				}
			}
			PluginDebug.AddInfo("OTP renewal",
				"Renewals: " + iAdditionalRenewals.ToString(),
				"RestartClipboardCountdown: " + bRestartClipboardCountdownOnce,
				"RenewalType: " + Config.OTPRenewal.ToString(),
				"ClipboardTimeout: " + Program.Config.Security.ClipboardClearAfterSeconds.ToString(),
				"OTP validity: " + iOTPValiditiy.ToString());

			if (iAdditionalRenewals < 1) return;
			//If current OTP will expire before the clipboard will be cleared, copy new one as soon as neccessary
			//Do _NOT_ extend the clipboard countdown

			m_tClipboardRenewalTimer = new Timer();
			m_tClipboardRenewalTimer.Tick += (o, e) =>
			{
				if (m_tClipboardRenewalTimer == null) return;
				m_tClipboardRenewalTimer.Stop();
				if (iAdditionalRenewals < 1 || Clipboard.GetText() != sOTP) //No renewal if something else is in the clipboard
				{
					m_tClipboardRenewalTimer.Dispose();
					m_tClipboardRenewalTimer = null;
					return;
				}

				//Set interval to lifetime ot OTP
				m_tClipboardRenewalTimer.Interval = kOTP.TOTPTimestep * 1000;
				m_tClipboardRenewalTimer.Start();
				sOTP = kOTP.GetOTP(false, true);
				iAdditionalRenewals--;

				//Copy new OTP to clipboard
				//Do NOT minimize again
				ClipboardUtil.Copy(sOTP, false, false, null, null, Program.MainForm.Handle);
				if (bRestartClipboardCountdownOnce)
				{
					bRestartClipboardCountdownOnce = false;
					Program.MainForm.StartClipboardCountdown();
				}
			};
			//m_tClipboardRenewalTimer.Tag = lParams;
			m_tClipboardRenewalTimer.Interval = kOTP.RemainingSeconds * 1000;
			m_tClipboardRenewalTimer.Start();
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
			if ((e.Context.Flags & SprCompileFlags.ExtActive) != SprCompileFlags.ExtActive) return;
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
					var newOTP = myOTP.Clone();
					newOTP.HOTPCounter++;
					OTPDAO.SaveOTP(newOTP, e.Context.Entry);
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
	public class QRForm : Form, KeePass.UI.IGwmWindow
	{
		public bool CanCloseWithoutDataLoss { get { return true; } }
	}
}