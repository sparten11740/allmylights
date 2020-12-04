using System;
using System.Drawing;

namespace AllMyLights
{
    public interface IColorSubject
    {
        IObservable<Color> Get();
    }
}