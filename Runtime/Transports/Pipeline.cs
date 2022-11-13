using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HttpTransport.Transports
{
    public interface IPipeline
    {
        void Add(IHandler handler);
        void Remove(IHandler handler);
    }

    public class Pipeline : IPipeline
    {
        private Transport _transport;
        private IChannel _channel;
        private List<IHandler> _handlers;

        public Pipeline(Transport transport, IChannel channel)
        {
            _transport = transport;
            _channel = channel;
            _handlers = new List<IHandler>();
        }

        public static void Break()
        {
            PipelineBreak.BreakTask();
        }

        public void Add(IHandler handler)
        {
            _handlers.Add(handler);
        }

        public void Remove(IHandler handler)
        {
            _handlers.Remove(handler);
        }

        public async Task<Request> Send(Request value)
        {
            var handlers = _handlers.ToArray();
            var currentValue = value;
            for (var i = handlers.Length - 1; i >= 0; i--)
            {
                var handler = handlers[i];
                try
                {
                    var task = handler.OnRequest(currentValue);
                    currentValue = await task;
                    if (_transport.Lifetime.IsTerminated) return null;
                    if (task.IsFaulted)
                    {
                        if (task.Exception != null && task.Exception.InnerException is PipelineBreak)
                        {
                            return null;
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e is PipelineBreak)
                    {
                        return null;
                    }
                    Debug.LogError(e);
                    throw e;
                }
            }

            return currentValue;
        }

        public async Task<Response> Receive(Response value)
        {
            var handlers = _handlers.ToArray();
            var currentValue = value;
            foreach (var handler in handlers)
            {
                try
                {
                    var task = handler.OnReceive(currentValue);
                    currentValue = await task;
                    if (_transport.Lifetime.IsTerminated) return null;
                    if (task.IsFaulted)
                    {
                        if (task.Exception != null && task.Exception.InnerException is PipelineBreak)
                        {
                            return null;
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e is PipelineBreak)
                    {
                        return null;
                    }
                    Debug.LogError(e);
                    throw e;
                }
            }

            return currentValue;
        }
    }
}
