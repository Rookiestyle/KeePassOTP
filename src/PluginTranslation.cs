using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;

using KeePass.Plugins;
using KeePass.Util;
using KeePassLib.Utility;

using PluginTools;
using System.Windows.Forms;

namespace PluginTranslation
{
	public class TranslationChangedEventArgs: EventArgs
	{
		public string OldLanguageIso6391 = string.Empty;
		public string NewLanguageIso6391 = string.Empty;

		public TranslationChangedEventArgs(string OldLanguageIso6391, string NewLanguageIso6391)
		{
			this.OldLanguageIso6391 = OldLanguageIso6391;
			this.NewLanguageIso6391 = NewLanguageIso6391;
		}
	}

	public static class PluginTranslate
	{
		public static long TranslationVersion = 0;
		public static event EventHandler<TranslationChangedEventArgs> TranslationChanged = null;
		private static string LanguageIso6391 = string.Empty;
		#region Definitions of translated texts go here
		public const string PluginName = "KeePassOTP";
		public static readonly string OTPCopyTrayNoEntries = @"KPOTP - No entries available";
		public static readonly string OTPCopyTrayEntries = @"KPOTP auto-type ([Ctrl] to copy)";
		public static readonly string Empty = @"N/A Uuid: {0}";
		public static readonly string Group = @"group: {0}";
		public static readonly string OTPSetup = @"OTP setup...";
		public static readonly string OTPCopy = @"Copy OTP";
		public static readonly string Error = @"ERROR";
		public static readonly string Title = @"Title: {0}";
		public static readonly string User = @"User: {0}";
		public static readonly string SeedSettings = @"Seed settings";
		public static readonly string Seed = @"Seed: ";
		public static readonly string AdvancedOptions = @"Use advanced options";
		public static readonly string Format = @"Format: ";
		public static readonly string OTPSettings = @"OTP settings";
		public static readonly string OTPType = @"Type:";
		public static readonly string OTPLength = @"Length:";
		public static readonly string OTPHash = @"Hash:";
		public static readonly string OTPTimestep = @"Timestep:";
		public static readonly string OTPCounter = @"Counter:";
		public static readonly string TimeCorrection = @"Time correction - not applicable for HOTP";
		public static readonly string URL = @"URL: ";
		public static readonly string TimeDiff = @"Diff: ";
		public static readonly string TimeCorrectionOff = @"No time correction";
		public static readonly string TimeCorrectionEntry = @"Use entry's URL";
		public static readonly string TimeCorrectionFixed = @"Use fixed URL";
		public static readonly string SetupTFA = @"Setup 2FA";
		public static readonly string TFADefined = @"2FA defined";
		public static readonly string OTP_OpenDB = @"Open OTP database - {0}";
		public static readonly string OTP_CreateDB_PWHint = @"All secrets will be stored in a separate database which will be included within the main database containing the entries as such.

Please provide the masterkey for your OTP secrets in the next form.";
		public static readonly string Options_CheckTFA = @"Indicate if 2FA usage is possible";
		public static readonly string Options_OTPSettings = @"OTP settings (db specific)";
		public static readonly string Options_UseOTPDB = @"Save OTP secrets in a separate database";
		public static readonly string Options_PreloadOTPDB = @"Auto-open OTP database when main database is opened";
		public static readonly string Options_Migrate2DB = @"Entry -> DB";
		public static readonly string Options_Migrate2DBHint = @"Move OTP secrets to separate DB";
		public static readonly string Options_Migrate2Entries = @"DB -> Entry";
		public static readonly string Options_Migrate2EntriesHint = @"Move OTP secrets to respective entries";
		public static readonly string MoveError = @"Error when trying to move OTP secrets. Is the OTP database open?";
		public static readonly string EntriesMoved = @"{0} OTP secret(s) move auccessfully";
		public static readonly string Options_Check2FA_Help = @"KeePassOTP will download a list of sites supporting two factor authentication from {0}
This list is then compared it with all URLs defined in the database, your URLs will not be sent.

The OTP column will show whether 2FA is possible but not yet configured.
Doubleclicking this hint will open the 2FA setup page.";
		public static readonly string Options_Help = @">> {Options_UseOTPDB} <<
Stores new OTP secrets in a separate database that can be encrypted using a different masterkey.
This database will be stored within the main database containing the entries as such.
While this increases security by adding a second masterkey for your OTP secrets, it will potentially prevent other ports like KeePass2Android to directly show OTP.

>> {Options_PreloadOTPDB} <<
Open the OTP database when opening the main database.
Deactivate this option to open the OTP database only on demand => As soon as an OTP is actually requested
The OTP database will be opened everytime the main database is synchronized - otherwise changes of the OTP database would not get synchronized and local changes would get lost.

>> OTP migration <<
Depening on above options, OTP secrets are stored either within the respective entry or within a separate database.
'{Options_Migrate2DB}' and '{Options_Migrate2Entries}' can be used to move the OTP secrets between those two locations.";
		public static readonly string MigrateOtherPlugins = @"Migrate from / to other plugins";
		public static readonly string MigrateOtherPlugins_Delete = @"Remove {0} settings for successfully migrated entries?";
		public static readonly string MigrateOtherPlugins_Result = @"{0} out of {1} entries were migrated successfully";
		public static readonly string Hotkey = @"KeePassOTP hotkey:";
		public static readonly string HintSyncRequiresUnlock = @"The database was synchronized.
Please unlock the OTP database to ensure proper synchronization of OTP data.

If the OTP database is not unlocked, OTP data cannot be synchronized and might result in data loss.";
		public static readonly string OTPDB_Opening = @"Opening OTP database...";
		public static readonly string OTPDB_Sync = @"Synchronizing OTP database...";
		public static readonly string OTPDB_Save = @"Saving OTP database...";
		public static readonly string OTPQRCode = @"Show QR code...";
		public static readonly string MigratePlaceholder = @"The KeePassOTP placeholder was changed and the previous placeholder is no longer valid.

Old value: {0}
New value: {1}

All occurrences of {0} need to be replaced by {1} for Auto-Type to work.
Replace now in currently loaded databases?";
		public static readonly string ConfirmOTPDBDelete = @"This will deactivate the KeePassOTP database. 
It will NOT delete the KeePassOTP database.

Click '{0}' to deactivate AND delete KeePassOTP database.
Click '{1}' to deactivate but not delete KeePassOTP database.";
		public static readonly string OTPBackupDone = @"Unlocking the existing OTP database failed and its content has been overwritten.
A backup was saved as attachment in the following entry:
{0}

Open it anytime to check the previous content.
Restoring the data needs to be done manually.";
		public static readonly string Placeholder = @"Placeholder:";
		public static readonly string PlaceholderAutoSubmit = @"{0} + Enter";
		public static readonly string ErrorGoogleAuthImport = @"Error parsing data";
		public static readonly string ErrorGoogleAuthImportCount = @"Error parsing data

Expected amount of OTP entries: 1
Found amount of OTP entries: {0}";
		public static readonly string SelectSingleEntry = @"Please check the single entry to use";
		public static readonly string Issuer = @"Issuer";
		public static readonly string CheckingTFA = @"Checking 2FA";
		public static readonly string ReadScreenForQRCode = @"Read OTP QR code from screen";
		public static readonly string ReadScreenForQRCodeExplain = @"KeePassOTP will now try to find and read the OTP QR code from your screen.
KeePass itself will drop to background for max {0} seconds to simplify reading the OTP QR code.

This message will not be shown again.";
		public static readonly string OTP_Setup_DragDrop = @"Drag & drop a valid OTP QR code";
		public static readonly string OTPRenewal = @"Renew copied OTP";
		public static readonly string OTPRenewal_Inactive = @"Never";
		public static readonly string OTPRenewal_RespectClipboardTimeout = @"Renew until clipboard gets cleared";
		public static readonly string OTPRenewal_PreventShortDuration = @"Renew if soon to expire";
		#endregion

