using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using KeePass;
using KeePassLib.Security;
using KeePassLib.Utility;
using PluginTools;
using PluginTranslation;

namespace KeePassOTP
{

	public partial class KeePassOTPSetup : Form, KeePass.UI.IGwmWindow
	{
		public bool CanCloseWithoutDataLoss { get { return !SettingsChanged(); } }

		public KPOTP OTP = null;
		private KPOTP m_OTPInitial = null;
		public string EntryUrl;

		private static Timer m_timer = null;
		private bool m_NoUpdate = true;
		private string m_BackupURL;

		public KeePassOTPSetup()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();

			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}

		public void InitEx()
		{
			Text = PluginTranslate.PluginName;
			gSeed.Text = PluginTranslate.SeedSettings;
			gOTP.Text = PluginTranslate.OTPSettings;
			gTime.Text = PluginTranslate.TimeCorrection;
			lSeed.Text = PluginTranslate.Seed;
			lFormat.Text = PluginTranslate.Format;
			lType.Text = PluginTranslate.OTPType;
			lLength.Text = PluginTranslate.OTPLength;
			lHash.Text = PluginTranslate.OTPHash;
			lTimestep.Text = PluginTranslate.OTPTimestep;
			lCounter.Text = PluginTranslate.OTPCounter;
			lTimeType.Text = PluginTranslate.OTPType;
			lURL.Text = PluginTranslate.URL;
			lTime.Text = PluginTranslate.TimeDiff;
			cbAdvanced.Text = PluginTranslate.AdvancedOptions;
			pbSearchScreen.Image = Resources.qr_code_screencapture;
			pbSearchScreen.Text = PluginTranslate.ReadScreenForQRCode;


			totpTimeCorrectionType.Items.Add(PluginTranslate.TimeCorrectionOff);
			totpTimeCorrectionType.Items.Add(PluginTranslate.TimeCorrectionEntry);
			totpTimeCorrectionType.Items.Add(PluginTranslate.TimeCorrectionFixed);

			m_NoUpdate = true;
			InitSettings(false);
			m_NoUpdate = false;
			UpdatePreview();

			CheckAdvancedMode();

			m_OTPInitial = OTP;

			OTP = new KPOTP();
			OTP.Issuer = m_OTPInitial.Issuer;
			OTP.Label = m_OTPInitial.Label;
			m_timer = new Timer();
			m_timer.Interval = 1000;
			m_timer.Tick += OnValueChanged;
			m_timer.Start();
		}

		private void InitSettings(bool bCheckAdvancedMode)
		{
			tbOTPSeed.Text = OTP.OTPSeed.ReadString();
			cbOTPType.SelectedIndex = (int)OTP.Type;
			cbOTPFormat.SelectedIndex = (int)OTP.Encoding;
			cbOTPLength.SelectedIndex = Math.Min(Math.Max(OTP.Length, 6), 10) - 6;
			cbOTPHashFunc.SelectedIndex = (int)OTP.Hash;
			tbTOTPTimestep.Text = OTP.TOTPTimestep.ToString();
			tbHOTPCounter.Text = OTP.HOTPCounter.ToString();

			if (bCheckAdvancedMode)
			{
				CheckAdvancedMode();
				return;
			}
			if (OTP.TimeCorrectionUrlOwn)
				totpTimeCorrectionType.SelectedIndex = 1;
			else if (string.IsNullOrEmpty(OTP.TimeCorrectionUrl) || (OTP.TimeCorrectionUrl == "OFF"))
				totpTimeCorrectionType.SelectedIndex = 0;
			else
			{
				totpTimeCorrectionType.SelectedIndex = 2;
				tbTOTPTimeCorrectionURL.Text = OTP.TimeCorrectionUrl;
			}
		}

		private bool SettingsChanged()
		{
			return !KPOTP.Equals(OTP, m_OTPInitial);
		}

