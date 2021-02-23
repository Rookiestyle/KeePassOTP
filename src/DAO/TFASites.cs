﻿using KeePass.DataExchange;
using KeePassLib.Serialization;
using KeePassLib.Utility;
using PluginTools;
using System;
using System.Collections.Generic;
using System.Linq;

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
			No
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
			if (!Config.CheckTFA) return;
			if ((m_LoadState == TFALoadProcess.Loaded) && !bReload) return;
			if (bReload)
			{
				lock (m_TFAReadLock)
				{
					m_LoadState = TFALoadProcess.NotStarted;

				}
			}
			System.Threading.ThreadPool.QueueUserWorkItem(ReadOTPSites);
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
				if (m_LoadState != TFALoadProcess.Loaded) return TFAPossible.Unknown;
				string urlpattern = CreatePattern(url);
				if (!m_dTFA.TryGetValue(urlpattern, out tfa))
				{
					tfa = new TFAData() { tfa = false };
					m_dTFA[urlpattern] = tfa;
					m_dTFA[url] = tfa;
				}
				else m_dTFA[url] = tfa;
				if (tfa.tfa) return TFAPossible.Yes;
				else return TFAPossible.No;
			}
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
			try
			{
				byte[] b = IOConnection.ReadFile(ioc);
				if (b != null) j = new JsonObject(new CharStream(StrUtil.Utf8.GetString(b)));
			}
			catch (Exception ex)
			{
				PluginDebug.AddError("Error reading OTP sites", 0, "Error: " + ex.Message);
				bException = true;
			}
			if (j == null)
			{
				if (!IOConnection.FileExists(ioc))
				{
					lock (m_TFAReadLock) { m_LoadState = TFALoadProcess.FileNotFound; }
					Tools.ShowError("Error reading OTP sites: File does not exist\n\n" + TFA_JSON_FILE);
				}
				else lock (m_TFAReadLock) { m_LoadState = TFALoadProcess.Error; }
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

			string scheme = string.Empty;
			string domain = string.Empty;
			string tld = string.Empty;
			int pos = -1;

			// (https)://(www.xyz.)(example).(com)<whatever>
			string pattern = @"(.*)\:\/\/(.+\.|)(.*)\.(\w+)";
			url = url.ToLowerInvariant();
			System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(pattern);
			System.Text.RegularExpressions.Match m = r.Match(url);

			if (m.Groups.Count == 5) //4 matches + overall pattern
			{
				scheme = m.Groups[1].Value;
				domain = m.Groups[3].Value;
				tld = m.Groups[4].Value;
			}
			else
			{
				string[] parts = url.Trim().ToLowerInvariant().Split('.');
				if (parts.Length < 2) return string.Empty;

				pos = parts[0].IndexOf(":");
				if (pos < 0) return string.Empty;
				scheme = parts[0].Substring(0, pos);
				if (parts[0].Length < (pos + 3))
					return string.Empty;
				parts[0] = parts[0].Substring(pos + 3); // scheme://DOMAIN

				int i = parts.Length - 1;
				domain = parts[i - 1] + @"\." + parts[i];
			}
			if ((scheme == "http") || (scheme == "ftp"))
				scheme += "s?";

			return scheme + @"://(.+\.|)" + domain + (string.IsNullOrEmpty(tld) ? string.Empty : @"\." + tld);
		}
	}
}
