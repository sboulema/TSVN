namespace SamirBoulema.TSVN.Options
{
    partial class OptionsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.rootFolderTextBox = new System.Windows.Forms.TextBox();
            this.browseButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.onItemAddedAddToSVNCheckBox = new System.Windows.Forms.CheckBox();
            this.onItemRenamedRenameInSVNCheckBox = new System.Windows.Forms.CheckBox();
            this.onItemRemovedRemoveFromSVNCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.closeOnEndCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(121, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Working copy root path:";
            // 
            // rootFolderTextBox
            // 
            this.rootFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rootFolderTextBox.Location = new System.Drawing.Point(9, 46);
            this.rootFolderTextBox.Name = "rootFolderTextBox";
            this.rootFolderTextBox.Size = new System.Drawing.Size(270, 20);
            this.rootFolderTextBox.TabIndex = 1;
            // 
            // browseButton
            // 
            this.browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseButton.Location = new System.Drawing.Point(285, 44);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(75, 23);
            this.browseButton.TabIndex = 2;
            this.browseButton.Text = "Browse...";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(228, 213);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 3;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(309, 213);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // onItemAddedAddToSVNCheckBox
            // 
            this.onItemAddedAddToSVNCheckBox.AutoSize = true;
            this.onItemAddedAddToSVNCheckBox.Location = new System.Drawing.Point(9, 81);
            this.onItemAddedAddToSVNCheckBox.Name = "onItemAddedAddToSVNCheckBox";
            this.onItemAddedAddToSVNCheckBox.Size = new System.Drawing.Size(238, 17);
            this.onItemAddedAddToSVNCheckBox.TabIndex = 5;
            this.onItemAddedAddToSVNCheckBox.Text = "Automatically add new files to source control ";
            this.onItemAddedAddToSVNCheckBox.UseVisualStyleBackColor = true;
            // 
            // onItemRenamedRenameInSVNCheckBox
            // 
            this.onItemRenamedRenameInSVNCheckBox.AutoSize = true;
            this.onItemRenamedRenameInSVNCheckBox.Location = new System.Drawing.Point(9, 104);
            this.onItemRenamedRenameInSVNCheckBox.Name = "onItemRenamedRenameInSVNCheckBox";
            this.onItemRenamedRenameInSVNCheckBox.Size = new System.Drawing.Size(259, 17);
            this.onItemRenamedRenameInSVNCheckBox.TabIndex = 6;
            this.onItemRenamedRenameInSVNCheckBox.Text = "Automatically move/rename files in source control";
            this.onItemRenamedRenameInSVNCheckBox.UseVisualStyleBackColor = true;
            // 
            // onItemRemovedRemoveFromSVNCheckBox
            // 
            this.onItemRemovedRemoveFromSVNCheckBox.AutoSize = true;
            this.onItemRemovedRemoveFromSVNCheckBox.Location = new System.Drawing.Point(9, 127);
            this.onItemRemovedRemoveFromSVNCheckBox.Name = "onItemRemovedRemoveFromSVNCheckBox";
            this.onItemRemovedRemoveFromSVNCheckBox.Size = new System.Drawing.Size(240, 17);
            this.onItemRemovedRemoveFromSVNCheckBox.TabIndex = 7;
            this.onItemRemovedRemoveFromSVNCheckBox.Text = "Automatically remove files from source control";
            this.onItemRemovedRemoveFromSVNCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.closeOnEndCheckBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.onItemRemovedRemoveFromSVNCheckBox);
            this.groupBox1.Controls.Add(this.rootFolderTextBox);
            this.groupBox1.Controls.Add(this.onItemRenamedRenameInSVNCheckBox);
            this.groupBox1.Controls.Add(this.browseButton);
            this.groupBox1.Controls.Add(this.onItemAddedAddToSVNCheckBox);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(372, 182);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Solution-specific Options";
            // 
            // closeOnEndCheckBox
            // 
            this.closeOnEndCheckBox.AutoSize = true;
            this.closeOnEndCheckBox.Location = new System.Drawing.Point(9, 150);
            this.closeOnEndCheckBox.Name = "closeOnEndCheckBox";
            this.closeOnEndCheckBox.Size = new System.Drawing.Size(204, 17);
            this.closeOnEndCheckBox.TabIndex = 8;
            this.closeOnEndCheckBox.Text = "Automatically close dialogs if no errors";
            this.closeOnEndCheckBox.UseVisualStyleBackColor = true;
            // 
            // OptionsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(396, 252);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OptionsDialog";
            this.Text = "Options";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox rootFolderTextBox;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.CheckBox onItemAddedAddToSVNCheckBox;
        private System.Windows.Forms.CheckBox onItemRenamedRenameInSVNCheckBox;
        private System.Windows.Forms.CheckBox onItemRemovedRemoveFromSVNCheckBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox closeOnEndCheckBox;
    }
}