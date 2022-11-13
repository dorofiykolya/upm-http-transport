using System;
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
        public static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        public Task<Request> OnRequest(Request value)
        {
            //if (!(value.Content is byte[]))
            {
                var json = JsonConvert.SerializeObject(value.Content, JsonSerializerSettings);
                value.Content = Encoding.UTF8.GetBytes(json);
            }

            return Task.FromResult(value);
        }

        public Task<Response> OnReceive(Response value)
        {
            if (value.ResponseCode != 0)
            {
                var json = value.Data != null ? Encoding.UTF8.GetString(value.Data) : null;
                JsonSerializerSettings.SerializationBinder = new StrippedTypeSerializationBinder(value.ResponseType.Assembly);
                if (json == null)
                {
                    value.Content = null;
                }
                else if (value.ResponseCode == 200)
                {
                    value.Content = JsonConvert.DeserializeObject(json, value.ResponseType, JsonSerializerSettings);
                }
                else if (value.ResponseCode == 400)
                {
                    try
                    {
                        var detail = JsonConvert.DeserializeObject<ErrorResponse>(json, JsonSerializerSettings);
                        value.ErrorCode = detail?.Code ?? 0;
                    }
                    catch (Exception)
                    {
                        value.Content = json;
                        value.ErrorCode = 0;
                    }
                }
                else
                {
                    value.ErrorCode = 0;
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

    public class ErrorResponse
    {
        public long Code { get; set; }
    }
}
