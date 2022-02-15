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
	public class TranslationChangedEventArgs : EventArgs
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
		/// <summary>
		/// KPOTP - No entries available
		/// </summary>
		public static readonly string OTPCopyTrayNoEntries = @"KPOTP - No entries available";
		/// <summary>
		/// KPOTP auto-type ([Ctrl] to copy)
		/// </summary>
		public static readonly string OTPCopyTrayEntries = @"KPOTP auto-type ([Ctrl] to copy)";
		/// <summary>
		/// N/A Uuid: {0}
		/// </summary>
		public static readonly string Empty = @"N/A Uuid: {0}";
		/// <summary>
		/// group: {0}
		/// </summary>
		public static readonly string Group = @"group: {0}";
		/// <summary>
		/// OTP setup...
		/// </summary>
		public static readonly string OTPSetup = @"OTP setup...";
		/// <summary>
		/// Copy OTP
		/// </summary>
		public static readonly string OTPCopy = @"Copy OTP";
		/// <summary>
		/// ERROR
		/// </summary>
		public static readonly string Error = @"ERROR";
		/// <summary>
		/// Title: {0}
		/// </summary>
		public static readonly string Title = @"Title: {0}";
		/// <summary>
		/// User: {0}
		/// </summary>
		public static readonly string User = @"User: {0}";
		/// <summary>
		/// Seed settings
		/// </summary>
		public static readonly string SeedSettings = @"Seed settings";
		/// <summary>
		/// Seed: 
		/// </summary>
		public static readonly string Seed = @"Seed: ";
		/// <summary>
		/// Use advanced options
		/// </summary>
		public static readonly string AdvancedOptions = @"Use advanced options";
		/// <summary>
		/// Format: 
		/// </summary>
		public static readonly string Format = @"Format: ";
		/// <summary>
		/// OTP settings
		/// </summary>
		public static readonly string OTPSettings = @"OTP settings";
		/// <summary>
		/// Type:
		/// </summary>
		public static readonly string OTPType = @"Type:";
		/// <summary>
		/// Length:
		/// </summary>
		public static readonly string OTPLength = @"Length:";
		/// <summary>
		/// Hash:
		/// </summary>
		public static readonly string OTPHash = @"Hash:";
		/// <summary>
		/// Timestep:
		/// </summary>
		public static readonly string OTPTimestep = @"Timestep:";
		/// <summary>
		/// Counter:
		/// </summary>
		public static readonly string OTPCounter = @"Counter:";
		/// <summary>
		/// Time correction - not applicable for HOTP
		/// </summary>
		public static readonly string TimeCorrection = @"Time correction - not applicable for HOTP";
		/// <summary>
		/// URL: 
		/// </summary>
		public static readonly string URL = @"URL: ";
		/// <summary>
		/// Diff: 
		/// </summary>
		public static readonly string TimeDiff = @"Diff: ";
		/// <summary>
		/// No time correction
		/// </summary>
		public static readonly string TimeCorrectionOff = @"No time correction";
		/// <summary>
		/// Use entry's URL
		/// </summary>
		public static readonly string TimeCorrectionEntry = @"Use entry's URL";
		/// <summary>
		/// Use fixed URL
		/// </summary>
		public static readonly string TimeCorrectionFixed = @"Use fixed URL";
		/// <summary>
		/// Setup 2FA
		/// </summary>
		public static readonly string SetupTFA = @"Setup 2FA";
		/// <summary>
		/// 2FA defined
		/// </summary>
		public static readonly string TFADefined = @"2FA defined";
		/// <summary>
		/// Open OTP database - {0}
		/// </summary>
		public static readonly string OTP_OpenDB = @"Open OTP database - {0}";
		/// <summary>
		/// All secrets will be stored in a separate database which will be included within the main database containing the entries as such.
		/// 
		/// Please provide the masterkey for your OTP secrets in the next form.
		/// </summary>
		public static readonly string OTP_CreateDB_PWHint = @"All secrets will be stored in a separate database which will be included within the main database containing the entries as such.

