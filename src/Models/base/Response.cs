using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
namespace Celin
{
    public class Response<T> where T : AIS.Request
    {
        public T Request { get; set; }
        public JObject Result { get; set; }
        public string ResultKey
        {
            get
            {
                if (typeof(T) == typeof(AIS.FormRequest)) return string.Format("fs_{0}", Request.formName);
                if (typeof(T) == typeof(AIS.DatabrowserRequest))
                {
                    var rq = Request as AIS.DatabrowserRequest;
                    return string.Format("fs_DATABROWSE_{0}", rq.targetName);
                }
                if (typeof(T) == typeof(AIS.StackFormRequest))
                {
                    var rq = Request as AIS.StackFormRequest;
                    return string.Format("fs_{0}", rq.formRequest.formName);
                }

                return null;
            }
        }
    }
}
