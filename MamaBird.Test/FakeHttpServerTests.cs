using System;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;

namespace MamaBird.Test
{
    public class FakeData
    {
        public string data;
    }
    
    public class FakeHttpServerTests
    {
        public int Port = 5099;
        public List<string> Prefixes = new List<string>();
        public FakeHttpServer Server;


        [Fact]
        public void FakeHttpServer_Constructor()
        {
            Server = new FakeHttpServer(Prefixes.ToArray());
            Assert.NotNull(Server.Config);
        }

        [Fact]
        public void FakeHttpServer_CanLoadConfigFromFile()
        {
            Prefixes.Add($"http://localhost:{Port}/");
            Server = new FakeHttpServer(Prefixes.ToArray());
            Server.LoadConfig(@".\TestAssets\testCase1.json");
            Assert.Single(Server.Config);
            Server.Stop();
            Server = null;
        }

        [Fact]
        public void FakeHttpServer_WhenNoConfigAdded_Returns404()
        {
            Prefixes.Add($"http://localhost:{Port}/");
            Server = new FakeHttpServer(Prefixes.ToArray());
            Server.Run();
            var client = new HttpClient();
            client.BaseAddress = new System.Uri("http://localhost:5099/test");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var content = new StringContent("{'data': 'test'}");
            var result = client.PostAsync(client.BaseAddress, content).Result;
            Assert.Equal(404, (int)result.StatusCode);
            Server.Stop();
            Server = null;
        }

        [Fact]
        public void FakeHttpServer_SingleRequest_ReturnsExpectedContent()
        {
            Prefixes.Add($"http://localhost:{Port}/");
            Server = new FakeHttpServer(Prefixes.ToArray());
            Server.Run();
            Server.LoadConfig(@".\TestAssets\testCase1.json");
            var client = new HttpClient();
            client.BaseAddress = new System.Uri("http://localhost:5099/test");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var content = new StringContent("{'data': 'test'}");
            var result = client.PostAsync(client.BaseAddress, content).Result;
            Assert.Equal(200, (int)result.StatusCode);
            var respContent = result.Content.ReadAsStringAsync().Result;
            Assert.NotNull(respContent);
            Assert.Equal("THIS IS A TEST!", respContent);
            Server.Stop();
            Server = null;
        }

        [Fact]
        public void FakeHttpServer_MultipleRequests_ReturnsExpectedContent()
        {
            Prefixes.Add($"http://localhost:{Port}/");
            Server = new FakeHttpServer(Prefixes.ToArray());
            Server.Run();
            Server.LoadConfig(@".\TestAssets\testCase2.json");
            var client = new HttpClient();
            client.BaseAddress = new System.Uri("http://localhost:5099/test");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var content = new StringContent("{'data': 'test'}");
            var result = client.PostAsync(client.BaseAddress, content).Result;
            Assert.Equal(200, (int)result.StatusCode);
            var respContent = result.Content.ReadAsStringAsync().Result;
            Assert.NotNull(respContent);
            Assert.Equal("THIS IS A TEST!", respContent);

            // Second request
            result = client.PostAsync(client.BaseAddress, content).Result;
            Assert.Equal(201, (int)result.StatusCode);
            Server.Stop();
            Server = null;
        }

        [Fact]
        public void FakeHttpServer_MultipleRoutes_ReturnsExpectedContent()
        {
            Prefixes.Add($"http://localhost:{Port}/");
            Server = new FakeHttpServer(Prefixes.ToArray());
            Server.Run();
            Server.LoadConfig(@".\TestAssets\testCase3.json");
            var client = new HttpClient();
            client.BaseAddress = new System.Uri("http://localhost:5099/test");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var content = new StringContent("{'data': 'test'}");
            var result = client.PostAsync(client.BaseAddress, content).Result;
            Assert.Equal(200, (int)result.StatusCode);
            var respContent = result.Content.ReadAsStringAsync().Result;
            var jsonResp = JsonConvert.DeserializeObject<FakeData>(respContent);
            Assert.NotNull(jsonResp);
            Assert.Equal("test", jsonResp.data);

            // Second request
            client = new HttpClient();
            client.BaseAddress = new System.Uri("http://localhost:5099/test2");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            result = client.PostAsync(client.BaseAddress, content).Result;
            Assert.Equal(201, (int)result.StatusCode);
            respContent = result.Content.ReadAsStringAsync().Result;
            jsonResp = JsonConvert.DeserializeObject<FakeData>(respContent);
            Assert.NotNull(jsonResp);
            Assert.Equal("test2", jsonResp.data);
            Server.Stop();
            Server = null;
        }
    }
}
