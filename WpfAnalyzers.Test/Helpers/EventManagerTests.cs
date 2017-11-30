namespace WpfAnalyzers.Test
{
    using NUnit.Framework;

    public class EventManagerTests
    {
        [TestCase("SizeChangedEvent", "OnSizeChanged", true)]
        [TestCase("SizeChanged", "OnSizeChanged", false)]
        [TestCase("MouseDownEvent", "OnSizeChanged", false)]
        [TestCase("MouseDownEvent", "On", false)]
        [TestCase("SizeChange_", "OnSizeChanged", false)]
        [TestCase("SizeChanged", "OnSizeChange_", false)]
        [TestCase("SizeChangedEvent", "OnSizeChangedHandler", false)]
        [TestCase("SizeChangedEvent", "SizeChangedHandler", false)]
        public void IsMatch(string eventName, string callbackName, bool expected)
        {
            Assert.AreEqual(expected, EventManager.IsMatch(callbackName, eventName));
        }

        [TestCase("SizeChangedEvent", true, "OnSizeChanged")]
        [TestCase("SizeChanged", true, null)]
        public void TryGetExpectedCallbackName(string eventName, bool expected, string expectedCallbackName)
        {
            Assert.AreEqual(expected, EventManager.TryGetExpectedCallbackName(eventName, out var callbackName));
            Assert.AreEqual(expectedCallbackName, callbackName);
        }
    }
}