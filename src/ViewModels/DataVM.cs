using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;
using ReactiveUI;
using ICSharpCode.AvalonEdit.Document;
using Pidgin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace Celin
{
    [AddINotifyPropertyChangedInterface]
    public class DataVM
    {
        readonly string _message = "Enter command below and press the Run button...";
        public string Message { get; set; }
        public string Request { get; set; } = string.Empty;
        public string Columns { get; set; } = string.Empty;
        public TextDocument Code { get; set; } = new TextDocument();
        public IEnumerable<JToken> ResultRows { get; set; }
        public IEnumerable<DataGridColumn> ResultColumns { get; set; }
        public ReactiveCommand<System.Reactive.Unit, Task> Submit { get; }
        JToken FindKey(JArray token, string key)
        {
            foreach (var e in token)
            {
                var res = FindKey(e, key);
                if (res != null) return res;
            }
            return null;
        }
        JToken FindKey(JObject token, string key)
        {
            var res = token.SelectToken(key);
            if (res is null)
            {
                foreach (var e in token)
                {
                    res = FindKey(e.Value, key);
                    if (res != null) return res;
                }
            }
            return res;
        }
        JToken FindKey(JToken token, string key)
        {
            switch (token.Type)
            {
                case JTokenType.Array:
                    return FindKey(token as JArray, key);
                case JTokenType.Object:
                    return FindKey(token as JObject, key);
            }
            return null;
        }
        void PopulateResult(JObject result)
        {
            var cols = new List<DataGridColumn>();
            var columns = FindKey(result, "columns") as JObject;
            if (columns is null)
            {
                var output = FindKey(result, "output") as JArray;
                if (output != null && output.Any())
                {
                    foreach (var e in output[0] as JObject)
                    {
                        if (e.Value.Type == JTokenType.Object)
                        {
                            if (e.Key.Equals("groupBy"))
                            {
                                foreach (var g in e.Value as JObject)
                                {
                                    var col = new DataGridTextColumn()
                                    {
                                        Header = g.Key,
                                        Binding = new Binding()
                                        {
                                            Converter = new JValueConverter(),
                                            ConverterParameter = $"groupBy.['{g.Key}']"
                                        }
                                    };
                                    cols.Add(col);
                                }
                            }
                        }
                        else
                        {
                            var col = new DataGridTextColumn()
                            {
                                Header = string.Format("{1}({0})", e.Key.Split('_')),
                                Binding = new Binding()
                                {
                                    Converter = new JValueConverter(),
                                    ConverterParameter = $"['{e.Key}']"
                                }
                            };
                            cols.Add(col);
                        }
                    }
                    ResultRows = output.ToArray();
                }
            }
            else
            {
                var names = new List<string>();
                foreach (var e in columns)
                {
                    names.Add(string.Format("{0}.{1}", e.Key.Split('_')));
                    var col = new DataGridTextColumn()
                    {
                        Header = e.Value,
                        Binding = new Binding()
                        {
                            Converter = new JValueConverter(),
                            ConverterParameter = $"['{e.Key}']"
                        }
                    };
                    cols.Add(col);
                }
                var rows = FindKey(result, "rowset");
                ResultRows = rows.ToArray();
                Columns = string.Join("\n", names);
            }
            ResultColumns = cols;
        }

        public DataVM()
        {
            Message = _message;
            Submit = ReactiveCommand.Create(async () =>
            {
                var result = AIS.Data.DataRequest.Parser.Parse(Code.Text);
                if (result.Success)
                {
                    result.Value.maxPageSize = "1000";
                    Message = _message;
                    Request = JsonConvert.SerializeObject(result.Value, Formatting.Indented);
                    var con = MainVM.Instance.ActiveConnection;
                    if (con is null)
                    {
                        Message = "No active connection!";
                    }
                    else
                    {
                        try
                        {
                            var response = await con.Server.RequestAsync<JObject>(result.Value);
                            PopulateResult(response);
                        }
                        catch (AIS.HttpWebException e)
                        {
                            Message = e.Message;
                        }
                    }
                }
                else
                {
                    Message = result.Error.ToString();
                    Request = "Failed!";
                }
            });
        }
    }
}
