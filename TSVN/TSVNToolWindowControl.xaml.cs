using SamirBoulema.TSVN.Properties;

namespace SamirBoulema.TSVN
{
    using EnvDTE;
    using Helpers;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Image = System.Windows.Controls.Image;

    /// <summary>
    /// Interaction logic for TSVNToolWindowControl.
    /// </summary>
    public partial class TSVNToolWindowControl
    {
        private DTE _dte;
        private CommandHelper _commandHelper;
        private FileHelper _fileHelper;
        private readonly ContextMenu _contextMenu;

        /// <summary>
        /// Initializes a new instance of the <see cref="TSVNToolWindowControl"/> class.
        /// </summary>
        public TSVNToolWindowControl()
        {
            InitializeComponent();
            _contextMenu = CreateContextMenu();
        }

        public void Update(List<string> pendingChanges, string solutionDir)
        {
            treeView.Items.Clear();

            var items = treeView.Items;
         
            if (!string.IsNullOrEmpty(solutionDir))
            {
                var root = new TreeViewItem { Header = new Label { Content = $"Changes ({pendingChanges.Count})" },
                    IsExpanded = true, FontWeight = FontWeights.Bold, Margin = new Thickness(0,0,0,10) };
                var solutionDirItem = CreateFolderTreeViewItem(solutionDir, "S", false);

                foreach (var change in pendingChanges)
                {
                    ProcessChange(solutionDirItem, solutionDir, change);
                }

                root.Items.Add(solutionDirItem);

                items.Add(root);

                commitButton.IsEnabled = true;
                revertButton.IsEnabled = true;
            }       
            else
            {
                commitButton.IsEnabled = false;
                revertButton.IsEnabled = false;
            }    
        }

        private void ProcessChange(TreeViewItem root, string solutionDir, string change)
        {
            var path = change.Substring(8);
            var pathParts = path.Split('\\');

            for (int i = 0; i < pathParts.Length; i++)
            {
                var item = FindItem(root, pathParts[i]);
                if (item == null)
                {
                    TreeViewItem newItem;
                    if (i == pathParts.Length - 1 && Directory.Exists(path))
                    {
                        newItem = CreateFileTreeViewItem(pathParts[i], solutionDir, path, change);
                    }
                    else
                    {
                        newItem = CreateFolderTreeViewItem(pathParts[i], change,  i == pathParts.Length - 1);
                    }
                        
                    root.Items.Add(newItem);
                    root = newItem;
                }
                else
                {
                    root = item;
                }
            }
        }

        private TreeViewItem FindItem(TreeViewItem root, string text)
        {
            foreach (TreeViewItem item in root.Items)
            {
                if (item.Uid.Equals(text))
                {
                    return item;
                }
            }

            return null;
        }

        private TSVNTreeViewItem CreateFileTreeViewItem(string text, string solutionDir, string path, string change)
        {
            var item = new TSVNTreeViewItem();
            item.IsExpanded = false;
            item.FontWeight = FontWeights.Normal;
            item.Path = Path.Combine(solutionDir, path);
            item.MouseDoubleClick += Item_MouseDoubleClick;
            item.Padding = new Thickness(-3);

            // create Tooltip
            item.ToolTip = $"Name: {text}\nFolder: {Path.GetDirectoryName(item.Path)}\nType: {GetTypeOfChange(change[0])}";

            // create stack panel
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            // create Image
            var image = new Image
            {
                Source =
                    File.Exists(item.Path)
                        ? ToImageSource(Icon.ExtractAssociatedIcon(item.Path))
                        : new BitmapImage(new Uri("Resources\\Document_16x.png", UriKind.Relative)),
                Width = 16,
                Height = 16
            };

            // Label
            Label lbl = new Label();
            lbl.Content = text;

            // Add into stack
            stack.Children.Add(image);
            stack.Children.Add(lbl);

            var typeOfChangeShort = GetTypeOfChangeShort(change[0]);
            if (!string.IsNullOrEmpty(typeOfChangeShort))
            {
                Label lblChange = new Label();
                lblChange.Content = typeOfChangeShort;
                lblChange.Foreground = new SolidColorBrush(Colors.Gray);
                stack.Children.Add(lblChange);
            }

            // assign stack to header
            item.Header = stack;

            item.ContextMenu = _contextMenu;

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
                case '?': return "[unv]";
                default: return string.Empty;
            }
        }

