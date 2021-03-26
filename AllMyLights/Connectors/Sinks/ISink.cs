namespace AllMyLights.Connectors.Sinks
{
    public interface ISink: IConnector
    {
        void Consume(object value);
    }
}
