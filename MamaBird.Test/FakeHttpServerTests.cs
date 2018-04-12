using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MamaBird.Test
{
    public class FakeData
    {
        public string data;
    }

    [TestClass]
    public class FakeHttpServerTests
    {
        public int Port = 5099;
        public List<string> Prefixes = new List<string>();
        public FakeHttpServer Server;

        [TestInitialize]
        public void Init()
        {
            Prefixes.Add($"http://localhost:{Port}/");
        }

        [TestCleanup]
        public void Cleanup()
        {
            Server.Stop();
            Server = null;
        }

        [TestMethod]
        public void FakeHttpServer_Constructor()
        {
            Server = new FakeHttpServer(Prefixes.ToArray());
            Assert.IsNotNull(Server.Config);
        }

        [TestMethod]
        public void FakeHttpServer_CanLoadConfigFromFile()
        {
            Server = new FakeHttpServer(Prefixes.ToArray());
            Server.LoadConfig(@".\TestAssets\testCase1.json");
            Assert.AreEqual(1, Server.Config.Count);
            
        }

        [TestMethod]
        public void FakeHttpServer_WhenNoConfigAdded_Returns404()
        {
            Server = new FakeHttpServer(Prefixes.ToArray());
            Server.Run();
            var client = new HttpClient();
            client.BaseAddress = new System.Uri("http://localhost:5099/test");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var content = new StringContent("{'data': 'test'}");
            var result = client.PostAsync(client.BaseAddress, content).Result;
            Assert.AreEqual(404, (int)result.StatusCode);
        }

        [TestMethod]
        public void FakeHttpServer_SingleRequest_ReturnsExpectedContent()
        {
            Server = new FakeHttpServer(Prefixes.ToArray());
            Server.Run();
            Server.LoadConfig(@".\TestAssets\testCase1.json");
            var client = new HttpClient();
            client.BaseAddress = new System.Uri("http://localhost:5099/test");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var content = new StringContent("{'data': 'test'}");
            var result = client.PostAsync(client.BaseAddress, content).Result;
            Assert.AreEqual(200, (int)result.StatusCode);
            var respContent = result.Content.ReadAsStringAsync().Result;
            Assert.IsNotNull(respContent);
            Assert.AreEqual("THIS IS A TEST!", respContent);
        }

        [TestMethod]
        public void FakeHttpServer_MultipleRequests_ReturnsExpectedContent()
        {
            Server = new FakeHttpServer(Prefixes.ToArray());
            Server.Run();
            Server.LoadConfig(@".\TestAssets\testCase2.json");
            var client = new HttpClient();
            client.BaseAddress = new System.Uri("http://localhost:5099/test");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var content = new StringContent("{'data': 'test'}");
            var result = client.PostAsync(client.BaseAddress, content).Result;
            Assert.AreEqual(200, (int)result.StatusCode);
            var respContent = result.Content.ReadAsStringAsync().Result;
            Assert.IsNotNull(respContent);
            Assert.AreEqual("THIS IS A TEST!", respContent);

            // Second request
            result = client.PostAsync(client.BaseAddress, content).Result;
            Assert.AreEqual(201, (int)result.StatusCode);
        }

        [TestMethod]
        public void FakeHttpServer_MultipleRoutes_ReturnsExpectedContent()
        {
            Server = new FakeHttpServer(Prefixes.ToArray());
            Server.Run();
            Server.LoadConfig(@".\TestAssets\testCase3.json");
            var client = new HttpClient();
            client.BaseAddress = new System.Uri("http://localhost:5099/test");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var content = new StringContent("{'data': 'test'}");
            var result = client.PostAsync(client.BaseAddress, content).Result;
            Assert.AreEqual(200, (int)result.StatusCode);
            var respContent = result.Content.ReadAsStringAsync().Result;
            var jsonResp = JsonConvert.DeserializeObject<FakeData>(respContent);
            Assert.IsNotNull(jsonResp);
            Assert.AreEqual("test", jsonResp.data);

            // Second request
            client = new HttpClient();
            client.BaseAddress = new System.Uri("http://localhost:5099/test2");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            result = client.PostAsync(client.BaseAddress, content).Result;
            Assert.AreEqual(201, (int)result.StatusCode);
            respContent = result.Content.ReadAsStringAsync().Result;
            jsonResp = JsonConvert.DeserializeObject<FakeData>(respContent);
            Assert.IsNotNull(jsonResp);
            Assert.AreEqual("test2", jsonResp.data);
        }
    }
}
