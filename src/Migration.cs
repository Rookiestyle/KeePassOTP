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

		public bool MigratePlaceholder(string from, string to, bool bTouchEntries)
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
				if (pe == null) { return true; }
				foreach (var a in pe.AutoType.Associations)
				{
					if (a.Sequence.Contains(from))
					{
						a.Sequence = a.Sequence.Replace(from, to);
						bChanged = true;
					}
				}
				if (pe.AutoType.DefaultSequence.Contains(from))
				{
					pe.AutoType.DefaultSequence = pe.AutoType.DefaultSequence.Replace(from, to);
					bChanged = true;
				}
				if (bTouchEntries) pe.Touch(true);
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
			MigratePlaceholder(OtherPluginPlaceholder, Config.Placeholder, false);
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
			MigratePlaceholder(Config.Placeholder, OtherPluginPlaceholder, false);
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
			MigratePlaceholder(m_OtherPluginPlaceholder, Config.Placeholder, false);
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
			MigratePlaceholder(Config.Placeholder, m_OtherPluginPlaceholder, false);
		}
	}
}
