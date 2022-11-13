using System;
using System.Net.Http;
using HttpTransport.Rpc;

namespace HttpTransport.Transports
{
    public class Request
    {
        public object RequestObject { get; }
        public int RequestId { get; }
        public string Uri { get; }
        public object Content { get; set; }
        public Type RequestType { get; }
        public Type ResponseType { get; }
        public AsyncResponse Response { get; }
        public RequestHeaders Headers { get; }
        public RequestFlags Flags { get; }
        public HttpMethod Method { get; }

        public Request(int requestId, string uri, Type requestType, Type responseType, object requestObject,
                AsyncResponse response, RequestFlags flags, HttpMethod method)
        {
            RequestId = requestId;
            RequestObject = requestObject;
            Uri = uri;
            RequestType = requestType;
            ResponseType = responseType;
            Response = response;
            Flags = flags;
            Method = method;
            Headers = new RequestHeaders();
        }
    }
}
