using Xunit;

namespace MamaBird.Test
{
    public class HttpInteractionTests
    {
        [Fact]
        public void HttpInteraction_CanConstructEmptyObject()
        {
            var httpInt = new HttpInteraction();
            Assert.NotNull(httpInt);
            Assert.True(string.IsNullOrEmpty(httpInt.Route));
            Assert.True(string.IsNullOrEmpty(httpInt.Content));
            Assert.Equal(0, httpInt.Delay);
        }

        [Fact]
        public void HttpInteraction_CanConstructFullObject()
        {
            var httpInt = new HttpInteraction("/test", "{'response': 'response'}", 100);
            Assert.NotNull(httpInt);
            Assert.Equal("/test", httpInt.Route);
            Assert.Equal("{'response': 'response'}", httpInt.Content);
            Assert.Equal(100, httpInt.Delay);
        }

        [Fact]
        public void HttpInteraction_CanConstructObjectWithoutDelay()
        {
            var httpInt = new HttpInteraction("/test", "test");
            Assert.NotNull(httpInt);
        }
    }
}
