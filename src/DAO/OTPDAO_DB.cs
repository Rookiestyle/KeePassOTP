using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using KeePass;
using KeePass.App;
using KeePass.App.Configuration;
using KeePass.DataExchange;
using KeePass.Forms;
using KeePass.UI;
using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Keys;
using KeePassLib.Security;
using KeePassLib.Serialization;
using KeePassLib.Utility;
using PluginTools;
using PluginTranslation;

namespace KeePassOTP
{
	public static partial class OTPDAO
	{
		public class OTPHandler_DB : OTPHandler_Base
		{
			public PwDatabase DB { get; private set; }
			public bool Valid { get; private set; }
			public bool OTPDB_Exists { get; private set; }
			public bool OTPDB_Opened { get; private set; }
			public PwDatabase OTPDB { get; private set; }

			public const string DBNAME = "KeePassOTP.DB";
			private const string UUID = "Entry UUID";
			private const string FILEFORMAT = "KeePass KDBX (2.x)";

			private bool m_bOpening = false;
			private PwEntry m_pe = null;
			private PwEntry m_peOTP = null;
			private string m_sInitialOTPDB = string.Empty;

			private static readonly DateTime INITTIME = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			private FileFormatProvider m_FFP = null;
			private Encoding Encode = new UTF8Encoding();
			private static IOConnectionInfo EmptyIOC = new IOConnectionInfo();
			private static KPOTP EmptyKPOTDB = new KPOTP();

			public OTPHandler_DB() : base()
			{
				Program.MainForm.FileSaving += OTPDB_FileSaved;
				DB = OTPDB = null;

				m_FFP = Program.FileFormatPool.Find(FILEFORMAT);
				if (m_FFP == null)
					throw new ArgumentNullException("ffp - Could not find FileFormatProvider");
			}

			public override void Cleanup()
			{
				Program.MainForm.FileSaving -= OTPDB_FileSaved;
			}

			public void FlagChanged(bool IsOTPDB)
			{
				PwDatabase db = IsOTPDB ? OTPDB : DB;
				if (db == null) return;
				db.SettingsChanged = DateTime.UtcNow;
				db.Modified = true;
				if (!IsOTPDB)
					Program.MainForm.UpdateUI(false, null, false, null, false, null, Program.MainForm.ActiveDatabase == db);
			}

			public bool SetDB(PwDatabase db, bool ForceOpen)
			{
				DB = OTPDB = null;
				Valid = OTPDB_Opened = OTPDB_Exists = false;
				if (db == null) return false;
				if (!db.IsOpen) return false;

				Valid = true;
				DB = db;
				OTPDB_Exists = DB.CustomData.Exists(DBNAME);
				if (!OTPDB_Opened && OTPDB_Exists && ForceOpen)
				{
					OTPDB_Open();
					OTPDB_Opened = (OTPDB != null) && OTPDB.IsOpen;
				}
				return true;
			}

			#region Open/Create OTP database
			public void OTPDB_Create()
			{
				if (m_bOpening) return;
				m_bOpening = true;
				OTPDB_Init(true);
				if (!OTPDB_ChangePassword(false))
				{
					PluginDebug.AddInfo("OTP DB - Creation cancelled");
					OTPDB_Exists = OTPDB_Opened = false;
					OTPDB = null;
					m_bOpening = false;
					return;
				}

				DB.CustomData.Set(Config.DBUsage, StrUtil.BoolToString(true));
				DB.CustomData.Set(Config.DBPreload, StrUtil.BoolToString(true));
				FlagChanged(false);

				OTPDB_Save();
				PluginDebug.AddInfo("OTP DB - Creation successful");
				m_bOpening = false;
			}

			public void OTPDB_Open()
			{
				if (m_bOpening) return;
				m_bOpening = true;

				if (OTPDB_Opened)
				{
					PluginDebug.AddInfo("OTP DB already opened");
					UpdateDBHeader();
					m_bOpening = false;
					return;
				}
				if (!OTPDB_Exists)
				{
					PluginDebug.AddError("OTP DB not available");
					m_bOpening = false;
					return;
				}

				OTPDB_Load();
				if (OTPDB_Opened) OTPDB.Modified = false;
				m_bOpening = false;
			}

			public void OTPDB_Close()
			{
				if (!Valid) return;
				if (!OTPDB_Opened) return;

				OTPDB_Save();
				OTPDB.Close();
				OTPDB_Opened = false;
			}

