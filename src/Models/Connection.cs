namespace Celin
{
    public class Connection : BaseCtx<Connection>
    {
        public AIS.Server Server { get; set; }
        public Connection(string id, string baseUrl) : base(id)
        {
            Server = new AIS.Server(baseUrl);
        }
    }
}
