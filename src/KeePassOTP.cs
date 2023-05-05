using KeePassLib.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

using PluginTools;
using KeePassLib.Security;
using KeePassLib.Serialization;

namespace KeePassOTP
{
	public enum KPOTPType : int
	{
		HOTP = 0,
		TOTP,
		STEAM,
		YANDEX
	}

	public enum KPOTPHash : int
	{
		SHA1 = 0,
		SHA256,
		SHA512
	}

	public enum KPOTPEncoding : int
	{
		BASE32 = 0,
		BASE64,
		HEX,
		UTF8
	}

	public class KPOTP
	{
		private static System.Reflection.MethodInfo miConfigureWebClient = null;

		public static readonly DateTime UnixStartUTC = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		private static Dictionary<string, TimeSpan> m_timeCorrectionUrls = new Dictionary<string, TimeSpan>();

		public KPOTPHash Hash = KPOTPHash.SHA1;
		private KPOTPType m_Type = KPOTPType.TOTP;
		public KPOTPType Type
		{
			get { return m_Type; }
			set
			{
				m_Type = value;
				if (m_Type == KPOTPType.STEAM) Length = 5;
				else Length = Length; // Ensure proper length (Steam = 5 digits)
			}
		}

		public KPOTPEncoding Encoding = KPOTPEncoding.BASE32;

		private string m_YandexPin = string.Empty;
		public string YandexPin
		{
			get { return m_YandexPin; }
			set { m_YandexPin = value; }
		}

		public int m_length = 6;
		public int Length
		{
			get { return m_length; }
			set
			{
				if (Type == KPOTPType.STEAM) m_length = 5;
				else m_length = Math.Min(10, Math.Max(value, 6));
			}
		}

		private int m_timestep = 30;
		public int TOTPTimestep
		{
			get { return m_timestep; }
			set { m_timestep = Math.Max(value, 1); }
		}

		private int m_counter = 0;
		public int HOTPCounter
		{
			get { return m_counter; }
			set { m_counter = Math.Max(value, 0); }
		}

		private byte[] m_key;
		private ProtectedString m_seed = ProtectedString.EmptyEx;
		public bool SanitizeChanged { get; private set; }
		public ProtectedString OTPSeed
		{
			get { return m_seed; }
			set { SetSeed(value); }
		}

		public bool TimeCorrectionUrlOwn = false;
		private string m_url = string.Empty;
		public string TimeCorrectionUrl
		{
			get { return m_url; }
			set { SetURL(value); }
		}

		public string Issuer;
		public string Label;
		public ProtectedString OTPAuthString
		{
			get { return GetOTPAuthString(); }
			set { SetOTPAuthString(value); }
		}

		public bool Valid 
		{ 
			get 
			{
				if (Type == KPOTPType.YANDEX)
				{
					if (string.IsNullOrEmpty(YandexPin)) return false;
					int l;
					if (!KeePassOTP.YandexPin.Verify(YandexPin, out l)) return false;
					return !m_seed.IsEmpty && !string.IsNullOrEmpty(YandexPin);
				}
				else return (m_key != null) && !m_seed.IsEmpty; 
			} 
		}

		public TimeSpan OTPTimeCorrection = TimeSpan.Zero;
		private DateTime UtcNow { get { return DateTime.UtcNow - OTPTimeCorrection; } }

		private long Ticks
		{
			get
			{
				var ElapsedSeconds = (long)Math.Floor((UtcNow - UnixStartUTC).TotalSeconds);
				return ElapsedSeconds / m_timestep;
			}
		}

		public int RemainingSeconds
		{
			get
			{
				var ElapsedSeconds = (long)Math.Floor((UtcNow - UnixStartUTC).TotalSeconds);
				return m_timestep - (int)(ElapsedSeconds % m_timestep);
			}
		}

		public ProtectedString RecoveryCodes = ProtectedString.EmptyEx;