			public void OTPDB_Remove()
			{
				if (!Valid) return;
				OTPDB = null;
				Valid = OTPDB_Opened = OTPDB_Exists = false;
				DB.CustomData.Remove(DBNAME);
			}

			private PwDatabase OTPDB_Load()
			{
				List<string> lMsg = new List<string>();
				try
				{
					OTPDB_Init(true);
					KeySources_Clear();
					byte[] bOTPDB = GetOTPDBData(); // ConvertFromCustomData(DB.CustomData.Get(DBNAME));
					if (bOTPDB == null) return null;
					AceKeyAssoc aka_old = null;
					for (int i = 0; i < Program.Config.Security.MasterKeyTries; i++)
					{
						bool bCancel = false;
						if (Program.Config.Defaults.RememberKeySources && (i == 0)) aka_old = KeySources_Load();
						CompositeKey ck = OTPDB_RequestPassword(false, out bCancel);
						if (bCancel)
						{
							lMsg.Add("Masterkey input cancelled by user");
							break;
						}
						OTPDB.MasterKey = ck;
						try
						{
							OTPDB_Synchronize(OTPDB, m_FFP, bOTPDB, PluginTranslate.OTPDB_Opening);
							lMsg.Add("Masterkey valid, attempt: " + (i + 1).ToString());
							Program.Config.Defaults.SetKeySources(EmptyIOC, OTPDB.MasterKey);
							AceKeyAssoc aka_new = Program.Config.Defaults.GetKeySources(EmptyIOC);
							KeySources_Clear();
							if (!KeySources_Equal(aka_old, aka_new)) KeySources_Save(aka_new);
							KeySources_Clear();
							OTPDAO.InitEntries(DB);
							UpdateDBHeader();
							if (OTPDB_Opened && CheckAndMigrate(DB))
								FlagChanged(false);
							return OTPDB;
						}
						catch { lMsg.Add("Masterkey invalid, attempt: " + (i + 1).ToString()); }
					}
					KeySources_Clear();
					OTPDB_Init(false);
					return OTPDB;
				}
				finally { PluginDebug.AddInfo("OTP DB - Load", 0, lMsg.ToArray()); }
			}

			private void OTPDB_Init(bool bCreateOpened)
			{
				OTPDB = new PwDatabase();
				OTPDB.RootGroup = new PwGroup();
				FlagChanged(true);
				if (bCreateOpened) OTPDB.New(new IOConnectionInfo(), new CompositeKey());
				OTPDB.RootGroup.LastModificationTime = INITTIME;
				OTPDB.MasterKeyChangeRec = -1;
				OTPDB.MasterKeyChangeForce = -1;
				OTPDB.MasterKeyChangeForceOnce = false;
				OTPDB.RootGroup.Name = DBNAME;
				OTPDB.RecycleBinEnabled = false;
				OTPDB.SettingsChanged = INITTIME;
				OTPDB_Exists = true;
				OTPDB_Opened = OTPDB.IsOpen;
			}
			#endregion

			#region Synchronization of OTP database
			private bool OTPDB_Reload()
			{
				try
				{
					byte[] bOTPDB = GetOTPDBData();
					if (bOTPDB == null) return false;

					//Create DB if required
					if (!OTPDB_Exists) SetDB(DB, true);
					else if (!OTPDB_Opened)
					{
						if (Config.ShowHintSyncRequiresUnlock) Tools.ShowInfo(PluginTranslate.HintSyncRequiresUnlock);
						while (!EnsureOTPSetupPossible(null))
						{
							if (Tools.AskYesNo(PluginTranslate.HintSyncRequiresUnlock) == DialogResult.No)
							{
								PwEntry peOTPDBBackup = new PwEntry(true, true);
								string sTitle = DBNAME + " Backup " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
								peOTPDBBackup.Strings.Set(PwDefs.TitleField, new ProtectedString(false, sTitle));
								if (!string.IsNullOrEmpty(m_sInitialOTPDB))
									peOTPDBBackup.Binaries.Set(DBNAME + ".kdbx", new ProtectedBinary(true, ConvertFromCustomData(m_sInitialOTPDB)));
								DB.RootGroup.AddEntry(peOTPDBBackup, true);
								peOTPDBBackup.Expires = false;
								FlagChanged(false);
								Tools.ShowInfo(string.Format(PluginTranslate.OTPBackupDone, sTitle));
								break;
							}
						}
						//DB was not opened yet
						//Try loading initially remembered database
						//This is required to not lose local changes during a sync
						//Current content of CustomData might already be replaced
						if (!string.IsNullOrEmpty(m_sInitialOTPDB))
						{
							byte[] bInitial = ConvertFromCustomData(m_sInitialOTPDB);
							m_sInitialOTPDB = string.Empty;
							OTPDB_Synchronize(OTPDB, m_FFP, bInitial, PluginTranslate.OTPDB_Opening);
						}
					}
					OTPDB_Synchronize(OTPDB, m_FFP, bOTPDB, PluginTranslate.OTPDB_Sync);
					InitEntries(DB);
					UpdateDBHeader();
					PluginDebug.AddInfo("OTP db reloaded");
					OTPDB_Opened = (OTPDB != null) && OTPDB.IsOpen;
					if (OTPDB_Opened && CheckAndMigrate(DB))
						FlagChanged(false);
					return true;
				}
				catch (Exception ex)
				{
					PluginDebug.AddError("Reloading OTP db failed", 0, ex.Message);
					return false;
				}
			}

