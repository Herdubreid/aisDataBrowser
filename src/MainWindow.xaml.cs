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
using System.Threading;

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

            // Insert the Default Tab
            /*ViewModel.Tabs.Add(new HeaderedItemViewModel()
            {
                Header = "Default",
                Content = new DataCtrl()
            });*/

            this.WhenActivated(d =>
            {
                ViewModel = MainVM.Instance;

                this.Bind(ViewModel,
                    m => m.InterTabClient,
                    v => v.TabController.InterTabClient)
                    .DisposeWith(d);
                this.OneWayBind(ViewModel,
                    m => m.Connections,
                    v => v.Connections.ItemsSource)
                    .DisposeWith(d);
                this.OneWayBind(ViewModel,
                    m => m.MaxReturnRowsItems,
                    v => v.MaxReturnRows.ItemsSource)
                    .DisposeWith(d);
                this.OneWayBind(ViewModel,
                    m => m.Tabs,
                    v => v.TabContainer.ItemsSource)
                    .DisposeWith(d);
                this.Bind(ViewModel,
                    m => m.ActiveConnection,
                    v => v.Connections.SelectedItem)
                    .DisposeWith(d);
                this.Bind(ViewModel,
                    m => m.MaxReturnRows,
                    v => v.MaxReturnRows.SelectedIndex)
                    .DisposeWith(d);
                this.Bind(ViewModel,
                    m => m.SelectedTab,
                    v => v.TabContainer.SelectedItem)
                    .DisposeWith(d);

                // Commands
                this.BindCommand(ViewModel,
                    m => m.NewDocument,
                    v => v.NewDocument)
                    .DisposeWith(d);
                this.BindCommand(ViewModel,
                    m => m.OpenDocument,
                    v => v.OpenDocument)
                    .DisposeWith(d);
                this.BindCommand(ViewModel,
                    m => m.SaveDocument,
                    v => v.SaveDocument)
                    .DisposeWith(d);
                this.BindCommand(ViewModel,
                    m => m.Connect,
                    v => v.Connect)
                    .DisposeWith(d);
                this.BindCommand(ViewModel,
                    m => m.AddConnection,
                    v => v.AddConnection)
                    .DisposeWith(d);
                this.BindCommand(ViewModel,
                    m => m.DeleteConnection,
                    v => v.DeleteConnection)
                    .DisposeWith(d);

                // Key Binding
                this.InputBindings.Add(new KeyBinding(ViewModel.NewDocument, Key.N, ModifierKeys.Control));
                this.InputBindings.Add(new KeyBinding(ViewModel.OpenDocument, Key.O, ModifierKeys.Control));
                this.InputBindings.Add(new KeyBinding(ViewModel.SaveDocument, Key.S, ModifierKeys.Control));

                // Insert the Default Tab
                ViewModel.Tabs.Add(new TabCtrl()
                {
                    Header = "Default",
                    Content = new DataCtrl()
                });

                ViewModel.SelectedTab = ViewModel.Tabs.First();
            });
        }
    }
}