        private void Item_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _dte.ExecuteCommand("File.OpenFile", ((TSVNTreeViewItem)sender).Path);
            e.Handled = true;
        }

        private TreeViewItem CreateFolderTreeViewItem(string text, string change, bool lastItem)
        {
            TreeViewItem item = new TreeViewItem();
            item.IsExpanded = true;
            item.FontWeight = FontWeights.Normal;
            item.Uid = text;
            item.Padding = new Thickness(-3);

            // Events
            item.Collapsed += Item_Collapsed;
            item.Expanded += Item_Expanded;

            // create stack panel
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            // create Image
            Image image = new Image();
            image.Source = new BitmapImage(new Uri("Resources\\FolderOpen_16x.png", UriKind.Relative));
            image.Width = 16;
            image.Height = 16;

            // Label
            Label lbl = new Label();
            lbl.Content = text;
            lbl.Foreground = new SolidColorBrush(Colors.Gray);

            // Add into stack
            stack.Children.Add(image);
            stack.Children.Add(lbl);

            var typeOfChangeShort = GetTypeOfChangeShort(change[0]);
            if (!string.IsNullOrEmpty(typeOfChangeShort) && lastItem)
            {
                Label lblChange = new Label();
                lblChange.Content = typeOfChangeShort;
                lblChange.Foreground = new SolidColorBrush(Colors.Gray);
                stack.Children.Add(lblChange);
            }

            // assign stack to header
            item.Header = stack;

            return item;
        }

        private void Item_Collapsed(object sender, RoutedEventArgs e)
        {
            var folderItem = sender as TreeViewItem;
            var folderItemImage = ((Image)((StackPanel)folderItem.Header).Children[0]);

            folderItemImage.Source = new BitmapImage(new Uri("Resources\\Folder_16x.png", UriKind.Relative));
            e.Handled = true;
        }

        private void Item_Expanded(object sender, RoutedEventArgs e)
        {
            var folderItem = sender as TreeViewItem;
            var folderItemImage = ((Image)((StackPanel)folderItem.Header).Children[0]);

            folderItemImage.Source = new BitmapImage(new Uri("Resources\\FolderOpen_16x.png", UriKind.Relative));
            e.Handled = true;
        }

        private static ImageSource ToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        public void SetDTE(DTE dte)
        {
            _dte = dte;
            _commandHelper = new CommandHelper(dte);
            _fileHelper = new FileHelper(dte);
        }

        private void commitButton_Click(object sender, RoutedEventArgs e)
        {
            _commandHelper.Commit();
        }

        private void revertButton_Click(object sender, RoutedEventArgs e)
        {
            _commandHelper.Revert();
        }

        private ContextMenu CreateContextMenu()
        {
            var menu = new ContextMenu();
            var commitItem = new MenuItem() { Header = "Commit" };
            commitItem.Click += CommitItem_Click;
            menu.Items.Add(commitItem);
            var revertItem = new MenuItem() { Header = "Revert" };
            revertItem.Click += RevertItem_Click;
            menu.Items.Add(revertItem);
            return menu;
        }

        private void CommitItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem contextMenuItem = (MenuItem)sender;
            ContextMenu contextMenu = (ContextMenu)contextMenuItem.Parent;
            if (contextMenu.PlacementTarget.GetType() == typeof(TSVNTreeViewItem))
            {
                TSVNTreeViewItem originatingTreeViewItem = (TSVNTreeViewItem)contextMenu.PlacementTarget;
                _commandHelper.Commit(originatingTreeViewItem.Path);
            }
        }

        private void RevertItem_Click(object sender, RoutedEventArgs e)
        {
            var contextMenuItem = (MenuItem)sender;
            var contextMenu = (ContextMenu)contextMenuItem.Parent;
            if (contextMenu.PlacementTarget.GetType() == typeof(TSVNTreeViewItem))
            {
                TSVNTreeViewItem originatingTreeViewItem = (TSVNTreeViewItem)contextMenu.PlacementTarget;
                _commandHelper.Revert(originatingTreeViewItem.Path);
            }
        }

        private void HideUnversionedButton_OnChecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.HideUnversioned = true;
            Settings.Default.Save();
            Update(_commandHelper.GetPendingChanges(), _fileHelper.GetSolutionDir());
            HideUnversionedButtonBorder.BorderThickness = new Thickness(1);
        }

        private void HideUnversionedButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.HideUnversioned = false;
            Settings.Default.Save();
            Update(_commandHelper.GetPendingChanges(), _fileHelper.GetSolutionDir());
            HideUnversionedButtonBorder.BorderThickness = new Thickness(0);
        }
    }
}