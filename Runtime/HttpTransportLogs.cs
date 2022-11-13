using System;
using System.Collections.Generic;

namespace HttpTransport
{
    public class HttpTransportLogs : IHttpTransportLogs
    {
        private int _limit;
        private readonly object _sync = new object();
        private readonly Queue<PacketLog> _collection = new Queue<PacketLog>();

        public HttpTransportLogs(int netLogLimit = 1000)
        {
            _limit = netLogLimit;
        }

        public PacketLog[] Collection
        {
            get
            {
                lock (_sync)
                {
                    return _collection.ToArray();
                }
            }
        }

        public int Limit
        {
            get
            {
                lock (_sync)
                {
                    return _limit;
                }
            }
            set
            {
                lock (_sync)
                {
                    _limit = value;
                }
            }
        }

        public void Push(long id, string key, object value, object content, PacketLogIO io)
        {
            lock (_sync)
            {
                var packet = new PacketLog
                {
                    Id = id,
                    Key = key,
                    Value = value,
                    Content = content,
                    Time = DateTime.Now,
                    Io = io
                };
                _collection.Enqueue(packet);
                while (_collection.Count > _limit)
                {
                    _collection.Dequeue();
                }
            }
        }

        public void Clear()
        {
            lock (_sync)
            {
                _collection.Clear();
            }
        }
    }

    public class PacketLog
    {
        public long Id;
        public string Key;
        public object Value;
        public object Content;
        public DateTime Time;
        public PacketLogIO Io;
    }

    public enum PacketLogIO
    {
        Response,
        Call,
        Notify
    }
}
