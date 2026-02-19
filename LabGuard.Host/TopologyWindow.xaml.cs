using System.Windows;

namespace LabGuard.Host
{
    public partial class TopologyWindow : Window
    {
        public TopologyWindow(HostListener listener)
        {
            InitializeComponent();
            TopologyViewControl.Initialize(listener);
        }
    }
}
