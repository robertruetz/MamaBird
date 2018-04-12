using System.Collections.Generic;

namespace MamaBird
{
    public class HttpInteraction
    {
        private string _route;
        private int _delay;
        private string _content;
        private int _statusCode;
        private Dictionary<string, object> _headers;

        public string Route { get => _route; set => _route = value; }
        public int Delay { get => _delay; set => _delay = value; }
        public Dictionary<string, object> Headers { get => _headers; set => _headers = value; }
        public int StatusCode { get => _statusCode; set => _statusCode = value; }
        public string Content { get => _content; set => _content = value; }

        public HttpInteraction()
        {
        }

        public HttpInteraction(string route, string content, int delay)
        {
            _route = route;
            _content = content;
            _delay = delay;
        }

        public HttpInteraction(string route, string content)
        {
            _route = route;
            _content = content;
        }
    }
}
