using System;

namespace System.Linq
{
    static class ThrowHelper
    {
        internal static Exception ArgumentNull(String paramName)
        {
            return new ArgumentNullException(paramName);
        }

        internal static Exception ArgumentOutOfRange(string paramName)
        {
            return new ArgumentOutOfRangeException(paramName);
        }

        internal static Exception NoElements()
        {
            return new InvalidOperationException("NoElements");
        }
    }
}
