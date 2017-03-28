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
            var item = new TSVNTreeViewItem
            {
                IsExpanded = false,
                FontWeight = FontWeights.Normal,
                Path = Path.Combine(solutionDir, path),
                Padding = new Thickness(-3)
            };
            item.MouseDoubleClick += Item_MouseDoubleClick;

            // create Tooltip
            item.ToolTip = $"Name: {text}\nFolder: {Path.GetDirectoryName(item.Path)}\nType: {GetTypeOfChange(change[0])}";

            // create stack panel
            var stack = new StackPanel { Orientation = Orientation.Horizontal };

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
            var lbl = new Label { Content = text };

            // Add into stack
            stack.Children.Add(image);
            stack.Children.Add(lbl);

            var typeOfChangeShort = GetTypeOfChangeShort(change[0]);
            if (!string.IsNullOrEmpty(typeOfChangeShort))
            {
                var lblChange = new Label
                {
                    Content = typeOfChangeShort,
                    Foreground = new SolidColorBrush(Colors.Gray)
                };
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
            var item = new TreeViewItem
            {
                IsExpanded = true,
                FontWeight = FontWeights.Normal,
                Uid = text,
                Padding = new Thickness(-3)
            };

            // Events
            item.Collapsed += Item_Collapsed;
            item.Expanded += Item_Expanded;

            // create stack panel
            var stack = new StackPanel {Orientation = Orientation.Horizontal};

            // create Image
            var image = new Image
            {
                Source = new BitmapImage(new Uri("Resources\\FolderOpen_16x.png", UriKind.Relative)),
                Width = 16,
                Height = 16
            };

            // Label
            var lbl = new Label
            {
                Content = text,
                Foreground = new SolidColorBrush(Colors.Gray)
            };

            // Add into stack
            stack.Children.Add(image);
            stack.Children.Add(lbl);

            var typeOfChangeShort = GetTypeOfChangeShort(change[0]);
            if (!string.IsNullOrEmpty(typeOfChangeShort) && lastItem)
            {
                var lblChange = new Label
                {
                    Content = typeOfChangeShort,
                    Foreground = new SolidColorBrush(Colors.Gray)
                };
                stack.Children.Add(lblChange);
            }

            // assign stack to header
            item.Header = stack;

            return item;
        }

        private static void Item_Collapsed(object sender, RoutedEventArgs e)
        {
            var folderItem = sender as TreeViewItem;
            var folderItemImage = (Image)((StackPanel)folderItem.Header).Children[0];

            folderItemImage.Source = new BitmapImage(new Uri("Resources\\Folder_16x.png", UriKind.Relative));
            e.Handled = true;
        }

        private static void Item_Expanded(object sender, RoutedEventArgs e)
        {
            var folderItem = sender as TreeViewItem;
            var folderItemImage = (Image)((StackPanel)folderItem.Header).Children[0];

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
        }

        private void commitButton_Click(object sender, RoutedEventArgs e)
        {
            CommandHelper.Commit();
        }

        private void revertButton_Click(object sender, RoutedEventArgs e)
        {
            CommandHelper.Revert();
        }

        private ContextMenu CreateContextMenu()
        {
            var menu = new ContextMenu();
            var commitItem = new MenuItem { Header = "Commit" };
            commitItem.Click += CommitItem_Click;
            menu.Items.Add(commitItem);
            var revertItem = new MenuItem { Header = "Revert" };
            revertItem.Click += RevertItem_Click;
            menu.Items.Add(revertItem);
            return menu;
        }

        private void CommitItem_Click(object sender, RoutedEventArgs e)
        {
            var contextMenuItem = (MenuItem)sender;
            var contextMenu = (ContextMenu)contextMenuItem.Parent;
            if (contextMenu.PlacementTarget.GetType() == typeof(TSVNTreeViewItem))
            {
                var originatingTreeViewItem = (TSVNTreeViewItem)contextMenu.PlacementTarget;
                CommandHelper.Commit(originatingTreeViewItem.Path);
            }
        }

        private void RevertItem_Click(object sender, RoutedEventArgs e)
        {
            var contextMenuItem = (MenuItem)sender;
            var contextMenu = (ContextMenu)contextMenuItem.Parent;
            if (contextMenu.PlacementTarget.GetType() == typeof(TSVNTreeViewItem))
            {
                var originatingTreeViewItem = (TSVNTreeViewItem)contextMenu.PlacementTarget;
                CommandHelper.Revert(originatingTreeViewItem.Path);
            }
        }

        private void HideUnversionedButton_OnChecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.HideUnversioned = true;
            Settings.Default.Save();
            Update(CommandHelper.GetPendingChanges(), CommandHelper.GetRepositoryRoot());
            HideUnversionedButtonBorder.BorderThickness = new Thickness(1);
        }

        private void HideUnversionedButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.HideUnversioned = false;
            Settings.Default.Save();
            Update(CommandHelper.GetPendingChanges(), CommandHelper.GetRepositoryRoot());
            HideUnversionedButtonBorder.BorderThickness = new Thickness(0);
        }
    }
}