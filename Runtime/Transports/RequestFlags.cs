using System;

namespace HttpTransport.Transports
{
    [Flags]
    public enum RequestFlags
    {
        Default = 0,
        Once = 1,
        HeaderQueue = 2
    }

    public static class RequestFlagsEx
    {
        public static bool IsOnce(this RequestFlags flags)
        {
            return (flags & RequestFlags.Once) == RequestFlags.Once;
        }

        public static bool IsFirst(this RequestFlags flags)
        {
            return (flags & RequestFlags.HeaderQueue) == RequestFlags.HeaderQueue;
        }
    }
}