		static KPOTP()
		{
			miConfigureWebClient = typeof(IOConnection).GetMethod("ConfigureWebClient",
				System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic,
				null,
				new Type[] { typeof(System.Net.WebClient) },
				null);
		}

		public KPOTP()
        {

        }

		public string GetOTP()
		{
			return GetOTP(false, false);
		}

		public string GetOTP(bool ShowNext, bool CheckTime)
		{
			if (!Valid) return string.Empty;
			//List of time correction data is filled asynchronously
			//Call it synchronously as it is required now!
			if (CheckTime && Type != KPOTPType.HOTP) GetTimeCorrection(m_url);

			if (Type == KPOTPType.YANDEX)
			{
				return GetYandexData(ShowNext);
			}
			byte[] data;
			switch (Type)
			{
				case KPOTPType.HOTP: data = GetHOTPData(ShowNext); break;
				default: data = GetTOTPData(ShowNext); break;
			}
			if (data == null) return string.Empty;
			byte[] hash = ComputeHash(data);
			MemUtil.ZeroByteArray(data);
			int offset = hash[hash.Length - 1] & 0xF;

			int binary = ((hash[offset] & 0x7F) << 24) |
							 ((hash[offset + 1] & 0xFF) << 16) |
							 ((hash[offset + 2] & 0xFF) << 8) |
							 (hash[offset + 3] & 0xFF);

			string result = string.Empty;
			switch (Type)
			{
				case KPOTPType.STEAM: result = GetSteamString(binary); break;
				default: result = (binary % (int)Math.Pow(10, Length)).ToString().PadLeft(Length, '0'); break;
			}

			return result + (string.IsNullOrEmpty(m_url) || m_timeCorrectionUrls.ContainsKey(m_url) ? string.Empty : "*");
		}

		public string ReadableOTP(string otp)
		{
			if (string.IsNullOrEmpty(otp)) return string.Empty;
			bool bFinal = otp.IndexOf("*") < 0;
			if (!bFinal) otp = otp.Replace("*", string.Empty);
			int split = (int)Math.Ceiling((decimal)otp.Length / 2);
			otp = otp.Substring(0, split) + " " + otp.Substring(split);
			if (!bFinal) otp += "*";
			return otp;
		}
		private string GetYandexData(bool showNext)
		{
			return GetYandexData(showNext, Ticks);
		}
		private string GetYandexData(bool showNext, long ticks)
		{
			YandexSecret ys;
			YandexPin yp;
			if (!YandexSecret.TryCreate(OTPSeed, out ys)) return null;
			if (!KeePassOTP.YandexPin.TryCreate(YandexPin, out yp)) return null;
			try
			{
				YaOtp yo = new YaOtp(ys, yp, () => showNext ? DateTime.UtcNow.AddSeconds(30) : DateTime.UtcNow);
				return yo.ComputeOtp();
			}
			catch
			{
				return null;
			}
		}

		private byte[] GetHOTPData(bool showNext)
		{
			return GetOTPData(showNext, m_counter);
		}

		private byte[] GetTOTPData(bool showNext)
		{
			return GetOTPData(showNext, Ticks);
		}

		/// <summary>
		/// Character set for authenticator code
		/// </summary>
		private static readonly char[] aSteamChars = new char[] { '2', '3', '4', '5',
			'6', '7', '8', '9', 'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'M', 'N', 'P',
			'Q', 'R', 'T', 'V', 'W', 'X', 'Y' };
		private string GetSteamString(int binary)
		{
			string result = string.Empty;
			for (int i = 0; i < Length; i++)
			{
				result += aSteamChars[binary % aSteamChars.Length];
				binary /= aSteamChars.Length;
			}

			return result;
		}

		private string GetYandexString(int binary)
		{
			string result = string.Empty;
			for (int i = 0; i < Length; i++)
			{
				result += aSteamChars[binary % aSteamChars.Length];
				binary /= aSteamChars.Length;
			}

			return result;
		}

