using KeePass.DataExchange;
using KeePass.Resources;
using KeePassLib.Serialization;
using KeePassLib.Utility;
using PluginTools;
using PluginTranslation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace KeePassOTP
{
	public static class TFASites
	{
		//const string TFA_JSON_FILE = "https://twofactorauth.org/api/v2/tfa.json";
		const string TFA_JSON_FILE_DEFAULT = "https://2fa.directory/api/v2/tfa.json";

		public static string TFA_JSON_FILE
		{
			get
			{
				return KeePass.Program.Config.CustomConfig.GetString("KeePassOTP.TFASiteCheckURL", TFA_JSON_FILE_DEFAULT);
			}
		}
		public enum TFAPossible
		{
			Unknown,
			Yes,
			No,
			Error,
		}

		private class TFAData
		{
			public string name;
			public string url;
			public string img;
			public bool tfa;
			public bool sms;
			public bool email;
			public bool phone;
			public bool software;
			public bool hardware;
			public string doc;
			public string category;
		}

		private enum TFALoadProcess
		{
			NotStarted,
			Loading,
			Loaded,
			FileNotFound,
			Error
		}

		private static TFALoadProcess m_LoadState = TFALoadProcess.NotStarted;
		private static Dictionary<string, TFAData> m_dTFA = new Dictionary<string, TFAData>();
		private static object m_TFAReadLock = new object();

		public static void Init(bool bReload)
		{
			//If TFA check is disabled, this was done on purpose => Don't show info
			Config.CheckTFA_InfoShown |= !Config.CheckTFA;

			if (!Config.CheckTFA_InfoShown)
            {
				string sQuestion = string.Format(PluginTranslate.Options_Check2FA_Help, TFA_JSON_FILE)
					+ "\n\n" + KPRes.AskContinue;
				Config.CheckTFA = DialogResult.Yes == Tools.AskYesNo(sQuestion);
				Config.CheckTFA_InfoShown = true;
			}
			if (!Config.CheckTFA) return;
			if ((m_LoadState == TFALoadProcess.Loaded) && !bReload) return;
			if (bReload)
			{
				lock (m_TFAReadLock)
				{
					m_LoadState = TFALoadProcess.NotStarted;
				}
			}
			System.Threading.ThreadPool.QueueUserWorkItem(ReadOTPSites, false);
		}
		public static string GetTFAUrl(string url)
		{
			if (!Config.CheckTFA) return string.Empty;
			if (IsTFAPossible(url) != TFAPossible.Yes) return string.Empty;
			TFAData tfa;
			if (!m_dTFA.TryGetValue(url, out tfa)) return string.Empty;
			return !string.IsNullOrEmpty(tfa.doc) ? tfa.doc : url;
		}

		public static TFAPossible IsTFAPossible(string url)
		{
			if (!Config.CheckTFA) return TFAPossible.No;
			TFAData tfa;
			if (m_dTFA.TryGetValue(url, out tfa))
			{
				if (tfa.tfa) return TFAPossible.Yes;
				else return TFAPossible.No;
			}
			lock (m_TFAReadLock)
			{
				if (m_LoadState == TFALoadProcess.Error)
				{
					if (m_RetryReadOTPSites == null)
					{
						m_RetryReadOTPSites = new System.Windows.Forms.Timer();
						m_RetryReadOTPSites.Tick += OnRereadOTPSitesTimerTick;
						m_RetryReadOTPSites.Interval = 30 * 1000;
						m_RetryReadOTPSites.Enabled = false;
					}
					TriggerReadOTPSites();
					return TFAPossible.Error;
				}

				if (m_LoadState == TFALoadProcess.FileNotFound) return TFAPossible.Unknown;
				if (m_LoadState != TFALoadProcess.Loaded) return TFAPossible.Error;
				KeePassLib.Delegates.GFunc<string, string, bool> IsRegexMatch = (string sRegex, string sUrl) =>
				  {
					  System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(sRegex);
					  return r.IsMatch(sUrl);
				  };
                tfa = m_dTFA.FirstOrDefault(x => IsRegexMatch(x.Key, url)).Value;
				if (tfa == null)
				{
					tfa = new TFAData() { tfa = false };
					m_dTFA[url] = tfa;
				}
				else m_dTFA[url] = tfa;
				if (tfa.tfa) return TFAPossible.Yes;
				else return TFAPossible.No;
			}
		}

		private static void OnRereadOTPSitesTimerTick(object sender, EventArgs e)
		{
			lock (m_TFAReadLock)
			{
				System.Threading.ThreadPool.QueueUserWorkItem(RetryReadOTPSites);
			}
		}

		private static System.Windows.Forms.Timer m_RetryReadOTPSites = null;
		private static void TriggerReadOTPSites()
		{
			if (m_RetryReadOTPSites.Enabled) return;
			m_RetryReadOTPSites.Enabled = true;
		}

		private static void RetryReadOTPSites(object s)
		{
			ReadOTPSites(true);
			m_RetryReadOTPSites.Enabled = false;
		}

		private static void ReadOTPSites(object s)
		{
			lock (m_TFAReadLock)
			{
				if (m_LoadState == TFALoadProcess.Loading) return;
				if (m_LoadState == TFALoadProcess.Loaded) return;
				if (m_LoadState == TFALoadProcess.FileNotFound) return;
				m_LoadState = TFALoadProcess.Loading;
			}
			m_dTFA.Clear();
			JsonObject j = null;
			bool bException = false;
			IOConnectionInfo ioc = IOConnectionInfo.FromPath(TFA_JSON_FILE);
			bool bExists = false;
			bool bNoInternet = false;
			string sError = "File not found or no internet connection";
			try
			{
				bExists = IOConnection.FileExists(ioc, true);
				byte[] b = IOConnection.ReadFile(ioc);
				if (b != null) j = new JsonObject(new CharStream(StrUtil.Utf8.GetString(b)));
			}
			catch (System.Net.WebException exWeb)
			{
				sError = exWeb.Message;
				PluginDebug.AddError("Error reading OTP sites", 0, "Error: " + sError);
				bException = true;
				bNoInternet = exWeb.Response == null || exWeb.Status == System.Net.WebExceptionStatus.NameResolutionFailure;
			}
			catch (Exception ex)
			{
				sError = ex.Message;
				PluginDebug.AddError("Error reading OTP sites", 0, "Error: " + ex.Message);
				bException = true;
			}
			if (j == null)
			{
				if (!bExists || bNoInternet)
				{
					lock (m_TFAReadLock) { m_LoadState = bNoInternet ? TFALoadProcess.Error : TFALoadProcess.FileNotFound; }
					if (!(bool)s) Tools.ShowError("Error reading OTP sites: " + sError + "\n\n" + TFA_JSON_FILE);
				}
				else lock (m_TFAReadLock) { m_LoadState = TFALoadProcess.FileNotFound; }
				if (!bException) PluginDebug.AddError("Error reading OTP sites", 0);
				return;
			}
			DateTime dtStart = DateTime.Now;
			foreach (KeyValuePair<string, object> kvp in j.Items)
			{
				List<string> keys = (kvp.Value as JsonObject).Items.Keys.ToList();
				for (int i = 0; i < keys.Count; i++)
				{
					JsonObject k = (kvp.Value as JsonObject).GetValue<JsonObject>(keys[i]);
					TFAData tfa = new TFAData();
					var tfaDetails = k.GetValueArray<string>("tfa");
					tfa.tfa = tfaDetails != null && tfaDetails.Length > 0;
					if (!tfa.tfa) continue;
					tfa.name = k.GetValue<string>("name");
					tfa.sms = tfaDetails.Contains("sms") ||	k.GetValue<bool>("sms", false);
					tfa.email = tfaDetails.Contains("email") || k.GetValue<bool>("email", false);
					tfa.phone = tfaDetails.Contains("phone") || k.GetValue<bool>("phone", false);
					tfa.software = tfaDetails.Contains("software") || k.GetValue<bool>("software", false);
					tfa.hardware = tfaDetails.Contains("hardwar") || k.GetValue<bool>("hardware", false);
					tfa.url = k.GetValue<string>("url");
					tfa.img = k.GetValue<string>("img");
					tfa.doc = k.GetValue<string>("doc");
					tfa.category = kvp.Key;
					m_dTFA[CreatePattern(tfa.url)] = tfa;
				}
			}
			DateTime dtEnd = DateTime.Now;
			lock (m_TFAReadLock)
			{
				m_LoadState = TFALoadProcess.Loaded;
				PluginDebug.AddInfo("OTP sites read", 0, "Required time: " + (dtEnd - dtStart).ToString(), "Number of sites: " + m_dTFA.Count.ToString());
			}
		}

		private static string CreatePattern(string url)
		{
			//Don't use Uri class for performance reasons

			if (url.IndexOf(":") < 1) return string.Empty;

			string scheme;
			string domain = string.Empty;

			// (https)://(www.xyz.)(example).(com|.co.uk)<whatever>
			//string pattern = @"(.*)\:\/\/([^\/]+)"; //@"(.*)\:\/\/(.+\.|)(.*)\.(\w+)";

			Uri u = new Uri(url.ToLowerInvariant());
			scheme = u.Scheme;
			domain = u.Host;
			if (domain.StartsWith("www.")) domain = domain.Substring(4);
			domain = domain.Replace(".", @"\.");
			if ((scheme == "https") || (scheme == "ftps"))
				scheme += "?";
			else if ((scheme == "http") || (scheme == "ftp"))
				scheme += "s?";

			return "^" + scheme + @"://(.+\.|)" + domain;
		}
	}
}
