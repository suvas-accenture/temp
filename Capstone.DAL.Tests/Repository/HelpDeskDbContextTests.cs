using Capstone.DAL.Models;
using Capstone.DAL.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Capstone.DAL.Tests.Repository;

public sealed class HelpDeskDbContextTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly HelpDeskDbContext _context;

    public HelpDeskDbContextTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        DbContextOptions<HelpDeskDbContext> options = new DbContextOptionsBuilder<HelpDeskDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new HelpDeskDbContext(options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task OnModelCreating_ShouldSeedExpectedRolesAndStatuses()
    {
        List<Role> roles = await _context.Roles
            .OrderBy(r => r.RoleId)
            .ToListAsync();

        List<Status> statuses = await _context.Statuses
            .OrderBy(s => s.StatusId)
            .ToListAsync();

        Assert.Equal(2, roles.Count);
        Assert.Equal("Admin", roles[0].RoleName);
        Assert.Equal("User", roles[1].RoleName);

        Assert.Equal(4, statuses.Count);
        Assert.Equal("Open", statuses[0].Description);
        Assert.Equal("In Progress", statuses[1].Description);
        Assert.Equal("Closed", statuses[2].Description);
        Assert.Equal("Re-Opened", statuses[3].Description);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistSingularTablesAndNavigationGraph()
    {
        User user = new()
        {
            UserName = "alice",
            Password = "Password123",
            CreatedOn = DateTime.UtcNow,
            RoleId = 2
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        ServiceRequest request = new()
        {
            Description = "VPN setup",
            Details = "Requesting remote VPN access",
            RaisedBy = "alice",
            RaisedOn = DateTime.UtcNow,
            Justification = "Remote sprint work",
            ReqStatus = 1
        };
        _context.ServiceRequests.Add(request);
        await _context.SaveChangesAsync();

        ServiceRequest? loaded = await _context.ServiceRequests
            .AsNoTracking()
            .Include(sr => sr.RaisedByUser)
                .ThenInclude(u => u!.Role)
            .Include(sr => sr.Status)
            .FirstOrDefaultAsync(sr => sr.RequestId == request.RequestId);

        Assert.NotNull(loaded);
        Assert.Equal("alice", loaded!.RaisedByUser?.UserName);
        Assert.Equal("User", loaded.RaisedByUser?.Role?.RoleName);
        Assert.Equal("Open", loaded.Status?.Description);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenDeletingReferencedRole_ShouldThrowDbUpdateExceptionDueToRestrict()
    {
        User user = new()
        {
            UserName = "bob",
            Password = "Password123",
            CreatedOn = DateTime.UtcNow,
            RoleId = 2
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _context.Entry(user).State = EntityState.Detached;

        Role role = await _context.Roles.FirstAsync(r => r.RoleId == 2);
        _context.Roles.Remove(role);

        await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}