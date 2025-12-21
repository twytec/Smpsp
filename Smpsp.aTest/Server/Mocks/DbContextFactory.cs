using Microsoft.EntityFrameworkCore;
using Smpsp.Server.Data;

namespace Smpsp.aTest.Server.Mocks
{
    public class DbContextFactory(PathService _ps) : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext()
        {
            return new AppDbContext(_ps);
        }
    }
}
