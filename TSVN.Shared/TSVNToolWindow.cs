using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Community.VisualStudio.Toolkit;
using System;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Imaging;

namespace SamirBoulema.TSVN
{
    public class TSVNToolWindow : BaseToolWindow<TSVNToolWindow>
    {
        public override string GetTitle(int toolWindowId)
            => "TSVN Pending Changes";

        public override Type PaneType => typeof(Pane);

        public override async Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
            return new TSVNToolWindowControl();
        }

        [Guid("81a57ae8-6550-4dd0-940c-503d379550cc")]
        internal class Pane : ToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.StatusInformation;
            }
        }
    }
}