			private void OTPDB_Synchronize(PwDatabase targetdb, FileFormatProvider fmtImp, byte[] bDecrypted, string text)
			{
				IStatusLogger dlgStatus = new OnDemandStatusDialog(true, Program.MainForm);

				dlgStatus.StartLogging(text, false);
				try
				{
					dlgStatus.SetText(text, LogStatusType.Info);

					PwDatabase pwImp;
					pwImp = new PwDatabase();
					pwImp.New(new IOConnectionInfo(), targetdb.MasterKey);
					pwImp.MemoryProtection = targetdb.MemoryProtection.CloneDeep();
					pwImp.MasterKey = targetdb.MasterKey;

					dlgStatus.SetText(text, LogStatusType.Info);

					using (var s = new MemoryStream(bDecrypted))
					{
						fmtImp.Import(pwImp, s, null);
					}
					targetdb.KdfParameters = pwImp.KdfParameters;
					targetdb.DataCipherUuid = pwImp.DataCipherUuid;
					targetdb.HistoryMaxItems = pwImp.HistoryMaxItems;
					targetdb.HistoryMaxSize = pwImp.HistoryMaxSize;

					PwMergeMethod mm = PwMergeMethod.Synchronize;

					targetdb.RootGroup.Uuid = pwImp.RootGroup.Uuid;
					targetdb.MergeIn(pwImp, mm, dlgStatus);
					CleanupSyncedDB(targetdb);
				}
				catch (Exception ex)
				{
					PluginDebug.AddError("Error loading OTP db", 0, ex.Message);
					throw ex;
				}
				finally { dlgStatus.EndLogging(); }
			}

			private void CleanupSyncedDB(PwDatabase targetdb)
			{
				//Previous versions of KeePassOTP added the synchronized rootgroup
				//as subgroup into the rootgroup of the new OTP database
				if (RemoveEmptyGroups(targetdb.RootGroup) > 0)
				{
					targetdb.SettingsChanged = DateTime.UtcNow;
					targetdb.Modified = true;
				}
			}

			private int RemoveEmptyGroups(PwGroup pg)
			{
				int removed = 0;
				if (pg == null) return removed;
				for (int i = pg.Groups.Count() - 1; i >= 0; i--)
				{
					PwGroup g = pg.Groups.GetAt((uint)i);
					removed += RemoveEmptyGroups(g);
					if ((g.Groups.UCount > 0) || (g.Entries.UCount > 0))
					{
						PluginDebug.AddError("Invalid group in OTP database", 0,
							"Group uuid: " + g.Uuid.ToHexString(),
							"Group name: " + g.Name,
							"Subgroups: " + g.Groups.UCount.ToString(),
							"Entries" + g.Entries.UCount.ToString());
						continue;
					}
					pg.Groups.Remove(g);
					removed++;
				}
				return removed;
			}
			#endregion

			#region Save OTP database
			public void RememberInitialOTPDB(PwDatabase db)
			{
				m_sInitialOTPDB = db.CustomData.Get(DBNAME);
			}

			private void OTPDB_FileSaved(object sender, FileSavingEventArgs e)
			{
				if (e.Cancel) return;
				if (e.Database.IOConnectionInfo.Path != DB.IOConnectionInfo.Path) return;
				if (!Valid) return;
				List<System.Diagnostics.StackFrame> lSF = new System.Diagnostics.StackTrace().GetFrames().ToList();
				bool bSync = lSF.Find(f => (f.GetMethod().Name == "Synchronize") && (f.GetMethod().ReflectedType == typeof(ImportUtil))) != null;

				//Save if OTP database exists and is unlocked and ImportUtil.Synchronize is not called
				if (!bSync && OTPDB_Exists && OTPDB_Opened)
					OTPDB_Save();
				else if (bSync) //Sync if ImportUtil.Synchronize is called, don't care about OTP database existance / lock status
				{					
					if (OTPDB_Reload()) OTPDB_Save();
				}
			}

