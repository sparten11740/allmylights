using System;
using System.Collections.Generic;
using System.Linq;
using OpenRGB.NET.Models;

namespace AllMyLights.Connectors.Sources.OpenRGB
{
    public class DeviceEqualityComparer : IEqualityComparer<Device>
    {
        public bool Equals(Device first, Device second)
        {
            if (first.Name != second.Name) return false;


            return first.Colors.SequenceEqual(second.Colors);
        }

        public int GetHashCode(Device device)
        {
            throw new NotImplementedException("Implement me");
        }
    }
}
