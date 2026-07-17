using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using ChoicePie.Backend.Shared.Application.Exceptions;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.Shared.Hosting.Exceptions.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ChoicePie.Backend.Shared.Hosting.Tests.Exceptions.Handlers;

[TestFixture]
public class BadRequestExceptionHandlerTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private BadRequestExceptionHandler _sut = null!;
    private DefaultHttpContext _httpContext = null!;
    private MemoryStream _responseBody = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new BadRequestExceptionHandler(Substitute.For<ILogger<BadRequestExceptionHandler>>());
        _responseBody = new MemoryStream();
        _httpContext = new DefaultHttpContext { Response = { Body = _responseBody } };
    }

    [Test]
    public async Task TryHandleAsync_GivenAggregateValidationException_WhenCalled_ThenWritesAllFieldErrors()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["Name"] = ["Name is required"],
            ["Code"] = ["Code too long"]
        };
        var exception = new AggregateValidationException(errors);

        var handled = await _sut.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        Assert.That(handled, Is.True);
        var response = await ReadResponseAsync();
        Assert.That(response.Errors, Is.Not.Null);
        Assert.That(response.Errors!.Keys, Is.EquivalentTo(new[] { "Name", "Code" }));
        Assert.That(response.Errors["Name"], Is.EquivalentTo(new[] { "Name is required" }));
    }

    [Test]
    public async Task TryHandleAsync_GivenPlainValidationException_WhenCalled_ThenStillWritesSingleFieldError()
    {
        var validationResult = new ValidationResult("Email is invalid", ["Email"]);
        var exception = new ValidationException(validationResult, null, null);

        var handled = await _sut.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        Assert.That(handled, Is.True);
        var response = await ReadResponseAsync();
        Assert.That(response.Errors, Is.Not.Null);
        Assert.That(response.Errors!["Email"], Is.EquivalentTo(new[] { "Email is invalid" }));
    }

    [Test]
    public async Task TryHandleAsync_GivenValidationException_WhenCalled_ThenSetsStatusCode400()
    {
        var validationResult = new ValidationResult("Email is invalid", ["Email"]);
        var exception = new ValidationException(validationResult, null, null);

        await _sut.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public async Task TryHandleAsync_GivenInvalidOperationException_WhenCalled_ThenReturnsBadRequestWithExceptionMessage()
    {
        var exception = new InvalidOperationException("something went wrong");

        var handled = await _sut.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        Assert.That(handled, Is.True);
        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        var response = await ReadResponseAsync();
        Assert.That(response.Message, Is.EqualTo("something went wrong"));
    }

    [Test]
    public async Task TryHandleAsync_GivenUnrelatedException_WhenCalled_ThenReturnsFalseAndDoesNotWriteResponse()
    {
        var exception = new NotImplementedException();

        var handled = await _sut.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        Assert.That(handled, Is.False);
        Assert.That(_responseBody.Length, Is.EqualTo(0));
    }

    private async Task<ApiErrorResponse> ReadResponseAsync()
    {
        _responseBody.Seek(0, SeekOrigin.Begin);
        var response = await JsonSerializer.DeserializeAsync<ApiErrorResponse>(_responseBody, JsonOptions);
        Assert.That(response, Is.Not.Null);
        return response!;
    }
}
