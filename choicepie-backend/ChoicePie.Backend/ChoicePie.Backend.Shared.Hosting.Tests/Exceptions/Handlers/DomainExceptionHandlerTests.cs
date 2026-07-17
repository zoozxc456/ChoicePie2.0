using System.Net;
using System.Text.Json;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.Shared.Hosting.Exceptions.Handlers;
using ChoicePie.Backend.Shared.Kernel.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ChoicePie.Backend.Shared.Hosting.Tests.Exceptions.Handlers;

[TestFixture]
public class DomainExceptionHandlerTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private DomainExceptionHandler _sut = null!;
    private DefaultHttpContext _httpContext = null!;
    private MemoryStream _responseBody = null!;

    private sealed class TestNotFoundException()
        : DomainException("internal log message", "找不到指定的資源。", "TEST_NOT_FOUND", HttpStatusCode.NotFound);

    [SetUp]
    public void SetUp()
    {
        _sut = new DomainExceptionHandler(Substitute.For<ILogger<DomainExceptionHandler>>());
        _responseBody = new MemoryStream();
        _httpContext = new DefaultHttpContext { Response = { Body = _responseBody } };
    }

    [Test]
    public async Task TryHandleAsync_GivenDomainException_WhenCalled_ThenSetsStatusCodeFromException()
    {
        var exception = new TestNotFoundException();

        var handled = await _sut.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        Assert.That(handled, Is.True);
        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
    }

    [Test]
    public async Task TryHandleAsync_GivenDomainException_WhenCalled_ThenWritesErrorCodeAndUserFriendlyMessageNotInternalMessage()
    {
        var exception = new TestNotFoundException();

        await _sut.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        var response = await ReadResponseAsync();
        Assert.That(response.Code, Is.EqualTo("TEST_NOT_FOUND"));
        Assert.That(response.Message, Is.EqualTo("找不到指定的資源。"));
        Assert.That(response.Message, Does.Not.Contain("internal log message"), "不該把只給開發人員看的內部訊息回給前端");
    }

    [Test]
    public async Task TryHandleAsync_GivenNonDomainException_WhenCalled_ThenReturnsFalseAndDoesNotWriteResponse()
    {
        var exception = new InvalidOperationException("not a domain exception");

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
