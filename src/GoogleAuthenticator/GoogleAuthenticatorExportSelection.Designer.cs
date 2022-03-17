namespace KeePassOTP
{
	partial class GoogleAuthenticatorExportSelection
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
            this.pDesc = new System.Windows.Forms.Panel();
            this.lDesc = new System.Windows.Forms.Label();
            this.lvEntries = new System.Windows.Forms.ListView();
            this.chGroup = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chEntryTitle = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pButtons = new System.Windows.Forms.Panel();
            this.bDeselectAll = new System.Windows.Forms.Button();
            this.bSelectAll = new System.Windows.Forms.Button();
            this.bOK = new System.Windows.Forms.Button();
            this.bCancel = new System.Windows.Forms.Button();
            this.chEntryUsername = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pDesc.SuspendLayout();
            this.pButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // pDesc
            // 
            this.pDesc.Controls.Add(this.lDesc);
            this.pDesc.Dock = System.Windows.Forms.DockStyle.Top;
            this.pDesc.Location = new System.Drawing.Point(0, 0);
            this.pDesc.Margin = new System.Windows.Forms.Padding(5);
            this.pDesc.Name = "pDesc";
            this.pDesc.Size = new System.Drawing.Size(1422, 65);
            this.pDesc.TabIndex = 0;
            // 
            // lDesc
            // 
            this.lDesc.AutoSize = true;
            this.lDesc.Location = new System.Drawing.Point(21, 14);
            this.lDesc.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lDesc.Name = "lDesc";
            this.lDesc.Size = new System.Drawing.Size(93, 32);
            this.lDesc.TabIndex = 1;
            this.lDesc.Text = "label1";
            // 
            // lvEntries
            // 
            this.lvEntries.CheckBoxes = true;
            this.lvEntries.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chGroup,
            this.chEntryTitle,
            this.chEntryUsername});
            this.lvEntries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvEntries.FullRowSelect = true;
            this.lvEntries.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvEntries.HideSelection = false;
            this.lvEntries.Location = new System.Drawing.Point(0, 65);
            this.lvEntries.Margin = new System.Windows.Forms.Padding(5);
            this.lvEntries.Name = "lvEntries";
            this.lvEntries.Size = new System.Drawing.Size(1422, 698);
            this.lvEntries.TabIndex = 10;
            this.lvEntries.UseCompatibleStateImageBehavior = false;
            this.lvEntries.View = System.Windows.Forms.View.Details;
            this.lvEntries.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lvEntries_ItemChecked);
            // 
            // pButtons
            // 
            this.pButtons.Controls.Add(this.bDeselectAll);
            this.pButtons.Controls.Add(this.bSelectAll);
            this.pButtons.Controls.Add(this.bOK);
            this.pButtons.Controls.Add(this.bCancel);
            this.pButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pButtons.Location = new System.Drawing.Point(0, 592);
            this.pButtons.Margin = new System.Windows.Forms.Padding(5);
            this.pButtons.Name = "pButtons";
            this.pButtons.Size = new System.Drawing.Size(1422, 171);
            this.pButtons.TabIndex = 2;
            // 
            // bDeselectAll
            // 
            this.bDeselectAll.AutoSize = true;
            this.bDeselectAll.Location = new System.Drawing.Point(40, 88);
            this.bDeselectAll.Margin = new System.Windows.Forms.Padding(5);
            this.bDeselectAll.Name = "bDeselectAll";
            this.bDeselectAll.Size = new System.Drawing.Size(178, 54);
            this.bDeselectAll.TabIndex = 30;
            this.bDeselectAll.Text = "Deselect All";
            this.bDeselectAll.UseVisualStyleBackColor = true;
            this.bDeselectAll.Click += new System.EventHandler(this.OnSelectDeselectAll);
            // 
            // bSelectAll
            // 
            this.bSelectAll.AutoSize = true;
            this.bSelectAll.Location = new System.Drawing.Point(40, 24);
            this.bSelectAll.Margin = new System.Windows.Forms.Padding(5);
            this.bSelectAll.Name = "bSelectAll";
            this.bSelectAll.Size = new System.Drawing.Size(178, 54);
            this.bSelectAll.TabIndex = 20;
            this.bSelectAll.Text = "Select All";
            this.bSelectAll.UseVisualStyleBackColor = true;
            this.bSelectAll.Click += new System.EventHandler(this.OnSelectDeselectAll);
            // 
            // bOK
            // 
            this.bOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bOK.Location = new System.Drawing.Point(997, 88);
            this.bOK.Margin = new System.Windows.Forms.Padding(5);
            this.bOK.Name = "bOK";
            this.bOK.Size = new System.Drawing.Size(178, 54);
            this.bOK.TabIndex = 40;
            this.bOK.Text = "OK";
            this.bOK.UseVisualStyleBackColor = true;
            // 
            // bCancel
            // 
            this.bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bCancel.Location = new System.Drawing.Point(1185, 88);
            this.bCancel.Margin = new System.Windows.Forms.Padding(5);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(178, 54);
            this.bCancel.TabIndex = 50;
            this.bCancel.Text = "Cancel";
            this.bCancel.UseVisualStyleBackColor = true;
            // 
            // GoogleAuthenticatorExportSelection
            // 
            this.AcceptButton = this.bOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bCancel;
            this.ClientSize = new System.Drawing.Size(1422, 763);
            this.Controls.Add(this.pButtons);
            this.Controls.Add(this.lvEntries);
            this.Controls.Add(this.pDesc);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GoogleAuthenticatorExportSelection";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "GoogleAuthenticatorExportSelection";
            this.Shown += new System.EventHandler(this.GoogleAuthenticatorImportSelection_Shown);
            this.pDesc.ResumeLayout(false);
            this.pDesc.PerformLayout();
            this.pButtons.ResumeLayout(false);
            this.pButtons.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel pDesc;
		private System.Windows.Forms.Label lDesc;
		private System.Windows.Forms.ListView lvEntries;
		private System.Windows.Forms.ColumnHeader chGroup;
		private System.Windows.Forms.ColumnHeader chEntryTitle;
		private System.Windows.Forms.Panel pButtons;
		private System.Windows.Forms.Button bOK;
		private System.Windows.Forms.Button bCancel;
        private System.Windows.Forms.Button bDeselectAll;
        private System.Windows.Forms.Button bSelectAll;
        private System.Windows.Forms.ColumnHeader chEntryUsername;
    }
}