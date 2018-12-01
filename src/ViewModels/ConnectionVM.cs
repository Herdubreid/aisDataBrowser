using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Security;
using System.ComponentModel;
using ReactiveUI;
using PropertyChanged;
using MaterialDesignThemes.Wpf;

namespace Celin
{
    [AddINotifyPropertyChangedInterface]
    public class ConnectionVM : IDataErrorInfo
    {
        Connection _edit = null;
        bool _exist = false;
        CancellationTokenSource cancelRequest = new CancellationTokenSource();
        public ReactiveCommand<PasswordBox, Task> Connect { get; }
        public ReactiveCommand<Unit, Unit> CancelRequest { get; }
        public string Id { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public bool NewConnection { get => _edit is null; }
        public bool CanConnect { get; private set; } = false;
        public bool Busy { get; set; } = false;

        public string Error => throw new NotImplementedException();

        public string this[string columnName]
        {
            get
            {
                string val = string.Empty;
                switch (columnName)
                {
                    case "Id":
                        if (_edit is null && !string.IsNullOrWhiteSpace(Id)
                        && MainVM.Instance.Connections.Any(c => c.Id.Equals(Id)))
                        {
                            _exist = true;
                            return string.Format("Connection {0} already exists!", Id);
                        }
                        _exist = false;
                        val = Id;
                        break;
                    case "BaseUrl":
                        val = BaseUrl;
                        break;
                    case "User":
                        val = User;
                        break;
                }
                CanConnect =
                    !string.IsNullOrWhiteSpace(Id) &&
                    !string.IsNullOrWhiteSpace(BaseUrl) &&
                    !string.IsNullOrWhiteSpace(User) &&
                    !_exist;

                return string.IsNullOrWhiteSpace(val)
                    ? string.Format("{0} is required!", columnName)
                    : null;
            }
        }

        public ConnectionVM(Connection connection) : this()
        {
            _edit = connection;
            Id = _edit.Id;
            BaseUrl = _edit.Server.BaseUrl;
            User = _edit.Server.AuthRequest.username;
        }

        public ConnectionVM()
        {
            var canConnect = this.WhenAnyValue(
                m => m.Id, m => m.BaseUrl, m => m.User,
                (id, baseUrl, user) =>
                !string.IsNullOrWhiteSpace(id) &&
                !string.IsNullOrWhiteSpace(baseUrl) &&
                !string.IsNullOrWhiteSpace(user))
                .DistinctUntilChanged();

            Connect = ReactiveCommand.Create<PasswordBox, Task>(async pb =>
            {
                Busy = true;
                var host = _edit is null ? new Connection(Id, BaseUrl) : _edit;
                host.Server.AuthRequest.username = User;
                host.Server.AuthRequest.password = pb.Password;
                host.Server.AuthRequest.requiredCapabilities = "dataServiceAggregation";
                try
                {
                    await host.Server.AuthenticateAsync(cancelRequest);
                    DialogHost.CloseDialogCommand.Execute(_edit, null);
                }
                catch (Exception)
                {
                }
                Busy = false;
            });
            CancelRequest = ReactiveCommand.Create(() =>
            {
                cancelRequest.Cancel();
            });
        }
    }
}
