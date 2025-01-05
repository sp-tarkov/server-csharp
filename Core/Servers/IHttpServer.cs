namespace Types.Servers;

public interface IHttpServer
{
    public void Load(WebApplicationBuilder builder);
    public bool IsStarted();
}