		private byte[] GetOTPData(bool showNext, long value)
		{
			byte[] result = new byte[0];
			if (showNext)
				result = MemUtil.UInt64ToBytes((ulong)value + 1);
			else
				result = MemUtil.UInt64ToBytes((ulong)value);
			Array.Reverse(result);
			return result;
		}

		private byte[] ComputeHash(byte[] data)
		{
			HMAC hmac = null;

			if (Hash == KPOTPHash.SHA256) hmac = new HMACSHA256(m_key);
			else if (Hash == KPOTPHash.SHA512) hmac = new HMACSHA512(m_key);
			else hmac = new HMACSHA1(m_key, true);

			byte[] r = hmac.ComputeHash(data);
			hmac.Clear();
			return r;
		}

		private ProtectedString GetOTPAuthString()
		{
			string otpPrefix = "otpauth://" + (Type == KPOTPType.HOTP ? "hotp" : "totp") + "/";
			if (!string.IsNullOrEmpty(Issuer) && !string.IsNullOrEmpty(Label))
				otpPrefix += Encode(Issuer, true) + ":" + Encode(Label, true);
			else if (string.IsNullOrEmpty(Issuer))
				otpPrefix += Encode(Label, true);
			//else if (string.IsNullOrEmpty(Label))
			//	otpPrefix += "/" + Encode(Issuer, true);
			otpPrefix += "?secret=";
			string otpSuffix = string.Empty;
			if (Hash != KPOTPHash.SHA1)
				otpSuffix += "&algorithm=" + Hash.ToString();
			if (Length != 6)
				otpSuffix += "&digits=" + Length.ToString();
			if ((Type == KPOTPType.TOTP) && (TOTPTimestep != 30))
				otpSuffix += "&period=" + TOTPTimestep.ToString();
			if (Type == KPOTPType.HOTP)
				otpSuffix += "&counter=" + HOTPCounter.ToString();
			if (Encoding != KPOTPEncoding.BASE32)
				otpSuffix += "&encoding=" + Encoding.ToString();
			if (!string.IsNullOrEmpty(Issuer))
				otpSuffix += "&issuer=" + Encode(Issuer, false);
			if (Type == KPOTPType.STEAM)
				otpSuffix += "&encoder=steam";
			else if (Type == KPOTPType.YANDEX)
				otpSuffix += "&yandexpin=" + Encode(YandexPin,false) + "&encoder=yandex"; 
			return new ProtectedString(true, otpPrefix) + m_seed + new ProtectedString(true, otpSuffix);
		}

