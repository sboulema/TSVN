using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using SamirBoulema.TSVN.Helpers;
using System;
using System.IO;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace SamirBoulema.TSVN.Options
{
    public partial class OptionsDialog : Form
    {
        private Options options;

        public OptionsDialog(DTE2 dte)
        {
            InitializeComponent();
            OptionsHelper.Dte = dte;

            Load += new EventHandler(OptionsDialog_Load);
        }

        private void OptionsDialog_Load(object sender, EventArgs e)
        {
            _ = LoadDialog();
        }

        private async Task LoadDialog()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (File.Exists(OptionsHelper.Dte.Solution.FileName))
            {
                options = await OptionsHelper .GetOptions();
                rootFolderTextBox.Text = options.RootFolder;
                onItemAddedAddToSVNCheckBox.Checked = options.OnItemAddedAddToSVN;
                onItemRenamedRenameInSVNCheckBox.Checked = options.OnItemRenamedRenameInSVN;
                onItemRemovedRemoveFromSVNCheckBox.Checked = options.OnItemRemovedRemoveFromSVN;
                closeOnEndCheckBox.Checked = options.CloseOnEnd;
            }
            else
            {
                rootFolderTextBox.Enabled = false;
                onItemAddedAddToSVNCheckBox.Checked = false;
                onItemRenamedRenameInSVNCheckBox.Checked = false;
                onItemRemovedRemoveFromSVNCheckBox.Checked = false;
                closeOnEndCheckBox.Checked = false;
                okButton.Enabled = false;
                browseButton.Enabled = false;
            }

            if (string.IsNullOrEmpty(rootFolderTextBox.Text))
            {
                rootFolderTextBox.Text = await CommandHelper.GetRepositoryRoot();
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OkButton_Click(object sender, EventArgs e)
            => _ = Save();

        private async Task Save()
        {
            options.RootFolder = rootFolderTextBox.Text;
            options.OnItemAddedAddToSVN = onItemAddedAddToSVNCheckBox.Checked;
            options.OnItemRenamedRenameInSVN = onItemRenamedRenameInSVNCheckBox.Checked;
            options.OnItemRemovedRemoveFromSVN = onItemRemovedRemoveFromSVNCheckBox.Checked;
            options.CloseOnEnd = closeOnEndCheckBox.Checked;
            await OptionsHelper.SaveOptions(options);
            Close();
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                rootFolderTextBox.Text = folderBrowserDialog1.SelectedPath;
            }
        }
    }
}
