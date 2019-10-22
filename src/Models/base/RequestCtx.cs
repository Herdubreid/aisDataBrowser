using System;
using System.Collections.Generic;
namespace Celin
{
    public abstract class RequestCtx<T1, T2> : BaseCtx<T2>
        where T1 : AIS.Request, new()
        where T2 : IBaseCtx
    {
        public static List<Response<T1>> Responses { get; } = new List<Response<T1>>();
        public T1 Request { get; set; } = new T1();
        public void Submit()
        {
            if (Connection.Current is null)
            {
                throw new Exception("No Server Context!");
            }
            else if (Connection.Current.Server.AuthResponse is null)
            {
                throw new Exception(string.Format("{0} not connected!", Connection.Current.Id));
            }
            else
            {
                /*var cancel = new CancellationTokenSource();
                var t = new Task<Tuple<bool, JObject>>(() => Connection.Current.Server.RequestAsync<JObject>(Request, cancel));
                if (Wait(t, cancel))
                {

                    if (t.Result.Item1)
                    {
                        Responses.Add(new Response<T1>() { Request = Request, Result = t.Result.Item2 });
                        BaseCmd.Success("Responses {0}.", Responses.Count);
                    }
                    else
                    {
                        BaseCmd.Error("Request failed!");
                    }
                }*/
            }
        }
        protected RequestCtx(string id) : base(id)
        {
            Request.maxPageSize = "10";
        }
    }
}
