﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using KeePass.DataExchange;
using KeePass.Resources;
using KeePassLib.Serialization;
using KeePassLib.Utility;
using PluginTools;
using PluginTranslation;

namespace KeePassOTP
{
  public static class TFASites
  {
    //const string TFA_JSON_FILE = "https://twofactorauth.org/api/v2/tfa.json";
    const string TFA_JSON_FILE_DEFAULT = "https://api.2fa.directory/v3/tfa.json";

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

    internal class TFAData
    {
      public string domain;
      public string img;
      public string url;
      public List<string> tfa = new List<string>();
      public string documentation;
      public string recovery;
      public string notes;
      public string contact;
      public List<string> regions;
      public List<string> additional_domains;
      public List<string> custom_software;
      public List<string> custom_hardware;
      public List<string> keywords;
      public bool tfa_possible { get { return tfa.Count > 0; } }
      public Regex RegexUrl = null;
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
      return !string.IsNullOrEmpty(tfa.documentation) ? tfa.documentation : url;
    }

    internal static TFAData GetTFAData(string url)
    {
      if (IsTFAPossible(url) != TFAPossible.Yes) return null;
      TFAData tfa;
      if (m_dTFA.TryGetValue(url, out tfa)) return tfa;
      return null;
    }

    public static TFAPossible IsTFAPossible(string url)
    {
      if (!Config.CheckTFA) return TFAPossible.No;
      TFAData tfa;
      if (m_dTFA.TryGetValue(url, out tfa))
      {
        if (tfa.tfa_possible) return TFAPossible.Yes;
        else return TFAPossible.No;
      }
      lock (m_TFAReadLock)
      {
        if (m_LoadState == TFALoadProcess.Error)
        {
          if (m_RetryReadOTPSites == null)
          {
            m_RetryReadOTPSites = new Timer();
            m_RetryReadOTPSites.Tick += OnRereadOTPSitesTimerTick;
            m_RetryReadOTPSites.Interval = 30 * 1000;
            m_RetryReadOTPSites.Enabled = false;
          }
          TriggerReadOTPSites();
          return TFAPossible.Error;
        }

        if (m_LoadState == TFALoadProcess.FileNotFound) return TFAPossible.Unknown;
        if (m_LoadState != TFALoadProcess.Loaded) return TFAPossible.Error;
        KeePassLib.Delegates.GFunc<Regex, string, bool> IsRegexMatch = (Regex r, string sUrl) =>
        {
          try
          {
            return r != null ? r.IsMatch(sUrl) : false;
          }
          catch (Exception ex)
          {
            PluginDebug.AddError("Error in URL regex matching", 0, "Error: " + ex.Message, "Regex: " + r.ToString());
            return false;
          }
        };
        tfa = m_dTFA.FirstOrDefault(x => IsRegexMatch(x.Value.RegexUrl, url)).Value;
        if (tfa == null)
        {
          tfa = new TFAData();
          m_dTFA[url] = tfa;
        }
        else m_dTFA[url] = tfa;
        if (tfa.tfa_possible) return TFAPossible.Yes;
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

    private static Timer m_RetryReadOTPSites = null;
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

    private static string GetJSonString(string sSearch, string v)
    {
      //Case 1: Simple string
      Regex r = new Regex(@"""" + v + @""":""(.*?)""");
      var aMatches = r.Matches(sSearch);
      if (aMatches != null && aMatches.Count > 0)
      {
        return aMatches[0].Groups[1].Value;
      }

      //Case 2: Multiple values
      r = new Regex(@"""" + v + @""":\[(.*?)\]");
      aMatches = r.Matches(sSearch);
      if (aMatches == null) return string.Empty;
      if (aMatches.Count < 1) return string.Empty;
      return aMatches[0].Groups[1].Value;
    }

    private static List<string> GetJSonList(string sSearch, string v)
    {
      string s = GetJSonString(sSearch, v);
      if (string.IsNullOrEmpty(s)) return new List<string>();

      return s.Split(new string[] { "\",\"", "\"" }, StringSplitOptions.RemoveEmptyEntries).ToList();
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
      List<string> lTFAEntries = new List<string>();
      bool bException = false;
      IOConnectionInfo ioc = IOConnectionInfo.FromPath(TFA_JSON_FILE);
      bool bExists = false;
      bool bNoInternet = false;
      string sError = "File not found or no internet connection";
      try
      {
        bExists = IOConnection.FileExists(ioc, true);
        byte[] b = IOConnection.ReadFile(ioc);
        if (b != null)
        {
          string content = StrUtil.Utf8.GetString(b);
          lTFAEntries = ParseJSON(content);
        }
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
      if (lTFAEntries == null || lTFAEntries.Count < 1)
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
      Dictionary<string, TFAData> dTFAEntries = new Dictionary<string, TFAData>();
      foreach (string tfaentry in lTFAEntries)
      {
        try
        {
          TFAData tfa = new TFAData();
          tfa.domain = GetJSonString(tfaentry, "domain");
          string sDomain = tfa.domain.ToLowerInvariant();
          if (!sDomain.StartsWith("http://") && !sDomain.StartsWith("https://")) tfa.domain = "https://" + tfa.domain;

          tfa.img = GetJSonString(tfaentry, "img");
          tfa.url = GetJSonString(tfaentry, "url");
          if (string.IsNullOrEmpty(tfa.url)) tfa.url = tfa.domain;
          tfa.tfa = GetJSonList(tfaentry, "tfa");
          tfa.documentation = GetJSonString(tfaentry, "documentation");
          tfa.recovery = GetJSonString(tfaentry, "recovery");
          tfa.notes = GetJSonString(tfaentry, "notes");
          tfa.contact = GetJSonString(tfaentry, "contact");
          tfa.regions = GetJSonList(tfaentry, "regions");
          tfa.additional_domains = GetJSonList(tfaentry, "additional_domains");
          tfa.custom_software = GetJSonList(tfaentry, "custom_software");
          tfa.custom_hardware = GetJSonList(tfaentry, "custom_hardware");
          tfa.keywords = GetJSonList(tfaentry, "keywords");
          string sRegexPattern = CreatePattern(tfa.domain);
          tfa.RegexUrl = new Regex(sRegexPattern);
          m_dTFA[sRegexPattern] = tfa;
        }
        catch (Exception exAll)
        {
          PluginDebug.AddError("Error reading OTP sites", 0, exAll.Message, tfaentry);
        }
      }
      DateTime dtEnd = DateTime.Now;
      lock (m_TFAReadLock)
      {
        m_LoadState = TFALoadProcess.Loaded;
        PluginDebug.AddInfo("OTP sites read", 0, "Required time: " + (dtEnd - dtStart).ToString(), "Number of sites: " + m_dTFA.Count.ToString());
      }
    }

    private static List<string> ParseJSON(string content)
    {
      bool bRepeat = true;
      MatchCollection aMatches = null;

      while (bRepeat)
      {
        try
        {
          Regex r = new Regex(@"\[(.*?)\](?=(,\[|\]))", RegexOptions.Singleline);
          aMatches = r.Matches(content);
          bRepeat = false;
        }
        catch (Exception ex) {
          PluginDebug.AddError("Error in ParseJSON", 0, ex.Message);
        }
      }
      List<string> lResult = new List<string>();

      lResult = aMatches.Cast<Match>().Select(m => m.Value).ToList();

      return lResult;
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
