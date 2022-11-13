using System;
using System.Collections.Generic;
using System.Text;
using HttpTransport.Handlers;
using Newtonsoft.Json;

namespace HttpTransport.Transports
{
    public class Response
    {
        public Request Request { get; }
        public int RequestId => Request.RequestId;
        public Type ResponseType => Request.ResponseType;
        public string Uri => Request.Uri;
        public Dictionary<string, string> ResponseHeaders { get; }
        public byte[] Data { get; }
        public long ResponseCode { get; }
        public object Content { get; set; }
        public long ErrorCode { get; set; }
        public bool IsNetworkError { get; set; }
        public bool IsFail => ResponseCode == 0 && IsNetworkError == false;

        public Response(Request request, Dictionary<string, string> responseHeaders, byte[] data, long responseCode,
                bool isNetworkError, string detail = default)
        {
            Request = request;
            ResponseHeaders = responseHeaders;
            Data = data;
            ResponseCode = responseCode;
            IsNetworkError = isNetworkError;
            if (IsNetworkError)
            {
                ErrorCode = 0;
            }
        }

        public object Debug
        {
            get
            {
                try
                {
                    var json = Encoding.UTF8.GetString(Data);

                    if (ResponseCode == 200)
                    {
                        return JsonConvert.DeserializeObject(json, ResponseType, JsonHandler.JsonSerializerSettings);
                    }

                    if (ResponseCode == 400)
                    {
                        var detail = JsonConvert.DeserializeObject<ErrorResponse>(json, JsonHandler.JsonSerializerSettings);
                        return detail.Code;
                    }
                }
                catch (Exception e)
                {
                    return e;
                }

                return null;
            }
        }

        public static Response Fail(Request request)
        {
            return new Response(request, new Dictionary<string, string>(), new byte[0], 0, false, "fail");
        }

        public static Response From(Request request, Response response)
        {
            return new Response(request, response.ResponseHeaders, response.Data, (long)response.ResponseCode, response.IsNetworkError, null);
        }
    }
}