Please provide the masterkey for your OTP secrets in the next form.";
		/// <summary>
		/// Click '{0}' to continue or click '{1}' to store OTP secrets within the respective entries.
		/// </summary>
		public static readonly string OTP_CreateDB_Question_Addendum = @"Click '{0}' to continue or click '{1}' to store OTP secrets within the respective entries.";
		/// <summary>
		/// Indicate if 2FA usage is possible
		/// </summary>
		public static readonly string Options_CheckTFA = @"Indicate if 2FA usage is possible";
		/// <summary>
		/// OTP settings (db specific)
		/// </summary>
		public static readonly string Options_OTPSettings = @"OTP settings (db specific)";
		/// <summary>
		/// Save OTP secrets in a separate database
		/// </summary>
		public static readonly string Options_UseOTPDB = @"Save OTP secrets in a separate database";
		/// <summary>
		/// Auto-open OTP database when main database is opened
		/// </summary>
		public static readonly string Options_PreloadOTPDB = @"Auto-open OTP database when main database is opened";
		/// <summary>
		/// Entry -> DB
		/// </summary>
		public static readonly string Options_Migrate2DB = @"Entry -> DB";
		/// <summary>
		/// Move OTP secrets to separate DB
		/// </summary>
		public static readonly string Options_Migrate2DBHint = @"Move OTP secrets to separate DB";
		/// <summary>
		/// DB -> Entry
		/// </summary>
		public static readonly string Options_Migrate2Entries = @"DB -> Entry";
		/// <summary>
		/// Move OTP secrets to respective entries
		/// </summary>
		public static readonly string Options_Migrate2EntriesHint = @"Move OTP secrets to respective entries";
		/// <summary>
		/// Error when trying to move OTP secrets. Is the OTP database open?
		/// </summary>
		public static readonly string MoveError = @"Error when trying to move OTP secrets. Is the OTP database open?";
		/// <summary>
		/// {0} OTP secret(s) move auccessfully
		/// </summary>
		public static readonly string EntriesMoved = @"{0} OTP secret(s) move auccessfully";
		/// <summary>
		/// KeePassOTP will download a list of sites supporting two factor authentication from {0}
		/// This list is then compared it with all URLs defined in the database, your URLs will not be sent.
		/// 
		/// The OTP column will show whether 2FA is possible but not yet configured.
		/// Doubleclicking this hint will open the 2FA setup page.
		/// </summary>
		public static readonly string Options_Check2FA_Help = @"KeePassOTP will download a list of sites supporting two factor authentication from {0}
This list is then compared it with all URLs defined in the database, your URLs will not be sent.

The OTP column will show whether 2FA is possible but not yet configured.
Doubleclicking this hint will open the 2FA setup page.";
		/// <summary>
		/// >> {Options_UseOTPDB} <<
		/// Stores new OTP secrets in a separate database that can be encrypted using a different masterkey.
		/// This database will be stored within the main database containing the entries as such.
		/// While this increases security by adding a second masterkey for your OTP secrets, it will potentially prevent other ports like KeePass2Android to directly show OTP.
		/// 
		/// >> {Options_PreloadOTPDB} <<
		/// Open the OTP database when opening the main database.
		/// Deactivate this option to open the OTP database only on demand => As soon as an OTP is actually requested
		/// The OTP database will be opened everytime the main database is synchronized - otherwise changes of the OTP database would not get synchronized and local changes would get lost.
		/// 
		/// >> OTP migration <<
		/// Depening on above options, OTP secrets are stored either within the respective entry or within a separate database.
		/// '{Options_Migrate2DB}' and '{Options_Migrate2Entries}' can be used to move the OTP secrets between those two locations.
		/// </summary>
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
		/// <summary>
		/// Migrate from / to other plugins
		/// </summary>
		public static readonly string MigrateOtherPlugins = @"Migrate from / to other plugins";
		/// <summary>
		/// Remove {0} settings for successfully migrated entries?
		/// </summary>
		public static readonly string MigrateOtherPlugins_Delete = @"Remove {0} settings for successfully migrated entries?";
		/// <summary>
		/// {0} out of {1} entries were migrated successfully
		/// </summary>
		public static readonly string MigrateOtherPlugins_Result = @"{0} out of {1} entries were migrated successfully";
		/// <summary>
		/// KeePassOTP hotkey:
		/// </summary>
		public static readonly string Hotkey = @"KeePassOTP hotkey:";
		/// <summary>
		/// The database was synchronized.
		/// Please unlock the OTP database to ensure proper synchronization of OTP data.
		/// 
		/// If the OTP database is not unlocked, OTP data cannot be synchronized and might result in data loss.
		/// </summary>
		public static readonly string HintSyncRequiresUnlock = @"The database was synchronized.
Please unlock the OTP database to ensure proper synchronization of OTP data.

If the OTP database is not unlocked, OTP data cannot be synchronized and might result in data loss.";
		/// <summary>
		/// Opening OTP database...
		/// </summary>
		public static readonly string OTPDB_Opening = @"Opening OTP database...";
		/// <summary>
		/// Synchronizing OTP database...
		/// </summary>
		public static readonly string OTPDB_Sync = @"Synchronizing OTP database...";
		/// <summary>
		/// Saving OTP database...
		/// </summary>
		public static readonly string OTPDB_Save = @"Saving OTP database...";
		/// <summary>
		/// Show QR code...
		/// </summary>
		public static readonly string OTPQRCode = @"Show QR code...";
		/// <summary>
		/// The KeePassOTP placeholder was changed and the previous placeholder is no longer valid.
		/// 
		/// Old value: {0}
		/// New value: {1}
		/// 
		/// All occurrences of {0} need to be replaced by {1} for Auto-Type to work.
		/// Replace now in currently loaded databases?
		/// </summary>
		public static readonly string MigratePlaceholder = @"The KeePassOTP placeholder was changed and the previous placeholder is no longer valid.

Old value: {0}
New value: {1}