		private void SetOTPAuthString(ProtectedString value)
		{
			m_key = null;
			m_seed = ProtectedString.EmptyEx;
			Issuer = PluginTranslation.PluginTranslate.PluginName;
			Label = string.Empty;

			//otpauth strings contain all parameters and the seed
			//we do NOT want to remove protection from the seed

			int idx = 0;
			int i = 0;
			int secretstart = -1;
			char[] c = value.ReadChars();
			try
			{
				List<char> lSecret1 = new List<char>(("?secret=").ToCharArray());
				List<char> lSecret2 = new List<char>(("&secret=").ToCharArray());

				//Search for parameter 'secret'
				for (i = 0; i < c.Length; i++)
				{
					char check = char.ToLower(c[i]);
					if ((check != lSecret1[idx]) && (check != lSecret2[idx]))
					{
						idx = 0;
						continue;
					}
					idx++;
					if (idx == lSecret1.Count)
					{
						secretstart = i + 1;
						break;
					}
				}
				if (secretstart == -1) return;
				idx = 0;
				for (i = secretstart; i < c.Length; i++)
				{
					if (c[i] == '&') break;
					idx++;
				}
			}
			finally { MemUtil.ZeroArray(c); }

			ProtectedString seed = value.Remove(secretstart + idx, value.Length - secretstart - idx);
			seed = seed.Remove(0, secretstart);

			string s = value.Remove(secretstart, idx).ReadString();
			Uri u = new Uri(s);
			var parameters = System.Web.HttpUtility.ParseQueryString(u.Query);
			if (parameters.Count < 1) return;
			List<string> lKeys = parameters.AllKeys.ToList();

			string sLabelIssuer = u.AbsolutePath;
			if (!string.IsNullOrEmpty(sLabelIssuer))
			{
				List<string> lIssuer = sLabelIssuer.Split(new string[] { ":", "%3a", "%3A" }, StringSplitOptions.None).ToList();
				if (lIssuer.Count > 1)
				{
					Label = Decode(lIssuer[1]).Replace("/", string.Empty);
					Issuer = Decode(lIssuer[0]).Replace("/", string.Empty);
				}
				else if (lIssuer.Count == 1) Label = Decode(lIssuer[0]).Replace("/", string.Empty);
			}

			Type = u.Host.ToLowerInvariant() == "hotp" ? KPOTPType.HOTP : KPOTPType.TOTP;
			if (Type == KPOTPType.TOTP)
				TOTPTimestep = MigrateInt(parameters.Get("period"), 30);
			else
				HOTPCounter = MigrateInt(parameters.Get("counter"), 0);
			string hash = parameters.Get("algorithm");
			if (!string.IsNullOrEmpty(hash)) hash = hash.ToLowerInvariant();
			Hash = KPOTPHash.SHA1;
			if (hash == "sha256") Hash = KPOTPHash.SHA256;
			else if (hash == "sha512") Hash = KPOTPHash.SHA512;
			Length = MigrateInt(parameters.Get("digits"), 6);

			string encoding = parameters.Get("encoding");
			if (!string.IsNullOrEmpty(encoding)) encoding = encoding.ToLowerInvariant();
			Encoding = KPOTPEncoding.BASE32;
			if (encoding == "base64") Encoding = KPOTPEncoding.BASE64;
			else if (encoding == "hex") Encoding = KPOTPEncoding.HEX;
			else if (encoding == "utf8") Encoding = KPOTPEncoding.UTF8;

			string encoder = parameters.Get("encoder");
			KPOTPType tType = Type;
			if (Enum.TryParse(encoder, true, out tType))
				Type = tType;

			string sIssuerParameter = parameters.Get("issuer");
			if (!string.IsNullOrEmpty(sIssuerParameter)) Issuer = Decode(sIssuerParameter);

			if (Type == KPOTPType.YANDEX)
			{
				YandexPin = parameters.Get("yandexpin");
			}

			//Remove %3d / %3D at the end of the seed
			c = seed.ReadChars();
			idx = c.Length - 3;
			i = 0;
			while (idx > 0)
			{
				if ((c[idx] == '%') && (c[idx + 1] == '3') && (char.ToLowerInvariant(c[idx + 2]) == 'd'))
				{
					i++;
					idx -= 3;
				}
				else break;
			}
			if (i > 0)
				seed = seed.Remove(seed.Length - (i * 3), i * 3);

			SetSeed(seed);
		}

		private string Encode(string s, bool PathEncode)
		{
			//UrlPathEncode required to encode issue and label within the 'path'
			//special handling required, if # is contained
			//
			//This will not be processed by UrlPathEncode, but will break creating an Uri instance for parsing the otpauth string
			return PathEncode ? System.Web.HttpUtility.UrlPathEncode(s).Replace("#", "%23") : System.Web.HttpUtility.UrlEncode(s);
		}

		private string Decode(string s)
		{
			return System.Web.HttpUtility.UrlDecode(s.Replace("%23", "#"));
		}

		private int MigrateInt(string v, int def)
		{
			int r;
			if (!int.TryParse(v, out r)) r = def;
			return r;
		}

		private string GetArraySection(char[] c, int v1, int v2)
		{
			if (c.Length < v1 + v2) return string.Empty;
			string r = string.Empty;
			while (v2-- > 0)
				r += c[v1++];
			return r;
		}

