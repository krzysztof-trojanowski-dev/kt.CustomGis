namespace kt.CustomGis.Core.Tests
{
    [TestFixture]
    public class PageRequestTests
    {
        [Test]
        public void Offset_WhenPageSizeEqual0_Return0()
        {
            var pageRequest = new PageRequest()
            {
                PageSize = 0,
                PageNumber = 2
            };

            Assert.That(pageRequest.Offset, Is.EqualTo(0));
        }

        [Test]
        public void Offset_WhenPageNumberIsEqual1_Return0()
        {
            var pageRequest = new PageRequest()
            {
                PageSize = 10,
                PageNumber = 1
            };
            Assert.That(pageRequest.Offset, Is.EqualTo(0));
        }

        [Test]
        public void Offset_WhenPageNumberIsGreaterThan1_ReturnCalculatedOffset()
        {
            var pageRequest = new PageRequest()
            {
                PageSize = 10,
                PageNumber = 2
            };
            Assert.That(pageRequest.Offset, Is.EqualTo(10));
        }
    }
}
