using KeePass;
using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Utility;
using PluginTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace KeePassOTP
{
	internal static class Config
	{
		internal const string DefaultPlaceholder = "{KPOTP}";
		internal const string OTPFIELD = "otp";
		internal const string TIMECORRECTION = "KeePassOTP.TimeCorrection";
		internal const string DBKeySources = "KeePassOTP.KeySources";
		internal const string DBUsage = "KeePassOTP.UseDBForOTPSeeds";
		internal const string DBPreload = "KeePassOTP.PreloadOTP";

		internal const int TOTPSoonExpiring = 5;

		private const string Config_CheckTFA = "KeePassOTP.CheckTFA";
		private const string Config_Hotkey = "KeePassOTP.Hotkey";
		private const string Config_Placeholder = "KeePassOTP.Placeholder";
		private const string Config_KPOTPAutoSubmit = "KeePassOTP.KPOTPAutoSubmit";
		private const string Config_ShowHintSyncRequiresUnlock = "KeePassOTP.ShowHintSyncRequiresUnlock";
		private const string Config_ReadScreenForQRCodeExplanationShown = "KeePassOTP.ReadScreenForQRCodeExplanationShown";
		private static int HotkeyID = -1;

		internal static void Init()
		{
			Hotkey = Hotkey;
		}

		internal static void Cleanup()
		{
			PTHotKeyManager.UnregisterHotKey(HotkeyID);
		}

		internal static bool KPOTPAutoSubmit
		{
			get { return Program.Config.CustomConfig.GetBool(Config_KPOTPAutoSubmit, false); }
			set { Program.Config.CustomConfig.SetBool(Config_KPOTPAutoSubmit, value); }
		}

		internal static bool ShowHintSyncRequiresUnlock
		{
			get
			{
				bool bShow = Program.Config.CustomConfig.GetBool(Config_ShowHintSyncRequiresUnlock, true);
				Program.Config.CustomConfig.SetBool(Config_ShowHintSyncRequiresUnlock, false);
				return bShow;
			}
		}

		internal static bool ReadScreenForQRCodeExplanationShown
		{
			get { return Program.Config.CustomConfig.GetBool(Config_ReadScreenForQRCodeExplanationShown, false); }
			set { Program.Config.CustomConfig.SetBool(Config_ReadScreenForQRCodeExplanationShown, value); }
		}
		

		internal static bool CheckTFA
		{
			get { return Program.Config.CustomConfig.GetBool(Config_CheckTFA, true); }
			set { Program.Config.CustomConfig.SetBool(Config_CheckTFA, value); }
		}

		#region Hotkey and placeholder
		private static string m_Placeholder = string.Empty;
		internal static string Placeholder
		{
			get
			{
				if (string.IsNullOrEmpty(m_Placeholder))
					m_Placeholder = Program.Config.CustomConfig.GetString(Config_Placeholder, DefaultPlaceholder);
				return m_Placeholder;
			}
			set
			{
				if (!ValidPlaceholder(value)) return;
				KeePass.Util.Spr.SprEngine.FilterPlaceholderHints.Remove(m_Placeholder);
				m_Placeholder = value;
				KeePass.Util.Spr.SprEngine.FilterPlaceholderHints.Add(m_Placeholder);
				Program.Config.CustomConfig.SetString(Config_Placeholder, m_Placeholder);
			}
		}
		private static bool ValidPlaceholder(string p)
		{
			if (string.IsNullOrEmpty(p)) return false;
			if (!p.StartsWith("{")) p = "{" + p;
			if (!p.EndsWith("}")) p = p + "}";
			if (p.IndexOf('{', 1) >= 0) return false;
			if (p.IndexOf('}') + 1 < p.Length) return false;
			if (KeePass.Util.Spr.SprEngine.FilterPlaceholderHints.Contains(p)) return false;
			return true;
		}

		internal static Keys Hotkey
		{
			get { return GetHotkey(); }
			set { SetHotkey(value); }
		}
		private static void SetHotkey(Keys value)
		{
			PTHotKeyManager.UnregisterHotKey(HotkeyID);
			Program.Config.CustomConfig.SetString(Config_Hotkey, value.ToString());
			if (value != Keys.None)	HotkeyID = PTHotKeyManager.RegisterHotKey(value);
		}
		private static Keys GetHotkey()
		{
			string helper = Program.Config.CustomConfig.GetString(Config_Hotkey, Keys.None.ToString());
			Keys result = Keys.None;
			try
			{
				result = (Keys)Enum.Parse(typeof(Keys), helper);
			}
			catch { }
			return result;
		}
		#endregion 
		
		internal static bool PreloadOTPDB(this PwDatabase db)
		{
			if (db == null) return true;

			if (db.CustomData.Exists(DBPreload)) return StrUtil.StringToBool(db.CustomData.Get(DBPreload));
			return true;
		}

		internal static void PreloadOTPDB(this PwDatabase db, bool value)
		{
			if (db == null) return;
			if (value == db.PreloadOTPDB()) return;

			db.CustomData.Set(DBPreload, StrUtil.BoolToString(value));
			db.Modified = true;
			db.SettingsChanged = DateTime.UtcNow;
			Program.MainForm.UpdateUI(false, null, false, null, false, null, Program.MainForm.ActiveDatabase == db);
		}

		internal static bool UseDBForOTPSeeds(this PwDatabase db)
		{
			if (db == null) return true;

			//Check whether OTP db is to be used
			//
			//If yes, also check whether a KeePassOTB.DB entry exists and 
			//migrate all required fields to the database's CustomData
			bool bExists = db.CustomData.Exists(DBUsage);
			bool bUse = StrUtil.StringToBool(db.CustomData.Get(DBUsage));
			if (bExists && !bUse) return bUse;

			return true;
		}

		internal static void UseDBForOTPSeeds(this PwDatabase db, bool value)
		{
			if (db == null) return;
			bool bUse = true; //default
			if (db.CustomData.Exists(DBUsage))
				bUse = StrUtil.StringToBool(db.CustomData.Get(DBUsage));

			//Explicitly set value, even if it's the default value
			db.CustomData.Set(DBUsage, StrUtil.BoolToString(value));

			if (bUse == value) return;
			db.Modified = true;
			db.SettingsChanged = DateTime.UtcNow;
			Program.MainForm.UpdateUI(false, null, false, null, false, null, Program.MainForm.ActiveDatabase == db);
			OTPDAO.InitEntries(db);
			OTPDAO.RemoveHandler(db.IOConnectionInfo.Path, true);
		}
	}

	/// <summary>
	/// Conversion routines for ProtectedString objects
	/// </summary>
	internal static class PSConvert
	{
		internal static byte[] ToBASE32(ProtectedString s)
		{
			if (s.IsEmpty || ((s.Length % 8) != 0))
			{
				return null;
			}

			char[] str = s.ReadChars();
			ulong uMaxBits = (ulong)str.Length * 5UL;
			List<byte> l = new List<byte>((int)(uMaxBits / 8UL) + 1);

			for (int i = 0; i < str.Length; i += 8)
			{
				ulong u = 0;
				int nBits = 0;

				for (int j = 0; j < 8; ++j)
				{
					char ch = str[i + j];
					if (ch == '=') break;

					ulong uValue;
					if ((ch >= 'A') && (ch <= 'Z'))
						uValue = (ulong)(ch - 'A');
					else if ((ch >= 'a') && (ch <= 'z'))
						uValue = (ulong)(ch - 'a');
					else if ((ch >= '2') && (ch <= '7'))
						uValue = (ulong)(ch - '2') + 26UL;
					else { MemUtil.ZeroArray(str); return null; }

					u <<= 5;
					u += uValue;
					nBits += 5;
				}

				int nBitsTooMany = (nBits % 8);
				u >>= nBitsTooMany;
				nBits -= nBitsTooMany;

				int idxNewBytes = l.Count;
				while (nBits > 0)
				{
					l.Add((byte)(u & 0xFF));
					u >>= 8;
					nBits -= 8;
				}
				l.Reverse(idxNewBytes, l.Count - idxNewBytes);
			}
			MemUtil.ZeroArray(str);
			return l.ToArray();
		}

		internal static byte[] HexStringToByteArray(ProtectedString s)
		{
			if (s == null) { throw new ArgumentNullException("s"); }

			int nStrLen = s.Length;
			if ((nStrLen & 1) != 0) { return null; }

			byte[] pb = new byte[nStrLen / 2];
			byte bt;
			char ch;

			char[] strHex = s.ReadChars();
			for (int i = 0; i < nStrLen; i += 2)
			{
				ch = strHex[i];

				if ((ch >= '0') && (ch <= '9'))
					bt = (byte)(ch - '0');
				else if ((ch >= 'a') && (ch <= 'f'))
					bt = (byte)(ch - 'a' + 10);
				else if ((ch >= 'A') && (ch <= 'F'))
					bt = (byte)(ch - 'A' + 10);
				else { bt = 0; }

				bt <<= 4;

				ch = strHex[i + 1];
				if ((ch >= '0') && (ch <= '9'))
					bt += (byte)(ch - '0');
				else if ((ch >= 'a') && (ch <= 'f'))
					bt += (byte)(ch - 'a' + 10);
				else if ((ch >= 'A') && (ch <= 'F'))
					bt += (byte)(ch - 'A' + 10);
				else { }

				pb[i >> 1] = bt;
			}

			MemUtil.ZeroArray(strHex);
			return pb;
		}

		internal static ProtectedString ParseGoogleAuthExport(string s, out int iOTPCount)
		{
			iOTPCount = 0;
			ProtectedString psResult = ProtectedString.Empty;
			try
			{
				var u = new Uri(s);
				var param = System.Web.HttpUtility.ParseQueryString(u.Query);
				var b = Convert.FromBase64String(param["data"]);
				GoogleAuthenticatorImport gi = DeserializeGoogleAuthMigrationData(b);

				GoogleAuthenticatorImport.OtpParameters gAuthData = null;
				iOTPCount = gi.otp_parameters.Count;
				if (iOTPCount != 1)
				{
					using (GoogleAuthenticatorImportSelection selForm = new GoogleAuthenticatorImportSelection())
					{
						Tools.GlobalWindowManager(selForm);
						selForm.InitEx(gi.otp_parameters);
						if (selForm.ShowDialog(KeePass.UI.GlobalWindowManager.TopWindow) == DialogResult.OK)
						{
							gAuthData = selForm.SelectedEntry;
							if (gAuthData != null) iOTPCount = 1;
						}
						else iOTPCount = -1;
					}
					if (iOTPCount != 1) throw new ArgumentException("Expected exactly one OTP object, found: " + iOTPCount.ToString());
				}
				else gAuthData = gi.otp_parameters[0];
				KPOTP otp = new KPOTP();
				switch (gAuthData.Algorithm)
				{
					case GoogleAuthenticatorImport.Algorithm.AlgorithmSha256:
						otp.Hash = KPOTPHash.SHA256;
						break;
					case GoogleAuthenticatorImport.Algorithm.AlgorithmSha512:
						otp.Hash = KPOTPHash.SHA512;
						break;
					default:
						otp.Hash = KPOTPHash.SHA1;
						break;
				}

				switch (gAuthData.Type)
				{
					case GoogleAuthenticatorImport.OtpType.OtpTypeHotp:
						otp.Type = KPOTPType.HOTP;
						otp.HOTPCounter = (int)gAuthData.Counter;
						break;
					default:
						otp.Type = KPOTPType.TOTP;
						break;
				}

				switch (gAuthData.Digits)
				{
					case GoogleAuthenticatorImport.DigitCount.DigitCountEight:
						otp.Length = 8;
						break;
					default:
						otp.Length = 6;
						break;
				}

				otp.Issuer = gAuthData.Issuer;
				otp.Label = string.IsNullOrEmpty(gAuthData.Issuer) ? gAuthData.Name : gAuthData.Name.Remove(0, gAuthData.Issuer.Length + 1);
				otp.Encoding = KPOTPEncoding.BASE32;

				byte[] bSeed = ConvertBase64ToBase32(gAuthData.Secret);
				otp.OTPSeed = new ProtectedString(true, bSeed);
				psResult = otp.OTPAuthString;
			}
			catch { }
			return psResult;
		}

		private static GoogleAuthenticatorImport DeserializeGoogleAuthMigrationData(byte[] b)
		{
			GoogleAuthenticatorImport gi = new GoogleAuthenticatorImport();
			using (var ms = new System.IO.MemoryStream())
			{
				ms.Write(b, 0, b.Length);
				ms.Position = 0;
				gi = ProtoBuf.Serializer.Deserialize<GoogleAuthenticatorImport>(ms);
			}
			return gi;
		}

		private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
		private const int InByteSize = 8;
		private const int OutByteSize = 5;
		internal static byte[] ConvertBase64ToBase32(byte[] bytes)
		{
			List<byte> lBytes = new List<byte>();
			if ((bytes == null) || (bytes.Length == 0)) return lBytes.ToArray();

			int iCurrentBytePosition = 0;

			// Offset inside a single byte that <bytesPosition> points to (from left to right)
			// 0 - highest bit, 7 - lowest bit
			int bytesSubPosition = 0;
			//byte to look up in the base32 dictionary
			byte outputBase32Byte = 0;
			//The number of bits filled in the current output byte
			int iCurrentOutputPosition = 0;

			// Iterate through input buffer until we reach past the end of it
			while (iCurrentBytePosition < bytes.Length)
			{
				// Calculate the number of bits we can extract out of current input byte to fill missing bits in the output byte
				int bitsAvailableInByte = Math.Min(InByteSize - bytesSubPosition, OutByteSize - iCurrentOutputPosition);
				// Make space in the output byte
				outputBase32Byte <<= bitsAvailableInByte;
				// Extract the part of the input byte and move it to the output byte
				outputBase32Byte |= (byte)(bytes[iCurrentBytePosition] >> (InByteSize - (bytesSubPosition + bitsAvailableInByte)));
				// Update current sub-byte position
				bytesSubPosition += bitsAvailableInByte;

				// Check overflow
				if (bytesSubPosition >= InByteSize)
				{
					// Move to the next byte
					iCurrentBytePosition++;
					bytesSubPosition = 0;
				}

				// Update current base32 byte completion
				iCurrentOutputPosition += bitsAvailableInByte;
				// Check overflow or end of input array
				if (iCurrentOutputPosition >= OutByteSize)
				{
					// Drop the overflow bits
					outputBase32Byte &= 0x1F;  // 0x1F = 00011111 in binary
					// Add current Base32 byte and convert it to character
					lBytes.Add((byte)Base32Alphabet[outputBase32Byte]);
					// Move to the next byte
					iCurrentOutputPosition = 0;
				}
			}

			// Check if we have a remainder
			if (iCurrentOutputPosition > 0)
			{
				// Move to the right bits
				outputBase32Byte <<= (OutByteSize - iCurrentOutputPosition);

				// Drop the overflow bits
				outputBase32Byte &= 0x1F;  // 0x1F = 00011111 in binary

				// Add current Base32 byte and convert it to character
				//builder.Append(Base32Alphabet[outputBase32Byte]);
				lBytes.Add((byte)Base32Alphabet[outputBase32Byte]);
			}

			return lBytes.ToArray();
		}
	}

	public static class EventHelper
	{
		private static Dictionary<Type, List<FieldInfo>> m_dicEventFieldInfos = new Dictionary<Type, List<FieldInfo>>();

		private static BindingFlags AllBindings
		{
			get { return BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static; }
		}

		private static List<FieldInfo> GetTypeEventFields(Type t)
		{
			if (m_dicEventFieldInfos.ContainsKey(t))
				return m_dicEventFieldInfos[t];

			List<FieldInfo> lst = new List<FieldInfo>();
			BuildEventFields(t, lst);
			m_dicEventFieldInfos[t] = lst;
			return lst;
		}

		private static void BuildEventFields(Type t, List<FieldInfo> lst)
		{
			// Type.GetEvent(s) gets all Events for the type AND it's ancestors
			// Type.GetField(s) gets only Fields for the exact type.
			//  (BindingFlags.FlattenHierarchy only works on PROTECTED & PUBLIC
			//   doesn't work because Fieds are PRIVATE)
			var eil = t.GetEvents(AllBindings);
			foreach (EventInfo ei in eil)
			{
				Type dt = ei.DeclaringType;
				FieldInfo fi = t.GetField(ei.Name, AllBindings);
				if (fi == null)
					fi = dt.GetField(ei.Name, AllBindings);
				if (fi == null)
					fi = t.GetField("Event" + ei.Name, AllBindings);
				if (fi == null)
					fi = dt.GetField("Event" + ei.Name, AllBindings);
				if (fi != null)
					lst.Add(fi);
			}
		}

		private static EventHandlerList GetStaticEventHandlerList(Type t, object obj)
		{
			MethodInfo mi = t.GetMethod("get_Events", AllBindings);
			return (EventHandlerList)mi.Invoke(obj, new object[] { });
		}

		public static void RemoveEventHandlers(this object obj, string EventName, List<Delegate> handlers)
		{
			if (obj == null)
				return;

			Type t = obj.GetType();
			List<FieldInfo> event_fields = GetTypeEventFields(t);

			foreach (FieldInfo fi in event_fields)
			{
				if (EventName != "" && (string.Compare("Event" + EventName, fi.Name, true) != 0) && (string.Compare(EventName, fi.Name, true) != 0))
					continue;

				if (fi.IsStatic)
				{
					EventInfo ei = t.GetEvent(fi.Name, AllBindings);
					if (ei == null)
						ei = t.GetEvent(EventName, AllBindings);
					if (ei == null)
						ei = fi.DeclaringType.GetEvent(fi.Name, AllBindings);
					if (ei == null)
						ei = fi.DeclaringType.GetEvent(EventName, AllBindings);
					if (ei == null)
						continue;

					foreach (Delegate del in handlers)
						ei.RemoveEventHandler(obj, del);
				}
				else
				{
					EventInfo ei = t.GetEvent(fi.Name, AllBindings);
					if (ei == null)
						ei = t.GetEvent(EventName, AllBindings);
					if (ei == null)
						ei = fi.DeclaringType.GetEvent(fi.Name, AllBindings);
					if (ei == null)
						ei = fi.DeclaringType.GetEvent(EventName, AllBindings);
					if (ei != null)
					{
						foreach (Delegate del in handlers)
							ei.RemoveEventHandler(obj, del);
					}
				}
			}
		}

		public static bool AddEventHandlers(this object obj, string EventName, List<Delegate> handlers)
		{
			if (obj == null) return false;

			Type t = obj.GetType();
			List<FieldInfo> event_fields = GetTypeEventFields(t);

			bool added = false;
			foreach (FieldInfo fi in event_fields)
			{
				if (EventName != "" && (string.Compare("Event" + EventName, fi.Name, true) != 0) && (string.Compare(EventName, fi.Name, true) != 0))
					continue;

				if (fi.IsStatic)
				{
					EventInfo ei = t.GetEvent(fi.Name, AllBindings);
					if (ei == null)
						ei = t.GetEvent(EventName, AllBindings);
					if (ei == null)
						ei = fi.DeclaringType.GetEvent(fi.Name, AllBindings);
					if (ei == null)
						ei = fi.DeclaringType.GetEvent(EventName, AllBindings);
					if (ei == null)
						continue;

					foreach (var del in handlers)
						ei.AddEventHandler(obj, del);
					added = true;
				}
				else
				{
					EventInfo ei = t.GetEvent(fi.Name, AllBindings);
					if (ei == null)
						ei = t.GetEvent(EventName, AllBindings);
					if (ei == null)
						ei = fi.DeclaringType.GetEvent(fi.Name, AllBindings);
					if (ei == null)
						ei = fi.DeclaringType.GetEvent(EventName, AllBindings);
					if (ei != null)
					{
						foreach (var del in handlers)
							ei.AddEventHandler(obj, del);
						added = true;
					}
				}
			}
			return added;
		}

		public static List<Delegate> GetEventHandlers(this object obj, string EventName)
		{
			List<Delegate> result = new List<Delegate>();
			if (obj == null) return result;

			Type t = obj.GetType();
			List<FieldInfo> event_fields = GetTypeEventFields(t);
			EventHandlerList static_event_handlers = null;

			foreach (FieldInfo fi in event_fields)
			{
				if (EventName != "" && (string.Compare("Event" + EventName, fi.Name, true) != 0) && (string.Compare(EventName, fi.Name, true) != 0))
					continue;

				if (fi.IsStatic)
				{
					if (static_event_handlers == null)
						static_event_handlers = GetStaticEventHandlerList(t, obj);

					object idx = fi.GetValue(obj);
					Delegate eh = static_event_handlers[idx];
					if (eh == null)
						continue;

					Delegate[] dels = eh.GetInvocationList();
					if (dels == null)
						continue;
					result.AddRange(dels);
				}
				else
				{
					EventInfo ei = t.GetEvent(fi.Name, AllBindings);
					if (ei == null)
						ei = t.GetEvent(EventName, AllBindings);
					if (ei == null)
						ei = fi.DeclaringType.GetEvent(fi.Name, AllBindings);
					if (ei == null)
						ei = fi.DeclaringType.GetEvent(EventName, AllBindings);
					if (ei != null)
					{
						object val = fi.GetValue(obj);
						Delegate mdel = (val as Delegate);
						if (mdel != null)
							result.AddRange(mdel.GetInvocationList());
					}
				}
			}
			return result;
		}
	}

	internal static class PSExtensions
	{
		internal static bool Contains(this ProtectedString ps, string search)
		{
			return Contains(ps, new ProtectedString(false, search));
		}

		internal static bool Contains(this ProtectedString ps, ProtectedString search)
		{
			return IndexOfSubString(ps, search) > 0;
		}

		internal static ProtectedString Replace(this ProtectedString ps, string search, string replace)
		{
			return Replace(ps, new ProtectedString(true, StrUtil.Utf8.GetBytes(search)), new ProtectedString(true, StrUtil.Utf8.GetBytes(replace)));
		}

		internal static ProtectedString Replace(this ProtectedString ps, ProtectedString search, ProtectedString replace)
		{
			ProtectedString result = ps;
			int i = IndexOfSubString(result, search);
			while (i >= 0)
			{
				var psAfter = result.Remove(0, i + search.Length);
				var psBefore = result.Remove(i, result.Length - i);
				result = psBefore + replace + psAfter;
				i = IndexOfSubString(result, search);
			}
			return result;
		}

		private static int IndexOfSubString(ProtectedString ps, ProtectedString search)
		{
			if (ps == null || ps.IsEmpty || search == null || search.IsEmpty) return -1;
			int i = 0;
			int max = ps.Length - search.Length;
			if (max < 0) return -1;
			var haystack = ps.ReadChars();
			var needle = search.ReadChars();
			char[] slice = null;
			try
			{
				while (i <= max)
				{
					slice = haystack.Slice(i, search.Length);
					if (needle.SequenceEqual(slice)) return i;
					i++;
				}
				return -1;
			}
			catch
			{
				return -1;
			}
			finally
			{
				MemUtil.ZeroArray(haystack);
				MemUtil.ZeroArray(needle);
				if (slice != null) MemUtil.ZeroArray(slice);
			}
		}

		internal static T[] Slice<T>(this T[] data, int index, int length)
		{
			T[] result = new T[length];
			Array.Copy(data, index, result, 0, length);
			return result;
		}
	}
}
