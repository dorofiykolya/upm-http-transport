using System.Collections;
using System.Collections.Generic;

namespace HttpTransport.Transports
{
    public class RequestHeaders : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>
        {
            {
                Headers.ContentType, HttpHelper.ContentTypeApplicationJson
            }
        };

        public string ContentType
        {
            get => _headers[Headers.ContentType];
            set => _headers[Headers.ContentType] = value;
        }

        public string Authorization
        {
            get
            {
                string value;
                _headers.TryGetValue(Headers.Authorization, out value);
                return value;
            }
            set
            {
                _headers[Headers.Authorization] = value;
            }
        }

        public Dictionary<string, string>.Enumerator GetEnumerator() => _headers.GetEnumerator();

        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator() => _headers.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