			public void OTPDB_SaveAfterMigration()
			{
				OTPDB_Save();
			}

			private void OTPDB_Save()
			{
				if (!Valid || !OTPDB_Opened || !OTPDB.Modified) return;
				OTPDB.Modified = false;
				UpdateDBHeader();
				PwExportInfo pei = new PwExportInfo(OTPDB.RootGroup, OTPDB);
				using (var s = new MemoryStream())
				{
					IStatusLogger sl = Program.MainForm.CreateShowWarningsLogger();
					sl.StartLogging(PluginTranslate.OTPDB_Save, true);
					m_FFP.Export(pei, s, sl);
					sl.EndLogging();
					s.Flush();
					SetOTPDBData(s.GetBuffer());
				}
				FlagChanged(false);
			}

			private void UpdateDBHeader()
			{
				OTPDB.CustomData.Set("MAIN_DB_Path", DB.IOConnectionInfo.Path);
				OTPDB.CustomData.Set("MAIN_DB_Name", DB.Name);
				OTPDB.RootGroup.Name = DB.RootGroup.Name;
			}
			#endregion

			#region Handle PwEntry
			public bool HasEntries()
			{
				if (!Valid) return false;
				if (!OTPDB_Exists) return false;
				if (!OTPDB_Opened) return false;
				return (OTPDB.RootGroup != null) && (OTPDB.RootGroup.GetEntriesCount(true) > 0);
			}

			public override bool EnsureOTPSetupPossible(PwEntry pe)
			{
				if (!Valid) return false;

				if (!OTPDB_Exists) OTPDB_Create();
				if (OTPDB_Exists) OTPDB_Open();
				return OTPDB_Exists && OTPDB_Opened;
			}

			public override bool EnsureOTPUsagePossible(PwEntry pe)
			{
				if (!Valid) return false;

				if (!OTPDB_Exists) return false;
				OTPDB_Open();
				return OTPDB_Opened;
			}

			public override OTPDefinition OTPDefined(PwEntry pe)
			{
				if (!SetEntry(pe, false)) return OTPDefinition.None;
				bool bDB = StrUtil.StringToBool(m_pe.Strings.ReadSafe(DBNAME));
				if (!bDB || !OTPDB_Exists) return OTPDefinition.None;
				bool bCreated = false;
				GetOTPEntry(false, out bCreated);
				if (!OTPDB_Opened || (m_peOTP == null)) return OTPDefinition.Partial;
				return m_peOTP.Strings.Exists(Config.OTPFIELD) ? OTPDefinition.Complete : OTPDefinition.None;
			}

			public override string GetReadableOTP(PwEntry pe)
			{
				if (!SetEntry(pe, false)) return string.Empty;
				
				EntryOTP otp = EnsureEntry();
				if (otp.ValidTo < DateTime.UtcNow)
				{
					if (otp.kpotp.Valid)
					{
						otp.ReadableOTP = otp.kpotp.ReadableOTP(otp.kpotp.GetOTP());
						otp.ValidTo = DateTime.UtcNow.AddSeconds(otp.kpotp.RemainingSeconds);
					}
					else if (otp.Loaded)
					{
						otp.ReadableOTP = PluginTranslate.Error;
						otp.ValidTo = DateTime.MaxValue;
					}
					if (otp.Loaded)
						UpdateOTPBuffer(pe, otp);
				}
				if ((!otp.Loaded && Config.UseDBForOTPSeeds(otp.db)) || (otp.ValidTo == DateTime.MaxValue))
					return otp.ReadableOTP;

				if (otp.kpotp.Type == KPOTPType.HOTP) return otp.ReadableOTP;
				int r = (otp.ValidTo - DateTime.UtcNow).Seconds + 1;
				return otp.ReadableOTP + (r < 6 ? " (" + r.ToString() + ")" : string.Empty);
			}

			public override KPOTP GetOTP(PwEntry pe)
			{
				if (!SetEntry(pe, false)) return EmptyKPOTDB;
				return EnsureEntry().kpotp;
			}

