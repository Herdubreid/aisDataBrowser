﻿using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Utils;
using Pidgin;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using static Pidgin.Parser;

namespace Celin
{
    public class DataColumn
    {
        public string Alias { get; set; }
        public string Description { get; set; }
        public override string ToString()
        {
            return $"{Alias} {Description}";
        }
    }
    public class DataVM : ReactiveObject
    {
        readonly string defaultMsg = "Enter command below and press any of the buttons on the left...";
        readonly string submittingMsg = "Submitting the query...";
        [Reactive] public TextDocument Code { get; set; } = new TextDocument();
        [Reactive] public DataColumn SelectedAlias { get; set; }
        [Reactive] public string Msg { get; set; }
        [Reactive] public string Request { get; set; }
        [Reactive] public string Response { get; set; }
        [Reactive] public bool Busy { get; set; }
        [Reactive] public IEnumerable<JsonElement> ResultRows { get; set; }
        [Reactive] public IEnumerable<DataGridColumn> ResultColumns { get; set; }
        [Reactive] public IEnumerable<DataColumn> AvailableColumns { get; set; }
        [Reactive] public int SelectedTabIndex { get; set; }
        public ReactiveCommand<System.Reactive.Unit, Task> Submit { get; }
        public ReactiveCommand<System.Reactive.Unit, Task> SubmitDemo { get; }
        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> GenerateRequest { get; }
        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> NextTab { get; }
        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> LastTab { get; }
        static bool FindProperty(string property, JsonElement element, out JsonElement found)
        {
            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in element.EnumerateArray())
                {
                    if (FindProperty(property, el, out found)) return true;
                }
            }
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty(property, out found)) return true;
                {
                    foreach (var el in element.EnumerateObject())
                    {
                        if (FindProperty(property, el.Value, out found)) return true;
                    }
                }
            }
            found = new JsonElement();
            return false;
        }
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
                foreach (var e in columns)
                {
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
            }
            ResultColumns = new ObservableCollection<DataGridColumn>(cols);
        }
        void PopulateColumns(JObject result)
        {
            var columns = FindKey(result, "columns") as JObject;
            if (columns is null) return;

            var cols = new List<DataColumn>();
            foreach (var e in columns)
            {
                cols.Add(new DataColumn()
                {
                    Alias = string.Format("{0}.{1}", e.Key.Split('_')),
                    Description = e.Value.ToString()
                });
            }
            AvailableColumns = cols;
        }
        void ResponseException(AIS.HttpWebException e)
        {
            if (e.HttpStatusCode == (HttpStatusCode)444)
            {
                Msg = "Expired!\nPlease Sign in again.";
            }
            else
            {
                Msg = e.Message;
            }
        }
        public DataVM()
        {
            Msg = defaultMsg;
            var canSubmit = Observable
                .CombineLatest(
                this.WhenAnyValue(m => m.Busy),
                MainVM.Instance.IsConnected)
                .Select(c => !c[0] && c[1]);

            Submit = ReactiveCommand.Create(async () =>
            {
                Busy = true;
                var cmd = Regex.Replace(Code.Text, @"\t|\n|\r", string.Empty).Trim(' ') + ';';
                var result = AIS.Data.DataRequest.Parser.Before(Char(';')).Parse(cmd);
                if (result.Success)
                {
                    result.Value.maxPageSize = MainVM.Instance.MaxReturnRowsItems.ElementAt(MainVM.Instance.MaxReturnRows);
                    Msg = submittingMsg;
                    Request = JsonConvert.SerializeObject(result.Value, Formatting.Indented);
                    try
                    {
                        var response = await MainVM.Instance.ActiveConnection.Server.RequestAsync<JObject>(result.Value);
                        Response = JsonConvert.SerializeObject(response, Formatting.Indented);
                        PopulateResult(response);
                        SelectedTabIndex = 1;
                        Msg = "Check the results in the grid...";
                    }
                    catch (AIS.HttpWebException e)
                    {
                        ResponseException(e);
                    }
                }
                else
                {
                    Msg = result.Error.ToString();
                    Request = "Failed!";
                }
                Busy = false;
            }, canSubmit);
            SubmitDemo = ReactiveCommand.Create(async () =>
            {
                Busy = true;
                var result = AIS.Data.DataSubject.Parser.Parse(Code.Text);
                if (result.Success)
                {
                    var request = new AIS.DatabrowserRequest()
                    {
                        targetName = result.Value.Name.ToUpper(),
                        targetType = result.Value.Type,
                        dataServiceType = "BROWSE",
                        formServiceDemo = "TRUE"
                    };
                    Request = JsonConvert.SerializeObject(request, Formatting.Indented);
                    try
                    {
                        Msg = submittingMsg;
                        var response = await MainVM.Instance.ActiveConnection.Server.RequestAsync<JObject>(request);
                        PopulateColumns(response);
                    }
                    catch (AIS.HttpWebException e)
                    {
                        ResponseException(e);
                    }
                }
                else
                {
                    Msg = result.Error.ToString();
                    Request = "Failed!";
                }
                Busy = false;
            }, canSubmit);
            GenerateRequest = ReactiveCommand.Create(() =>
            {
                Msg = defaultMsg;
                var cmd = Regex.Replace(Code.Text, @"\t|\n|\r", string.Empty).Trim(' ') + ';';
                var result = AIS.Data.DataRequest.Parser.Before(Char(';')).Parse(cmd);
                if (result.Success)
                {
                    result.Value.maxPageSize = MainVM.Instance.MaxReturnRowsItems.ElementAt(MainVM.Instance.MaxReturnRows);
                    Request = JsonConvert.SerializeObject(result.Value, Formatting.Indented);
                    SelectedTabIndex = 2;
                }
                else
                {
                    Msg = result.Error.ToString();
                    Request = "Failed!";
                }
            });
        }
    }
}
