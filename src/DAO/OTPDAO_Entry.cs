using System;
using System.Collections.Generic;
using KeePass;
using KeePassLib;
using KeePassLib.Security;
using PluginTools;

namespace KeePassOTP
{
	public static partial class OTPDAO //data access object
	{
		private class OTPHandler_Entry: OTPHandler_Base
		{
			public override bool EnsureOTPSetupPossible(PwEntry pe)
			{
				return pe != null;
			}

			public override bool EnsureOTPUsagePossible(PwEntry pe)
			{
				return EnsureOTPSetupPossible(pe);
			}

			public override string GetReadableOTP(PwEntry pe)
			{
				if (pe == null) return string.Empty;
				EntryOTP otp = EnsureEntry(pe);
				if (otp.ValidTo <= DateTime.UtcNow)
				{
					if (otp.kpotp.Valid)
					{
						otp.ReadableOTP = otp.kpotp.ReadableOTP(otp.kpotp.GetOTP());
						otp.ValidTo = DateTime.UtcNow.AddSeconds(otp.kpotp.RemainingSeconds);
					}
					else
					{
						otp.ReadableOTP = PluginTranslation.PluginTranslate.Error;
						otp.ValidTo = DateTime.MaxValue;
					}
					UpdateOTPBuffer(pe, otp);
				}
				if ((otp.ValidTo == DateTime.MaxValue) ||(otp.kpotp.Type == KPOTPType.HOTP))
					return otp.ReadableOTP;
				else
				{
					int r = (otp.ValidTo - DateTime.UtcNow).Seconds + 1;
					return otp.ReadableOTP + (r <= Config.TOTPSoonExpiring ? " (" + r.ToString() + ")" : string.Empty);
				}
			}

			public override KPOTP GetOTP(PwEntry pe)
			{
				return EnsureEntry(pe).kpotp;
			}

			public override void SaveOTP(KPOTP myOTP, PwEntry pe)
			{
				KPOTP prev = GetOTP(pe);
				bool OnlyCounterChanged = false;
				if (!SettingsChanged(pe, prev, myOTP, out OnlyCounterChanged)) return;

				PluginDebug.AddInfo("Update OTP data",
					"Entry uuid: " + pe.Uuid.ToString(), 
					"Only change of HOTP counter: " + OnlyCounterChanged.ToString());
				if (!OnlyCounterChanged)
				{
					//Create backup if something else than only the HOTP counter was changed
					pe.CreateBackup(pe.GetDB());
				}
				if (myOTP.OTPSeed.IsEmpty)
				{
					pe.Strings.Remove(Config.OTPFIELD);
					pe.CustomData.Remove(Config.TIMECORRECTION);
					pe.Strings.Remove(Config.RECOVERY);
				}
				else
				{
					//pe.Strings.Set(Config.SETTINGS, new ProtectedString(false, otpSettings));
					//pe.Strings.Set(Config.SEED, myOTP.OTPSeed);
					pe.Strings.Set(Config.OTPFIELD, myOTP.OTPAuthString);
					if (myOTP.RecoveryCodes.IsEmpty) pe.Strings.Remove(Config.RECOVERY);
					else pe.Strings.Set(Config.RECOVERY, myOTP.RecoveryCodes); 
					if (myOTP.TimeCorrectionUrlOwn)
						pe.CustomData.Set(Config.TIMECORRECTION, "OWNURL");
					else if (string.IsNullOrEmpty(myOTP.TimeCorrectionUrl) || (myOTP.TimeCorrectionUrl == "OFF"))
						pe.CustomData.Remove(Config.TIMECORRECTION);
					else
						pe.CustomData.Set(Config.TIMECORRECTION, myOTP.TimeCorrectionUrl);
				}
				pe.Touch(true, false);
			}

            private EntryOTP EnsureEntry(PwEntry pe)
			{
				EntryOTP otp;
				if (m_dEntryOTPData.TryGetValue(pe, out otp) && !IgnoreBuffer) 
					return otp;
				otp.db = pe.GetDB();
				otp.Loaded = true;
				otp.OTPDefined = OTPDefined(pe);
				if (otp.OTPDefined != OTPDefinition.Complete)
				{
					otp.ReadableOTP = otp.OTPDefined == OTPDefinition.Partial ? "???" : string.Empty;
					otp.ValidTo = DateTime.MaxValue;
					otp.kpotp = new KPOTP();
					InitIssuerLabel(otp.kpotp, pe);
				}
				else
				{
					otp.kpotp = GetSettings(pe);
					otp.ValidTo = KPOTP.UnixStartUTC;
				}
				UpdateOTPBuffer(pe, otp);
				PluginDebug.AddInfo("Fill OTP buffer", 0,
					"Entry uuid: " + pe.Uuid.ToString(),
					"OTP defined: " + otp.OTPDefined.ToString(),
					"OTP setup valid: " + otp.kpotp.Valid.ToString());
				return otp;
			}

            public override OTPDefinition OTPDefined(PwEntry pe)
			{
				return pe.Strings.Exists(Config.OTPFIELD) ? OTPDefinition.Complete : OTPDefinition.None;
			}

			private static KPOTP GetSettings(PwEntry pe)
			{
				KPOTP myOTP = new KPOTP();
				/*
				 * 
				myOTP.OTPSeed = ProtectedString.EmptyEx;
				string settings = pe.Strings.ReadSafe(Config.SETTINGS);
				PluginDebug.AddInfo("Set OTP settings", 0, "Uuid: " + pe.Uuid.ToString(), "Settings: " + settings);
				myOTP.Settings = settings;
				myOTP.OTPSeed = pe.Strings.GetSafe(Config.SEED);
				*/
				myOTP.OTPAuthString = pe.Strings.Get(Config.OTPFIELD);
				myOTP.RecoveryCodes = pe.Strings.GetSafe(Config.RECOVERY);
				string timeCorrection = pe.CustomData.Get(Config.TIMECORRECTION);
				timeCorrection = string.IsNullOrEmpty(timeCorrection) ? string.Empty : timeCorrection;
				if (timeCorrection == "OWNURL")
				{
					myOTP.TimeCorrectionUrlOwn = true;
					string url = pe.Strings.GetSafe(PwDefs.UrlField).ReadString();
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
		}
	}
}
