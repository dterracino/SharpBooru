namespace TA.SharpBooru.Server
{
    public class Server
    {
        public abstract string ServerName { get; }

        public abstract void Start();
        public abstract void Stop();
        public abstract object GetClient();
        public abstract void HandleClient(object Client);
    }
}
