using KeePass;
using KeePass.App;
using KeePass.App.Configuration;
using KeePass.Resources;
using KeePass.UI;
using KeePassLib.Security;
using PluginTranslation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeePassOTP
{
    public struct KPOTP_QRCodeData
    {
        public string Issuer;
        public string Label;
        public ProtectedString Authstring;
    }
    public partial class QRForm : Form, KeePass.UI.IGwmWindow
    {
        public bool CanCloseWithoutDataLoss { get { return true; } }

        private KPOTP_QRCodeData[] _aQR;
        //private ProtectedString[] _aQR;
        private int _idx = -1;

        private int _QR_SIZE_X = DpiUtil.ScaleIntX(320);
        private int _QR_SIZE_Y = DpiUtil.ScaleIntY(320);

        public QRForm()
        {
            InitializeComponent();
        }

        public void InitEx(bool bAutoClose, string sIssuer, string sLabel, params ProtectedString[] aQR)
        {
            List<KPOTP_QRCodeData> lQR = new List<KPOTP_QRCodeData>();
            foreach (var psAuth in aQR)
                lQR.Add(new KPOTP_QRCodeData() { Issuer = sIssuer, Label = sLabel, Authstring = psAuth });
            InitEx(bAutoClose, lQR);
        }

        public void InitEx(bool bAutoClose, List<KPOTP_QRCodeData> lQR)
        {
            _aQR = lQR.ToArray();
            Text = PluginTranslate.PluginName;
            bBack.Text = KPRes.ButtonBack;
            bNext.Text = KPRes.ButtonNext;

            AceColumn colPw = Program.Config.MainWindow.FindColumn(AceColumnType.Password);
            bool bShowPassword = (colPw != null) ? !colPw.HideWithAsterisks : false;
            if (Program.Config.UI.Hiding.SeparateHidingSettings)
                cbToggleAuthstring.Checked = Program.Config.UI.Hiding.HideInEntryWindow;
            else
                cbToggleAuthstring.Checked = !bShowPassword;
            SecureTextBoxEx.InitEx(ref tbAuthstring);
            tbAuthstring.EnableProtection(cbToggleAuthstring.Checked);

            var m = typeof(PwInputControlGroup).GetMethod("ConfigureHideButton", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            bool bConfiguredButton = false;
            if (m != null)
            {
                try
                {
                    m.Invoke(null, new object[] { cbToggleAuthstring, null });
                    bConfiguredButton = true;
                }
                catch { }
            }
            if (!bConfiguredButton) cbToggleAuthstring.Text = "***";

            tbAuthstring.ReadOnly = true;

            lAuthstring.Text = PluginTranslate.Seed;

            if (_aQR != null && _aQR.Length > 0) bNext_Click(null, null);

            if (bAutoClose) StartTimer();
        }

        private void StartTimer()
        {
            Timer tClose = new Timer();
            tClose.Interval = 30000;
            tClose.Tick += (o, e1) =>
            {
                tClose.Stop();
                tClose.Dispose();
                Close();
            };
            tClose.Start();
        }

        private void AdjustFields()
        {
            if (string.IsNullOrEmpty(lIssuer.Text) && string.IsNullOrEmpty(lLabel.Text))
            {
                pIssuerLabel.Visible = false;
                pButtons.Top = pIssuerLabel.Top;
            }
            if (_aQR != null && _aQR.Length == 1) pButtons.Visible = false;

            int iHeight = 0;
            foreach (Control c in Controls) if (c.Visible) iHeight += c.Height;

            ClientSize = new Size(pbQR.ClientSize.Width, iHeight);
        }

        private void QRForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) Close();
        }

        private void bBack_Click(object sender, EventArgs e)
        {
            _idx--;
            ShowQR();
            CheckButtons();
        }

        private void bNext_Click(object sender, EventArgs e)
        {
            _idx++;
            ShowQR();
            CheckButtons();
        }

        private void CheckButtons()
        {
            bBack.Enabled = _idx > 0;
            bNext.Enabled = _idx < _aQR.Length - 1;
        }

        private Dictionary<int, Bitmap> _bmp = new Dictionary<int, Bitmap>();
        private void ShowQR()
        {
            lIndex.Text = (_idx + 1).ToString() + " / " + (_aQR.Length).ToString();
            lIndex.Left = (pButtons.ClientSize.Width - lIndex.Width) / 2;
            lIssuer.Text = _aQR[_idx].Issuer;
            lLabel.Text = _aQR[_idx].Label;
            tbAuthstring.TextEx = _aQR[_idx].Authstring;
            if (!_bmp.ContainsKey(_idx))
            {
                try
                {
                    ZXing.BarcodeWriter zBW = new ZXing.BarcodeWriter();
                    zBW.Options.Width = _QR_SIZE_X;
                    zBW.Options.Height = _QR_SIZE_Y; 
                    zBW.Format = ZXing.BarcodeFormat.QR_CODE;
                    _bmp[_idx] = zBW.Write(_aQR[_idx].Authstring.ReadString());
                }
                catch (Exception ex)
                {
                    PluginTools.Tools.ShowError(ex.Message);
                    pbQR.Visible = false;
                    return;
                }
            }
            if (!pbQR.Visible) pbQR.Visible = true;
            pbQR.Image = new Bitmap(_bmp[_idx], _bmp[_idx].Size); //Assigning bmp directly did not work in my Ubuntu VM...
            pbQR.ClientSize = pbQR.Image.Size;
        }

        private void QRForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            GlobalWindowManager.RemoveWindow(this);
        }

        private void QRForm_Shown(object sender, EventArgs e)
        {
            GlobalWindowManager.AddWindow(this, this);
            AdjustFields();
        }

        private void cbToggleNewPassword_CheckedChanged(object sender, EventArgs e)
        {
            tbAuthstring.EnableProtection(cbToggleAuthstring.Checked);
        }
    }
}
