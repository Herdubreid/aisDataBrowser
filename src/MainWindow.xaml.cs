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
using Dragablz;

namespace Celin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ReactiveWindow<MainVM>
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = MainVM.Instance;
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel,
                    m => m.Connections,
                    v => v.Connections.ItemsSource)
                    .DisposeWith(d);
                this.OneWayBind(ViewModel,
                    m => m.Tabs,
                    v => v.TabContainer.ItemsSource)
                    .DisposeWith(d);
                this.Bind(ViewModel,
                    m => m.ActiveConnection,
                    v => v.Connections.SelectedItem)
                    .DisposeWith(d);
                // Commands
                this.BindCommand(ViewModel,
                    m => m.NewDocument,
                    v => v.NewDocument)
                    .DisposeWith(d);
                this.BindCommand(ViewModel,
                    m => m.AddConnection,
                    v => v.AddConnection)
                    .DisposeWith(d);
                this.BindCommand(ViewModel,
                    m => m.EditConnection,
                    v => v.EditConnection)
                    .DisposeWith(d);
                this.BindCommand(ViewModel,
                    m => m.DeleteConnection,
                    v => v.DeleteConnection)
                    .DisposeWith(d);
            });
        }
    }
}