All occurrences of {0} need to be replaced by {1} for Auto-Type to work.
Replace now in currently loaded databases?";
		/// <summary>
		/// This will deactivate the KeePassOTP database. 
		/// It will NOT delete the KeePassOTP database.
		/// 
		/// Click '{0}' to deactivate AND delete KeePassOTP database.
		/// Click '{1}' to deactivate but not delete KeePassOTP database.
		/// </summary>
		public static readonly string ConfirmOTPDBDelete = @"This will deactivate the KeePassOTP database. 
It will NOT delete the KeePassOTP database.

Click '{0}' to deactivate AND delete KeePassOTP database.
Click '{1}' to deactivate but not delete KeePassOTP database.";
		/// <summary>
		/// Unlocking the existing OTP database failed and its content has been overwritten.
		/// A backup was saved as attachment in the following entry:
		/// {0}
		/// 
		/// Open it anytime to check the previous content.
		/// Restoring the data needs to be done manually.
		/// </summary>
		public static readonly string OTPBackupDone = @"Unlocking the existing OTP database failed and its content has been overwritten.
A backup was saved as attachment in the following entry:
{0}

Open it anytime to check the previous content.
Restoring the data needs to be done manually.";
		/// <summary>
		/// Placeholder:
		/// </summary>
		public static readonly string Placeholder = @"Placeholder:";
		/// <summary>
		/// {0} + Enter
		/// </summary>
		public static readonly string PlaceholderAutoSubmit = @"{0} + Enter";
		/// <summary>
		/// Error parsing data
		/// </summary>
		public static readonly string ErrorGoogleAuthImport = @"Error parsing data";
		/// <summary>
		/// Error parsing data
		/// 
		/// Expected amount of OTP entries: 1
		/// Found amount of OTP entries: {0}
		/// </summary>
		public static readonly string ErrorGoogleAuthImportCount = @"Error parsing data

Expected amount of OTP entries: 1
Found amount of OTP entries: {0}";
		/// <summary>
		/// Please check the single entry to use
		/// </summary>
		public static readonly string SelectSingleEntry = @"Please select the single entry to use";
		/// <summary>
		/// Please select the entries to export
		/// </summary>
		public static readonly string SelectEntriesForExport = @"Please select the entries to export";
		/// <summary>
		/// Issuer
		/// </summary>
		public static readonly string Issuer = @"Issuer";
		/// <summary>
		/// Checking 2FA
		/// </summary>
		public static readonly string CheckingTFA = @"Checking 2FA";
		/// <summary>
		/// Read OTP QR code from screen
		/// </summary>
		public static readonly string ReadScreenForQRCode = @"Read OTP QR code from screen";
		/// <summary>
		/// KeePassOTP will now try to find and read the OTP QR code from your screen.
		/// KeePass itself will drop to background for max {0} seconds to simplify reading the OTP QR code.
		/// 
		/// This message will not be shown again.
		/// </summary>
		public static readonly string ReadScreenForQRCodeExplain = @"KeePassOTP will now try to find and read the OTP QR code from your screen.
KeePass itself will drop to background for max {0} seconds to simplify reading the OTP QR code.

This message will not be shown again.";
		/// <summary>
		/// Drag & drop a valid OTP QR code
		/// </summary>
		public static readonly string OTP_Setup_DragDrop = @"Drag & drop a valid OTP QR code";
		/// <summary>
		/// Renew copied OTP
		/// </summary>
		public static readonly string OTPRenewal = @"Renew copied OTP";
		/// <summary>
		/// Never
		/// </summary>
		public static readonly string OTPRenewal_Inactive = @"Never";
		/// <summary>
		/// Renew until clipboard gets cleared
		/// </summary>
		public static readonly string OTPRenewal_RespectClipboardTimeout = @"Renew until clipboard gets cleared";
		/// <summary>
		/// Renew if soon to expire
		/// </summary>
		public static readonly string OTPRenewal_PreventShortDuration = @"Renew if soon to expire";
		/// <summary>
		/// OTP display mode:
		/// </summary>
		public static readonly string OTPDisplayMode_label = @"OTP display mode:";
		/// <summary>
		/// Recovery codes
		/// </summary>
		public static readonly string RecoveryCodes = @"Recovery codes";
		/// <summary>
		/// Setup
		/// </summary>
		public static readonly string TFA_SetupURL = @"Setup";
		/// <summary>
		/// Recovery
		/// </summary>
		public static readonly string TFA_RecoveryURL = @"Recovery";
		/// <summary>
		/// Local hotkey
		/// </summary>
		public static readonly string LocalHotkey = @"Local hotkey";
		/// <summary>
		/// The hotkey will work only within KeePass and instead of Auto-Type it will copy the OTP value into the clipboard
		/// </summary>
		public static readonly string LocalHotkeyTooltip = @"The hotkey will work only within KeePass and instead of Auto-Type it will copy the OTP value into the clipboard";
		/// <summary>
		/// Next
		/// </summary>
		public static readonly string CurrentOTP = @"OTP:";
		/// <summary>
		/// Next
		/// </summary>
		public static readonly string NextOTP = @"Next:";
		/// <summary>
		/// N/A
		/// </summary>
		public static readonly string NotAvailable = @"N/A";
		/// <summary>
		/// Deselect All
		/// </summary>
		public static readonly string DeselectAll = @"Deselect All";
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