		/// <summary>
		/// Remove spaces and in case of BASE32 correct some characters that are misread often
		/// </summary>
		/// <param name="seed"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		private ProtectedString SanitizeSeed(ProtectedString seed, KPOTPEncoding encoding)
		{
			Dictionary<byte, byte> dReplace = new Dictionary<byte, byte>();
			dReplace[(byte)'0'] = (byte)'O';
			dReplace[(byte)'1'] = (byte)'L';
			dReplace[(byte)'8'] = (byte)'B';
			try
			{
				ProtectedString work = ProtectedString.EmptyEx;
				foreach (byte b in seed.ReadUtf8())
				{
					if ((char)b == ' ')
					{
						SanitizeChanged = true; //Remember we changed something, important for .Equals
						continue;
					}
					if ((encoding == KPOTPEncoding.BASE32) && dReplace.ContainsKey(b))
					{
						SanitizeChanged = true; //Remember we changed something, important for .Equals
						work += new ProtectedString(true, new byte[] { dReplace[b] });
					}
					else work += new ProtectedString(true, new byte[] { b });
				}
				return work;
			}
			catch { return ProtectedString.EmptyEx; }
		}

		public void ResetSanitizedChange()
		{
			SanitizeChanged = false;
		}

		private void SetSeed(ProtectedString value)
		{
			ProtectedString work = SanitizeSeed(value, Encoding);
			m_seed = work;
			if (Type == KPOTPType.YANDEX) return;
			try
			{
				if (Encoding == KPOTPEncoding.BASE32)
				{
					while ((work.Length % 8) != 0) work += new ProtectedString(true, "=");
					m_key = PSConvert.ToBASE32(work);
				}
				else if (Encoding == KPOTPEncoding.BASE64)
				{
					while ((work.Length % 8) != 0) work += new ProtectedString(true, "=");
					m_key = Convert.FromBase64CharArray(work.ReadChars(), 0, work.Length);
				}
				else if (Encoding == KPOTPEncoding.HEX)
				{
					m_key = PSConvert.HexStringToByteArray(value);
				}
				else if (Encoding == KPOTPEncoding.UTF8)
				{
					m_key = work.ReadUtf8();
				}
				else
				{
					if (m_key != null) m_key.Initialize();
					m_seed = ProtectedString.EmptyEx;
				}
			}
			catch
			{
				if (m_key != null) m_key.Initialize();
				m_seed = ProtectedString.EmptyEx;
			}
		}

		/// Calculate time offset for all relevant entries
		/// Do NOT use Task.Run as this requires .NET 4.5 which will cause issues on Mono
		/// Mono reports .NET 4.0.3 being installed despite higher versions can be used
		/// This results in KeePass refusing to compile the plgx
		private /* async */ void SetURL(string url)
		{
			m_url = url == null ? string.Empty : url;
			//Don't use Task at all (https://github.com/Rookiestyle/KeePassOTP/issues/31)
			KeePassLib.Delegates.GAction<object> act = new KeePassLib.Delegates.GAction<object>((object o) => { OTPTimeCorrection = GetTimeCorrection(url); });
			System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(act));
		}