			public override void SaveOTP(KPOTP myOTP, PwEntry pe)
			{
				if (!SetEntry(pe, true)) return;

				//string otpSettings = myOTP.Settings;
				KPOTP prev = GetOTP(pe);
				bool OnlyCounterChanged = false;
				if (!SettingsChanged(pe, prev, myOTP, out OnlyCounterChanged)) return;

				PluginDebug.AddInfo("Update OTP data",
					"Entry uuid: " + pe.Uuid.ToHexString(),
					"Only change of HOTP counter: " + OnlyCounterChanged.ToString());

				bool bCreated = false;
				GetOTPEntry(true, out bCreated);
				if (!OnlyCounterChanged)
				{
					//Create backup if something else than only the HOTP counter was changed
					if (!bCreated) m_peOTP.CreateBackup(OTPDB);
				}
				m_peOTP.Strings.Set(Config.OTPFIELD, myOTP.OTPAuthString);
				if (myOTP.TimeCorrectionUrlOwn)
					m_peOTP.Strings.Set(Config.TIMECORRECTION, new ProtectedString(false, "OWNURL"));
				else if (string.IsNullOrEmpty(myOTP.TimeCorrectionUrl) || (myOTP.TimeCorrectionUrl == "OFF"))
					m_peOTP.Strings.Remove(Config.TIMECORRECTION);
				else
					m_peOTP.Strings.Set(Config.TIMECORRECTION, new ProtectedString(false, myOTP.TimeCorrectionUrl));
				
				Touch(m_peOTP);
				if (myOTP.OTPSeed.IsEmpty)
					m_pe.Strings.Remove(DBNAME);
				else
					m_pe.Strings.Set(DBNAME, new ProtectedString(false, StrUtil.BoolToString(true)));
				FlagChanged(false);
				FlagChanged(true);

				m_pe.Touch(true);
			}

			public void UpdateOTPData(PwEntry pe, bool bBackup)
			{
				if (!SetEntry(pe, false)) return;
				if (!OTPDB_Opened) return;

				if (m_peOTP == null) return;
				bool bChanged = UpdateStringRequired(PwDefs.TitleField);
				bChanged |= UpdateStringRequired(PwDefs.UserNameField);
				bChanged |= UpdateStringRequired(PwDefs.UrlField);

				PluginDebug.AddInfo("Update OTP entry - non OTP fields",
					"Entry uuid: " + m_pe.Uuid.ToHexString(),
					"Update of non OTP fields required: " + bChanged.ToString(),
					"Backup old OTP entry: " + (bChanged ? bBackup.ToString() : "N/A"));

				if (!bChanged) return;

				if (bBackup) m_peOTP.CreateBackup(OTPDB);
				UpdateString(PwDefs.TitleField);
				UpdateString(PwDefs.UserNameField);
				UpdateString(PwDefs.UrlField);
				FlagChanged(true);
			}

			private bool SetEntry(PwEntry pe, bool ForceOpen)
			{
				if (!Valid) return false;
				if (pe == null) return false;

				m_pe = pe;
				m_peOTP = null;
				if (ForceOpen && !OTPDB_Opened) OTPDB_Open();
				GetOTPEntry();
				return true;
			}

			private void GetOTPEntry()
			{
				bool bDummy;
				GetOTPEntry(false, out bDummy);
			}

			private void GetOTPEntry(bool bCreate, out bool Created)
			{
				Created = false;
				m_peOTP = null;
				if (!OTPDB_Opened) return;
				string uuid = m_pe.Uuid.ToHexString();
				m_peOTP = OTPDB.RootGroup.GetEntries(true).Where(x => x.Strings.ReadSafe(UUID) == uuid).FirstOrDefault();
				if (m_peOTP != null)
				{
					string s = "Found OTP entry for main entry " + m_pe.Uuid.ToHexString();
					if (!PluginDebug.HasMessage(PluginDebug.LogLevelFlags.Info, s)) PluginDebug.AddInfo(s);
				}
				if ((m_peOTP != null) || !bCreate) return;
				m_peOTP = new PwEntry(true, true);
				OTPDB.RootGroup.AddEntry(m_peOTP, true);
				m_peOTP.Strings.Set(UUID, new ProtectedString(false, uuid));
				UpdateString(PwDefs.TitleField);
				UpdateString(PwDefs.UserNameField);
				UpdateString(PwDefs.UrlField);
				Created = true;
				if (m_peOTP != null)
				{
					PluginDebug.AddInfo("Created OTP entry for main entry " + m_pe.Uuid.ToHexString());
				}
			}

