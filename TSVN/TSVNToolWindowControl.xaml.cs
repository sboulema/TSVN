namespace SamirBoulema.TSVN
{
    using EnvDTE;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Interaction logic for TSVNToolWindowControl.
    /// </summary>
    public partial class TSVNToolWindowControl : UserControl
    {
        private DTE dte;

        /// <summary>
        /// Initializes a new instance of the <see cref="TSVNToolWindowControl"/> class.
        /// </summary>
        public TSVNToolWindowControl()
        {
            InitializeComponent();
        }

        public void Update(List<string> pendingChanges, string solutionDir)
        {
            treeView.Items.Clear();

            var items = treeView.Items;
         
            if (!string.IsNullOrEmpty(solutionDir))
            {
                var root = new TreeViewItem() { Header = $"Changes ({pendingChanges.Count})", IsExpanded = true, FontWeight = FontWeights.Bold, Margin = new Thickness(0,0,0,10) };
                var solutionDirItem = CreateFolderTreeViewItem(solutionDir);

                foreach (var change in pendingChanges)
                {
                    ProcessChange(solutionDirItem, solutionDir, change);
                }

                root.Items.Add(solutionDirItem);

                items.Add(root);
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
                    if (i == pathParts.Length - 1)
                    {
                        newItem = CreateFileTreeViewItem(pathParts[i], solutionDir, path);
                    }
                    else
                    {
                        newItem = CreateFolderTreeViewItem(pathParts[i]);
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

        private TSVNTreeViewItem CreateFileTreeViewItem(string text, string solutionDir, string path)
        {
            var item = new TSVNTreeViewItem();
            item.IsExpanded = false;
            item.FontWeight = FontWeights.Normal;
            item.Path = Path.Combine(solutionDir, path);
            item.MouseDoubleClick += Item_MouseDoubleClick;
            item.Padding = new Thickness(-3);

            // create stack panel
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            // create Image
            Image image = new Image();
            image.Source = ToImageSource(System.Drawing.Icon.ExtractAssociatedIcon(item.Path));
            image.Width = 16;
            image.Height = 16;

            // Label
            Label lbl = new Label();
            lbl.Content = text;

            // Add into stack
            stack.Children.Add(image);
            stack.Children.Add(lbl);

            //if (change[0].Equals('A'))
            //{
            //    Label lblChange = new Label();
            //    lblChange.Content = "[add]";
            //    lblChange.Foreground = new SolidColorBrush(Colors.Gray);
            //    stack.Children.Add(lblChange);
            //}

            // assign stack to header
            item.Header = stack;

            return item;
        }

        private void Item_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            dte.ExecuteCommand("File.OpenFile", ((TSVNTreeViewItem)sender).Path);
            e.Handled = true;
        }

        private TreeViewItem CreateFolderTreeViewItem(string text)
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

        private ImageSource ToImageSource(System.Drawing.Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        public void SetDTE(DTE dte)
        {
            this.dte = dte;
        }
    }
}