using SamirBoulema.TSVN.Helpers;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using SamirBoulema.TSVN.Models;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Task = System.Threading.Tasks.Task;
using Community.VisualStudio.Toolkit;
using Settings = SamirBoulema.TSVN.Properties.Settings;
using File = System.IO.File;

namespace SamirBoulema.TSVN
{
    /// <summary>
    /// Interaction logic for TSVNToolWindowControl.
    /// </summary>
    public partial class TSVNToolWindowControl : UserControl
    {
        public PendingChangesViewModel ViewModel;

        public TSVNToolWindowControl()
        {
            Loaded += OnLoaded;

            InitializeComponent();

            ViewModel = new PendingChangesViewModel();

            DataContext = ViewModel;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            VS.Events.DocumentEvents.Saved += DocumentEvents_Saved;

            VS.Events.SolutionEvents.OnAfterOpenSolution += SolutionEvents_OnAfterOpenSolution1;

            HideUnversionedButton.IsChecked = Settings.Default.HideUnversioned;

            _ = Update();
        }

        private void SolutionEvents_OnAfterOpenSolution1(SolutionItem obj)
        {
            _ = Update();
        }

        private void DocumentEvents_Saved(object sender, string e)
        {
            _ = Update();
        }

        private async Task Update()
            => Update(CommandHelper.GetPendingChanges(), await CommandHelper.GetRepositoryRoot());

        public void Update(List<string> pendingChanges, string solutionDir)
        {
            ViewModel.Root.Clear();
         
            if (!string.IsNullOrEmpty(solutionDir))
            {
                var root = new TSVNTreeViewFolderItem
                {
                    Label = $"Changes ({pendingChanges.Count})",
                    Foreground = ToBrush(EnvironmentColors.ToolWindowTextColorKey),
                    IsExpanded = true
                };

                var solutionDirItem = CreateFolderTreeViewItem(solutionDir, "S");

                foreach (var change in pendingChanges)
                {
                    ProcessChange(solutionDirItem, solutionDir, change);
                }

                root.Items.Add(solutionDirItem);

                ViewModel.Root.Add(root);

                commitButton.IsEnabled = true;
                revertButton.IsEnabled = true;
            }       
            else
            {
                commitButton.IsEnabled = false;
                revertButton.IsEnabled = false;
            }    
        }

        private void ProcessChange(TSVNTreeViewFolderItem root, string solutionDir, string change)
        {
            if (change.Length <= 8) return;

            var path = change.Substring(8);
            var pathParts = path.Split('\\');

            for (var i = 0; i < pathParts.Length; i++)
            {
                var item = FindItem(root, pathParts[i]);
                if (item == null)
                {
                    TSVNTreeViewItem newItem;
                    if (i == pathParts.Length - 1 && File.Exists(Path.Combine(solutionDir, path)))
                    {
                        newItem = CreateFileTreeViewItem(pathParts[i], solutionDir, path, change);
                    }
                    else
                    {
                        newItem = CreateFolderTreeViewItem(pathParts[i], change);
                    }
                        
                    root.Items.Add(newItem);

                    root = newItem as TSVNTreeViewFolderItem;
                }
                else if (item is TSVNTreeViewFolderItem folderItem)
                {
                    root = folderItem;
                }
            }
        }

        private TSVNTreeViewItem FindItem(TSVNTreeViewFolderItem root, string text)
        {
            if (root?.Items == null) return null;

            foreach (var item in root.Items)
            {
                if (item.Label.Equals(text))
                {
                    return item;
                }
            }

            return null;
        }

        private TSVNTreeViewItem CreateFileTreeViewItem(string text, string solutionDir, string path, string change)
        {
            var filePath = Path.Combine(solutionDir, path);

            var item = new TSVNTreeViewFileItem
            {
                Path = filePath,
                Tooltip = $"Name: {text}\nFolder: {Path.GetDirectoryName(filePath)}\nType: {GetTypeOfChange(change[0])}",
                Label = text,
                ChangeType = GetTypeOfChangeShort(change[0]),
                PendingIconSource = GetPendingIconOfChange(change[0]),
                PendingTooltip = GetPendingTooltipOfChange(change[0]),
                FileIconSource = ToImageSource(Icon.ExtractAssociatedIcon(filePath)),
                FileIconVisibility = File.Exists(filePath) ? Visibility.Visible : Visibility.Collapsed,
                IconVisibility = File.Exists(filePath) ? Visibility.Collapsed : Visibility.Visible,
                IconSource = "Resources\\XAML\\Document_16x.xaml",
                Foreground = ToBrush(EnvironmentColors.ToolWindowTextColorKey)
            };

            return item;
        }

