using ExShrinkSidebar.UI.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExShrinkSidebar.UI.ViewModels.MainWindow
{
    public class MainViewModel : ViewModelBase
    {
        //private string _title;
        //private DockEdge _currentEdge;
        //private bool _isSidebarVisible;

        //public string Title
        //{
        //    get => _title;
        //    set => SetProperty(ref _title, value);
        //}

        //public DockEdge CurrentEdge
        //{
        //    get => _currentEdge;
        //    set => SetProperty(ref _currentEdge, value);
        //}

        //public bool IsSidebarVisible
        //{
        //    get => _isSidebarVisible;
        //    set => SetProperty(ref _isSidebarVisible, value);
        //}

        public override void Cleanup()
        {
            base.Cleanup();
        }
    }
}
