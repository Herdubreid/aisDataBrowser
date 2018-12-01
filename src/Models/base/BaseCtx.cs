using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
namespace Celin
{
    class GridConverter : CustomCreationConverter<AIS.Grid>
    {
        public override AIS.Grid Create(Type objectType)
        {
            return null;
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            if (jsonObject.ContainsKey("gridRowInsertEvents"))
            {
                return jsonObject.ToObject<AIS.GridInsert>();
            }
            return jsonObject.ToObject<AIS.GridUpdate>();
        }
    }
    class ActionConverter : CustomCreationConverter<AIS.Action>
    {
        public override AIS.Action Create(Type objectType)
        {
            return null;
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            if (jsonObject.ContainsKey("controlID"))
            {
                return jsonObject.ToObject<AIS.FormAction>();
            }
            var ga = JsonConvert.DeserializeObject<AIS.GridAction>(jsonObject.ToString(), new GridConverter());
            return ga;
        }
    }
    public interface IBaseCtx
    {
        string Id { get; set; }
    }
    public abstract class BaseCtx<T> : IBaseCtx
        where T : IBaseCtx
    {
        public string Id { get; set; }
        public static List<T> List { get; set; } = new List<T>();
        public static T Current { get; set; }
        public static bool Select(string id)
        {
            if (List.Exists(f => f.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
            {
                Current = List.Find(f => f.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                return true;
            }
            return false;
        }
        public static IEnumerable<T> Load(string fname)
        {
            try
            {
                using (StreamReader sr = File.OpenText(fname))
                {
                    var list = JsonConvert.DeserializeObject<List<T>>(sr.ReadToEnd(), new ActionConverter());
                    if (list != null && list.Count > 0)
                    {
                        List = list;
                        Current = List.First();
                        return list;
                    }
                }
            }
            catch (Exception) { }
            return new List<T>();
        }
        public static void Save(IEnumerable<T> list, string fname)
        {
            try
            {
                using (StreamWriter sw = File.CreateText(fname))
                {
                    sw.Write(JsonConvert.SerializeObject(list));
                }
            }
            catch (Exception) { }
        }
        protected BaseCtx(string id)
        {
            Id = id;
        }
    }
}
