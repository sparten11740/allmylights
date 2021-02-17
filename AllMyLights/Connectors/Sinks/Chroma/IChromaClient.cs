using System.Drawing;
using System.Threading.Tasks;

namespace AllMyLights.Connectors.Sinks.Chroma
{
    public interface IChromaClient
    {
        Task<InitializationResponse> InitializeAsync();
        Task SendHeartbeatAsync();

        Task<UpdateResponse> UpdateAsync(string device, string payload);
        Task<UpdateResponse> ApplyStaticEffectAsync(string device, Color color);
    }
}