		private static TimeSpan GetTimeCorrection(string value)
		{
			Uri fullUrl = null;
			TimeSpan timeCorrection = TimeSpan.Zero;

			bool bKeyFound = m_timeCorrectionUrls.ContainsKey(value);

			//Quick check for 'valid' URL
			if (value.IndexOf(":") <= 0)
			{
				if (!string.IsNullOrEmpty(value) && !bKeyFound)
					PluginDebug.AddError("OTP time correction", 0, "Invalid URL: " + value, "Time correction: " + TimeSpan.Zero.ToString());
				lock (m_timeCorrectionUrls) { m_timeCorrectionUrls[value] = TimeSpan.Zero; }
				return m_timeCorrectionUrls[value];
			}

			//check for given string
			if (bKeyFound)
			{
				m_timeCorrectionUrls.TryGetValue(value, out timeCorrection);
				return timeCorrection;
			}

			//Calculate time offset
			try
			{
				fullUrl = new Uri(value);
			}
			catch (Exception ex)
			{
				lock (m_timeCorrectionUrls)
				{
					m_timeCorrectionUrls[value] = TimeSpan.Zero;
				}
				PluginDebug.AddError("OTP time correction", 0, "URL: " + value, "Error: " + ex.Message, "Time correction: " + m_timeCorrectionUrls[value].ToString());
				return m_timeCorrectionUrls[value];
			}

			string url = fullUrl.Scheme + "://" + fullUrl.Host;
			if (m_timeCorrectionUrls.ContainsKey(url))
			{
				m_timeCorrectionUrls.TryGetValue(url, out timeCorrection);
				if (!m_timeCorrectionUrls.ContainsKey(value))
					lock (m_timeCorrectionUrls) { m_timeCorrectionUrls[value] = timeCorrection; }
				PluginDebug.AddInfo("OTP time correction", 0, "URL: " + value, "Mapped URL: " + url, "Time correction: " + m_timeCorrectionUrls[value].ToString());
				return timeCorrection;
			}

			bool bException = false;
			try
			{
				System.Net.WebClient WebClient = new System.Net.WebClient();
				if (miConfigureWebClient != null) // Try to set KeePass' proxy settings
				{
					try { miConfigureWebClient.Invoke(null, new object[] { WebClient }); }
					catch { }
				}
				WebClient.DownloadData(url);
				var DateHeader = WebClient.ResponseHeaders.Get("Date");
				timeCorrection = DateTime.UtcNow - DateTime.Parse(DateHeader, System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat).ToUniversalTime();
			}
			catch (Exception ex)
			{
				timeCorrection = TimeSpan.Zero;
				bException = true;
				PluginDebug.AddError("OTP time correction", 0, "URL: " + url, "Error: " + ex.Message, "Time correction: " + timeCorrection.ToString());
			}
			lock (m_timeCorrectionUrls)
			{
				if (!m_timeCorrectionUrls.ContainsKey(value) && !bException)
					PluginDebug.AddInfo("OTP time correction", 0, "URL: " + url, "Time correction: " + timeCorrection.ToString());
				m_timeCorrectionUrls[value] = timeCorrection;
				m_timeCorrectionUrls[url] = timeCorrection;
			}
			return timeCorrection;
		}

