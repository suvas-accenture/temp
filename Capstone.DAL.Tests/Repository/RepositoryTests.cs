using Capstone.DAL.DTOs;
using Capstone.DAL.Models;
using Capstone.DAL.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Capstone.DAL.Tests.Repository;

public sealed class RepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly HelpDeskDbContext _context;
    private readonly IRepository _repository;

    public RepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        DbContextOptions<HelpDeskDbContext> options = new DbContextOptionsBuilder<HelpDeskDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new HelpDeskDbContext(options);
        _context.Database.EnsureCreated();

        _context.Users.AddRange(
            new User
            {
                UserName = "alice",
                Password = "Password123",
                CreatedOn = DateTime.UtcNow,
                RoleId = 2
            },
            new User
            {
                UserName = "admin",
                Password = "AdminPassword123",
                CreatedOn = DateTime.UtcNow,
                RoleId = 1
            });
        _context.SaveChanges();

        _repository = new Capstone.DAL.Repository.Repository(_context);
    }

    [Fact]
    public void Authenticate_WithValidCredentials_ShouldReturnTrue()
    {
        bool result = _repository.Authenticate(new User
        {
            UserName = "alice",
            Password = "Password123"
        });

        Assert.True(result);
    }

    [Theory]
    [InlineData("alice", "WrongPassword")]
    [InlineData("unknown_user", "Password123")]
    [InlineData("", "Password123")]
    [InlineData("alice", "")]
    public void Authenticate_WithInvalidOrMissingCredentials_ShouldReturnFalse(string userName, string password)
    {
        bool result = _repository.Authenticate(new User
        {
            UserName = userName,
            Password = password
        });

        Assert.False(result);
    }

    [Fact]
    public void GetUser_WhenUserExists_ShouldReturnUserWithRoleNavigation()
    {
        User? user = _repository.GetUser("admin");

        Assert.NotNull(user);
        Assert.Equal("admin", user!.UserName);
        Assert.Equal("Admin", user.Role?.RoleName);
    }

    [Fact]
    public void GetUser_WhenUserDoesNotExist_ShouldReturnNull()
    {
        User? user = _repository.GetUser("does_not_exist");

        Assert.Null(user);
    }

    [Fact]
    public void RaiseRequest_ShouldDefaultStatusToOpenAndReturnNewId()
    {
        ServiceRequest request = new()
        {
            Description = "Monitor replacement",
            Details = "Flickering display on secondary monitor",
            RaisedBy = "alice",
            Justification = "Daily development work"
        };

        int createdId = _repository.RaiseRequest(request);
        ServiceRequest? loaded = _repository.GetRequestById(createdId);

        Assert.True(createdId > 0);
        Assert.NotNull(loaded);
        Assert.Equal(1, loaded!.ReqStatus);
        Assert.NotEqual(default, loaded.RaisedOn);
    }

    [Fact]
    public void ViewRequests_ShouldReturnAllOrFilterByUserName()
    {
        _repository.RaiseRequest(new ServiceRequest
        {
            Description = "Alice request",
            Details = "Details for Alice",
            RaisedBy = "alice",
            Justification = "Work"
        });

        _repository.RaiseRequest(new ServiceRequest
        {
            Description = "Admin request",
            Details = "Details for Admin",
            RaisedBy = "admin",
            Justification = "Operations"
        });

        List<ServiceRequest> allRequests = _repository.ViewRequests();
        List<ServiceRequest> aliceRequests = _repository.ViewRequests("alice");

        Assert.Equal(2, allRequests.Count);
        Assert.Single(aliceRequests);
        Assert.Equal("alice", aliceRequests[0].RaisedBy);
    }

    [Fact]
    public void CloseAndReOpenRequest_ShouldUpdateStatusCodes()
    {
        int createdId = _repository.RaiseRequest(new ServiceRequest
        {
            Description = "Access issue",
            Details = "Repo permission needed",
            RaisedBy = "alice",
            Justification = "Feature branch"
        });

        bool closed = _repository.CloseRequest(createdId);
        ServiceRequest? afterClose = _repository.GetRequestById(createdId);

        bool reopened = _repository.ReOpenRequest(new ServiceRequest { RequestId = createdId });
        ServiceRequest? afterReopen = _repository.GetRequestById(createdId);

        Assert.True(closed);
        Assert.Equal(3, afterClose?.ReqStatus);
        Assert.True(reopened);
        Assert.Equal(4, afterReopen?.ReqStatus);
    }

    [Fact]
    public async Task GetSummaryRequestByUserAsync_ShouldReturnCountsAndProjectedItems()
    {
        int firstId = _repository.RaiseRequest(new ServiceRequest
        {
            Description = "Request 1",
            Details = "First request",
            RaisedBy = "alice",
            Justification = "Task 1"
        });

        int secondId = _repository.RaiseRequest(new ServiceRequest
        {
            Description = "Request 2",
            Details = "Second request",
            RaisedBy = "alice",
            Justification = "Task 2"
        });

        _repository.CloseRequest(firstId);
        _repository.ReOpenRequest(new ServiceRequest { RequestId = secondId });

        SummaryResponse summary = await _repository.GetSummaryRequestByUserAsync("alice");

        Assert.Equal("alice", summary.UserName);
        Assert.Equal(2, summary.TotalRequests);
        Assert.Equal(0, summary.OpenRequests);
        Assert.Equal(1, summary.ClosedRequests);
        Assert.Equal(1, summary.ReOpenedRequests);
        Assert.Equal(2, summary.Requests.Count);
        Assert.Contains(summary.Requests, item => item.RequestId == firstId && item.Status == "Closed");
        Assert.Contains(summary.Requests, item => item.RequestId == secondId && item.Status == "Re-Opened");
    }

    [Fact]
    public void DeleteRequest_WhenRequestExists_ShouldRemoveRecord()
    {
        int createdId = _repository.RaiseRequest(new ServiceRequest
        {
            Description = "Temporary ticket",
            Details = "To be deleted",
            RaisedBy = "alice",
            Justification = "Cleanup test"
        });

        bool deleted = _repository.DeleteRequest(createdId);
        ServiceRequest? loaded = _repository.GetRequestById(createdId);

        Assert.True(deleted);
        Assert.Null(loaded);
    }

    [Fact]
    public void MissingRequestId_ShouldReturnFalseOrNullForOperations()
    {
        const int nonExistentId = 99999;

        Assert.False(_repository.CloseRequest(nonExistentId));
        Assert.False(_repository.ReOpenRequest(new ServiceRequest { RequestId = nonExistentId }));
        Assert.False(_repository.DeleteRequest(nonExistentId));
        Assert.Null(_repository.GetRequestById(nonExistentId));
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}