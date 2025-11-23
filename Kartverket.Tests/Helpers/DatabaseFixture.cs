using FirstWebApplication.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Kartverket.Tests.Helpers
{
    // Fixture for creating in-memory database contexts for testing
    public static class DatabaseFixture
    {
        // Creates an in-memory database context for testing
        public static ApplicationDBContext CreateContext(string? databaseName = null)
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDBContext(options);
        }
    }
}

