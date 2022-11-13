using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using HttpTransport.Transports;

namespace HttpTransport.Channels
{
    public class PacketMessageQueue : IEnumerable<WebRemoteChannel.Packet>
    {
        private readonly Queue<WebRemoteChannel.Packet> _packetsQueue = new Queue<WebRemoteChannel.Packet>();
        private readonly Queue<WebRemoteChannel.Packet> _firstQueue = new Queue<WebRemoteChannel.Packet>(); //priority queue

        public int Count => _firstQueue.Count + _packetsQueue.Count;

        public IEnumerator<WebRemoteChannel.Packet> GetEnumerator()
        {
            foreach (var packet in _firstQueue)
            {
                yield return packet;
            }

            foreach (var packet in _packetsQueue)
            {
                yield return packet;
            }
        }

        public WebRemoteChannel.Packet Dequeue() => _firstQueue.Count > 0 ? _firstQueue.Dequeue() : _packetsQueue.Dequeue();

        public Task<Response> Enqueue(Request request)
        {
            var task = new TaskCompletionSource<Response>();

            var isFirst = request.Flags.IsFirst();
            var queue = isFirst ? _firstQueue : _packetsQueue;

            if (TryFindPacket(queue, request, out var packet))
            {
                packet.Response.Task.ContinueWith(t => task.SetResult(t.Result));
            }
            else
            {
                queue.Enqueue(new WebRemoteChannel.Packet()
                {
                    Request = request,
                    Response = task,
                });
            }

            return task.Task;
        }

        private static bool TryFindPacket(Queue<WebRemoteChannel.Packet> queue, Request request,
                out WebRemoteChannel.Packet packetResult)
        {
            packetResult = default;
            foreach (var packet in queue)
            {
                if (packet.Request == request)
                {
                    packetResult = packet;
                    return true;
                }
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
