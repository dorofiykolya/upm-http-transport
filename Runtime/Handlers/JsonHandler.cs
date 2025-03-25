using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HttpTransport.Transports;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HttpTransport.Handlers
{
    public class JsonHandler : IHandler
    {
        public delegate void ErrorHandler(Response response);

        public class Options
        {
            public readonly HashSet<HttpStatusCode> Success = new HashSet<HttpStatusCode>()
            {
                HttpStatusCode.OK,
                HttpStatusCode.Created,
                HttpStatusCode.Accepted
            };

            public readonly List<ErrorHandler> ErrorHandlers = new List<ErrorHandler>();
        }

        private readonly Options _options = new Options();
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings();

        public JsonHandler(Action<JsonSerializerSettings> settings = null, Action<Options> options = null)
        {
            _options.ErrorHandlers.Add((response) =>
            {
                string json = null;
                try
                {
                    json = response.Data != null ? Encoding.UTF8.GetString(response.Data) : null;
                    if (json != null)
                    {
                        var detail = JsonConvert.DeserializeObject<ErrorResponse>(json, _jsonSerializerSettings);
                        response.ErrorCode = detail?.Code ?? 0;
                    }
                }
                catch (Exception)
                {
                    response.Content = json;
                    response.ErrorCode = 0;
                }
            });

            settings?.Invoke(_jsonSerializerSettings);
            options?.Invoke(_options);
        }

        public Task<Request> OnRequest(Request value)
        {
            var json = JsonConvert.SerializeObject(value.Content, _jsonSerializerSettings);
            value.Content = Encoding.UTF8.GetBytes(json);

            return Task.FromResult(value);
        }

        public Task<Response> OnReceive(Response value)
        {
            if (value.ResponseCode != 0)
            {
                var json = value.Data != null ? Encoding.UTF8.GetString(value.Data) : null;
                if (json == null)
                {
                    value.Content = null;
                }
                else if (_options.Success.Contains((HttpStatusCode)value.ResponseCode))
                {
                    value.Content = JsonConvert.DeserializeObject(json, value.ResponseType, _jsonSerializerSettings);
                }
                else
                {
                    foreach (var handler in _options.ErrorHandlers)
                    {
                        handler(value);
                    }
                }
            }

            return Task.FromResult(value);
        }
    }

    public class StrippedTypeSerializationBinder : DefaultSerializationBinder
    {
        private readonly Assembly _assembly;

        public StrippedTypeSerializationBinder(Assembly assembly)
        {
            _assembly = assembly;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            return base.BindToType(_assembly.GetName().Name, typeName);
        }
    }

    [Serializable]
    public class ErrorResponse
    {
        [JsonProperty("code")] public long Code;
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
    }
}