namespace PhoenixToolkit.Services;

internal sealed class NoOpPhoenixServerServiceController : IPhoenixServerServiceController
{
    public bool StopAndWait(string serviceName) => false;

    public void StartAndWait(string serviceName)
    {
    }
}
