using System.Collections.Generic;

namespace AllMyLights.Platforms
{
    public interface IDesktop
    {
        void SetBackground(string filePath);
        void SetBackgrounds(Dictionary<int, string> filePathByScreen);
        IEnumerable<int> GetScreens();
    }
}