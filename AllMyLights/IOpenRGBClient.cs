using System.Drawing;

namespace AllMyLights
{
    public interface IOpenRGBClient
    {
        void UpdateAll(Color color);
    }
}