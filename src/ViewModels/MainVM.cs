using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Controls;
using PropertyChanged;
using ReactiveUI;
using Dragablz;
using MaterialDesignThemes.Wpf;
using System.Windows;

namespace Celin
{
    [AddINotifyPropertyChangedInterface]
    public class MainVM
    {
        string Folder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Celin", "aisDataBrowser");
        readonly string sfname = "server.ctx";
        public static MainVM Instance { get; } = new MainVM();
        public ReactiveCommand<Unit, Unit> NewDocument { get; }
        public ReactiveCommand<Unit, Task> AddConnection { get; }
        public ReactiveCommand<Unit, Task> EditConnection { get; }
        public ReactiveCommand<Unit, Task> DeleteConnection { get; }
        public ObservableCollection<HeaderedItemViewModel> Tabs { get; } = new ObservableCollection<HeaderedItemViewModel>();
        public ObservableCollection<Connection> Connections { get; }
        public Connection ActiveConnection { get; set; }
        public MainVM()
        {
            Connections = new ObservableCollection<Connection>(Connection.Load(Path.Combine(Folder, sfname)));
            ActiveConnection = Connections.Any() ? Connections.First() : null;
            Tabs.Add(new HeaderedItemViewModel()
            {
                Header = "New Tab",
                Content = new DataCtrl()
            });
            // Commands
            NewDocument = ReactiveCommand.Create(() =>
            {
                Tabs.Add(new HeaderedItemViewModel()
                {
                    Header = string.Format("New Tab ({0})", Tabs.Count),
                    Content = new DataCtrl()
                });
            });
            AddConnection = ReactiveCommand.Create(async () =>
            {
                var dlg = new ConnectionDlg();
                var result = await DialogHost.Show(dlg);
                if (result is Connection)
                {
                    Connections.Add(result as Connection);
                }
            });
            EditConnection = ReactiveCommand.Create(async () =>
            {
                var dlg = new ConnectionDlg(ActiveConnection);
                var result = await DialogHost.Show(dlg);
            },
            this.WhenAny(m => m.ActiveConnection, v => v.Value != null));
            DeleteConnection = ReactiveCommand.Create(async () =>
            {
                var dlg = new ConfirmDlg(string.Format("Are you sure you want to delete '{0}'?", ActiveConnection.Id), "Delete");
                var result = (bool)await DialogHost.Show(dlg);
                if (result)
                {
                    Connections.Remove(ActiveConnection);
                }
            },
            this.WhenAny(m => m.ActiveConnection, v => v.Value != null));
        }
        ~MainVM()
        {
            if (!Directory.Exists(Folder)) Directory.CreateDirectory(Folder);
            Connection.Save(Connections, Path.Combine(Folder, sfname));
        }
    }
}
