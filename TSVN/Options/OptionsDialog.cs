using EnvDTE;
using System;
using System.IO;
using System.Windows.Forms;

namespace SamirBoulema.TSVN.Options
{
    public partial class OptionsDialog : Form
    {
        private Options options;

        public OptionsDialog(DTE dte)
        {
            InitializeComponent();
            OptionsHelper.Dte = dte;

            if (File.Exists(dte.Solution.FileName))
            {
                options = OptionsHelper.GetOptions();
                rootFolderTextBox.Text = options.RootFolder;
            }
            else
            {
                rootFolderTextBox.Enabled = false;
                okButton.Enabled = false;
                browseButton.Enabled = false;
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            options.RootFolder = rootFolderTextBox.Text;
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
