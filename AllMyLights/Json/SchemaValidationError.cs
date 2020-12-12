namespace AllMyLights.Json
{
    public class SchemaValidationError
    {
        public string Path { get; }
        public string Message { get; }

        public SchemaValidationError(string path, string message)
        {
            Path = path;
            Message = message;
        }

        public override string ToString() => $"{Path}: {Message}";
    }
}