        private string GetTypeOfChange(char change)
        {
            switch (change)
            {
                case 'A': return "Add";
                case 'D': return "Delete";
                case 'R': return "Replaced";
                case '!': return "Missing";
                case 'M': return "Modified";
                case '?': return "Unversioned";
                default: return string.Empty;
            }
        }

        private string GetTypeOfChangeShort(char change)
        {
            switch (change)
            {
                case 'A': return "[add]";
                case 'D': return "[del]";
                case 'R': return "[rep]";
                case '!': return "[mis]";
                case 'M': return "[mod]";
                case '?': return "[unv]";
                default: return string.Empty;
            }
        }

        private string GetPendingIconOfChange(char change)
        {
            switch (change)
            {
                case 'A': return "Resources\\XAML\\PendingAdd.xaml";
                case 'D': return "Resources\\XAML\\PendingDelete.xaml";
                case 'M': return "Resources\\XAML\\PendingEdit.xaml";
                default: return "Resources\\XAML\\CheckedIn.xaml";
            }
        }

        private string GetPendingTooltipOfChange(char change)
        {
            switch (change)
            {
                case 'A': return "Pending add";
                case 'D': return "Pending delete";
                case 'M': return "Pending edit";
                default: return "Checked in";
            }
        }

        private TSVNTreeViewFolderItem CreateFolderTreeViewItem(string text, string change)
        {
            return new TSVNTreeViewFolderItem
            {
                Label = text,
                ChangeType = GetTypeOfChangeShort(change[0]),
                Foreground = new SolidColorBrush(Colors.Gray),
                IsExpanded = true,
                FileIconVisibility = Visibility.Collapsed,
                IconVisibility = Visibility.Visible
            };
        }

        /// <summary>
        /// Convert from VSTheme EnvironmentColor to a XAML SolidColorBrush
        /// </summary>
        /// <param name="key">VSTheme EnvironmentColor key</param>
        /// <returns>XAML SolidColorBrush</returns>
        private static SolidColorBrush ToBrush(ThemeResourceKey key)
        {
            var color = VSColorTheme.GetThemedColor(key);
            return new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        private static ImageSource ToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        private void commitButton_Click(object sender, RoutedEventArgs e) => _ = Commit();

        private async Task Commit() => await CommandHelper.Commit();

        private void revertButton_Click(object sender, RoutedEventArgs e) => _ = Revert();

        private async Task Revert() => await CommandHelper.Revert();

        private void refreshButton_Click(object sender, RoutedEventArgs e) => _ = Refresh();

        private async Task Refresh() => Update(CommandHelper.GetPendingChanges(), await CommandHelper.GetRepositoryRoot());

        private void HideUnversionedButton_OnChecked(object sender, RoutedEventArgs e) => _ = ToggleUnversioned(true);

        private void HideUnversionedButton_OnUnchecked(object sender, RoutedEventArgs e) => _ = ToggleUnversioned(false);

        private async Task ToggleUnversioned(bool hide)
        {
            Settings.Default.HideUnversioned = hide;
            Settings.Default.Save();
            Update(CommandHelper.GetPendingChanges(), await CommandHelper.GetRepositoryRoot());
        }

        private void TreeView_Collapsed(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TreeViewItem item &&
                item.DataContext is TSVNTreeViewFolderItem tsvnFolderItem)
            {
                tsvnFolderItem.IconSource = "Resources\\XAML\\Folder_16x.xaml";
            }
        }

        private void TreeView_Expanded(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TreeViewItem item &&
                item.DataContext is TSVNTreeViewFolderItem tsvnFolderItem)
            {
                tsvnFolderItem.IconSource = "Resources\\XAML\\FolderOpen_16x.xaml";
            }
        }

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            var filePath = ((e.OriginalSource as FrameworkElement).DataContext as TSVNTreeViewItem)?.Path;
            _ = FileHelper.OpenFile(filePath);
        }

        private void Commit_Click(object sender, RoutedEventArgs e)
        {
            var filePath = ((e.OriginalSource as MenuItem).DataContext as TSVNTreeViewItem).Path;
            CommandHelper.Commit(filePath).FireAndForget();
        }

        private void Revert_Click(object sender, RoutedEventArgs e)
        {
            var filePath = ((e.OriginalSource as MenuItem).DataContext as TSVNTreeViewItem).Path;
            CommandHelper.Revert(filePath).FireAndForget();
        }

        private void ShowDifferences_Click(object sender, RoutedEventArgs e)
        {
            var filePath = ((e.OriginalSource as MenuItem).DataContext as TSVNTreeViewItem).Path;
            CommandHelper.ShowDifferences(filePath).FireAndForget();
        }
    }
}