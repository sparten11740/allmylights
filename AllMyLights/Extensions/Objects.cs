using System;
using System.Collections.Generic;
using System.Text;

namespace AllMyLights.Extensions
{
    public static class Objects
    {
        public static R Let<T, R>(this T self, Func<T, R> block)
        {
            return block(self);
        }
    }
}
