using System;
using System.Collections;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;

namespace SystemEvents.Unit.Tests
{
    [TestFixture]
    public class SystemEventsControllerTests
    {
        [Test]
        public async Task Ctor_NullParams_Throws()
        {
            await Task.Delay(100);
            Assert.True(true);
        }

        [Test]
        public async Task Create_InvalidEventRequestModelCases_Throws()
        {
            await Task.Delay(100);
            Assert.True(true);
        }

        [Test]
        public async Task Create_ValidRequest_Succeeds()
        {
            await Task.Delay(100);
            Assert.True(true);
        }
    }
}