		/// Calculate time offset for all relevant entries
		/// Do NOT use Task.Run as this requires .NET 4.5 which will cause issues on Mono
		/// Mono reports .NET 4.0.3 being installed despite higher versions can be used
		/// This results in KeePass refusing to compile the plgx
		public static /*async*/ void GetTimingsAsync(KeePassLib.PwDatabase db)
		{
			//Don't use TraverseTree as db content might change during processing
			//and this will result in an exception since TraverseTree uses 'foreach'

			//Don't use Task at all (https://github.com/Rookiestyle/KeePassOTP/issues/31)
			KeePassLib.Delegates.GAction<object> act = new KeePassLib.Delegates.GAction<object>((object o) => 
			{
				DateTime dtStart = DateTime.Now;
				var lEntries = db.RootGroup.GetEntries(true).Where(e => OTPDAO.OTPDefined(e) != OTPDAO.OTPDefinition.None).ToList(); //We're not interested in sites without OTP being set up
				var lUrl = new List<string>();
				foreach (var pe in lEntries)
				{
					//Only calculate if time correction is not disabled, cf. https://github.com/Rookiestyle/KeePassOTP/issues/134
					var kpotp = OTPDAO.GetOTP(pe);
					if (kpotp == null) continue;
					if (string.IsNullOrEmpty(kpotp.TimeCorrectionUrl) && !kpotp.TimeCorrectionUrlOwn) continue;
					var sUrl = kpotp.TimeCorrectionUrlOwn ? pe.Strings.ReadSafe(KeePassLib.PwDefs.UrlField) : kpotp.TimeCorrectionUrl;
					if (string.IsNullOrEmpty(sUrl) || lUrl.Contains(sUrl)) continue;
					
					lUrl.Add(sUrl);
				}
				foreach (string url in lUrl)
				{
					if (m_timeCorrectionUrls.ContainsKey(url)) continue;
					GetTimeCorrection(url);
					System.Threading.Thread.Sleep(100);
				};
				DateTime dtEnd = DateTime.Now;
				PluginDebug.AddInfo("Calculated OTP time corrections", 0, "Start: " + dtStart.ToLongTimeString(), "End: " + dtEnd.ToLongTimeString());

			});
			System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(act));
		}

		public static bool Equals(KPOTP otp1, KPOTP otp2)
		{
			bool dummy;
			return Equals(otp1, otp2, null, out dummy);
		}

		public static bool Equals(KPOTP otp1, KPOTP otp2, string url, out bool OnlyCounterChanged)
		{
			OnlyCounterChanged = false;
			if ((otp1 == null) && (otp2 == null)) return false;
			if ((otp1 == null) || (otp2 == null)) return true;

			if (!otp1.OTPSeed.Equals(otp2.OTPSeed, false)) return false;
			if (otp1.SanitizeChanged || otp2.SanitizeChanged) return false;
			if (otp1.Encoding != otp2.Encoding) return false;
			if (otp1.Hash != otp2.Hash) return false;
			if (otp1.Type != otp2.Type) return false;
			if (otp1.Length != otp2.Length) return false;
			if ((otp1.Type != KPOTPType.HOTP) && (otp1.TOTPTimestep != otp2.TOTPTimestep)) return false;
			if ((otp1.Type != KPOTPType.HOTP) && (otp1.TimeCorrectionUrlOwn != otp2.TimeCorrectionUrlOwn)) return false;
			if (otp1.Type == KPOTPType.YANDEX && otp1.YandexPin != otp2.YandexPin) return false;
			if (otp1.Type != KPOTPType.HOTP)
			{
				if (otp1.TimeCorrectionUrl != otp2.TimeCorrectionUrl) return false;
				if ((otp1.TimeCorrectionUrl == "OWNURL") && !string.IsNullOrEmpty(url) && (otp2.TimeCorrectionUrl != url)) return false;
				if ((otp2.TimeCorrectionUrl == "OWNURL") && !string.IsNullOrEmpty(url) && (otp1.TimeCorrectionUrl != url)) return false;
			}
			if (!otp1.RecoveryCodes.Equals(otp2.RecoveryCodes, false)) return false;

			if (otp1.Issuer != otp2.Issuer) return false;
			if (otp1.Label != otp2.Label) return false;

			if (otp1.Type == KPOTPType.HOTP && (otp1.HOTPCounter != otp2.HOTPCounter))
			{
				OnlyCounterChanged = true;
				return false;
			}

			return true;
		}

		public KPOTP Clone()
		{
			KPOTP result = new KPOTP();

			result.Type = this.Type;
			result.Encoding = this.Encoding;
			result.Hash = this.Hash;
			result.Length = this.Length;
			result.Issuer = this.Issuer;
			result.Label = this.Label;
			result.OTPSeed = this.OTPSeed;
			result.HOTPCounter = this.HOTPCounter;
			result.TimeCorrectionUrl = this.TimeCorrectionUrl;
			result.TimeCorrectionUrlOwn = this.TimeCorrectionUrlOwn;
			result.TOTPTimestep = this.TOTPTimestep;
			result.OTPTimeCorrection = this.OTPTimeCorrection;
			result.RecoveryCodes = new ProtectedString(true, this.RecoveryCodes.ReadUtf8());

			return result;
		}
	}
}
