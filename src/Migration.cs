using KeePass.UI;
using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Delegates;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using PluginTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace KeePassOTP
{
	public class MigrationBase
	{
		internal PwDatabase m_db = null;
		internal bool m_bInitialized = false;
		public void SetDB(PwDatabase db)
		{
			m_db = db;
			m_bInitialized = (db != null) && db.IsOpen;
			if (!m_bInitialized) PluginDebug.AddError("Invalid DB provided");
		}

		internal int MigrateInt(string v, int def)
		{
			int r;
			if (!int.TryParse(v, out r)) r = def;
			return r;
		}
		internal string MigrateString(string v)
		{
			v = v.Replace("%3d", "=");
			v = v.Replace("%3D", "=");
			return v;
		}

		public virtual void MigrateToKeePassOTP(bool bRemove, out int EntriesOverall, out int EntriesMigrated)
		{
			EntriesOverall = EntriesMigrated = -1;
		}
		public virtual void MigrateFromKeePassOTP(bool bRemove, out int EntriesOverall, out int EntriesMigrated)
		{
			EntriesOverall = EntriesMigrated = -1;
		}

		public bool MigratePlaceholder(string from, string to, PwEntry pe, out bool bChanged)
		{
			bChanged = false;
			if (pe == null) { return true; }
			bool bBackupRequired = true;
			foreach (var a in pe.AutoType.Associations)
			{
				if (a.Sequence.Contains(from))
				{
					if (bBackupRequired)
					{
						bBackupRequired = false;
						pe.CreateBackup(m_db);
					}
					a.Sequence = a.Sequence.Replace(from, to);
					bChanged = true;
				}
			}
			if (pe.AutoType.DefaultSequence.Contains(from))
			{
				if (bBackupRequired)
				{
					bBackupRequired = false;
					pe.CreateBackup(m_db);
				}
				pe.AutoType.DefaultSequence = pe.AutoType.DefaultSequence.Replace(from, to);
				bChanged = true;
			}
			foreach (var s in pe.Strings.GetKeys())
			{
				ProtectedString ps = pe.Strings.Get(s);
				if (!ps.Contains(from)) continue;
				if (bBackupRequired)
				{
					bBackupRequired = false;
					pe.CreateBackup(m_db);
				}
				ps = ps.Replace(from, to);
				pe.Strings.Set(s, ps);
				bChanged = true;
			}
			if (bChanged) pe.Touch(true, false);
			return bChanged;
		}
		public bool MigratePlaceholder(string from, string to)

		{
			bool bChanged = false;
			if (!m_bInitialized) return bChanged;
			GroupHandler gh = delegate (PwGroup pg)
			{
				if (pg == null) { return true; }
				if (pg.DefaultAutoTypeSequence.Contains(from))
				{
					pg.DefaultAutoTypeSequence = pg.DefaultAutoTypeSequence.Replace(from, to);
					pg.Touch(true, false);
					bChanged = true;
				}
				return true;
			};

			EntryHandler eh = delegate (PwEntry pe)
			{
				bool bChangedEntry;
				MigratePlaceholder(from, to, pe, out bChangedEntry);
				bChanged |= bChangedEntry;
				return true;
			};

			m_db.RootGroup.TraverseTree(TraversalMethod.PreOrder, gh, eh);
			if (m_db.RootGroup.DefaultAutoTypeSequence.Contains(from))
			{
				m_db.RootGroup.DefaultAutoTypeSequence = m_db.RootGroup.DefaultAutoTypeSequence.Replace(from, to);
				m_db.RootGroup.Touch(true, false);
				bChanged = true;
			}

			return bChanged;
		}

		private IStatusLogger m_sl = null;
		private int m_EntryCount = 0;
		private int m_EntryIndex = 0;
		private Form m_Form = null;
		protected void InitLogger(string sCaption, int max)
		{
			m_sl = StatusUtil.CreateStatusDialog(GlobalWindowManager.TopWindow, out m_Form, sCaption, null, false, false);
			m_EntryCount = max;
			m_EntryIndex = 0;
			m_sl.SetProgress(0);
			KeePass.Program.MainForm.UIBlockInteraction(true);
		}

		protected void IncreaseLogger()
		{
			if (m_EntryCount == 0) return;
			if (m_sl == null) return;
			m_EntryIndex++;
			uint percentage = (uint)(100 * m_EntryIndex / m_EntryCount);
			m_sl.SetProgress(percentage);
			m_sl.SetText(percentage.ToString() + "% - " + m_EntryIndex.ToString() + " / " + m_EntryCount.ToString(), LogStatusType.Info);
		}

		protected void EndLogger()
		{
			if (m_sl != null)
			{
				m_sl.EndLogging();
				m_sl = null;
				if (m_Form != null)
				{
					m_Form.Dispose();
					m_Form = null;
				}
				KeePass.Program.MainForm.UIBlockInteraction(false);
			}
		}
	}

	public class MigrationKeeOTP : MigrationBase
	{
		private const string OtherPluginPlaceholder = "{TOTP}";
		public override void MigrateToKeePassOTP(bool bRemove, out int EntriesOverall, out int EntriesMigrated)
		{
			EntriesOverall = EntriesMigrated = -1;
			if (!m_bInitialized) return;
			EntriesOverall = EntriesMigrated = 0;

			List<PwEntry> lEntries = m_db.RootGroup.GetEntries(true).Where(x => x.Strings.Exists("otp")).ToList();
			EntriesOverall = lEntries.Count;
			if (lEntries.Count == 0) return;

			if (!OTPDAO.EnsureOTPSetupPossible(lEntries[0])) return;
			OTPDAO.OTPHandler_Base handler = OTPDAO.GetOTPHandler(lEntries[0]);
			InitLogger("KeeOTP -> KeePassOTP", lEntries.Count);
			try
			{
				foreach (PwEntry pe in lEntries)
				{
					IncreaseLogger();
					string old = pe.Strings.ReadSafe("otp");
					if (string.IsNullOrEmpty(old)) continue;
					var parameters = old.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
					var otp = OTPDAO.GetOTP(pe);
					foreach (var parameter in parameters)
					{
						var kvp = parameter.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
						if (kvp.Length != 2) continue;
						switch (kvp[0].ToLowerInvariant())
						{
							case "key":
								otp.OTPSeed = new ProtectedString(true, MigrateString(kvp[1]));
								break;
							case "type":
								otp.Type = kvp[1].ToLower() != "hotp" ? KPOTPType.TOTP : KPOTPType.HOTP;
								break;
							case "step":
								otp.TOTPTimestep = MigrateInt(kvp[1], 30);
								break;
							case "counter":
								otp.HOTPCounter = MigrateInt(kvp[1], 0);
								break;
							case "size":
								otp.Length = MigrateInt(kvp[1], 6);
								break;
							default: break;
						}
					}
					if (otp.Valid)
					{
						EntriesMigrated++;
						try
						{
							handler.IgnoreBuffer = true;
							OTPDAO.SaveOTP(otp, pe);
						}
						finally { handler.IgnoreBuffer = false; }
						pe.Touch(true);
						//Only remove setting if we migrated to OTP-DB
						//Do not remove if KeePassOTP stores OTP data within the entry
						if (bRemove && Config.UseDBForOTPSeeds(pe.GetDB())) pe.Strings.Remove("otp");
					}
					else
					{
						string s = string.Empty;
						for (int i = 0; i < parameters.Count(); i++)
						{
							if (parameters[i].ToLowerInvariant().StartsWith("key="))
								s += "key=<secret>";
							else
								s += parameters[i];
							if (i < parameters.Count() - 1) s += "&";
						}
						PluginDebug.AddError("Migration of entry failed",
							"Uuid: " + pe.Uuid.ToHexString(),
							"OTP data: " + s);
					}
				}
			}
			finally { EndLogger(); }
			MigratePlaceholder(OtherPluginPlaceholder, Config.Placeholder);
		}

		public override void MigrateFromKeePassOTP(bool bRemove, out int EntriesOverall, out int EntriesMigrated)
		{
			EntriesOverall = EntriesMigrated = -1;
			if (!m_bInitialized) return;
			EntriesOverall = EntriesMigrated = 0;

			OTPDAO.OTPHandler_DB h = OTPDAO.GetOTPHandler(m_db);
			if ((h != null) && !h.EnsureOTPUsagePossible(null)) return;

			PwObjectList<PwEntry> lEntries = m_db.RootGroup.GetEntries(true);
			if (lEntries.Count() == 0) return;

			OTPDAO.OTPHandler_Base handler = OTPDAO.GetOTPHandler(lEntries.GetAt(0));
			InitLogger("KeePassOTP -> KeeOTP", lEntries.Count());
			try
			{
				foreach (PwEntry pe in lEntries)
				{
					IncreaseLogger();
					KPOTP otp = OTPDAO.GetOTP(pe);
					if (!otp.Valid) continue;
					EntriesOverall++;
					if (otp.Encoding != KPOTPEncoding.BASE32)
					{
						PluginDebug.AddError("Migration of entry failed",
							"Uuid: " + pe.Uuid.ToHexString(),
							"Encoding not supported: " + otp.Encoding.ToString());
						continue;
					}
					if (otp.Hash != KPOTPHash.SHA1)
					{
						PluginDebug.AddError("Migration of entry failed",
							"Uuid: " + pe.Uuid.ToHexString(),
							"Hash not supported: " + otp.Hash.ToString());
						continue;
					}
					if (otp.Type != KPOTPType.TOTP)
					{
						PluginDebug.AddError("Migration of entry failed",
							"Uuid: " + pe.Uuid.ToHexString(),
							"Type not supported: " + otp.Type.ToString());
						continue;
					}

					string s = "key=" + otp.OTPSeed.ReadString();
					if (otp.Length != 6)
						s += "&size=" + otp.Length.ToString();
					if (otp.Type == KPOTPType.HOTP)
					{
						s += "&type=hotp";
						if (otp.HOTPCounter > 0)
							s += "&counter=" + otp.HOTPCounter.ToString();
					}
					if ((otp.Type == KPOTPType.TOTP) && (otp.TOTPTimestep != 30))
						s += "&step=" + otp.TOTPTimestep.ToString();
					pe.Strings.Set("otp", new ProtectedString(true, s));
					if (pe.Strings.Exists("otp"))
						EntriesMigrated++;
					if (bRemove)
					{
						otp.OTPSeed = ProtectedString.EmptyEx;
						try
						{
							handler.IgnoreBuffer = true;
							OTPDAO.SaveOTP(otp, pe);
						}
						finally { handler.IgnoreBuffer = false; }
					}
				}
			}
			finally
			{
				EndLogger();
			}
			MigratePlaceholder(Config.Placeholder, OtherPluginPlaceholder);
		}
	}

	public class MigrationTraytotp : MigrationBase
	{
		private string m_OtherPluginPlaceholder = "TOTP";
		private string m_TOTP_Seed = "TOTP Seed";
		private string m_TOTP_Settings = "TOTP Settings";

		public MigrationTraytotp()
		{
			m_OtherPluginPlaceholder = "{" + KeePass.Program.Config.CustomConfig.GetString("autotype_fieldname", m_OtherPluginPlaceholder) + "}";
			m_TOTP_Seed = KeePass.Program.Config.CustomConfig.GetString("totpseed_stringname", m_TOTP_Seed);
			m_TOTP_Settings = KeePass.Program.Config.CustomConfig.GetString("totpsettings_stringname", m_TOTP_Settings);
		}

		public override void MigrateToKeePassOTP(bool bRemove, out int EntriesOverall, out int EntriesMigrated)
		{
			EntriesOverall = EntriesMigrated = -1;
			if (!m_bInitialized) return;
			EntriesOverall = EntriesMigrated = 0;

			List<PwEntry> lEntries = m_db.RootGroup.GetEntries(true).Where(x => x.Strings.Exists(m_TOTP_Seed)).ToList();
			EntriesOverall = lEntries.Count;
			if (lEntries.Count == 0) return;

			if (!OTPDAO.EnsureOTPSetupPossible(lEntries[0])) return;
			OTPDAO.OTPHandler_Base handler = OTPDAO.GetOTPHandler(lEntries[0]);
			InitLogger("KeeTrayTOTP -> KeePassOTP", lEntries.Count);
			try
			{
				foreach (PwEntry pe in lEntries)
				{
					IncreaseLogger();
					string seed = pe.Strings.ReadSafe(m_TOTP_Seed);
					string settings = pe.Strings.ReadSafe(m_TOTP_Settings);
					if (string.IsNullOrEmpty(settings))
					{
						PluginDebug.AddError("Migration of entry failed",
							"Uuid: " + pe.Uuid.ToHexString(),
							"OTP data: not defined");
						continue;
					}
					var parameters = settings.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
					if (parameters.Count() < 2)
					{
						PluginDebug.AddError("Migration of entry failed",
							"Uuid: " + pe.Uuid.ToHexString(),
							"OTP data: " + settings);
						continue;
					}
					var otp = OTPDAO.GetOTP(pe);
					otp.OTPSeed = new ProtectedString(true, MigrateString(seed));
					otp.TOTPTimestep = MigrateInt(parameters[0], 30);
					if (parameters[1].ToLowerInvariant() == "s") otp.Type = KPOTPType.STEAM;
					else
					{
						int l = MigrateInt(parameters[1], -1);
						if (l == -1)
						{
							PluginDebug.AddError("Migration of entry failed",
								"Uuid: " + pe.Uuid.ToHexString(),
								"OTP data: " + settings);
							continue;
						}
						otp.Length = l;
					}
					if ((parameters.Count() > 2) && !string.IsNullOrEmpty(parameters[2]))
						otp.TimeCorrectionUrl = parameters[2];
					if (otp.Valid)
					{
						EntriesMigrated++;
						try
						{
							handler.IgnoreBuffer = true;
							OTPDAO.SaveOTP(otp, pe);
						}
						finally { handler.IgnoreBuffer = false; }
						if (bRemove)
						{
							pe.Strings.Remove(m_TOTP_Seed);
							pe.Strings.Remove(m_TOTP_Settings);
						}
					}
					else
					{
						string s = string.Empty;
						for (int i = 0; i < parameters.Count(); i++)
						{
							if (parameters[i].ToLowerInvariant().StartsWith("key="))
								s += "key=<secret>";
							else
								s += parameters[i];
							if (i < parameters.Count() - 1) s += "&";
						}
						PluginDebug.AddError("Migration of entry failed",
							"Uuid: " + pe.Uuid.ToHexString(),
							"OTP data: " + s);
					}
				}
			}
			finally
			{
				EndLogger();
			}
			MigratePlaceholder(m_OtherPluginPlaceholder, Config.Placeholder);
		}

		public override void MigrateFromKeePassOTP(bool bRemove, out int EntriesOverall, out int EntriesMigrated)
		{
			EntriesOverall = EntriesMigrated = -1;
			if (!m_bInitialized) return;
			EntriesOverall = EntriesMigrated = 0;

			OTPDAO.OTPHandler_DB h = OTPDAO.GetOTPHandler(m_db);
			if ((h != null) && !h.EnsureOTPUsagePossible(null)) return;

			PwObjectList<PwEntry> lEntries = m_db.RootGroup.GetEntries(true);
			if (lEntries.Count() == 0) return;

			OTPDAO.OTPHandler_Base handler = OTPDAO.GetOTPHandler(lEntries.GetAt(0));
			InitLogger("KeePassOTP -> KeeTrayTOTP", lEntries.Count());
			try
			{
				foreach (PwEntry pe in lEntries)
				{
					IncreaseLogger();
					KPOTP otp = OTPDAO.GetOTP(pe);
					if (!otp.Valid) continue;
					EntriesOverall++;
					if (otp.Encoding != KPOTPEncoding.BASE32)
					{
						PluginDebug.AddError("Migration of entry failed",
							"Uuid: " + pe.Uuid.ToHexString(),
							"Encoding not supported: " + otp.Encoding.ToString());
						continue;
					}
					if (otp.Hash != KPOTPHash.SHA1)
					{
						PluginDebug.AddError("Migration of entry failed",
							"Uuid: " + pe.Uuid.ToHexString(),
							"Hash not supported: " + otp.Hash.ToString());
						continue;
					}
					if (otp.Type == KPOTPType.HOTP)
					{
						PluginDebug.AddError("Migration of entry failed",
							"Uuid: " + pe.Uuid.ToHexString(),
							"Type not supported: " + otp.Type.ToString());
						continue;
					}
					string settings = otp.TOTPTimestep.ToString() + ";" + (otp.Type == KPOTPType.TOTP ? otp.Length.ToString() : "S");
					if (otp.TimeCorrectionUrlOwn)
						settings += ";" + pe.Strings.ReadSafe(PwDefs.UrlField);
					else if (!string.IsNullOrEmpty(otp.TimeCorrectionUrl))
						settings += ";" + otp.TimeCorrectionUrl;
					pe.Strings.Set(m_TOTP_Seed, otp.OTPSeed);
					pe.Strings.Set(m_TOTP_Settings, new ProtectedString(false, settings));
					EntriesMigrated++;
					if (bRemove)
					{
						otp.OTPSeed = ProtectedString.EmptyEx;
						try
						{
							handler.IgnoreBuffer = true;
							OTPDAO.SaveOTP(otp, pe);
						}
						finally { handler.IgnoreBuffer = false; }
					}
				}
			}
			finally { EndLogger(); }
			MigratePlaceholder(Config.Placeholder, m_OtherPluginPlaceholder);
		}
	}

	public class MigrationKeePass : MigrationBase
	{
		private static Dictionary<KPOTPEncoding, string> m_dHotpStrings = null;
		private static Dictionary<KPOTPEncoding, string> m_dTotpStrings = null;

		static MigrationKeePass()
		{
			m_dHotpStrings = new Dictionary<KPOTPEncoding, string>();
			m_dHotpStrings[KPOTPEncoding.UTF8] = "HmacOtp-Secret";
			m_dHotpStrings[KPOTPEncoding.HEX] = "HmacOtp-Secret-Hex";
			m_dHotpStrings[KPOTPEncoding.BASE32] = "HmacOtp-Secret-Base32";
			m_dHotpStrings[KPOTPEncoding.BASE64] = "HmacOtp-Secret-Base64";

			m_dTotpStrings = new Dictionary<KPOTPEncoding, string>();
			m_dTotpStrings[KPOTPEncoding.UTF8] = "TimeOtp-Secret";
			m_dTotpStrings[KPOTPEncoding.HEX] = "TimeOtp-Secret-Hex";
			m_dTotpStrings[KPOTPEncoding.BASE32] = "TimeOtp-Secret-Base32";
			m_dTotpStrings[KPOTPEncoding.BASE64] = "TimeOtp-Secret-Base64";
		}

		public override void MigrateToKeePassOTP(bool bRemove, out int EntriesOverall, out int EntriesMigrated)
		{
			EntriesOverall = EntriesMigrated = -1;
			if (!m_bInitialized) return;

			MigrateToKeePassOTP_Hotp(bRemove, out EntriesOverall, out EntriesMigrated);

			int Totp_EntriesOverall;
			int Totp_EntriesMigrated;
			MigrateToKeePassOTP_Totp(bRemove, out Totp_EntriesOverall, out Totp_EntriesMigrated);

			EntriesOverall += Totp_EntriesOverall;
			EntriesMigrated += Totp_EntriesMigrated;
		}

		private const string PLACEHOLDER_HOTP = "{HMACOTP}";
		private const string HOTP_COUNTER = "HmacOtp-Counter";
		private void MigrateToKeePassOTP_Hotp(bool bRemove, out int EntriesOverall, out int EntriesMigrated)
		{
			EntriesOverall = EntriesMigrated = 0;
			List<PwEntry> lEntries = m_db.RootGroup.GetEntries(true).Where(x => x.Strings.Exists(HOTP_COUNTER)).ToList();
			EntriesOverall = lEntries.Count;
			if (lEntries.Count == 0) return;

			if (!OTPDAO.EnsureOTPSetupPossible(lEntries[0])) return;
			OTPDAO.OTPHandler_Base handler = OTPDAO.GetOTPHandler(lEntries[0]);
			InitLogger("KeePass -> KeePassOTP (HOTP)", lEntries.Count);
			try
			{
				foreach (PwEntry pe in lEntries)
				{
					IncreaseLogger();
					KPOTPEncoding enc = KPOTPEncoding.BASE32;
					bool bFound = false;
					string seed = null;
					foreach (KeyValuePair<KPOTPEncoding, string> kvp in m_dHotpStrings)
					{
						if (pe.Strings.Exists(kvp.Value))
						{
							enc = kvp.Key;
							seed = pe.Strings.ReadSafe(kvp.Value);
							bFound = true;
							break;
						}
					}
					if (!bFound)
					{
						PluginDebug.AddError("Migration of entry failed",
							"Uuid: " + pe.Uuid.ToHexString(),
							"OTP data: not defined");
						continue;
					}
					var otp = OTPDAO.GetOTP(pe);
					otp.Type = KPOTPType.HOTP;
					otp.Encoding = enc;
					otp.OTPSeed = new ProtectedString(true, MigrateString(seed));
					otp.HOTPCounter = MigrateInt(pe.Strings.ReadSafe(HOTP_COUNTER), 0);
					if (otp.Valid)
					{
						EntriesMigrated++;
						try
						{
							handler.IgnoreBuffer = true;
							OTPDAO.SaveOTP(otp, pe);
						}
						finally { handler.IgnoreBuffer = false; }
						if (bRemove)
						{
							pe.Strings.Remove(m_dHotpStrings[enc]);
							pe.Strings.Remove(HOTP_COUNTER);
						}
					}
					else
					{
						PluginDebug.AddError("Migration of entry failed",
							"Uuid: " + pe.Uuid.ToHexString(),
							"OTP data: " + m_dHotpStrings[enc]);
					}
				}
			}
			finally
			{
				EndLogger();
			}
			MigratePlaceholder(PLACEHOLDER_HOTP, Config.Placeholder);
		}

		private const string PLACEHOLDER_TOTP = "{TIMEOTP}";
		private const string TOTPLENGTH = "TimeOtp-Length";
		private const string TOTPPERIOD = "TimeOtp-Period";
		private const string TOTPHASH = "TimeOtp-Algorithm";
		private void MigrateToKeePassOTP_Totp(bool bRemove, out int EntriesOverall, out int EntriesMigrated)
		{
			EntriesOverall = EntriesMigrated = 0;
			Dictionary<PwEntry, KPOTPEncoding> dEntries = new Dictionary<PwEntry, KPOTPEncoding>();
			foreach (KeyValuePair<KPOTPEncoding, string> kvp in m_dTotpStrings)
			{
				List<PwEntry> lHelp = m_db.RootGroup.GetEntries(true).Where(x => x.Strings.Exists(kvp.Value)).ToList();
				foreach (PwEntry pe in lHelp)
				{
					if (!dEntries.ContainsKey(pe)) dEntries[pe] = kvp.Key;
				}
			}
			EntriesOverall = dEntries.Count;
			if (dEntries.Count == 0) return;

			if (!OTPDAO.EnsureOTPSetupPossible(dEntries.Keys.First())) return;
			OTPDAO.OTPHandler_Base handler = OTPDAO.GetOTPHandler(dEntries.Keys.First());
			InitLogger("KeePass -> KeePassOTP (TOTP)", dEntries.Count);
			try
			{
				foreach (KeyValuePair<PwEntry, KPOTPEncoding> kvp in dEntries)
				{
					IncreaseLogger();
					KPOTPEncoding enc = kvp.Value;
					PwEntry pe = kvp.Key;

					var otp = OTPDAO.GetOTP(pe);
					otp.Encoding = enc;
					otp.OTPSeed = new ProtectedString(true, MigrateString(pe.Strings.ReadSafe(m_dTotpStrings[enc])));

					string hash = pe.Strings.ReadSafe(TOTPHASH).ToLowerInvariant();
					if (hash.Contains("sha-512")) otp.Hash = KPOTPHash.SHA512;
					else if (hash.Contains("sha-256")) otp.Hash = KPOTPHash.SHA256;
					else otp.Hash = KPOTPHash.SHA1;

					otp.Length = MigrateInt(pe.Strings.ReadSafe(TOTPLENGTH), 6);
					otp.TOTPTimestep = MigrateInt(pe.Strings.ReadSafe(TOTPPERIOD), 30);

					otp.HOTPCounter = MigrateInt(pe.Strings.ReadSafe(HOTP_COUNTER), 0);
					if (otp.Valid)
					{
						EntriesMigrated++;
						try
						{
							handler.IgnoreBuffer = true;
							OTPDAO.SaveOTP(otp, pe);
						}
						finally { handler.IgnoreBuffer = false; }
						if (bRemove)
						{
							pe.Strings.Remove(m_dTotpStrings[enc]);
							pe.Strings.Remove(TOTPHASH);
							pe.Strings.Remove(TOTPLENGTH);
							pe.Strings.Remove(TOTPPERIOD);
						}
					}
					else
					{
						PluginDebug.AddError("Migration of entry failed",
							"Uuid: " + pe.Uuid.ToHexString(),
							"OTP data: " + m_dHotpStrings[enc]);
					}
				}
			}
			finally
			{
				EndLogger();
			}
			MigratePlaceholder(PLACEHOLDER_TOTP, Config.Placeholder);
		}

		private static Version m_vKeePass247 = new Version(2, 47);
		public override void MigrateFromKeePassOTP(bool bRemove, out int EntriesOverall, out int EntriesMigrated)
		{
			EntriesOverall = EntriesMigrated = -1;
			if (!m_bInitialized) return;
			EntriesOverall = EntriesMigrated = 0;

			OTPDAO.OTPHandler_DB h = OTPDAO.GetOTPHandler(m_db);
			if ((h != null) && !h.EnsureOTPUsagePossible(null)) return;

			PwObjectList<PwEntry> lEntries = m_db.RootGroup.GetEntries(true);
			if (lEntries.Count() == 0) return;

			OTPDAO.OTPHandler_Base handler = OTPDAO.GetOTPHandler(lEntries.GetAt(0));
			InitLogger("KeePassOTP -> KeeTrayTOTP", lEntries.Count());
			try
			{
				foreach (PwEntry pe in lEntries)
				{
					IncreaseLogger();
					KPOTP otp = OTPDAO.GetOTP(pe);
					if (!otp.Valid) continue;
					EntriesOverall++;
					if (otp.Type != KPOTPType.HOTP && otp.Type != KPOTPType.TOTP)
					{
						PluginDebug.AddError("Migration of entry failed",
							"Uuid: " + pe.Uuid.ToHexString(),
							"Type not supported: " + otp.Type.ToString());
						continue;
					}

					if (otp.Type == KPOTPType.TOTP)
					{
						if (Tools.KeePassVersion < m_vKeePass247)
						{
							PluginDebug.AddError("Migration of entry failed",
							"Uuid: " + pe.Uuid.ToHexString(),
							"Type not supported: " + otp.Type.ToString(),
							"Minimum required KeePass version: " + m_vKeePass247.ToString());
						}
						foreach (var line in m_dTotpStrings)
						{
							if (line.Key == otp.Encoding) pe.Strings.Set(line.Value, otp.OTPSeed);
							else pe.Strings.Remove(line.Value);
						}
						
						if (otp.TOTPTimestep == 30) pe.Strings.Remove(TOTPPERIOD);
						else pe.Strings.Set(TOTPPERIOD, new ProtectedString(false, otp.TOTPTimestep.ToString()));

						if (otp.Length == 6) pe.Strings.Remove(TOTPLENGTH);
						else pe.Strings.Set(TOTPLENGTH, new ProtectedString(false, otp.Length.ToString()));

						if (otp.Hash == KPOTPHash.SHA1) pe.Strings.Remove(TOTPHASH);
						else if (otp.Hash == KPOTPHash.SHA256) pe.Strings.Set(TOTPHASH, new ProtectedString(false, "HMAC-SHA-256"));
						else if (otp.Hash == KPOTPHash.SHA512) pe.Strings.Set(TOTPHASH, new ProtectedString(false, "HMAC-SHA-512"));

						bool bDummy;
						MigratePlaceholder(Config.Placeholder, PLACEHOLDER_TOTP, pe, out bDummy);
					}
					else if (otp.Type == KPOTPType.HOTP)
					{
						if (otp.Length != 6)
						{
							PluginDebug.AddError("Migration of entry failed",
								"Uuid: " + pe.Uuid.ToHexString(),
								"Length not supported: " + otp.Length.ToString());
							continue;
						}
						if (otp.Hash != KPOTPHash.SHA1)
						{
							PluginDebug.AddError("Migration of entry failed",
								"Uuid: " + pe.Uuid.ToHexString(),
								"Hash not supported: " + otp.Hash.ToString());
							continue;
						}

						foreach (var line in m_dHotpStrings)
						{
							if (line.Key == otp.Encoding) pe.Strings.Set(line.Value, otp.OTPSeed);
							else pe.Strings.Remove(line.Value);
						}
						pe.Strings.Set(HOTP_COUNTER, new ProtectedString(false, otp.HOTPCounter.ToString()));

						bool bDummy;
						MigratePlaceholder(Config.Placeholder, PLACEHOLDER_HOTP, pe, out bDummy);
					}

					EntriesMigrated++;
					if (bRemove)
					{
						otp.OTPSeed = ProtectedString.EmptyEx;
						try
						{
							handler.IgnoreBuffer = true;
							OTPDAO.SaveOTP(otp, pe);
						}
						finally { handler.IgnoreBuffer = false; }
					}
				}
			}
			finally { EndLogger(); }
			MigratePlaceholder(Config.Placeholder, PLACEHOLDER_TOTP); //In case something is defined on group level (could be right, could be wrong, ...)
		}
	}
}
