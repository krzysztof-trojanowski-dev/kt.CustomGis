namespace kt.CustomGis.Core.Tests
{
    [TestFixture]
    public class PagedResultTests
    {
        private PagedResult<int> _pagedResult;
        
        [Test]
        public void Items_WhenModelIsEmpty_ReturnEmptyCollection()
        {
            _pagedResult = new PagedResult<int>();

            Assert.That(_pagedResult.Items, Is.Empty);
            Assert.That(_pagedResult.HasPreviousPage, Is.False);
            Assert.That(_pagedResult.HasNextPage, Is.False);
            Assert.That(_pagedResult.TotalCount, Is.EqualTo(0));
            Assert.That(_pagedResult.TotalPages, Is.EqualTo(0));
        }

        [Test]
        public void HasPreviousPage_WhenPageNumberIsGreaterThan1_ReturnTrue()
        {
            _pagedResult = new PagedResult<int>
            {
                PageNumber = 2
            };

            Assert.That(_pagedResult.HasPreviousPage, Is.True);
        }

        [Test]
        public void HasNextPage_WhenPageNumberIsLessThanTotalPages_ReturnTrue()
        {
            _pagedResult = new PagedResult<int>
            {
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 20
            };

            Assert.That(_pagedResult.HasNextPage, Is.True);
        }
    }
}
