using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace MamaBird.Test
{
    [TestClass]
    public class HttpInteractionTests
    {
        [TestMethod]
        public void HttpInteraction_CanConstructEmptyObject()
        {
            var httpInt = new HttpInteraction();
            Assert.IsNotNull(httpInt);
            Assert.IsTrue(string.IsNullOrEmpty(httpInt.Route));
            Assert.IsTrue(string.IsNullOrEmpty(httpInt.Content));
            Assert.AreEqual(0, httpInt.Delay);
        }

        [TestMethod]
        public void HttpInteraction_CanConstructFullObject()
        {
            var httpInt = new HttpInteraction("/test", "{'response': 'response'}", 100);
            Assert.IsNotNull(httpInt);
            Assert.AreEqual(httpInt.Route, "/test");
            Assert.AreEqual(httpInt.Content, "{'response': 'response'}");
            Assert.AreEqual(100, httpInt.Delay);
        }

        [TestMethod]
        public void HttpInteraction_CanConstructObjectWithoutDelay()
        {
            var httpInt = new HttpInteraction("/test", "test");
            Assert.IsNotNull(httpInt);
        }
    }
}
