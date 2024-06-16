namespace KeePassOTP
{
	partial class GoogleAuthenticatorImportSelection
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
            this.chIssuer = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chLabel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pButtons = new System.Windows.Forms.Panel();
            this.bOK = new System.Windows.Forms.Button();
            this.bCancel = new System.Windows.Forms.Button();
            this.pDesc.SuspendLayout();
            this.pButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // pDesc
            // 
            this.pDesc.Controls.Add(this.lDesc);
            this.pDesc.Dock = System.Windows.Forms.DockStyle.Top;
            this.pDesc.Location = new System.Drawing.Point(0, 0);
            this.pDesc.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
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
            this.chIssuer,
            this.chLabel});
            this.lvEntries.FullRowSelect = true;
            this.lvEntries.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvEntries.HideSelection = false;
            this.lvEntries.Location = new System.Drawing.Point(0, 65);
            this.lvEntries.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.lvEntries.Name = "lvEntries";
            this.lvEntries.Size = new System.Drawing.Size(1419, 630);
            this.lvEntries.TabIndex = 1;
            this.lvEntries.UseCompatibleStateImageBehavior = false;
            this.lvEntries.View = System.Windows.Forms.View.Details;
            this.lvEntries.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lvEntries_ItemChecked);
            // 
            // pButtons
            // 
            this.pButtons.Controls.Add(this.bOK);
            this.pButtons.Controls.Add(this.bCancel);
            this.pButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pButtons.Location = new System.Drawing.Point(0, 559);
            this.pButtons.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.pButtons.Name = "pButtons";
            this.pButtons.Size = new System.Drawing.Size(1422, 138);
            this.pButtons.TabIndex = 2;
            // 
            // bOK
            // 
            this.bOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bOK.Location = new System.Drawing.Point(997, 45);
            this.bOK.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.bOK.Name = "bOK";
            this.bOK.Size = new System.Drawing.Size(178, 54);
            this.bOK.TabIndex = 2;
            this.bOK.Text = "OK";
            this.bOK.UseVisualStyleBackColor = true;
            // 
            // bCancel
            // 
            this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bCancel.Location = new System.Drawing.Point(1186, 45);
            this.bCancel.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(178, 54);
            this.bCancel.TabIndex = 3;
            this.bCancel.Text = "Cancel";
            this.bCancel.UseVisualStyleBackColor = true;
            // 
            // GoogleAuthenticatorImportSelection
            // 
            this.AcceptButton = this.bOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bCancel;
            this.ClientSize = new System.Drawing.Size(1422, 697);
            this.Controls.Add(this.pButtons);
            this.Controls.Add(this.lvEntries);
            this.Controls.Add(this.pDesc);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GoogleAuthenticatorImportSelection";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "GoogleAuthenticatorImportSelection";
            this.Shown += new System.EventHandler(this.GoogleAuthenticatorImportSelection_Shown);
            this.pDesc.ResumeLayout(false);
            this.pDesc.PerformLayout();
            this.pButtons.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel pDesc;
		private System.Windows.Forms.Label lDesc;
		private System.Windows.Forms.ListView lvEntries;
		private System.Windows.Forms.ColumnHeader chIssuer;
		private System.Windows.Forms.ColumnHeader chLabel;
		private System.Windows.Forms.Panel pButtons;
		private System.Windows.Forms.Button bOK;
		private System.Windows.Forms.Button bCancel;
	}
}