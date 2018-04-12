using System;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Net.Http.Server;

namespace MamaBird
{
    public class FakeHttpServer
    {
        // private HttpListener _listener = new HttpListener();
        private Func<HttpListenerRequest, string> _responderMethod;
        private Dictionary<string, Queue<HttpInteraction>> _config;
        private WebListenerSettings _settings;
        private WebListener _webListener;
        private string[] _prefixes;

        // public HttpListener Listener { get => _listener; set => _listener = value; }
        public Func<HttpListenerRequest, string> ResponderMethod { get => _responderMethod; set => _responderMethod = value; }
        public Dictionary<string, Queue<HttpInteraction>> Config { get => _config; set => _config = value; }
        public string[] Prefixes { get => _prefixes; set => _prefixes = value; }
        public WebListenerSettings Settings { get => _settings; set => _settings = value; }
        public WebListener WebListener { get => _webListener; set => _webListener = value; }

        public FakeHttpServer(string[] prefixes)
        {
            _prefixes = prefixes;
            _config = new Dictionary<string, Queue<HttpInteraction>>();
            _settings = new WebListenerSettings();
            foreach (var prefix in _prefixes)
            {
                _settings.UrlPrefixes.Add(prefix);
            }
            _webListener = new WebListener(_settings);
            _webListener.Start();
        }

        public void LoadConfig(string filepath)
        {
            var fileStr = File.ReadAllText(filepath);
            var testCase = JsonConvert.DeserializeObject<TestCase>(fileStr);
            foreach (var hI in testCase.HttpInteractions)
            {
                if (!_config.ContainsKey(hI.Route))
                {
                    _config.Add(hI.Route, new Queue<HttpInteraction>());
                }
                _config[hI.Route].Enqueue(hI);
            }
        }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem((x) =>
            {
                Console.WriteLine($"FakeHttpServer listening... ");
                try
                {
                    while (_webListener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            var ctx = c as RequestContext;
                            try
                            {
                                var request = ctx.Request;
                                var response = ctx.Response;
                                response.StatusCode = 404;
                                var rawUrl = request.RawUrl;
                                if (_config.ContainsKey(rawUrl))
                                {
                                    var interaction = _config[rawUrl].Dequeue();
                                    if (interaction.Delay > 0)
                                    {
                                        Thread.Sleep(interaction.Delay);
                                    }
                                    if (interaction.Headers.Count > 0)
                                    {
                                        foreach (var item in interaction.Headers)
                                        {
                                            //TODO: Support headers in response
                                        }
                                    }
                                    response.StatusCode = interaction.StatusCode;
                                    var buffer = Encoding.UTF8.GetBytes(interaction.Content);
                                    response.ContentLength = buffer.Length;
                                    response.ContentType = "text/plain";
                                    response.Body.Write(buffer, 0, buffer.Length);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                            finally
                            {
                                ctx.Dispose();
                            }
                        }, _webListener.AcceptAsync().Result);
                    }
                }
                catch
                { }
            });
        }

        public void Stop()
        {
            _webListener.Dispose();
        }
    }
}
