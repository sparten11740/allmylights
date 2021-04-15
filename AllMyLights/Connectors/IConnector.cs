namespace AllMyLights.Connectors
{
    public interface IConnector
    {
        string Id { get; }
        object GetInfo();
    }
}
