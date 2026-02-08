using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
    const string TFA_JSON_FILE_DEFAULT = "https://api.2fa.directory/v4/all.json";

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
      public string url;
      public List<string> tfa = new List<string>();
      public string documentation;
      public string recovery;
      public string notes;
      public List<string> custom_software;
      public List<string> custom_hardware;
      public bool tfa_possible { get { return tfa.Count > 0; } }
      public Regex RegexUrl = null;

      public override string ToString()
      {
        if (domain != null && url != null) return "D: " + domain + " | U: " + url;
        if (domain != null) return "D: " + domain;
        if (url != null) return "U: " + url;
        return base.ToString();
      }
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
      ReadOTPSites(s);
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
      List<string> lTFAEntries = new List<string>();
      bool bException = false;
      bool bErrorNoExists = false;
      bool bErrorNoInternet = false;
      bool bOutdated = false;
      List<string> lMsg = new List<string>();
      string sContent = GetTFAFileLocal(out bOutdated, lMsg);
      string sError = string.Empty;
      if (string.IsNullOrEmpty(sContent) || bOutdated)
        sContent = GetTFAFileRemote(TFA_JSON_FILE, sContent, out sError, out bErrorNoExists, out bErrorNoInternet, lMsg);

      JsonObject jsonTFA = null;
      try
      {
        jsonTFA = new JsonObject(new CharStream(sContent));
      }
      catch (Exception ex) 
      {
        jsonTFA = null;
        if (!string.IsNullOrEmpty(sContent) && !bOutdated)
        {
          sContent = GetTFAFileRemote(TFA_JSON_FILE, sContent, out sError, out bErrorNoExists, out bErrorNoInternet, lMsg);
          try { jsonTFA = new JsonObject(new CharStream(sContent)); } catch { jsonTFA = null; }
        }
      }
      if (jsonTFA == null || jsonTFA.Items.Count < 1)
      {
        if (bErrorNoExists || bErrorNoInternet)
        {
          lock (m_TFAReadLock) { m_LoadState = bErrorNoInternet ? TFALoadProcess.Error : TFALoadProcess.FileNotFound; }
          if (!(bool)s) Tools.ShowError("Error reading OTP sites: " + sError + "\n\n" + TFA_JSON_FILE);
        }
        else lock (m_TFAReadLock) { m_LoadState = TFALoadProcess.FileNotFound; }
        PluginDebug.AddError("Error reading OTP sites", 0, lMsg.ToArray());
        return;
      }
      DateTime dtStart = DateTime.Now;
      foreach (var item in jsonTFA.Items)
      {
        try
        {
          var tfa = new TFAData();
          JsonObject o = item.Value as JsonObject;
          tfa.domain = item.Key;
          if (!tfa.domain.StartsWith("http://") && !tfa.domain.StartsWith("https://")) tfa.domain = "https://" + tfa.domain;
          tfa.documentation = GetJsonString(o, "documentation");
          tfa.tfa = GetJsonStringList(o, "methods");// o.GetValue<string[]>("method").ToList();
          if (tfa.tfa.Count == 0) continue;

          tfa.url = GetJsonString(o, "url");
          if (string.IsNullOrEmpty(tfa.url)) tfa.url = tfa.domain;
          tfa.recovery = GetJsonString(o, "recovery");
          tfa.notes = GetJsonString(o, "notes");
          tfa.custom_software = GetJsonStringList(o, "custom-software");
          tfa.custom_hardware = GetJsonStringList(o, "custom-hardware");
          string sPattern = CreatePattern(tfa.domain);
          tfa.RegexUrl = new Regex(sPattern);

          m_dTFA[sPattern] = tfa;
        }
        catch (Exception ex) { }
      }
      DateTime dtEnd = DateTime.Now;
      lock (m_TFAReadLock)
      {
        m_LoadState = TFALoadProcess.Loaded;
        lMsg.Add("Required time: " + (dtEnd - dtStart).ToString());
        lMsg.Add("Number of sites: " + m_dTFA.Count.ToString());
        PluginDebug.AddInfo("OTP sites read", 0, lMsg.ToArray());
      }
    }

    private static string GetJsonString(JsonObject o, string sField)
    {
      if (o == null) return string.Empty;
      var s = o.GetValue<string>(sField);
      return string.IsNullOrEmpty(s) ? string.Empty : s;
    }
    private static List<string> GetJsonStringList(JsonObject o, string sField)
    {
      if (o == null) return new List<string>();
      var a = o.GetValueArray<string>(sField);
      if (a == null) return new List<string>();
      return a.ToList();
    }

    private static string _sLocalFile = string.Empty;
    static TFASites()
    {
      _sLocalFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeePassOTP", "tfa_sites.json"); ;
    }

    private static string GetTFAFileLocal(out bool bOutdated, List<string> lMsg)
    {
      bOutdated = false;
      if (Config.MaxAgeLocalFile == Config.DONT_USE_LOCAL_FILE)
      {
        lMsg.Add("Local file disabled");
        return string.Empty;
      }
      lMsg.Add("Check local file: " + _sLocalFile);
      if (!File.Exists(_sLocalFile))
      {
        lMsg.Add("Local file does not exist");
        return string.Empty;
      }
      var dtLastWrite = File.GetLastWriteTimeUtc(_sLocalFile);
      lMsg.Add("Local file exists, last change (UTC): " + dtLastWrite);
      var tsAge = DateTime.UtcNow - dtLastWrite;
      if (Config.MaxAgeLocalFile == Config.ALWAYS_USE_LOCAL_FILE)
      {
        lMsg.Add("Local file must be used");
      }
      else
      {
        bOutdated = tsAge.TotalDays >= 2;
        lMsg.Add("File usable: " + (bOutdated ? "No, outdated" : "Yes"));
      }
      return File.ReadAllText(_sLocalFile);
    }

    private static string GetTFAFileRemote(string sFile, string sLocalContent, out string sError, out bool bErrorNoExists, out bool bErrorNoInternet, List<string> lMsg)
    {
      IOConnectionInfo ioc = IOConnectionInfo.FromPath(sFile);
      bErrorNoExists = false;
      bErrorNoInternet = false;
      sError = "File not found or no internet connection";
      string sContent = string.Empty;
      lMsg.Add("Read remote tfa file: " + sFile);
      try
      {
        bErrorNoInternet = !IOConnection.FileExists(ioc, true);
        byte[] b = IOConnection.ReadFile(ioc);
        if (b != null)
        {
          sContent = StrUtil.Utf8.GetString(b);
        }
      }
      catch (Exception ex)
      {
        sError = ex.Message;
        lMsg.Add("Error: " + sError);
        if (ex is WebException)
          bErrorNoInternet = (ex as WebException).Response == null || (ex as WebException).Status == WebExceptionStatus.NameResolutionFailure;
      }
      if (!string.IsNullOrEmpty(sContent)) 
      {
        Directory.CreateDirectory(UrlUtil.GetFileDirectory(_sLocalFile, false, true));
        File.WriteAllText(_sLocalFile, sContent);
        lMsg.Add("Updated local file");
        return sContent;
      }
      return sLocalContent;
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
