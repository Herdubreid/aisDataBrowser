using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reactive.Disposables;
using ReactiveUI;

namespace Celin
{
    /// <summary>
    /// Interaction logic for DataCtrl.xaml
    /// </summary>
    public partial class DataCtrl : UserControl, IViewFor<DataVM>
    {
        public DataVM ViewModel { get; set; }
        object IViewFor.ViewModel { get => ViewModel; set => throw new NotImplementedException(); }

        public DataCtrl()
        {
            InitializeComponent();
            ViewModel = new DataVM();

            this.WhenActivated(d =>
            {
                DataContext = ViewModel;
                // Commands
                this.BindCommand(ViewModel,
                    m => m.Submit,
                    v => v.Submit)
                    .DisposeWith(d);
            });
        }
    }
}
