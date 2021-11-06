
namespace KeePassOTP
{
    partial class KPOTP_Details
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbNotes = new System.Windows.Forms.TextBox();
            this.llDocURL = new System.Windows.Forms.LinkLabel();
            this.llSetupUrl = new System.Windows.Forms.LinkLabel();
            this.lRecoveryURL = new System.Windows.Forms.Label();
            this.lNotes = new System.Windows.Forms.Label();
            this.lDocURL = new System.Windows.Forms.Label();
            this.lSetupURL = new System.Windows.Forms.Label();
            this.llRecoveryURL = new System.Windows.Forms.LinkLabel();
            this.lTFA_Modes = new System.Windows.Forms.Label();
            this.lbTFA_Modes = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbNotes
            // 
            this.tbNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbNotes.Location = new System.Drawing.Point(197, 126);
            this.tbNotes.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.tbNotes.Multiline = true;
            this.tbNotes.Name = "tbNotes";
            this.tbNotes.ReadOnly = true;
            this.tbNotes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbNotes.Size = new System.Drawing.Size(640, 185);
            this.tbNotes.TabIndex = 13;
            // 
            // llDocURL
            // 
            this.llDocURL.AutoSize = true;
            this.llDocURL.Location = new System.Drawing.Point(197, 42);
            this.llDocURL.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.llDocURL.Name = "llDocURL";
            this.llDocURL.Size = new System.Drawing.Size(128, 32);
            this.llDocURL.TabIndex = 12;
            this.llDocURL.TabStop = true;
            this.llDocURL.Text = "Doc URL";
            this.llDocURL.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkClicked);
            // 
            // llSetupUrl
            // 
            this.llSetupUrl.AutoSize = true;
            this.llSetupUrl.Location = new System.Drawing.Point(197, 0);
            this.llSetupUrl.Margin = new System.Windows.Forms.Padding(0);
            this.llSetupUrl.Name = "llSetupUrl";
            this.llSetupUrl.Size = new System.Drawing.Size(153, 32);
            this.llSetupUrl.TabIndex = 11;
            this.llSetupUrl.TabStop = true;
            this.llSetupUrl.Text = "Setup URL";
            this.llSetupUrl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkClicked);
            // 
            // lRecoveryURL
            // 
            this.lRecoveryURL.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lRecoveryURL.AutoSize = true;
            this.lRecoveryURL.Location = new System.Drawing.Point(0, 84);
            this.lRecoveryURL.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.lRecoveryURL.Name = "lRecoveryURL";
            this.lRecoveryURL.Size = new System.Drawing.Size(197, 32);
            this.lRecoveryURL.TabIndex = 10;
            this.lRecoveryURL.Text = "lRecoveryURL";
            // 
            // lNotes
            // 
            this.lNotes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lNotes.AutoSize = true;
            this.lNotes.Location = new System.Drawing.Point(0, 126);
            this.lNotes.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.lNotes.Name = "lNotes";
            this.lNotes.Size = new System.Drawing.Size(197, 185);
            this.lNotes.TabIndex = 9;
            this.lNotes.Text = "Notes";
            // 
            // lDocURL
            // 
            this.lDocURL.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lDocURL.AutoSize = true;
            this.lDocURL.Location = new System.Drawing.Point(0, 42);
            this.lDocURL.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.lDocURL.Name = "lDocURL";
            this.lDocURL.Size = new System.Drawing.Size(197, 32);
            this.lDocURL.TabIndex = 8;
            this.lDocURL.Text = "Doc URL";
            // 
            // lSetupURL
            // 
            this.lSetupURL.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lSetupURL.AutoSize = true;
            this.lSetupURL.Location = new System.Drawing.Point(0, 0);
            this.lSetupURL.Margin = new System.Windows.Forms.Padding(0);
            this.lSetupURL.Name = "lSetupURL";
            this.lSetupURL.Size = new System.Drawing.Size(197, 32);
            this.lSetupURL.TabIndex = 7;
            this.lSetupURL.Text = "Setup URL";
            // 
            // llRecoveryURL
            // 
            this.llRecoveryURL.AutoSize = true;
            this.llRecoveryURL.Location = new System.Drawing.Point(197, 84);
            this.llRecoveryURL.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.llRecoveryURL.Name = "llRecoveryURL";
            this.llRecoveryURL.Size = new System.Drawing.Size(190, 32);
            this.llRecoveryURL.TabIndex = 14;
            this.llRecoveryURL.TabStop = true;
            this.llRecoveryURL.Text = "RecoveryURL";
            this.llRecoveryURL.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkClicked);
            // 
            // lTFA_Modes
            // 
            this.lTFA_Modes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lTFA_Modes.AutoSize = true;
            this.lTFA_Modes.Location = new System.Drawing.Point(0, 321);
            this.lTFA_Modes.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.lTFA_Modes.Name = "lTFA_Modes";
            this.lTFA_Modes.Size = new System.Drawing.Size(197, 243);
            this.lTFA_Modes.TabIndex = 15;
            this.lTFA_Modes.Text = "TFA_Modes";
            // 
            // lbTFA_Modes
            // 
            this.lbTFA_Modes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbTFA_Modes.FormattingEnabled = true;
            this.lbTFA_Modes.ItemHeight = 31;
            this.lbTFA_Modes.Location = new System.Drawing.Point(197, 321);
            this.lbTFA_Modes.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.lbTFA_Modes.Name = "lbTFA_Modes";
            this.lbTFA_Modes.Size = new System.Drawing.Size(640, 243);
            this.lbTFA_Modes.TabIndex = 16;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.lSetupURL, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lbTFA_Modes, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.llSetupUrl, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lTFA_Modes, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.lDocURL, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.llRecoveryURL, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.lNotes, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.llDocURL, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lRecoveryURL, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tbNotes, 1, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(16, 31);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(837, 564);
            this.tableLayoutPanel1.TabIndex = 17;
            // 
            // KPOTP_Details
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "KPOTP_Details";
            this.Padding = new System.Windows.Forms.Padding(16, 31, 16, 0);
            this.Size = new System.Drawing.Size(869, 595);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox tbNotes;
        private System.Windows.Forms.LinkLabel llDocURL;
        private System.Windows.Forms.LinkLabel llSetupUrl;
        private System.Windows.Forms.Label lRecoveryURL;
        private System.Windows.Forms.Label lNotes;
        private System.Windows.Forms.Label lDocURL;
        private System.Windows.Forms.Label lSetupURL;
        private System.Windows.Forms.LinkLabel llRecoveryURL;
        private System.Windows.Forms.Label lTFA_Modes;
        private System.Windows.Forms.ListBox lbTFA_Modes;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
