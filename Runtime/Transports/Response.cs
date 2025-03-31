using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public string Details { get; }

        public Response(
            Request request,
            Dictionary<string, string> responseHeaders,
            byte[] data,
            long responseCode,
            bool isNetworkError,
            string detail = null
        )
        {
            Request = request;
            ResponseHeaders = responseHeaders;
            Data = data;
            ResponseCode = responseCode;
            IsNetworkError = isNetworkError;
            Details = detail;
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
                    var code = (HttpStatusCode)ResponseCode;
                    try
                    {
                        if (new[] { HttpStatusCode.OK, HttpStatusCode.Accepted, HttpStatusCode.Created }.Contains(code))
                        {
                            return JsonConvert.DeserializeObject(json, ResponseType);
                        }

                        return JsonConvert.DeserializeObject<ErrorResponse>(json);
                    }
                    catch (Exception e)
                    {
                        var detail = JsonConvert.DeserializeObject<ErrorResponse>(json);
                        return detail;
                    }
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }

        public static Response Fail(Request request)
        {
            return new Response(
                request,
                new Dictionary<string, string>(),
                new byte[0],
                0,
                false,
                "fail"
            );
        }

        public static Response From(Request request, Response response)
        {
            return new Response(
                request,
                response.ResponseHeaders,
                response.Data,
                response.ResponseCode,
                response.IsNetworkError,
                response.Details
            );
        }
    }
}