		#region NO changes in this area
		private static StringDictionary m_translation = new StringDictionary();

		public static void Init(Plugin plugin, string LanguageCodeIso6391)
		{
			List<string> lDebugStrings = new List<string>();
			m_translation.Clear();
			bool bError = true;
			LanguageCodeIso6391 = InitTranslation(plugin, lDebugStrings, LanguageCodeIso6391, out bError);
			if (bError && (LanguageCodeIso6391.Length > 2))
			{
				LanguageCodeIso6391 = LanguageCodeIso6391.Substring(0, 2);
				lDebugStrings.Add("Trying fallback: " + LanguageCodeIso6391);
				LanguageCodeIso6391 = InitTranslation(plugin, lDebugStrings, LanguageCodeIso6391, out bError);
			}
			if (bError)
			{
				PluginDebug.AddError("Reading translation failed", 0, lDebugStrings.ToArray());
				LanguageCodeIso6391 = "en";
			}
			else
			{
				List<FieldInfo> lTranslatable = new List<FieldInfo>(
					typeof(PluginTranslate).GetFields(BindingFlags.Static | BindingFlags.Public)
					).FindAll(x => x.IsInitOnly);
				lDebugStrings.Add("Parsing complete");
				lDebugStrings.Add("Translated texts read: " + m_translation.Count.ToString());
				lDebugStrings.Add("Translatable texts: " + lTranslatable.Count.ToString());
				foreach (FieldInfo f in lTranslatable)
				{
					if (m_translation.ContainsKey(f.Name))
					{
						lDebugStrings.Add("Key found: " + f.Name);
						f.SetValue(null, m_translation[f.Name]);
					}
					else
						lDebugStrings.Add("Key not found: " + f.Name);
				}
				PluginDebug.AddInfo("Reading translations finished", 0, lDebugStrings.ToArray());
			}
			if (TranslationChanged != null)
			{
				TranslationChanged(null, new TranslationChangedEventArgs(LanguageIso6391, LanguageCodeIso6391));
			}
			LanguageIso6391 = LanguageCodeIso6391;
			lDebugStrings.Clear();
		}

