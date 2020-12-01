using System;
using System.Collections.Generic;
using System.Text;

namespace AllMyLights.Extensions
{
    public static class Dictionaries
    {

        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
        {
            dict.TryGetValue(key, out TValue value);
            return value ?? defaultValue;
        }
    }
}
