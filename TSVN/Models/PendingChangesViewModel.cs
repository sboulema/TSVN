using Caliburn.Micro;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace SamirBoulema.TSVN.Models
{
    public class PendingChangesViewModel : PropertyChangedBase
    {
        public PendingChangesViewModel()
        {
            _root = new ObservableCollection<TSVNTreeViewItem>();
        }

        private ObservableCollection<TSVNTreeViewItem> _root;
        public ObservableCollection<TSVNTreeViewItem> Root
        {
            get
            {
                return _root;
            }
            set
            {
                _root = value;
                NotifyOfPropertyChange();
            }
        }
    }

    public class TSVNTreeViewFileItem : TSVNTreeViewItem
    {
        public string Tooltip { get; set; }
    }

    public class TSVNTreeViewFolderItem : TSVNTreeViewItem
    {
        public TSVNTreeViewFolderItem()
        {
            _items = new ObservableCollection<TSVNTreeViewItem>();
        }

        private ObservableCollection<TSVNTreeViewItem> _items;
        public ObservableCollection<TSVNTreeViewItem> Items
        {
            get
            {
                return _items;
            }
            set
            {
                _items = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsExpanded { get; set; }
    }

    public class TSVNTreeViewItem : PropertyChangedBase
    {
        public string Label { get; set; }
        public string Path { get; set; }
        public SolidColorBrush Foreground { get; set; }
        public string ChangeType { get; set; }

        private ImageSource _imageSource;
        public ImageSource ImageSource
        {
            get
            {
                return _imageSource;
            }
            set
            {
                _imageSource = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
