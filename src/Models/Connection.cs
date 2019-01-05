namespace Celin
{
    public class Connection : BaseCtx<Connection>
    {
        public AIS.Server Server { get; set; }
        public string Name => Server.AuthResponse is null ? Id : string.Format("{0} ({1})", Id, Server.AuthResponse.username);
        public Connection(string id, string baseUrl) : base(id)
        {
            Server = new AIS.Server(baseUrl);
        }
    }
}
