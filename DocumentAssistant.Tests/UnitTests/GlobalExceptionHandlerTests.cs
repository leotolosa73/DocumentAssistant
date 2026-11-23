using DocumentAssistant.Api.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using Xunit;

namespace DocumentAssistant.Tests.UnitTests;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_ShouldReturn500WithoutExposingInternalException()
    {
        // Arrange
        var handler = new GlobalExceptionHandler(
            NullLogger<GlobalExceptionHandler>.Instance);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Response.Body = new MemoryStream();

        var exception = new InvalidOperationException(
            "Internal sensitive information.");

        // Act
        var handled = await handler.TryHandleAsync(
            context,
            exception,
            CancellationToken.None);

        // Assert
        Assert.True(handled);

        Assert.Equal(
            StatusCodes.Status500InternalServerError,
            context.Response.StatusCode);

        context.Response.Body.Position = 0;

        using var jsonDocument = await JsonDocument.ParseAsync(
            context.Response.Body);

        var root = jsonDocument.RootElement;

        Assert.Equal(
            "An unexpected error occurred.",
            root.GetProperty("title").GetString());

        Assert.Equal(
            500,
            root.GetProperty("status").GetInt32());

        Assert.Equal(
            "The server encountered an unexpected error.",
            root.GetProperty("detail").GetString());

        Assert.Equal(
            "/api/test",
            root.GetProperty("instance").GetString());

        Assert.DoesNotContain(
            "Internal sensitive information.",
            root.ToString());
    }
}