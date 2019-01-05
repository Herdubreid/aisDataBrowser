using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows;

namespace Celin
{
    /// <summary>
    /// Interaction logic for ConnectionDlg.xaml
    /// </summary>
    public partial class ConnectionDlg : ReactiveUserControl<ConnectionVM>
    {
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
                this.OneWayBind(ViewModel,
                    m => m.NewConnection,
                    v => v.BaseUrl.IsEnabled)
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