		private void UpdatePreview()
		{
			if (m_NoUpdate) return;
			if (StrUtil.IsDataUri(tbOTPSeed.Text))
			{
				try
				{
					OTP.OTPAuthString = ParseFromImageByteArray(StrUtil.DataUriToData(tbOTPSeed.Text));
					InitSettings(true);
				}
				catch { tbOTPSeed.Text = string.Empty; }
			}
			if (tbOTPSeed.Text.ToLowerInvariant().StartsWith("otpauth://"))
			{
				OTP.OTPAuthString = new ProtectedString(true, tbOTPSeed.Text);
				InitSettings(true);
			}
			else if (tbOTPSeed.Text.ToLowerInvariant().StartsWith("otpauth-migration://"))
			{
				m_NoUpdate = true;
				try
				{
					int iCount;
					ProtectedString psGoogleAuth = PSConvert.ParseGoogleAuthExport(tbOTPSeed.Text, out iCount);
					if ((iCount == 1) && (psGoogleAuth.Length > 0))
					{
						//tbOTPSeed.Text = psGoogleAuth.ReadString();
						OTP.OTPAuthString = psGoogleAuth;
						InitSettings(true);
						return;
					}
					tbOTPSeed.Text = string.Empty;

					if (iCount > 1) Tools.ShowError(string.Format(PluginTranslate.ErrorGoogleAuthImportCount, iCount.ToString()));
					else if (iCount == 0) Tools.ShowError(PluginTranslate.ErrorGoogleAuthImport);
					return;
				}
				finally { m_NoUpdate = false; }
			}

			else OTP.OTPSeed = new ProtectedString(true, tbOTPSeed.Text);

			if (cbOTPFormat.SelectedIndex == 0) OTP.Encoding = KPOTPEncoding.BASE32;
			if (cbOTPFormat.SelectedIndex == 1) OTP.Encoding = KPOTPEncoding.BASE64;
			if (cbOTPFormat.SelectedIndex == 2) OTP.Encoding = KPOTPEncoding.HEX;
			if (cbOTPFormat.SelectedIndex == 3) OTP.Encoding = KPOTPEncoding.UTF8;

			OTP.Length = cbOTPLength.SelectedIndex + 6;

			int dummy = -1;
			if (int.TryParse(tbTOTPTimestep.Text, out dummy)) OTP.TOTPTimestep = dummy;
			if (int.TryParse(tbHOTPCounter.Text, out dummy)) OTP.HOTPCounter = dummy;

			if (cbOTPHashFunc.SelectedIndex == 0) OTP.Hash = KPOTPHash.SHA1;
			if (cbOTPHashFunc.SelectedIndex == 1) OTP.Hash = KPOTPHash.SHA256;
			if (cbOTPHashFunc.SelectedIndex == 2) OTP.Hash = KPOTPHash.SHA512;

			if (cbOTPType.SelectedIndex == 0) OTP.Type = KPOTPType.HOTP;
			else if (cbOTPType.SelectedIndex == 1) OTP.Type = KPOTPType.TOTP;
			else if (cbOTPType.SelectedIndex == 2) OTP.Type = KPOTPType.STEAM;

			pbTOTPLifetime.Maximum = OTP.TOTPTimestep;
			pbTOTPLifetime.Value = OTP.RemainingSeconds;
			gTime.Enabled = tbTOTPTimestep.Visible = lTimestep.Visible = OTP.Type == KPOTPType.TOTP;
			pbTOTPLifetime.Visible = OTP.Type != KPOTPType.HOTP;
			tbHOTPCounter.Visible = lCounter.Visible = OTP.Type == KPOTPType.HOTP;

			cbOTPLength.Enabled = OTP.Type != KPOTPType.STEAM;
			if (OTP.Type == KPOTPType.STEAM)
			{
				if (!cbOTPLength.Items.Contains("5")) cbOTPLength.Items.Add("5");
				cbOTPLength.SelectedIndex = cbOTPLength.Items.Count - 1;
			}
			else if (cbOTPLength.Items.Contains("5"))
			{
				cbOTPLength.Items.RemoveAt(cbOTPLength.Items.Count - 1);
				cbOTPLength.SelectedIndex = 0;
			}

			if (!tbTOTPTimeCorrectionURL.Focused)
			{
				if (totpTimeCorrectionType.SelectedIndex == 0)
				{
					if (!tbTOTPTimeCorrectionURL.ReadOnly) m_BackupURL = tbTOTPTimeCorrectionURL.Text;
					tbTOTPTimeCorrectionURL.Text = string.Empty;
					tbTOTPTimeCorrectionURL.ReadOnly = true;
					OTP.TimeCorrectionUrlOwn = false;
					OTP.TimeCorrectionUrl = string.Empty;
				}
				else if (totpTimeCorrectionType.SelectedIndex == 1)
				{
					if (!tbTOTPTimeCorrectionURL.ReadOnly) m_BackupURL = tbTOTPTimeCorrectionURL.Text;
					tbTOTPTimeCorrectionURL.Text = EntryUrl;
					tbTOTPTimeCorrectionURL.ReadOnly = true;
					OTP.TimeCorrectionUrlOwn = true;
					OTP.TimeCorrectionUrl = EntryUrl;
				}
				else
				{
					if (tbTOTPTimeCorrectionURL.ReadOnly)
						tbTOTPTimeCorrectionURL.Text = string.IsNullOrEmpty(m_BackupURL) ? EntryUrl : m_BackupURL;
					tbTOTPTimeCorrectionURL.ReadOnly = false;
					OTP.TimeCorrectionUrlOwn = false;
					OTP.TimeCorrectionUrl = tbTOTPTimeCorrectionURL.Text;
				}
			}
			totpTimeCorrectionValue.Text = OTP.OTPTimeCorrection.ToString();

			string otpValue = OTP.Valid ? OTP.ReadableOTP(OTP.GetOTP(false, true)) : PluginTranslate.Error;
			otpPreview.Text = "OTP: " + (string.IsNullOrEmpty(otpValue) ? "N/A" : otpValue);
			if ((OTP.Type != KPOTPType.HOTP) && OTP.RemainingSeconds <= Config.TOTPSoonExpiring)
			{
				otpPreview.ForeColor = System.Drawing.Color.Red;
				pbTOTPLifetime.ForeColor = System.Drawing.Color.Red;
			}
			else
			{
				otpPreview.ForeColor = System.Drawing.SystemColors.ControlText;
				pbTOTPLifetime.ForeColor = System.Drawing.SystemColors.Highlight;
			}

			otpValue = OTP.Valid ? OTP.ReadableOTP(OTP.GetOTP(true, true)) : PluginTranslate.Error;
			otpPreviewNext.Text = "Next: " + (string.IsNullOrEmpty(otpValue) ? "N/A" : otpValue);
		}

		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			m_timer.Tick -= OnValueChanged;
			UpdatePreview();
		}

		private void OnValueChanged(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		private void cbAdvanced_CheckedChanged(object sender, EventArgs e)
		{
			gOTP.Visible = gTime.Visible = cbAdvanced.Checked;
			if (cbAdvanced.Checked)
			{
				Height += (gOTP.Height + gTime.Height);
			}
			else
			{
				Height -= (gOTP.Height + gTime.Height);
				cbOTPFormat.SelectedIndex = 0;
				cbOTPLength.SelectedIndex = 0;
				tbTOTPTimestep.Text = "30";
				cbOTPHashFunc.SelectedIndex = 0;
				cbOTPType.SelectedIndex = 1;

				totpTimeCorrectionType.SelectedIndex = 0;
				UpdatePreview();
			}
		}

		private void CheckAdvancedMode()
		{
			cbAdvanced.Checked = (OTP.Type != KPOTPType.TOTP)
				|| (OTP.Length != 6)
				|| (OTP.TOTPTimestep != 30)
				|| (OTP.Encoding != KPOTPEncoding.BASE32)
				|| (OTP.Hash != KPOTPHash.SHA1)
				|| (!string.IsNullOrEmpty(OTP.TimeCorrectionUrl) && !OTP.TimeCorrectionUrlOwn);
		}

		private void pbQR_DragDrop(object sender, DragEventArgs e)
		{
			// Drag&Drop is different depending on the source
			// Try different options and stop when a valid otpauth string is found
			// 
			//
			// 1. local file / network file
			// 2. Bitmap
			// 3. img-tag (html)
			// 4. String
			// 5. Text

			ProtectedString otp = ProtectedString.EmptyEx;
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				var f = e.Data.GetData(DataFormats.FileDrop) as string[];
				if (f != null)
				{
					otp = ParseFromImageFile(f[0]);
					if (!IsValidOtpAuth(otp)) otp = CheckAndConvertGoogleAuthFormat(otp);
				}
			}
			if (!IsValidOtpAuth(otp))
			{
				otp = ParseFromImage(e.Data.GetData(DataFormats.Bitmap) as System.Drawing.Bitmap);
				if (!IsValidOtpAuth(otp)) otp = CheckAndConvertGoogleAuthFormat(otp);
			}
			if (!IsValidOtpAuth(otp))
			{
				otp = ParseFromTextHtml(e.Data.GetData("text/html"));
				if (!IsValidOtpAuth(otp)) otp = CheckAndConvertGoogleAuthFormat(otp);
			}
			if (!IsValidOtpAuth(otp))
			{
				try { otp = new ProtectedString(true, e.Data.GetData(DataFormats.StringFormat) as string); }
				catch { otp = ProtectedString.EmptyEx; }
				if (!IsValidOtpAuth(otp)) otp = CheckAndConvertGoogleAuthFormat(otp);
			}
			if (!IsValidOtpAuth(otp))
			{
				try { otp = new ProtectedString(true, e.Data.GetData(DataFormats.Text) as string); }
				catch { otp = ProtectedString.EmptyEx; }
				if (!IsValidOtpAuth(otp)) otp = CheckAndConvertGoogleAuthFormat(otp);
			}
			if (IsValidOtpAuth(otp))
			{
				OTP.OTPAuthString = otp;
				m_NoUpdate = true; 
				InitSettings(true);
				m_NoUpdate = false;
			}
		}

		private ProtectedString CheckAndConvertGoogleAuthFormat(ProtectedString otp)
		{
			if (otp.ReadString().ToLowerInvariant().StartsWith("otpauth-migration://offline?data="))
			{
				int iOTPCount = 0;
				try { otp = PSConvert.ParseGoogleAuthExport(otp.ReadString(), out iOTPCount); }
				catch { }
				if (iOTPCount > 1) Tools.ShowError(string.Format(PluginTranslate.ErrorGoogleAuthImportCount, iOTPCount.ToString()));
				else if (iOTPCount == 0) Tools.ShowError(PluginTranslate.ErrorGoogleAuthImport);
			}
			return otp;
		}

		private bool IsValidOtpAuth(ProtectedString otp)
		{
			if (otp == null) return false;
			if (otp.Length < 11) return false;
			KPOTP check = new KPOTP();
			check.OTPAuthString = otp;
			return check.Valid;
		}

		private ProtectedString ParseFromTextHtml(object obj)
		{
			string html = string.Empty;
			if (obj is string)
			{
				html = (string)obj;
			}
			else if (obj is System.IO.MemoryStream)
			{
				System.IO.MemoryStream ms = (System.IO.MemoryStream)obj;
				byte[] buffer = new byte[ms.Length];
				ms.Read(buffer, 0, (int)ms.Length);
				if (buffer[1] == (byte)0)  // Detecting unicode
				{
					html = Encoding.Unicode.GetString(buffer);
				}
				else
				{
					html = Encoding.ASCII.GetString(buffer);
				}
			}
			var match = new System.Text.RegularExpressions.Regex(@"<img.* src=""([^""]*)""").Match(html);
			if (match.Success) return ParseFromImageFile(match.Groups[1].Value);
			else return ParseFromImageFile(html);
		}

		private ProtectedString ParseFromImageFile(string sFile)
		{
			if (string.IsNullOrEmpty(sFile)) return ProtectedString.EmptyEx;
			KeePassLib.Serialization.IOConnectionInfo iocInfo = new KeePassLib.Serialization.IOConnectionInfo();
			iocInfo.Path = sFile;
			byte[] buffer = KeePassLib.Serialization.IOConnection.ReadFile(iocInfo);
			return ParseFromImageByteArray(buffer);
		}

		private ProtectedString ParseFromImageByteArray(byte[] buffer)
		{
			if (buffer == null || buffer.Length == 0) return ProtectedString.EmptyEx;
			System.IO.MemoryStream memstr = new System.IO.MemoryStream(buffer);
			System.Drawing.Bitmap img = System.Drawing.Image.FromStream(memstr) as System.Drawing.Bitmap;
			return ParseFromImage(img);
		}

		private ProtectedString ParseFromImage(System.Drawing.Bitmap bitmap)
		{
			if (bitmap == null) return ProtectedString.EmptyEx;
			ZXing.BarcodeReader r = new ZXing.BarcodeReader();
			ZXing.Result result = r.Decode(bitmap);
			if (result != null) return new ProtectedString(true, result.Text);
			return ProtectedString.EmptyEx;
		}

		private void pbQR_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.All | DragDropEffects.Link;
		}

		private void KeePassOTPSetup_Shown(object sender, EventArgs e)
		{
			pbQR.Image = Resources.qr_code;
			((Control)pbQR).AllowDrop = true;
		}

		private void bSearchScreen_Click(object sender, EventArgs e)
		{
			int iSeconds = 30;
			ShowReadScreenForQRCodeExplanation(!RightToLeftLayout, iSeconds);

			if (pbSearchScreen.Text != PluginTranslate.ReadScreenForQRCode)
			{
				pbSearchScreen.Text = PluginTranslate.ReadScreenForQRCode;
				pbSearchScreen.Image = Resources.qr_code_screencapture;
				return;
			}

			pbSearchScreen.Text = KeePass.Resources.KPRes.Cancel;
			pbSearchScreen.Image = KeePass.UI.UIUtil.CreateGrayImage(Resources.qr_code_screencapture);
			
			bool bWasTopMost = Program.MainForm.TopMost;
			SaveOpacityOrDropToBackground();
			SearchScreenForQRCode(iSeconds);
			Activate();
			BringToFront();
			Program.MainForm.TopMost = bWasTopMost;
			RestoreOpacity();
			pbSearchScreen.Text = PluginTranslate.ReadScreenForQRCode;
			pbSearchScreen.Image = Resources.qr_code_screencapture;
		}

		private Dictionary<Form, double> m_dOpacity = new Dictionary<Form, double>();
		private static MethodInfo m_miLoseFocus = null;
		private static bool? m_bFoundLoseFocus = null;
		private void SaveOpacityOrDropToBackground()
		{
			if (!m_bFoundLoseFocus.HasValue)
			{
				var t = Program.MainForm.GetType().Assembly.GetType("KeePass.Native.NativeMethods");
				m_miLoseFocus = t.GetMethod("LoseFocus", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(Form), typeof(bool) }, null);
				m_bFoundLoseFocus = m_miLoseFocus != null;
			}

			if (m_bFoundLoseFocus.HasValue && m_bFoundLoseFocus.Value)
			{
				Program.MainForm.TopMost = false;
				m_miLoseFocus.Invoke(null, new object[] { this, true });
				return;
			}

			if (RightToLeftLayout) return;
			m_dOpacity[Program.MainForm] = Program.MainForm.Opacity;
			m_dOpacity[this] = Opacity;

			Program.MainForm.Opacity = 0.25;
			Opacity = 0.25;
		}

		private void RestoreOpacity()
		{
			foreach (KeyValuePair<Form, double> kvp in m_dOpacity)
				kvp.Key.Opacity = kvp.Value;
			m_dOpacity.Clear();
		}

		private bool SearchScreenForQRCode(int iSeconds)
		{
			int iSleep = 250;
			try
			{
				DateTime dtEnd = DateTime.Now.AddSeconds(iSeconds);
				while (dtEnd > DateTime.Now)
				{
					System.Threading.Thread.Sleep(iSleep);
					Application.DoEvents();
					Bitmap bmp = new Bitmap(SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height, PixelFormat.Format32bppArgb);

					using (Graphics g = Graphics.FromImage(bmp))
					{
						g.CopyFromScreen(SystemInformation.VirtualScreen.X, SystemInformation.VirtualScreen.Y, 0, 0, SystemInformation.VirtualScreen.Size, CopyPixelOperation.SourceCopy);
					}

					ProtectedString otp = ParseFromImage(bmp);
					bmp.Dispose();
					if (IsValidOtpAuth(otp))
					{
						OTP.OTPAuthString = otp;
						m_NoUpdate = true;
						InitSettings(true);
						m_NoUpdate = false; 
						return true;
					}
					if (pbSearchScreen.Text == PluginTranslate.ReadScreenForQRCode) return false;
				}
			}
			catch { }
			return false;
		}

		private void ShowReadScreenForQRCodeExplanation(bool bUseOpacity, int iSeconds)
		{
			if (!bUseOpacity) return;
			if (Config.ReadScreenForQRCodeExplanationShown) return;
			Config.ReadScreenForQRCodeExplanationShown = true;
			Tools.ShowInfo(string.Format(PluginTranslate.ReadScreenForQRCodeExplain, iSeconds));
		}

		private void pbQR_MouseHover(object sender, EventArgs e)
		{
			toolTip1.Show(PluginTranslate.OTP_Setup_DragDrop, pbQR);
		}

		private void bSearchScreen_MouseHover(object sender, EventArgs e)
		{
			toolTip1.Show(PluginTranslate.ReadScreenForQRCode, pbSearchScreen);
		}
	}
}