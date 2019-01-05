using MaterialDesignThemes.Wpf;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Celin
{
    public class ConnectionVM : ReactiveObject, IDataErrorInfo
    {
        Connection _edit = null;
        bool _exist = false;
        CancellationTokenSource cancelRequest { get; set; }
        [Reactive]public string Id { get; set; }
        [Reactive]public string BaseUrl { get; set; }
        [Reactive]public string User { get; set; }
        [Reactive]public string Msg { get; set; }
        [Reactive]public bool CanConnect { get; set; }
        [Reactive]public bool Busy { get; set; }
        public bool NewConnection { get => _edit is null; }
        public ReactiveCommand<PasswordBox, Task> Connect { get; }
        public ReactiveCommand<Unit, Unit> CancelRequest { get; }

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
            Connect = ReactiveCommand.Create<PasswordBox, Task>(async pb =>
            {
                Busy = true;
                Msg = string.Empty;
                var host = _edit is null ? new Connection(Id, BaseUrl) : _edit;
                host.Server.AuthRequest.username = User;
                host.Server.AuthRequest.password = pb.Password;
                host.Server.AuthRequest.requiredCapabilities = "dataServiceAggregation";
                try
                {
                    cancelRequest = new CancellationTokenSource();
                    await host.Server.AuthenticateAsync(cancelRequest);
                    DialogHost.CloseDialogCommand.Execute(host, null);
                }
                catch (Exception e)
                {
                    Msg = e.Message;
                }
                finally
                {
                    Busy = false;
                }
            });
            CancelRequest = ReactiveCommand.Create(() =>
            {
                cancelRequest.Cancel();
            });
        }
    }
}
