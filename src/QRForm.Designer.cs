
namespace KeePassOTP
{
    partial class QRForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pbQR = new System.Windows.Forms.PictureBox();
            this.pButtons = new System.Windows.Forms.Panel();
            this.lIndex = new System.Windows.Forms.Label();
            this.bNext = new System.Windows.Forms.Button();
            this.bBack = new System.Windows.Forms.Button();
            this.pIssuerLabel = new System.Windows.Forms.Panel();
            this.cbToggleAuthstring = new System.Windows.Forms.CheckBox();
            this.tbAuthstring = new KeePass.UI.SecureTextBoxEx();
            this.lAuthstring = new System.Windows.Forms.Label();
            this.lLabel = new System.Windows.Forms.Label();
            this.lIssuer = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbQR)).BeginInit();
            this.pButtons.SuspendLayout();
            this.pIssuerLabel.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbQR
            // 
            this.pbQR.Location = new System.Drawing.Point(0, 0);
            this.pbQR.Name = "pbQR";
            this.pbQR.Size = new System.Drawing.Size(320, 320);
            this.pbQR.TabIndex = 0;
            this.pbQR.TabStop = false;
            // 
            // pButtons
            // 
            this.pButtons.Controls.Add(this.lIndex);
            this.pButtons.Controls.Add(this.bNext);
            this.pButtons.Controls.Add(this.bBack);
            this.pButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pButtons.Location = new System.Drawing.Point(0, 715);
            this.pButtons.Name = "pButtons";
            this.pButtons.Size = new System.Drawing.Size(818, 124);
            this.pButtons.TabIndex = 1;
            // 
            // lIndex
            // 
            this.lIndex.AutoSize = true;
            this.lIndex.Location = new System.Drawing.Point(345, 50);
            this.lIndex.Name = "lIndex";
            this.lIndex.Size = new System.Drawing.Size(91, 32);
            this.lIndex.TabIndex = 2;
            this.lIndex.Text = "lIndex";
            // 
            // bNext
            // 
            this.bNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bNext.Location = new System.Drawing.Point(556, 30);
            this.bNext.Name = "bNext";
            this.bNext.Size = new System.Drawing.Size(250, 70);
            this.bNext.TabIndex = 1;
            this.bNext.Text = "Next";
            this.bNext.UseVisualStyleBackColor = true;
            this.bNext.Click += new System.EventHandler(this.bNext_Click);
            // 
            // bBack
            // 
            this.bBack.Location = new System.Drawing.Point(12, 30);
            this.bBack.Name = "bBack";
            this.bBack.Size = new System.Drawing.Size(250, 70);
            this.bBack.TabIndex = 0;
            this.bBack.Text = "Back";
            this.bBack.UseVisualStyleBackColor = true;
            this.bBack.Click += new System.EventHandler(this.bBack_Click);
            // 
            // pIssuerLabel
            // 
            this.pIssuerLabel.Controls.Add(this.cbToggleAuthstring);
            this.pIssuerLabel.Controls.Add(this.tbAuthstring);
            this.pIssuerLabel.Controls.Add(this.lAuthstring);
            this.pIssuerLabel.Controls.Add(this.lLabel);
            this.pIssuerLabel.Controls.Add(this.lIssuer);
            this.pIssuerLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pIssuerLabel.Location = new System.Drawing.Point(0, 521);
            this.pIssuerLabel.Name = "pIssuerLabel";
            this.pIssuerLabel.Size = new System.Drawing.Size(818, 194);
            this.pIssuerLabel.TabIndex = 2;
            // 
            // cbToggleAuthstring
            // 
            this.cbToggleAuthstring.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbToggleAuthstring.Location = new System.Drawing.Point(719, 102);
            this.cbToggleAuthstring.Margin = new System.Windows.Forms.Padding(5);
            this.cbToggleAuthstring.Name = "cbToggleAuthstring";
            this.cbToggleAuthstring.Size = new System.Drawing.Size(71, 54);
            this.cbToggleAuthstring.TabIndex = 208;
            this.cbToggleAuthstring.Text = "***";
            this.cbToggleAuthstring.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbToggleAuthstring.UseVisualStyleBackColor = true;
            this.cbToggleAuthstring.CheckedChanged += new System.EventHandler(this.cbToggleNewPassword_CheckedChanged);
            // 
            // tbAuthstring
            // 
            this.tbAuthstring.Location = new System.Drawing.Point(117, 111);
            this.tbAuthstring.Margin = new System.Windows.Forms.Padding(5);
            this.tbAuthstring.Name = "tbAuthstring";
            this.tbAuthstring.Size = new System.Drawing.Size(592, 38);
            this.tbAuthstring.TabIndex = 206;
            // 
            // lAuthstring
            // 
            this.lAuthstring.AutoSize = true;
            this.lAuthstring.Location = new System.Drawing.Point(19, 117);
            this.lAuthstring.Name = "lAuthstring";
            this.lAuthstring.Size = new System.Drawing.Size(51, 32);
            this.lAuthstring.TabIndex = 2;
            this.lAuthstring.Text = "Uri";
            // 
            // lLabel
            // 
            this.lLabel.AutoSize = true;
            this.lLabel.Location = new System.Drawing.Point(19, 69);
            this.lLabel.Name = "lLabel";
            this.lLabel.Size = new System.Drawing.Size(93, 32);
            this.lLabel.TabIndex = 1;
            this.lLabel.Text = "lLabel";
            // 
            // lIssuer
            // 
            this.lIssuer.AutoSize = true;
            this.lIssuer.Location = new System.Drawing.Point(19, 17);
            this.lIssuer.Name = "lIssuer";
            this.lIssuer.Size = new System.Drawing.Size(98, 32);
            this.lIssuer.TabIndex = 0;
            this.lIssuer.Text = "lIssuer";
            // 
            // QRForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(818, 839);
            this.Controls.Add(this.pIssuerLabel);
            this.Controls.Add(this.pButtons);
            this.Controls.Add(this.pbQR);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "QRForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "QRForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.QRForm_FormClosing);
            this.Shown += new System.EventHandler(this.QRForm_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.QRForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.pbQR)).EndInit();
            this.pButtons.ResumeLayout(false);
            this.pButtons.PerformLayout();
            this.pIssuerLabel.ResumeLayout(false);
            this.pIssuerLabel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbQR;
        private System.Windows.Forms.Panel pButtons;
        private System.Windows.Forms.Button bNext;
        private System.Windows.Forms.Button bBack;
        private System.Windows.Forms.Panel pIssuerLabel;
        private System.Windows.Forms.Label lLabel;
        private System.Windows.Forms.Label lIssuer;
        private System.Windows.Forms.Label lIndex;
        private System.Windows.Forms.CheckBox cbToggleAuthstring;
        private KeePass.UI.SecureTextBoxEx tbAuthstring;
        private System.Windows.Forms.Label lAuthstring;
    }
}