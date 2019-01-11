using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Utils;
using ReactiveUI;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Celin
{
    /// <summary>
    /// Interaction logic for DataCtrl.xaml
    /// </summary>
    public partial class DataCtrl : ReactiveUserControl<DataVM>
    {
        public bool Load(string FileName)
        {
            try
            {
                using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (StreamReader reader = FileReader.OpenStream(fs, System.Text.Encoding.Default))
                    {
                        ViewModel.Code = new TextDocument(reader.ReadToEnd());
                        ViewModel.Code.FileName = FileName;
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool Save(string FileName)
        {
            try
            {
                File.WriteAllText(FileName, ViewModel.Code.Text);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public DataCtrl()
        {
            InitializeComponent();
            ViewModel = new DataVM();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel,
                    m => m.Msg,
                    v => v.Msg.Text)
                    .DisposeWith(d);
                this.OneWayBind(ViewModel,
                    m => m.AvailableColumns,
                    v => v.AvailableColumns.ItemsSource)
                    .DisposeWith(d);
                this.Bind(ViewModel,
                    m => m.SelectedAlias,
                    v => v.AvailableColumns.SelectedItem)
                    .DisposeWith(d);
                this.Bind(ViewModel,
                    m => m.Code,
                    v => v.Editor.Document)
                    .DisposeWith(d);
                this.Bind(ViewModel,
                    m => m.SelectedTabIndex,
                    v => v.TabContainer.SelectedIndex)
                    .DisposeWith(d);

                /*ViewModel
                .WhenAnyValue(m => m.SelectedAlias)
                .Where(c => c != null)
                .Subscribe(c =>
                {
                    Editor.Document.Insert(Editor.CaretOffset, c.Alias);
                    Editor.Focus();
                });*/

                ViewModel
                .WhenAnyValue(m => m.ResultColumns)
                .Where(cols => cols != null)
                .Subscribe(cols =>
                {
                    Result.Columns.Clear();
                    foreach (var c in cols) Result.Columns.Add(c);
                })
                .DisposeWith(d);
                this.OneWayBind(ViewModel,
                    m => m.ResultRows,
                    v => v.Result.ItemsSource)
                    .DisposeWith(d);
                this.OneWayBind(ViewModel,
                    m => m.Request,
                    v => v.Request.Text)
                    .DisposeWith(d);
                this.OneWayBind(ViewModel,
                    m => m.Response,
                    v => v.Response.Text)
                    .DisposeWith(d);

                // Commands
                this.BindCommand(ViewModel,
                    m => m.Submit,
                    v => v.Submit)
                    .DisposeWith(d);
                this.BindCommand(ViewModel,
                    m => m.SubmitDemo,
                    v => v.SubmitDemo)
                    .DisposeWith(d);
                this.BindCommand(ViewModel,
                    m => m.GenerateRequest,
                    v => v.GenerateRequest)
                    .DisposeWith(d);

                // Key Bindings
                this.InputBindings.Add(new KeyBinding(ViewModel.Submit, Key.K, ModifierKeys.Control));
                this.InputBindings.Add(new KeyBinding(ViewModel.SubmitDemo, Key.J, ModifierKeys.Control));
                this.InputBindings.Add(new KeyBinding(ViewModel.GenerateRequest, Key.L, ModifierKeys.Control));
            });
        }
    }
}
