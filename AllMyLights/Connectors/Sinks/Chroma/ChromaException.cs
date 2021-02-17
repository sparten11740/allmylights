using System;
using System.Collections.Generic;
using System.Text;

namespace AllMyLights.Connectors.Sinks.Chroma
{
    public class ChromaException: Exception
    {
        public ChromaException(string message): base(message) { }
    }
}
