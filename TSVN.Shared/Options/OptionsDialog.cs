using EnvDTE80;
using SamirBoulema.TSVN.Helpers;
using System;
using System.IO;
using System.Windows.Forms;

namespace SamirBoulema.TSVN.Options
{
    public partial class OptionsDialog : Form
    {
        private Options options;

        public OptionsDialog(DTE2 dte)
        {
            InitializeComponent();
            OptionsHelper.Dte = dte;

            if (File.Exists(dte.Solution.FileName))
            {
                options = OptionsHelper.GetOptions();
                rootFolderTextBox.Text = options.RootFolder;
                onItemAddedAddToSVNCheckBox.Checked = options.OnItemAddedAddToSVN;
                onItemRenamedRenameInSVNCheckBox.Checked = options.OnItemRenamedRenameInSVN;
                onItemRemovedRemoveFromSVNCheckBox.Checked = options.OnItemRemovedRemoveFromSVN;
            }
            else
            {
                rootFolderTextBox.Enabled = false;
                onItemAddedAddToSVNCheckBox.Checked = false;
                onItemRenamedRenameInSVNCheckBox.Checked = false;
                onItemRemovedRemoveFromSVNCheckBox.Checked = false;
                okButton.Enabled = false;
                browseButton.Enabled = false;
            }

            if (string.IsNullOrEmpty(rootFolderTextBox.Text))
            {
                rootFolderTextBox.Text = CommandHelper.GetRepositoryRoot();
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            options.RootFolder = rootFolderTextBox.Text;
            options.OnItemAddedAddToSVN = onItemAddedAddToSVNCheckBox.Checked;
            options.OnItemRenamedRenameInSVN = onItemRenamedRenameInSVNCheckBox.Checked;
            options.OnItemRemovedRemoveFromSVN = onItemRemovedRemoveFromSVNCheckBox.Checked;
            OptionsHelper.SaveOptions(options);
            Close();
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                rootFolderTextBox.Text = folderBrowserDialog1.SelectedPath;
            }
        }
    }
}
