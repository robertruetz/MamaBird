using System;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace MamaBird
{
    public class FakeHttpServer
    {
        private HttpListener _listener = new HttpListener();
        private Func<HttpListenerRequest, string> _responderMethod;
        private Dictionary<string, Queue<HttpInteraction>> _config;
        private string[] _prefixes;

        public HttpListener Listener { get => _listener; set => _listener = value; }
        public Func<HttpListenerRequest, string> ResponderMethod { get => _responderMethod; set => _responderMethod = value; }
        public Dictionary<string, Queue<HttpInteraction>> Config { get => _config; set => _config = value; }
        public string[] Prefixes { get => _prefixes; set => _prefixes = value; }

        public FakeHttpServer(string[] prefixes)
        {
            _prefixes = prefixes;
            _config = new Dictionary<string, Queue<HttpInteraction>>();
            foreach (var prefix in _prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }
            _listener.Start();
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
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            var ctx = c as HttpListenerContext;
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
                                    response.ContentLength64 = buffer.Length;
                                    response.OutputStream.Write(buffer, 0, buffer.Length);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                            finally
                            {
                                ctx.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                    }
                }
                catch
                { }
            });
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}
