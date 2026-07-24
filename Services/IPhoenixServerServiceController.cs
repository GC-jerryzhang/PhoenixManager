namespace PhoenixToolkit.Services;

internal interface IPhoenixServerServiceController
{
    bool StopAndWait(string serviceName);

    void StartAndWait(string serviceName);
}
