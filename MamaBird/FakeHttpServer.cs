using System;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Linq;

namespace MamaBird
{
    public class FakeHttpServer
    {

        private IWebHost _host;
        private int _port;

        private Dictionary<string, Queue<HttpInteraction>> _config;

        public Dictionary<string, Queue<HttpInteraction>> Config { get => _config; set => _config = value; }
        public IWebHost Host { get => _host; set => _host = value; }
        public int Port { get => _port; set => _port = value; }

        public FakeHttpServer(int port = 5000)
        {
            _port = port;
            _config = new Dictionary<string, Queue<HttpInteraction>>();
        }

        public void Run()
        {
            try
            {
                _host = WebHost.CreateDefaultBuilder()
                        .UseUrls("http://localhost:" + _port.ToString())
                        .Configure(app =>
                        {
                            app.Run(async context =>
                            {
                                string response = "RUNNING!";
                                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(response);
                                var interaction = new HttpInteraction()
                                {
                                    StatusCode = 404,
                                    Content = "Not Found"
                                };
                                if (_config.ContainsKey(context.Request.Path))
                                {
                                    interaction = _config[context.Request.Path].Dequeue();
                                }
                                context.Response.StatusCode = interaction.StatusCode;
                                if (interaction.Headers != null)
                                {
                                    foreach (var header in interaction.Headers)
                                    {
                                        // TODO Add support for returning headers.
                                    }
                                }
                                buffer = Encoding.UTF8.GetBytes(interaction.Content);
                                context.Response.ContentLength = buffer.Length;
                                context.Response.ContentType = "application/json";
                                await context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
                                return;
                            });
                        }).Build();
                _host.Start();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                
            }
        }

        public void Stop()
        {
            if (_host == null)
            {
                return;
            }
            _host.StopAsync().Wait();
            _host.Dispose();
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

        //public void Run()
        //{
        //    ThreadPool.QueueUserWorkItem((x) =>
        //    {
        //        Console.WriteLine($"FakeHttpServer listening... ");
        //        try
        //        {
        //            while (_listener.IsListening)
        //            {
        //                ThreadPool.QueueUserWorkItem((c) =>
        //                {
        //                    var ctx = c as HttpListenerContext;
        //                    try
        //                    {
        //                        var request = ctx.Request;
        //                        var response = ctx.Response;
        //                        response.StatusCode = 404;
        //                        var rawUrl = request.RawUrl;
        //                        if (_config.ContainsKey(rawUrl))
        //                        {
        //                            var interaction = _config[rawUrl].Dequeue();
        //                            if (interaction.Delay > 0)
        //                            {
        //                                Thread.Sleep(interaction.Delay);
        //                            }
        //                            if (interaction.Headers.Count > 0)
        //                            {
        //                                foreach (var item in interaction.Headers)
        //                                {
        //                                    //TODO: Support headers in response
        //                                }
        //                            }
        //                            response.StatusCode = interaction.StatusCode;
        //                            var buffer = Encoding.UTF8.GetBytes(interaction.Content);
        //                            response.ContentLength64 = buffer.Length;
        //                            response.OutputStream.Write(buffer, 0, buffer.Length);
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        Console.WriteLine(ex.ToString());
        //                    }
        //                    finally
        //                    {
        //                        ctx.Response.OutputStream.Close();
        //                    }
        //                }, _listener.GetContext());
        //            }
        //        }
        //        catch
        //        { }
        //    });
        //}
    }
}