			private EntryOTP EnsureEntry()
			{
				EntryOTP otp;
				if (m_dEntryOTPData.TryGetValue(m_pe, out otp) && !IgnoreBuffer) return otp;
				otp.db = DB;
				otp.OTPDefined = OTPDefined(m_pe);
				otp.Loaded = OTPDB_Opened;
				otp.ReadableOTP = otp.OTPDefined == OTPDefinition.Partial ? "???" : string.Empty;
				otp.ValidTo = otp.OTPDefined != OTPDefinition.None ? KPOTP.UnixStartUTC : DateTime.MaxValue;
				if (otp.OTPDefined != OTPDefinition.Complete)
				{
					otp.ReadableOTP = otp.OTPDefined == OTPDefinition.Partial ? "???" : string.Empty;
					otp.ValidTo = DateTime.MaxValue;
					otp.kpotp = new KPOTP();
					InitIssuerLabel(otp.kpotp, m_pe);
				}
				else
				{
					otp.kpotp = GetSettings();
					otp.ValidTo = KPOTP.UnixStartUTC;
				}
				UpdateOTPBuffer(m_pe, otp);
				PluginDebug.AddInfo("Fill OTP buffer", 0,
					"Entry uuid: " + m_pe.Uuid.ToHexString(),
					"OTP db exists: " + OTPDB_Exists.ToString(),
					"OTP db loaded: " + otp.Loaded.ToString(),
					"OTP defined: " + otp.OTPDefined.ToString(),
					"OTP setup valid: " + otp.kpotp.Valid.ToString());
				return otp;
			}

			private void Touch(PwEntry pe)
			{
				//Do NOT use pe.Touch
				//This will process all eventhandlers and might result
				//in issues as most likely plugins are not aware of entries in 
				//the OTP database
				if (pe == null) return;
				pe.LastAccessTime = pe.LastModificationTime = DateTime.UtcNow;

				PwGroup pg = pe.ParentGroup;
				while (pg != null)
				{
					pg.LastAccessTime = pg.LastModificationTime = pe.LastModificationTime;
					pg = pg.ParentGroup;
				}
			}

			private bool UpdateStringRequired(string field)
			{
				if (!m_pe.Strings.Exists(field)) return m_peOTP.Strings.Exists(field);
				ProtectedString psNew = m_pe.Strings.Get(field);
				if (m_peOTP.Strings.Exists(field) && psNew.Equals(m_peOTP.Strings.Get(field), false)) return false;
				return true;
			}

			private void UpdateString(string field)
			{
				ProtectedString ps = m_pe.Strings.Get(field);
				if (ps == null)
					m_peOTP.Strings.Remove(field);
				else
					m_peOTP.Strings.Set(field, ps);
			}

			private KPOTP GetSettings()
			{
				GetOTPEntry();
				KPOTP myOTP = new KPOTP();
				if (m_peOTP == null)
					return EmptyKPOTDB;// myOTP;
				/*
				myOTP.OTPSeed = ProtectedString.EmptyEx;
				string settings = m_peOTP.Strings.ReadSafe(Config.SETTINGS);
				PluginDebug.AddInfo("Get OTP settings", 0, "Uuid: " + m_pe.Uuid.ToHexString(), "OTP Uuid: " + m_peOTP.Uuid.ToHexString(), "Settings: " + settings);
				*/
				myOTP.OTPAuthString = m_peOTP.Strings.GetSafe(Config.OTPFIELD);
				string timeCorrection = m_peOTP.Strings.ReadSafe(Config.TIMECORRECTION);
				if (timeCorrection == "OWNURL")
				{
					myOTP.TimeCorrectionUrlOwn = true;
					string url = m_pe.Strings.GetSafe(PwDefs.UrlField).ReadString();
					if (!string.IsNullOrEmpty(url))
						myOTP.TimeCorrectionUrl = url;
				}
				else
				{
					myOTP.TimeCorrectionUrlOwn = false;
					myOTP.TimeCorrectionUrl = timeCorrection;
				}
				return myOTP;
			}
			#endregion

			#region Convert encrypted PwDatabase from/to string
			private byte[] GetOTPDBData()
			{
				if (!DB.CustomData.Exists(DBNAME)) return null;
				return ConvertFromCustomData(DB.CustomData.Get(DBNAME));
			}

			private void SetOTPDBData(byte[] otpdata)
			{
				m_sInitialOTPDB = ConvertToCustomData(otpdata);
				DB.CustomData.Set(DBNAME, m_sInitialOTPDB);
			}

