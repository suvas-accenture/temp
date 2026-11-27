using System.Net;
using System.Text.Json;
using Capstone.API.GenericResponse;
using Capstone.API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Capstone.API.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task InvokeAsync_WhenNoException_CallsNextAndKeepsResponse()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = async ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status200OK;
            await ctx.Response.WriteAsync("ok");
        };

        var middleware = new ExceptionHandlingMiddleware(next, NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Equal("ok", body);
    }

    [Fact]
    public async Task InvokeAsync_WhenKeyNotFoundException_Returns404ResponseResult()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = _ => throw new KeyNotFoundException("Ticket 42 not found.");
        var middleware = new ExceptionHandlingMiddleware(next, NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        context.Response.Body.Position = 0;
        var payload = await JsonSerializer.DeserializeAsync<ResponseResult<object>>(context.Response.Body, JsonOptions);

        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal("Ticket 42 not found.", payload.Message);
        Assert.Null(payload.Data);
        Assert.Contains("Ticket 42 not found.", payload.Errors!);
    }

    [Fact]
    public async Task InvokeAsync_WhenArgumentException_Returns400ResponseResult()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = _ => throw new ArgumentException("Invalid priority value.");
        var middleware = new ExceptionHandlingMiddleware(next, NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        context.Response.Body.Position = 0;
        var payload = await JsonSerializer.DeserializeAsync<ResponseResult<object>>(context.Response.Body, JsonOptions);

        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal("Invalid priority value.", payload.Message);
        Assert.Null(payload.Data);
        Assert.Contains("Invalid priority value.", payload.Errors!);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnhandledException_Returns500ResponseResult()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = _ => throw new InvalidOperationException("Database lock timeout.");
        var middleware = new ExceptionHandlingMiddleware(next, NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        context.Response.Body.Position = 0;
        var payload = await JsonSerializer.DeserializeAsync<ResponseResult<object>>(context.Response.Body, JsonOptions);

        Assert.NotNull(payload);
        Assert.False(payload.Success);
        Assert.Equal("An unexpected error occurred while processing your request.", payload.Message);
        Assert.Null(payload.Data);
        Assert.Contains("Database lock timeout.", payload.Errors!);
    }
}