using Dragablz;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Utils;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Celin
{
    public class DataSource
    {
        public string ObjectName { get; set; }
        public string Description { get; set; }
        public override string ToString()
        {
            return $"{ObjectName} {Description}";
        }
    }
    public class MainVM : ReactiveObject
    {
        static readonly string FILE_FILTER = "Celin's Query Language (*.cql)|*.cql|All Files (*.*)|*.*";

        string Folder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Celin", "aisDataBrowser");
        readonly string sfname = "server.ctx";
        public static MainVM Instance { get; } = new MainVM();
        [Reactive] public int MaxReturnRows { get; set; } = 2;
        [Reactive] public Connection ActiveConnection { get; set; }
        [Reactive] public HeaderedItemViewModel SelectedTab { get; set; }
        public IInterTabClient InterTabClient { get; } = new MainInterTabClient();
        readonly ObservableAsPropertyHelper<AIS.AuthResponse> authResponse;
        public AIS.AuthResponse AuthResponse { get => authResponse.Value; }
        public ReactiveCommand<Unit, Unit> NewDocument { get; }
        public ReactiveCommand<Unit, Unit> OpenDocument { get; }
        public ReactiveCommand<Unit, Unit> SaveDocument { get; }
        public ReactiveCommand<Unit, Task> Connect { get; }
        public ReactiveCommand<Unit, Task> AddConnection { get; }
        public ReactiveCommand<Unit, Task> EditConnection { get; }
        public ReactiveCommand<Unit, Task> DeleteConnection { get; }
        public ObservableCollection<Connection> Connections { get; }
        public IEnumerable<string> MaxReturnRowsItems { get; } = new string[] { "10", "100", "500", "1000", "5000", "10000" };
        public ObservableCollection<HeaderedItemViewModel> Tabs { get; } = new ObservableCollection<HeaderedItemViewModel>();
        public IObservable<bool> IsConnected { get; set; }

        async void GetDataSources()
        {
            var f9860 = await ActiveConnection.Server.RequestAsync<F9860.Response>(new F9860.Request());
            /*DataSources = f9860.fs_DATABROWSE_F9860.data.gridData.rowset.Select(r =>
            {
                return new DataSource()
                {
                    ObjectName = r.F9860_OBNM,
                    Description = r.F9860_MD
                };
            });*/
        }
        public MainVM()
        {
            Connections = new ObservableCollection<Connection>(Connection.Load(Path.Combine(Folder, sfname)));
            ActiveConnection = Connections.Any() ? Connections.First() : null;

            var connectionActive = this.WhenAny(m => m.ActiveConnection, v => v.Value != null);

            authResponse = this
                .WhenAnyValue(m => m.ActiveConnection.Server.AuthResponse)
                .ToProperty(this, nameof(AuthResponse));

            IsConnected = this
                .WhenAnyValue(m => m.AuthResponse)
                .Select(a =>
                {
                    return a != null;
                });

            // Commands
            NewDocument = ReactiveCommand.Create(() =>
            {
                Tabs.Add(new HeaderedItemViewModel()
                {
                    Header = string.Format("New Tab ({0})", Tabs.Count),
                    Content = new DataCtrl()
                });

                SelectedTab = Tabs.Last();
            });

            OpenDocument = ReactiveCommand.Create(() =>
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = FILE_FILTER;
                if (dlg.ShowDialog() == true)
                {
                    var content = new DataCtrl();
                    if (content.Load(dlg.FileName))
                    {
                        Tabs.Add(new HeaderedItemViewModel()
                        {
                            Header = dlg.SafeFileName,
                            Content = content
                        });

                        SelectedTab = Tabs.Last();
                    }
                }
            });

            SaveDocument = ReactiveCommand.Create(() =>
            {
                var content = (SelectedTab as HeaderedItemViewModel).Content as DataCtrl;
                if (content.Editor.Document.FileName is null)
                {
                    SaveFileDialog dlg = new SaveFileDialog();
                    dlg.Filter = FILE_FILTER;
                    if (dlg.ShowDialog() == true)
                    {
                        if (content.Save(dlg.FileName))
                        {
                            SelectedTab.Header = dlg.SafeFileName;
                        }
                    }
                }
                else
                {
                    content.Save(content.Editor.Document.FileName);
                }
            });

            Connect = ReactiveCommand.Create(async () =>
            {
                var dlg = new ConnectionDlg(ActiveConnection);
                var result = await DialogHost.Show(dlg);
                this.RaisePropertyChanged("ActiveConnection");
            },
            this.WhenAny(m => m.ActiveConnection, v => v.Value != null));
            AddConnection = ReactiveCommand.Create(async () =>
            {
                var dlg = new ConnectionDlg();
                var result = await DialogHost.Show(dlg);
                if (result is Connection)
                {
                    Connections.Add(result as Connection);
                    ActiveConnection = result as Connection;
                }
            });

            DeleteConnection = ReactiveCommand.Create(async () =>
            {
                var dlg = new ConfirmDlg(string.Format("Are you sure you want to delete '{0}'?", ActiveConnection.Id), "Delete");
                var result = (bool)await DialogHost.Show(dlg);
                if (result)
                {
                    Connections.Remove(ActiveConnection);
                    ActiveConnection = Connections.Any() ? Connections.First() : null;
                }
            },
            connectionActive);
        }
        ~MainVM()
        {
            if (!Directory.Exists(Folder)) Directory.CreateDirectory(Folder);
            Connection.Save(Connections, Path.Combine(Folder, sfname));
        }
    }
}