			public static byte[] ConvertFromCustomData(string customdata)
			{
				if (string.IsNullOrEmpty(customdata)) return null;
				return Convert.FromBase64String(customdata);
			}

			public static string ConvertToCustomData(byte[] otpdata)
			{
				if (otpdata == null) return string.Empty;
				return Convert.ToBase64String(otpdata);
			}
			#endregion

			#region Password handling of OTP database
			public bool OTPDB_ChangePassword(bool change)
			{
				List<string> lMsg = new List<string>();
				try
				{
					if (!Valid || !OTPDB_Exists || !OTPDB_Opened) return false;
					if (change && !AppPolicy.Try(AppPolicyId.ChangeMasterKey)) return false;

					if (change && !AppPolicy.Current.ChangeMasterKeyNoKey && !ReAskKey())
					{
						lMsg.Add("Invalid masterkey entered");
						return false;
					}
					if (!change) Tools.ShowInfo(PluginTranslate.OTP_CreateDB_PWHint);

					bool bCancel;
					KeySources_Clear();
					AceKeyAssoc aka_old = KeySources_Load();
					OTPDB.MasterKey = OTPDB_RequestPassword(true, out bCancel);
					KeySources_Clear();
					if (bCancel)
					{
						lMsg.Add("Password change cancelled by user");
						return false;
					}
					else
						lMsg.Add("Password change done");

					//Store key sources to reread them in the proper format
					//SetKeySources won't do anything if KeePass config does not allow key sources to be remembered
					Program.Config.Defaults.SetKeySources(EmptyIOC, OTPDB.MasterKey);
					AceKeyAssoc aka_new = Program.Config.Defaults.GetKeySources(EmptyIOC);
					KeySources_Clear();

					FlagChanged(false);
					FlagChanged(true);
					if (KeySources_Equal(aka_old, aka_new)) return true;
					KeySources_Save(aka_new);
					return true;
				}
				finally { PluginDebug.AddInfo("OTP DB - change password", 0, lMsg.ToArray()); }
			}

			public bool ReAskKey()
			{
				if (!Valid || !OTPDB_Exists || !OTPDB_Opened) return false;
				KeySources_Clear();
				Program.Config.Defaults.SetKeySources(EmptyIOC, OTPDB.MasterKey);
				bool success = KeePass.Util.KeyUtil.ReAskKey(OTPDB, true);
				KeySources_Clear();
				return success;
			}

			private CompositeKey OTPDB_RequestPassword(bool bSetNewPassword, out bool bCancel)
			{
				if (!bSetNewPassword)
				{
					KeyPromptForm kpf = new KeyPromptForm();
					string title = string.Format(PluginTranslate.OTP_OpenDB, string.IsNullOrEmpty(DB.Name) ? UrlUtil.GetFileName(DB.IOConnectionInfo.Path) : DB.Name);
					kpf.InitEx(OTPDB.IOConnectionInfo, false, false, title);
					bCancel = kpf.ShowDialog() != DialogResult.OK;
					if (bCancel) return new CompositeKey();
					return kpf.CompositeKey;
				}
				KeyCreationForm kcf = new KeyCreationForm();
				kcf.InitEx(null, true);
				bCancel = kcf.ShowDialog() != DialogResult.OK;
				if (bCancel) return OTPDB.MasterKey;
				return kcf.CompositeKey;
			}

			private void KeySources_Clear()
			{
				Program.Config.Defaults.SetKeySources(EmptyIOC, null);
			}

			private void KeySources_Save(AceKeyAssoc aka)
			{
				try
				{
					if (!Program.Config.Defaults.RememberKeySources)
					{
						DB.CustomData.Remove(Config.DBKeySources);
						return;
					}
					using (StringWriter writer = new StringWriter())
					{
						XmlSerializer xsKeySources = new XmlSerializer(typeof(AceKeyAssoc));
						xsKeySources.Serialize(writer, aka);
						writer.Flush();
						DB.CustomData.Set(Config.DBKeySources, writer.ToString());
						FlagChanged(false);
					}
				}
				catch { }
			}

			private bool KeySources_Equal(AceKeyAssoc aka_old, AceKeyAssoc aka_new)
			{
				if ((aka_old == null) || (aka_new == null)) return false;
				if (aka_old.Password != aka_new.Password) return false;
				if (aka_old.KeyFilePath != aka_new.KeyFilePath) return false;
				if (aka_old.KeyProvider != aka_new.KeyProvider) return false;
				if (aka_old.UserAccount != aka_new.UserAccount) return false;
				return true;
			}