		private static string InitTranslation(Plugin plugin, List<string> lDebugStrings, string LanguageCodeIso6391, out bool bError)
		{
			if (string.IsNullOrEmpty(LanguageCodeIso6391))
			{
				lDebugStrings.Add("No language identifier supplied, using 'en' as fallback");
				LanguageCodeIso6391 = "en";
			}
			string filename = GetFilename(plugin.GetType().Namespace, LanguageCodeIso6391);
			lDebugStrings.Add("Translation file: " + filename);

			if (!File.Exists(filename)) //If e. g. 'plugin.zh-tw.language.xml' does not exist, try 'plugin.zh.language.xml'
			{
				lDebugStrings.Add("File does not exist");
				bError = true;
				return LanguageCodeIso6391;
			}
			else
			{
				string translation = string.Empty;
				try { translation = File.ReadAllText(filename); }
				catch (Exception ex)
				{
					lDebugStrings.Add("Error reading file: " + ex.Message);
					LanguageCodeIso6391 = "en";
					bError = true;
					return LanguageCodeIso6391;
				}
				XmlSerializer xs = new XmlSerializer(m_translation.GetType());
				lDebugStrings.Add("File read, parsing content");
				try
				{
					m_translation = (StringDictionary)xs.Deserialize(new StringReader(translation));
				}
				catch (Exception ex)
				{
					string sException = ex.Message;
					if (ex.InnerException != null) sException += "\n" + ex.InnerException.Message;
					lDebugStrings.Add("Error parsing file: " + sException);
					LanguageCodeIso6391 = "en";
					MessageBox.Show("Error parsing translation file\n\n" + sException, PluginName, MessageBoxButtons.OK, MessageBoxIcon.Error);
					bError = true;
					return LanguageCodeIso6391;
				}
				bError = false;
				return LanguageCodeIso6391;
			}
		}

		private static string GetFilename(string plugin, string lang)
		{
			string filename = UrlUtil.GetFileDirectory(WinUtil.GetExecutable(), true, true);
				filename += KeePass.App.AppDefs.PluginsDir + UrlUtil.LocalDirSepChar + "Translations" + UrlUtil.LocalDirSepChar;
				filename += plugin + "." + lang + ".language.xml";
			return filename;
		}
		#endregion
	}

	#region NO changes in this area
	[XmlRoot("Translation")]
	public class StringDictionary : Dictionary<string, string>, IXmlSerializable
	{
		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			bool wasEmpty = reader.IsEmptyElement;
			reader.Read();
			if (wasEmpty) return;
			bool bFirst = true;
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				if (bFirst)
				{
					bFirst = false;
					try
					{
						reader.ReadStartElement("TranslationVersion");
						PluginTranslate.TranslationVersion = reader.ReadContentAsLong();
						reader.ReadEndElement();
					}
					catch { }
				}
				reader.ReadStartElement("item");
				reader.ReadStartElement("key");
				string key = reader.ReadContentAsString();
				reader.ReadEndElement();
				reader.ReadStartElement("value");
				string value = reader.ReadContentAsString();
				reader.ReadEndElement();
				this.Add(key, value);
				reader.ReadEndElement();
				reader.MoveToContent();
			}
			reader.ReadEndElement();
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement("TranslationVersion");
			writer.WriteString(PluginTranslate.TranslationVersion.ToString());
			writer.WriteEndElement();
			foreach (string key in this.Keys)
			{
				writer.WriteStartElement("item");
				writer.WriteStartElement("key");
				writer.WriteString(key);
				writer.WriteEndElement();
				writer.WriteStartElement("value");
				writer.WriteString(this[key]);
				writer.WriteEndElement();
				writer.WriteEndElement();
			}
		}
	}
	#endregion
}
