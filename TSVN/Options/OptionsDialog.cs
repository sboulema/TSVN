using EnvDTE;
using System;
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
            options = OptionsHelper.GetOptions();
            rootFolderTextBox.Text = options.RootFolder;
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
