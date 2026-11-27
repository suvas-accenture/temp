using Capstone.API.GenericResponse;
using Xunit;

namespace Capstone.API.Tests.GenericResponse;

public class ResponseResultTests
{
    [Fact]
    public void Ok_SetsSuccessTrueAndPopulatesData()
    {
        var response = ResponseResult<string>.Ok("Ticket #101", "Created successfully");

        Assert.True(response.Success);
        Assert.Equal("Created successfully", response.Message);
        Assert.Equal("Ticket #101", response.Data);
        Assert.Null(response.Errors);
    }

    [Fact]
    public void Ok_UsesDefaultMessageWhenOmitted()
    {
        var response = ResponseResult<int>.Ok(42);

        Assert.True(response.Success);
        Assert.Equal("Request completed successfully.", response.Message);
        Assert.Equal(42, response.Data);
        Assert.Null(response.Errors);
    }

    [Fact]
    public void Fail_SetsSuccessFalseAndPopulatesErrors()
    {
        var errors = new List<string> { "Title is required." };
        var response = ResponseResult<string>.Fail("Validation failed", errors);

        Assert.False(response.Success);
        Assert.Equal("Validation failed", response.Message);
        Assert.Null(response.Data);
        Assert.Equal(errors, response.Errors);
    }

    [Fact]
    public void Fail_AllowsNullErrors()
    {
        var response = ResponseResult<object>.Fail("Not found");

        Assert.False(response.Success);
        Assert.Equal("Not found", response.Message);
        Assert.Null(response.Data);
        Assert.Null(response.Errors);
    }
}