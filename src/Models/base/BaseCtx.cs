using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Celin
{
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
                    var list = JsonSerializer.Deserialize<List<T>>(sr.ReadToEnd(), new JsonSerializerOptions
                    {
                        Converters =
                        {
                            new AIS.ActionJsonConverter(),
                            new AIS.GridActionJsonConverter()
                        }
                    });
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
                    sw.Write(JsonSerializer.Serialize(list, new JsonSerializerOptions
                    {
                        Converters =
                        {
                            new AIS.ActionJsonConverter(),
                            new AIS.GridActionJsonConverter()
                        }
                    }));
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