			private AceKeyAssoc KeySources_Load()
			{
				if (!Program.Config.Defaults.RememberKeySources) return null;
				AceKeyAssoc aka = null;

				if (!DB.CustomData.Exists(Config.DBKeySources)) return aka;
				string k = DB.CustomData.Get(Config.DBKeySources);
				try
				{
					XmlSerializer xsKeySources = new XmlSerializer(typeof(AceKeyAssoc));
					aka = (AceKeyAssoc)xsKeySources.Deserialize(new StringReader(k));
					Program.Config.Defaults.KeySources.RemoveAll(x => x.DatabasePath == aka.DatabasePath);
					Program.Config.Defaults.KeySources.Add(aka);
				}
				catch (Exception) { }
				return aka;
			}
			#endregion

			#region Migrate OTP data between main database and OTP dabase
			public int MigrateOTP2DB()
			{
				if (!Valid || !OTPDB_Exists || !OTPDB_Opened) return -1;
				int moved = 0;
				//process all entries having OTP settings
				PwEntry peBackup = m_pe;
				PwEntry pe_OTPBackup = m_peOTP;
				foreach (PwEntry pe in DB.RootGroup.GetEntries(true).Where(x => x.Strings.Exists(Config.OTPFIELD)))
				{
					m_pe = pe;
					ProtectedString otpfield = m_pe.Strings.GetSafe(Config.OTPFIELD);
					ProtectedString timecorrection = m_pe.Strings.GetSafe(Config.TIMECORRECTION);
					if (otpfield.IsEmpty) continue;
					bool bCreated = false;
					GetOTPEntry(true, out bCreated);
					m_peOTP.Strings.Set(Config.OTPFIELD, otpfield);
					if (!timecorrection.IsEmpty) m_peOTP.Strings.Set(Config.TIMECORRECTION, timecorrection);
					else m_peOTP.Strings.Remove(Config.TIMECORRECTION);
					//Seed has been added to OTP db, increase moved-counter
					moved++;
					m_pe.Strings.Remove(Config.OTPFIELD);
					m_pe.Strings.Remove(Config.TIMECORRECTION);
					m_pe.Strings.Set(DBNAME, new ProtectedString(false, StrUtil.BoolToString(true)));
					m_pe.Touch(true);
					FlagChanged(false);
				}
				m_pe = peBackup;
				m_peOTP = pe_OTPBackup;
				if (moved > 0) OTPDB_Save();
				return moved;
			}

			public int MigrateOTP2Entry()
			{
				if (!Valid || !OTPDB_Exists || !OTPDB_Opened) return -1;

				int moved = 0;
				foreach (PwEntry peOTP in OTPDB.RootGroup.GetEntries(true))
				{
					PwUuid uuid = new PwUuid(MemUtil.HexStringToByteArray(peOTP.Strings.ReadSafe(UUID)));
					foreach (PwEntry pe in DB.RootGroup.GetEntries(true).Where(x => uuid.Equals(x.Uuid)))
					{
						pe.Strings.Set(Config.OTPFIELD, peOTP.Strings.GetSafe(Config.OTPFIELD));
						if (peOTP.Strings.Exists(Config.TIMECORRECTION))
							pe.Strings.Set(Config.TIMECORRECTION, peOTP.Strings.GetSafe(Config.TIMECORRECTION));
						else
							pe.Strings.Remove(Config.TIMECORRECTION);
						pe.Strings.Remove(DBNAME);
						pe.Touch(true);
						moved++;
					}
				}

				if (moved > 0) FlagChanged(false);
				OTPDB_Save();
				return moved;
			}
			#endregion

			#region Only for migration purpose, to be deleted in future releases
			[Obsolete]
			public static PwEntry GetOTPDBEntry(PwDatabase db)
			{
				if ((db == null) || !db.IsOpen) return null;

				List<PwEntry> lEntries = db.RootGroup.GetEntries(true).Where(x => x.Strings.ReadSafe(PwDefs.TitleField) == DBNAME).ToList();
				if (lEntries.Count == 0) return null;
				PwGroup gRecycle = db.RecycleBinEnabled ? db.RootGroup.FindGroup(db.RecycleBinUuid, true) : null;
				foreach (PwEntry pe in lEntries)
				{
					if ((gRecycle != null) && pe.IsContainedIn(gRecycle)) continue;
					return pe;
				}
				return null;
			}
			#endregion
		}
	}
}