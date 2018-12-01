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
using System.Reactive.Linq;
using System.Reactive.Disposables;
using ReactiveUI;
using MaterialDesignThemes.Wpf;

namespace Celin
{
    /// <summary>
    /// Interaction logic for ConnectionDlg.xaml
    /// </summary>
    public partial class ConnectionDlg : UserControl, IViewFor<ConnectionVM>
    {
        object IViewFor.ViewModel { get => ViewModel; set => throw new NotImplementedException(); }
        public ConnectionVM ViewModel { get; set; }

        public void InitVM(ConnectionVM vm)
        {
            ViewModel = vm;
            this.WhenActivated(d =>
            {
                // Visibility Form/Progress
                this.OneWayBind(ViewModel,
                    m => m.Busy,
                    v => v.Progress.Visibility,
                    b => b ? Visibility.Visible : Visibility.Collapsed)
                    .DisposeWith(d);
                this.OneWayBind(ViewModel,
                    m => m.Busy,
                    v => v.Form.Visibility,
                    b => b ? Visibility.Collapsed : Visibility.Visible)
                    .DisposeWith(d);

                // Readonly New/Edit
                this.OneWayBind(ViewModel,
                    m => m.NewConnection,
                    v => v.Id.IsEnabled)
                    .DisposeWith(d);

                // Fields use xaml binding
                DataContext = ViewModel;

                // Commands
                this.BindCommand(ViewModel,
                    m => m.Connect,
                    v => v.Connect)
                    .DisposeWith(d);
                this.BindCommand(ViewModel,
                    m => m.CancelRequest,
                    v => v.CancelRequest)
                    .DisposeWith(d);
            });
        }
        public ConnectionDlg(Connection connection)
        {
            InitializeComponent();
            InitVM(new ConnectionVM(connection));
        }
        public ConnectionDlg()
        {
            InitializeComponent();
            InitVM(new ConnectionVM());
        }
    }
}
