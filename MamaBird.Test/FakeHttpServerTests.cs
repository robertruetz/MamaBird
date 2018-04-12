using Newtonsoft.Json;
using System;
using System.Net.Http;
using Xunit;

namespace MamaBird.Test
{
    public class FakeData
    {
        public string data;
    }
    
    public class FakeHttpServerTests : IDisposable
    {
        public int Port = 5099;
        public FakeHttpServer Server;

        public FakeHttpServerTests()
        {
            Server = new FakeHttpServer(Port);
        }

        public void Dispose()
        {
            Server.Stop();
            Server = null;
        }

        [Fact]
        public void FakeHttpServer_Constructor()
        {
            Assert.Equal(Port, Server.Port);
            Assert.NotNull(Server.Config);
            Assert.Empty(Server.Config);
        }

        [Fact]
        public void FakeHttpServer_CanLoadConfigFromFile()
        {
            Server = new FakeHttpServer(Port);
            Server.LoadConfig(@"./TestAssets/testCase1.json");
            Assert.Single(Server.Config);
        }

        [Fact]
        public void FakeHttpServer_WhenNoConfigAdded_Returns404()
        {
            Server.Run();
            var client = new HttpClient();
            client.BaseAddress = new System.Uri("http://localhost:5099/test");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var content = new StringContent("{'data': 'test'}");
            var result = client.PostAsync(client.BaseAddress, content).Result;
            Assert.Equal(404, (int)result.StatusCode);
        }

        [Fact]
        public void FakeHttpserver_SingleRequest_ReturnsExpectedContent()
        {
            Server.LoadConfig(@"./TestAssets/testCase1.json");
            Server.Run();
            var client = new HttpClient();
            client.BaseAddress = new System.Uri("http://localhost:5099/test");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var content = new StringContent("{'data': 'test'}");
            var result = client.PostAsync(client.BaseAddress, content).Result;
            Assert.Equal(200, (int)result.StatusCode);
            var respContent = result.Content.ReadAsStringAsync().Result;
            Assert.NotNull(respContent);
            Assert.Equal("THIS IS A TEST!", respContent);
        }

        [Fact]
        public void FakeHttpServer_MultipleRequests_ReturnsExpectedContent()
        {
            Server.LoadConfig(@"./TestAssets/testCase2.json");
            Server.Run();
            Assert.Equal(2, Server.Config["/test"].Count);
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
        }

        [Fact]
        public void FakeHttpServer_MultipleRoutes_ReturnsExpectedContent()
        {
            Server.LoadConfig(@"./TestAssets/testCase3.json");
            Server.Run();
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
        }
    }
}
