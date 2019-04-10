using Caliburn.Micro;
using System.Collections.ObjectModel;
using System.Windows;
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

        private ImageSource _fileIconSource;
        public ImageSource FileIconSource
        {
            get
            {
                return _fileIconSource;
            }
            set
            {
                _fileIconSource = value;
                NotifyOfPropertyChange();
            }
        }

        private string _pendingIconSource;
        public string PendingIconSource
        {
            get
            {
                return _pendingIconSource;
            }
            set
            {
                _pendingIconSource = value;
                NotifyOfPropertyChange();
            }
        }

        public string PendingTooltip { get; set; }
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
    }

    public class TSVNTreeViewItem : PropertyChangedBase
    {
        public string Label { get; set; }
        public string Path { get; set; }
        public SolidColorBrush Foreground { get; set; }
        public string ChangeType { get; set; }

        private string _iconSource;
        public string IconSource
        {
            get
            {
                return _iconSource;
            }
            set
            {
                _iconSource = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _fileIconVisibility;
        public Visibility FileIconVisibility
        {
            get
            {
                return _fileIconVisibility;
            }
            set
            {
                _fileIconVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _iconVisibility;
        public Visibility IconVisibility
        {
            get
            {
                return _iconVisibility;
            }
            set
            {
                _iconVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                _isExpanded = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
