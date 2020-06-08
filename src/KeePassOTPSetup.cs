using System;
using System.Windows.Forms;
using KeePassLib.Security;
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
			if (tbOTPSeed.Text.ToLowerInvariant().StartsWith("otpauth://"))
			{
				OTP.OTPAuthString = new ProtectedString(true, tbOTPSeed.Text);
				InitSettings(true);
			}
			else
				OTP.OTPSeed = new ProtectedString(true, tbOTPSeed.Text);

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
			if (cbOTPType.SelectedIndex == 1) OTP.Type = KPOTPType.TOTP;

			pbTOTPLifetime.Maximum = OTP.TOTPTimestep;
			pbTOTPLifetime.Value = OTP.RemainingSeconds;
			gTime.Enabled = pbTOTPLifetime.Visible = tbTOTPTimestep.Visible = lTimestep.Visible = OTP.Type == KPOTPType.TOTP;
			tbHOTPCounter.Visible = lCounter.Visible = OTP.Type == KPOTPType.HOTP;

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
			if ((OTP.Type == KPOTPType.TOTP) && OTP.RemainingSeconds <= 5)
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
	}
}